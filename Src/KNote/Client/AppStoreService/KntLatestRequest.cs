namespace KNote.Client.AppStoreService;

/// <summary>
/// The latest request of a kind (the note selected, the text searched, the note type chosen...): a new one cancels the
/// one still running, whose answer would be out of date, and so does Dispose (the component's: nobody waits for it any
/// more). A cancelled request comes back as Canceled, and its answer must not be used. The request gets the token to
/// pass to IStore (see BaseService.SendAsync: a cancelled call isn't notified as an error).
/// </summary>
/// <example>
/// var result = await loadNote.RunAsync(ct => store.Notes.GetAsync(noteId, ct));
/// if (result.Canceled) return;
/// note = result.Value.IsValid ? result.Value.Entity : null;
/// </example>
public sealed class KntLatestRequest : IDisposable
{
    private CancellationTokenSource? current;
    private bool disposed;

    public async Task<KntRequestResult<T>> RunAsync<T>(Func<CancellationToken, Task<T>> request)
    {
        if (disposed)
            return KntRequestResult<T>.CanceledResult;

        current?.Cancel();
        using var cancellation = new CancellationTokenSource();
        current = cancellation;
        try
        {
            var value = await request(cancellation.Token);
            return cancellation.IsCancellationRequested ? KntRequestResult<T>.CanceledResult : new KntRequestResult<T>(false, value);
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            return KntRequestResult<T>.CanceledResult;
        }
        finally
        {
            // Before it is disposed: a later Cancel only finds the one running, if any.
            if (current == cancellation)
                current = null;
        }
    }

    /// <summary>Cancels the request running, if any (e.g. nothing to show any more).</summary>
    public void Cancel() => current?.Cancel();

    public void Dispose()
    {
        disposed = true;
        Cancel();
    }
}

/// <summary>The answer of a KntLatestRequest: Value, unless Canceled (then Value must not be used).</summary>
public readonly record struct KntRequestResult<T>(bool Canceled, T Value)
{
    public static KntRequestResult<T> CanceledResult => new(true, default!);
}
