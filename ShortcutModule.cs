using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QsysMods.Core;

namespace Designer.Modules.BetterShortcuts;

public sealed record ShortcutSettings(Dictionary<string, string> Overrides);
public sealed class ShortcutModule : ModModule
{
    private ShortcutBindings? bindings;
    public ShortcutModule() => Mods.Register("better-shortcuts", main =>
    {
        EditorActions.Register(main); Commands.DiscoverNative(main);
        bindings = new ShortcutBindings(main);
        var settings = Preferences.Register("better-shortcuts", "Better Shortcuts", () => new ShortcutSettings(new()), value => Editor(value, bindings), bindings.Validate);
        bindings.Apply(settings.Value);
        settings.Changed += () => bindings.Apply(settings.Value);
        Commands.Changed += () => bindings.Apply(settings.Value);
        Commands.Register(new("better-shortcuts/preferences", "Better Shortcuts…", "Better Shortcuts", () => Preferences.Open("better-shortcuts"), () => true, InMenu: true));
        main.Closed += (_, _) => bindings.Dispose();
    });
    private static PreferenceEditor<ShortcutSettings> Editor(ShortcutSettings value, ShortcutBindings bindings)
    {
        var draft = new Dictionary<string, string>(value.Overrides);
        var panel = new StackPanel { Margin = new Thickness(16) };
        panel.Children.Add(new TextBlock { Text = "Choose a command, then enter a shortcut such as Ctrl+Alt+D. Clear disables it; Reset restores its default. Changes apply when you choose OK.", TextWrapping = TextWrapping.Wrap });
        var search = new TextBox { Margin = new Thickness(0,12,0,8), ToolTip = "Search commands" }; panel.Children.Add(search);
        var list = new ListBox { Height = 180, DisplayMemberPath = "Label" }; panel.Children.Add(list);
        var gesture = new TextBox { Margin = new Thickness(0,12,0,8), ToolTip = "Shortcut (for example Ctrl+Alt+D)" }; panel.Children.Add(gesture);
        var status = new TextBlock { TextWrapping = TextWrapping.Wrap };
        var buttons = new WrapPanel(); panel.Children.Add(buttons); panel.Children.Add(status);
        var choices = Commands.All.OrderBy(c => c.Definition.Group).ThenBy(c => c.Definition.Name).Select(c => new Choice(c.Definition.Id, c.Definition.Group + " — " + c.Definition.Name)).ToArray();
        list.ItemsSource = choices;
        search.TextChanged += (_, _) => list.ItemsSource = choices.Where(c => c.Label.Contains(search.Text, StringComparison.OrdinalIgnoreCase)).ToArray();
        list.SelectionChanged += (_, _) => { if (list.SelectedItem is Choice choice) { gesture.Text = bindings.Display(choice.Id, draft); status.Text = ""; } };
        void Button(string label, Action<string> action)
        {
            var button = new Button { Content = label, Margin = new Thickness(0,0,8,8), Padding = new Thickness(10,5,10,5) };
            button.Click += (_, _) =>
            {
                if (list.SelectedItem is not Choice choice) return;
                try { action(choice.Id); gesture.Text = bindings.Display(choice.Id, draft); status.Text = "Staged. Choose OK to save."; }
                catch (Exception ex) { status.Text = ex.Message; }
            }; buttons.Children.Add(button);
        }
        Button("Assign", id =>
        {
            var candidate = new Dictionary<string, string>(draft) { [id] = ShortcutBindings.Format(ShortcutBindings.Parse(gesture.Text)) };
            bindings.Validate(new(candidate)); draft = candidate;
        });
        Button("Clear", id => draft[id] = "");
        Button("Reset", id => draft.Remove(id));
        return new(panel, () => new(draft));
    }
    private sealed record Choice(string Id, string Label);
}
