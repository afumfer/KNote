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

    public Func<Task<Result<List<DeletedNoteDto>>>>? GetDeletedNotesAsyncImpl { get; set; }
    public Func<Guid, Task<Result<NoteRecoverResultDto>>>? RecoverNoteAsyncImpl { get; set; }
    public Func<Guid, Task<Result>>? RemoveDeletedNoteAsyncImpl { get; set; }
    public Func<RepositoryChangesQueryDto, Task<Result<RepositoryChangesPageDto>>>? GetRepositoryChangesAsyncImpl { get; set; }
    public Func<RepositoryChangesQueryDto, Task<Result<List<string>>>>? GetRepositoryChangesUsersAsyncImpl { get; set; }

    public Task<Result<RepositoryChangesPageDto>> GetRepositoryChangesAsync(RepositoryChangesQueryDto query) =>
        (GetRepositoryChangesAsyncImpl ?? throw new NotSupportedException($"{nameof(GetRepositoryChangesAsync)} not configured for this test"))(query);

    public Task<Result<List<string>>> GetRepositoryChangesUsersAsync(RepositoryChangesQueryDto query) =>
        (GetRepositoryChangesUsersAsyncImpl ?? throw new NotSupportedException($"{nameof(GetRepositoryChangesUsersAsync)} not configured for this test"))(query);

    public Func<Task<Result<HistoryRetentionInfoDto>>>? GetRetentionAsyncImpl { get; set; }
    public Func<HistoryRetentionDto, Task<Result<HistoryRetentionDto>>>? SaveRetentionAsyncImpl { get; set; }

    public Task<Result<List<DeletedNoteDto>>> GetDeletedNotesAsync() =>
        (GetDeletedNotesAsyncImpl ?? throw new NotSupportedException($"{nameof(GetDeletedNotesAsync)} not configured for this test"))();

    public Task<Result<NoteRecoverResultDto>> RecoverNoteAsync(Guid noteId) =>
        (RecoverNoteAsyncImpl ?? throw new NotSupportedException($"{nameof(RecoverNoteAsync)} not configured for this test"))(noteId);

    public Task<Result> RemoveDeletedNoteAsync(Guid noteId) =>
        (RemoveDeletedNoteAsyncImpl ?? throw new NotSupportedException($"{nameof(RemoveDeletedNoteAsync)} not configured for this test"))(noteId);

    public Task<Result<HistoryRetentionInfoDto>> GetRetentionAsync() =>
        (GetRetentionAsyncImpl ?? throw new NotSupportedException($"{nameof(GetRetentionAsync)} not configured for this test"))();

    public Task<Result<HistoryRetentionDto>> SaveRetentionAsync(HistoryRetentionDto retention) =>
        (SaveRetentionAsyncImpl ?? throw new NotSupportedException($"{nameof(SaveRetentionAsync)} not configured for this test"))(retention);

    public Task<Result<NoteRestoreResultDto>> RestoreNoteVersionAsync(Guid noteId, Guid klogId) =>
        (RestoreNoteVersionAsyncImpl ?? throw new NotSupportedException($"{nameof(RestoreNoteVersionAsync)} not configured for this test"))(noteId, klogId);
}
