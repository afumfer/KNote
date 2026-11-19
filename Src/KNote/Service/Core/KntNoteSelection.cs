using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;
using KNote.Service.Interfaces;

namespace KNote.Service.Core;

/// <summary>
/// The selection of the notes of a summary (KNote Atlas, stage 2, 2.3, CU-15, chapters 29, 30 and 33), without the AI: the notes
/// modified in the period or with task activity in it (the repository's query, D6) and, combined with «and», of a folder and its
/// subfolders, with some words (the search by words of always, in the topic and the text) and about something (by meaning, among
/// the notes of the period, from the minimum similarity of the search, without its cap of 100: S7). Each one is decided by
/// NoteRelevanceRules with the noise policy, the tags, the corrections of «Note relevance» and its signals; KNote's own notes are
/// only counted. Nothing is written.
/// </summary>
internal static class KntNoteSelection
{
    public const string PeriodError = "The end of the period is before its start.";

    public static async Task<NoteSelectionResultDto> BuildAsync(IKntService service, NoteSelectionRequestDto request,
        CancellationToken cancellationToken, ResultBase result, Dictionary<Guid, (NoteSelectionSourceDto Source, string Text)> texts = null)
    {
        var from = request.From.Date;
        var to = request.To.Date.AddDays(1);
        if (to <= from)
        {
            result.AddErrorMessage(PeriodError);
            return null;
        }

        var resSources = await service.Repository.Notes.GetSelectionSourcesAsync(from, to);
        if (!resSources.IsValid)
        {
            result.AddListErrorMessage(resSources.ListErrorMessage);
            return null;
        }
        var sources = resSources.Entity;

        var resFolders = await service.Repository.Folders.GetAllAsync();
        var folders = (resFolders.IsValid ? resFolders.Entity : null) ?? new List<FolderInfoDto>();
        if (request.FolderId is Guid folderId)
        {
            var subtree = Subtree(folders, folderId);
            sources = sources.Where(s => subtree.Contains(s.FolderId)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(request.Keywords) && sources.Count > 0)
        {
            var found = await service.Notes.GetSearchMinimalAsync(new NotesSearchDto { TextSearch = request.Keywords, SearchInDescription = true });
            if (!found.IsValid)
            {
                result.AddListErrorMessage(found.ListErrorMessage);
                return null;
            }
            var ids = found.Entity.Select(n => n.NoteId).ToHashSet();
            sources = sources.Where(s => ids.Contains(s.NoteId)).ToList();
        }

        var meaningUsed = !string.IsNullOrWhiteSpace(request.MeaningText);
        if (meaningUsed && sources.Count > 0)
        {
            var ids = await AboutAsync(service, request.MeaningText, sources.Select(s => s.NoteId).ToHashSet(), cancellationToken, result);
            if (ids == null)
                return null;
            sources = sources.Where(s => ids.Contains(s.NoteId)).ToList();
        }

        var context = await KntNoisePolicy.LoadContextAsync(service);
        var policy = context.Clean(await KntNoisePolicy.ReadAsync(service));
        var (keptCorrections, keptAnswers, relevanceNote) = await KntRelevanceNotes.ReadAsync(service, result);
        var corrections = keptCorrections.ToDictionary(c => c.NoteId);
        var answers = keptAnswers.ToDictionary(a => a.NoteId);
        if (!result.IsValid)
            return null;

        var selection = new NoteSelectionResultDto { MeaningUsed = meaningUsed, RelevanceNote = relevanceNote };
        var notes = new List<(NoteSelectionSourceDto Source, EnumNoiseReason Noise, string Text)>();
        foreach (var source in sources)
        {
            var noise = NoiseRules.Classify(new NoteClassificationDto
            {
                NoteId = source.NoteId, NoteNumber = source.NoteNumber, FolderId = source.FolderId, NoteTypeId = source.NoteTypeId, Tags = source.Tags
            }, policy, context.KNoteNoteTypeIds, context.KNoteFolderIds);
            if (noise == EnumNoiseReason.KNoteNote)
            {
                selection.KNoteNotes++;
                continue;
            }
            notes.Add((source, noise, ReadingText(service, source.ContentType, source.Description)));
        }

        // The citations #n between the notes of the selection (one per citing note, not itself).
        var citedBy = new Dictionary<int, int>();
        foreach (var (source, _, text) in notes)
            foreach (var number in NoteCitationLinker.CitedNumbers(text).Where(n => n != source.NoteNumber))
                citedBy[number] = citedBy.GetValueOrDefault(number) + 1;

        var folderNames = folders.ToDictionary(f => f.FolderId, f => f.Name);
        foreach (var (source, noise, text) in notes)
        {
            if (texts != null)
                texts[source.NoteId] = (source, text);
            var note = new NoteSelectionDto
            {
                NoteId = source.NoteId, NoteNumber = source.NoteNumber, Topic = source.Topic, FolderId = source.FolderId,
                FolderName = folderNames.GetValueOrDefault(source.FolderId), ModificationDateTime = source.ModificationDateTime,
                ContentHash = NoteRelevanceRules.ContentHash(source.Topic, text)
            };
            var signals = NoteRelevanceRules.SignalsOf(source, text, citedBy.GetValueOrDefault(source.NoteNumber));
            // An answer of the AI only while the note is as it was (2.4, D9).
            var answer = answers.GetValueOrDefault(source.NoteId);
            NoteRelevanceRules.Decide(note, noise, signals, corrections.GetValueOrDefault(source.NoteId),
                answer?.ContentHash == note.ContentHash ? answer : null);
            selection.Notes.Add(note);
        }
        selection.Notes = NoteRelevanceRules.Order(selection.Notes);
        return selection;
    }

    // The folder and its subfolders.
    private static HashSet<Guid> Subtree(List<FolderInfoDto> folders, Guid folderId)
    {
        var children = folders.Where(f => f.ParentId != null).ToLookup(f => f.ParentId.Value, f => f.FolderId);
        var result = new HashSet<Guid> { folderId };
        var pending = new Queue<Guid>(new[] { folderId });
        while (pending.Count > 0)
            foreach (var child in children[pending.Dequeue()])
                if (result.Add(child))
                    pending.Enqueue(child);
        return result;
    }

    // The notes among candidates about text, by meaning (the index brought up to date first, as the search).
    private static async Task<HashSet<Guid>> AboutAsync(IKntService service, string text, HashSet<Guid> candidates,
        CancellationToken cancellationToken, ResultBase result)
    {
        var index = await KntSemanticSearcher.OpenAsync(service, result);
        if (index == null)
            return null;

        await KntSemanticIndexer.RunAsync(service, new KntSemanticIndexer.Options
        {
            SkipIfBusy = true,
            MaxNotes = SemanticIndexRefreshRequest.DefaultMaxNotes,
            MaxDuration = SemanticIndexRefreshRequest.DefaultMaxDuration,
            MaxChunksPerNote = KntSemanticIndexer.QuickRefreshMaxChunks,
            CancellationToken = cancellationToken
        });

        float[] query;
        try
        {
            query = await KntQueryVectorCache.GetAsync(index.Generator, SemanticSearchRules.QueryText(index.Models.QueryInstruction, text),
                index.Models.EmbeddingDimensions, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            result.AddErrorMessage($"The model {index.Models.EmbeddingModel} could not calculate the vector of «{text}»: {ex.Message}");
            return null;
        }

        var snapshot = await KntSemanticSearcher.VectorsAsync(service, index, cancellationToken);
        return SemanticSearchRules.Rank(snapshot.Chunks, query, SemanticSearchRules.MinSimilarity, candidates.Contains)
            .Select(m => m.NoteId).ToHashSet();
    }

    /// <summary>
    /// A period summary ready to be read (2.5): the selection and, of each note that goes in (In, and the doubtful ones as they are:
    /// they go in), what the AI reads: its data and the path of its folder, the values of the attributes of «By concept», its cleaned
    /// text, its tasks with activity in the period and the descriptions of its resources (not the whole text of its documents, P2).
    /// </summary>
    public static async Task<PeriodSummaryPreparedDto> PreparePeriodAsync(IKntService service, PeriodSummaryRequestDto request,
        CancellationToken cancellationToken, ResultBase result)
    {
        var texts = new Dictionary<Guid, (NoteSelectionSourceDto Source, string Text)>();
        var selection = await BuildAsync(service, request.Selection, cancellationToken, result, texts);
        if (selection == null)
            return null;

        var from = request.Selection.From.Date;
        var to = request.Selection.To.Date.AddDays(1);
        var paths = new Dictionary<Guid, string>();
        var prepared = new PeriodSummaryPreparedDto { Selection = selection };

        // The attributes of «By concept» (the ones asked for that still exist and group) and their values, read at once.
        var attributeValues = new Dictionary<Guid, Dictionary<string, string>>();
        if (request.ConceptAttributeIds?.Count > 0)
        {
            var attributes = await service.Repository.KAttributes.GetAllAsync();
            if (!attributes.IsValid)
            {
                result.AddListErrorMessage(attributes.ListErrorMessage);
                return null;
            }
            var ids = PeriodConceptRules.Chosen(request.ConceptAttributeIds, PeriodConceptRules.Choices(attributes.Entity));
            prepared.ConceptAttributes = PeriodConceptRules.Columns(attributes.Entity, ids);
            if (ids.Count > 0)
            {
                var values = await service.Repository.Notes.GetAttributeValuesAsync(ids);
                if (!values.IsValid)
                {
                    result.AddListErrorMessage(values.ListErrorMessage);
                    return null;
                }
                attributeValues = PeriodConceptRules.ValuesByNote(values.Entity, attributes.Entity, ids);
            }
        }
        foreach (var note in selection.Notes.Where(n => n.IsIncluded))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var (source, text) = texts[note.NoteId];
            if (!paths.TryGetValue(source.FolderId, out var path))
            {
                var resPath = await service.Folders.GetPathAsync(source.FolderId);
                paths[source.FolderId] = path = resPath.IsValid ? resPath.Entity : note.FolderName;
            }

            var period = new PeriodNoteDto
            {
                NoteId = note.NoteId, NoteNumber = note.NoteNumber, Topic = note.Topic, FolderPath = path, Tags = source.Tags,
                ModificationDateTime = source.ModificationDateTime, Text = text, Score = note.Score,
                Attributes = attributeValues.GetValueOrDefault(note.NoteId) ?? new()
            };
            if (source.TasksWithActivity > 0)
            {
                var tasks = await service.Repository.Notes.GetNoteTasksAsync(note.NoteId);
                if (tasks.IsValid)
                {
                    var withActivity = tasks.Entity.Where(t => PeriodSummaryRules.HasActivity(t, from, to)).ToList();
                    period.Tasks = withActivity.Select(PeriodSummaryRules.TaskText).ToList();
                    PeriodSummaryRules.SetTaskCounts(period, withActivity, from, to);
                }
            }
            if (source.Resources > 0)
            {
                var resources = await service.Notes.GetResourcesInfoAsync(note.NoteId);
                if (resources.IsValid)
                    period.Resources = resources.Entity.Where(r => !string.IsNullOrWhiteSpace(r.Description))
                        .Select(r => PeriodSummaryRules.ResourceText(r.NameOut, r.Description)).ToList();
            }
            prepared.Notes.Add(period);
        }
        return prepared;
    }

    /// <summary>The doubtful notes as the AI reads them (2.4): their data and the beginning of their cleaned text, with the same
    /// fingerprint as the selection. KNote's own notes and the ones that no longer exist are left out.</summary>
    public static async Task<List<NoteToClassifyDto>> ToClassifyAsync(IKntService service, IEnumerable<Guid> noteIds, ResultBase result)
    {
        var context = await KntNoisePolicy.LoadContextAsync(service);
        var folders = (await service.Repository.Folders.GetAllAsync()).Entity?.ToDictionary(f => f.FolderId, f => f.Name) ?? new();
        var types = (await service.Repository.NoteTypes.GetAllAsync()).Entity?.ToDictionary(t => t.NoteTypeId, t => t.Name) ?? new();

        var notes = new List<NoteToClassifyDto>();
        foreach (var noteId in (noteIds ?? Enumerable.Empty<Guid>()).Distinct())
        {
            var res = await service.Repository.Notes.GetAsync(noteId);
            if (!res.IsValid)
            {
                result.AddListErrorMessage(res.ListErrorMessage);
                return null;
            }
            var note = res.Entity;
            if (note == null || context.KNoteFolderIds.Contains(note.FolderId) || (note.NoteTypeId is Guid t && context.KNoteNoteTypeIds.Contains(t)))
                continue;
            var text = ReadingText(service, note.ContentType, note.Description);
            notes.Add(new NoteToClassifyDto
            {
                NoteId = note.NoteId, NoteNumber = note.NoteNumber, Topic = note.Topic, FolderName = folders.GetValueOrDefault(note.FolderId),
                NoteTypeName = note.NoteTypeId is Guid typeId ? types.GetValueOrDefault(typeId) : null, Tags = note.Tags,
                Text = NoteClassificationRules.Cut(text), ContentHash = NoteRelevanceRules.ContentHash(note.Topic, text)
            });
        }
        return notes;
    }

    // The text of a note as it is read: Markdown (an HTML note converted), cleaned (NoteReadingCleaner).
    private static string ReadingText(IKntService service, string contentType, string description)
    {
        var html = contentType != null
            && string.Equals(new NoteInfoDto { ContentType = contentType }.GetContentTypeExt().ForDescription, "html", StringComparison.OrdinalIgnoreCase);
        return NoteReadingCleaner.Clean(html ? service.Notes.UtilHtmlToMarkdown(description ?? "") : description ?? "");
    }
}
