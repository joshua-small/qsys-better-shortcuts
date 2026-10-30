namespace Designer.Modules.BetterShortcuts;

// A conflict preview is valid only until the draft or command registry changes.
// Keep this lifecycle independent of WPF and of Core's conflict planning.
internal sealed class ShortcutChangeReview<T> where T : class
{
    internal T? Pending { get; private set; }
    private long revision;

    internal void Show(T? change) { revision++; Pending = change; }
    internal void Invalidate() => Show(null);

    internal bool TryApply(Action<T> validate, Action<T> stage)
    {
        if (Pending is not { } change) return false;
        long reviewedRevision = revision;
        validate(change);
        // Validation may synchronously notify subscribers of a changed draft
        // or registry. Even a replacement preview needs its own confirmation.
        if (reviewedRevision != revision) return false;
        // Staging notifies the shared draft's subscribers synchronously. Consume
        // approval first so a callback cannot apply the same plan a second time.
        Invalidate();
        stage(change);
        return true;
    }
}
