using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.Service.ServicesCommands;

// The search evaluation of KNote Atlas (delivery 0.6, chapter 18): the test queries of the note «Search test queries»,
// evaluated one by one by the app (D4) by words and by meaning in the indexes chosen, and the report saved as a note.
// ProjectManager: it measures the search of the repository, it is not an everyday search.

[KntAuthorize(EnumRoles.ProjectManager)]
public class KntSemanticSearchGetSearchEvaluationAsyncCommand : KntCommandServiceBase<Result<SearchEvaluationSetupDto>>
{
    public KntSemanticSearchGetSearchEvaluationAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<SearchEvaluationSetupDto>> Execute()
    {
        var result = new Result<SearchEvaluationSetupDto>();

        var note = await KntSearchEvaluationNotes.TestQueriesNoteAsync(Service, result);
        if (note == null)
            return result;

        var (queries, issues) = SearchTestQueries.Parse(note.Description);
        var resNotes = await Service.Notes.GetFilterMinimalAsync(new NotesFilterDto());
        if (!resNotes.IsValid)
        {
            result.AddListErrorMessage(resNotes.ListErrorMessage);
            return result;
        }
        var numbers = resNotes.Entity.Select(n => n.NoteNumber).ToHashSet();
        foreach (var query in queries)
            foreach (var missing in query.ExpectedNotes.Where(n => !numbers.Contains(n)))
                issues.Add($"Row {query.Row}: the note #{missing} does not exist.");

        var resModels = await Service.SystemValues.GetRepositoryAiModelsAsync();
        if (!resModels.IsValid)
        {
            result.AddListErrorMessage(resModels.ListErrorMessage);
            return result;
        }
        var currentKey = resModels.Entity.GetIndexKey();
        var resIndexes = await Repository.SemanticIndex.GetIndexModelsAsync();
        if (!resIndexes.IsValid)
        {
            result.AddListErrorMessage(resIndexes.ListErrorMessage);
            return result;
        }

        result.Entity = new SearchEvaluationSetupDto
        {
            TestQueriesNoteId = note.NoteId,
            TestQueriesNoteNumber = note.NoteNumber,
            Queries = queries,
            Issues = issues,
            QueryInstruction = resModels.Entity.QueryInstruction,
            Indexes = resIndexes.Entity
                .Select(index =>
                {
                    var (model, dimensions) = RepositoryAiModelsDto.ParseIndexKey(index.IndexKey);
                    return new SearchEvaluationIndexDto
                    {
                        IndexKey = index.IndexKey,
                        Model = model,
                        Dimensions = dimensions,
                        IsCurrent = index.IndexKey == currentKey,
                        Available = Service.EmbeddingGenerators?.Find(model) != null,
                        Notes = index.Notes,
                        Chunks = index.Chunks
                    };
                })
                .OrderByDescending(i => i.IsCurrent)
                .ThenBy(i => i.IndexKey)
                .ToList()
        };
        return result;
    }
}

// A test query by words (the search of always, in topic and description, D5: in the order the list shows it) and by
// meaning in each index (the search of 0.4: its minimum and maximum), with the repository's query instruction and, if
// asked, without it (D3). The index is measured as it is: nothing is brought up to date. A way that fails (e.g. the model
// of an index does not answer) keeps its error and the others go on.
[KntAuthorize(EnumRoles.ProjectManager)]
public class KntSemanticSearchEvaluateQueryAsyncCommand : KntCommandServiceBase<SearchEvaluationQueryRequestDto, Result<SearchEvaluationQueryResultDto>>
{
    private readonly CancellationToken _cancellationToken;

    public KntSemanticSearchEvaluateQueryAsyncCommand(IKntService service, SearchEvaluationQueryRequestDto request, CancellationToken cancellationToken)
        : base(service, request)
    {
        _cancellationToken = cancellationToken;
    }

    public override Result ValidateParam()
    {
        var result = new Result();
        if (string.IsNullOrWhiteSpace(Param?.Query?.Query))
            result.AddErrorMessage("The test query is required.");
        else if (Param.Query.ExpectedNotes == null || Param.Query.ExpectedNotes.Count == 0)
            result.AddErrorMessage("The test query has no expected notes.");
        return result;
    }

    public override async Task<Result<SearchEvaluationQueryResultDto>> Execute()
    {
        var result = new Result<SearchEvaluationQueryResultDto>();
        var query = Param.Query;
        var expected = query.ExpectedNotes.ToHashSet();
        var evaluation = new SearchEvaluationQueryResultDto { Query = query };

        var excluded = await KntSystemNotes.GetSystemNoteTypeIdsAsync(Repository);
        evaluation.Runs.Add(await ByWordsAsync(query, expected, excluded));

        var notes = await KntSemanticSearcher.AllowedNotesAsync(Service, null, result);
        if (notes == null)
            return result;
        var resModels = await Service.SystemValues.GetRepositoryAiModelsAsync();
        if (!resModels.IsValid)
        {
            result.AddListErrorMessage(resModels.ListErrorMessage);
            return result;
        }
        var models = resModels.Entity;
        var instructions = new List<bool> { true };
        if (Param.AlsoWithoutInstruction && !string.IsNullOrWhiteSpace(models.QueryInstruction))
            instructions.Add(false);

        foreach (var indexKey in (Param.IndexKeys ?? new List<string>()).Where(k => !string.IsNullOrWhiteSpace(k)).Distinct())
        {
            var (model, dimensions) = RepositoryAiModelsDto.ParseIndexKey(indexKey);
            var generator = Service.EmbeddingGenerators?.Find(model);
            KntSemanticIndexCache.Snapshot snapshot = null;
            foreach (var withInstruction in instructions)
            {
                var run = new SearchEvaluationRunDto
                {
                    Variant = SearchEvaluationReport.VariantName(EnumNotesSearchMode.Meaning, indexKey, withInstruction),
                    Mode = EnumNotesSearchMode.Meaning,
                    IndexKey = indexKey,
                    WithInstruction = withInstruction
                };
                evaluation.Runs.Add(run);
                if (generator == null)
                {
                    run.Error = $"This installation has no embeddings provider with the model {model}.";
                    continue;
                }
                try
                {
                    snapshot ??= await KntSemanticSearcher.VectorsAsync(Service, new KntSemanticSearcher.Index(models, indexKey, generator), _cancellationToken);
                    var clock = Stopwatch.StartNew();
                    var text = SemanticSearchRules.QueryText(withInstruction ? models.QueryInstruction : null, query.Query);
                    var vectors = await generator.GenerateAsync(new[] { text }, _cancellationToken);
                    if (vectors == null || vectors.Count != 1)
                        throw new KntServiceException($"The model gave {vectors?.Count ?? 0} vectors for 1 text.");
                    var vector = EmbeddingVector.Normalize(vectors[0], dimensions);
                    var matches = SemanticSearchRules.Rank(snapshot.Chunks, vector, SemanticSearchRules.MinSimilarity, notes.ContainsKey)
                        .Take(SemanticSearchRules.MaxResults)
                        .ToList();
                    run.Milliseconds = (int)clock.ElapsedMilliseconds;
                    Measure(run, matches.Select(m => notes[m.NoteId].NoteNumber).ToList(), expected);
                    run.Similarities = matches.Select(m => m.Similarity).ToList();
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    run.Error = ex.Message;
                }
            }
        }

        result.Entity = evaluation;
        return result;
    }

    private async Task<SearchEvaluationRunDto> ByWordsAsync(SearchTestQueryDto query, HashSet<int> expected, List<Guid> excluded)
    {
        var run = new SearchEvaluationRunDto { Variant = SearchEvaluationReport.VariantName(EnumNotesSearchMode.Words), Mode = EnumNotesSearchMode.Words };
        try
        {
            var clock = Stopwatch.StartNew();
            var resWords = await Service.Notes.GetSearchMinimalAsync(new NotesSearchDto
            {
                TextSearch = query.QueryByWords,
                SearchInDescription = true,
                ExcludeNoteTypeIds = excluded
            });
            run.Milliseconds = (int)clock.ElapsedMilliseconds;
            if (!resWords.IsValid)
                run.Error = resWords.ErrorMessage;
            else
                Measure(run, resWords.Entity.Select(n => n.NoteNumber).ToList(), expected);
        }
        catch (Exception ex)
        {
            run.Error = ex.Message;
        }
        return run;
    }

    // The results, the position (from 1) of the first expected note (null, none) and the expected notes in the first ones.
    private static void Measure(SearchEvaluationRunDto run, List<int> noteNumbers, HashSet<int> expected)
    {
        run.Results = noteNumbers.Count;
        var index = noteNumbers.FindIndex(expected.Contains);
        run.Position = index < 0 ? null : index + 1;
        run.ExpectedInTop = noteNumbers.Take(SearchEvaluationReport.TopResults).Where(expected.Contains).Distinct().Count();
    }
}

[KntAuthorize(EnumRoles.ProjectManager)]
public class KntSemanticSearchSaveSearchEvaluationReportAsyncCommand : KntCommandServiceBase<SearchEvaluationReportDto, Result<NoteDto>>
{
    public KntSemanticSearchSaveSearchEvaluationReportAsyncCommand(IKntService service, SearchEvaluationReportDto report) : base(service, report)
    {

    }

    public override Result ValidateParam()
    {
        var result = new Result();
        if (string.IsNullOrWhiteSpace(Param?.Markdown))
            result.AddErrorMessage("The report is required.");
        return result;
    }

    public override async Task<Result<NoteDto>> Execute()
    {
        var result = new Result<NoteDto>();
        result.Entity = await KntSearchEvaluationNotes.SaveReportAsync(Service, Param.Markdown, result);
        return result;
    }
}
