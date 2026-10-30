using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using QsysMods.Core;
using InputKey = System.Windows.Input.Key;

namespace Designer.Modules.ShortcutsLab;

public sealed partial class Probe
{
    private const string ReviewTarget = "shortcuts-lab/review-target";
    private const string ReviewDonor = "shortcuts-lab/review-donor";
    private const string ReviewUnrelated = "shortcuts-lab/review-unrelated";

    // Inspect the real Preferences session instead of substituting a second editor.
    // Fail if the private session shape changes; never guess a different draft.
    private static ControlDraft ReviewDraft(Window preferences)
    {
        var sessions = typeof(Preferences).GetField("sessions", Host.Static)!.GetValue(null)!;
        object?[] args = [preferences, null];
        if (sessions.GetType().GetMethod("TryGetValue")!.Invoke(sessions, args) is not true)
            throw new InvalidOperationException("No native Preferences draft was attached.");
        return (ControlDraft)args[1]!.GetType().GetProperty("Controls")!.GetValue(args[1])!;
    }

    private static bool SameBindings(BindingSettings a, BindingSettings b) =>
        SettingsJson.Same(JsonSerializer.SerializeToNode(a), JsonSerializer.SerializeToNode(b));

    private static async Task ReviewAcceptance(Window main)
    {
        Window? prefs = null;
        string original = File.ReadAllText(SharedSettings.PathName);
        File.WriteAllText(Path.Combine(Output, "reviews-settings-before.json"), original);
        try
        {
            prefs = OpenPreferences();
            var tab = await Editor(prefs);
            var shared = ReviewDraft(prefs);
            var overlay = Preferences.Logical(tab).OfType<Grid>().Single(e => e.Name == "ShortcutEditOverlay");
            var accept = Preferences.Logical(tab).OfType<Button>().Single(e => e.Name == "ShortcutAccept");
            var table = Preferences.Logical(tab).OfType<DataGrid>().Single(e => e.Name == "ShortcutTable");
            int stages = 0;
            shared.Changed += () => stages++;
            BindingSettings Seed(string donor)
            {
                var keys = new Dictionary<string, string>(ModControls.Current.Bindings.Overrides)
                {
                    [ReviewTarget] = "F21", [ReviewDonor] = donor + "; F24 > J",
                    [ReviewUnrelated] = "F24 > G"
                };
                var editors = new Dictionary<string, string>(ModControls.Current.Bindings.EditorOverrides ?? new())
                { [ReviewUnrelated] = "F24 > G" };
                var value = new BindingSettings(keys, editors);
                ModControls.Bindings.Validate(value);
                shared.StageBindings(value);
                return value;
            }
            void OpenReview(string kind)
            {
                if (kind == "bulk") Click(tab, "Review mod defaults");
                else
                {
                    Select(tab, ReviewTarget);
                    Click(tab, kind == "reset" ? "Reset" : "Change shortcut");
                    if (kind == "individual") Key(Recorder(tab), InputKey.F22);
                }
                Check(overlay.Visibility == Visibility.Visible && accept.IsEnabled, kind + " preview is visible and confirmable");
                Check(Equals(accept.Content, kind == "bulk" ? "Apply all" : "Overwrite"), kind + " shows the conflict confirmation");
            }
            foreach (var kind in new[] { "individual", "reset", "bulk" })
            {
                Seed(kind == "individual" ? "F22" : "F23");
                OpenReview(kind);
                var newer = new BindingSettings(new(shared.Value.Bindings.Overrides) { [ReviewUnrelated] = "F24 > H" },
                    new(shared.Value.Bindings.EditorOverrides!) { [ReviewUnrelated] = "F24 > H" });
                shared.StageBindings(newer);
                int before = stages;
                Check(overlay.Visibility == Visibility.Collapsed, kind + " closes when the shared draft changes");
                // Invoke the old control's callback even though it is now hidden.
                accept.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                Host.Call(Recorder(tab), "Confirm");
                Check(stages == before && SameBindings(shared.Value.Bindings, newer),
                    kind + " stale callbacks preserve newer assignments and editor consent");
                Check(table.Items.Cast<object>().Any(row => (string)Get(row, "Id")! == ReviewUnrelated
                    && ((string)Get(row, "KeybindingText")!).Contains("H")), kind + " table reflects the newer binding");
            }
            foreach (var recording in new[] { true, false })
            {
                var expected = Seed("F22");
                if (recording) { Select(tab, ReviewTarget); Click(tab, "Change shortcut"); }
                else OpenReview("individual");
                int before = stages;
                Commands.Register(new("shortcuts-lab/registry-" + recording, "Registry notification " + recording,
                    "Acceptance", () => { }, () => true));
                Check(overlay.Visibility == Visibility.Collapsed, "Registry refresh closes " + (recording ? "recording" : "review"));
                accept.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                Check(stages == before && SameBindings(shared.Value.Bindings, expected), "Registry refresh does not apply the old plan");
            }
            Seed("F22"); OpenReview("individual");
            int normalBefore = stages;
            accept.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            accept.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            Check(stages == normalBefore + 1 && overlay.Visibility == Visibility.Collapsed, "Normal overwrite stages once despite repeated confirmation");
            Check(shared.Value.Bindings.Overrides[ReviewTarget] == "F22"
                && shared.Value.Bindings.Overrides[ReviewDonor] == "F24 > J"
                && shared.Value.Bindings.EditorOverrides![ReviewUnrelated] == "F24 > G",
                "Normal overwrite transfers only the conflict and preserves unrelated consent");
            Check(table.SelectedItem is { } selected && (string)Get(selected, "Id")! == ReviewTarget,
                "Normal overwrite retains selection after synchronous draft notification");
            prefs.Close(); prefs = null;
            await Task.Delay(300);
            Check(File.ReadAllText(SharedSettings.PathName) == original, "Preferences Cancel preserves saved settings after all staged reviews");

            prefs = OpenPreferences(); tab = await Editor(prefs); shared = ReviewDraft(prefs);
            overlay = Preferences.Logical(tab).OfType<Grid>().Single(e => e.Name == "ShortcutEditOverlay");
            accept = Preferences.Logical(tab).OfType<Button>().Single(e => e.Name == "ShortcutAccept");
            table = Preferences.Logical(tab).OfType<DataGrid>().Single(e => e.Name == "ShortcutTable");
            Seed("F23"); OpenReview("bulk");
            normalBefore = stages;
            shared.Changed += () => stages++;
            accept.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            Check(stages == normalBefore + 1, "Fresh bulk review stages once");
            Check(shared.Value.Bindings.Overrides[ReviewTarget] == "F23"
                && shared.Value.Bindings.Overrides[ReviewDonor] == "F24 > J", "Fresh bulk review transfers its reviewed conflict");
            var saved = shared.Value.Bindings;
            Save(prefs); prefs = null;
            await Until(() => SameBindings(ModControls.Current.Bindings, saved));
            prefs = OpenPreferences(); tab = await Editor(prefs);
            Check(SameBindings(ReviewDraft(prefs).Value.Bindings, saved), "Preferences OK and reopen retain the accepted fresh plan");
            prefs.Close(); prefs = null;
            File.WriteAllText(Path.Combine(Output, "reviews-result.json"), JsonSerializer.Serialize(new {
                success = true, checks, physicalInput = false, eventDelivery = "real Preferences draft and command registry",
                settingsAfter = SharedSettings.PathName, originalSettings = "reviews-settings-before.json"
            }, SettingsJson.Format));
        }
        catch (Exception ex)
        {
            prefs?.Close();
            File.WriteAllText(Path.Combine(Output, "reviews-result.json"), JsonSerializer.Serialize(new {
                success = false, checks, error = ex.ToString(), physicalInput = false
            }, SettingsJson.Format));
        }
    }
}
