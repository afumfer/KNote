namespace KNote.Client.AppStoreService;

/// <summary>
/// One operation at a time (a save, a delete...). Blazor runs an async event handler again if the user clicks or
/// presses Enter while it awaits the server: while one runs, another call is ignored (a double click saves once).
/// IsBusy shows it in the buttons (RadzenButton IsBusy or Disabled). Each component keeps its own, one per group of
/// operations that must not overlap; WebAssembly has a single thread, so no lock is needed.
/// </summary>
public sealed class KntSingleFlight
{
    public bool IsBusy { get; private set; }

    /// <summary>Runs the operation, unless another one is running: then nothing is done and it returns false.</summary>
    public async Task<bool> RunAsync(Func<Task> operation)
    {
        if (IsBusy)
            return false;

        IsBusy = true;
        try
        {
            await operation();
        }
        finally
        {
            IsBusy = false;
        }
        return true;
    }
}
