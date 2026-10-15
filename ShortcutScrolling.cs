using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Designer.Modules.BetterShortcuts;

internal static class ShortcutScrolling
{
    internal static void Attach(DataGrid table)
    {
        Window? owner = null;
        var wheel = new ShortcutWheelSteps();
        void Scroll(object sender, MouseWheelEventArgs e)
        {
            if (e.OriginalSource is not DependencyObject source || !Contains(table, source)) return;
            var viewer = Descendants(table).OfType<ScrollViewer>().FirstOrDefault();
            if (viewer is null) return;
            // Preferences' outer ScrollViewer consumes PreviewMouseWheel. Handle
            // only this table at the window, before that native ancestor runs.
            e.Handled = true;
            int lines = SystemParameters.WheelScrollLines;
            int steps = wheel.Take(e.Delta, lines);
            for (int i = 0; i < Math.Abs(steps); i++)
                if (lines < 0) { if (steps > 0) viewer.PageUp(); else viewer.PageDown(); }
                else { if (steps > 0) viewer.LineUp(); else viewer.LineDown(); }
        }
        MouseWheelEventHandler handler = Scroll;
        table.Loaded += (_, _) =>
        {
            var window = Window.GetWindow(table);
            if (ReferenceEquals(window, owner)) return;
            owner?.RemoveHandler(Mouse.PreviewMouseWheelEvent, handler);
            owner = window;
            owner?.AddHandler(Mouse.PreviewMouseWheelEvent, handler, true);
        };
        table.Unloaded += (_, _) => { owner?.RemoveHandler(Mouse.PreviewMouseWheelEvent, handler); owner = null; };
    }
    private static bool Contains(DependencyObject root, DependencyObject child)
    {
        for (DependencyObject? current = child; current is not null; current = current is Visual or Visual3D
            ? VisualTreeHelper.GetParent(current) : LogicalTreeHelper.GetParent(current))
            if (ReferenceEquals(root, current)) return true;
        return false;
    }
    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i); yield return child;
            foreach (var nested in Descendants(child)) yield return nested;
        }
    }
}
