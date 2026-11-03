using System;
using System.Linq;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Service.Core;

/// <summary>
/// The notes of KNote Atlas' search evaluation, in the folder «KNote Atlas» (KntSystemNotes): the one of the test queries
/// (created the first time with SearchTestQueries.Template, edited with the note editor) and the reports saved. Errors
/// are added to the command's result.
/// </summary>
internal static class KntSearchEvaluationNotes
{
    private const string BypassReason = "KNote Atlas: the notes of the search evaluation";

    public static async Task<NoteDto> TestQueriesNoteAsync(IKntService service, ResultBase result)
    {
        var noteTypeId = await KntSystemNotes.EnsureNoteTypeAsync(service, KntConst.TestQueriesTag,
            "KNote Atlas: the test queries of the search (one note).", BypassReason, result);
        if (!result.IsValid)
            return null;
        var folder = await KntSystemNotes.EnsureFolderAsync(service, KntConst.AtlasFolderName, BypassReason, result);
        if (!result.IsValid)
            return null;

        var noteId = await KntSystemNotes.FindOrCreateAsync(service, result,
            find: async () =>
            {
                var resNotes = await service.Notes.GetFilterMinimalAsync(new NotesFilterDto { NoteTypeId = noteTypeId });
                if (!resNotes.IsValid)
                {
                    result.AddListErrorMessage(resNotes.ListErrorMessage);
                    return Guid.Empty;
                }
                // With several (copies), always the oldest one.
                return resNotes.Entity?.OrderBy(n => n.NoteNumber).FirstOrDefault()?.NoteId ?? Guid.Empty;
            },
            create: () => SaveNewAsync(service, KntConst.TestQueriesTopic, SearchTestQueries.Template, noteTypeId, folder.FolderId, result));
        if (!result.IsValid)
            return null;

        var resNote = await service.Notes.GetAsync(noteId);
        if (resNote.Entity == null)
            result.AddErrorMessage("The note of the search test queries could not be read.");
        return resNote.Entity;
    }

    public static async Task<NoteDto> SaveReportAsync(IKntService service, string markdown, ResultBase result)
    {
        var noteTypeId = await KntSystemNotes.EnsureNoteTypeAsync(service, KntConst.SearchEvaluationTag,
            "KNote Atlas: the reports of the search evaluations.", BypassReason, result);
        if (!result.IsValid)
            return null;
        var folder = await KntSystemNotes.EnsureFolderAsync(service, KntConst.AtlasFolderName, BypassReason, result);
        if (!result.IsValid)
            return null;

        var noteId = await SaveNewAsync(service, $"Search evaluation · {DateTime.Now:yyyy-MM-dd HH:mm}", markdown, noteTypeId, folder.FolderId, result);
        if (!result.IsValid)
            return null;
        return (await service.Notes.GetAsync(noteId)).Entity;
    }

    private static async Task<Guid> SaveNewAsync(IKntService service, string topic, string description, Guid noteTypeId, Guid folderId,
        ResultBase result)
    {
        var resNew = await service.Notes.NewAsync();
        if (!resNew.IsValid)
        {
            result.AddListErrorMessage(resNew.ListErrorMessage);
            return Guid.Empty;
        }
        var note = resNew.Entity;
        note.Topic = topic;
        note.Description = description;
        note.NoteTypeId = noteTypeId;
        note.FolderId = folderId;

        var resSave = await service.Notes.SaveAsync(note);
        if (!resSave.IsValid)
        {
            result.AddListErrorMessage(resSave.ListErrorMessage);
            return Guid.Empty;
        }
        return resSave.Entity.NoteId;
    }
}
