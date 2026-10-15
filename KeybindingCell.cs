using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Designer.Modules.BetterShortcuts;

internal sealed class KeybindingCell : StackPanel
{
    public KeybindingCell() { }
    public static readonly DependencyProperty RowProperty = DependencyProperty.Register(nameof(Row), typeof(ShortcutRow), typeof(KeybindingCell),
        new PropertyMetadata(null, (owner, _) => ((KeybindingCell)owner).Render()));
    public ShortcutRow? Row { get => (ShortcutRow?)GetValue(RowProperty); set => SetValue(RowProperty, value); }

    private void Render()
    {
        Children.Clear();
        if (Row is not { } row) return;
        AutomationProperties.SetName(this, row.KeybindingText);
        ToolTip = row.KeybindingText + (row.Keybinding.Contains("Oem", StringComparison.OrdinalIgnoreCase)
            ? "\nPunctuation uses US key labels; the printed label can differ with keyboard layout." : "");
        int index = 0;
        foreach (var line in row.KeybindingLines)
        {
            var tokens = new WrapPanel { VerticalAlignment = VerticalAlignment.Center };
            if (index++ > 0) tokens.Children.Add(Label("or "));
            foreach (var part in line.Parts)
            {
                if (!part.IsKey) { tokens.Children.Add(Label(part.Text)); continue; }
                var keycap = new Border
                {
                    CornerRadius = new CornerRadius(3), BorderThickness = new Thickness(1, 1, 1, 2),
                    Padding = new Thickness(5, 1, 5, 1), Margin = new Thickness(0, 1, 0, 1),
                    Child = new TextBlock { Text = part.Text, FontSize = 11, VerticalAlignment = VerticalAlignment.Center }
                };
                keycap.SetBinding(Border.BackgroundProperty, InheritedBrush("keycap"));
                keycap.SetBinding(Border.BorderBrushProperty, InheritedBrush("border"));
                tokens.Children.Add(keycap);
            }
            Children.Add(tokens);
        }
    }
    private static TextBlock Label(string text) => new() { Text = text, VerticalAlignment = VerticalAlignment.Center };
    private static Binding InheritedBrush(string kind) => new()
    {
        Path = new PropertyPath("(0)", System.Windows.Documents.TextElement.ForegroundProperty),
        RelativeSource = new RelativeSource(RelativeSourceMode.Self),
        Converter = new ShortcutBrushConverter(), ConverterParameter = kind
    };
    internal static Binding TableBrush(string kind) => new(nameof(Control.Foreground))
    {
        RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGrid), 1),
        Converter = new ShortcutBrushConverter(), ConverterParameter = kind
    };
}

// Derive the palette from Designer's live foreground resource. Explicit row and
// cell templates ensure the native implicit DataGrid style cannot cover stripes.
internal sealed class ShortcutBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool dark = value is SolidColorBrush brush && (brush.Color.R * 299 + brush.Color.G * 587 + brush.Color.B * 114) > 128000;
        string color = (dark, parameter as string) switch
        {
            (true, "alternate") => "#242424", (true, "keycap") => "#303030", (true, "border") => "#5A5A5A",
            (true, "hover") => "#323232", (true, _) => "#191919",
            (false, "alternate") => "#F2F2F2", (false, "keycap") => "#EEEEEE", (false, "border") => "#B8B8B8",
            (false, "hover") => "#E4E4E4", _ => "#FFFFFF"
        };
        var result = (SolidColorBrush)new BrushConverter().ConvertFromInvariantString(color)!;
        result.Freeze(); return result;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
