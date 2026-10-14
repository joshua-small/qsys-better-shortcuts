using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using QsysMods.Core;

namespace Designer.Modules.BetterShortcuts;

internal sealed class ShortcutBindings : IDisposable
{
    private readonly Window window;
    private readonly Dictionary<RoutedCommand, InputGesture[]> originals = new();
    private readonly List<(UIElement Owner, InputBinding Binding)> removed = new(), added = new();
    private readonly List<WeakReference<UIElement>> scopes = new();
    private readonly List<(MenuItem Item, string Original)> hints = new();
    private ShortcutSettings current = new(new());
    internal ShortcutBindings(Window window)
    {
        this.window = window;
        // The native floating Component Controls window has a separate input route.
        EventManager.RegisterClassHandler(Host.Type("QSC.QSys.windows.ComponentInfo"), FrameworkElement.LoadedEvent, new RoutedEventHandler((sender, _) =>
        {
            if (sender is not UIElement element || scopes.Any(s => s.TryGetTarget(out var target) && ReferenceEquals(target, element))) return;
            scopes.Add(new(element)); BindFloating(element);
        }), true);
    }
    internal static KeyGesture? Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var gesture = new KeyGestureConverter().ConvertFromInvariantString(text.Trim()) as KeyGesture ?? throw new ArgumentException("Enter a valid shortcut.");
        if (gesture.Key == Key.None || ((gesture.Modifiers & (ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Windows)) == 0 && gesture.Key is not (>= Key.F1 and <= Key.F24)))
            throw new ArgumentException("Use Ctrl, Alt, or a function key so ordinary typing remains available.");
        return gesture;
    }
    internal static string Format(KeyGesture? gesture) => GestureFormatting.Serialize(gesture);
    private IEnumerable<KeyGesture> Effective(RegisteredCommand command, Dictionary<string, string> overrides)
    {
        if (overrides.TryGetValue(command.Definition.Id, out var text)) return GestureFormatting.ParseList(text);
        if (command.Definition.Native is { } native) return originals.TryGetValue(native, out var saved) ? saved.OfType<KeyGesture>() : native.InputGestures.OfType<KeyGesture>();
        return command.Definition.DefaultGesture is { } value ? new[] { value } : Array.Empty<KeyGesture>();
    }
    internal string Display(string id, Dictionary<string, string> overrides) => GestureFormatting.Join(Effective(Commands.All.Single(c => c.Definition.Id == id), overrides));
    internal string Input(string id, Dictionary<string, string> overrides) =>
        Format(Effective(Commands.All.Single(c => c.Definition.Id == id), overrides).FirstOrDefault());
    internal ShortcutChange Prepare(string id, string? value, Dictionary<string, string> draft, Dictionary<string, string>? editorOverrides = null)
    {
        var selected = Commands.All.Single(c => c.Definition.Id == id);
        var desired = value is null ? Effective(selected, new()).ToArray() : GestureFormatting.ParseList(value);
        var change = ShortcutChanges.Prepare(draft, id, value, desired, Commands.All.Select(c =>
            new ShortcutAssignment(c.Definition.Id, c.Definition.Name, Effective(c, draft).ToArray())), editorOverrides);
        var editorConflicts = OriginalInputs(selected.Definition.Native is null && selected.Definition.AllowInText)
            .Where(input => desired.Any(g => g.Key == input.Key && g.Modifiers == input.Modifiers)
            && !Commands.All.Any(c => ReferenceEquals(c.Definition.Native, input.Command) || ReferenceEquals(c.Command, input.Command)))
            .ToArray();
        if (editorConflicts.Length > 0)
            change.EditorOverrides[id] = GestureFormatting.Join(editorConflicts.Select(i => new KeyGesture(i.Key, i.Modifiers)));
        var editorNames = editorConflicts.Select(input =>
            (input.Command is RoutedUICommand ui ? ui.Text : input.Command is RoutedCommand routed ? routed.Name : "Editor binding")
                + " (" + KeybindingPresentation.Text(Format(new KeyGesture(input.Key, input.Modifiers)), false) + ")");
        return change with { Conflicts = change.Conflicts.Concat(editorNames).Distinct().ToArray() };
    }
    internal CommandSnapshot[] Snapshot(Dictionary<string, string> overrides) => Commands.All.Select(c => new CommandSnapshot(
        c.Definition.Id, c.Definition.Name, c.Definition.Native is null ? c.Definition.Group : "Q-SYS",
        Display(c.Definition.Id, overrides), Display(c.Definition.Id, new()), Context(c),
        c.Definition.Native?.OwnerType.FullName, c.Definition.Native?.Name)).ToArray();
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
            : "Provided by " + command.Definition.Group + "; command must be available"
    };
    internal void Validate(ShortcutSettings settings)
    {
        if (settings.Overrides is null || settings.Overrides.Count > 10000) throw new ArgumentException("Invalid shortcut preferences.");
        foreach (var pair in settings.Overrides)
        {
            var command = Commands.All.FirstOrDefault(c => c.Definition.Id == pair.Key);
            foreach (var gesture in GestureFormatting.ParseList(pair.Value))
            {
                // Conflict transfer can retain a provider's ordinary-key alternative.
                if (command is not null && Effective(command, new()).Any(g => GestureFormatting.Same(g, gesture))) continue;
                Parse(Format(gesture)); // Retain bindings for temporarily absent mods.
            }
        }
        if (settings.EditorOverrides?.Count > 10000) throw new ArgumentException("Invalid editor shortcut preferences.");
        foreach (var pair in settings.EditorOverrides ?? new())
        {
            var command = Commands.All.FirstOrDefault(c => c.Definition.Id == pair.Key);
            foreach (var gesture in GestureFormatting.ParseList(pair.Value))
                if (command is not null && !Effective(command, settings.Overrides).Any(g => GestureFormatting.Same(g, gesture)))
                    throw new ArgumentException("An editor overwrite must match an assigned shortcut.");
        }
        var occupied = new Dictionary<(Key, ModifierKeys), RegisteredCommand>();
        foreach (var command in Commands.All)
            foreach (var gesture in Effective(command, settings.Overrides))
            {
                var key = (gesture.Key, gesture.Modifiers);
                if (occupied.TryGetValue(key, out var other) && other.Definition.Id != command.Definition.Id)
                {
                    // Host commands may intentionally share a key in different native contexts.
                    if (other.Definition.Native is not null && command.Definition.Native is not null
                        && Effective(other, new()).Any(g => GestureFormatting.Same(g, gesture))
                        && Effective(command, new()).Any(g => GestureFormatting.Same(g, gesture))) continue;
                    throw new ArgumentException(Format(gesture) + " conflicts: " + other.Definition.Name + " / " + command.Definition.Name);
                }
                occupied[key] = command;
            }
        // Explicit editor InputBindings are warned about in Prepare and suppressed
        // reversibly by Apply. They must not turn a confirmed overwrite into an error.
    }
    internal void Apply(ShortcutSettings settings)
    {
        Validate(settings); Restore(); current = settings;
        SuppressEditorBindings(Elements(window));
        foreach (var command in Commands.All)
        {
            var def = command.Definition;
            if (def.Native is { } native)
            {
                if (!settings.Overrides.ContainsKey(def.Id)) continue;
                originals.TryAdd(native, native.InputGestures.Cast<InputGesture>().ToArray());
                native.InputGestures.Clear();
                foreach (var element in Elements(window))
                    foreach (var input in element.InputBindings.Cast<InputBinding>().Where(b => ReferenceEquals(b.Command, native)).ToArray())
                    { element.InputBindings.Remove(input); removed.Add((element, input)); }
            }
            foreach (var gesture in Effective(command, settings.Overrides)) Add(window, command, gesture);
        }
        foreach (var weak in scopes.ToArray()) if (weak.TryGetTarget(out var scope)) BindFloating(scope);
        foreach (var item in Menus((ItemsControl)window.FindName("mainMenu")))
        {
            var entry = Commands.All.FirstOrDefault(c => ReferenceEquals(c.Command, item.Command) || ReferenceEquals(c.Definition.Native, item.Command));
            if (entry is null || (entry.Definition.Native is not null && !settings.Overrides.ContainsKey(entry.Definition.Id))) continue;
            hints.Add((item,item.InputGestureText)); item.InputGestureText = Display(entry.Definition.Id, settings.Overrides);
        }
        CommandManager.InvalidateRequerySuggested();
    }
    private void Add(UIElement target, RegisteredCommand entry, KeyGesture gesture)
    {
        // Routed commands preserve the host's CanExecute checks and focus route.
        var binding = new KeyBinding(entry.Command, gesture) { CommandTarget = window };
        target.InputBindings.Add(binding); added.Add((target, binding));
    }
    private void BindFloating(UIElement target)
    {
        SuppressEditorBindings(Elements(target), floating: true);
        foreach (var command in Commands.All.Where(c => c.Definition.Native is null && c.Definition.AllowInText))
            foreach (var gesture in Effective(command, current.Overrides))
                if (!target.InputBindings.OfType<KeyBinding>().Any(b => b.Key == gesture.Key && b.Modifiers == gesture.Modifiers)) Add(target, command, gesture);
    }
    private IEnumerable<KeyBinding> OriginalInputs(bool includeFloating) => Elements(window)
        .Concat(includeFloating ? scopes.SelectMany(s => s.TryGetTarget(out var target) ? Elements(target) : []) : [])
        .Distinct().SelectMany(e => e.InputBindings.OfType<KeyBinding>())
        .Where(b => !added.Any(a => ReferenceEquals(a.Binding, b)))
        .Concat(removed.Where(r => includeFloating || Window.GetWindow(r.Owner) == window).Select(r => r.Binding).OfType<KeyBinding>()).Distinct();
    private void SuppressEditorBindings(IEnumerable<UIElement> elements, bool floating = false)
    {
        // Only keys explicitly approved in the conflict prompt may displace an
        // unregistered editor binding. Retained alternatives get no extra ownership.
        var claimed = Commands.All.Where(c => current.EditorOverrides?.ContainsKey(c.Definition.Id) == true)
            .Where(c => !floating || (c.Definition.Native is null && c.Definition.AllowInText))
            .SelectMany(c => GestureFormatting.ParseList(current.EditorOverrides![c.Definition.Id])
                .Select(g => (Command: c, Gesture: g))).ToArray();
        foreach (var element in elements)
            foreach (var input in element.InputBindings.OfType<KeyBinding>().ToArray())
            {
                if (added.Any(a => ReferenceEquals(a.Binding, input))) continue;
                if (!claimed.Any(c => c.Gesture.Key == input.Key && c.Gesture.Modifiers == input.Modifiers
                    && !ReferenceEquals(c.Command.Command, input.Command) && !ReferenceEquals(c.Command.Definition.Native, input.Command))) continue;
                element.InputBindings.Remove(input); removed.Add((element, input));
            }
    }
    private static IEnumerable<UIElement> Elements(DependencyObject root)
    {
        if (root is UIElement element) yield return element;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++) foreach (var child in Elements(VisualTreeHelper.GetChild(root, i))) yield return child;
    }
    private void Restore()
    {
        foreach (var (item, original) in hints) item.InputGestureText = original; hints.Clear();
        foreach (var (owner, binding) in added) owner.InputBindings.Remove(binding); added.Clear();
        foreach (var (owner, binding) in removed) owner.InputBindings.Add(binding); removed.Clear();
        foreach (var pair in originals) { pair.Key.InputGestures.Clear(); foreach (var gesture in pair.Value) pair.Key.InputGestures.Add(gesture); }
    }
    private static IEnumerable<MenuItem> Menus(ItemsControl parent)
    { foreach (var item in parent.Items.OfType<MenuItem>()) { yield return item; foreach (var child in Menus(item)) yield return child; } }
    public void Dispose() => Restore();
}
