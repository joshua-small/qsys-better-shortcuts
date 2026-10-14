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
    internal static string Format(KeyGesture? gesture) => gesture is null ? "" : new KeyGestureConverter().ConvertToInvariantString(gesture)!;
    private IEnumerable<KeyGesture> Effective(RegisteredCommand command, Dictionary<string, string> overrides)
    {
        if (overrides.TryGetValue(command.Definition.Id, out var text)) return Parse(text) is { } gesture ? new[] { gesture } : Array.Empty<KeyGesture>();
        if (command.Definition.Native is { } native) return originals.TryGetValue(native, out var saved) ? saved.OfType<KeyGesture>() : native.InputGestures.OfType<KeyGesture>();
        return command.Definition.DefaultGesture is { } value ? new[] { value } : Array.Empty<KeyGesture>();
    }
    internal string Display(string id, Dictionary<string, string> overrides) => string.Join("; ", Effective(Commands.All.Single(c => c.Definition.Id == id), overrides).Select(Format));
    internal void Validate(ShortcutSettings settings)
    {
        if (settings.Overrides is null || settings.Overrides.Count > 10000) throw new ArgumentException("Invalid shortcut preferences.");
        foreach (var pair in settings.Overrides) Parse(pair.Value); // Retain bindings for temporarily absent mods.
        var occupied = new Dictionary<(Key, ModifierKeys), RegisteredCommand>();
        foreach (var command in Commands.All)
            foreach (var gesture in Effective(command, settings.Overrides))
            {
                var key = (gesture.Key, gesture.Modifiers);
                if (occupied.TryGetValue(key, out var other) && other.Definition.Id != command.Definition.Id)
                {
                    // Host commands may intentionally share a key in different native contexts.
                    if (other.Definition.Native is not null && command.Definition.Native is not null
                        && !settings.Overrides.ContainsKey(other.Definition.Id) && !settings.Overrides.ContainsKey(command.Definition.Id)) continue;
                    throw new ArgumentException(Format(gesture) + " conflicts: " + other.Definition.Name + " / " + command.Definition.Name);
                }
                occupied[key] = command;
            }
        foreach (var element in Elements(window))
            foreach (var input in element.InputBindings.OfType<KeyBinding>().Where(b => !added.Any(a => ReferenceEquals(a.Binding, b))))
            {
                if (!occupied.TryGetValue((input.Key, input.Modifiers), out var desired)) continue;
                if (ReferenceEquals(input.Command, desired.Definition.Native)) continue;
                if (Commands.All.Any(c => ReferenceEquals(c.Definition.Native, input.Command) && settings.Overrides.ContainsKey(c.Definition.Id))) continue;
                if (desired.Definition.Native is not null && !settings.Overrides.ContainsKey(desired.Definition.Id)) continue;
                throw new ArgumentException(Format(new KeyGesture(input.Key, input.Modifiers)) + " is already used by an editor binding.");
            }
    }
    internal void Apply(ShortcutSettings settings)
    {
        Validate(settings); Restore(); current = settings;
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
        foreach (var command in Commands.All.Where(c => c.Definition.Native is null && c.Definition.AllowInText))
            foreach (var gesture in Effective(command, current.Overrides))
                if (!target.InputBindings.OfType<KeyBinding>().Any(b => b.Key == gesture.Key && b.Modifiers == gesture.Modifiers)) Add(target, command, gesture);
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
