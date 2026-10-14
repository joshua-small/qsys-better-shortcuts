# Q-SYS Designer 10.5 shortcut comparison

Reviewed October 14, 2026, against the live [Shortcuts table](https://help.qsys.com/Content/Q-Sys_Designer/Q-Sys_Designer_Shortcuts.htm) (footer: Q-SYS 10.5.0) and the installed Designer build. The comparison uses documentation and static implementation evidence, with two owner-reported application observations added below on the same date. The agent performed no Designer launch, installation, Core connection, or design edit. The owner observations do not establish acceptance of the new table or the broader input/context matrix.

## Coverage and provenance

`ShortcutCatalog.cs` retains all **60** official rows: Objects 27, Editing 10, Individual Controls 6, Q-SYS Designs 17. This includes mouse gestures and multi-step procedures, not just assignable commands. Instructions are summarized; the source links provide the full text. Six additional actions documented elsewhere and two additional native wheel gestures appear separately. The running command registry supplies all other available native and mod commands, including commands with no default binding.

These observations are version-specific and do not establish supported Q-SYS SDK contracts.


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
- `MainWindow.MainWindow_MouseWheel` handles **Ctrl+Shift+mouse wheel** to change application scale when the event has not already been handled. **Owner confirmed on October 14, 2026:** the gesture zooms the application in and out on the current installation. No description was located in the reviewed Help pages or targeted Help search. This is a candidate documentation omission, not proof that no Q-SYS document mentions it.
- **Owner-confirmed wording correction:** Alt + right-to-left box selection selects connector pins within the box on either side, not only the right side. The owner verified this on October 14, 2026 and specified the corrected description **Select only connector pins**, now used in the table. The original Help wording is retained in the row's details for provenance. The owner observation resolves that specific wording question. The fixture, mode, and keyboard layout were not separately recorded, so this is scoped to the reported observation.
- **Zoom key ambiguity:** the code uses `Ctrl+OemPlus` and `Ctrl+Add`, `Ctrl+OemMinus` and `Ctrl+Subtract`, plus both number-row and keypad 0/9. The public `Ctrl++` notation hides the physical key distinction. On keyboards where `+` requires Shift, the extra modifier can select application zoom instead. Test the actual keyboard layout; do not silently normalize those gestures together.
- **Names differ without behavior conflicts:** Check Design is `Check Resources`; Actual Size is `Zoom Default`; Load from Core and Connect is the native `Upload` command with a generic Show Dialog label. The table gives these their documented names and maps by exact owner/name identity.
- **Unbound is not undocumented:** center alignment, distribution, center packing, lock/unlock, UCI operations, and other tools can exist as commands without default keyboard bindings. They remain visible as Unassigned when registered. No keys are invented for them.
- **Context matters:** Remap/Relink are bound by the UCI view; F3 belongs to the script editor; Ctrl+H is installed by a bundled module using a direct input binding. A main-window registry alone cannot establish their remapping support. These remain reference rows when no matching registered command is present. Monaco handles its editing keys separately, so this change does not intercept its Ctrl+F/Ctrl+H or reopen the previously resolved search conflict.

## Table behavior and next acceptance

The four columns are Command, Keybinding, When, and Source. Search matches all four plus details and section names. Keybinding shows the effective staged assignment for registered commands; the selected row retains the documented and native defaults. Source identifies Q-SYS, the providing mod, User overrides, Help reference rows, or static native references. Clear remains a User override; Reset removes the override. Read-only references cannot be assigned, cleared, or reset.

The owner's first table screenshots exposed a display bug: WPF's `KeyGestureConverter` appends a comma and the native `DisplayString`, producing values such as `Ctrl+0,Ctrl+0` and `Ctrl+NumPad0,Ctrl+0`. These are serialized labels, not extra shortcuts. Serialization now uses only the key and modifiers, and duplicate native key/modifier identities are removed before display. The table renders individual keycaps, uses **or** for genuine alternatives, and keeps number-row/keypad distinctions. OEM punctuation is displayed with US key labels and a layout tooltip; the original modifiers are unchanged. Explicit row/cell templates preserve alternating backgrounds under Designer's native table styling. These display fixes do not change command routing.

When is a readable context summary, not a VS Code expression language or a new execution gate. For commands without a specific reviewed context it explicitly defers to the provider's availability rules. Native execution guards remain intact. Exact native owner/name matching avoids accidentally combining a mod action with a same-named Q-SYS command; unmatched or ambiguous identities remain references. The later overwrite flow described below extends settings and reversible input-binding replacement.

Offline checks cover complete section counts, unique identities, instructions, absent commands, native/mod name collisions, staged Assign/Clear/Reset display, and search. The WPF project is compiled against the local Designer references. These do not validate rendering, physical delivery, or Designer acceptance.

The formatting revision adds regression coverage for display-string serialization, key/modifier identity deduplication, aliases, number-row/keypad alternatives, OEM punctuation, exact serialization round trips, individual plus-key tokens, and **then** versus **or** in reference procedures.

The owner also reported that the wheel did not scroll the list. The outer Preferences scroller consumed `PreviewMouseWheel` before the table received it. Better Shortcuts now handles wheel input at the Preferences window only when the original source belongs to this table, then scrolls the table's own viewer. The handler is detached when the table unloads. Windows' line/page preference and partial wheel deltas are preserved.

The recorder now opens directly on row double-click or Enter. It captures a key plus modifiers, resolves WPF system/IME/dead-key wrappers, preserves physical keypad/OEM identity, and consumes capture events locally. Enter confirms a complete recorded combination; Escape, focus loss, page unload, or window deactivation cancels. The editor names detected conflicting commands and offers Overwrite/Cancel before any staged change. Confirming transfers only the conflicting gestures; other alternatives survive, using semicolon-separated override values. Optional `EditorOverrides` records explicit consent to suppress direct editor bindings and restores them when that ownership is cleared. Existing single-value settings load without granting new takeover consent. Preferences OK still saves the draft; Cancel discards it.

The design references are [VS Code's shortcut editor and conflict inspection](https://code.visualstudio.com/docs/configure/keybindings) and [IntelliJ's recording dialog and conflict warnings](https://www.jetbrains.com/help/idea/configuring-keyboard-and-mouse-shortcuts.html). The owner's requested double-click/keyboard flow and explicit overwrite choice are applied here; this is not a claim of identical behavior across products. The conflict inventory covers registered commands and inspected direct input bindings in applicable current scopes, not browser-hosted editor keys, class bindings, future scopes or OS interception.

Forty portable checks and fifty-three standalone WPF formatting/recorder/conflict/settings/wheel checks pass without starting Designer or showing UI. Actual double-click activation, key delivery, overwrite routing, focus/cancellation and appearance remain for owner testing.

[Issue #2](https://github.com/joshua-small/qsys-better-shortcuts/issues/2) remains the owner of the physical/context test matrix. Alt + right-to-left pin selection and Ctrl+Shift+mouse-wheel application zoom have the owner confirmations above. First inspect the new table in both themes and at smaller window sizes, then test search/sorting, reference selection, staged edits/Cancel, Reset conflicts, persistence, keyboard-layout zoom, and Ctrl+mouse-wheel canvas zoom. Continue with the already tracked main/floating/editor/UCI matrix. Use copied disposable fixtures; no live Core is needed for the first UI pass.
