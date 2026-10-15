using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using QsysMods.Core;

namespace Designer.Modules.ShortcutsLab;

public sealed partial class Probe
{
    private static readonly List<string> checks = [];
    private static void Check(bool value, string name)
    { if (!value) throw new InvalidOperationException(name); checks.Add(name); }
    private static JsonObject Read() => SettingsJson.Parse(File.ReadAllText(SharedSettings.PathName));
    private static async Task Until(Func<bool> ready)
    { for (int i = 0; i < 60; i++) { await Task.Delay(250); if (ready()) return; } throw new TimeoutException(SharedSettings.Error ?? "Acceptance timed out"); }
    private static async Task Apply(BindingChange change)
    {
        var doc = Read();
        doc["mods"]![ModControls.SettingsId]!["Bindings"] = JsonSerializer.SerializeToNode(new BindingSettings(change.Overrides, change.EditorOverrides));
        File.WriteAllText(SharedSettings.PathName, SettingsJson.Text(doc));
        await Until(() => SharedSettings.Error is not null || SettingsJson.Same(JsonSerializer.SerializeToNode(ModControls.Current.Bindings), doc["mods"]![ModControls.SettingsId]!["Bindings"]));
        Check(SharedSettings.Error is null, "Live binding plan accepted");
    }
    private static Window OpenPreferences()
    { var w = (Window)Activator.CreateInstance(Host.Type("QSC.QSys.windows.Dialogs.PreferencesDialog"))!; w.Owner = Application.Current.MainWindow; w.Show(); return w; }
    private static void Save(Window w) => Preferences.Logical(w).OfType<Button>().Single(b => b.IsDefault).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
    private static async Task<TabItem> Editor(Window w)
    {
        var tab = Preferences.Logical(w).OfType<TabItem>().Single(t => Equals(t.Tag, "qsys-mod:better-shortcuts"));
        Preferences.Logical(w).OfType<TabControl>().Single().SelectedItem = tab; await Task.Delay(300); return tab;
    }
    private static void Click(DependencyObject root, string label) => Preferences.Logical(root).OfType<Button>()
        .Single(b => Equals(b.Content, label)).RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
    private static void Select(TabItem tab, string id)
    {
        var table = Preferences.Logical(tab).OfType<DataGrid>().Single(t => t.Name == "ShortcutTable");
        table.SelectedItem = table.Items.Cast<object>().Single(row => (string)Host.Property(row, "Id") == id);
    }
    private static FrameworkElement Recorder(TabItem tab) => Preferences.Logical(tab).OfType<FrameworkElement>().Single(e => e.Name == "ShortcutRecorder");
    private static void Key(FrameworkElement target, System.Windows.Input.Key key)
    {
        var source = PresentationSource.FromVisual(target)!;
        foreach (var routed in new[] { Keyboard.PreviewKeyDownEvent, Keyboard.PreviewKeyUpEvent })
        {
            var e = new KeyEventArgs(Keyboard.PrimaryDevice, source, Environment.TickCount, key) { RoutedEvent = routed };
            target.RaiseEvent(e);
            // The overlay handles Escape-down before it reaches the recorder;
            // its key-up has no native default action and need not be consumed.
            if (key != System.Windows.Input.Key.Escape || routed == Keyboard.PreviewKeyDownEvent)
                Check(e.Handled, "Recorder consumes " + key + " " + routed.Name);
        }
    }
    private static IEnumerable<MenuItem> Menus(ItemsControl parent)
    { foreach (var m in parent.Items.OfType<MenuItem>()) { yield return m; foreach (var child in Menus(m)) yield return child; } }
    private static IEnumerable<DependencyObject> Visual(DependencyObject root)
    { yield return root; for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++) foreach (var child in Visual(VisualTreeHelper.GetChild(root, i))) yield return child; }
    private static async Task AdapterAcceptance()
    {
        var createUci = Commands.All.Single(c => c.Definition.Name == "Add New User Control Interface").Definition;
        Check(createUci.CanExecute(), "Native UCI creation available"); createUci.Execute();
        await Until(() => Adapter("Uci") is not null && Adapter("Canvas") is FrameworkElement);
        var canvas = (FrameworkElement)Adapter("Canvas")!;
        int Count() => Items(Get(canvas, "AllGraphicObjects")).Length;
        foreach (var kind in new[] { "button", "knob", "text", "fader", "meter", "indicator", "flexbox" })
        {
            int before = Count();
            var action = Commands.All.Single(c => c.Definition.Id == "better-shortcuts/uci-add-" + kind).Definition;
            Check(action.CanExecute(), "UCI " + kind + " creation available"); action.Execute(); await Task.Delay(200);
            if (kind != "flexbox")
            {
                var menu = PresentationSource.CurrentSources.Cast<PresentationSource>().Select(s => s.RootVisual).OfType<DependencyObject>()
                    .SelectMany(Visual).OfType<ContextMenu>().Single(m => m.IsOpen);
                Check(menu.Items.OfType<MenuItem>().Any(), "UCI " + kind + " exposes native subtype choices");
                menu.Items.OfType<MenuItem>().First().RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent)); menu.IsOpen = false;
            }
            await Until(() => Count() > before);
            Check(Count() == before + 1, "UCI " + kind + " inserts one graphic object");
            Check(ApplicationCommands.Undo.CanExecute(null, canvas), "UCI " + kind + " insertion is undoable");
            ApplicationCommands.Undo.Execute(null, canvas); await Until(() => Count() == before);
            Check(Count() == before, "Undo removes exactly the new UCI " + kind);
        }
        foreach (var (id, label) in new[] { ("audio", "Audio"), ("control", "Control"), ("layout", "Layout"), ("monitor", "Monitor"), ("scripting", "Scripting"), ("uci-buttons", "UCI Buttons"), ("video", "Video") })
        {
            var action = Commands.All.Single(c => c.Definition.Id == "better-shortcuts/focus-category-" + id).Definition;
            Check(action.CanExecute(), label + " library category available"); action.Execute(); await Task.Delay(100);
            var model = ((FrameworkElement)Adapter("LibraryView")!).DataContext;
            var selected = Get(model, "SelectedTreeViewItem");
            Check(Get(selected, "Expanded") is true && Get(selected, "Label")?.ToString()?.StartsWith(label) == true,
                label + " category expanded and selected without inserting a component");
        }
    }
    private static async Task Acceptance(Window main)
    {
        Window? prefs = null;
        string? original = null;
        try
        {
            Check(main.Title.StartsWith("untitled", StringComparison.OrdinalIgnoreCase), "Blank disposable design");
            Check(SharedSettings.Error is null, "Shared settings initialized");
            original = File.ReadAllText(SharedSettings.PathName);
            string nativeCopy = Commands.NativeId(ApplicationCommands.Copy);
            Check(Commands.All.Any(c => c.Definition.Id == nativeCopy), "Native Copy discovered");
            var plan = ModControls.Bindings.Prepare(new Dictionary<string, string?> {
                ["shortcuts-lab/donor"] = "Alt+Shift+D; Ctrl+Alt+F24",
                ["shortcuts-lab/legacy"] = "Ctrl+Alt+F22", [nativeCopy] = "F24 > G"
            }, ModControls.Current.Bindings.Overrides, ModControls.Current.Bindings.EditorOverrides);
            await Apply(plan);
            var seeded = File.ReadAllText(SharedSettings.PathName);
            prefs = OpenPreferences(); var tab = await Editor(prefs);
            Click(tab, "Review mod defaults");
            var warning = Preferences.Logical(tab).OfType<TextBlock>().Single(t => t.Text.StartsWith("This replaces the assignments"));
            Check(warning.Text.Contains("Lab legacy default: Ctrl+Alt+F23"), "Bulk review includes legacy single-key mod defaults");
            Check(warning.Text.Contains("Lab custom donor") && warning.Text.Contains("Conflicts to overwrite"), "Bulk review names the displaced custom owner");
            Check(!warning.Text.Split('\n').Any(line => line.StartsWith("Copy:")), "Bulk review excludes unmodified native defaults");
            Click(tab, "Cancel");
            Check(File.ReadAllText(SharedSettings.PathName) == seeded, "Cancel bulk review preserves persisted assignments");
            Click(tab, "Review mod defaults"); Click(tab, "Apply all");
            Check(ModControls.Current.Bindings.Overrides["shortcuts-lab/legacy"] == "Ctrl+Alt+F22", "Bulk apply remains a draft before Preferences OK");
            prefs.Close(); prefs = null; await Task.Delay(500);
            Check(File.ReadAllText(SharedSettings.PathName) == seeded, "Preferences Cancel discards staged bulk overwrite");
            prefs = OpenPreferences(); tab = await Editor(prefs);
            Click(tab, "Review mod defaults"); Click(tab, "Apply all"); Save(prefs); prefs = null;
            await Until(() => ModControls.Current.Bindings.Overrides.GetValueOrDefault("shortcuts-lab/legacy") == "Ctrl+Alt+F23");
            Check(ModControls.Current.Bindings.Overrides["shortcuts-lab/donor"] == "Ctrl+Alt+F24", "Bulk overwrite preserves the donor's unrelated alternative");
            Check(ModControls.Current.Bindings.Overrides[nativeCopy] == "F24 > G", "Bulk overwrite preserves unrelated native remap");
            await Until(() => Read()["mods"]![ModControls.SettingsId]!["Bindings"]!["Overrides"]!["shortcuts-lab/legacy"]!.GetValue<string>() == "Ctrl+Alt+F23");
            Check(Read()["mods"]![ModControls.SettingsId]!["Bindings"]!["Overrides"]!["shortcuts-lab/legacy"]!.GetValue<string>() == "Ctrl+Alt+F23", "Bulk save persists into live JSON");
            prefs = OpenPreferences(); tab = await Editor(prefs);
            Select(tab, "better-shortcuts/design-elements"); Click(tab, "Change shortcut");
            var recorder = Recorder(tab); Key(recorder, System.Windows.Input.Key.F23); Key(recorder, System.Windows.Input.Key.G);
            Check((string)Host.Property(recorder, "Gesture") == "F23 > G", "Native recorder handles two routed strokes");
            Key(recorder, System.Windows.Input.Key.Escape);
            Check(ModControls.Current.Bindings.Overrides["better-shortcuts/design-elements"] == "Alt+Shift+D", "Recorder Escape preserves the live binding");
            Click(tab, "Change shortcut"); recorder = Recorder(tab);
            Key(recorder, System.Windows.Input.Key.F23); Key(recorder, System.Windows.Input.Key.G); Key(recorder, System.Windows.Input.Key.Return);
            Check(prefs.IsVisible && ModControls.Current.Bindings.Overrides["better-shortcuts/design-elements"] == "Alt+Shift+D", "Enter stages the chord without saving Preferences");
            Save(prefs); prefs = null;
            await Until(() => ModControls.Current.Bindings.Overrides.GetValueOrDefault("better-shortcuts/design-elements") == "F23 > G");
            await Until(() => Read()["mods"]![ModControls.SettingsId]!["Bindings"]!["Overrides"]!["better-shortcuts/design-elements"]!.GetValue<string>() == "F23 > G");
            Check(Read()["mods"]![ModControls.SettingsId]!["Bindings"]!["Overrides"]!["better-shortcuts/design-elements"]!.GetValue<string>() == "F23 > G", "Recorder chord saves through native Preferences into JSON");
            var menu = Menus((Menu)Host.Field(main, "mainMenu")).Single(m => Equals(m.Header, "Toggle Design Elements"));
            Check(menu.InputGestureText == "F23 then G", "Saved chord immediately refreshes the real View menu");
            var document = Host.Field(main, "_DocHost");
            var properties = Host.Field(document, "_toolWindow_Properties");
            Host.Call(properties, "Float"); await Task.Delay(300);
            Check(Get(properties, "IsFloating") is true && Get(properties, "State")?.ToString() == "Docked", "Floating tool reproduces Actipro Docked state");
            foreach (var id in new[] { "right-dock", "all-sidebars" })
            {
                var action = Commands.All.Single(c => c.Definition.Id == "better-shortcuts/" + id).Definition;
                Check(action.CanExecute(), id + " available with other docked panels");
                action.Execute(); await Task.Delay(200);
                Check(Get(properties, "State")?.ToString() == "Docked" && Get(properties, "IsFloating") is true, id + " leaves floating Properties unchanged");
                action.Execute(); await Task.Delay(200);
            }
            Host.Call(properties, "Close"); await Task.Delay(200);
            var allDocks = Commands.All.Single(c => c.Definition.Id == "better-shortcuts/all-sidebars").Definition;
            allDocks.Execute(); await Task.Delay(200);
            Check(Get(properties, "IsOpen") is false, "All docks leaves closed Properties closed");
            allDocks.Execute();
            await AdapterAcceptance();
            // Leave the two remaps active for physical main-window/context-menu checks.
            File.WriteAllText(Path.Combine(Output, "original-settings.json"), original);
            File.WriteAllText(Path.Combine(Output, "result.json"), JsonSerializer.Serialize(new { success = true, checks, physicalInput = false }, SettingsJson.Format));
        }
        catch (Exception ex)
        {
            prefs?.Close();
            if (original is not null) File.WriteAllText(Path.Combine(Output, "original-settings.json"), original);
            File.WriteAllText(Path.Combine(Output, "result.json"), JsonSerializer.Serialize(new { success = false, checks, error = ex.ToString(), physicalInput = false }, SettingsJson.Format));
        }
    }
}
