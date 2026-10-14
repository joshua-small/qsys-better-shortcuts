using QsysMods.Core;

namespace Designer.Modules.BetterShortcuts;

public sealed class ShortcutModule : ModModule
{
    private ShortcutBindings? bindings;
    public ShortcutModule() => Mods.Register("better-shortcuts", main =>
    {
        Commands.DiscoverNative(main); EditorActions.Register(main); ExpandedActions.Register(main);
        bindings = new ShortcutBindings();
        Preferences.RegisterControlsPage("better-shortcuts", "Better Shortcuts", draft => ShortcutEditor.Create(draft, bindings));
        Commands.Register(new("better-shortcuts/preferences", "Better Shortcuts…", "Better Shortcuts", () => Preferences.Open("better-shortcuts"), () => true, InMenu: true));
    });
}
