using System.Windows.Input;

namespace Designer.Modules.BetterShortcuts;

internal sealed class ShortcutRecording
{
    public string Gesture { get; private set; } = "";
    public bool IsRecording { get; private set; }
    public bool HasCandidate { get; private set; }
    public string Message { get; private set; } = "";
    public void Set(string gesture) { Gesture = gesture; IsRecording = false; HasCandidate = false; Message = ""; }
    public void Begin() { IsRecording = true; HasCandidate = false; Message = "Press a shortcut. Escape cancels."; }
    public void Cancel() { IsRecording = false; HasCandidate = false; Message = "Recording canceled."; }
    public void Capture(Key key, ModifierKeys modifiers, bool repeat = false)
    {
        if (!IsRecording || repeat) return;
        if (key == Key.Escape && modifiers == ModifierKeys.None) { Cancel(); return; }
        if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin)
        { Message = "Hold the modifiers and press another key. Escape cancels."; return; }
        if (key is Key.None or Key.System or Key.ImeProcessed or Key.DeadCharProcessed)
        { Message = "That key could not be recorded. Try another combination."; return; }
        if ((modifiers & (ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Windows)) == 0 && key is not (>= Key.F1 and <= Key.F24))
        { Message = "Include Ctrl or Alt, or use a function key. Escape cancels."; return; }
        try
        {
            Gesture = GestureFormatting.Serialize(new KeyGesture(key, modifiers));
            IsRecording = false; HasCandidate = true; Message = "Press Enter to accept, or another combination to change it. Escape cancels.";
        }
        catch (ArgumentException) { Message = "That combination cannot be assigned. Try another shortcut."; }
    }
    internal static Key Resolve(Key key, Key systemKey, Key imeKey, Key deadKey) => key switch
    { Key.System => systemKey, Key.ImeProcessed => imeKey, Key.DeadCharProcessed => deadKey, _ => key };
}

// Wheel units are 120 per detent. Retain partial deltas from precision wheels,
// and honor Windows' disabled, line-count, and one-page settings.
internal sealed class ShortcutWheelSteps
{
    private long remainder;
    internal int Take(int delta, int lines)
    {
        if (lines == 0) { remainder = 0; return 0; }
        remainder += (long)delta * (lines < 0 ? 1 : lines);
        int steps = (int)(remainder / 120); remainder %= 120; return steps;
    }
}
