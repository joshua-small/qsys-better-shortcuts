namespace Designer.Modules.BetterShortcuts;

// Factual shortcut inventory. Longer instructions are summarized in our own words.
// Keep one entry per official table row, including distinct contextual uses of a key.
internal sealed record ShortcutReference(string Id, string Section, string Command, string Keybinding,
    string When, string Details = "", string? NativeName = null,
    string NativeOwner = "QSC.QSys.MainWindow", string SourceUrl = ShortcutCatalog.HelpUrl,
    bool InOfficialTable = true);

internal static class ShortcutCatalog
{
    internal const string HelpUrl = "https://help.qsys.com/Content/Q-Sys_Designer/Q-Sys_Designer_Shortcuts.htm";
    internal const string GettingStartedUrl = "https://help.qsys.com/Content/Q-Sys_Designer/002_Getting_Started.htm";
    internal const string TraceUrl = "https://help.qsys.com/Content/Q-Sys_Designer/Using_the_Schematic/Signal_Wire_Tracing.htm";
    internal const string OrganizingUrl = "https://help.qsys.com/content/Q-SYS_Designer/Using_the_Schematic/Labeling_and_Organizing_Schematic_Elements.htm";
    internal const string ApplicationOwner = "System.Windows.Input.ApplicationCommands";
    internal static readonly ShortcutReference[] All =
    [
        new("align-left", "Objects", "Align Left", "Ctrl+L", "Multiple unlocked objects selected", NativeName: "Horizontal Align Left"),
        new("align-top", "Objects", "Align Top", "Ctrl+T", "Multiple unlocked objects selected", NativeName: "Vertical Align Top"),
        new("align-bottom", "Objects", "Align Bottom", "Ctrl+B", "Multiple unlocked objects selected", NativeName: "Vertical Align Bottom"),
        new("align-right", "Objects", "Align Right", "Ctrl+R", "Multiple unlocked objects selected", NativeName: "Horizontal Align Right"),
        new("pack-left", "Objects", "Pack Left", "Ctrl+Shift+L", "Multiple unlocked objects selected", NativeName: "Pack Left"),
        new("pack-top", "Objects", "Pack Top", "Ctrl+Shift+T", "Multiple unlocked objects selected", NativeName: "Pack Top"),
        new("pack-bottom", "Objects", "Pack Bottom", "Ctrl+Shift+B", "Multiple unlocked objects selected", NativeName: "Pack Bottom"),
        new("pack-right", "Objects", "Pack Right", "Ctrl+Shift+R", "Multiple unlocked objects selected", NativeName: "Pack Right"),
        new("group", "Objects", "Group", "Ctrl+G", "Groupable objects selected", NativeName: "Group"),
        new("ungroup", "Objects", "Ungroup", "Ctrl+Shift+G", "A group is selected", NativeName: "Ungroup"),
        new("front", "Objects", "Bring to Front", "Ctrl+]", "Reorderable objects selected", NativeName: "Bring to Front"),
        new("back", "Objects", "Send to Back", "Ctrl+[", "Reorderable objects selected", NativeName: "Send to Back"),
        new("select-add", "Objects", "Select multiple objects one at a time", "Ctrl+click / Shift+click", "Selecting objects"),
        new("select-remove", "Objects", "Deselect one object", "Ctrl+click", "Multiple objects selected"),
        new("constrain", "Objects", "Constrain movement to one axis", "Shift+drag", "Moving an object"),
        new("drag-copy", "Objects", "Make a copy", "Alt+drag", "Dragging an object"),
        new("snap", "Objects", "Disable snap guides", "Ctrl+drag", "Moving an object"),
        new("signal-link", "Objects", "Create a Signal Link", "Space while dragging a wire", "Dragging from a component pin"),
        new("label", "Objects", "Create a component label", "Type text", "One component selected"),
        new("wire-break", "Objects", "Orthogonal routing", "Drag a wire", "Editing a wire", "Dragging introduces a breakpoint."),
        new("intersect", "Objects", "Select intersecting objects", "Drag selection right to left", "Selecting on the canvas"),
        new("encompass", "Objects", "Select fully enclosed objects", "Drag selection left to right", "Selecting on the canvas"),
        new("alt-intersect", "Objects", "Select intersecting objects (alternate)", "Alt+drag selection left to right", "Selecting on the canvas"),
        new("pins", "Objects", "Select only connector pins", "Alt+drag selection right to left", "Selecting component pins", "Owner confirmed on October 14, 2026: selects connector pins inside the selection box on either side. This corrects the Help table's right-side-only wording."),
        new("multi-wire", "Objects", "Connect multiple wires", "Select pins, then drag one selected pin", "Compatible pins on the target component", "Mixed signal types are allowed when the target has matching pins."),
        new("multi-name", "Objects", "Create multiple output Signal Names", "Select output pins; type a name; Enter", "Multiple output pins selected", "Designer appends sequential numbers to the base name."),
        new("paste-names", "Objects", "Connect output Signal Names to inputs", "Ctrl+C, then Ctrl+V", "Output Signal Names, then an equal number of input pins", "Copy the selected output Signal Names, select the destination input pins, then paste."),
        new("cut", "Editing", "Cut", "Ctrl+X", "Focused editor permits cutting", NativeName: "Cut", NativeOwner: ApplicationOwner),
        new("copy", "Editing", "Copy", "Ctrl+C", "Focused editor has a copyable selection", NativeName: "Copy", NativeOwner: ApplicationOwner),
        new("paste", "Editing", "Paste", "Ctrl+V", "Focused editor accepts the clipboard contents", NativeName: "Paste", NativeOwner: ApplicationOwner),
        new("delete", "Editing", "Delete", "Delete", "Focused editor has a deletable selection", NativeName: "Delete", NativeOwner: ApplicationOwner),
        new("duplicate", "Editing", "Duplicate", "Ctrl+D", "Duplicable objects selected", NativeName: "Duplicate"),
        new("select-all", "Editing", "Select All", "Ctrl+A", "Focused editor supports selection", NativeName: "SelectAll", NativeOwner: ApplicationOwner),
        new("undo", "Editing", "Undo", "Ctrl+Z", "An undoable operation is available", NativeName: "Undo", NativeOwner: ApplicationOwner),
        new("redo", "Editing", "Redo", "Ctrl+Y", "A redoable operation is available", NativeName: "Redo", NativeOwner: ApplicationOwner),
        new("find", "Editing", "Find", "Ctrl+F", "Designer Find; script editors have their own search", NativeName: "Find"),
        new("replace", "Editing", "Search and Replace", "Ctrl+H", "Designer Search and Replace; script search is separate", "The bundled SearchAndReplace module installs its own input binding; it is not a remappable Core registry command."),
        new("control-values", "Individual Controls", "Copy settings between controls", "Ctrl+C, then Ctrl+V", "Source controls, then destination controls selected", "Hold Ctrl when selecting a button. Copy the source selection, select the destination, then paste."),
        new("control-multiple", "Individual Controls", "Set multiple controls together", "Select controls, then adjust one", "Selected controls are the same type"),
        new("component-values", "Individual Controls", "Copy and paste component settings", "Context menu: Copy, then Paste", "Run or Emulate; source and target are the same component type", "Some transient values, such as an Audio Player's playback time, are excluded. This paste cannot be undone with Ctrl+Z."),
        new("control-position", "Individual Controls", "Paste a control at its original position", "Ctrl+C; Ctrl+V; Shift+click", "Pasting onto another schematic or UCI page", "After copying, begin paste on the destination page. Hold Shift through the placement click to retain the original coordinates."),
        new("remap", "Individual Controls", "Remap Controls", "Ctrl+M", "UCI editor; edit tools active", "The native binding belongs to the UCI view and may be absent from the main-window command registry.", NativeName: "Remap UCI Controls [BETA]"),
        new("relink", "Individual Controls", "Relink Controls", "Ctrl+Alt+L", "UCI document active; edit tools active", "The native binding belongs to the UCI view and may be absent from the main-window command registry.", NativeName: "Relink UCI Controls [BETA]"),
        new("new", "Q-SYS Designs", "New Design", "Ctrl+N", "Designer", NativeName: "New", NativeOwner: ApplicationOwner),
        new("open", "Q-SYS Designs", "Open a Design", "Ctrl+O", "Designer", NativeName: "Open", NativeOwner: ApplicationOwner),
        new("sample", "Q-SYS Designs", "Open Sample Design", "Ctrl+Shift+O", "Designer", NativeName: "Open Sample Design"),
        new("save", "Q-SYS Designs", "Save the Current Design", "Ctrl+S", "Current design can be saved", NativeName: "Save", NativeOwner: ApplicationOwner),
        new("save-as", "Q-SYS Designs", "Save As", "Ctrl+Shift+S", "Current design can be saved", NativeName: "SaveAs", NativeOwner: ApplicationOwner),
        new("check", "Q-SYS Designs", "Check Design", "Shift+F6", "Current design is available", "Native command name: Check Resources.", NativeName: "Check Resources"),
        new("zoom-in", "Q-SYS Designs", "Zoom In", "Ctrl++", "Schematic or UCI canvas active", "Designer binds Ctrl+OemPlus and Ctrl+Add separately; physical key labels depend on keyboard layout.", NativeName: "Zoom In"),
        new("zoom-out", "Q-SYS Designs", "Zoom Out", "Ctrl+-", "Schematic or UCI canvas active", "Main keyboard and numeric keypad bindings are separate.", NativeName: "Zoom Out"),
        new("actual", "Q-SYS Designs", "Actual Size", "Ctrl+0", "Schematic or UCI canvas active", "Both number-row 0 and numeric-keypad 0 are defined.", NativeName: "Zoom Default"),
        new("fit", "Q-SYS Designs", "Fit to Screen", "Ctrl+9", "Schematic or UCI canvas active", "Both number-row 9 and numeric-keypad 9 are defined.", NativeName: "Zoom Fit To Screen"),
        new("scroll", "Q-SYS Designs", "Horizontal Scroll", "Shift+mouse wheel", "Scrollable schematic, UCI, or editor"),
        new("help", "Q-SYS Designs", "Help", "F1", "Designer; selection can determine the help topic", NativeName: "Help", NativeOwner: ApplicationOwner),
        new("find-next", "Q-SYS Designs", "Find Next", "F3", "Searching in the Script Editor", "Editor-owned shortcut; not a global Designer command."),
        new("run", "Q-SYS Designs", "Save to Core & Run", "F5", "Native connection and deployment checks pass", NativeName: "Connect"),
        new("emulate", "Q-SYS Designs", "Emulate", "F6", "Native emulation checks pass", NativeName: "Emulate"),
        new("disconnect", "Q-SYS Designs", "Disconnect from the Core", "F7", "Native disconnect checks pass", NativeName: "Disconnect"),
        new("load", "Q-SYS Designs", "Load from Core and Connect", "F8", "Designer can open the Core selection dialog", "Native command name: Upload; its label is Show Dialog…", NativeName: "Upload"),
        new("app-zoom-in", "Additional Q-SYS Help", "Application Zoom In", "Ctrl+Shift++", "Designer interface", "Documented in Getting Started, absent from the Shortcuts table.", "Application Zoom In", SourceUrl: GettingStartedUrl, InOfficialTable: false),
        new("app-zoom-out", "Additional Q-SYS Help", "Application Zoom Out", "Ctrl+Shift+-", "Designer interface", "Documented in Getting Started, absent from the Shortcuts table.", "Application Zoom Out", SourceUrl: GettingStartedUrl, InOfficialTable: false),
        new("app-zoom-reset", "Additional Q-SYS Help", "Application Zoom Actual Size", "Ctrl+Shift+Backspace", "Designer interface", "Documented in Getting Started, absent from the Shortcuts table.", "Application Zoom Actual Size", SourceUrl: GettingStartedUrl, InOfficialTable: false),
        new("trace-lock", "Additional Q-SYS Help", "Trace Lock", "Ctrl+Shift+K", "Trace mode on; active or already locked trace", "Documented in Signal Wire Tracing for Run/Emulate, absent from the Shortcuts table.", "Trace Lock", SourceUrl: TraceUrl, InOfficialTable: false),
        new("nudge", "Additional Q-SYS Help", "Move selection by one unit", "Arrow keys", "Canvas edit tools; unlocked selection; nonnegative destination", "Documented in Organizing Your Design; implemented by NudgeTool rather than a routed command.", SourceUrl: OrganizingUrl, InOfficialTable: false),
        new("nudge-large", "Additional Q-SYS Help", "Move selection by ten units", "Ctrl+Arrow keys", "Canvas edit tools; unlocked selection; nonnegative destination", "Documented in Organizing Your Design; implemented by NudgeTool rather than a routed command.", SourceUrl: OrganizingUrl, InOfficialTable: false)
    ];
}
