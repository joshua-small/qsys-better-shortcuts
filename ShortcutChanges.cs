using System.Windows.Input;

namespace Designer.Modules.BetterShortcuts;

internal sealed record ShortcutAssignment(string Id, string Name, KeyGesture[] Gestures);
internal sealed record ShortcutChange(Dictionary<string, string> Overrides, string[] Conflicts, Dictionary<string, string> EditorOverrides);

internal static class ShortcutChanges
{
    // Work on a copy: dismissing the warning must leave every staged binding intact.
    // An empty value disables a command; null restores all of its provider defaults.
    internal static ShortcutChange Prepare(Dictionary<string, string> draft, string id, string? value,
        KeyGesture[] desired, IEnumerable<ShortcutAssignment> assignments, Dictionary<string, string>? editorOverrides = null)
    {
        var next = new Dictionary<string, string>(draft);
        var editorNext = new Dictionary<string, string>(editorOverrides ?? new());
        editorNext.Remove(id); // Changing/resetting the target re-evaluates its editor conflicts.
        if (value is null) next.Remove(id); else next[id] = value;
        var conflicts = new List<string>();
        foreach (var other in assignments.Where(a => a.Id != id))
        {
            var matching = other.Gestures.Where(g => desired.Any(d => GestureFormatting.Same(g, d))).ToArray();
            if (matching.Length == 0) continue;
            conflicts.Add(other.Name + " (" + KeybindingPresentation.Text(GestureFormatting.Join(matching), false) + ")");
            next[other.Id] = GestureFormatting.Join(other.Gestures.Where(g => !matching.Any(m => GestureFormatting.Same(g, m))));
            if (editorNext.TryGetValue(other.Id, out var claimed))
            {
                var retained = GestureFormatting.Join(GestureFormatting.ParseList(claimed).Where(g => !matching.Any(m => GestureFormatting.Same(g, m))));
                if (retained.Length == 0) editorNext.Remove(other.Id); else editorNext[other.Id] = retained;
            }
        }
        return new(next, conflicts.ToArray(), editorNext);
    }
}
