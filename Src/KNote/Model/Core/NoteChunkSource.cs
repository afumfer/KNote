using System;
using System.Collections.Generic;
using System.Linq;
using KNote.Model.Dto;

namespace KNote.Model;

/// <summary>
/// What of a note goes to the semantic index (the input of NoteChunkBuilder), independent of how the note is read.
/// Description is Markdown: a note written in HTML comes already converted (the indexer does it).
/// </summary>
public class NoteChunkSource
{
    public string Topic { get; set; }
    public string NoteType { get; set; }
    public string FolderPath { get; set; }
    public string Tags { get; set; }
    public List<NoteChunkAttribute> Attributes { get; set; } = new();
    public string Description { get; set; }
    public List<NoteChunkTask> Tasks { get; set; } = new();
    public List<NoteChunkResource> Resources { get; set; } = new();

    public static NoteChunkSource From(NoteExtendedDto note, string folderPath, string description) => new()
    {
        Topic = note.Topic,
        NoteType = note.NoteTypeDto?.Name,
        FolderPath = folderPath,
        Tags = note.Tags,
        Attributes = (note.KAttributesDto ?? new())
            .OrderBy(a => a.Order).ThenBy(a => a.Name)
            .Select(a => new NoteChunkAttribute(a.Name, a.Value))
            .ToList(),
        Description = description,
        Tasks = (note.Tasks ?? new())
            .Select(t => new NoteChunkTask(t.NoteTaskId, t.GetTopic(200), t.Description, t.Resolved))
            .ToList(),
        Resources = (note.Resources ?? new())
            .OrderBy(r => r.Order).ThenBy(r => r.NameOut)
            .Select(r => new NoteChunkResource(r.ResourceId, r.NameOut, r.FileType, r.Description))
            .ToList()
    };
}

public record NoteChunkAttribute(string Name, string Value);

// Topic: the one the lists of tasks show (NoteTaskDto.GetTopic: its tags or the first line of its description).
public record NoteChunkTask(Guid TaskId, string Topic, string Description, bool Resolved);

// Name: the file name without the note's prefix (ResourceInfoDto.NameOut).
public record NoteChunkResource(Guid ResourceId, string Name, string FileType, string Description);
