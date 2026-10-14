using QsysMods.Core;

namespace Designer.Modules.BetterShortcuts;

// Presentation adapter only. Core is the sole binding owner and settings authority.
internal sealed class ShortcutBindings
{
    private ControlBindings Engine => ModControls.Bindings;
    internal string Display(string id, Dictionary<string, string> overrides) => Engine.Display(id, overrides);
    internal string Input(string id, Dictionary<string, string> overrides) => Engine.Input(id, overrides);
    internal BindingChange Prepare(string id, string? value, Dictionary<string, string> draft, Dictionary<string, string>? editors = null)
        => Engine.Prepare(id, value, draft, editors);
    internal void Validate(BindingSettings settings) => Engine.Validate(settings);
    internal CommandSnapshot[] Snapshot(ControlSettings settings) => Commands.All.Select(c =>
    {
        var description = ModControls.Describe(c, settings);
        bool replacement = ModControls.Proposals.Any(p => p.IsReplacement && p.CommandId == c.Definition.Id);
        return new CommandSnapshot(c.Definition.Id, c.Definition.Name, description.Source,
            Display(c.Definition.Id, settings.Bindings.Overrides), Display(c.Definition.Id, new()),
            replacement || c.Definition.When is not null ? description.When : Context(c),
            c.Definition.Native?.OwnerType.FullName, c.Definition.Native?.Name, description.Details, replacement && description.Source != "Q-SYS");
    }).ToArray();
    private static string Context(RegisteredCommand command) => command.Definition.Id switch
    {
        "better-shortcuts/design-elements" => "Design Elements panel is open",
        "better-shortcuts/properties" => "Properties panel is open",
        "better-shortcuts/schematic-elements" => "Schematic Elements panel is open",
        "better-shortcuts/all-sidebars" => "At least one sidebar is docked or auto-hidden",
        "better-shortcuts/focus-schematic-search" => "Schematic Elements panel is open; search field available",
        var id when id.StartsWith("better-shortcuts/focus-", StringComparison.Ordinal) => "Design Elements panel is open",
        "better-shortcuts/dark-mode" => "Designer; native theme command available",
        "better-shortcuts/component-controls" => "Native Component Controls command available",
        "better-shortcuts/programmer-mode" => "Script Programmer Mode menu is enabled",
        "better-shortcuts/preferences" => "Designer",
        _ => command.Definition.Native is not null ? "Designer decides availability in the focused editor"
            : "Designer; availability depends on the current selection and focus"
    };
}
