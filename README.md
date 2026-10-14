# Better Shortcuts

Customize mod and native Designer commands in **File > Preferences > Better Shortcuts**, also reachable from **Tools > Q-SYS Mods > Better Shortcuts**. The searchable, sortable table has **Command**, **Keybinding**, **When**, and **Source** columns. It includes all **60 entries** from the [official Q-SYS Shortcuts table](https://help.qsys.com/Content/Q-Sys_Designer/Q-Sys_Designer_Shortcuts.htm), six additional shortcuts documented elsewhere, two native wheel gestures, and commands registered by Designer and installed mods.

Search by command, keybinding, context, or source. Select a row to see its instructions and documented/default binding; the Q-SYS Help button opens that row's official source. Reference rows cover mouse gestures, multi-step procedures, and contextual commands that are not registered for remapping. They are visible but cannot be assigned, cleared, or reset. Numeric-keypad alternatives remain distinct.

For an assignable command, enter one gesture and choose Assign, Clear, or Reset. The table immediately shows the staged keybinding and **User** source. Clear disables the binding; Reset restores its provider default. OK applies changes; Cancel discards them. Conflicting assignments, including conflicting resets, are rejected before changing the draft. When summarizes context in plain language; Designer and each mod still decide whether a command is available. No global keyboard hook is installed.

See the [10.5 documentation/code comparison](docs/SHORTCUT-AUDIT-10.5.md) for table omissions, physical-key ambiguities, and the owner-confirmed correction **Select only connector pins** for Alt + right-to-left selection. The owner also confirmed application zoom with Ctrl+Shift+mouse wheel. [Issue #2](https://github.com/joshua-small/qsys-better-shortcuts/issues/2) tracks the remaining application and physical-key acceptance; the new table itself has not yet passed that phase.

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

Overrides survive temporarily absent mods. Native replacements restore their original gestures on Reset. Floating Component Controls Info gets the applicable editor actions without depending on the Info Filter mod.

Requires [Q-SYS Mods Core](https://github.com/joshua-small/qsys-mods-core) 0.1.x, Mod Manager 0.8+, and the exact supported Designer 10.5 build. Use the manager's bundle migration. Build `Designer.Modules.BetterShortcuts.csproj` with .NET 10.0.112, an adjacent Core checkout, and local `DesignerDir` references.

Core's guarded native driver verifies registration, duplicate rejection, remapping/reset, sidebar round trips, all seven focus actions, and theme changes (127 registered commands in the inspected host). Routed-command tests are not a claim of physical-key delivery in every editor or operating-system shortcut configuration.

That native-driver evidence predates the table UI. Offline catalog/projection checks for this update run with `dotnet run --project tests/Catalog --configuration Release --artifacts-path artifacts/catalog`. Build with `dotnet build Designer.Modules.BetterShortcuts.csproj --configuration Release --artifacts-path artifacts/wsl -p:DesignerDir="/mnt/c/Program Files/QSC/Q-SYS Designer 10.5"` through the repository's WSL route. The adjacent Core test driver is adapted separately for table selection; a compile is not a completed native test run.

Extracted from Schematic Tools commit `13c52e6`. [Split tracking](https://github.com/joshua-small/qsys-schematic-tools/issues/4).
