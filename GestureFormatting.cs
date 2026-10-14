using System.Windows.Input;

namespace Designer.Modules.BetterShortcuts;

internal static class GestureFormatting
{
    // DisplayString is presentation metadata, not a second binding. Do not let
    // KeyGestureConverter append it to the serialized key and modifiers.
    internal static string Serialize(KeyGesture? gesture) => gesture is null ? "" :
        new KeyGestureConverter().ConvertToInvariantString(new KeyGesture(gesture.Key, gesture.Modifiers))!;
    internal static string Join(IEnumerable<KeyGesture> gestures) => string.Join("; ", gestures
        .DistinctBy(g => (g.Key, g.Modifiers)).Select(Serialize));
}
