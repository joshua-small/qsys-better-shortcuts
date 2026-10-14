using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;

namespace Designer.Modules.BetterShortcuts;

internal sealed class ShortcutRecorder : Border
{
    private readonly ShortcutRecording recording = new();
    private readonly Button record = new() { Content = "Record shortcut", Padding = new Thickness(10, 5, 10, 5), Margin = new Thickness(0, 0, 10, 0) };
    private readonly KeybindingCell preview = new();
    private readonly TextBlock prompt = new() { TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center };
    private readonly HashSet<Key> held = new();
    private Window? owner;
    internal string Gesture => recording.Gesture;
    internal bool IsRecording => recording.IsRecording;
    internal event Action? Changed;
    public ShortcutRecorder()
    {
        Name = "ShortcutRecorder";
        AutomationProperties.SetName(this, "Shortcut recorder");
        var layout = new DockPanel(); DockPanel.SetDock(record, Dock.Left); layout.Children.Add(record);
        var display = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        display.Children.Add(preview); display.Children.Add(prompt); layout.Children.Add(display); Child = layout;
        record.Click += (_, _) => { if (recording.IsRecording) Cancel(); else Begin(); };
        AddHandler(Keyboard.PreviewKeyDownEvent, new KeyEventHandler(HandleKeyDown), true);
        AddHandler(Keyboard.PreviewKeyUpEvent, new KeyEventHandler(HandleKeyUp), true);
        LostKeyboardFocus += (_, _) => { if (!IsKeyboardFocusWithin && recording.IsRecording) Cancel(); };
        IsEnabledChanged += (_, _) => { if (!IsEnabled && recording.IsRecording) Cancel(); };
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
    internal void SetGesture(string gesture) { recording.Set(gesture); held.Clear(); Update(); }
    internal void Begin() { recording.Begin(); record.Focus(); Update(); }
    internal void Capture(Key key, ModifierKeys modifiers, bool repeat = false)
    { recording.Capture(key, modifiers, repeat); Update(); }
    private void Cancel() { recording.Cancel(); Update(); }
    private void Deactivated(object? sender, EventArgs e) { if (recording.IsRecording) Cancel(); held.Clear(); }
    private static Key ActualKey(KeyEventArgs e) => ShortcutRecording.Resolve(e.Key, e.SystemKey, e.ImeProcessedKey, e.DeadCharProcessedKey);
    private void HandleKeyDown(object sender, KeyEventArgs e)
    {
        Key key = ActualKey(e);
        // Keep repeats and the corresponding key-up consumed after capture, so
        // Enter cannot save Preferences and Alt cannot activate its menu.
        if (!recording.IsRecording) { if (held.Contains(key)) e.Handled = true; return; }
        e.Handled = true; held.Add(key); Capture(key, e.KeyboardDevice.Modifiers, e.IsRepeat);
    }
    private void HandleKeyUp(object sender, KeyEventArgs e)
    { if (held.Remove(ActualKey(e)) || recording.IsRecording) e.Handled = true; }
    private void Update()
    {
        record.Content = recording.IsRecording ? "Cancel recording" : "Record shortcut";
        preview.Row = new("recording", "Recorded shortcut", string.IsNullOrEmpty(Gesture) ? "Unassigned" : Gesture, "", "", "", "recording", null, "");
        prompt.Text = recording.Message.Length > 0 ? recording.Message : "Record a combination, then choose Assign.";
        AutomationProperties.SetHelpText(record, prompt.Text);
        Changed?.Invoke();
    }
}
