# Q-SYS Designer 10.5 shortcut comparison

Reviewed October 14, 2026, against the live [Shortcuts table](https://help.qsys.com/Content/Q-Sys_Designer/Q-Sys_Designer_Shortcuts.htm) (footer: Q-SYS 10.5.0) and the installed Designer build. This is documentation and static implementation evidence, not physical-key or application acceptance. No Designer launch, installation, Core connection, or design edit was performed for this comparison.

## Coverage and provenance

`ShortcutCatalog.cs` retains all **60** official rows: Objects 27, Editing 10, Individual Controls 6, Q-SYS Designs 17. This includes mouse gestures and multi-step procedures, not just assignable commands. Instructions are summarized; the source links provide the full text. Six additional actions documented elsewhere and two additional native wheel gestures appear separately. The running command registry supplies all other available native and mod commands, including commands with no default binding.

Read-only static inspection covered the Designer command classes, canvas editing tools, main-window zoom handlers, UCI Remap/Relink commands, both editor wrappers, and the bundled Search and Replace module's binding. Vendor code and raw decompilation stay outside this repository. These are private implementation observations, not supported Q-SYS SDK contracts.

| Local input | SHA-256 |
|---|---|
| Q-Sys Designer.dll | `0e7150f8dc02e0159e019188780fb003ce025f5ec18c491609cf337d849bda60` |
| QscCommon.dll (English UCI command labels) | `d396132c55cdccacec0b19ed073e2688264bfdc27cb83d1e2f9d34e81da00a80` |
| Designer.Modules.SearchAndReplace.dll | `78ae61c27dd7041a1a1800a12d7d5a7c8b7405194139794e8fe58a32bab63640` |

## Findings

No contradictory keyboard assignment was found among the inspected commands corresponding to the official table. Alignment, packing, grouping, ordering, editing, file operations, Check Design, zoom, F5–F8, Remap (`Ctrl+M`), and Relink (`Ctrl+Alt+L`) agree. The legacy editor defines Find Next as F3; the bundled Search and Replace module defines Ctrl+H. F1 uses WPF's Help command. This is not a claim that every documented procedure or keyboard layout has passed an application test.

The following are **missing from the central table, but documented elsewhere**:

| Command | Keybinding | Other official source |
|---|---|---|
| Application Zoom In | Ctrl+Shift++ | [Getting Started](https://help.qsys.com/Content/Q-Sys_Designer/002_Getting_Started.htm) |
| Application Zoom Out | Ctrl+Shift+- | [Getting Started](https://help.qsys.com/Content/Q-Sys_Designer/002_Getting_Started.htm) |
| Application Zoom Actual Size | Ctrl+Shift+Backspace | [Getting Started](https://help.qsys.com/Content/Q-Sys_Designer/002_Getting_Started.htm) |
| Trace Lock | Ctrl+Shift+K | [Signal Wire Tracing](https://help.qsys.com/Content/Q-Sys_Designer/Using_the_Schematic/Signal_Wire_Tracing.htm) |
| Nudge selection | Arrow keys | [Organizing Your Design](https://help.qsys.com/content/Q-SYS_Designer/Using_the_Schematic/Labeling_and_Organizing_Schematic_Elements.htm) |
| Nudge selection by ten units | Ctrl+Arrow keys | [Organizing Your Design](https://help.qsys.com/content/Q-SYS_Designer/Using_the_Schematic/Labeling_and_Organizing_Schematic_Elements.htm) |

Additional implementation findings:

- `GraphicCanvas.Zoom` handles **Ctrl+mouse wheel**, without Shift, for canvas zoom. This is absent from the table; a popup-zoom fix in the [10.4 release notes](https://help.qsys.com/Content/Q-Sys_Designer/QDS_Release_Notes.htm?TocPath=Q-SYS+Designer+Software%7C_____1) also acknowledges it. That unversioned release-note URL can move to a later release.
- `MainWindow.MainWindow_MouseWheel` handles **Ctrl+Shift+mouse wheel** to change application scale when the event has not already been handled. No description was located in the reviewed Help pages or targeted Help search. This is a candidate documentation omission, not proof that no Q-SYS document mentions it.
- **Potential wording mismatch:** the table describes Alt + right-to-left box selection as selecting only right-side connector pins. `SelectionTool.OnMouseLeftButtonUp` actually reverses intersection/enclosure selection when Alt is held, then `TrySelect` considers canvas elements and their children. `EditSelectionFilter` has no right-side-pin-only filter. The documented gesture remains in the UI with a note; test a box containing left pins, right pins, and a fully enclosed object before declaring the public wording wrong.
- **Zoom key ambiguity:** the code uses `Ctrl+OemPlus` and `Ctrl+Add`, `Ctrl+OemMinus` and `Ctrl+Subtract`, plus both number-row and keypad 0/9. The public `Ctrl++` notation hides the physical key distinction. On keyboards where `+` requires Shift, the extra modifier can select application zoom instead. Test the actual keyboard layout; do not silently normalize those gestures together.
- **Names differ without behavior conflicts:** Check Design is `Check Resources`; Actual Size is `Zoom Default`; Load from Core and Connect is the native `Upload` command with a generic Show Dialog label. The table gives these their documented names and maps by exact owner/name identity.
- **Unbound is not undocumented:** center alignment, distribution, center packing, lock/unlock, UCI operations, and other tools can exist as commands without default keyboard bindings. They remain visible as Unassigned when registered. No keys are invented for them.
- **Context matters:** Remap/Relink are bound by the UCI view; F3 belongs to the script editor; Ctrl+H is installed by a bundled module using a direct input binding. A main-window registry alone cannot establish their remapping support. These remain reference rows when no matching registered command is present. Monaco handles its editing keys separately, so this change does not intercept its Ctrl+F/Ctrl+H or reopen the previously resolved search conflict.

## Table behavior and next acceptance

The four columns are Command, Keybinding, When, and Source. Search matches all four plus details and section names. Keybinding shows the effective staged assignment for registered commands; the selected row retains the documented and native defaults. Source identifies Q-SYS, the providing mod, User overrides, Help reference rows, or static native references. Clear remains a User override; Reset removes the override. Read-only references cannot be assigned, cleared, or reset.

When is a readable context summary, not a VS Code expression language or a new execution gate. For commands without a specific reviewed context it explicitly defers to the provider's availability rules. The existing routing, settings format, and native guards are unchanged. Exact native owner/name matching avoids accidentally combining a mod action with a same-named Q-SYS command; unmatched or ambiguous identities remain references.

Offline checks cover complete section counts, unique identities, instructions, absent commands, native/mod name collisions, staged Assign/Clear/Reset display, and search. The WPF project is compiled against the local Designer references. These do not validate rendering, physical delivery, or Designer acceptance.

[Issue #2](https://github.com/joshua-small/qsys-better-shortcuts/issues/2) remains the owner of the physical/context test matrix. First inspect the table in both themes and at smaller window sizes, then test search/sorting, reference selection, staged edits/Cancel, Reset conflicts, persistence, keyboard-layout zoom, the Alt-selection wording, and the two wheel gestures. Continue with the already tracked main/floating/editor/UCI matrix. Use copied disposable fixtures; no live Core is needed for the first UI pass.
