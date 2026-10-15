using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QsysMods.Core;

namespace Designer.Modules.ShortcutsLab;

public sealed partial class Probe
{
    // Setup only: all keyboard acceptance is real input observed by Physical.cs.
    private static async Task PrepareEditors(Window main)
    {
        try
        {
            if (!main.Title.StartsWith("untitled", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Editor setup requires a synthetic untitled design.");
            if (SharedSettings.Error is not null) throw new InvalidOperationException(SharedSettings.Error);
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
            ObservePhysicalWindows();
            PhysicalReceipt("editor-ready", main);
            File.WriteAllText(Path.Combine(Output, "editors-ready.json"), JsonSerializer.Serialize(new {
                ready = true, physicalInputTested = false,
                coreName = Get(core, "Name"),
                instruction = "Click the UCI script editor. Test ordinary editing, Ctrl+Alt+F8/F9 then G exclusions, F10/F11 then G opt-ins; F5 must show the native unnamed-Core validation message, F6 emulates and F7 disconnects. Never select a physical Core."
            }, SettingsJson.Format));
        }
        catch (Exception ex)
        { File.WriteAllText(Path.Combine(Output, "editors-ready.json"), JsonSerializer.Serialize(new { ready = false, error = ex.ToString() }, SettingsJson.Format)); }
    }
}
