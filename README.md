# Better Shortcuts

Customize mod and native Designer commands in **File > Preferences > Better Shortcuts**, also reachable from **Tools > Q-SYS Mods > Better Shortcuts**. Search for an action, enter a gesture, and choose Assign, Clear, or Reset. OK applies changes; Cancel discards them. Conflicting assignments are rejected before changing bindings. Native commands retain their availability and focus behavior; no global keyboard hook is installed.

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

Extracted from Schematic Tools commit `13c52e6`. [Split tracking](https://github.com/joshua-small/qsys-schematic-tools/issues/4).
