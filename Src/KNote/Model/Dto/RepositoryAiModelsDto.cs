using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KNote.Model.Dto;

/// <summary>
/// The AI models a repository declares (KNote Atlas, SystemValues of scope KntConst.AiScope): the embedding model of
/// its semantic index, which every installation working with the repository must use, and the LLM recommended for its
/// AI tasks. Where each model runs (Ollama host, API key) is not here: it belongs to each installation.
/// </summary>
public class RepositoryAiModelsDto : SmartModelDtoBase
{
    public const int MaxModelLength = 256;
    public const int MaxQueryInstructionLength = 1000;
    public const int MinEmbeddingDimensions = 32;
    public const int MaxEmbeddingDimensions = 8192;

    #region Property definitions

    // Model name, as the providers call it (e.g. qwen3-embedding:0.6b). Empty: no semantic search in this repository.
    private string _embeddingModel;
    [MaxLength(MaxModelLength)]
    public string EmbeddingModel
    {
        get { return _embeddingModel; }
        set
        {
            if (_embeddingModel != value)
            {
                _embeddingModel = value;
                OnPropertyChanged("EmbeddingModel");
            }
        }
    }

    // Vector size kept in the index; null: the model's own. Fewer than the model's are cut and normalized again
    // (only valid with models trained for it, such as Qwen3-Embedding or OpenAI's text-embedding-3).
    private int? _embeddingDimensions;
    public int? EmbeddingDimensions
    {
        get { return _embeddingDimensions; }
        set
        {
            if (_embeddingDimensions != value)
            {
                _embeddingDimensions = value;
                OnPropertyChanged("EmbeddingDimensions");
            }
        }
    }

    // Task description sent before the searches (not before the notes), for the models that expect one, such as
    // Qwen3-Embedding ("Instruct: {instruction}\nQuery:{query}"). Empty: the query alone.
    private string _queryInstruction;
    [MaxLength(MaxQueryInstructionLength)]
    public string QueryInstruction
    {
        get { return _queryInstruction; }
        set
        {
            if (_queryInstruction != value)
            {
                _queryInstruction = value;
                OnPropertyChanged("QueryInstruction");
            }
        }
    }

    // Recommended LLM for summaries, knowledge base entries... (optional).
    private string _llmModel;
    [MaxLength(MaxModelLength)]
    public string LlmModel
    {
        get { return _llmModel; }
        set
        {
            if (_llmModel != value)
            {
                _llmModel = value;
                OnPropertyChanged("LlmModel");
            }
        }
    }

    // Read only (filled when read, not saved): whether this installation has an embeddings provider of EmbeddingModel.
    public bool EmbeddingModelAvailable { get; set; }

    #endregion

    // The same models (EmbeddingModelAvailable apart), compared as they are saved.
    public bool SameEmbeddingModel(RepositoryAiModelsDto other) =>
        string.Equals(Normalize(EmbeddingModel), Normalize(other?.EmbeddingModel), StringComparison.OrdinalIgnoreCase)
        && EmbeddingDimensions == other?.EmbeddingDimensions;

    /// <summary>
    /// The key of the semantic index of these models (NoteChunks.Model): the embedding model, with "@" and the
    /// dimensions when the repository asks for other ones than the model's (two indexes of a model with different
    /// dimensions are two indexes). Null without an embedding model.
    /// </summary>
    public string GetIndexKey()
    {
        var model = Normalize(EmbeddingModel);
        if (model == null)
            return null;
        return EmbeddingDimensions == null ? model : $"{model}@{EmbeddingDimensions}";
    }

    // The model and the dimensions of an index key (GetIndexKey): "qwen3-embedding:4b@1024" or "qwen3-embedding:4b".
    public static (string Model, int? Dimensions) ParseIndexKey(string indexKey)
    {
        indexKey = Normalize(indexKey);
        if (indexKey == null)
            return (null, null);
        var at = indexKey.LastIndexOf('@');
        return at > 0 && int.TryParse(indexKey[(at + 1)..], out var dimensions)
            ? (indexKey[..at], dimensions)
            : (indexKey, null);
    }

    public static string Normalize(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    #region Validations

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

        Validator.TryValidateProperty(EmbeddingModel,
           new ValidationContext(this, null, null) { MemberName = "EmbeddingModel" }, results);
        Validator.TryValidateProperty(QueryInstruction,
           new ValidationContext(this, null, null) { MemberName = "QueryInstruction" }, results);
        Validator.TryValidateProperty(LlmModel,
           new ValidationContext(this, null, null) { MemberName = "LlmModel" }, results);

        if (EmbeddingDimensions is int dimensions && (dimensions < MinEmbeddingDimensions || dimensions > MaxEmbeddingDimensions))
            results.Add(new ValidationResult(
                $"The dimensions must be between {MinEmbeddingDimensions} and {MaxEmbeddingDimensions} (or empty: the model's own).",
                new[] { "EmbeddingDimensions" }));

        if (Normalize(EmbeddingModel) == null && (EmbeddingDimensions != null || Normalize(QueryInstruction) != null))
            results.Add(new ValidationResult(
                "The dimensions and the query instruction need an embedding model.",
                new[] { "EmbeddingModel" }));

        return results;
    }

    #endregion
}

/// <summary>An AI model this installation can use (its providers), as the clients see it: never its host or key.</summary>
public class AiModelInfoDto
{
    // EnumAiProvider value.
    public string Provider { get; set; }

    public string Model { get; set; }

    // A paid service (AiProviderRef.IsPaid).
    public bool IsPaid { get; set; }
}

/// <summary>The embedding and chat models of this installation's AI providers (to choose the repository's models).</summary>
public class AiInstallationModelsDto
{
    public List<AiModelInfoDto> EmbeddingModels { get; set; } = new();

    public List<AiModelInfoDto> ChatModels { get; set; } = new();
}
