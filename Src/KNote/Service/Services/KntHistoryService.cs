using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Service.Interfaces;
using KNote.Service.ServicesCommands;

namespace KNote.Service.Services;

public class KntHistoryService : KntServiceBase, IKntHistoryService
{
    public KntHistoryService(IKntService service) : base(service)
    {

    }

    public async Task<Result<NoteHistoryPageDto>> GetNoteHistoryAsync(Guid noteId, int skip = 0, int take = 0) =>
        await ExecuteCommand(new KntHistoryGetNoteHistoryAsyncCommand(Service, noteId, skip, take));

    public async Task<Result<NoteVersionViewDto>> GetNoteVersionAsync(Guid noteId, Guid klogId) =>
        await ExecuteCommand(new KntHistoryGetNoteVersionAsyncCommand(Service, noteId, klogId));

    public async Task<Result<NoteComparisonDto>> GetNoteComparisonAsync(Guid noteId, Guid klogId, bool withCurrentNote = false) =>
        await ExecuteCommand(new KntHistoryGetNoteComparisonAsyncCommand(Service, noteId, klogId, withCurrentNote));

    public async Task<Result<NoteRestoreResultDto>> RestoreNoteVersionAsync(Guid noteId, Guid klogId) =>
        await ExecuteCommand(new KntHistoryRestoreNoteVersionAsyncCommand(Service, noteId, klogId));

    public async Task<Result<List<DeletedNoteDto>>> GetDeletedNotesAsync() =>
        await ExecuteCommand(new KntHistoryGetDeletedNotesAsyncCommand(Service));

    public async Task<Result<NoteRecoverResultDto>> RecoverNoteAsync(Guid noteId) =>
        await ExecuteCommand(new KntHistoryRecoverNoteAsyncCommand(Service, noteId));

    public async Task<Result> RemoveDeletedNoteAsync(Guid noteId) =>
        await ExecuteCommand(new KntHistoryRemoveDeletedNoteAsyncCommand(Service, noteId));

    public async Task<Result<RepositoryChangesPageDto>> GetRepositoryChangesAsync(RepositoryChangesQueryDto query) =>
        await ExecuteCommand(new KntHistoryGetRepositoryChangesAsyncCommand(Service, query));

    public async Task<Result<List<string>>> GetRepositoryChangesUsersAsync(RepositoryChangesQueryDto query) =>
        await ExecuteCommand(new KntHistoryGetRepositoryChangesUsersAsyncCommand(Service, query));

    public async Task<Result<HistoryRetentionInfoDto>> GetRetentionAsync() =>
        await ExecuteCommand(new KntHistoryGetRetentionAsyncCommand(Service));

    public async Task<Result<HistoryRetentionDto>> SaveRetentionAsync(HistoryRetentionDto retention) =>
        await ExecuteCommand(new KntHistorySaveRetentionAsyncCommand(Service, retention));
}
