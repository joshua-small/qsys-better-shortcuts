namespace Designer.Modules.BetterShortcuts;

// EditorOverrides stores only explicitly confirmed takeovers of direct editor
// bindings. Missing data in older preferences means no such takeovers.
public sealed record ShortcutSettings(Dictionary<string, string> Overrides, Dictionary<string, string>? EditorOverrides = null);
