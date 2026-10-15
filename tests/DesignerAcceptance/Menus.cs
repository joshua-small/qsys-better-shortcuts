using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using QsysMods.Core;

namespace Designer.Modules.ShortcutsLab;

public sealed partial class Probe
{
    private static string? lastMenus;
    private static bool menuBindingsDisposed;
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<FrameworkElement, object> menuObservers = new();
    private static string? MenuText(object? value) => value switch {
        null => null, string s => s, ToolTip t => MenuText(t.Content),
        DependencyObject d => string.Join(" | ", Visual(d).OfType<TextBlock>().Select(t => t.Text)),
        _ => value.ToString()
    };
    // Passive native-template and model receipts. No keyboard or pointer injection.
    private static void ObserveMenus(Window main)
    {
        var roots = PresentationSource.CurrentSources.Cast<PresentationSource>().Select(s => s.RootVisual)
            .OfType<DependencyObject>().ToArray();
        var elements = roots.SelectMany(Visual).OfType<FrameworkElement>().ToArray();
        var popups = elements.Where(e => e.GetType().FullName == "QSC.QSys.windows.PopupToolBar" || e is ContextMenu).ToArray();
        foreach (var root in popups.Append(main))
        {
            if (menuObservers.TryGetValue(root, out _)) continue;
            menuObservers.Add(root, new object());
            root.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler((_, e) => {
                if (e.OriginalSource is ButtonBase b)
                    File.AppendAllText(Path.Combine(Output, "menu-clicks.jsonl"), JsonSerializer.Serialize(new {
                        time = DateTime.UtcNow, type = b.GetType().FullName, tooltip = MenuText(b.ToolTip),
                        command = (b.Command as RoutedCommand)?.Name, b.IsEnabled }) + Environment.NewLine);
            }), true);
        }
        var canvas = Adapter("Canvas");
        var report = new {
            menuBindingsDisposed,
            canvas = canvas?.GetType().FullName,
            objects = Items(Get(canvas, "AllGraphicObjects")).Select(g => new {
                type = g.GetType().FullName, codeName = Get(Get(g, "obj"), "CodeName"),
                position = Get(g, "Position")?.ToString(), size = Get(g, "Size")?.ToString(),
                selected = Get(g, "IsSelected"), locked = Get(g, "IsLocked") }),
            popups = popups.Select(p => new { type = p.GetType().FullName,
                buttons = Visual(p).OfType<ButtonBase>().Select(b => new {
                    type = b.GetType().FullName, b.Name, b.IsEnabled, b.IsVisible,
                    command = (b.Command as RoutedCommand)?.Name,
                    id = b.Command is RoutedCommand c ? Commands.NativeId(c) : null,
                    tooltipType = b.ToolTip?.GetType().FullName, tooltip = MenuText(b.ToolTip) }),
                items = (p is ContextMenu menu ? Menus(menu) : Visual(p).OfType<MenuItem>()).Select(m => new {
                    header = MenuText(m.Header), m.InputGestureText, m.IsEnabled, m.IsVisible,
                    command = (m.Command as RoutedCommand)?.Name,
                    id = m.Command is RoutedCommand c ? Commands.NativeId(c) : null }) }),
            tooltips = elements.OfType<ToolTip>().Where(t => t.IsOpen).Select(t => MenuText(t.Content))
        };
        string json = JsonSerializer.Serialize(report, SettingsJson.Format);
        if (json == lastMenus) return;
        lastMenus = json;
        File.WriteAllText(Path.Combine(Output, "menus-state.json"), json);
        File.AppendAllText(Path.Combine(Output, "menus-history.jsonl"), JsonSerializer.Serialize(new { time = DateTime.UtcNow, report }) + Environment.NewLine);
    }
    private static async Task PrepareMenus(Window main)
    {
        try
        {
            if (!main.Title.StartsWith("untitled", StringComparison.OrdinalIgnoreCase) || SharedSettings.Error is not null)
                throw new InvalidOperationException("Menu setup requires a healthy synthetic untitled design.");
            var canvas = (FrameworkElement)Adapter("Canvas")!;
            var manager = Host.Type("QSC.QSys.App").GetField("Manager", Host.Static)!.GetValue(null)!;
            var creator = Activator.CreateInstance(Host.Type("QSC.QSys.Drag.ComponentCreator`1").MakeGenericType(Host.Type("QSC.QSys.Design.Component")))!;
            creator.GetType().GetProperty("ClassName")!.SetValue(creator, "gain");
            var operation = Invoke(manager, "StartOperation");
            for (int i = 0; i < 3; i++)
            {
                var model = Invoke(creator, "Create", new Point(200 + i * 180, 200 + i * 60))!;
                model.GetType().GetProperty("CodeName")!.SetValue(model, "MenuGain" + (i + 1));
                Invoke(Get(Get(canvas, "Root"), "CurrentLayer")!, "AddChild", model); Host.Call(model, "Attach");
            }
            Invoke(manager, "EndOperation", operation); await Task.Delay(300);
            var selection = HostSelection.GlobalSelection;
            Host.Call(selection, "Clear");
            foreach (var graphic in Items(Get(canvas, "AllGraphicObjects")).Where(g => Get(Get(g, "obj"), "CodeName") is string s && s.StartsWith("MenuGain")))
                Invoke(selection, "Add", graphic, false);
            Host.Call(selection, "OnSelectionChanged"); canvas.Focus();
            // Prepare the UCI variant through native creation APIs as well;
            // this setup is not counted as physical menu/input acceptance.
            Commands.All.Single(c => c.Definition.Name == "Add New User Control Interface").Definition.Execute();
            await Until(() => Adapter("Uci") is not null && Adapter("Canvas") is FrameworkElement);
            canvas = (FrameworkElement)Adapter("Canvas")!;
            for (int i = 0; i < 3; i++)
            {
                Commands.All.Single(c => c.Definition.Id == "better-shortcuts/uci-add-button").Definition.Execute();
                await Task.Delay(100);
                var chooser = PresentationSource.CurrentSources.Cast<PresentationSource>().Select(s => s.RootVisual)
                    .OfType<DependencyObject>().SelectMany(Visual).OfType<ContextMenu>().Single(m => m.IsOpen);
                chooser.Items.OfType<MenuItem>().First().RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                chooser.IsOpen = false; await Task.Delay(150);
                var graphic = Items(Get(canvas, "AllGraphicObjects")).Single(g => Get(g, "IsSelected") is true);
                var model = Get(graphic, "obj")!;
                model.GetType().GetProperty("Position")!.SetValue(model, new Point(160 + i * 140, 140 + i * i * 70));
                Host.Call(manager, "EndOperation");
            }
            Host.Call(selection, "Clear");
            foreach (var graphic in Items(Get(canvas, "AllGraphicObjects"))) Invoke(selection, "Add", graphic, false);
            Host.Call(selection, "OnSelectionChanged"); canvas.Focus();
            if (Environment.GetEnvironmentVariable("QSYS_SHORTCUTS_LAB_MENU_BASELINE") == "1")
            {
                var enabled = NativeClipboardRoundTrip();
                ModControls.Bindings.Dispose(); menuBindingsDisposed = true;
                var disposed = NativeClipboardRoundTrip();
                File.WriteAllText(Path.Combine(Output, "native-clipboard-result.json"), JsonSerializer.Serialize(new {
                    enabled, disposed, note = "Synthetic native CopyControlSet round trips; not physical copy/paste acceptance. Binding engine disposed for the second case; other mod services remain loaded."
                }, SettingsJson.Format));
            }
            ObserveMenus(main);
            File.WriteAllText(Path.Combine(Output, "menus-ready.json"), JsonSerializer.Serialize(new { ready = true, physicalInputTested = false, menuBindingsDisposed }, SettingsJson.Format));
        }
        catch (Exception ex)
        { File.WriteAllText(Path.Combine(Output, "menus-ready.json"), JsonSerializer.Serialize(new { ready = false, error = ex.ToString() }, SettingsJson.Format)); }
    }
    private static object NativeClipboardRoundTrip()
    {
        try
        {
            var type = Host.Type("QSC.QSys.Commands.CopyControlSet");
            var value = Activator.CreateInstance(type)!;
            type.GetProperty("ClassName")!.SetValue(value, "gain");
            type.GetProperty("Controls")!.SetValue(value, new Dictionary<string, string> { ["mute"] = "0" });
            var clipboard = Host.Type("QSC.Common.Clipboard");
            clipboard.GetMethod("SetData", Host.Static)!.Invoke(null, ["QSys.CopyControls", value, true]);
            var present = clipboard.GetMethod("GetDataPresent", Host.Static, null, [typeof(string)], null)!.Invoke(null, ["QSys.CopyControls"]);
            var getter = clipboard.GetMethods(Host.Static).Single(m => m.Name == "GetData" && m.IsGenericMethod && m.GetParameters().Length == 1).MakeGenericMethod(type);
            var read = getter.Invoke(null, ["QSys.CopyControls"]);
            return new { formatPresent = present, typedReadSucceeded = read is not null,
                classMatches = read is not null && Equals(type.GetProperty("ClassName")!.GetValue(read), "gain") };
        }
        catch (Exception ex) { return new { error = ex.ToString() }; }
    }
}
