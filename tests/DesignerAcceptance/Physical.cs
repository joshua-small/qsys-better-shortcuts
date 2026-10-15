using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using QsysMods.Core;
using Keys = System.Windows.Input.Key;

namespace Designer.Modules.ShortcutsLab;

// Explicit lab mode: real OS input only. Never raises or synthesizes key events.
public sealed partial class Probe
{
    private static readonly Dictionary<string, int> physicalRuns = new();
    private static readonly HashSet<Window> observedWindows = new();
    private static void RegisterPhysicalCommands()
    {
        foreach (var (id, binding, text) in new[] {
            ("ordinary-single", "Ctrl+Alt+F8", false), ("ordinary-chord", "Ctrl+Alt+F9 > G", false),
            ("opt-single", "Ctrl+Alt+F10", true), ("opt-chord", "Ctrl+Alt+F11 > G", true) })
            Commands.Register(new("shortcuts-lab/physical-" + id, "Physical " + id, "Physical acceptance",
                () => { physicalRuns[id] = physicalRuns.GetValueOrDefault(id) + 1; PhysicalReceipt("dispatch", null, id); },
                () => true, AllowInText: text) { DefaultBinding = binding });
    }
    private static void PhysicalReceipt(string kind, Window? window, string? command = null, KeyEventArgs? key = null)
    {
        var focused = Keyboard.FocusedElement as FrameworkElement;
        var focusWindow = focused is null ? null : Window.GetWindow(focused);
        var record = new { time = DateTime.UtcNow, pid = Environment.ProcessId, kind, command,
            window = window?.Title, windowType = window?.GetType().FullName, active = window?.IsActive,
            focusWindow = focusWindow?.Title, focused = focused?.GetType().FullName, textInput = ControlInput.IsTextInput(),
            key = key is null ? null : (key.Key == Keys.System ? key.SystemKey : key.Key).ToString(),
            modifiers = Keyboard.Modifiers.ToString(), handled = key?.Handled, repeat = key?.IsRepeat,
            counts = physicalRuns.ToDictionary(p => p.Key, p => p.Value) };
        File.AppendAllText(Path.Combine(Output, "physical-events.jsonl"), JsonSerializer.Serialize(record) + Environment.NewLine);
    }
    private static void ObservePhysicalWindows()
    {
        ObserveNativePopups();
        foreach (var window in Application.Current.Windows.Cast<Window>())
        {
            if (!observedWindows.Add(window)) continue;
            void Record(object sender, KeyEventArgs e)
            {
                var key = e.Key == Keys.System ? e.SystemKey : e.Key;
                // Only the documented test strokes, never arbitrary typed content.
                if (key is Keys.F8 or Keys.F9 or Keys.F10 or Keys.F11 or Keys.G or Keys.Escape
                    || (editorMode && key is Keys.F5 or Keys.F6 or Keys.F7))
                    PhysicalReceipt(e.RoutedEvent.Name, (Window)sender, key: e);
            }
            window.AddHandler(Keyboard.PreviewKeyDownEvent, new KeyEventHandler(Record), true);
            window.AddHandler(Keyboard.PreviewKeyUpEvent, new KeyEventHandler(Record), true);
            if (editorMode)
                window.AddHandler(CommandManager.PreviewExecutedEvent, new ExecutedRoutedEventHandler((sender, e) => {
                    if (e.Command is RoutedCommand command && command.Name is "Connect" or "Emulate" or "Disconnect")
                        PhysicalReceipt("native-preview-executed", (Window)sender, command.Name);
                }), true);
            window.Closed += (_, _) => observedWindows.Remove(window);
        }
        File.WriteAllText(Path.Combine(Output, "physical-counts.json"), JsonSerializer.Serialize(new {
            time = DateTime.UtcNow, counts = physicalRuns,
            bindings = Commands.All.Where(c => c.Definition.Id.StartsWith("shortcuts-lab/physical-")).Select(c => new {
                id = c.Definition.Id, effective = ModControls.Bindings.Display(c.Definition.Id, ModControls.Current.Bindings.Overrides)
            }) }, SettingsJson.Format));
    }
    private static void ObserveNativePopups()
    {
        var popups = PresentationSource.CurrentSources.Cast<PresentationSource>().Select(s => s.RootVisual)
            .OfType<DependencyObject>().SelectMany(Visual).OfType<FrameworkElement>()
            .Where(e => e.GetType().FullName == "QSC.QSys.windows.PopupToolBar").ToArray();
        if (popups.Length == 0) return;
        string? Text(object? value) => value switch {
            null => null, string s => s, ToolTip t => Text(t.Content),
            DependencyObject d => string.Join(" | ", Visual(d).OfType<TextBlock>().Select(t => t.Text)),
            _ => value.ToString()
        };
        File.WriteAllText(Path.Combine(Output, "native-popups.json"), JsonSerializer.Serialize(new {
            time = DateTime.UtcNow,
            popups = popups.Select(p => new { type = p.GetType().FullName, buttons = Visual(p).OfType<ButtonBase>().Select(b => new {
                type = b.GetType().FullName, b.IsEnabled,
                command = (b.Command as RoutedCommand)?.Name,
                id = b.Command is RoutedCommand c ? Commands.NativeId(c) : null,
                tooltipType = b.ToolTip?.GetType().FullName, tooltip = Text(b.ToolTip)
            }) })
        }, SettingsJson.Format));
    }
    private static async Task PreparePhysical(Window main)
    {
        try
        {
            if (!main.Title.StartsWith("untitled", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Physical setup requires a synthetic untitled design.");
            if (SharedSettings.Error is not null) throw new InvalidOperationException(SharedSettings.Error);
            var canvas = (FrameworkElement)Adapter("Canvas")!;
            var manager = Host.Type("QSC.QSys.App").GetField("Manager", Host.Static)!.GetValue(null)!;
            var creator = Activator.CreateInstance(Host.Type("QSC.QSys.Drag.ComponentCreator`1").MakeGenericType(Host.Type("QSC.QSys.Design.Component")))!;
            creator.GetType().GetProperty("ClassName")!.SetValue(creator, "gain");
            var operation = Invoke(manager, "StartOperation");
            var model = Invoke(creator, "Create", new Point(200, 200))!;
            model.GetType().GetProperty("CodeName")!.SetValue(model, "PhysicalGain");
            Invoke(Get(Get(canvas, "Root"), "CurrentLayer")!, "AddChild", model);
            Host.Call(model, "Attach"); Invoke(manager, "EndOperation", operation);
            await Task.Delay(300);
            var graphic = Items(Get(canvas, "AllGraphicObjects")).Single(g => ReferenceEquals(Get(g, "obj"), model));
            var selection = HostSelection.GlobalSelection;
            Host.Call(selection, "Clear"); Invoke(selection, "Add", graphic, false); Host.Call(selection, "OnSelectionChanged");
            canvas.Focus();
            Commands.Native("NamedComponentInfo").Execute(null, main);
            await Task.Delay(300);
            ObservePhysicalWindows();
            PhysicalReceipt("ready", main);
            File.WriteAllText(Path.Combine(Output, "physical-ready.json"), JsonSerializer.Serialize(new {
                ready = true, physicalInputTested = false,
                instruction = "Focus floating Component Controls; Ctrl+Alt+F8 and F9 then G must not dispatch; Ctrl+Alt+F10 and F11 then G must dispatch once. Close/reopen and repeat."
            }, SettingsJson.Format));
        }
        catch (Exception ex)
        { File.WriteAllText(Path.Combine(Output, "physical-ready.json"), JsonSerializer.Serialize(new { ready = false, error = ex.ToString() }, SettingsJson.Format)); }
    }
}
