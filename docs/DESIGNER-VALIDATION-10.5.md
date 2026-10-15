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

F2 initially did nothing for a selected UCI control. A correction using the native `_ControlName` and `tbCodeName` fields was built and installed. At the end of that run, its Designer retest was pending: the Windows automation helper failed with `foreground window did not report a process id`, including after refreshing window selection. Recorder, bulk-overwrite and live-refresh coverage at that point was limited to offline tests. The subsequent disposable run below supersedes that pending UCI F2 result and extends native component acceptance; historical saved-remap tests and the owner's single-key recorder acceptance remain separate evidence.

The implementation and proposed defaults are listed in [the keymap](DEFAULT-KEYMAP.md); untested adapters and remaining blank proposals stay tracked in [issue #4](https://github.com/joshua-small/qsys-better-shortcuts/issues/4). No live Core, DSP or hardware behavior was exercised.

## Disposable acceptance and review fixes (2026-10-15 UTC)

This run used a separate Designer 10.5.0 copy with native Roaming, Local, Documents and User profile folders redirected before startup. The runtime matched merged Core main `3ea01ac7bf66f2f80b636f4ceff4380dfb33ee02` (reviewed source head `d4ed9a1`). Better Shortcuts started at `9d3068d` and was rebuilt with the two review fixes below. No regular installation or public release was performed. The guarded [DesignerAcceptance fixture](../tests/DesignerAcceptance/README.md) and raw receipts are separate from the product; vendor binaries, preferences, captures and synthetic designs remain local.

Two defects were reproduced and corrected:

- **Review mod defaults omitted original single-key registrations.** It now includes a mod's `DefaultGesture`, as well as sequence defaults and mod-declared defaults on native actions. It still excludes ordinary Q-SYS defaults, so bulk adoption does not reset an unrelated native Copy remap.
- **Spatial/all-dock actions changed floating panels.** Actipro reports `State=Docked` for a tool inside a floating host. Filtering `IsFloating` explicitly keeps that panel out of spatial and all-dock actions. Native regression checks and physical right/all-dock checks passed with floating Properties and main-window panels together.

The final Release build completed with zero warnings/errors. The 45 offline catalog checks and 41 Windows WPF formatting/recorder-state/wheel checks passed. The actual disposable Designer passed **84 native integration assertions** through the fixture:

- Bulk review includes a legacy mod default, names the displaced owner, and excludes unmodified native defaults. Review Cancel and Preferences Cancel preserve saved settings; Apply stays staged until OK. Overwrite preserves the donor's unrelated alternative and an unrelated native remap, then persists to live JSON.
- The actual recorder accepts two WPF routed strokes, cancels on Escape and stages on Enter without closing Preferences. Preferences OK saves the chord to JSON and immediately refreshes the real View menu hint. This is component/routed-event acceptance, not physical recorder input.
- Floating Properties remains floating under right/all-dock toggles; a closed Properties panel stays closed.
- Each UCI button, knob, text, fader, meter and indicator action exposes the native subtype chooser; the first subtype inserts one object and Undo removes exactly that object. Flexbox insertion/Undo also passes. These checks do not enumerate every subtype.
- All seven library-category adapters expand and select the expected category without inserting a component.

| Physical input in the disposable Designer | Observed result |
| --- | --- |
| Left/all dock shortcuts; final right/all dock retests | Main panels hide/restore; floating Properties is unchanged by the corrected spatial/all actions. |
| F2 on a selected UCI button | Focuses `_ControlName` and selects the complete `Control_1` name. The prior UCI F2 blocker is resolved. |
| Text, Header, Group Box and Polygon graphic shortcuts | Native tools create the intended objects by drag or polygon vertices; Ctrl+Z removes each inserted object. |
| UCI layer hide/show and lock/unlock | Selected layer state changes; the locked layer prevents the button creation action. |
| UCI layer collapse/expand and Alt+Up/Down | Collapse/expand work; two layers reorder in both directions with the layer tree focused. |
| UCI Script/Layout and dock/Toolbox chords | Switches the active surface, including return from focused docked Monaco with explicit opt-in; local dock collapse and Toolbox restoration work. |
| Focused color swatch, then I | Opens its native palette; Escape dismisses it. The shortcut remains scoped to swatch focus. |
| Alt+2 and Video category chord | Restores/selects Schematic Pages and expands/selects Video Components respectively. |
| Live JSON remap to Ctrl+Alt+F11 then P | Dispatch changes without restart and hides/restores Design Elements. Escape, an invalid completion and expiry of the displayed ten-second wait leave the panel unchanged. |

Native text-field context menus retained their local Ctrl+C hint while the global Copy command was remapped. This verifies local editor preservation, not live refresh of a global context-menu label. The observed canvas context menu displayed icons rather than shortcut labels; Core's automated presentation tests remain the evidence for that general path.

The original Designer process was preserved. Personal HardLink, HardLinkNames and mod preferences remained byte-identical to their start-of-run hashes. Personal `Prefs.xml` changed during the session; compared with the preserved earlier XML baseline, the only changed entry was `assetRefreshToken`. Only the hash was captured at this run's start, so that comparison does not establish the exact within-run XML delta or its writer. No process-level writer trace was collected in this run, and no token value was published or restored. Native profile-redirection receipts and scoped process paths were checked independently of those personal-file hashes.

The owner explicitly chose automated recorder coverage and left **physical two-stroke recorder/Preferences save acceptance open**. Physical delivery into floating Component Controls was not reliably attributable through the input helper. Detached Actipro script windows remain outside Core's attached scopes. Schematic `tbCodeName` F2, swapped dock layouts, alternate keyboard layouts/AltGr and the wider editor/context matrix remain unaccepted. These limits stay tracked in [Core #4](https://github.com/joshua-small/qsys-mods-core/issues/4), [Shortcuts #2](https://github.com/joshua-small/qsys-better-shortcuts/issues/2) and [Shortcuts #4](https://github.com/joshua-small/qsys-better-shortcuts/issues/4). Public catalog dependencies and release work remain separate.
