using System;
using System.ComponentModel.DataAnnotations;

namespace KNote.Repository.EntityFramework.Entities;

/// <summary>
/// A row of the semantic index of KNote Atlas (schema revision 7, see KntSchemaUpdater and NoteChunkDto). Without
/// Timestamp: the rows of a note are never edited one by one, they are all replaced. Its foreign key to Notes is a
/// cascade delete (ModelBuilderExtensions).
/// </summary>
public class NoteChunk
{
    [Key]
    public Guid NoteChunkId { get; set; }

    public Guid NoteId { get; set; }

    public int SourceType { get; set; }

    // Guid.Empty for the note's own fragments (not null: see the unique index in ModelBuilderExtensions).
    public Guid SourceId { get; set; }

    public int ChunkIndex { get; set; }

    // The page of a fragment of a document (KNote Atlas, stage 4, D8); null in the others.
    public int? Page { get; set; }

    [Required]
    public string Content { get; set; }

    [Required]
    [MaxLength(64)]
    public string ContentHash { get; set; }

    [Required]
    [MaxLength(64)]
    public string SourceHash { get; set; }

    public bool Stale { get; set; }

    [Required]
    [MaxLength(256)]
    public string Model { get; set; }

    public int Dimensions { get; set; }

    [Required]
    public byte[] Vector { get; set; }

    public DateTime IndexedDateTime { get; set; }
}
