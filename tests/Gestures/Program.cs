using System.Windows.Input;
using Designer.Modules.BetterShortcuts;

int checks = 0;
void Check(bool condition, string message) { if (!condition) throw new Exception(message); checks++; }
var control = ModifierKeys.Control;
var zoom = control | ModifierKeys.Shift;
var zero = new KeyGesture(Key.D0, control, "Ctrl+0");
var keypad = new KeyGesture(Key.NumPad0, control, "Ctrl+0");
Check(new KeyGestureConverter().ConvertToInvariantString(zero) == "Ctrl+0,Ctrl+0", "The regression fixture must reproduce the native display-string serialization.");
Check(GestureFormatting.Join(new[] { zero, new KeyGesture(Key.D0, control, "Other label"), keypad }) == "Ctrl+0; Ctrl+NumPad0", "Deduplicate physical identities and discard display aliases, preserving keypad keys.");
var plus = GestureFormatting.Join(new[] { new KeyGesture(Key.OemPlus, zoom, "Ctrl+Shift++"), new KeyGesture(Key.Add, zoom, "Ctrl+Shift++") });
Check(plus == "Ctrl+Shift+OemPlus; Ctrl+Shift+Add", "Application zoom must not repeat native display labels.");
Check(KeybindingPresentation.Text(plus, false) == "Ctrl + Shift + = or Ctrl + Shift + NumPad +", "Application zoom must distinguish main-keyboard and keypad alternatives.");
Check(GestureFormatting.Serialize(new KeyGesture(Key.Delete, ModifierKeys.None, "Del")) == "Delete", "Delete/Del is one key.");
Check(GestureFormatting.Serialize(new KeyGesture(Key.F7, ModifierKeys.None, "F7")) == "F7", "Function-key labels must not duplicate.");
foreach (var gesture in new[] { zero, keypad, new KeyGesture(Key.OemPlus, zoom, "Ctrl+Shift++"), new KeyGesture(Key.Add, zoom), new KeyGesture(Key.OemComma, control, "Ctrl+,"), new KeyGesture(Key.Oem6, control, "Ctrl+]"), new KeyGesture(Key.F7) })
{
    var parsed = (KeyGesture)new KeyGestureConverter().ConvertFromInvariantString(GestureFormatting.Serialize(gesture))!;
    Check(parsed.Key == gesture.Key && parsed.Modifiers == gesture.Modifiers, "Canonical settings must round-trip the exact key identity.");
}
var recording = new ShortcutRecording();
recording.Set("Ctrl+D"); recording.Begin(); recording.Capture(Key.LeftCtrl, control);
Check(recording.IsRecording && recording.Gesture == "Ctrl+D", "A modifier alone must not replace a gesture.");
recording.Capture(Key.Escape, ModifierKeys.None);
Check(!recording.IsRecording && recording.Gesture == "Ctrl+D", "Escape must cancel without changing the prior gesture.");
recording.Begin(); recording.Capture(Key.A, ModifierKeys.None);
Check(recording.IsRecording && recording.Gesture == "Ctrl+D", "Plain typing is not a supported global binding.");
recording.Capture(Key.Enter, ModifierKeys.None);
Check(recording.IsRecording && recording.Gesture == "Ctrl+D", "Bare Enter must not finish recording or save preferences.");
recording.Capture(Key.F11, control | ModifierKeys.Alt, true);
Check(recording.IsRecording, "Auto-repeat must not capture a shortcut.");
recording.Capture(Key.F11, control | ModifierKeys.Alt);
Check(!recording.IsRecording && recording.Gesture == "Ctrl+Alt+F11", "Capture modifiers and key as a single canonical gesture.");
recording.Capture(Key.A, control);
Check(recording.Gesture == "Ctrl+Alt+F11", "Keys outside a recording session must not alter the candidate.");
recording.Begin(); recording.Capture(Key.NumPad0, control);
Check(recording.Gesture == "Ctrl+NumPad0", "Recording must preserve keypad identity.");
recording.Begin(); recording.Capture(Key.OemPlus, control | ModifierKeys.Shift);
Check(recording.Gesture == "Ctrl+Shift+OemPlus", "Recording must preserve OEM key and exact modifiers.");
recording.Begin(); recording.Capture(Key.Enter, control);
Check(!recording.IsRecording && recording.Gesture == "Ctrl+Return", "Modified Enter must be capturable without saving Preferences.");
recording.Begin(); recording.Cancel();
Check(!recording.IsRecording && recording.Gesture == "Ctrl+Return", "Focus-loss cancellation must preserve the previous candidate.");
recording.Begin(); recording.Set("Ctrl+S");
Check(!recording.IsRecording && recording.Gesture == "Ctrl+S", "Changing rows must end recording and replace the candidate.");
Check(ShortcutRecording.Resolve(Key.System, Key.F10, Key.None, Key.None) == Key.F10, "Alt shortcuts must use SystemKey.");
Check(ShortcutRecording.Resolve(Key.ImeProcessed, Key.None, Key.A, Key.None) == Key.A, "IME-processed keys must resolve before validation.");
Check(ShortcutRecording.Resolve(Key.DeadCharProcessed, Key.None, Key.None, Key.Oem7) == Key.Oem7, "Dead keys must retain their physical identity.");
var wheel = new ShortcutWheelSteps();
Check(wheel.Take(-120, 3) == -3 && wheel.Take(120, 3) == 3, "Wheel direction and Windows line count must be honored.");
Check(wheel.Take(-20, 3) == 0 && wheel.Take(-20, 3) == -1, "Precision wheel partial deltas must accumulate.");
Check(wheel.Take(-240, -1) == -2 && wheel.Take(120, -1) == 1, "Windows page scrolling must use detents, not line count.");
Check(wheel.Take(-120, 0) == 0, "Disabled wheel scrolling must be respected.");
recording.Set("Ctrl+D"); recording.Begin();
Check(!recording.HasCandidate, "Opening an editor must not accept the previous binding as a newly recorded candidate.");
recording.Capture(Key.LeftCtrl, control);
Check(!recording.HasCandidate, "Modifiers alone must not enable confirmation.");
recording.Capture(Key.K, control);
Check(recording.HasCandidate && recording.Gesture == "Ctrl+K", "A complete capture enables keyboard confirmation.");
recording.Cancel();
Check(!recording.HasCandidate, "Cancel discards a recorded candidate even after key capture ends.");

var draft = new Dictionary<string, string> { ["absent/mod"] = "Ctrl+Alt+F12", ["target"] = "Ctrl+D" };
var assignments = new[] {
    new ShortcutAssignment("target", "Target", [new(Key.D, control)]),
    new ShortcutAssignment("copy", "Copy", [new(Key.C, control), new(Key.Insert, control)]),
    new ShortcutAssignment("copy2", "Copy elsewhere", [new(Key.C, control)]),
    new ShortcutAssignment("zoom", "Zoom", [zero, keypad])
};
var transfer = ShortcutChanges.Prepare(draft, "target", "Ctrl+C", [new(Key.C, control)], assignments);
Check(transfer.Conflicts.Length == 2 && transfer.Conflicts.Any(c => c.StartsWith("Copy elsewhere")), "Warn about every affected command, not only the first collision.");
Check(transfer.Overrides["target"] == "Ctrl+C" && transfer.Overrides["copy2"] == "", "Overwrite transfers the gesture and explicitly disables commands with no remaining bindings.");
Check(transfer.Overrides["copy"] == "Ctrl+Insert", "Overwrite must retain non-conflicting alternatives.");
Check(draft.Count == 2 && draft["target"] == "Ctrl+D" && !draft.ContainsKey("copy"), "Preview or canceled overwrite must not mutate the draft.");
Check(transfer.Overrides["absent/mod"] == "Ctrl+Alt+F12", "Overwrite must preserve temporarily absent commands.");
var keypadTransfer = ShortcutChanges.Prepare(draft, "target", "Ctrl+0", [zero], assignments);
Check(keypadTransfer.Overrides["zoom"] == "Ctrl+NumPad0", "Number-row conflicts must not remove the distinct keypad binding.");
var reset = ShortcutChanges.Prepare(draft, "target", null, [new(Key.C, control), new(Key.Insert, control)], assignments);
Check(!reset.Overrides.ContainsKey("target") && reset.Overrides["copy"] == "" && reset.Conflicts.Length == 2, "A conflicting reset must preview transfer of all restored defaults.");
var clear = ShortcutChanges.Prepare(draft, "target", "", [], assignments);
Check(clear.Conflicts.Length == 0 && clear.Overrides.Count == draft.Count, "Clear must not change another command.");
var same = ShortcutChanges.Prepare(draft, "target", "Ctrl+D", [new(Key.D, control)], assignments);
Check(same.Conflicts.Length == 0, "A command must not conflict with itself.");
Check(GestureFormatting.Join(GestureFormatting.ParseList("Ctrl+S; Ctrl+Insert; Ctrl+S")) == "Ctrl+S; Ctrl+Insert", "Stored alternative gestures must round-trip and deduplicate.");
Check(GestureFormatting.ParseList("Ctrl+S").Single().Key == Key.S && GestureFormatting.ParseList("").Length == 0, "Existing single-value and cleared preferences remain compatible.");
var retained = ShortcutChanges.Prepare(draft, "target", "Ctrl+C", [new(Key.C, control)],
    [new("native", "Native action", [new(Key.C, control), new(Key.Delete), new(Key.F7)])]);
Check(GestureFormatting.ParseList(retained.Overrides["native"]).Select(g => g.Key).SequenceEqual(new[] { Key.Delete, Key.F7 }), "Transfer must preserve native ordinary-key and function-key alternatives.");
var editorClaims = new Dictionary<string, string> { ["copy"] = "Ctrl+C; Ctrl+Insert", ["target"] = "Ctrl+D", ["absent/editor"] = "Ctrl+H" };
var editorTransfer = ShortcutChanges.Prepare(draft, "target", "Ctrl+C", [new(Key.C, control)], assignments, editorClaims);
Check(editorTransfer.EditorOverrides["copy"] == "Ctrl+Insert", "Editor overwrite ownership must retain only surviving alternatives.");
Check(!editorTransfer.EditorOverrides.ContainsKey("target") && editorTransfer.EditorOverrides["absent/editor"] == "Ctrl+H", "Target changes must re-evaluate editor ownership while preserving absent owners.");
Check(editorClaims["target"] == "Ctrl+D" && editorClaims["copy"] == "Ctrl+C; Ctrl+Insert", "Cancel must preserve editor overwrite ownership too.");
var oldSettings = System.Text.Json.JsonSerializer.Deserialize<ShortcutSettings>("{\"Overrides\":{\"target\":\"Ctrl+D\"}}")!;
Check(oldSettings.Overrides["target"] == "Ctrl+D" && oldSettings.EditorOverrides is null, "Old preferences must load without inventing editor overwrite consent.");
var newSettings = new ShortcutSettings(transfer.Overrides, new() { ["target"] = "Ctrl+C" });
var reopened = System.Text.Json.JsonSerializer.Deserialize<ShortcutSettings>(System.Text.Json.JsonSerializer.Serialize(newSettings))!;
Check(reopened.Overrides["copy"] == "Ctrl+Insert" && reopened.EditorOverrides!["target"] == "Ctrl+C", "Saved conflict transfer and explicit editor ownership must survive reopen.");
Console.WriteLine($"Passed {checks} WPF formatting, recorder, conflict and wheel checks. No Designer process or UI was started.");
