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

Without `-Suite`, the module only observes the selected document, focus, layer state, panel states (including `IsFloating`), command availability and effective bindings. It does not record typed text or inject input. `shortcut-evidence/state.json` refreshes during manual/Computer Use checks. `result.json` reports suite success, individual assertions and errors; absence of a result is not a pass. Receipts, settings, designs and vendor binaries stay local and out of Git.

The scoped physical checks and remaining limits are recorded in [Designer acceptance](../../docs/DESIGNER-VALIDATION-10.5.md). Compare hashes of personal preferences and verify the original Designer process remains running after testing; a lab marker alone is insufficient evidence of isolation.

## Context and layout mode

Pass `-Contexts` instead of `-Suite` to exercise the remaining context/layout matrix. The fixture registers synthetic commands only in the redirected test process, raises WPF events through actual Designer controls, manipulates native dock layouts, and creates/undoes synthetic schematic objects and a UCI. It checks shared prefixes, held keys, availability, focus/click/modal/page-change/timeout cancellation, swapped/mixed docks, sidebar focus, schematic F2, nested schematics, docked Monaco and floating Component Controls. Detached script windows are checked as an unsupported scope: even opted-in commands must remain inactive there.

A fixture context menu using the real native Copy command exercises the actual opening route and live JSON remap/Clear/Reset. It does not stand for every Designer-specific icon-menu template. `contexts-result.json` is the terminal pass/fail receipt; `progress.json` records completed assertions but is not a pass. This mode never exercises the physical shortcut recorder. Keep that acceptance open separately.
