using Designer.Modules.BetterShortcuts;

int checks = 0;
void Check(bool condition, string message) { if (!condition) throw new Exception(message); checks++; }
void Reject(Action action, string message)
{
    try { action(); } catch (ArgumentException) { checks++; return; }
    throw new Exception(message);
}

// Synthetic planner output: these tests exercise review/confirmation lifecycle,
// not Core's key parsing, conflict detection or host routing.
var current = new Draft(new() { ["preferences"] = "Ctrl+OemComma", ["other"] = "Ctrl+K; Ctrl+L" },
    new() { ["other"] = "Ctrl+K" });
var overwrite = new Draft(new() { ["preferences"] = "Ctrl+OemComma", ["other"] = "Ctrl+L", ["target"] = "Ctrl+K" },
    new() { ["target"] = "Ctrl+K" });
var review = new ShortcutChangeReview<Draft>();
int validations = 0, stages = 0;
void Validate(Draft _) => validations++;
void Stage(Draft next) { stages++; current = next; }

Check(!review.TryApply(Validate, Stage) && validations == 0 && stages == 0,
    "Accept without a reviewed plan must not validate or stage anything.");
review.Show(overwrite);
Check(stages == 0 && current.Bindings["other"] == "Ctrl+K; Ctrl+L",
    "Showing an overwrite warning must not change the draft.");
Check(review.TryApply(Validate, Stage) && validations == 1 && stages == 1,
    "A confirmed current preview must stage exactly once.");
Check(ReferenceEquals(current, overwrite) && current.Bindings["preferences"] == "Ctrl+OemComma"
    && current.Bindings["other"] == "Ctrl+L" && current.Editors["target"] == "Ctrl+K",
    "Confirmation must pass through the reviewed plan, including unrelated keys, alternatives and editor consent.");
Check(!review.TryApply(Validate, Stage) && stages == 1,
    "Repeated Enter must not replay a consumed approval.");

foreach (var context in new[] { "individual overwrite", "reset", "bulk defaults" })
{
    review.Show(overwrite);
    var newer = new Draft(new() { ["preferences"] = "Ctrl+Alt+OemComma", ["other"] = "Ctrl+L" },
        new() { ["other"] = "Ctrl+L" });
    current = newer;
    review.Invalidate(); // The editor invokes this on shared draft/registry notification.
    Check(!review.TryApply(Validate, Stage) && ReferenceEquals(current, newer)
        && current.Editors["other"] == "Ctrl+L" && !current.Editors.ContainsKey("target"),
        $"A stale {context} must not restore old custom keys or editor permissions.");
}
review.Show(overwrite);
review.Show(null);
Check(!review.TryApply(Validate, Stage), "An invalid replacement capture must revoke the previous preview.");
review.Show(overwrite);
review.Invalidate();
review.Show(overwrite);
Check(review.TryApply(Validate, Stage), "The user can explicitly review and accept again after invalidation.");

review.Show(overwrite);
int before = stages;
Reject(() => review.TryApply(_ => throw new ArgumentException("Conflicting request"), Stage),
    "Validation rejection must propagate to the editor's error handling.");
Check(stages == before, "A rejected plan must never be staged.");
review.Invalidate();

review.Show(overwrite);
Check(!review.TryApply(_ => review.Invalidate(), Stage) && stages == before,
    "A shared change during validation must invalidate the in-flight confirmation.");
review.Show(overwrite);
var replacement = new Draft(new() { ["target"] = "Ctrl+J" }, new());
Check(!review.TryApply(_ => review.Show(replacement), Stage) && stages == before,
    "A replacement preview created during validation needs separate confirmation.");
Check(ReferenceEquals(review.Pending, replacement) && review.TryApply(Validate, Stage)
    && ReferenceEquals(current, replacement), "Only a separately confirmed replacement may be staged.");

review.Show(overwrite);
bool nestedApplied = true;
before = stages;
Check(review.TryApply(Validate, next =>
{
    nestedApplied = review.TryApply(Validate, Stage);
    Stage(next);
    review.Invalidate(); // Shared draft notification during staging.
}), "A current plan can stage while shared subscribers run.");
Check(!nestedApplied && stages == before + 1 && review.Pending is null,
    "Synchronous subscriber callbacks must not apply approval twice.");

review.Show(overwrite);
Reject(() => review.TryApply(Validate, _ => throw new ArgumentException("Staging rejected")),
    "Staging failures must propagate.");
Check(!review.TryApply(Validate, Stage), "A failed staging callback must not leave replayable approval.");
Check(stages == before + 1, "Failure must not execute an unrelated staging callback.");

Console.WriteLine($"Passed {checks} shortcut review lifecycle checks. No WPF, vendor assemblies, Designer or saved preferences used.");

sealed record Draft(Dictionary<string, string> Bindings, Dictionary<string, string> Editors);
