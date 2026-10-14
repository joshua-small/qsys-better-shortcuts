# Better Shortcuts

Customize mod and native Designer commands in **File > Preferences > Better Shortcuts**, also reachable from **Tools > Q-SYS Mods > Better Shortcuts**. The searchable, sortable table has **Command**, **Keybinding**, **When**, and **Source** columns. It includes all **60 entries** from the [official Q-SYS Shortcuts table](https://help.qsys.com/Content/Q-Sys_Designer/Q-Sys_Designer_Shortcuts.htm), six additional shortcuts documented elsewhere, two native wheel gestures, and commands registered by Designer and installed mods.

Search by command, keybinding, context, or source. Select a row to see its instructions and documented/default binding; the Q-SYS Help button opens that row's official source. Reference rows cover mouse gestures, multi-step procedures, and contextual commands that are not registered for remapping. They are visible but cannot be assigned, cleared, or reset. Numeric-keypad alternatives remain distinct.

Each key appears in its own keycap, with genuine alternatives on separate lines marked **or**. Duplicate native display labels are removed. For example, Actual Size shows **Ctrl + 0** or **Ctrl + NumPad 0**; Application Zoom In shows **Ctrl + Shift + =** or **Ctrl + Shift + NumPad +**. Punctuation uses US key labels, with a keyboard-layout note in the tooltip. Number-row and keypad keys are distinct physical bindings. Alternating rows, hover, and selection use separate backgrounds in both light and dark themes. Reference procedures retain **then** between successive steps.

**Double-click an assignable row, press the new shortcut, then press Enter.** Keyboard users can select the row and press Enter to open the same editor; **Change shortcut** is also available. Keycaps show the candidate. Escape, clicking outside the editor, leaving its focus, or switching windows cancels the edit. Modifier-only presses and auto-repeat do not capture a shortcut. Include Ctrl or Alt, or use a function key, to keep ordinary typing available. Enter confirms only after a new combination has been recorded; recording alone does not change the draft.

If the combination is already assigned, the editor names all detected conflicts and offers **Overwrite** or **Cancel**. Enter confirms the visible overwrite warning. Overwrite moves only matching gestures away from the conflicting commands, preserving their other alternatives; it does not silently allow competing commands. Clear disables the selected command; Reset restores all provider defaults and uses the same warning if those defaults conflict. Changes stay staged until Preferences **OK**; Preferences **Cancel** discards the whole draft. The table shows staged assignments and **User** sources immediately. When describes execution context; Source identifies the effective provider, including scoped Better Packing replacements and native passthrough. Mod failure, pending review and replacement-off status appear separately in row details. Designer and each mod still determine availability. No global keyboard hook is installed.

The interaction references are [VS Code's searchable shortcut editor and conflict inspection](https://code.visualstudio.com/docs/configure/keybindings) and [IntelliJ's key recorder and visible conflict warnings](https://www.jetbrains.com/help/idea/configuring-keyboard-and-mouse-shortcuts.html). Better Shortcuts uses the owner's requested direct row editing and explicit overwrite confirmation. It supports single key combinations, not sequential chords or configurable context expressions.

The mouse wheel over the table scrolls the list using Windows' line/page setting, including partial movements from precision wheels. This page routes the event before Designer's outer Preferences scroller can consume it; wheel input elsewhere is unchanged.

See the [10.5 documentation/code comparison](docs/SHORTCUT-AUDIT-10.5.md) for table omissions, physical-key ambiguities, and the owner-confirmed correction **Select only connector pins** for Alt + right-to-left selection. The owner also confirmed application zoom with Ctrl+Shift+mouse wheel. [Issue #2](https://github.com/joshua-small/qsys-better-shortcuts/issues/2) tracks the remaining application and physical-key acceptance. Scoped Designer checks for the Pack-row fix are recorded below.

Defaults:

| Shortcut | Action |
|---|---|
| Ctrl+Alt+1 / 2 / 3 | Toggle Design Elements / Properties / Schematic Elements |
| Ctrl+Alt+0 | Collapse all open docked sidebars; next press expands all |
| Ctrl+Alt+D | Toggle Designer's temporary light/dark mode |
| Ctrl+Alt+I | View Component Controls |
| Ctrl+Alt+P | Toggle Script Programmer Mode |

The all-sidebar action leaves closed and floating panels alone. In a mixed layout it collapses the expanded panels. Dark-mode toggle uses the native temporary toggle, so Q-SYS's saved Light/Dark/Auto preference still controls startup.

Additional assignable actions focus Inventory, Schematic Pages, User Control Interfaces, Snapshots, Named Controls, Inspector, and Schematic Elements search. They start unbound. The native command catalog also includes Distribute and available UCI import/export operations. Commands absent from the current native registry are not invented. Other mods register stable command IDs through Core and appear automatically; their menus still work without Better Shortcuts.

Overrides survive temporarily absent mods. Native replacements restore their original gestures on Reset. An override string may contain semicolon-separated alternatives when conflict transfer preserves part of a native binding set; older single-value settings still load. Optional `EditorOverrides` records only explicitly confirmed takeovers of direct editor input bindings, which are removed reversibly while the assignment is active. These confirmations do not grant ownership of unrelated keys. Floating Component Controls Info gets the applicable editor actions without depending on the Info Filter mod. Browser-hosted editors, OS-reserved keys, class bindings and editor scopes created later are not an exhaustive conflict inventory.

Requires [Q-SYS Mods Core](https://github.com/joshua-small/qsys-mods-core) 0.1.3 or newer within 0.1.x, Mod Manager 0.8+, and the exact supported Designer 10.5 build. Use the manager's bundle migration. Build `Designer.Modules.BetterShortcuts.csproj` with .NET 10.0.112, an adjacent Core checkout, and local `DesignerDir` references.

Core's guarded native driver verifies registration, duplicate rejection, remapping/reset, sidebar round trips, all seven focus actions, and theme changes (127 registered commands in the inspected host). Routed-command tests are not a claim of physical-key delivery in every editor or operating-system shortcut configuration.

That native-driver evidence predates the table UI. Offline catalog/projection checks for this update run with `dotnet run --project tests/Catalog --configuration Release --artifacts-path artifacts/catalog`. Build with `dotnet build Designer.Modules.BetterShortcuts.csproj --configuration Release --artifacts-path artifacts/wsl -p:DesignerDir="/mnt/c/Program Files/QSC/Q-SYS Designer 10.5"` through the repository's WSL route. The adjacent Core test driver is adapted separately for table selection; a compile is not a completed native test run.

The owner opened the initial table and reported duplicate labels, missing stripes, consumed wheel input, and unnecessary Record/Assign clicks. Current checks cover 45 offline catalog/presentation cases and 36 Windows WPF formatting, recorder-state and wheel-delta cases. Conflict-transfer and settings-compatibility checks moved to Core alongside the binding engine. Run the latter with `dotnet run --project tests/Gestures --configuration Release --artifacts-path artifacts/gestures` on Windows; it uses WPF's key types without opening a UI or requiring Designer. On this Windows-hosted checkout, compile that project through WSL and execute its built DLL with the Windows .NET runtime.

On 2026-10-14, matching 0.1.3 builds passed a scoped Designer 10.5 check: one row per directional Pack action; actual Ctrl+Shift+B/L/R/T keycaps; wheel scrolling; double-click opening the recorder; Clear, Reset, OK and reopen; and source changes when Better Packing is disabled or re-enabled. All four keys executed Better Packing once on a disposable two-object schematic; Pack Left also executed Q-SYS with replacement disabled. Cancel discarded a staged provider change without changing the preferences file. The automation tool deactivated the modal when sending recorder keys, so candidate capture, Enter confirmation and conflict overwrite through physical key input remain unverified. This does not complete the wider editor/context matrix in issue #2.

Extracted from Schematic Tools commit `13c52e6`. [Split tracking](https://github.com/joshua-small/qsys-schematic-tools/issues/4).

## Shared controls (0.1.3)

Core is now the sole binding engine and settings authority. This module is a presentation adapter; it does not install a second set of bindings. Old Overrides and EditorOverrides migrate without losing alternatives, absent-mod keys or explicit editor takeover consent. Core retains those native overrides when Better Shortcuts is absent.

Better Packing's four native replacements default on and are switched together by **Preferences > Better Packing > Disable default Q-SYS control overrides**. Each direction has one row with its actual keys and effective provider. Core migrates assignments from the old duplicate mod rows onto the standard actions. The two additional center operations remain distinct and unbound by default. The table reports user remapping and the selected provider, with native defaults/documentation retained in details. Turning off replacement changes the source to Q-SYS while preserving the keys.

**Review mod controls…** is shown only when a ready mod has other reviewable proposals. It opens the same shared draft for first-run choices and bulk overwrite. Better Packing does not appear in this checklist. With only Packing's grouped replacements, the general Mod Controls page and menu are hidden.

Install the matching Core/Packing/Shortcuts 0.1.3 test builds together. The signed catalog's minimum-Core policy must be updated before public release. The scoped Designer checks above are separate from the compiled-only native driver and do not establish full editor, UCI or hardware acceptance.
