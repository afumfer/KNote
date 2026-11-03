using System;
using System.Threading;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Service.Interfaces;

/// <summary>
/// The semantic index of the repository and the search by meaning (KNote Atlas). The index is
/// of the repository's embedding model (SystemValues.GetRepositoryAiModelsAsync) and is calculated with this
/// installation's generator of that model (IKntService.EmbeddingGenerators). See KntSemanticIndexer.
/// </summary>
public interface IKntSemanticSearchService
{
    /// <summary>How the index of the repository's model is, and the indexes of other models kept in the repository.</summary>
    Task<Result<SemanticIndexStatusDto>> GetIndexStatusAsync();

    /// <summary>
    /// Creates or updates the index (its pending notes) or, with Rebuild, calculates every vector again. Long: its
    /// progress is reported and it can be canceled (what is done is kept). Admin.
    /// </summary>
    Task<Result<SemanticIndexProgressDto>> RunIndexAsync(SemanticIndexRunRequest request);

    /// <summary>
    /// Brings the index up to date before a search: some pending notes, within a limit, and nothing while another run
    /// goes on. Any user.
    /// </summary>
    Task<Result<SemanticIndexProgressDto>> RefreshPendingAsync(SemanticIndexRefreshRequest request);

    /// <summary>Removes the index of another model than the repository's (that one is rebuilt, not removed). Admin.</summary>
    Task<Result> DeleteModelIndexAsync(string indexKey);

    /// <summary>
    /// Searches by meaning: the notes most similar to the text (at least SemanticSearchRules.MinSimilarity, at most
    /// SemanticSearchRules.MaxResults), the most similar first, among every note of the index or, with a filter, among
    /// the ones that meet it. Before, it brings the index up to date (as RefreshPendingAsync). Not paged. Any user.
    /// </summary>
    Task<Result<SemanticSearchResultDto>> SearchAsync(SemanticSearchRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// The notes most similar to a note (at least SemanticSearchRules.MinSimilarNotes, at most MaxSimilarNotes), not
    /// related to it yet. Before, the note is brought up to date if it is pending. Not saved anywhere. Any user.
    /// </summary>
    Task<Result<SemanticSearchResultDto>> GetSimilarNotesAsync(Guid noteId, CancellationToken cancellationToken = default);
}

public class SemanticIndexRunRequest
{
    public bool Rebuild { get; set; }
    public IProgress<SemanticIndexProgressDto> Progress { get; set; }
    public CancellationToken CancellationToken { get; set; }

    public override string ToString() => $"Rebuild: {Rebuild}";
}

public class SemanticIndexRefreshRequest
{
    // D8: up to 50 notes or about 10 seconds, whichever comes first.
    public const int DefaultMaxNotes = 50;
    public static readonly TimeSpan DefaultMaxDuration = TimeSpan.FromSeconds(10);

    public int MaxNotes { get; set; } = DefaultMaxNotes;
    public TimeSpan MaxDuration { get; set; } = DefaultMaxDuration;
    public CancellationToken CancellationToken { get; set; }

    public override string ToString() => $"MaxNotes: {MaxNotes}, MaxDuration: {MaxDuration}";
}
