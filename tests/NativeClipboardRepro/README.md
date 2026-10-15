# Native component-control clipboard reproduction

Manual Windows desktop diagnostic for the hash-guarded Designer 10.5.0 build. It loads the installed Q-SYS assemblies in a separate .NET 10 STA process, creates a synthetic `CopyControlSet` (`gain`, `mute=0`), and calls the native clipboard wrapper. It does not start Designer, load Mods Core/Better Shortcuts, read preferences or connect to hardware. **It replaces the Windows clipboard contents.**

Build with `dotnet build tests/NativeClipboardRepro/ClipboardRepro.csproj -c Release --artifacts-path artifacts/clipboard`, then run the resulting DLL on Windows with the installed Designer directory as its sole argument. Use a process with normal desktop clipboard access. A sandbox access-denied exception is not evidence of a native serialization failure. The BinaryFormatter project setting matches Designer's runtime configuration; it does not repair the clipboard path or recommend enabling an unsafe formatter in production.

Observed on .NET 10.0.13: the custom format is present, the native typed read returns null, and the raw value is `System.NotSupportedException: BinaryFormatter is not supported in clipboard or drag-and-drop operations.` A plain-text round trip succeeds and `modsLoaded` is false. This explains the tested disabled native Paste All path; it is not a successful copy/paste acceptance result. No vendor binary or workaround is supplied.

## Native Paste All follow-up

The Designer-side reproduction uses two Gain components of the same type in a disposable design:

1. Enter local Emulate and give the Gains different Mute values.
2. Right-click the source component body and choose **Copy All from Component**.
3. Right-click the destination component body and inspect **Paste All to Component**.

Expected: Paste All becomes available and transfers the applicable control values. Observed in the mod-enabled lab, including the owner's mouse input: Paste All stays disabled. The separate diagnostic above establishes the serialization failure without mod assemblies; a full unmodified Designer GUI reproduction has **not** been recorded. Before filing a vendor support case, repeat these steps in an isolated unmodified Designer instance and include both results, the exact Designer version/hash, the runtime version and the diagnostic output. Do not attach personal preferences, designs or vendor binaries.

This failure is consistent with [Microsoft's .NET 10 clipboard migration guidance](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/migration/clipboard-dataobject-net10): custom objects requiring BinaryFormatter no longer round-trip through the legacy APIs, and a legacy read can return a `NotSupportedException` object. The recommended migration uses JSON and type-safe reads. Ask Q-SYS for a defect reference and a supported fix; no support report has been sent by this work.

Keep successful Copy All/Paste All acceptance open in [Core #4](https://github.com/joshua-small/qsys-mods-core/issues/4) and [Better Shortcuts #2](https://github.com/joshua-small/qsys-better-shortcuts/issues/2), then retest after a vendor fix. This native failure does not block the independently verified menu hints or Legacy editor input fix. If an immediate workaround becomes necessary, propose a separate version-guarded compatibility change using a bounded, validated payload, with invalid-data and same-component-type checks and an exit plan when Q-SYS fixes it. That proposal is not implemented or accepted here. Do not globally enable BinaryFormatter or fold clipboard serialization into shortcut routing.
