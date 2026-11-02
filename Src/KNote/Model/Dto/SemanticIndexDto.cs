using System;

namespace KNote.Model.Dto;

/// <summary>
/// What a fragment of the semantic index comes from (NoteChunks.SourceType; persisted by value, never renumber).
/// </summary>
public enum NoteChunkSourceType
{
    Data = 0,
    Text = 1,
    Task = 2,
    Resource = 3
}

/// <summary>
/// A row of the semantic index (NoteChunks, IKntSemanticIndexRepository): a fragment of a note (NoteChunkBuilder) with
/// its vector, calculated with Model. SourceId is the task or the resource of the fragment, Guid.Empty for the note's
/// own fragments (data and text). Vector is null where only the content is read (GetChunkContentsAsync).
/// </summary>
public class NoteChunkDto
{
    public Guid NoteChunkId { get; set; }
    public Guid NoteId { get; set; }
    public NoteChunkSourceType SourceType { get; set; }
    public Guid SourceId { get; set; }
    public int ChunkIndex { get; set; }
    public string Content { get; set; }
    public string ContentHash { get; set; }
    public string SourceHash { get; set; }
    public bool Stale { get; set; }
    public string Model { get; set; }
    public int Dimensions { get; set; }
    public float[] Vector { get; set; }
    public DateTime IndexedDateTime { get; set; }
}

/// <summary>
/// The part of a row of the semantic index the search keeps in memory (IKntSemanticIndexRepository.GetVectorsAsync).
/// </summary>
public class NoteChunkVectorDto
{
    public Guid NoteChunkId { get; set; }
    public Guid NoteId { get; set; }
    public NoteChunkSourceType SourceType { get; set; }
    public float[] Vector { get; set; }
}

/// <summary>
/// How the semantic index of a model is (IKntSemanticIndexRepository.GetIndexWatermarkAsync): the number of rows and the
/// last time one was written, which tell the cache of vectors whether it has to read anything again.
/// LastIndexedDateTime is null without rows.
/// </summary>
public class SemanticIndexWatermarkDto
{
    public int Count { get; set; }
    public DateTime? LastIndexedDateTime { get; set; }
}
