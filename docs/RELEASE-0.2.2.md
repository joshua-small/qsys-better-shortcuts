# Better Shortcuts 0.2.2

When assignments or registered commands change while an overwrite, Reset, or
bulk-default review is open, the review closes. Reopen it to review the current
conflicts. An older confirmation cannot restore an earlier draft. Each accepted
plan is consumed before staging, so synchronous notifications cannot apply it
twice. Unrelated custom bindings and explicit editor permissions remain in the
current draft.

Installation preserves existing preferences. Shortcut defaults, settings format,
and compatibility requirements are unchanged: Mods Core 0.1.6 or newer within
0.1.x and Mod Manager 0.8 or newer. The existing exact-build checks and consent
for untested Designer builds still apply. This release does not add support for
another Designer build.

The 20 portable review lifecycle checks cover stale individual, Reset, and bulk
reviews, replacement previews, validation rejection, and reentrant callbacks.
The 45 catalog/projection checks cover the shortcut table model. These checks
do not establish physical keyboard, menu rendering, or hardware behavior.
The focused [Designer review mode](../tests/DesignerAcceptance/README.md#review-invalidation-mode)
exercises the real Preferences draft separately; its fixture is not included in
the installable package. Historical application checks retain their original
build and scope in the [validation guide](DESIGNER-VALIDATION-10.5.md).
