using System;
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
}
