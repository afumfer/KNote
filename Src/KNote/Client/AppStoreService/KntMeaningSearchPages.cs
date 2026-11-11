using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService;

/// <summary>
/// The pages of a notes list found by meaning (KNote Atlas), for NotesGrid.LoadPage: the search is done once, when the
/// grid asks for its first page (the pages it asks at once share it), and the pages are cut from its notes in memory,
/// in the order asked (NoteListSorter; none: the most similar first). The search by meaning is not paged in the server.
/// A search that fails is done again by the next page asked (the grid's Retry). Dispose (a new search, or the page
/// left) cancels the one in progress.
/// </summary>
public sealed class KntMeaningSearchPages : IDisposable
{
    private readonly Func<CancellationToken, Task<Result<SemanticSearchResultDto>>> _search;
    private readonly CancellationTokenSource _disposal = new();
    private Task<Result<SemanticSearchResultDto>>? _found;

    public KntMeaningSearchPages(Func<CancellationToken, Task<Result<SemanticSearchResultDto>>> search)
    {
        _search = search;
    }

    /// <summary>The notes of the index not up to date after the search (D8); 0 until it is done.</summary>
    public int PendingNotes { get; private set; }

    /// <summary>Of PendingNotes (4.6b): the ones that wait for the full indexing, and the ones whose documents have no file here.</summary>
    public int NotesForFullRun { get; private set; }

    public int NotesWithoutFiles { get; private set; }

    /// <summary>The search is done (PendingNotes is known), to show the notice.</summary>
    public event Action? Found;

    public async Task<Result<List<NoteMinimalDto>>> LoadPageAsync(PageIdentifier page, NotesOrder? order, CancellationToken cancellationToken)
    {
        var found = _found ??= SearchAsync();
        Result<SemanticSearchResultDto> result;
        try
        {
            result = await found.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // As a request canceled (BaseService.SendAsync): not valid, and not notified.
            var canceled = new Result<List<NoteMinimalDto>>();
            canceled.AddErrorMessage("Canceled.");
            return canceled;
        }

        var pageResult = new Result<List<NoteMinimalDto>>();
        if (!result.IsValid)
        {
            if (_found == found)
                _found = null;
            pageResult.AddListErrorMessage(result.ListErrorMessage);
            return pageResult;
        }

        var notes = NoteListSorter.Sort(result.Entity.Notes, order?.Column, order?.Ascending ?? true);
        pageResult.Entity = notes.Skip(page.Offset).Take(page.PageSize).Cast<NoteMinimalDto>().ToList();
        pageResult.TotalCount = notes.Count;
        return pageResult;
    }

    private async Task<Result<SemanticSearchResultDto>> SearchAsync()
    {
        var result = await _search(_disposal.Token);
        if (result.IsValid && !_disposal.IsCancellationRequested)
        {
            PendingNotes = result.Entity.PendingNotes;
            NotesForFullRun = result.Entity.NotesForFullRun;
            NotesWithoutFiles = result.Entity.NotesWithoutFiles;
            Found?.Invoke();
        }
        return result;
    }

    public void Dispose()
    {
        _disposal.Cancel();
        _disposal.Dispose();
    }
}
