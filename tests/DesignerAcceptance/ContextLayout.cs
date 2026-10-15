using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QsysMods.Core;
using Keys = System.Windows.Input.Key;

namespace Designer.Modules.ShortcutsLab;

public sealed partial class Probe
{
    private static readonly Dictionary<string, int> contextRuns = new();
    private static bool contextAvailable = true;
    private static void RegisterContextCommands()
    {
        foreach (var (id, binding, text) in new[] { ("single", "F18", false), ("chord-g", "F17 > G", false), ("chord-h", "F17 > H", false),
            ("opt-single", "F19", true), ("opt-chord", "F20 > G", true) })
            Commands.Register(new("shortcuts-lab/" + id, "Context " + id, "Context acceptance", () => contextRuns[id] = Runs(id) + 1,
                () => contextAvailable, AllowInText: text) { DefaultBinding = binding, ShortcutContext = () => contextAvailable });
    }
    private static int Runs(string id) => contextRuns.GetValueOrDefault(id);
    private static object? Invoke(object target, string name, params object?[] args) => target.GetType().GetMethods(Host.Instance)
        .Single(m => m.Name == name && m.GetParameters().Length == args.Length && m.GetParameters().Select((p, i) => args[i] is null || p.ParameterType.IsInstanceOfType(args[i])).All(x => x)).Invoke(target, args);
    private static bool RoutedKey(UIElement target, Keys key, bool up = false)
    {
        var e = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(target)!, Environment.TickCount, key)
            { RoutedEvent = up ? Keyboard.PreviewKeyUpEvent : Keyboard.PreviewKeyDownEvent };
        target.RaiseEvent(e); return e.Handled;
    }
    private static bool Stroke(UIElement target, Keys key) { bool consumed = RoutedKey(target, key); RoutedKey(target, key, true); return consumed; }
    private static void Action(string id) { var a = Commands.All.Single(c => c.Definition.Id == "better-shortcuts/" + id).Definition; Check(a.CanExecute(), id + " is available"); a.Execute(); }
    private static async Task RoutingAcceptance(Window main, FrameworkElement canvas)
    {
        canvas.Focus(); await Task.Delay(100);
        Check(Stroke(canvas, Keys.F18) && Runs("single") == 1, "Canvas single dispatches once through the actual route");
        Check(Stroke(canvas, Keys.F17) && Runs("chord-g") == 0, "Prefix waits without dispatch");
        Check(Stroke(canvas, Keys.G) && Runs("chord-g") == 1, "First shared-prefix completion dispatches once");
        Stroke(canvas, Keys.F17); Stroke(canvas, Keys.H);
        Check(Runs("chord-h") == 1 && Runs("chord-g") == 1, "Distinct completion selects only its owner");
        RoutedKey(canvas, Keys.F18); RoutedKey(canvas, Keys.F18); RoutedKey(canvas, Keys.F18, true);
        Check(Runs("single") == 2, "Held single key cannot repeat execution");
        RoutedKey(canvas, Keys.F17); RoutedKey(canvas, Keys.F17); RoutedKey(canvas, Keys.F17, true); Stroke(canvas, Keys.G);
        Check(Runs("chord-g") == 2, "Repeated prefix does not complete or duplicate a chord");
        Stroke(canvas, Keys.F17); Stroke(canvas, Keys.Escape); Stroke(canvas, Keys.G);
        Check(Runs("chord-g") == 2, "Escape cancels without replay");
        Stroke(canvas, Keys.F17); Check(Stroke(canvas, Keys.X), "Invalid completion is consumed"); Stroke(canvas, Keys.G);
        Check(Runs("chord-g") == 2, "Invalid completion cancels without dispatch");
        Stroke(canvas, Keys.F17); contextAvailable = false; Stroke(canvas, Keys.G);
        Check(Runs("chord-g") == 2, "Availability is rechecked at completion");
        Check(!Stroke(canvas, Keys.F18) && !Stroke(canvas, Keys.F17), "Unavailable commands leave input unclaimed"); contextAvailable = true;
        var text = Visual(main).OfType<TextBox>().First(t => t.IsVisible && t.IsEnabled && !t.IsReadOnly);
        Stroke(canvas, Keys.F17); text.Focus(); await Task.Delay(100); Stroke(text, Keys.G);
        Check(Runs("chord-g") == 2 && ControlInput.IsTextInput(), "Focus into native text cancels a pending chord");
        string before = text.Text;
        Check(!Stroke(text, Keys.F18) && !Stroke(text, Keys.F17), "Native text excludes ordinary mod defaults");
        Stroke(text, Keys.F19); Stroke(text, Keys.F20); Stroke(text, Keys.G);
        Check(Runs("opt-single") == 1 && Runs("opt-chord") == 1 && text.Text == before, "Text opt-in executes both forms without mutating field text");
        canvas.Focus(); Stroke(canvas, Keys.F17);
        canvas.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) { RoutedEvent = Mouse.PreviewMouseDownEvent });
        Stroke(canvas, Keys.G); Check(Runs("chord-g") == 2, "Mouse input cancels a pending chord");
        Stroke(canvas, Keys.F17); await Task.Delay(10500); Stroke(canvas, Keys.G);
        Check(Runs("chord-g") == 2, "Actual dispatcher timeout cancels a pending chord");
        Stroke(canvas, Keys.F17);
        var prefs = (Window)Activator.CreateInstance(Host.Type("QSC.QSys.windows.Dialogs.PreferencesDialog"))!; prefs.Owner = main;
        Exception? failure = null;
        _ = main.Dispatcher.BeginInvoke(new System.Action(() => { try {
            Stroke(prefs, Keys.G); Stroke(prefs, Keys.F19); Stroke(prefs, Keys.F20); Stroke(prefs, Keys.G);
            Check(Runs("chord-g") == 2 && Runs("opt-single") == 1 && Runs("opt-chord") == 1, "Owned modal blocks pending and opted-in main commands");
        } catch(Exception e) { failure=e; } finally { prefs.Close(); } }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        prefs.ShowDialog(); if(failure is not null) throw failure;
        canvas.Focus(); Stroke(canvas, Keys.G); Check(Runs("chord-g") == 2, "Closing modal does not revive pending input");
    }
    private static async Task LayoutAcceptance(Window main, object document)
    {
        var inventory = Host.Field(document, "_toolWindow_Inventory"); var properties = Host.Field(document, "_toolWindow_Properties"); var library = Host.Field(document, "_toolWindow_Library");
        var site = Get(inventory, "DockSite")!;
        async Task Dock(object panel, string side) { Invoke(panel, "Dock", site, Enum.Parse(Host.Type("ActiproSoftware.Windows.Controls.Side"), side)); main.UpdateLayout(); await Task.Delay(200); }
        string State(object p) => Get(p,"State")!.ToString()!;
        bool Left(object p) => ((FrameworkElement)p).TranslatePoint(new Point(((FrameworkElement)p).ActualWidth / 2, 0), main).X < main.ActualWidth / 2;
        await Dock(inventory,"Right"); await Dock(properties,"Left"); await Dock(library,"Left");
        Check(!Left(inventory) && Left(properties) && Left(library), "Native dock swap puts Inventory right and both other panels left");
        Action("left-dock"); await Task.Delay(150);
        Check(State(inventory)=="Docked" && State(properties)=="AutoHide" && State(library)=="AutoHide", "Left shortcut follows swapped geometry");
        Action("left-dock"); await Task.Delay(150);
        Check(State(properties)=="Docked" && State(library)=="Docked", "Left shortcut restores both swapped panels");
        Action("right-dock"); await Task.Delay(150);
        Check(State(inventory)=="AutoHide" && State(properties)=="Docked", "Right shortcut follows swapped Inventory");
        Action("all-sidebars"); await Task.Delay(150);
        Check(State(inventory)=="AutoHide" && State(properties)=="AutoHide" && State(library)=="AutoHide", "Mixed layout collapses remaining expanded panels");
        Action("all-sidebars"); await Task.Delay(150);
        Check(State(inventory)=="Docked" && State(properties)=="Docked" && State(library)=="Docked", "All docks restores the swapped layout");
        await Dock(inventory,"Left"); await Dock(properties,"Right"); await Dock(library,"Right");
    }
    private static async Task SidebarFocusAcceptance(object document)
    {
        var design=Host.Field(document,"_DesignElements");
        var expanded=(DependencyProperty)Host.Type("QSC.QSys.windows.AccordionPanel").GetField("IsExpandedProperty",Host.Static)!.GetValue(null)!;
        var targets=new List<object>();
        foreach(var (id,field) in new[] {("inventory","_InventoryViewer"),("schematic-pages","_PageViewer"),("uci","_UciViewer"),
            ("snapshots","_SnapshotViewer"),("named-controls","_ExternalControlViewer"),("inspector","_Inspector")})
        {
            var viewer=(FrameworkElement)Host.Field(design,field); viewer.SetValue(expanded,false);
            Action("focus-"+id); await Task.Delay(100);
            Check(viewer.GetValue(expanded) is true && viewer.IsKeyboardFocusWithin, id+" expands and receives actual keyboard focus");
            Action("focus-"+id); Check(viewer.GetValue(expanded) is true,id+" repeated focus does not collapse its section");
            targets.Add(new {id,type=Keyboard.FocusedElement?.GetType().FullName,name=(Keyboard.FocusedElement as FrameworkElement)?.Name});
        }
        Action("focus-schematic-search");
        Check(Keyboard.FocusedElement is TextBox && ((FrameworkElement)Host.Field(document,"_toolWindow_Library")).IsKeyboardFocusWithin,"Schematic search focuses its native editable field");
        targets.Add(new {id="schematic-search",type=Keyboard.FocusedElement?.GetType().FullName,name=(Keyboard.FocusedElement as FrameworkElement)?.Name});
        File.WriteAllText(Path.Combine(Output,"focus-targets.json"),JsonSerializer.Serialize(targets,SettingsJson.Format));
    }
    private static async Task SchematicAcceptance(Window main, FrameworkElement canvas)
    {
        var manager = Host.Type("QSC.QSys.App").GetField("Manager", Host.Static)!.GetValue(null)!;
        var root = Get(canvas, "Root")!;
        var creator = Activator.CreateInstance(Host.Type("QSC.QSys.Drag.ComponentCreator`1").MakeGenericType(Host.Type("QSC.QSys.Design.Component")))!;
        creator.GetType().GetProperty("ClassName")!.SetValue(creator, "gain");
        var operation = Invoke(manager, "StartOperation");
        var model = Invoke(creator, "Create", new Point(200, 200))!;
        model.GetType().GetProperty("CodeName")!.SetValue(model, "ContextGain");
        Invoke(Get(root,"CurrentLayer")!, "AddChild", model); Host.Call(model, "Attach"); Invoke(manager, "EndOperation", operation);
        await Task.Delay(300);
        var graphic = Items(Get(canvas, "AllGraphicObjects")).Single(g => ReferenceEquals(Get(g,"obj"), model));
        var selection = HostSelection.GlobalSelection;
        Host.Call(selection, "Clear"); Invoke(selection, "Add", graphic, false); Host.Call(selection, "OnSelectionChanged"); canvas.Focus(); await Task.Delay(200);
        Check(Adapter("NameEditor") is TextBox { Name: "tbCodeName" }, "Schematic component exposes its native Code Name field");
        var properties=Host.Field(Host.Field(main,"_DocHost"),"_toolWindow_Properties");
        Host.Call(properties,"AutoHide"); await Task.Delay(100);
        Check(Commands.All.Single(c=>c.Definition.Id=="better-shortcuts/edit-code-name").Definition.CanExecute(),"F2 remains available while the eligible Properties panel is auto-hidden");
        Check(Stroke(canvas, Keys.F2), "Schematic F2 is claimed by the registered shortcut");
        Check(Keyboard.FocusedElement is TextBox { Name: "tbCodeName", SelectionLength: 11 }, "Schematic F2 selects the complete native Code Name");
        Check(Get(properties,"State")?.ToString()=="Docked","F2 restores the auto-hidden Properties panel");
        canvas.Focus();
        Host.Call(selection,"Clear"); Host.Call(selection,"OnSelectionChanged"); Host.Call(manager,"Undo"); await Task.Delay(150);
        Check(!Items(Get(canvas,"AllGraphicObjects")).Any(g=>ReferenceEquals(Get(g,"obj"), model)), "Schematic fixture creation is removed by one Undo");
        Check(!Commands.All.Single(c=>c.Definition.Id=="better-shortcuts/edit-code-name").Definition.CanExecute(), "F2 becomes unavailable after the component selection is removed");
    }
    private static async Task UciEditorAcceptance(Window main)
    {
        var create = Commands.All.Single(c => c.Definition.Name == "Add New User Control Interface").Definition;
        Check(create.CanExecute(), "Native UCI action is available for context fixture"); create.Execute();
        await Until(() => Adapter("Uci") is not null && Adapter("Canvas") is FrameworkElement);
        var canvas = (FrameworkElement)Adapter("Canvas")!; canvas.Focus();
        int single = Runs("single"), chord = Runs("chord-h"); Stroke(canvas,Keys.F18); Stroke(canvas,Keys.F17); Stroke(canvas,Keys.H);
        Check(Runs("single")==single+1 && Runs("chord-h")==chord+1, "UCI canvas routes a single and a shared-prefix chord once");
        Action("uci-add-button"); await Task.Delay(100);
        var chooser=PresentationSource.CurrentSources.Cast<PresentationSource>().Select(s=>s.RootVisual).OfType<DependencyObject>().SelectMany(Visual).OfType<ContextMenu>().Single(m=>m.IsOpen);
        chooser.Items.OfType<MenuItem>().First().RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent)); chooser.IsOpen=false; await Task.Delay(150);
        canvas.Focus();
        Check(Stroke(canvas,Keys.F2) && Keyboard.FocusedElement is TextBox {Name:"_ControlName",SelectionLength:>0},"UCI F2 still focuses and selects the current control name");
        canvas.Focus(); ApplicationCommands.Undo.Execute(null,canvas); await Task.Delay(100);
        Action("uci-script"); await Task.Delay(1200);
        var editors = Visual(main).OfType<FrameworkElement>().Where(v => v.IsVisible && (v.GetType().FullName?.Contains("WebView") == true || v.GetType().FullName?.Contains("Chromium") == true)).ToArray();
        File.WriteAllText(Path.Combine(Output,"editors.json"), JsonSerializer.Serialize(editors.Select(v => new {type=v.GetType().FullName,v.Name,v.Focusable}),SettingsJson.Format));
        var editor = editors.First(v => v.Focusable); editor.Focus(); await Task.Delay(100);
        Check(ControlInput.IsTextInput(), "Docked Monaco is recognized as text input");
        single=Runs("single"); int opt=Runs("opt-single"), optChord=Runs("opt-chord");
        Check(!Stroke(editor,Keys.F18) && !Stroke(editor,Keys.F17), "Docked Monaco leaves default single/chord prefixes unclaimed");
        Stroke(editor,Keys.F19); Stroke(editor,Keys.F20); Stroke(editor,Keys.G);
        Check(Runs("single")==single && Runs("opt-single")==opt+1 && Runs("opt-chord")==optChord+1, "Docked Monaco routes only explicit text opt-ins");
        var script = Get(Adapter("Uci"),"scriptEditorWindow")!;
        Host.Call(script,"Float"); await Task.Delay(300);
        var detached=Window.GetWindow(editor)!;
        Check(detached != main && detached.IsVisible, "Native script document detaches into its own host");
        opt=Runs("opt-single"); optChord=Runs("opt-chord"); Stroke(editor,Keys.F19); Stroke(editor,Keys.F20); Stroke(editor,Keys.G);
        Check(Runs("opt-single")==opt && Runs("opt-chord")==optChord, "Unsupported detached script scope leaves even opted-in keys alone");
        detached.Close(); main.Activate(); await Task.Delay(300); Action("uci-layout");
    }
    private static async Task NestedAcceptance(Window main, object document, FrameworkElement rootCanvas)
    {
        var manager=Host.Type("QSC.QSys.App").GetField("Manager",Host.Static)!.GetValue(null)!;
        var operation=Invoke(manager,"StartOperation");
        var creator=Activator.CreateInstance(Host.Type("QSC.QSys.windows.ContainerCreate"))!;
        var container=Invoke(creator,"Create",new Point(200,200))!;
        Host.Type("QSC.QSys.Design.ContainerHelper").GetMethod("UpdateContainer",Host.Static)!.Invoke(null,[container]);
        Host.Call(container,"Attach"); Invoke(Get(Get(rootCanvas,"Root"),"CurrentLayer")!,"AddChild",container);
        Invoke(container,"AllocateResources",Get(rootCanvas,"Root")); Invoke(manager,"EndOperation",operation);
        var nestedRoot=Items(Get(container,"Canvases"))[0];
        Invoke(document,"OpenSchematicPage",nestedRoot,null);
        await Until(()=>ReferenceEquals(Get(Adapter("Canvas"),"Root"),nestedRoot));
        var nested=(FrameworkElement)Adapter("Canvas")!; nested.Focus();
        int single=Runs("single"),chord=Runs("chord-g");
        Stroke(nested,Keys.F18); Stroke(nested,Keys.F17); Stroke(nested,Keys.G);
        Check(Runs("single")==single+1 && Runs("chord-g")==chord+1, "Nested schematic routes single keys and chords once");
        Stroke(nested,Keys.F17); Invoke(document,"ActivatePage",Get(rootCanvas,"Root"));
        rootCanvas.Focus(); Stroke(rootCanvas,Keys.G);
        Check(Runs("chord-g")==chord+1, "Changing schematic pages cancels a pending chord");
        Invoke(document,"ClosePage",nestedRoot); Host.Call(manager,"Undo");
        Check(!Items(Get(Get(rootCanvas,"Root"),"AllChildren")).Contains(container), "Nested schematic fixture is removed by one Undo");
    }
    private static async Task FloatingAcceptance(Window main)
    {
        main.Activate(); main.Focus(); Keyboard.Focus(main);
        Commands.Native("NamedComponentInfo").Execute(null,main); await Task.Delay(300);
        var component=Application.Current.Windows.Cast<Window>().SelectMany(Visual).OfType<FrameworkElement>().First(v=>v.GetType().FullName=="QSC.QSys.windows.ComponentInfo");
        var floating=Window.GetWindow(component)!;
        Check(floating!=main && floating.IsVisible,"Component Controls reaches its actual floating presentation source");
        int ordinary=Runs("single"),opt=Runs("opt-single"),chord=Runs("opt-chord");
        Stroke(component,Keys.F18); Stroke(component,Keys.F17); Stroke(component,Keys.G);
        Stroke(component,Keys.F19); Stroke(component,Keys.F20); Stroke(component,Keys.G);
        Check(Runs("single")==ordinary && Runs("opt-single")==opt+1 && Runs("opt-chord")==chord+1,"Floating Component Controls permits only its supported opted-in mod routes");
        Stroke(component,Keys.F20);
        Check(RoutedKey(component,Keys.Escape,true), "Floating Escape release cancels when key-down never reaches WPF");
        Check(!Stroke(component,Keys.G) && Runs("opt-chord")==chord+1, "Missing Escape-down regression cannot dispatch its later completion");
        Stroke(component,Keys.F20); Stroke(component,Keys.G);
        Check(Runs("opt-chord")==chord+2, "A new floating chord executes once after release cancellation");
        floating.Close(); main.Activate();
        Commands.Native("NamedComponentInfo").Execute(null,main); await Task.Delay(300);
        component=Application.Current.Windows.Cast<Window>().SelectMany(Visual).OfType<FrameworkElement>().First(v=>v.GetType().FullName=="QSC.QSys.windows.ComponentInfo");
        floating=Window.GetWindow(component)!; Stroke(component,Keys.F19);
        Check(Runs("opt-single")==opt+2,"Closing and reopening Component Controls does not duplicate dispatch"); floating.Close();
    }
    private static async Task HintAcceptance(FrameworkElement canvas)
    {
        // A fixture menu with a real native command exercises Designer's event
        // route; it does not claim coverage of every native icon-menu template.
        var original = ModControls.Current.Bindings;
        var priorMenu = canvas.ContextMenu;
        var menu = new ContextMenu { PlacementTarget = canvas };
        var item = new MenuItem { Header="Copy", Command=ApplicationCommands.Copy, InputGestureText="Ctrl+C" };
        menu.Items.Add(item); canvas.ContextMenu=menu;
        string id=Commands.NativeId(ApplicationCommands.Copy);
        async Task Set(string? key) => await Apply(ModControls.Bindings.Prepare(id,key,ModControls.Current.Bindings.Overrides,ModControls.Current.Bindings.EditorOverrides));
        void Open()
        {
            var args=(ContextMenuEventArgs)Activator.CreateInstance(typeof(ContextMenuEventArgs),Host.Instance,null,[canvas,true],null)!;
            args.RoutedEvent=ContextMenuService.ContextMenuOpeningEvent; canvas.RaiseEvent(args); menu.IsOpen=true;
        }
        try
        {
            await Set("F22 > G"); Open(); await Task.Delay(100);
            Check(item.InputGestureText=="F22 then G", "Actual context-opening route displays the native command's chord");
            await Set("F22 > H");
            Check(menu.IsOpen && item.InputGestureText=="F22 then H", "An open Designer context menu refreshes after live JSON remapping");
            await Set(""); Check(item.InputGestureText=="", "Live Clear removes the open context-menu hint");
            await Set(null); Check(item.InputGestureText=="Ctrl+C", "Live Reset restores the native context-menu hint");
            Check(ReferenceEquals(item.Command,ApplicationCommands.Copy), "Hint refresh preserves the native command identity");
            menu.IsOpen=false;
            Check(item.InputGestureText=="Ctrl+C", "Closing the context menu restores its original presentation");
        }
        finally
        {
            menu.IsOpen=false; canvas.ContextMenu=priorMenu;
            var doc=Read(); doc["mods"]![ModControls.SettingsId]!["Bindings"]=JsonSerializer.SerializeToNode(original);
            File.WriteAllText(SharedSettings.PathName,SettingsJson.Text(doc));
            await Until(()=>SettingsJson.Same(JsonSerializer.SerializeToNode(ModControls.Current.Bindings),JsonSerializer.SerializeToNode(original)));
        }
    }
    private static async Task ContextLayoutAcceptance(Window main)
    {
        try
        {
            Check(main.Title.StartsWith("untitled", StringComparison.OrdinalIgnoreCase), "Context suite requires a synthetic untitled design");
            Check(SharedSettings.Error is null, "Context suite starts with healthy shared settings");
            main.Activate(); main.Focus(); Keyboard.Focus(main); await Task.Delay(200);
            Check(Keyboard.Modifiers == ModifierKeys.None, "Neutral keyboard state for routed-event tests");
            var document = Host.Field(main, "_DocHost");
            var canvas = Adapter("Canvas");
            await RoutingAcceptance(main, (FrameworkElement)canvas!);
            await LayoutAcceptance(main, document);
            await SidebarFocusAcceptance(document);
            await HintAcceptance((FrameworkElement)canvas!);
            await SchematicAcceptance(main, (FrameworkElement)canvas!);
            await NestedAcceptance(main, document, (FrameworkElement)canvas!);
            await UciEditorAcceptance(main);
            await FloatingAcceptance(main);
            File.WriteAllText(Path.Combine(Output, "contexts-result.json"), JsonSerializer.Serialize(new { success = true, checks, physicalInput = false }, SettingsJson.Format));
        }
        catch (Exception ex)
        { File.WriteAllText(Path.Combine(Output, "contexts-result.json"), JsonSerializer.Serialize(new { success = false, checks, error = ex.ToString(), physicalInput = false }, SettingsJson.Format)); }
    }
}
