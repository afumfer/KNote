using System;
using System.Collections.Generic;

namespace KNote.Model.Dto;

/// <summary>
/// A test query of the search (KNote Atlas, the note «Search test queries»): what a person searches, the notes that
/// should be found (their numbers) and, optionally, the words that person would search by words (WordsQuery; empty,
/// the query itself). Row is its row in the note's table, to tell the user where a problem is.
/// </summary>
public class SearchTestQueryDto
{
    public int Row { get; set; }
    public string Query { get; set; }
    public List<int> ExpectedNotes { get; set; } = new();
    public string WordsQuery { get; set; }

    public string QueryByWords => string.IsNullOrWhiteSpace(WordsQuery) ? Query : WordsQuery;
}

/// <summary>An index of the repository that can be evaluated: its key, model and dimensions, and its size.</summary>
public class SearchEvaluationIndexDto
{
    public string IndexKey { get; set; }
    public string Model { get; set; }
    public int? Dimensions { get; set; }
    // The index of the repository's embedding model.
    public bool IsCurrent { get; set; }
    // This installation has its model: the vector of each query can be calculated.
    public bool Available { get; set; }
    public int Notes { get; set; }
    public int Chunks { get; set; }
}

/// <summary>
/// What a search evaluation starts from: the note of the test queries (created the first time), its queries and the
/// problems found in them, the indexes to choose and the repository's query instruction.
/// </summary>
public class SearchEvaluationSetupDto
{
    public Guid TestQueriesNoteId { get; set; }
    public int TestQueriesNoteNumber { get; set; }
    public List<SearchTestQueryDto> Queries { get; set; } = new();
    public List<string> Issues { get; set; } = new();
    public List<SearchEvaluationIndexDto> Indexes { get; set; } = new();
    public string QueryInstruction { get; set; }
}

/// <summary>One test query to evaluate: by words, and by meaning in each index (also without the instruction, if asked).</summary>
public class SearchEvaluationQueryRequestDto
{
    public SearchTestQueryDto Query { get; set; }
    public List<string> IndexKeys { get; set; } = new();
    public bool AlsoWithoutInstruction { get; set; }

    public override string ToString() => $"Row: {Query?.Row}, Indexes: {string.Join(", ", IndexKeys ?? new())}, AlsoWithoutInstruction: {AlsoWithoutInstruction}";
}

/// <summary>
/// One way of searching in an evaluation: by words, or by meaning in an index, with or without the query instruction.
/// Variant is its name in the report (SearchEvaluationReport.VariantName).
/// </summary>
public class SearchEvaluationRunDto
{
    public string Variant { get; set; }
    public EnumNotesSearchMode Mode { get; set; }
    public string IndexKey { get; set; }
    public bool WithInstruction { get; set; }
    // Position (from 1) of the first expected note among the results; null, not found.
    public int? Position { get; set; }
    public int Results { get; set; }
    // How many of the expected notes are among the first SearchEvaluationReport.TopResults results.
    public int ExpectedInTop { get; set; }
    // By meaning: the similarity of each result, in their order (the one of the first expected note is the one at
    // Position); by words, empty.
    public List<float> Similarities { get; set; } = new();
    // The time of the query (by meaning, its vector and the ranking; not the load of the index's vectors).
    public int Milliseconds { get; set; }
    // Why it could not be done (e.g. the model of the index did not answer).
    public string Error { get; set; }
}

/// <summary>The result of a test query in each way of searching.</summary>
public class SearchEvaluationQueryResultDto
{
    public SearchTestQueryDto Query { get; set; }
    public List<SearchEvaluationRunDto> Runs { get; set; } = new();
}

/// <summary>The report of an evaluation (Markdown, SearchEvaluationReport.Markdown), to save it as a note.</summary>
public class SearchEvaluationReportDto
{
    public string Markdown { get; set; }
}
