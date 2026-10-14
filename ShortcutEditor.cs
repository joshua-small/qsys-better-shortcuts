using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using QsysMods.Core;

namespace Designer.Modules.BetterShortcuts;

internal static class ShortcutEditor
{
    internal static PreferenceEditor<ShortcutSettings> Create(ShortcutSettings value, ShortcutBindings bindings)
    {
        var draft = new Dictionary<string, string>(value.Overrides);
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
        var gesture = new TextBox { Name = "ShortcutGesture", Margin = new Thickness(0, 0, 10, 0), Padding = new Thickness(6), ToolTip = "Enter a shortcut, for example Ctrl+Alt+D" };
        AutomationProperties.SetName(gesture, "Shortcut to assign"); controls.Children.Add(gesture); Place(controls, 4);
        var status = new TextBlock { Margin = new Thickness(0, 6, 0, 0), TextWrapping = TextWrapping.Wrap, Text = "Assign, Clear, or Reset a selected command. OK saves; Cancel discards changes." }; Place(status, 5);
        var actions = new List<Button>();
        void Select()
        {
            var selected = table.SelectedItem as ShortcutRow;
            gesture.IsEnabled = selected?.CanAssign == true;
            foreach (var action in actions) action.IsEnabled = gesture.IsEnabled;
            gesture.Text = selected?.CommandId is { } id ? bindings.Input(id, draft) : "";
            detail.Text = selected?.Details ?? "Select a row for details. Reference rows describe mouse gestures or editor actions that cannot be reassigned here.";
            help.Visibility = selected?.SourceUrl is null ? Visibility.Collapsed : Visibility.Visible;
        }
        void Refresh(string? selectedId = null)
        {
            selectedId ??= (table.SelectedItem as ShortcutRow)?.Id;
            var rows = ShortcutTable.Build(bindings.Snapshot(draft), draft);
            var filtered = rows.Where(row => row.Matches(search.Text)).ToArray();
            table.ItemsSource = filtered;
            table.SelectedItem = filtered.FirstOrDefault(row => row.Id == selectedId);
            count.Text = filtered.Length == 0 ? "No matching shortcuts" : $"{filtered.Length} of {rows.Length}";
            Select();
        }
        void Button(string label, Action<Dictionary<string, string>, string> change)
        {
            var button = new Button { Content = label, Margin = new Thickness(0, 0, 8, 0), Padding = new Thickness(10, 5, 10, 5) };
            button.Click += (_, _) =>
            {
                if (table.SelectedItem is not ShortcutRow { CommandId: { } id } selected) return;
                try
                {
                    var candidate = new Dictionary<string, string>(draft);
                    change(candidate, id); bindings.Validate(new(candidate)); draft = candidate;
                    Refresh(selected.Id); status.Text = "Staged. Choose OK to save, or Cancel to discard.";
                }
                catch (Exception ex) { status.Text = ex.Message; }
            };
            buttons.Children.Add(button); actions.Add(button);
        }
        Button("Assign", (next, id) => next[id] = ShortcutBindings.Format(ShortcutBindings.Parse(gesture.Text)));
        Button("Clear", (next, id) => next[id] = "");
        Button("Reset", (next, id) => next.Remove(id));
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
        Refresh();
        return new(panel, () => new(draft));
    }
    private sealed class ViewportHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is double height && double.IsFinite(height) && height > 0 ? Math.Max(320, height - 32) : 420d;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}
