using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Service.Interfaces;

/// <summary>
/// The history of the notes (03-Historial.md), what the apps show of it: the «History» tab of a note (H.3). Reading it is for
/// any user, like reading the note.
/// </summary>
public interface IKntHistoryService
{
    /// <summary>A page of the rows of a note, the newest first (NoteHistoryPageDto.PageSize when take is 0). Guest.</summary>
    Task<Result<NoteHistoryPageDto>> GetNoteHistoryAsync(Guid noteId, int skip = 0, int take = 0);

    /// <summary>The note as it was after a change of its history (H18), with its names resolved. Guest.</summary>
    Task<Result<NoteVersionViewDto>> GetNoteVersionAsync(Guid noteId, Guid klogId);

    /// <summary>«Compare» (H22): what a change did, or that version against the note now (withCurrentNote). Guest.</summary>
    Task<Result<NoteComparisonDto>> GetNoteComparisonAsync(Guid noteId, Guid klogId, bool withCurrentNote = false);

    /// <summary>«Restore» (H23): the note as it was after that change, in a row Restored of its own. Staff.</summary>
    Task<Result<NoteRestoreResultDto>> RestoreNoteVersionAsync(Guid noteId, Guid klogId);

    /// <summary>«Recover deleted notes» (H24): the deleted notes that can still be recovered, the newest first. Staff.</summary>
    Task<Result<List<DeletedNoteDto>>> GetDeletedNotesAsync();

    /// <summary>Recovers a deleted note (H24, H26): its own data, the same id, its number if free. Staff.</summary>
    Task<Result<NoteRecoverResultDto>> RecoverNoteAsync(Guid noteId);

    /// <summary>«Remove permanently» (H27): empties the versions of a deleted note; it can no longer be recovered. Admin.</summary>
    Task<Result> RemoveDeletedNoteAsync(Guid noteId);

    /// <summary>The retention of the history and the day it started (H25). Guest.</summary>
    Task<Result<HistoryRetentionInfoDto>> GetRetentionAsync();

    /// <summary>Keeps the retention of the history (H25); it applies in the next purge. Admin.</summary>
    Task<Result<HistoryRetentionDto>> SaveRetentionAsync(HistoryRetentionDto retention);
}
