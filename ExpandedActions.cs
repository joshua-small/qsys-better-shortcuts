using System.Collections;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Data;
using QsysMods.Core;

namespace Designer.Modules.BetterShortcuts;

// Version-scoped adapters to Designer 10.5 actions. No saved design or DSP APIs.
internal static class ExpandedActions
{
    private static object document = null!;
    private static Window window = null!;
    private static object? Get(object? value, string name) => value?.GetType().GetProperty(name, Host.Instance)?.GetValue(value)
        ?? value?.GetType().GetField(name, Host.Instance)?.GetValue(value);
    private static object? Static(string type, string name) => Host.Type(type).GetProperty(name, Host.Static)?.GetValue(null)
        ?? Host.Type(type).GetField(name, Host.Static)?.GetValue(null);
    private static void Set(object value, string name, object? next) => value.GetType().GetProperty(name, Host.Instance)!.SetValue(value, next);
    private static object? Call(object value, string name, params object?[] arguments)
    {
        var method = value.GetType().GetMethods(Host.Instance).Single(m => m.Name == name && m.GetParameters().Length == arguments.Length
            && m.GetParameters().Select((p, i) => arguments[i] is null ? !p.ParameterType.IsValueType : p.ParameterType.IsInstanceOfType(arguments[i])).All(x => x));
        return method.Invoke(value, arguments);
    }
    private static IEnumerable<object> Items(object? items) => items is IEnumerable list ? list.Cast<object>() : [];
    private static FrameworkElement? Page => Get(document, "CurrentOpenCanvas") is { } key ? Call(document, "GetPage", key) as FrameworkElement : null;
    private static object? Uci => Page is { } page ? EditorActions.Visual(page).FirstOrDefault(p => p.GetType().FullName == "QSC.QSys.windows.UCI.WebPanelControl") : null;
    private static object? Layers => Get(Uci, "Layers");
    private static object? LayerTree => Get(Layers, "_TreeView");
    private static FrameworkElement? Canvas => Page is { } page ? EditorActions.Visual(page).OfType<FrameworkElement>()
        .FirstOrDefault(p => Host.Type("QSC.QSys.Graphic.GraphicCanvas").IsInstanceOfType(p) && p.IsVisible && Get(p, "Root") is not null) : null;
    private static bool DesignMode => Get(Static("QSC.QSys.ControlConnection.Manager", "Status"), "IsConnected") is false;
    private static bool EditableCanvas => DesignMode && Canvas is { } canvas && Get(canvas, "IsLocked") is false
        && Static("QSC.QSys.Tools.Set", "IsEditToolActive") is true;
    private static bool UciEditable => DesignMode && Uci is not null && Get(Layers, "IsPanelLocked") is false;
    private static object[] SelectedLayers => Items(Get(LayerTree, "SelectedItems")).Where(x => x.GetType().FullName == "QSC.QSys.Design.Layer").ToArray();
    // Inactive dock tabs are absent from the visual tree, but retain their content.
    private static object? Toolbox => Get(Get(Uci, "toolWindow_UCIToolBox"), "Content") is DependencyObject root
        ? Preferences.Logical(root).FirstOrDefault(p => p.GetType().FullName == "QSC.QSys.windows.UCI.UCIToolBox") : null;
    private static void Focus(object target)
    {
        var element = (FrameworkElement)target;
        for (DependencyObject? parent = element; parent is not null; parent = ControlInput.Parent(parent))
            if (parent.GetType().FullName == "ActiproSoftware.Windows.Controls.Docking.ToolWindow")
            { if (Get(parent, "State")?.ToString() == "AutoHide") Host.Call(EditorActions.Target(parent), "Dock"); Host.Call(parent, "Activate"); break; }
        element.UpdateLayout();
        if (!element.MoveFocus(new TraversalRequest(FocusNavigationDirection.First))) { element.Focusable = true; element.Focus(); }
    }
    private static void Add(string id, string name, string? keys, string when, Action execute, Func<bool> available, bool text = false)
        => Commands.Register(new("better-shortcuts/" + id, name, "Better Shortcuts", execute, available, AllowInText: text)
        { DefaultBinding = keys, ShortcutContext = available, When = when });
    private static void Native(string name, string keys) => Commands.RegisterDefaultBinding(Commands.Native(name), keys, "Better Shortcuts", "Designer; native action available", () => !ControlInput.IsTextInput());
    internal static void Register(Window main)
    {
        window = main; document = Host.Field(main, "_DocHost");
        Native("NewSchematicPage", "Ctrl+Shift+N > Ctrl+Shift+P");
        Native("DesignProperties", "Alt+Return");
        var design = Host.Field(document, "_DesignElements");
        var inventory = Host.Field(design, "_InventoryViewer");
        var uciViewer = (FrameworkElement)Host.Field(design, "_UciViewer");
        var newUci = (RoutedCommand)Static("QSC.QSys.windows.UCI.WebPagesViewer", "NewWebPanel")!;
        if (!Commands.All.Any(c => c.Definition.Id == Commands.NativeId(newUci)))
            Commands.Register(new(Commands.NativeId(newUci), "Add New User Control Interface", "Q-SYS", () => newUci.Execute(null, uciViewer),
                () => newUci.CanExecute(null, uciViewer), Native: newUci));
        Commands.RegisterDefaultBinding(newUci, "Ctrl+Shift+N > Ctrl+Shift+U", "Better Shortcuts", "Design mode; new UCI action available", () => DesignMode && !ControlInput.IsTextInput());
        Add("add-inventory", "Add New Inventory Item", "Ctrl+Shift+N > Ctrl+Shift+I", "Design mode; Inventory selector available", () =>
        { Focus(inventory); ((ToggleButton)Host.Field(inventory, "inventoryButton")).IsChecked = true; },
            () => DesignMode && Get(inventory, "inventoryButton") is ToggleButton { IsEnabled: true });
        foreach (var (id, name, field, key) in new[] { ("layers", "Layers", "Layers", "L"), ("toolbox", "Toolbox", "toolWindow_UCIToolBox", "T"), ("variables", "Variables", "VariablesTab", "V") })
            Add("uci-focus-" + id, "Focus Current UCI " + name, "Ctrl+Alt+U > " + key, "Current UCI editor", () => Focus(Get(Uci, field)!), () => Get(Uci, field) is FrameworkElement, true);
        Add("uci-dock", "Toggle Current UCI Left Dock", "Ctrl+Alt+U > D", "Current UCI editor; local dock available",
            () => Host.Call(Get(Uci, "toolboxes")!, Get(Get(Uci, "toolWindow_UCIToolBox"), "State")?.ToString() == "AutoHide" ? "Dock" : "AutoHide"), () => Get(Uci, "toolboxes") is not null, true);
        foreach (var expand in new[] { true, false })
            Add("uci-" + (expand ? "expand" : "collapse") + "-layers", (expand ? "Expand" : "Collapse") + " All UCI Layers", "Ctrl+Alt+U > " + (expand ? "E" : "C"), "Current UCI editor",
                () => { Focus(Layers!); Call(LayerTree!, expand ? "ExpandAll" : "CollapseAll"); }, () => LayerTree is not null, true);
        foreach (var (direction, key) in new[] { ("Up", "Up"), ("Down", "Down") })
            Add("uci-layer-" + direction.ToLowerInvariant(), "Move Selected UCI Layer " + direction, "Alt+" + key, "UCI Layers focused; one movable layer selected; Design mode",
                () => Call(Layers!, direction + "_Executed", Layers, new RoutedEventArgs()),
                () => UciEditable && LayerTree is FrameworkElement { IsKeyboardFocusWithin: true } && SelectedLayers.Length == 1 && Call(Layers!, "Can" + direction + "Execute") is true);
        foreach (var (id, property, name, key) in new[] { ("visibility", "IsHidden", "Show/Hide Selected UCI Layers", "H"), ("lock", "IsLocked", "Lock/Unlock Selected UCI Layers", "K") })
            Add("uci-layer-" + id, name, "Ctrl+Alt+U > " + key, "Current UCI; layer selection; Design mode; panel unlocked", () =>
            { var layers = SelectedLayers; bool next = !layers.All(l => Get(l, property) is true); foreach (var layer in layers) Set(layer, property, next); }, () => UciEditable && SelectedLayers.Length > 0);
        Add("uci-layout", "Focus Selected UCI Layout", "Ctrl+Alt+U > D1", "Current UCI editor", () =>
        { for (DependencyObject? p = Get(Uci, "_WebPage") as DependencyObject; p is not null; p = ControlInput.Parent(p))
            if (p.GetType().FullName == "ActiproSoftware.Windows.Controls.Docking.DocumentWindow") { Host.Call(p, "Activate"); break; } }, () => Uci is not null, true);
        Add("uci-script", "Focus Selected UCI Script", "Ctrl+Alt+U > D2", "Current UCI editor; script document available",
            () => Host.Call(Get(Uci, "scriptEditorWindow")!, "Activate"), () => Get(Uci, "scriptEditorWindow") is not null, true);
        var addVariable = (RoutedCommand)Static("QSC.QSys.windows.UCI.WebPanelVariables", "Add")!;
        Add("uci-add-variable", "Add UCI Variable", "Ctrl+Shift+N > Ctrl+Shift+V", "Current UCI; Design mode; Variables action available", () =>
        { var variables = (FrameworkElement)Get(Uci, "Variables")!; Focus(variables); addVariable.Execute(null, variables); },
            () => UciEditable && Get(Uci, "Variables") is FrameworkElement variables && addVariable.CanExecute(null, variables));
        foreach (var (name, category, key) in new[] { ("Button", "Buttons", "B"), ("Knob", "Knobs", "K"), ("Text", "Text", "T"), ("Fader", "Faders", "F"), ("Meter", "Meters", "M"), ("Indicator", "Indicators", "I") })
            Add("uci-add-" + name.ToLowerInvariant(), "Add UCI Toolbox " + name, "Ctrl+Alt+B > " + key, "Editable UCI canvas; choose a native control subtype", () => ChooseControl(category), () => EditableCanvas && Uci is not null && Toolbox is not null);
        Add("uci-add-flexbox", "Add UCI Toolbox Flexbox", "Alt+Shift+F", "Editable UCI canvas; inserts at visible canvas center", () => CreateControl(Toolbox!, FindTool("Graphic Tools", "Flexbox")!), () => EditableCanvas && Uci is not null && Toolbox is not null);
        foreach (var (id, name, handler, keys) in new[] { ("text", "Text Block", "TextBlock_Click", "Alt+Shift+T"), ("header", "Header", "Header_Click", "Alt+Shift+H"), ("group", "Group Box", "GroupBox_Click", "Alt+Shift+G"), ("polygon", "Polygon", "Region_Click", "Ctrl+Alt+G > P") })
            Add("add-" + id, "Add " + name + " to Design", keys, "Editable schematic or UCI canvas; activates native drawing tool", () =>
            { Canvas!.Focus(); Call(GraphicTools!, handler, null, new RoutedEventArgs()); }, () => EditableCanvas && GraphicTools is not null);
        foreach (var (name, key) in new[] { ("Audio", "A"), ("Control", "O"), ("Layout", "L"), ("Monitor", "M"), ("Scripting", "S"), ("UCI Buttons", "U"), ("Video", "V") })
            Add("focus-category-" + name.ToLowerInvariant().Replace(' ', '-'), "Focus Schematic Elements " + name, "Alt+C > Alt+" + key,
                "Schematic Elements library available", () => FocusCategory(name), () => LibraryView is not null, true);
        var autoStyles = (MenuItem)Host.Field(main, "mnuAutoUpdateCss");
        Add("auto-update-styles", "Auto Update Styles", null, "Designer; Auto Update Styles menu available", () => Set(main, "AutoUpdateCss", !(bool)Get(main, "AutoUpdateCss")!), () => autoStyles.IsEnabled);
        Commands.RegisterMenuHint(autoStyles, "better-shortcuts/auto-update-styles");
        Add("edit-code-name", "Edit Code Name/Control Name", "F2", "Design mode; selected object has an editable Code Name or Control Name field", () =>
        {
            var editor = NameEditor!; Focus(Host.Field(document, "_toolWindow_Properties"));
            editor.BringIntoView(); editor.Focus(); editor.SelectAll();
        }, () => DesignMode && NameEditor is not null);
        Commands.Register(new("better-shortcuts/context-color", "Context Color Picker", "Better Shortcuts", () =>
        {
            var swatch = FocusedSwatch!;
            var popup = (Popup)Get(swatch, "_ColorPopup")!;
            popup.PlacementTarget = (UIElement)Get(swatch, "_ColorRect")!; popup.IsOpen = true;
        }, () => FocusedSwatch is { } swatch && Call(swatch, "IsSelectionLocked") is false)
        { DefaultBinding = "I", AllowOrdinaryKey = true, ShortcutContext = () => FocusedSwatch is not null && !ControlInput.IsTextInput(), When = "Color swatch focused; selection unlocked; text input excluded" });
    }
    private static TextBox? NameEditor
    {
        get
        {
            var root = (DependencyObject)Host.Field(document, "editorStackControl");
            return EditorActions.Visual(root).OfType<TextBox>().FirstOrDefault(t => t.IsEnabled && !t.IsReadOnly && ActiveEditor(t, root) &&
                (t.Name is "DebugControlName" or "_ControlName" or "tbCodeName" || BindingOperations.GetBindingExpression(t, TextBox.TextProperty)?.ParentBinding.Path?.Path is "CodeName" or "ControlName"));
        }
    }
    private static bool ActiveEditor(DependencyObject field, DependencyObject root)
    {
        // Native property editors stay materialized after their selection is
        // removed. Ignore collapsed editor ancestors, but allow the enclosing
        // Properties dock to be auto-hidden so F2 can restore it.
        for (DependencyObject? current = field; current is not null; current = ControlInput.Parent(current))
        {
            if (current is UIElement { Visibility: not Visibility.Visible }) return false;
            if (ReferenceEquals(current, root)) return true;
        }
        return false;
    }
    private static object? FocusedSwatch
    {
        get
        {
            for (var p = Keyboard.FocusedElement as DependencyObject; p is not null; p = ControlInput.Parent(p))
                if (p.GetType().FullName == "QSC.QSys.windows.Editors.ColorSwatch") return p;
            return null;
        }
    }
    private static DependencyObject LibraryRoot => (DependencyObject)Get(Host.Field(document, "_toolWindow_Library"), "Content")!;
    private static object? GraphicTools => Preferences.Logical(LibraryRoot).FirstOrDefault(p => p.GetType().FullName == "QSC.QSys.windows.GraphicTools");
    private static FrameworkElement? LibraryView => Preferences.Logical(LibraryRoot).OfType<FrameworkElement>().FirstOrDefault(p => p.GetType().FullName == "QSC.QSys.windows.SchematicElements.ComponentListStack");
    private static object? FindTool(string category, string? name = null) => Items(Get(Toolbox, "ToolBoxControls")).Where(x => Equals(Get(x, "Name"), category))
        .SelectMany(x => Items(Get(x, "Children"))).FirstOrDefault(x => name is null || Equals(Get(x, "Name"), name));
    private static void ChooseControl(string category)
    {
        var toolbox = Toolbox!; var page = Page;
        var menu = new ContextMenu { PlacementTarget = Canvas, Placement = PlacementMode.Center };
        foreach (var type in Items(Get(toolbox, "ToolBoxControls")).Where(x => Equals(Get(x, "Name"), category)).SelectMany(x => Items(Get(x, "Children"))))
        {
            var item = new MenuItem { Header = Get(type, "Name") };
            item.Click += (_, _) => { if (ReferenceEquals(Page, page) && EditableCanvas) CreateControl(toolbox, type); };
            menu.Items.Add(item);
        }
        menu.IsOpen = true; menu.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
    }
    private static void CreateControl(object toolbox, object type)
    {
        var canvas = Canvas!; var controlType = Get(type, "ControlType")!;
        var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(controlType.GetType()))!; list.Add(controlType);
        var creator = Activator.CreateInstance(Host.Type("QSC.QSys.windows.UCI.UCIToolBoxCreator"), Get(toolbox, "Controller"), list, toolbox)!;
        // SnappedCreator.Run owns the host's selection tracking and undo completion.
        FrameworkElement viewport = canvas;
        for (DependencyObject? p = canvas; p is not null; p = ControlInput.Parent(p)) if (p is ScrollContentPresenter presenter) { viewport = presenter; break; }
        var point = viewport.TranslatePoint(new Point(viewport.ActualWidth / 2, viewport.ActualHeight / 2), canvas);
        canvas.Focus(); Call(creator, "Initialize"); Call(creator, "Run", canvas, null, point);
    }
    private static void FocusCategory(string name)
    {
        var view = LibraryView!; Focus(view); var model = view.DataContext;
        Set(model, "SearchText", ""); Set(model, "SelectedParentTag", null);
        bool Find(IEnumerable<object> nodes)
        {
            foreach (var node in nodes)
            {
                string label = Get(node, "Label")?.ToString() ?? "";
                if (label.Equals(name, StringComparison.OrdinalIgnoreCase) || label.Equals(name + " Components", StringComparison.OrdinalIgnoreCase))
                { Set(node, "Expanded", true); Set(model, "SelectedTreeViewItem", node); return true; }
                if (Find(Items(Get(node, "Children")))) { Set(node, "Expanded", true); return true; }
            }
            return false;
        }
        if (!Find(Items(Get(model, "AllSchematicItems")))) throw new InvalidOperationException("The current library does not contain the " + name + " category.");
        view.UpdateLayout();
        var selected = Get(model, "SelectedTreeViewItem");
        var item = EditorActions.Visual(view).OfType<TreeViewItem>().FirstOrDefault(t => ReferenceEquals(t.DataContext, selected));
        if (item is not null) { item.IsSelected = true; item.BringIntoView(); item.Focus(); }
    }
}
