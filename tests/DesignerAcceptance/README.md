# Designer 10.5 acceptance fixture

This fixture loads only in an explicitly prepared, profile-redirected Core acceptance copy. It is excluded from the product project and must never be packaged or installed in regular Designer. Use the adjacent Core repository's `tests/LiveSettings` preparation instructions first; copying Designer alone does not isolate native preferences.

Build with the configured .NET 10 SDK, an adjacent Core checkout, and local vendor references:

```text
dotnet build tests/DesignerAcceptance/Probe.csproj -c Release --artifacts-path <isolated-build-directory> -p:DesignerDir=<supported-Designer-directory>
```

On a Windows-hosted checkout, build through the configured WSL route and pass the resulting Windows paths to the native launcher:

```powershell
./tests/DesignerAcceptance/Start-Lab.ps1 -LabDirectory <prepared-copy> -BuildDirectory <build-directory>/bin -CoreBuildDirectory <Core-acceptance-build>/bin -Suite
```

The launcher verifies the exact Designer/ModuleCommon hashes and the profile marker, stops only the matching disposable Designer and Chromium processes, archives previous receipts, and loads the Core startup hook to redirect native profile folders. `-Fresh` archives the lab profile/settings first; it does not reset personal preferences. Dismiss any native startup/recovery dialog before inspecting results. Do not change privacy or authentication settings to run these tests.

With `-Suite`, the module registers two fixture commands and exercises the actual Preferences page: legacy defaults, conflict disclosure, preserved alternatives, bulk Cancel/Apply/OK, recorder routed events, JSON persistence and menu refresh. It also checks floating/closed panels, UCI subtype creation and undo, and library-category adapters. The suite deliberately changes the disposable layout and creates an unsaved synthetic UCI. It leaves two test remaps for follow-on checks and saves the original lab settings alongside the receipt. These are native component/routed-event checks, not physical keyboard tests.

With none of `-Suite`, `-Contexts`, `-Physical`, `-Editors` or `-Menus`, the module only observes the selected document, focus, layer state, panel states (including `IsFloating`), command availability and effective bindings. It does not record typed text or inject input. `shortcut-evidence/state.json` refreshes during manual/Computer Use checks. `result.json` reports `-Suite` success, individual assertions and errors; absence of a result is not a pass. Receipts, settings, designs and vendor binaries stay local and out of Git.

The scoped physical checks and remaining limits are recorded in [Designer acceptance](../../docs/DESIGNER-VALIDATION-10.5.md). Compare hashes of personal preferences and preserve any original Designer process that was running before testing; a lab marker alone is insufficient evidence of isolation.

## Context and layout mode

Pass `-Contexts` instead of `-Suite` to exercise the remaining context/layout matrix. The fixture registers synthetic commands only in the redirected test process, raises WPF events through actual Designer controls, manipulates native dock layouts, creates/undoes synthetic schematic objects, and adds an unsaved synthetic UCI. It checks shared prefixes, held keys, availability, focus/click/modal/page-change/timeout cancellation, swapped/mixed docks, sidebar focus, schematic F2, nested schematics, docked Monaco and floating Component Controls. Detached script windows are checked as an unsupported scope: even opted-in commands must remain inactive there.

A fixture context menu using the real native Copy command exercises the actual opening route and live JSON remap/Clear/Reset. It does not stand for every Designer-specific icon-menu template. The floating suite also deliberately omits Escape key-down, verifies that its release cancels the chord, then verifies a new chord still runs once. `contexts-result.json` is the terminal pass/fail receipt; `progress.json` records completed assertions but is not a pass. This mode never exercises the physical shortcut recorder; use the separate physical evidence below for that acceptance.

## Physical input attribution

Use `-Physical -Fresh` instead of either automated suite. This explicitly creates a synthetic Gain named PhysicalGain and opens its native floating Component Controls window. It registers four disposable commands through Core, with no alternate input engine or synthetic keyboard events:

| Input | Main canvas | Floating Component Controls / text input |
| --- | --- | --- |
| Ctrl+Alt+F8 | ordinary-single increments once | excluded |
| Ctrl+Alt+F9, release, G | ordinary-chord increments once | excluded |
| Ctrl+Alt+F10 | opt-single increments once | increments once |
| Ctrl+Alt+F11, release, G | opt-chord increments once | increments once |

`physical-ready.json` confirms setup only, not acceptance. Check the effective bindings in `physical-counts.json` before entering keys. `physical-events.jsonl` records only F8–F11, G and Escape, with modifiers, receiving window, focus type, handled state and execution counts; it does not capture arbitrary typed content. The passive observers attach to newly opened windows so close/reopen can be checked. Keep every chord within ten seconds. Confirm each expected count delta and that the receiving window was the intended floating host, then repeat after closing/reopening Component Controls. A helper that activates the main window does not establish floating delivery even if the command executes. Human keyboard entry is needed in that case.

The observer also writes `native-popups.json` while an actual schematic `PopupToolBar` is open, recording button command identity and tooltip properties. Open it by right-clicking blank canvas beside the selected Gain. To check live hints, preserve the lab settings, remap native Copy to a free chord in the isolated JSON, then Clear and Reset it while the popup stays open; reopen and compare. Restore the lab settings afterwards. A property receipt does not establish tooltip-hover rendering or button execution.

No test binding is written into the personal profile. Do not run another acceptance mode against the same live process while a person is entering test keys. Avoid an active input helper during the human Escape check: a helper's emergency-stop shortcut can intercept key-down. Attribute missing events from the receipts, not assumptions about Designer. Preserve receipts before relaunching.

## Editor and native function-key input

Use `-Editors -Fresh` for a separate synthetic UCI with one button and its docked script editor. This mode is mutually exclusive with the other acceptance modes. It registers the same four test bindings above and sets the synthetic Core name to empty before opening the editor. `editors-ready.json` must report `ready: true` and an empty `coreName` before testing F5. Setup uses native APIs; it is not keyboard acceptance.

Click the actual editor and verify `state.json` reports the intended focused type and `textInput: true`. Type a harmless Lua comment, check native Undo/Redo and Find, and check ordinary-command exclusion versus explicit opt-in. For an excluded chord, its completion should remain normal text. For an opted-in chord, the completion must execute once without insertion; Escape must cancel it. Keep strokes within ten seconds. Use human input if the helper cannot preserve focus or meet that interval, and identify the input source in the report. Repeat representative text checks in the native Control Name property field.

The [native Q-SYS shortcuts](https://help.qsys.com/Content/Q-Sys_Designer/Q-Sys_Designer_Shortcuts.htm) are F5 Save to Core & Run, F6 Emulate and F7 Disconnect. Test F5 only with this unnamed synthetic Core: Designer must show “Give your core a name!” and stop before deployment. Never select or connect to hardware. Dismiss that message, press F6 to enter local emulation, then F7 to return to Design mode. Compare `hostState` and `nativeRunCommands` in `state.json`; the original native F5/F6/F7 gestures must remain intact.

Only in `-Editors` mode, the passive whitelist adds F5/F6/F7 and logs native `Connect`, `Emulate` and `Disconnect` command entry. Native `Connect` is the routed name of Designer's `Deploy` command, not a class named Connect. A command-entry receipt alone does not prove completion: also observe the native validation message or the expected host-state transition. Preserve receipts before relaunching. This checks the selected docked editor and property field, not every browser surface or unsupported detached script window.

## Legacy editor variation

For the Legacy editor use `-Editors -LegacyEditor -Fresh`. The fixture changes only the redirected `ScriptEditor` preference, waits for native readback, and opens the same synthetic UCI. It focuses the actual Actipro `EditorView` and runs four native routed-event checks for text recognition, ordinary exclusion, explicit opt-in and Escape cancellation. `legacy-routing.json` is their separate pass receipt, explicitly marked `physicalInputTested: false`. This regression requires the Core Legacy text-input guard fix.

Physical input begins **after** the `editor-ready` marker in `physical-events.jsonl`; G/Escape events before that marker belong to the synthetic regression. Do not count those as physical acceptance. `editors-ready.json` remains setup evidence only. Repeat the editing/chord/native function-key procedure above, attributing each external input and preserving the terminal counters.

## Native icon-menu variation

Use `-Menus -Fresh` to prepare three synthetic Gains and three UCI buttons. It leaves the UCI selected; switch to Page 1 for the schematic variant. `menus-ready.json` confirms setup only. The passive observer writes `menus-state.json`, timestamped `menus-history.jsonl` and native `menu-clicks.jsonl`, including command IDs, tooltip content, enabled/visible states and model positions/lock state. No keyboard or mouse events are synthesized by this mode.

Open the native icon popup and preserve its defaults. In the **lab** settings only, map its 28 registered commands to distinct free chords, then Clear and Reset while the popup stays open. Compare hints, command IDs and enabled/visible states after every change; reopen and verify one actual hovered tooltip. Exercise representative Align/Duplicate/Undo on schematic and Pack/page-center/Lock/Unlock on UCI, comparing model receipts. The noncommand Unlock button should retain its native tooltip.

In local emulation, right-click both a control and a component body to exercise the different Copy/Paste/Copy All/Paste All availability. A hint update does not establish that native clipboard execution works. On the tested 10.5.0 build, Copy All leaves Paste All disabled, including with owner mouse input. See the [mod-free clipboard diagnostic](../NativeClipboardRepro/README.md).

Optional `-Menus -MenuBaseline -Fresh` performs a synthetic native CopyControlSet round trip, disposes Core's binding engine, and repeats it. `native-clipboard-result.json` labels that comparison accurately: other mod services remain loaded, so it is not a mod-free run. This option replaces the clipboard. Do not edit live settings after disposing the engine, since settings reapplication can attach routing again. Relaunch normally to resume menu testing. The launcher rejects `-MenuBaseline` without `-Menus`, `-LegacyEditor` without `-Editors`, and all combinations of primary modes before touching the lab.

## Physical recorder/save acceptance

Use the isolated Designer in observer mode (none of `-Suite`, `-Contexts`, `-Physical`, `-Editors` or `-Menus`) and record the Designer version and exact Core/Shortcuts source or assembly hashes. A person must enter the recorder strokes with the keyboard while Preferences retains focus; raising routed events does not establish this path. The current input helper loses recorder focus, so it cannot substitute for that input.

1. Open Preferences > Better Shortcuts, select Actual Size, and open Change shortcut. Record a conflict-free two-stroke chord, for example Ctrl+Alt+F12, release the keys, then G. Confirm both strokes appear in order. If a conflict is reported, cancel and choose a free chord for this recorder check.
2. Press Enter to accept the recording. Preferences must stay open with the staged assignment. Click Preferences OK to save.
3. Verify the table on reopening Preferences, the View > Actual Size hint and the isolated settings JSON all show the saved sequence. Cancel the reopened Preferences dialog without making more changes.
4. Zoom the synthetic canvas to 110%, focus the canvas, and physically press the recorded chord. It must return to 100% without restarting Designer. Record the observed result and preserve the personal preference hashes.

The separate [physical retest PR #5](https://github.com/joshua-small/qsys-better-shortcuts/pull/5) records owner-assisted capture/save, saved JSON, immediate menu refresh and subsequent helper-driven chord execution on Core `3ea01ac` / Shortcuts `21b87a9`. The [evidence reconciliation](../../docs/DESIGNER-VALIDATION-10.5.md#physical-recorder-evidence-reconciliation-2026-10-15) accepts the physical recorder/save item in [Core #4](https://github.com/joshua-small/qsys-mods-core/issues/4). It does not claim a separately observed Preferences reopening or restart; step 3 above is a useful repeat-test check, while the accepted retest establishes persistence through the saved JSON. No recorder implementation changed in the later context/layout fixes. The broader physical-input acceptance remains separate.
