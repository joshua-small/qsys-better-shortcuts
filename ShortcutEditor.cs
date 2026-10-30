using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using QsysMods.Core;

namespace Designer.Modules.BetterShortcuts;

internal static class ShortcutEditor
{
    internal static FrameworkElement Create(ControlDraft shared, ShortcutBindings bindings)
    {
        var value = shared.Value.Bindings;
        var draft = new Dictionary<string, string>(value.Overrides);
        var draftEditors = new Dictionary<string, string>(value.EditorOverrides ?? new());
        var panel = new Grid { Margin = new Thickness(16) };
        // Preferences wraps every page in a ScrollViewer, which otherwise gives
        // the grid unlimited height and lets the table push its editor offscreen.
        panel.SetBinding(FrameworkElement.HeightProperty, new Binding(nameof(ScrollViewer.ViewportHeight))
        {
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ScrollViewer), 1),
            Converter = new ViewportHeightConverter()
        });
        foreach (var height in new[] { GridLength.Auto, GridLength.Auto, new GridLength(1, GridUnitType.Star), GridLength.Auto, GridLength.Auto, GridLength.Auto })
            panel.RowDefinitions.Add(new RowDefinition { Height = height });
        void Place(UIElement element, int row) { Grid.SetRow(element, row); panel.Children.Add(element); }
        Place(new TextBlock { Text = "Keyboard Shortcuts", FontSize = 20, FontWeight = FontWeights.SemiBold }, 0);
        var searchBar = new DockPanel { Margin = new Thickness(0, 10, 0, 8) };
        var count = new TextBlock { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(12, 0, 0, 0) };
        DockPanel.SetDock(count, Dock.Right); searchBar.Children.Add(count);
        searchBar.Children.Add(new TextBlock { Text = "Search", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) });
        var search = new TextBox { Name = "ShortcutSearch", ToolTip = "Search commands, keybindings, conditions, or sources", Padding = new Thickness(6) };
        AutomationProperties.SetName(search, "Search keyboard shortcuts"); searchBar.Children.Add(search); Place(searchBar, 1);
        var table = new DataGrid
        {
            Name = "ShortcutTable", AutoGenerateColumns = false, IsReadOnly = true,
            CanUserAddRows = false, CanUserDeleteRows = false, CanUserReorderColumns = false,
            SelectionMode = DataGridSelectionMode.Single, SelectionUnit = DataGridSelectionUnit.FullRow,
            HeadersVisibility = DataGridHeadersVisibility.Column, GridLinesVisibility = DataGridGridLinesVisibility.None,
            RowHeaderWidth = 0, MinHeight = 120, EnableRowVirtualization = true, AlternationCount = 2
        };
        AutomationProperties.SetName(table, "Keyboard shortcuts");
        ScrollViewer.SetVerticalScrollBarVisibility(table, ScrollBarVisibility.Auto);
        ShortcutScrolling.Attach(table);
        // Use Designer's changing light/dark resources rather than fixed colors.
        object Resource(string name) => Host.Type("ActiproSoftware.Windows.Themes.AssetResourceKeys").GetProperty(name, Host.Static)!.GetValue(null)!;
        var background = Resource("WindowTitleBarExtensionBackgroundActiveBrushKey");
        var foreground = Resource("ContainerForegroundLowNormalBrushKey");
        var selectedBackground = Resource("ListItemBackgroundSelectedNormalBrushKey");
        table.SetResourceReference(Control.BackgroundProperty, background);
        table.SetResourceReference(Control.ForegroundProperty, foreground);
        var headerStyle = new Style(typeof(DataGridColumnHeader));
        headerStyle.Setters.Add(new Setter(Control.BackgroundProperty, new DynamicResourceExtension(background)));
        headerStyle.Setters.Add(new Setter(Control.ForegroundProperty, new DynamicResourceExtension(foreground)));
        headerStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(8)));
        headerStyle.Setters.Add(new Setter(Control.FontWeightProperty, FontWeights.SemiBold));
        table.ColumnHeaderStyle = headerStyle;
        var cellStyle = new Style(typeof(DataGridCell));
        cellStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(8, 4, 8, 4)));
        cellStyle.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(0)));
        cellStyle.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.Transparent));
        var cellBorder = new FrameworkElementFactory(typeof(Border));
        cellBorder.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Control.BackgroundProperty));
        cellBorder.SetValue(Border.PaddingProperty, new TemplateBindingExtension(Control.PaddingProperty));
        var content = new FrameworkElementFactory(typeof(ContentPresenter));
        content.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        cellBorder.AppendChild(content);
        cellStyle.Setters.Add(new Setter(Control.TemplateProperty, new ControlTemplate(typeof(DataGridCell)) { VisualTree = cellBorder }));
        table.CellStyle = cellStyle;
        var rowStyle = new Style(typeof(DataGridRow));
        rowStyle.Setters.Add(new Setter(Control.BackgroundProperty, KeybindingCell.TableBrush("row")));
        rowStyle.Setters.Add(new Setter(FrameworkElement.ToolTipProperty, new Binding(nameof(ShortcutRow.Details))));
        var rowBorder = new FrameworkElementFactory(typeof(Border));
        rowBorder.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Control.BackgroundProperty));
        rowBorder.AppendChild(new FrameworkElementFactory(typeof(DataGridCellsPresenter)));
        rowStyle.Setters.Add(new Setter(Control.TemplateProperty, new ControlTemplate(typeof(DataGridRow)) { VisualTree = rowBorder }));
        var alternate = new Trigger { Property = ItemsControl.AlternationIndexProperty, Value = 1 };
        alternate.Setters.Add(new Setter(Control.BackgroundProperty, KeybindingCell.TableBrush("alternate")));
        rowStyle.Triggers.Add(alternate);
        var hover = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
        hover.Setters.Add(new Setter(Control.BackgroundProperty, KeybindingCell.TableBrush("hover")));
        rowStyle.Triggers.Add(hover);
        var selection = new Trigger { Property = DataGridRow.IsSelectedProperty, Value = true };
        selection.Setters.Add(new Setter(Control.BackgroundProperty, new DynamicResourceExtension(selectedBackground)));
        rowStyle.Triggers.Add(selection);
        table.RowStyle = rowStyle;
        void Column(string title, string property, double width)
        {
            var text = new Style(typeof(TextBlock));
            text.Setters.Add(new Setter(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis));
            text.Setters.Add(new Setter(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center));
            text.Setters.Add(new Setter(FrameworkElement.ToolTipProperty, new Binding(property)));
            table.Columns.Add(new DataGridTextColumn
            {
                Header = title, Binding = new Binding(property), SortMemberPath = property,
                Width = new DataGridLength(width, DataGridLengthUnitType.Star), MinWidth = 100, ElementStyle = text
            });
        }
        Column("Command", nameof(ShortcutRow.Command), 3);
        var keycaps = new FrameworkElementFactory(typeof(KeybindingCell));
        keycaps.SetBinding(KeybindingCell.RowProperty, new Binding());
        table.Columns.Add(new DataGridTemplateColumn
        {
            Header = "Keybinding", SortMemberPath = nameof(ShortcutRow.KeybindingText),
            Width = new DataGridLength(2.8, DataGridLengthUnitType.Star), MinWidth = 180,
            CellTemplate = new DataTemplate { VisualTree = keycaps }
        });
        Column("When", nameof(ShortcutRow.When), 3);
        Column("Source", nameof(ShortcutRow.Source), 1.7);
        Place(table, 2);
        var detailPanel = new DockPanel { Margin = new Thickness(0, 8, 0, 6), MaxHeight = 110 };
        var help = new Button { Content = "Q-SYS Help ↗", Margin = new Thickness(12, 0, 0, 0), Padding = new Thickness(8, 4, 8, 4), VerticalAlignment = VerticalAlignment.Top };
        DockPanel.SetDock(help, Dock.Right); detailPanel.Children.Add(help);
        var detail = new TextBlock { TextWrapping = TextWrapping.Wrap };
        detailPanel.Children.Add(new ScrollViewer { Content = detail, VerticalScrollBarVisibility = ScrollBarVisibility.Auto }); Place(detailPanel, 3);
        var controls = new DockPanel();
        var buttons = new StackPanel { Orientation = Orientation.Horizontal }; DockPanel.SetDock(buttons, Dock.Right); controls.Children.Add(buttons);
        controls.Children.Add(new TextBlock { Text = "Double-click a command or press Enter to change its shortcut.", VerticalAlignment = VerticalAlignment.Center }); Place(controls, 4);
        var status = new TextBlock { Margin = new Thickness(0, 6, 0, 0), TextWrapping = TextWrapping.Wrap, Text = "OK saves; Cancel discards changes." }; Place(status, 5);
        var overlay = new Grid { Name = "ShortcutEditOverlay", Background = new SolidColorBrush(Color.FromArgb(100, 0, 0, 0)), Visibility = Visibility.Collapsed };
        Grid.SetRowSpan(overlay, panel.RowDefinitions.Count); Panel.SetZIndex(overlay, 10); panel.Children.Add(overlay);
        KeyboardNavigation.SetTabNavigation(overlay, KeyboardNavigationMode.Cycle);
        var card = new Border { Padding = new Thickness(20), Margin = new Thickness(20), MaxWidth = 620,
            HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center, BorderThickness = new Thickness(1) };
        card.SetResourceReference(Border.BackgroundProperty, background);
        card.SetResourceReference(Border.BorderBrushProperty, foreground);
        var editing = new StackPanel(); card.Child = editing; overlay.Children.Add(card);
        var title = new TextBlock { FontSize = 16, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap };
        var gesture = new ShortcutRecorder();
        var warning = new TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 10) };
        AutomationProperties.SetLiveSetting(warning, AutomationLiveSetting.Polite);
        editing.Children.Add(title); editing.Children.Add(gesture);
        editing.Children.Add(new ScrollViewer { Content = warning, MaxHeight = 180, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });
        var editButtons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var accept = new Button { Name = "ShortcutAccept", Content = "Accept", Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(0, 0, 8, 0) };
        var cancel = new Button { Content = "Cancel", Padding = new Thickness(12, 5, 12, 5) };
        editButtons.Children.Add(accept); editButtons.Children.Add(cancel); editing.Children.Add(editButtons);
        ShortcutRow? editingRow = null;
        bool reviewingDefaults = false;
        var changeReview = new ShortcutChangeReview<BindingChange>();
        void CloseEdit()
        {
            editingRow = null; reviewingDefaults = false; changeReview.Invalidate(); gesture.Finish();
            overlay.Visibility = Visibility.Collapsed;
            table.IsEnabled = search.IsEnabled = controls.IsEnabled = true;
            table.Focus();
        }
        void ShowChange(BindingChange? change)
        {
            changeReview.Show(change); accept.IsEnabled = change is not null;
            var conflicts = change?.Conflicts ?? [];
            accept.Content = conflicts.Length > 0 ? "Overwrite" : "Accept";
            warning.Text = conflicts.Length == 0 ? "Enter accepts. Escape cancels. Preferences OK saves your changes."
                : "Already used by:\n" + string.Join("\n", conflicts.Select(c => "• " + c))
                    + "\n\nOverwrite moves the matching shortcuts to this command. Other shortcuts stay assigned. Enter overwrites; Escape cancels.";
        }
        void CommitEdit()
        {
            if (changeReview.Pending is null || (editingRow is null && !reviewingDefaults)) return;
            var selectedId = editingRow?.Id;
            try
            {
                if (!changeReview.TryApply(
                    change => bindings.Validate(new(change.Overrides, change.EditorOverrides)),
                    change =>
                    {
                        draft = change.Overrides;
                        draftEditors = change.EditorOverrides;
                        shared.StageBindings(new(draft, draftEditors));
                    })) return;
                CloseEdit(); Refresh(selectedId);
                status.Text = "Staged. Choose OK to save, or Cancel to discard.";
            }
            catch (Exception ex) { ShowChange(null); warning.Text = status.Text = ex.Message; }
        }
        void Edit(ShortcutRow selected, bool reset = false)
        {
            if (!selected.CanAssign || selected.CommandId is not { } id) return;
            editingRow = selected;
            title.Text = (reset ? "Restore default shortcuts: " : "Change shortcut: ") + selected.Command;
            gesture.Visibility = reset ? Visibility.Collapsed : Visibility.Visible;
            ShowChange(reset ? bindings.Prepare(id, null, draft, draftEditors) : null);
            if (reset && changeReview.Pending!.Conflicts.Length == 0) { CommitEdit(); return; }
            table.IsEnabled = search.IsEnabled = controls.IsEnabled = false;
            overlay.Visibility = Visibility.Visible;
            if (reset) accept.Focus();
            else
            {
                gesture.AllowOrdinaryKey = Commands.All.Single(c => c.Definition.Id == id).Definition.AllowOrdinaryKey;
                gesture.SetGesture(bindings.Input(id, draft)); gesture.Begin();
            }
        }
        var actions = new List<Button>();
        void Select()
        {
            var selected = table.SelectedItem as ShortcutRow;
            foreach (var action in actions) action.IsEnabled = selected?.CanAssign == true;
            detail.Text = selected?.Details ?? "Select a row for details. Reference rows describe mouse gestures or editor actions that cannot be reassigned here.";
            help.Visibility = selected?.SourceUrl is null ? Visibility.Collapsed : Visibility.Visible;
        }
        void Refresh(string? selectedId = null)
        {
            selectedId ??= (table.SelectedItem as ShortcutRow)?.Id;
            var sourceOverrides = new Dictionary<string, string>(draft);
            var snapshots = bindings.Snapshot(shared.Value with { Bindings = new(draft, draftEditors) });
            foreach (var entry in snapshots.Where(entry => draftEditors.ContainsKey(entry.Id)))
                sourceOverrides.TryAdd(entry.Id, entry.Keybinding);
            var rows = ShortcutTable.Build(snapshots, sourceOverrides);
            var filtered = rows.Where(row => row.Matches(search.Text)).ToArray();
            table.ItemsSource = filtered;
            table.SelectedItem = filtered.FirstOrDefault(row => row.Id == selectedId);
            count.Text = filtered.Length == 0 ? "No matching shortcuts" : $"{filtered.Length} of {rows.Length}";
            Select();
        }
        Button Button(string label, Action<Dictionary<string, string>, string> change)
        {
            var button = new Button { Content = label, Margin = new Thickness(0, 0, 8, 0), Padding = new Thickness(10, 5, 10, 5) };
            button.Click += (_, _) =>
            {
                if (table.SelectedItem is not ShortcutRow { CommandId: { } id } selected) return;
                try
                {
                    var candidate = new Dictionary<string, string>(draft);
                    change(candidate, id);
                    var candidateEditors = new Dictionary<string, string>(draftEditors); candidateEditors.Remove(id);
                    bindings.Validate(new(candidate, candidateEditors)); draft = candidate; draftEditors = candidateEditors;
                    shared.StageBindings(new(draft, draftEditors));
                    Refresh(selected.Id); status.Text = "Staged. Choose OK to save, or Cancel to discard.";
                }
                catch (Exception ex) { status.Text = ex.Message; }
            };
            buttons.Children.Add(button); actions.Add(button);
            return button;
        }
        Button("Clear", (next, id) => next[id] = "");
        var defaults = new Button { Content = "Review mod defaults", Margin = new Thickness(0, 0, 8, 0), Padding = new Thickness(10, 5, 10, 5) };
        defaults.Click += (_, _) =>
        {
            try
            {
                var commands = Commands.All.Where(c => Commands.IsAvailable(c) &&
                    (c.Definition.DefaultBinding is not null || Commands.DefaultFor(c.Definition.Id) is not null
                        || (c.Definition.Native is null && c.Definition.DefaultGesture is not null))).ToArray();
                var requests = commands.ToDictionary(c => c.Definition.Id, c => (string?)ModControls.Bindings.DefaultDisplay(c.Definition.Id));
                var change = ModControls.Bindings.Prepare(requests, draft, draftEditors);
                reviewingDefaults = true; editingRow = null;
                title.Text = "Apply all mod shortcut defaults"; gesture.Visibility = Visibility.Collapsed;
                ShowChange(change);
                warning.Text = "This replaces the assignments below and moves any conflicting keys. Other custom bindings stay assigned.\n\n"
                    + string.Join("\n", commands.Select(c => c.Definition.Name + ": " + requests[c.Definition.Id]))
                    + (change.Conflicts.Length == 0 ? "" : "\n\nConflicts to overwrite:\n" + string.Join("\n", change.Conflicts));
                accept.Content = "Apply all";
                table.IsEnabled = search.IsEnabled = controls.IsEnabled = false;
                overlay.Visibility = Visibility.Visible; accept.Focus();
            }
            catch (Exception ex) { status.Text = ex.Message; }
        };
        buttons.Children.Add(defaults);
        foreach (var label in new[] { "Change shortcut", "Reset" })
        {
            var button = new Button { Content = label, Margin = new Thickness(0, 0, 8, 0), Padding = new Thickness(10, 5, 10, 5) };
            button.Click += (_, _) => { if (table.SelectedItem is ShortcutRow selected) Edit(selected, label == "Reset"); };
            buttons.Children.Add(button); actions.Add(button);
        }
        table.MouseDoubleClick += (_, e) =>
        {
            if (e.ChangedButton != MouseButton.Left || e.OriginalSource is not DependencyObject source
                || ItemsControl.ContainerFromElement(table, source) is not DataGridRow { Item: ShortcutRow selected }) return;
            e.Handled = true; Edit(selected);
        };
        table.PreviewKeyDown += (_, e) =>
        {
            if (e.Key != Key.Enter || Keyboard.Modifiers != ModifierKeys.None || e.IsRepeat) return;
            e.Handled = true; if (table.SelectedItem is ShortcutRow selected) Edit(selected);
        };
        gesture.Changed += () =>
        {
            if (overlay.Visibility != Visibility.Visible || editingRow?.CommandId is not { } id || gesture.Visibility != Visibility.Visible) return;
            try { ShowChange(gesture.HasCandidate ? bindings.Prepare(id, gesture.Gesture, draft, draftEditors) : null); }
            catch (Exception ex) { ShowChange(null); warning.Text = ex.Message; }
        };
        gesture.Confirmed += CommitEdit;
        gesture.Canceled += CloseEdit;
        accept.Click += (_, _) => CommitEdit();
        cancel.Click += (_, _) => CloseEdit();
        overlay.PreviewKeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape && Keyboard.Modifiers == ModifierKeys.None) { e.Handled = true; CloseEdit(); }
        };
        overlay.MouseDown += (_, e) => { if (ReferenceEquals(e.OriginalSource, overlay)) CloseEdit(); };
        overlay.LostKeyboardFocus += (_, _) => overlay.Dispatcher.BeginInvoke(new Action(() =>
        { if (overlay.Visibility == Visibility.Visible && !overlay.IsKeyboardFocusWithin) CloseEdit(); }));
        search.TextChanged += (_, _) => Refresh();
        table.SelectionChanged += (_, _) => Select();
        help.Click += (_, _) =>
        {
            if (table.SelectedItem is not ShortcutRow { SourceUrl: { } url }) return;
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
            catch (Exception ex) { status.Text = ex.Message; }
        };
        panel.Loaded += (_, _) =>
        {
            // The native Preferences dialog starts at 700 x 500. Grow for this
            // table without changing the shared Core preference-page contract.
            if (Window.GetWindow(panel) is { } window)
            {
                window.Width = Math.Max(window.ActualWidth, Math.Min(1180, SystemParameters.WorkArea.Width - 40));
                window.Height = Math.Max(window.ActualHeight, Math.Min(760, SystemParameters.WorkArea.Height - 40));
            }
        };
        void SharedChanged()
        {
            changeReview.Invalidate();
            if (overlay.Visibility == Visibility.Visible)
            {
                CloseEdit();
                status.Text = "Shortcut assignments or commands changed. Reopen the shortcut editor to review the current conflicts.";
            }
            draft = new(shared.Value.Bindings.Overrides); draftEditors = new(shared.Value.Bindings.EditorOverrides ?? new());
            Refresh();
        }
        if (ModControls.HasReviewControls)
        {
            var review = new Button { Content = "Review mod controls…", Padding = new Thickness(10, 5, 10, 5) };
            review.Click += (_, _) => Preferences.SelectPage(panel, "mod-controls"); buttons.Children.Add(review);
        }
        panel.Loaded += (_, _) => { shared.Changed -= SharedChanged; shared.Changed += SharedChanged; ModControls.Changed -= SharedChanged; ModControls.Changed += SharedChanged; SharedChanged(); };
        panel.Unloaded += (_, _) => { shared.Changed -= SharedChanged; ModControls.Changed -= SharedChanged; };
        Refresh();
        return panel;
    }
    private sealed class ViewportHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is double height && double.IsFinite(height) && height > 0 ? Math.Max(320, height - 32) : 420d;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
