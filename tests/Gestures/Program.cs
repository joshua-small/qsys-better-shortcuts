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
Console.WriteLine($"Passed {checks} WPF gesture-formatting checks. No Designer process or UI was started.");
