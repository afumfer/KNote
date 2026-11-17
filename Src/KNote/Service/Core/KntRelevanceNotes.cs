using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;

namespace KNote.Service.Core;

/// <summary>
/// «Note relevance» (@KNoteRelevance, KNote Atlas, stage 2, chapter 30, D10): the system note, in the folder «KNote Atlas», where
/// KNote keeps the corrections of the selection of the summaries made by people and the classification of the doubtful notes by the
/// AI (RelevanceNote is its text). One per repository (with several, the oldest), created the first time it is written. Its writes
/// are made one at a time per database and read the note again before saving it, so that two people saving at once don't lose each
/// other's corrections; writing it takes away the rows of the notes that no longer exist. It is a protected note
/// (KntAtlasNotes.ProtectedNoteTypeNames): by hand, only ProjectManager and Admin save it; KNote writes it for a Staff inside an
/// authorized command (KntAuthorizationBypass). Errors are added to the command's result.
/// </summary>
internal static class KntRelevanceNotes
{
    private const string BypassReason = "KNote Atlas: the corrections of the selection of the summaries (Note relevance)";

    // The note is read and written again by each write: one at a time per database (in this process).
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> WriteLocks = new();

    /// <summary>The corrections and the answers of the AI kept; none without the note (nothing is created).</summary>
    public static async Task<(List<RelevanceCorrectionDto> Corrections, List<RelevanceAiAnswerDto> AiAnswers)> ReadAsync(
        IKntService service, ResultBase result)
    {
        var note = await KntAtlasNotes.FindFirstAsync(service, KntConst.RelevanceTag, result);
        return RelevanceNote.Parse(note?.Description);
    }

    /// <summary>The changes of «Review selection», saved by the current user; it gives the corrections as they are kept.</summary>
    public static async Task<List<RelevanceCorrectionDto>> SaveCorrectionsAsync(IKntService service,
        IReadOnlyList<RelevanceCorrectionChangeDto> changes, ResultBase result)
    {
        var user = await KntSystemNotes.GetCurrentUserAsync(service, result);
        if (!result.IsValid)
            return null;

        // The notes corrected now, with their number and topic for the table.
        var notes = new Dictionary<Guid, (int NoteNumber, string Topic)>();
        foreach (var change in changes.Where(c => c.Decision != null))
        {
            var note = (await service.Notes.GetAsync(change.NoteId)).Entity;
            if (note != null)
                notes[note.NoteId] = (note.NoteNumber, note.Topic);
        }

        return await WriteAsync(service, result, (corrections, answers) =>
            (RelevanceNote.Apply(corrections, changes, id => notes.TryGetValue(id, out var n) ? n : null, UserName(user), DateTime.Now), answers));
    }

    /// <summary>The answers of the AI for some doubtful notes (2.4), each one replacing the note's one.</summary>
    public static Task<List<RelevanceCorrectionDto>> SaveAiAnswersAsync(IKntService service, IReadOnlyList<RelevanceAiAnswerDto> answers,
        ResultBase result) =>
        WriteAsync(service, result, (corrections, kept) => (corrections, RelevanceNote.ApplyAiAnswers(kept, answers)));

    // Writes the note with what change makes of what it has (read again inside the lock).
    private static async Task<List<RelevanceCorrectionDto>> WriteAsync(IKntService service, ResultBase result,
        Func<List<RelevanceCorrectionDto>, List<RelevanceAiAnswerDto>, (List<RelevanceCorrectionDto>, List<RelevanceAiAnswerDto>)> change)
    {
        using var bypass = KntAuthorizationBypass.Begin(service, BypassReason);

        var writeLock = WriteLocks.GetOrAdd(service.RepositoryRef?.ConnectionString ?? "", _ => new SemaphoreSlim(1, 1));
        await writeLock.WaitAsync();
        try
        {
            var note = await KntAtlasNotes.EnsureSingleAsync(service, KntConst.RelevanceTag,
                "KNote Atlas: the corrections of the selection of the summaries and the classification of the doubtful notes by the AI.",
                KntConst.RelevanceTopic, RelevanceNote.Write(null, null), result);
            if (note == null || !result.IsValid)
                return null;

            var (corrections, answers) = RelevanceNote.Parse(note.Description);
            (corrections, answers) = change(corrections, answers);

            // The rows of the notes that no longer exist go away.
            var existing = await service.Repository.Notes.GetClassificationsAsync();
            if (existing.IsValid)
            {
                var ids = existing.Entity.Select(n => n.NoteId).ToHashSet();
                corrections = corrections.Where(c => ids.Contains(c.NoteId)).ToList();
                answers = answers.Where(a => ids.Contains(a.NoteId)).ToList();
            }

            note.Description = AtlasTemplates.NoteText(RelevanceNote.Write(corrections, answers));
            var saved = await service.Notes.SaveAsync(note);
            if (!saved.IsValid)
            {
                result.AddListErrorMessage(saved.ListErrorMessage);
                return null;
            }
            return corrections;
        }
        finally
        {
            writeLock.Release();
        }
    }

    private static string UserName(UserDto user) => string.IsNullOrWhiteSpace(user?.FullName) ? user?.UserName : user.FullName;
}
