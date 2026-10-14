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

Console.WriteLine($"Passed {checks} WPF formatting, recorder and wheel checks. No Designer process or UI was started.");
