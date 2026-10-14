# Designer shortcut acceptance — 10.5.0

## Historical 0.1.4 build and scope

On 2026-10-14, Core 0.1.4 (beafd5738039dc3a6b641847f22bfd7ccfe2cd57) and Better Shortcuts 0.1.4 (0add5bca3743ed75d59cf2d07ed746f4ad7495ad) were tested in the installed Designer. Better Packing remained 0.1.3. These checks used keyboard input and observed Designer UI; they did not use a live Core.

## Individual actions and saved remaps

| Action | Mod default tested | Temporary saved remap tested | Observed result |
| --- | --- | --- | --- |
| Toggle Design Elements | Ctrl+Alt+1 | Ctrl+Shift+Alt+1 | Default collapsed/restored the named panel while a UCI was active; remapped key collapsed it on a root schematic. |
| Toggle Properties | Ctrl+Alt+2 | Ctrl+Shift+Alt+2 | Default collapsed/restored only Properties; remapped key collapsed it on a root schematic. |
| Toggle Schematic Elements | Ctrl+Alt+3 | Ctrl+Shift+Alt+3 | Default collapsed/restored only Schematic Elements; remapped key collapsed it on a root schematic. |
| Script Programmer Mode | Ctrl+Alt+P | Ctrl+Shift+Alt+P | Both keys checked the native Tools item. Clicking that item restored the initial unchecked state. |
| View Component Controls Info | Ctrl+Alt+I | Ctrl+Shift+Alt+I | Both keys opened one Component Controls window. No component was selected, so the native selection prompt was shown. |

The View > Sidebars hints displayed all three temporary assignments. Both Tools hints displayed their temporary assignments. The old Ctrl+Alt+1 no longer collapsed Design Elements while its override was active. Ctrl+Alt+0 restored the three panels after the remapped collapse tests.

The automated input helper deactivated the Preferences dialog when typing in its search field. Therefore the temporary five-key configuration was prepared in a separate copy, loaded only while Designer was closed, and checked after restart. This establishes saved-remap dispatch and menu presentation, **not a new end-to-end recorder/save/live-refresh test**. The owner separately confirmed recorder capture and conflicts work. Live hint refresh, Clear/Reset and preservation of native click handlers remain covered by Core's 124 WPF checks.

Original mod preferences were restored byte-for-byte after testing. The initial unsaved design was preserved to a separate local file and reopened; its hash stayed unchanged. The application was left with all three panels expanded, programmer mode off and the original bindings. Raw captures, preference copies and the design remain local.

## Proposed-binding conflict observations

Alt+F, Alt+T and Alt+H opened File, Tools and Help respectively in Designer 10.5.0. They are existing menu access keys and must be included in a future conflict inventory; the command registry alone does not enumerate them.

The expanded bindings and chords were not installed by those 0.1.4 tests. The subsequent 0.1.5 work is recorded below. The broader editor, selection, floating-window and UCI command matrix remains tracked in [issue #2](https://github.com/joshua-small/qsys-better-shortcuts/issues/2).

## Chords and expanded mappings (0.1.5)

On 2026-10-14, coordinated local Core and Better Shortcuts 0.1.5 builds were installed through Mod Manager transactions; Packing stayed 0.1.3 and every unrelated module pin stayed unchanged. No public release was published. The latest transaction changed only the Shortcuts DLL for the F2 field lookup correction. Tests used separate disposable schematic/UCI files. The initially unsaved user design was saved separately before testing and was not used as the mutation fixture.

| Physical-key check | Observed result |
| --- | --- |
| Shift+Alt+D / E / P | Each individual main panel hid and restored with the shared modifier family. |
| Shift+Alt+] | The two right-side main panels hid together and restored. |
| Ctrl+Shift+N then Ctrl+Shift+P | Created a second schematic page. |
| Ctrl+Shift+N then Ctrl+Shift+U | Created a UCI. |
| Ctrl+Alt+U then V / T | Focused the UCI Variables / Toolbox tab. |
| Ctrl+Shift+N then Ctrl+Shift+V | Added Var_1 through the native action. |
| Shift+Alt+F | Inserted a Flexbox. Initial test exposed an inactive-tab lookup defect; the corrected build inserted it with Toolbox never opened. |
| Ctrl+Alt+B then B, Enter | Opened the native button subtype chooser; Enter inserted a momentary control. |
| Alt+C then Alt+A | Displayed available completions, then expanded and focused Audio Components. |
| Ctrl+Alt+P | Checked the native Script Programmer Mode menu item; a second press restored the initial state. |
| Ctrl+I | Opened one Component Controls Info window with the native selection prompt. |
| Tools menu hints | Displayed Ctrl+Alt+P and Ctrl+I for those two actions. |

The installed preferences file matched its original SHA-256 after every installation (`ca14bfb4c4a98ce8d4f6c2ebefa447389da43844bc96515467c1e9f6365c447e`). No bulk default overwrite was applied to personal settings. The existing Preferences Ctrl+, assignment remains preserved.

F2 initially did nothing for a selected UCI control. The name-field lookup was corrected, built and installed. **Its final Designer retest remains pending:** the Windows automation helper subsequently failed with `foreground window did not report a process id`, including after refreshing window selection. New chord recorder capture/save, bulk overwrite, live chord-remap menu refresh and native context-menu labels remain covered by offline state/planner/presentation tests, not by a completed 0.1.5 UI run. Historical saved-remap tests above and the owner's single-key recorder acceptance remain separate evidence.

Other pending physical checks include left/all dock keys, the complete UCI layer/creation/context matrix, graphic-tool placement/undo, name-field focus and color-swatch focus. The implementation and proposed defaults are listed in [the keymap](DEFAULT-KEYMAP.md); untested adapters and remaining blank proposals stay tracked in [issue #4](https://github.com/joshua-small/qsys-better-shortcuts/issues/4). No live Core, DSP or hardware behavior was exercised.
