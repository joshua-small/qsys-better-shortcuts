# Shortcut review lifecycle

Run from the repository root:

```sh
dotnet run --project tests/Review --configuration Release --artifacts-path artifacts/review
```

This portable suite compiles `ShortcutChangeReview.cs` from production. Synthetic
plans stand in for Core planner results; they do not reimplement conflict
detection. The controller must pass the accepted plan through unchanged,
invalidate it when shared state changes, reject a plan invalidated during
validation, and consume approval before synchronous staging callbacks.

The suite covers individual overwrites, Reset and bulk-default reviews, including
preservation of newer unrelated assignments and explicit editor takeover
permissions. It also checks absent/invalid previews, validation/staging failures,
replacement previews and repeated/reentrant confirmation. It never reads or
writes user preferences.

## Application acceptance still required

These checks do not establish WPF event delivery, focus, keyboard capture, native
command routing or Core conflict detection. The production editor invalidates
reviews from both `ControlDraft.Changed` and `ModControls.Changed`. A future
authorized, isolated Designer run should use the focused
[`-Reviews` acceptance mode](../DesignerAcceptance/README.md#review-invalidation-mode)
for native draft/registry notifications and separately verify the visible and
physical-input behavior:

1. Open an individual conflict preview. Trigger a shared draft update using a
   controlled fixture, changing an unrelated custom binding and editor takeover
   choice. Confirm the overlay closes, the newer draft remains visible, and
   Enter cannot apply the old preview.
2. Repeat for Reset with conflicts and Review mod defaults. Verify that reopening
   builds a fresh preview and that only its explicitly accepted transfers apply.
3. Trigger a command-registry notification while recording or reviewing. Verify
   cancellation and refreshed rows without applying the old plan.
4. With no intervening change, accept an individual edit and a bulk review.
   Confirm each stages once, updates the table and menu hints, and preserves
   unrelated alternatives. Preferences Cancel must still discard all staged
   changes; Preferences OK/reopen is a separate persistence check.

Retain the existing US-layout and supported-host limits. Do not infer support for
detached script windows, runtime UCI popups or additional browser editors from
this portable suite.
