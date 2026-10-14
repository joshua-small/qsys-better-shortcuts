using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QsysMods.Core;

namespace Designer.Modules.BetterShortcuts;

internal static class ShortcutMenus
{
    internal static void Install(Window window, MenuItem programmer, RoutedCommand info)
    {
        var menu = (Menu)Host.Field(window, "mainMenu");
        var view = menu.Items.OfType<MenuItem>().Single(m => (m.Header as string)?.Replace("_", "") == "View");
        Commands.RegisterMenuHint(programmer, "better-shortcuts/programmer-mode");

        MenuItem Action(string id, string label) => new()
        {
            Header = label,
            Command = Commands.All.Single(c => c.Definition.Id == "better-shortcuts/" + id).Command,
            CommandTarget = window
        };
        var sidebars = new MenuItem { Header = "Sidebars" };
        sidebars.Items.Add(Action("design-elements", "Toggle Design Elements"));
        sidebars.Items.Add(Action("properties", "Toggle Properties"));
        sidebars.Items.Add(Action("schematic-elements", "Toggle Schematic Elements"));
        sidebars.Items.Add(new Separator());
        sidebars.Items.Add(Action("left-dock", "Toggle Left Dock"));
        sidebars.Items.Add(Action("right-dock", "Toggle Right Dock"));
        sidebars.Items.Add(Action("all-sidebars", "Toggle All Docks"));
        var separator = new Separator();
        var darkMode = Action("dark-mode", "Toggle Dark Mode");
        view.Items.Add(separator); view.Items.Add(sidebars); view.Items.Add(darkMode);
        window.Closed += (_, _) =>
        {
            view.Items.Remove(separator); view.Items.Remove(sidebars); view.Items.Remove(darkMode);
        };
    }
    private static IEnumerable<MenuItem> Descendants(ItemsControl parent)
    {
        foreach (var item in parent.Items.OfType<MenuItem>())
        { yield return item; foreach (var child in Descendants(item)) yield return child; }
    }
}
