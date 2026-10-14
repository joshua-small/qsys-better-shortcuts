using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using QsysMods.Core;

namespace Designer.Modules.BetterShortcuts;

internal static class EditorActions
{
    internal static void Register(Window window)
    {
        var document = Host.Field(window, "_DocHost");
        var panels = new[] { Host.Field(document, "_toolWindow_Inventory"), Host.Field(document, "_toolWindow_Properties"), Host.Field(document, "_toolWindow_Library") };
        var appInfo = Host.Type("QSC.QSys.App").GetField("AppInfo", Host.Static)!.GetValue(null)!;
        var programmer = appInfo.GetType().GetProperty("ScriptProgrammerMode")!;
        var programmerMenu = (MenuItem)Host.Field(window, "mnuScriptProgrammerMode");
        void Add(string id, string label, string keys, Action run, Func<bool> enabled) => Commands.Register(new("better-shortcuts/" + id, label, "Better Shortcuts", run, enabled,
            AllowInText: true) { DefaultBinding = keys });
        var ids = new[] { "design-elements", "properties", "schematic-elements" };
        var names = new[] { "Design Elements", "Properties", "Schematic Elements" };
        var keys = new[] { "Alt+Shift+D", "Alt+Shift+P", "Alt+Shift+E" };
        for (int i = 0; i < panels.Length; i++)
        {
            var panel = panels[i];
            Add(ids[i], "Toggle " + names[i], keys[i], () => Toggle(panel), () => Host.Property(panel, "IsOpen") is true);
        }
        Add("all-sidebars", "Toggle All Docks", "Alt+Shift+Oem5", () => ToggleAll(panels), () => SidebarPanels(panels).Any());
        Add("left-dock", "Toggle Left Dock", "Alt+Shift+Oem4", () => ToggleAll(SpatialPanels(window, panels, true)), () => SpatialPanels(window, panels, true).Any());
        Add("right-dock", "Toggle Right Dock", "Alt+Shift+Oem6", () => ToggleAll(SpatialPanels(window, panels, false)), () => SpatialPanels(window, panels, false).Any());
        var theme = Commands.Native("ColorSchemeToggle");
        Add("dark-mode", "Toggle Light/Dark Mode", "Ctrl+Alt+D", () => theme.Execute(null, window), () => theme.CanExecute(null, window));
        var info = Commands.Native("NamedComponentInfo");
        Commands.RegisterNativeAlias("better-shortcuts/component-controls", info);
        Commands.RegisterDefaultBinding(info, "Ctrl+I", "Better Shortcuts", "Designer; native Component Controls action available", () => !ControlInput.IsTextInput());
        Add("programmer-mode", "Script Programmer Mode", "Ctrl+Alt+P", () => programmer.SetValue(appInfo, !(bool)programmer.GetValue(appInfo)!), () => programmerMenu.IsEnabled);
        ShortcutMenus.Install(window, programmerMenu, info);
        var design = Host.Field(document, "_DesignElements");
        var expanded = (DependencyProperty)Host.Type("QSC.QSys.windows.AccordionPanel").GetField("IsExpandedProperty",Host.Static)!.GetValue(null)!;
        int focusKey = 1;
        foreach (var (id, label, field) in new[] {
            ("inventory", "Inventory", "_InventoryViewer"), ("schematic-pages", "Schematic Pages", "_PageViewer"),
            ("uci", "User Control Interfaces", "_UciViewer"), ("snapshots", "Snapshots", "_SnapshotViewer"),
            ("named-controls", "Named Controls", "_ExternalControlViewer"), ("inspector", "Inspector", "_Inspector") })
        {
            var viewer = (FrameworkElement)Host.Field(design,field);
            Commands.Register(new("better-shortcuts/focus-"+id, "Focus "+label, "Better Shortcuts", () =>
            {
                if (Host.Property(panels[0],"State").ToString()=="AutoHide") Host.Call(Target(panels[0]),"Dock");
                viewer.SetValue(expanded,true); viewer.UpdateLayout();
                if (!viewer.MoveFocus(new TraversalRequest(FocusNavigationDirection.First))) { viewer.Focusable=true; viewer.Focus(); }
            }, () => Host.Property(panels[0],"IsOpen") is true, AllowInText:true) { DefaultBinding = "Alt+" + focusKey++ });
        }
        Commands.Register(new("better-shortcuts/focus-schematic-search", "Focus Schematic Elements Search", "Better Shortcuts", () =>
        {
            if (Host.Property(panels[2],"State").ToString()=="AutoHide") Host.Call(Target(panels[2]),"Dock");
            var root=(FrameworkElement)panels[2]; root.UpdateLayout();
            var search=Visual(root).OfType<TextBox>().Where(t=>t.IsVisible && t.IsEnabled && !t.IsReadOnly).SingleOrDefault()
                ?? throw new InvalidOperationException("The current Schematic Elements layout has no unique search field.");
            search.Focus(); search.SelectAll();
        }, () => Host.Property(panels[2],"IsOpen") is true, AllowInText:true) { DefaultBinding = "Alt+S" });
    }
    internal static IEnumerable<DependencyObject> Visual(DependencyObject root)
    { yield return root; for(int i=0;i<VisualTreeHelper.GetChildrenCount(root);i++) foreach(var child in Visual(VisualTreeHelper.GetChild(root,i))) yield return child; }
    internal static object Target(object panel) => panel is FrameworkElement { Parent: { } parent } && parent.GetType().FullName == "ActiproSoftware.Windows.Controls.Docking.ToolWindowContainer" ? parent : panel;
    private static void Toggle(object panel) => Host.Call(Target(panel), Host.Property(panel, "State").ToString() == "AutoHide" ? "Dock" : "AutoHide");
    private static IEnumerable<object> SidebarPanels(IEnumerable<object> panels) => panels.Where(p => Host.Property(p, "IsOpen") is true && Host.Property(p, "State").ToString() is "Docked" or "AutoHide");
    internal static void ToggleAll(IEnumerable<object> panels)
    {
        var sidebars = SidebarPanels(panels).ToArray();
        bool expand = sidebars.All(p => Host.Property(p, "State").ToString() == "AutoHide");
        var targets = sidebars.Where(p => (Host.Property(p, "State").ToString() == "AutoHide") == expand).Select(Target).Distinct(ReferenceEqualityComparer.Instance).ToArray();
        foreach (var target in targets) Host.Call(target, expand ? "Dock" : "AutoHide");
    }
    private static readonly Dictionary<object, bool> lastSide = new(ReferenceEqualityComparer.Instance);
    private static object[] SpatialPanels(Window window, IEnumerable<object> panels, bool left) => SidebarPanels(panels).Where(panel =>
    {
        if (Host.Property(panel, "State").ToString() == "Docked" && panel is FrameworkElement { IsVisible: true, ActualWidth: > 0 } element)
            lastSide[panel] = element.TranslatePoint(new Point(element.ActualWidth / 2, 0), window).X < window.ActualWidth / 2;
        return lastSide.TryGetValue(panel, out var side) ? side == left : (panel == panels.First()) == left;
    }).ToArray();
}
