using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;

namespace Designer.Modules.BetterShortcuts;

internal sealed class ShortcutRecorder : Border
{
    private readonly ShortcutRecording recording = new();
    private readonly KeybindingCell preview = new();
    private readonly TextBlock prompt = new() { TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center };
    private readonly HashSet<Key> held = new();
    private Window? owner;
    private bool editing;
    internal string Gesture => recording.Gesture;
    internal bool IsRecording => recording.IsRecording;
    internal bool HasCandidate => recording.HasCandidate;
    internal bool AllowOrdinaryKey { set => recording.AllowOrdinaryKey = value; }
    internal event Action? Changed;
    internal event Action? Confirmed;
    internal event Action? Canceled;
    public ShortcutRecorder()
    {
        Name = "ShortcutRecorder";
        Focusable = true; Padding = new Thickness(12); MinHeight = 80;
        AutomationProperties.SetName(this, "Shortcut recorder");
        var display = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        display.Children.Add(preview); display.Children.Add(prompt); Child = display;
        AddHandler(Keyboard.PreviewKeyDownEvent, new KeyEventHandler(HandleKeyDown), true);
        AddHandler(Keyboard.PreviewKeyUpEvent, new KeyEventHandler(HandleKeyUp), true);
        IsEnabledChanged += (_, _) => { if (!IsEnabled && editing) Cancel(); };
        Loaded += (_, _) =>
        {
            if (owner is not null) owner.Deactivated -= Deactivated;
            owner = Window.GetWindow(this);
            if (owner is not null) owner.Deactivated += Deactivated;
        };
        Unloaded += (_, _) =>
        {
            if (owner is not null) owner.Deactivated -= Deactivated;
            owner = null; Cancel(); held.Clear();
        };
        Update();
    }
    internal void SetGesture(string gesture) { editing = false; recording.Set(gesture); held.Clear(); Update(); }
    internal void Begin() { editing = true; held.Clear(); recording.Begin(); Focus(); Update(); }
    internal void Capture(Key key, ModifierKeys modifiers, bool repeat = false)
    { recording.Capture(key, modifiers, repeat); Update(); }
    internal void Confirm() { if (editing && HasCandidate) Confirmed?.Invoke(); }
    internal void Cancel() { var wasEditing = editing; editing = false; recording.Cancel(); Update(); if (wasEditing) Canceled?.Invoke(); }
    internal void Finish() { editing = false; recording.Set(Gesture); Update(); }
    private void Deactivated(object? sender, EventArgs e) { if (editing) Cancel(); held.Clear(); }
    private static Key ActualKey(KeyEventArgs e) => ShortcutRecording.Resolve(e.Key, e.SystemKey, e.ImeProcessedKey, e.DeadCharProcessedKey);
    private void HandleKeyDown(object sender, KeyEventArgs e)
    {
        Key key = ActualKey(e);
        // Keep repeats and the corresponding key-up consumed after capture, so
        // Enter cannot save Preferences and Alt cannot activate its menu.
        if (!editing) { if (held.Contains(key)) e.Handled = true; return; }
        if (key == Key.Tab && (e.KeyboardDevice.Modifiers & ~ModifierKeys.Shift) == ModifierKeys.None) return;
        e.Handled = true;
        if (e.IsRepeat || !held.Add(key)) return;
        var modifiers = e.KeyboardDevice.Modifiers;
        if (key == Key.Escape && modifiers == ModifierKeys.None) { Cancel(); return; }
        if (key == Key.Enter && modifiers == ModifierKeys.None && HasCandidate) { Confirm(); return; }
        if (!recording.IsRecording) recording.NextStroke();
        Capture(key, modifiers);
    }
    private void HandleKeyUp(object sender, KeyEventArgs e)
    { if (held.Remove(ActualKey(e)) || editing) e.Handled = true; }
    private void Update()
    {
        preview.Row = new("recording", "Recorded shortcut", string.IsNullOrEmpty(Gesture) ? "Unassigned" : Gesture, "", "", "", "recording", null, "");
        prompt.Text = recording.Message.Length > 0 ? recording.Message : "Press a shortcut. Enter accepts; Escape cancels.";
        AutomationProperties.SetHelpText(this, prompt.Text);
        Changed?.Invoke();
    }
}
