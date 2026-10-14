# Better Shortcuts default keymap (0.1.5)

These are mod defaults for the exact supported Designer 10.5 build, not changes to Q-SYS's published defaults. Saved assignments and explicit clears take precedence. A conflicting new default stays inactive until the user chooses to overwrite it. The table and menus show effective keys; row details retain the default and conflict information. **Ctrl+Shift+P is reserved for the separate Command Palette mod.**

**then** means release the first combination and press the second within ten seconds. Unless shown, the second key needs no modifier. Chords cancel on Escape, invalid completion, mouse click, focus/window change or timeout. Actions that modify a design use native actions/tools and remain subject to Design mode, locking and selection conditions.

| Action | Default |
| --- | --- |
| Toggle Design Elements / Schematic Elements / Properties | Shift+Alt+D / E / P |
| Toggle Left / Right / All Docks | Shift+Alt+[ / ] / \ |
| Toggle Light/Dark Mode | Ctrl+Alt+D |
| Script Programmer Mode | Ctrl+Alt+P |
| View Component Controls Info | Ctrl+I |
| Design Properties | Alt+Enter |
| Focus Inventory / Schematic Pages / UCIs / Snapshots / Named Controls / Inspector | Alt+1 / 2 / 3 / 4 / 5 / 6 |
| Focus Schematic Elements Search | Alt+S |
| Add Text Block / Header / Group Box | Shift+Alt+T / H / G |
| Add Polygon | Ctrl+Alt+G then P |
| Add UCI Flexbox | Shift+Alt+F |
| Edit Code Name/Control Name | F2, when the selected object exposes an editable name field |
| Context Color Picker | I, only with an unlocked color swatch focused; this opens the native picker, not an eyedropper over the canvas |

Left/right/all dock commands affect the three main panels. Closed and floating panels are left alone. The current UCI's local dock has its own command below. Text/Header/Group/Polygon activate the native drawing tool on a schematic or UCI canvas; Flexbox is inserted at the visible UCI canvas center.

| Creation action | Default |
| --- | --- |
| Add New Inventory Item | Ctrl+Shift+N then Ctrl+Shift+I |
| Add New Schematic Page | Ctrl+Shift+N then Ctrl+Shift+P |
| Add New User Control Interface | Ctrl+Shift+N then Ctrl+Shift+U |
| Add UCI Variable | Ctrl+Shift+N then Ctrl+Shift+V |

Ctrl+Shift+P appears only as the completion of the page chord; pressing it alone is free for Command Palette.

| Schematic Elements category | Default |
| --- | --- |
| Audio / Control / Layout / Monitor / Scripting / UCI Buttons / Video | Alt+C then Alt+A / O / L / M / S / U / V |

Category actions clear the component search, expand the category and focus it. Bare Alt+F/T/H are avoided because Designer uses them for File/Tools/Help.

| Current UCI action | Default |
| --- | --- |
| Focus Layers / Toolbox / Variables | Ctrl+Alt+U then L / T / V |
| Toggle UCI Left Dock | Ctrl+Alt+U then D |
| Expand / Collapse All Layers | Ctrl+Alt+U then E / C |
| Show/Hide / Lock/Unlock Selected Layers | Ctrl+Alt+U then H / K |
| Focus Layout / Script | Ctrl+Alt+U then 1 / 2 |
| Move Selected Layer Up / Down | Alt+Up / Down, with the Layers tree focused and one movable layer selected |
| Add Button / Knob / Text / Fader / Meter / Indicator | Ctrl+Alt+B then B / K / T / F / M / I |

UCI control creation first opens a native subtype chooser, then inserts that subtype at the visible canvas center. Layer visibility/locking requires an unlocked UCI and a layer selection.

Auto Update Styles is now assignable and linked to its existing menu hint, but has no default. Existing native export/import, configuration and resource actions remain available wherever the host registers them. The remaining blank suggestions (including Add Selected to Named Controls, Lock All and a broader color-picker context) require further design/native-adapter work in [issue #4](https://github.com/joshua-small/qsys-better-shortcuts/issues/4). A listed implementation is not a claim that every context has passed Designer testing; see [acceptance](DESIGNER-VALIDATION-10.5.md).
