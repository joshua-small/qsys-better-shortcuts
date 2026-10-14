using QsysMods.Core;

namespace Designer.Modules.BetterShortcuts;

public sealed class ShortcutModule : ModModule
{
    private ShortcutBindings? bindings;
    public ShortcutModule() => Mods.Register("better-shortcuts", main =>
    {
        EditorActions.Register(main); Commands.DiscoverNative(main);
        bindings = new ShortcutBindings(main);
        var settings = Preferences.Register("better-shortcuts", "Better Shortcuts", () => new ShortcutSettings(new()),
            value => ShortcutEditor.Create(value, bindings), bindings.Validate);
        bindings.Apply(settings.Value);
        settings.Changed += () => bindings.Apply(settings.Value);
        Commands.Changed += () => bindings.Apply(settings.Value);
        Commands.Register(new("better-shortcuts/preferences", "Better Shortcuts…", "Better Shortcuts", () => Preferences.Open("better-shortcuts"), () => true, InMenu: true));
        main.Closed += (_, _) => bindings.Dispose();
    });
}
