using System.Collections;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using QsysMods.Core;

namespace Designer.Modules.ShortcutsLab;

// Observer by default; the explicit -Suite launcher option runs native integration
// checks. Never load outside the profile-redirected disposable acceptance copy.
public sealed partial class Probe : ModModule
{
    private DispatcherTimer? timer;
    private static readonly string Output = Path.Combine(AppContext.BaseDirectory, "shortcut-evidence");
    private static readonly Type Adapters = typeof(BetterShortcuts.ShortcutModule).Assembly.GetType("Designer.Modules.BetterShortcuts.ExpandedActions")!;
    private static object? Get(object? item, string name) => item?.GetType().GetProperty(name, Host.Instance)?.GetValue(item)
        ?? item?.GetType().GetField(name, Host.Instance)?.GetValue(item);
    private static object? Adapter(string name) => Adapters.GetProperty(name, Host.Static)?.GetValue(null);
    private static object[] Items(object? value) => value is IEnumerable list ? list.Cast<object>().ToArray() : [];
    public Probe()
    {
        if (!Host.IsLab || !File.Exists(Path.Combine(AppContext.BaseDirectory, ".settings-live-lab.json"))) return;
        Mods.Register("shortcuts-lab", main =>
        {
            var nativePath = (string)Host.Type("QSC.Common.SettingsManager").GetProperty("Path", Host.Static)!.GetValue(null)!;
            if (!nativePath.StartsWith(Path.Combine(AppContext.BaseDirectory, "profile"), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Native preferences are not redirected.");
            Directory.CreateDirectory(Output);
            bool suite = Environment.GetEnvironmentVariable("QSYS_SHORTCUTS_LAB_SUITE") == "1";
            if (suite)
            {
                Commands.Register(new("shortcuts-lab/legacy", "Lab legacy default", "Acceptance", () => { }, () => true,
                    new KeyGesture(System.Windows.Input.Key.F23, ModifierKeys.Control | ModifierKeys.Alt)));
                Commands.Register(new("shortcuts-lab/donor", "Lab custom donor", "Acceptance", () => { }, () => true));
            }
            bool started = false;
            timer = new DispatcherTimer(DispatcherPriority.ContextIdle) { Interval = TimeSpan.FromMilliseconds(300) };
            timer.Tick += async (_, _) =>
            {
                try { Snapshot(main); }
                catch (Exception ex) { File.WriteAllText(Path.Combine(Output, "observer-error.txt"), ex.ToString()); }
                if (suite && !started && main.IsEnabled && File.Exists(SharedSettings.PathName) && Mods.IsReady("better-shortcuts"))
                { started = true; await Task.Delay(2500); await Acceptance(main); }
            };
            timer.Start();
        });
    }
    private static void Snapshot(Window main)
    {
        var document = Host.Field(main, "_DocHost");
        var focused = Keyboard.FocusedElement as FrameworkElement;
        var ancestors = new List<string>();
        for (var item = focused as DependencyObject; item is not null; item = ControlInput.Parent(item)) ancestors.Add(item.GetType().FullName!);
        var layers = Adapter("Layers");
        var tree = Adapter("LayerTree");
        var canvas = Adapter("Canvas");
        var page = Adapter("Page");
        var uci = Adapter("Uci");
        var props = new[] { "_toolWindow_Inventory", "_toolWindow_Properties", "_toolWindow_Library" };
        var commands = Commands.All.Where(c => c.Definition.Id.StartsWith("better-shortcuts/")).Select(c => new {
            id = c.Definition.Id, available = Safe(() => c.Definition.CanExecute()),
            binding = ModControls.Bindings.Display(c.Definition.Id, ModControls.Current.Bindings.Overrides)
        });
        var state = new {
            pid = Environment.ProcessId, time = DateTime.UtcNow, title = main.Title, error = SharedSettings.Error,
            ready = Mods.IsReady("better-shortcuts"), focused = focused?.GetType().FullName, focusName = focused?.Name,
            selectionLength = (focused as TextBox)?.SelectionLength, textInput = ControlInput.IsTextInput(), ancestors,
            windows = Application.Current.Windows.Cast<Window>().Select(w => new { w.Title, type = w.GetType().FullName, w.IsActive, w.IsEnabled }),
            panels = props.ToDictionary(p => p, p => new { state = Get(Host.Field(document, p), "State")?.ToString(), open = Get(Host.Field(document, p), "IsOpen"), floating = Get(Host.Field(document, p), "IsFloating") }),
            page = page?.GetType().FullName, canvas = canvas?.GetType().FullName, editable = Adapter("EditableCanvas"),
            uci = uci is not null, uciEditable = Adapter("UciEditable"),
            selectedLayers = Items(Get(tree, "SelectedItems")).Select(l => new { type = l.GetType().FullName, name = Get(l, "Name"), hidden = Get(l, "IsHidden"), locked = Get(l, "IsLocked") }),
            layerItems = Items(Get(tree, "Items")).Select(l => new { type = l.GetType().FullName, name = Get(l, "Name") }),
            nameEditor = (Adapter("NameEditor") as FrameworkElement)?.Name,
            swatch = Adapter("FocusedSwatch")?.GetType().FullName,
            commands
        };
        File.WriteAllText(Path.Combine(Output, "state.json"), JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true }));
    }
    private static bool Safe(Func<bool> value) { try { return value(); } catch { return false; } }
}
