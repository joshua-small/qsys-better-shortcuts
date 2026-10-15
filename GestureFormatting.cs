using System.Windows.Input;

namespace Designer.Modules.BetterShortcuts;

internal static class GestureFormatting
{
    internal static string SerializeStroke(Key key, ModifierKeys modifiers)
    {
        var parts = new List<string>();
        if (modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
        if (modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
        if (modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
        if (modifiers.HasFlag(ModifierKeys.Windows)) parts.Add("Windows");
        parts.Add(new KeyConverter().ConvertToInvariantString(key)!);
        return string.Join("+", parts);
    }
    // DisplayString is presentation metadata, not a second binding. Do not let
    // KeyGestureConverter append it to the serialized key and modifiers.
    internal static string Serialize(KeyGesture? gesture) => gesture is null ? "" :
        new KeyGestureConverter().ConvertToInvariantString(new KeyGesture(gesture.Key, gesture.Modifiers))!;
    internal static string Join(IEnumerable<KeyGesture> gestures) => string.Join("; ", gestures
        .DistinctBy(g => (g.Key, g.Modifiers)).Select(Serialize));
    internal static KeyGesture[] ParseList(string text) => string.IsNullOrWhiteSpace(text) ? [] : text.Split(';')
        .Select(part => new KeyGestureConverter().ConvertFromInvariantString(part.Trim()) as KeyGesture
            ?? throw new ArgumentException("Invalid shortcut."))
        .DistinctBy(g => (g.Key, g.Modifiers)).ToArray();
    internal static bool Same(KeyGesture left, KeyGesture right) => left.Key == right.Key && left.Modifiers == right.Modifiers;
}
