using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QsysMods.Core;
using Keys = System.Windows.Input.Key;

namespace Designer.Modules.ShortcutsLab;

public sealed partial class Probe
{
    // Setup and a separately reported Legacy routed-event regression. Physical
    // acceptance starts at the editor-ready marker and uses external input only.
    private static async Task PrepareEditors(Window main)
    {
        try
        {
            if (!main.Title.StartsWith("untitled", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Editor setup requires a synthetic untitled design.");
            if (SharedSettings.Error is not null) throw new InvalidOperationException(SharedSettings.Error);
            bool legacy = Environment.GetEnvironmentVariable("QSYS_SHORTCUTS_LAB_LEGACY_EDITOR") == "1";
            if (legacy)
            {
                var settings = Read(); settings["qsys"]!["general"]!["ScriptEditor"] = "Legacy";
                File.WriteAllText(SharedSettings.PathName, SettingsJson.Text(settings));
                await Until(() => SharedSettings.Error is not null || Get(Get(Activator.CreateInstance(
                    Host.Type("QSC.QSys.windows.Dialogs.GeneralSettings")), "Model"), "ScriptEditor")?.ToString() == "Legacy");
                if (SharedSettings.Error is not null) throw new InvalidOperationException(SharedSettings.Error);
            }
            // Native Deploy.CustomRun stops at a local validation message for
            // an unnamed Core, before calling DeployProvider. Never target hardware.
            var manager = Host.Type("QSC.QSys.App").GetField("Manager", Host.Static)!.GetValue(null)!;
            var core = Get(Get(manager, "Document"), "Core")!;
            core.GetType().GetProperty("Name")!.SetValue(core, "");
            var create = Commands.All.Single(c => c.Definition.Name == "Add New User Control Interface").Definition;
            if (!create.CanExecute()) throw new InvalidOperationException("UCI creation unavailable.");
            create.Execute();
            await Until(() => Adapter("Uci") is not null && Adapter("Canvas") is FrameworkElement);
            var add = Commands.All.Single(c => c.Definition.Id == "better-shortcuts/uci-add-button").Definition;
            if (!add.CanExecute()) throw new InvalidOperationException("UCI button creation unavailable.");
            add.Execute(); await Task.Delay(100);
            var chooser = PresentationSource.CurrentSources.Cast<PresentationSource>().Select(s => s.RootVisual)
                .OfType<DependencyObject>().SelectMany(Visual).OfType<ContextMenu>().Single(m => m.IsOpen);
            chooser.Items.OfType<MenuItem>().First().RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
            chooser.IsOpen = false;
            await Task.Delay(200);
            var script = Commands.All.Single(c => c.Definition.Id == "better-shortcuts/uci-script").Definition;
            if (!script.CanExecute()) throw new InvalidOperationException("UCI script unavailable.");
            script.Execute(); await Task.Delay(1200);
            if (legacy) await LegacyRouting(main);
            ObservePhysicalWindows();
            PhysicalReceipt("editor-ready", main);
            File.WriteAllText(Path.Combine(Output, "editors-ready.json"), JsonSerializer.Serialize(new {
                ready = true, physicalInputTested = false,
                editor = legacy ? "Legacy" : "VSCode",
                coreName = Get(core, "Name"),
                instruction = "Click the UCI script editor. Test ordinary editing, Ctrl+Alt+F8/F9 then G exclusions, F10/F11 then G opt-ins; F5 must show the native unnamed-Core validation message, F6 emulates and F7 disconnects. Never select a physical Core."
            }, SettingsJson.Format));
        }
        catch (Exception ex)
        { File.WriteAllText(Path.Combine(Output, "editors-ready.json"), JsonSerializer.Serialize(new { ready = false, error = ex.ToString() }, SettingsJson.Format)); }
    }
    private static async Task LegacyRouting(Window main)
    {
        var editor = Visual(main).OfType<FrameworkElement>().Single(v => v.IsVisible &&
            v.GetType().FullName == "ActiproSoftware.Windows.Controls.SyntaxEditor.SyntaxEditor");
        var view = Visual(editor).OfType<UIElement>().Single(v => v.GetType().FullName ==
            "ActiproSoftware.Windows.Controls.SyntaxEditor.Primitives.EditorView");
        main.Activate();
        await Until(() => view.Focus() && ReferenceEquals(Keyboard.FocusedElement, view));
        Check(ControlInput.IsTextInput(), "Native Legacy editor view is recognized as text input");
        Check(!Stroke(view, Keys.F18) && !Stroke(view, Keys.F17) && Runs("single") == 0 && Runs("chord-g") == 0,
            "Native Legacy editor excludes ordinary single and chord prefixes");
        Stroke(view, Keys.F19); Stroke(view, Keys.F20); Stroke(view, Keys.G);
        Check(Runs("opt-single") == 1 && Runs("opt-chord") == 1, "Native Legacy editor allows explicit single and chord opt-ins once");
        Stroke(view, Keys.F20); Stroke(view, Keys.Escape); Stroke(view, Keys.G);
        Check(Runs("opt-chord") == 1, "Native Legacy editor Escape cancels the opted-in chord");
        File.WriteAllText(Path.Combine(Output, "legacy-routing.json"), JsonSerializer.Serialize(new {
            passed = true, physicalInputTested = false, focused = view.GetType().FullName, checks
        }, SettingsJson.Format));
    }
}
