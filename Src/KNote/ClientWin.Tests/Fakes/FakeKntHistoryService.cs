using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Interfaces;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>
/// Minimal IKntHistoryService test double: the members exercised by the tests work through settable delegates; everything
/// else throws, so an unexpectedly-touched member fails loudly instead of silently returning a default value.
/// </summary>
internal class FakeKntHistoryService : IKntHistoryService
{
    public Func<Guid, int, int, Task<Result<NoteHistoryPageDto>>>? GetNoteHistoryAsyncImpl { get; set; }
    public Func<Guid, Guid, Task<Result<NoteVersionViewDto>>>? GetNoteVersionAsyncImpl { get; set; }

    public Task<Result<NoteHistoryPageDto>> GetNoteHistoryAsync(Guid noteId, int skip = 0, int take = 0) =>
        (GetNoteHistoryAsyncImpl ?? throw new NotSupportedException($"{nameof(GetNoteHistoryAsync)} not configured for this test"))(noteId, skip, take);

    public Task<Result<NoteVersionViewDto>> GetNoteVersionAsync(Guid noteId, Guid klogId) =>
        (GetNoteVersionAsyncImpl ?? throw new NotSupportedException($"{nameof(GetNoteVersionAsync)} not configured for this test"))(noteId, klogId);

    public Func<Guid, Guid, bool, Task<Result<NoteComparisonDto>>>? GetNoteComparisonAsyncImpl { get; set; }
    public Func<Guid, Guid, Task<Result<NoteRestoreResultDto>>>? RestoreNoteVersionAsyncImpl { get; set; }

    public Task<Result<NoteComparisonDto>> GetNoteComparisonAsync(Guid noteId, Guid klogId, bool withCurrentNote = false) =>
        (GetNoteComparisonAsyncImpl ?? throw new NotSupportedException($"{nameof(GetNoteComparisonAsync)} not configured for this test"))(noteId, klogId, withCurrentNote);

    public Task<Result<NoteRestoreResultDto>> RestoreNoteVersionAsync(Guid noteId, Guid klogId) =>
        (RestoreNoteVersionAsyncImpl ?? throw new NotSupportedException($"{nameof(RestoreNoteVersionAsync)} not configured for this test"))(noteId, klogId);
}
