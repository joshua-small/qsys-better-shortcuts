namespace Designer.Modules.BetterShortcuts;

// This projection is independent of WPF so coverage, matching and staged display
// can be verified without starting Designer or altering the command registry.
internal sealed record CommandSnapshot(string Id, string Name, string Source, string Keybinding,
    string DefaultKeybinding, string When, string? NativeOwner = null, string? NativeName = null);

internal sealed record ShortcutRow(string Id, string Command, string Keybinding, string When,
    string Source, string Details, string? CommandId, string? SourceUrl, string Section)
{
    public bool CanAssign => CommandId is not null;
    public string KeybindingText => KeybindingPresentation.Text(Keybinding, !CanAssign);
    public KeybindingLine[] KeybindingLines => KeybindingPresentation.Lines(Keybinding, !CanAssign);
    public bool Matches(string query) => query.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
        .All(term => string.Join(" ", Command, Keybinding, KeybindingText, When, Source, Details, Section, CommandId)
            .Contains(term, StringComparison.OrdinalIgnoreCase));
}

internal static class ShortcutTable
{
    internal static readonly ShortcutRow[] InternalGestures =
    [
        new("internal/canvas-wheel", "Zoom canvas with mouse wheel", "Ctrl+mouse wheel", "Canvas zoom handler; Shift not held",
            "Q-SYS internals (reference)", "Designer 10.5 GraphicCanvas.Zoom. Static code evidence; physical delivery still needs testing. Also mentioned in the 10.4 release-note popup-zoom fix. Not remappable here.", null, null, "Additional native gestures"),
        new("internal/application-wheel", "Zoom application with mouse wheel", "Ctrl+Shift+mouse wheel", "Designer receives an unhandled wheel event",
            "Q-SYS internals (reference)", "Designer 10.5 MainWindow_MouseWheel changes the application scale. Owner confirmed application zoom in and out with this gesture on October 14, 2026. Not found in the reviewed Help pages or targeted Help search. Not remappable here.", null, null, "Additional native gestures")
    ];
    internal static ShortcutRow[] Build(IEnumerable<CommandSnapshot> commands, IReadOnlyDictionary<string, string> overrides)
    {
        var available = commands.ToArray();
        var matched = new HashSet<string>(StringComparer.Ordinal);
        var rows = new List<ShortcutRow>(InternalGestures);
        foreach (var reference in ShortcutCatalog.All)
        {
            // Names are not unique across mods. Match only the exact native identity.
            var candidates = reference.NativeName is null ? [] : available.Where(c =>
                c.NativeOwner == reference.NativeOwner && c.NativeName == reference.NativeName).ToArray();
            var command = candidates.Length == 1 ? candidates[0] : null;
            if (command is not null) matched.Add(command.Id);
            string keybinding = command is null ? reference.Keybinding : BoundText(command, overrides);
            string source = command is null ? "Q-SYS Help (reference)" : Source(command, overrides);
            string note = command is null
                ? "Reference only — this gesture or contextual action cannot be reassigned here."
                : $"Default in this Designer: {Unbound(command.DefaultKeybinding)}. Native availability and focus rules still apply.";
            rows.Add(new("help/" + reference.Id, reference.Command, keybinding, reference.When, source,
                $"{reference.Section}. Documented: {reference.Keybinding}. {reference.Details} {note}".Trim(),
                command?.Id, reference.SourceUrl, reference.Section));
        }
        foreach (var command in available.Where(c => !matched.Contains(c.Id)))
            rows.Add(new(command.Id, command.Name, BoundText(command, overrides), command.When,
                Source(command, overrides), $"Default: {Unbound(command.DefaultKeybinding)}. {command.When}",
                command.Id, null, command.Source));
        return rows.OrderBy(r => r.Command, StringComparer.CurrentCultureIgnoreCase).ThenBy(r => r.When).ToArray();
    }
    private static string Unbound(string value) => string.IsNullOrEmpty(value) ? "Unassigned" : value;
    private static string BoundText(CommandSnapshot command, IReadOnlyDictionary<string, string> overrides) =>
        overrides.TryGetValue(command.Id, out var value) && value.Length == 0 ? "Unassigned" : Unbound(command.Keybinding);
    private static string Source(CommandSnapshot command, IReadOnlyDictionary<string, string> overrides) =>
        overrides.ContainsKey(command.Id) ? "User · " + command.Source : command.Source;
}
