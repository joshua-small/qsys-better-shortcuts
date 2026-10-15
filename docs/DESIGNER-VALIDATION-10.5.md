# Designer shortcut acceptance — 10.5.0

Physical two-stroke recorder/Preferences-save, populated floating Component Controls, docked UCI Monaco and native Control Name property-field acceptance are complete for the tested builds. The latest [native icon menus and Legacy editor follow-up](#native-icon-menus-and-legacy-editor-2026-10-15) adds icon presentation/click coverage and a Core text-input fix; native component Paste All remains blocked by the reproduced host clipboard failure. Both docked editors preserve F5/F6/F7. Earlier evidence remains below as version-specific history.

## Native icon menus and Legacy editor, 2026-10-15

The profile-redirected Designer 10.5.0 lab exercised native menus on Core `e7f085c` / Shortcuts `b2e1ba6`. Legacy testing then found and fixed a Core defect in `8e8817f`: ordinary mod commands could run while the docked Actipro `EditorView` had focus because its `SyntaxEditor` ancestor was not recognized as text input. The corrected shared guard recognizes that editor and derived types without a vendor assembly dependency. It does not attach detached script windows or change saved bindings. Better Shortcuts product source is unchanged.

### Native icon-menu results

The schematic, UCI edit and emulating component/control menus use the same native `QSC.QSys.windows.PopupToolBar` template with context-dependent visibility and availability. Its 29 buttons comprise 28 registered native commands and a noncommand Unlock button. The UCI runtime `PopupTool` is a different surface and is not covered by these editing-menu results.

| Check | Observed result |
| --- | --- |
| Schematic and UCI live hints | All 28 command tooltips followed distinct chord remaps, Clear and Reset while the native popup stayed open. Native command IDs and enabled/visible states stayed unchanged. Clear removed only shortcut suffixes; Reset restored the exact originals. Hidden/disabled buttons were checked as properties, not claimed as visible interactions. Unlock retained its native tooltip. |
| Actual hover | The owner hovered Align Left and confirmed `Horizontal Align Left (Ctrl+Alt+F12 then H)`. The passive open-ToolTip receipt also contains that text. Reopening retained the effective hint. |
| Schematic clicks | Align Left moved all three Gains to the same X while retaining their Y positions; one native Undo restored them. Duplicate followed native placement behavior, committed three additional Gains, and one Undo restored the original count. |
| UCI clicks | Native Pack Left stacked three 36×16 buttons vertically with aligned left edges. Horizontal Center in Page set all X positions to 622 on the 1280-wide page. Lock locked all three; the native Unlock button then unlocked only the selected target. |
| Emulating variants | Right-clicking a Gain control versus its component body produced the native Copy/Paste/Copy All/Paste All availability differences. Both whole-component hints followed live remapping. Copy All was enabled on a component, but Paste All stayed disabled after copying, including when the owner repeated the operation with the mouse. |

The initial UCI fixture placed its three buttons at the same location; the click/packing results above are attributed to those recorded starting positions. The final fixture sets distinct native model positions and commits each operation; the corrected positions were verified in the subsequent baseline run. Synthetic fixture setup is not physical input acceptance.

### Native clipboard blocker

The binding-engine-enabled and binding-engine-disposed comparisons both report the custom clipboard format present but a null native typed read. Other mod services remain loaded in that comparison. A separate STA process using the installed Q-SYS assemblies **without Core, Better Shortcuts or Qmm assemblies** reproduces the failure on .NET 10.0.13: raw clipboard data is `System.NotSupportedException: BinaryFormatter is not supported in clipboard or drag-and-drop operations.` Plain-text round-trip succeeds. An earlier sandbox-denied run is excluded from this evidence.

The [guarded reproduction](../tests/NativeClipboardRepro/README.md) supplies synthetic data only. This isolates the tested native serialization path from shortcut routing; it does not prove every native clipboard operation fails. **Successful component Copy All/Paste All remains unaccepted.** No vendor binary, runtime policy or unsafe compatibility workaround was changed. Keep this blocker on Core #4 pending a vendor fix or separately scoped remedy and retest.

### Corrected docked Legacy editor

Before the fix, physical Ctrl+Alt+F8 ran an ordinary mod command once with `EditorView` focused and `textInput: false`. The focused WPF ancestry regression also failed. After the fix, all **201 Core WPF controls checks** pass, including seven new recognition/ancestry checks. Four separate native routed-event assertions pass for recognition, ordinary single/chord exclusion, explicit single/chord opt-ins and Escape cancellation. They are labeled `physicalInputTested: false`; events before the `editor-ready` marker belong to that automated regression.

External input after that marker establishes the physical result:

- Windows helper: harmless Lua comment entry, native Ctrl+Z Undo, Ctrl+Y Redo and Ctrl+F Find work. Ordinary Ctrl+Alt+F8 stays unhandled by Core and inactive; opted-in Ctrl+Alt+F10 executes once.
- Owner: Ctrl+Alt+F9 then G stays unhandled/inactive; Ctrl+Alt+F11 then G executes once. Ctrl+Alt+F11 then Escape then G adds no execution. Escape key-down is absent and release is present/handled; the existing release fallback cancels correctly. The owner reports that Escape stopped Computer Use. No further GUI input was issued.
- Windows helper, before the owner check: F5 displays “Give your core a name!” for the unnamed synthetic Core, without deployment. F6 enters local Emulate (`connected=true`, `emulator=true`); F7 returns to Design with both false. Receipts identify EditorView focus and `textInput: true`; effective bindings and native gesture collections remain F5/F6/F7. These results use key receipts and visible/recorded outcomes, not separate native command-entry receipts.

Final physical counters: ordinary single **0**, ordinary chord **0**, opted-in single **1**, opted-in chord **1**. All owner sequences completed within ten seconds. The tested corrected Core runtime SHA-256 is `77B96DE005281901301E2A529B9C652E40CE3B94A2F7023B8C310D8741B8F3E2`; the physical fixture is `B54589A3C15F4C518DB043BF1E20ABE8D1CD84C1F12503DEBF065A01D5A48D98`. Final fixture-only documentation/comment changes do not alter this attribution. Final fixture and standalone diagnostic Release builds pass with zero warnings/errors, all 45 catalog checks pass, and launcher parsing plus 12 invalid mode combinations pass before filesystem side effects.

After preserving receipts and stopping only the disposable Designer and its children, native Prefs.xml, both HardLink files and mod preferences matched their same-start SHA-256 hashes byte-for-byte. Custom bindings, including Ctrl+,, are preserved without restoration. No regular installation, release, signed catalog or hardware deployment changed. The native clipboard blocker remains open; detached script hosts remain unsupported. Other browser/editor hosts, runtime UCI popups, alternate layouts/AltGr and OS-reserved combinations are not accepted by this run. The owner uses US layout only and requests no reserved shortcuts. Existing feature proposals stay separate.

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

F2 initially did nothing for a selected UCI control. Static inspection identified the native `_ControlName` and `tbCodeName` fields, and that correction was built and installed. At the end of that run, its Designer retest was pending: the Windows automation helper failed with `foreground window did not report a process id`, including after refreshing window selection. Recorder, bulk-overwrite and live-refresh coverage at that point was limited to offline tests. The subsequent disposable run below supersedes that pending UCI F2 result and extends native component acceptance; historical saved-remap tests and the owner's single-key recorder acceptance remain separate evidence.

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

At the end of that run, physical two-stroke recorder/Preferences save, schematic Code Name F2 and swapped layouts were still pending. The physical retest below resolves recorder/save and positive F2 checks; the separate automated follow-up covers swapped layouts and the later F2 deselection regression. Physical floating-window delivery, alternate keyboard layouts/AltGr and the wider physical editor/context matrix remain unaccepted, and detached Actipro script windows remain unsupported. These residual limits stay tracked in [Core #4](https://github.com/joshua-small/qsys-mods-core/issues/4), [Shortcuts #2](https://github.com/joshua-small/qsys-better-shortcuts/issues/2) and [Shortcuts #4](https://github.com/joshua-small/qsys-better-shortcuts/issues/4).

## F2, physical recorder and bulk-overwrite retest (2026-10-15 UTC)

The follow-up used the same isolated Designer 10.5.0 copy, with the final reviewed Shortcuts build (`1c0e533`, merged as `21b87a9`) and Core `3ea01ac`. No implementation, regular installation or public release changed. The UCI F2 check also passed in the installed 0.1.5 build on a disposable fixture.

| Check | Input and observed result |
| --- | --- |
| F2 on a selected UCI control | The input helper pressed F2; Properties focused Control Name and selected the complete `Control_1` value. |
| F2 on a schematic component | The input helper pressed F2; Properties focused `tbCodeName` and selected the complete 13-character `Status Core-1` name. The native observer independently recorded focus and selection length. |
| Record and save a chord | The owner double-clicked Actual Size, physically entered Ctrl+Alt+F12 then G, confirmed with Enter and saved Preferences. The saved JSON contains `Ctrl+Alt+F12 > G`. This resolves the earlier physical recorder/save gap, with owner-assisted input rather than automated recorder input. |
| Bulk overwrite | In the disposable profile, Copy deliberately held Shift+Alt+D and a separate F24 then G alternative; Design Elements was remapped away from its default. The owner selected Review mod defaults, Apply all and Preferences OK. Saved JSON restored Design Elements to Shift+Alt+D, removed only that conflicting alternative from Copy and retained F24 then G. |
| Preserve unrelated assignments | Bulk overwrite retained the independent Preferences Ctrl+, assignment and the newly recorded Actual Size chord. No personal profile was used for this overwrite test. |
| Live menu hints | Tools immediately displayed temporary Ctrl+Alt+F10 then P/I remaps for Programmer Mode and Component Controls Info. After bulk overwrite it displayed their restored Ctrl+Alt+P and Ctrl+I defaults. View > Actual Size displayed Ctrl+Alt+F12 then G immediately after saving. No restart was needed. |
| Execute the recorded chord | After Zoom In changed the canvas to 110%, the input helper pressed Ctrl+Alt+F12 then G. Actual Size returned it to 100%. The temporary Programmer Mode chord also executed successfully. |

The input helper deactivates the Preferences recorder/review overlay when delivering keys or wheel input, so the owner supplied those specific recorder and bulk-apply actions. Saved settings and subsequent menu/dispatch results were independently checked. This does not turn the earlier 84 routed-event assertions into physical-input evidence or expand acceptance to other contexts.

The synthetic test design was saved separately and the isolated Designer closed. Personal mod preferences remained byte-identical to the pre-test copy (SHA-256 `ca14bfb4c4a98ce8d4f6c2ebefa447389da43844bc96515467c1e9f6365c447e`), including the custom Ctrl+, binding. The previously preserved user design also retained its original hash. Raw settings copies, focus receipts and synthetic designs remain local. The broader outstanding contexts above remain open in issue #2.

## Automated context and layout acceptance (2026-10-14)

The next run used a new profile-redirected Designer 10.5.0 copy, merged Core `23336c5` and Shortcuts `21b87a9` plus the fixes described here. Dates in this section use America/Los_Angeles; local receipts carry UTC timestamps. The final `-Contexts` fixture passed **92 native assertions**, with a zero-warning Release build, **181 Core WPF controls checks**, **45 catalog checks** and **41 Shortcuts WPF checks**. This is automated application/routed-event coverage, not physical keyboard or recorder acceptance.

| Context/layout case | Automated result |
| --- | --- |
| Root schematic and shared prefixes | Single keys and both completions dispatch once. Held keys/prefixes do not repeat. Escape, invalid completion, click, focus change, unavailable targets, actual ten-second dispatcher timeout and modal Preferences cancel safely. |
| Native text and docked Monaco | Ordinary mod singles/chord prefixes remain unclaimed. Explicit text opt-ins execute once; native text-field content stays unchanged. |
| Nested schematic | Both forms dispatch once; changing pages cancels a pending chord; one Undo removes the synthetic container. |
| Floating Component Controls | Only supported opted-in mod routes dispatch. Closing/reopening the native window does not duplicate execution. |
| Detached UCI script | Its separate Actipro host remains outside Core's supported routes; even opted-in test keys do not dispatch. This confirms the exclusion, not new detached-window support. |
| Swapped and mixed docks | Inventory moved right and Properties/Library left. Spatial actions followed actual sides, restored both panels, and all-dock actions collapsed only remaining expanded panels before restoring all. |
| Sidebar focus | All six Design Elements sections expand, receive focus and stay expanded on repeat. Inspector focuses native `InspectorBorder`; component search focuses native `Spec` TextBox. |
| Schematic and UCI F2 | Schematic `tbCodeName` selects the complete name and reopens an auto-hidden Properties panel. Empty selection disables F2. UCI `_ControlName` still focuses/selects correctly. |
| Context-menu hints | A fixture menu on the real canvas, using native Copy and the actual context-opening route, follows live JSON remap, Clear and Reset while open. Command identity is preserved; closing restores original presentation. This does not cover every native icon-menu template. |

The run exposed two defects. Core refreshed hints only when context menus opened, leaving an already-open menu stale after a live settings edit; it now refreshes tracked open menus on apply while retaining original close-time presentation. Shortcuts considered a cached name field eligible after its native parent editor collapsed on deselection; F2 now rejects collapsed editor ancestors without rejecting an auto-hidden Properties dock. Both have regression coverage in the final native run; the Core hint case also has an offline WPF regression that failed before the fix.

The guarded fixture is in [tests/DesignerAcceptance](../tests/DesignerAcceptance/README.md). Test-only commands, startup hooks, raw receipts, vendor binaries and preference copies are excluded from product packages. This automated run left physical recorder/save acceptance open by owner instruction; the evidence reconciliation below separately resolves that item. Physical floating-window delivery, alternate layouts/AltGr, OS-reserved keys and browser-hosted input beyond the tested WPF route remain separate. No release, catalog change or regular Designer installation was performed by this follow-up.

The disposable instance was stopped after the final run. Personal `Prefs.xml`, HardLink, HardLinkNames and mod preferences matched their start-of-run hashes exactly; a same-start XML copy also confirmed no changed preference entries. Raw before/after copies and tested assembly hashes remain local.

## Physical recorder evidence reconciliation (2026-10-15)

Review of the original retest session and preserved receipts is sufficient to close the physical two-stroke recorder/Preferences-save item in [Core #4](https://github.com/joshua-small/qsys-mods-core/issues/4). The owner confirmed completing the requested Actual Size recording (Ctrl+Alt+F12, then G, Enter), bulk review and Preferences OK. The preserved post-save JSON independently contains `Ctrl+Alt+F12 > G`; the session records the immediate View menu hint and helper-driven execution returning zoom from 110% to 100%. Recording/save input came from the owner; subsequent chord execution came through the Windows input helper. These are not the earlier synthetic WPF recorder events.

The same saved JSON confirms Design Elements restored to Shift+Alt+D, Copy retaining its unrelated F24 then G alternative, Preferences retaining Ctrl+, and the Tools defaults Ctrl+Alt+P / Ctrl+I. The native F2 receipt records `tbCodeName` with a 13-character selection. Contemporaneous hash comparisons confirm preservation of personal **mod** preferences and the saved original design. They do not establish unchanged native `Prefs.xml` for that retest or resolve the earlier `assetRefreshToken` attribution.

This accepts the observed path on Designer 10.5.0, Core `3ea01ac` and Shortcuts `21b87a9` (reviewed head `1c0e533`). Source comparison through Core PR #8 and Shortcuts PR #6 confirms the recorder, Preferences-save and chord-dispatch implementations are unchanged; the later product fixes concern open context-menu hints and inactive F2 editors. The later 92-assertion context/layout run remains separate automated evidence. No fresh physical run, separately observed Preferences reopening, application-restart persistence, alternate-layout coverage or physical floating-window delivery is claimed by this reconciliation. Those wider limits remain open; the whole Core issue is not closed.

## Floating Component Controls and native icon toolbar (2026-10-15)

This follow-up used the disposable, profile-redirected Designer 10.5.0 copy. Better Shortcuts product code remained at `21a186157147abe1a8472489be23e0263e0f77ea`; only its acceptance fixture and documentation changed. Core started at `59fb2eb`; the final integrated fixes are [Core `025786b`](https://github.com/joshua-small/qsys-mods-core/commit/025786b). Physical floating checks were performed before the native tooltip addition; the final integrated build passed the automated run and native popup checks below. Exact tested assembly hashes and raw receipts remain local.

The `-Physical` fixture creates an unsaved Gain named PhysicalGain and passively logs a small whitelist of test keys, receiving windows, focus types and command counts. It registers test commands through Core without synthesizing key events. The owner supplied keyboard input because the input helper could activate the main window instead of the floating host. Each accepted run was attributed to the actual `Gain (PhysicalGain) Controls` Actipro floating host containing `QSC.QSys.windows.ComponentInfo`, with its TextBox focused, not inferred from successful execution alone.

| Check | Evidence and result |
| --- | --- |
| Ordinary-command exclusion | In populated floating Component Controls, physical Ctrl+Alt+F8 and Ctrl+Alt+F9 then G did not dispatch the ordinary test commands. |
| Explicit text opt-in | Physical Ctrl+Alt+F10 and Ctrl+Alt+F11 then G dispatched their opted-in commands once per valid input. |
| Close/reopen | The owner closed only Component Controls, reopened it with Ctrl+I from the selected Gain and repeated both opted-in inputs. On the corrected build the final counts were exactly two singles and two chords, with no duplicate dispatch. |
| Escape cancellation | One initial sequence contained Escape release but no observed Escape key-down, leaving the chord armed. Core now cancels on release as a fallback. The owner's corrected-build Escape retest dispatched nothing; that retest contained both down/up, so it does not alone prove the missing-down path. |
| Missing Escape-down regression | Offline WPF tests failed before the fix. Both the final WPF suite and native floating route deliberately omit Escape down, verify release cancellation and verify a fresh valid chord still executes once. |
| Actual native icon toolbar | Right-clicking blank schematic canvas beside the selected Gain opened Designer's `PopupToolBar`. Its native Copy tooltip was originally just `Copy`. The fix showed `Copy (Ctrl+C or Ctrl+Insert)`, then `Copy (Ctrl+Alt+F12 then G)` after live JSON remapping, `Copy` after Clear, and the original defaults after Reset and reopen. |

The missing Escape key-down is not attributed to Actipro or Designer: an external input helper's emergency-stop interception is also possible. The fix is a defensive cancellation fallback. The popup check observed the actual native tooltip property and command identity; it does not claim a separate hover-rendering check or button execution. Core preserves rich tooltip objects, native commands and original close-time presentation, with offline regression coverage for remap/Clear/Reset, reopen and disposal.

The final Release builds had zero warnings/errors. **194 Core WPF checks** and **95 native automated context/layout assertions** passed. The latter retain the earlier 92 cases and add three floating Escape-release assertions. Automated routing, human input and native property observations remain distinct evidence layers.

The owner confirmed **US layout only and no reserved shortcuts**. Alternate layouts/AltGr and OS-reserved combinations are therefore outside this acceptance scope and remain untested, rather than blocking this user's floating-window result. Detached Actipro script windows remain unsupported. Broader physical browser/editor contexts, native F5/F6/F7 preservation and additional icon templates remain tracked in [Core #4](https://github.com/joshua-small/qsys-mods-core/issues/4) and [Shortcuts #2](https://github.com/joshua-small/qsys-better-shortcuts/issues/2). Do not repeat the already accepted recorder/save test to close those different cases.

After the final run, the disposable Designer and its child Chromium processes were stopped. Personal `Prefs.xml`, HardLink, HardLinkNames and mod preferences matched their same-start SHA-256 values byte-for-byte, including the custom Ctrl+, binding. This does not identify the writer of the earlier historical `assetRefreshToken` change. No personal settings were restored or overwritten; no regular installation, catalog change or public release occurred.

## Physical editor and native function-key acceptance (2026-10-15)

The follow-up used profile-redirected Designer 10.5.0 with unchanged product code from Core merge `41d8c98dc20d63529122189eb791d83437311921` and Shortcuts merge `dc6e64e6b81d542dd9681b1a847d1c12cd28414c`. The new [fixture `-Editors` mode](../tests/DesignerAcceptance/README.md#editor-and-native-function-key-input) creates only an unsaved synthetic UCI/button, opens its docked script and leaves the synthetic Core unnamed. Its native-API setup is separate from keyboard acceptance. The final fixture build passed with zero warnings/errors; no product implementation changed.

| Physical input path | Observed result |
| --- | --- |
| Docked UCI Monaco editing, Windows input helper | A harmless Lua comment appeared. Ctrl+Z removed it, Ctrl+Y restored it, and Ctrl+F opened one native Monaco Find widget with the expected match. |
| Monaco single shortcuts, helper | Ordinary Ctrl+Alt+F8 remained unhandled and executed nothing; explicitly opted-in Ctrl+Alt+F10 executed once. Receipts identify focused `CefSharp.Wpf.ChromiumWebBrowser` and `textInput=true`. |
| Monaco chords, owner keyboard | Two valid Ctrl+Alt+F11 then G inputs each executed exactly once. Ctrl+Alt+F9 then G stayed excluded, leaving G as text. Two armed-then-Escape sequences cancelled, and subsequent G reached the editor without dispatch. One cancellation receipt contains only Escape release, the other both down/up; neither produced a command. No cause is assigned to the missing key-down. |
| Native F5, helper with Monaco focused | Designer's native `Connect` command entered once and displayed “Give your core a name!” The unnamed synthetic Core stopped the native Deploy command before deployment. This validates entry routing and the guard, not a hardware deployment. |
| Native F6/F7, helper with Monaco focused | F6 entered native `Emulate` once and produced `mode=Emulate`, `connected=true`, `emulator=true`; F7 returned to Design with both flags false. F7 down/up reached the focused browser unhandled by Core; no separate `Disconnect` command-entry receipt was captured. The visible transition and host-state receipt establish the result. Native gesture collections and effective bindings stayed F5/F6/F7. |
| Native Control Name property, helper | F2 selected all nine characters of `Control_1` in `_ControlName` (`QSC.QSys.windows.Editors.ValidatingTextBox`). Typing, Ctrl+A and Enter-to-commit worked. After committing `PropertyProbe`, Ctrl+Z restored `Control_1`. Ordinary Ctrl+Alt+F8 stayed excluded; opted-in Ctrl+Alt+F10 executed once. |
| Property chords, owner keyboard | With that validating textbox focused, Ctrl+Alt+F9 then G inserted one G without dispatch. Ctrl+Alt+F11 then G executed once and inserted no second G. The observed field was `Control_1g`; the live model still showed `Control_1` until applying the edit. |

The [official Designer shortcuts](https://help.qsys.com/Content/Q-Sys_Designer/Q-Sys_Designer_Shortcuts.htm) identify F5 as Save to Core & Run, F6 as Emulate and F7 as Disconnect from Core. Native inspection of this exact Designer binary explains the property Undo check: `ValidatingTextBox` binds WPF Undo to the document manager, not a separate draft-text undo history. Ctrl+Z did not erase an uncommitted draft; committing the rename before Undo established the native behavior. This is observed implementation evidence, not a new public API contract.

The helper preserved editor focus but exceeded the ten-second interval in one attempted chord; that completion correctly became ordinary text. The owner supplied the timing-sensitive checks. Final passive counters were **ordinary single 0, ordinary chord 0, opted-in single 2, opted-in chord 3** across Monaco and the property field. Each dispatch is attributed to the intended focused editor. A physical Escape used in the test also stopped Computer Use; work resumed only after the owner clarified and authorized continuation.

After stopping only the disposable Designer and its child Chromium processes, all four personal files matched their same-start SHA-256 hashes: native `Prefs.xml`, `HardLink.json`, `HardLinkNames.json` and mod `preferences.json`. The custom Ctrl+, binding and all other saved bytes are unchanged. No personal file was restored or overwritten. Exact tested assembly hashes, focus/event receipts and before/after copies remain local; no regular installation, release or catalog update occurred.

This closes the scoped docked UCI Monaco/property-field and native function-key acceptance in [Core #4](https://github.com/joshua-small/qsys-mods-core/issues/4). It does not claim every browser/editor surface, detached Actipro script support, additional icon-menu templates, a live Core or hardware behavior. The owner uses US layout only and requests no OS-reserved shortcuts. Those limits and separate feature proposals remain explicit; the accepted recorder/save and floating checks need no repeat.
