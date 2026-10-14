# Designer shortcut acceptance — 10.5.0

## Installed build and scope

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

The proposed expanded bindings, chords, sidebar expansion commands and optional Command Palette remain design work. None were installed by these tests. The broader editor, selection, floating-window and UCI command matrix remains tracked in [issue #2](https://github.com/joshua-small/qsys-better-shortcuts/issues/2).
