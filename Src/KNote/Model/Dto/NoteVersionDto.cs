using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KNote.Model.Dto;

/// <summary>
/// How a note's own data was (H6): what KLogs.PreviousContent keeps, as JSON, of the rows that changed it. It is not a table:
/// each version is a read of the PreviousContent of one row. Its tasks, resources, alarms and relations have no versions.
/// The attributes are kept by id with their value, only those with one (an attribute without a value is an empty one).
/// </summary>
public class NoteVersionDto
{
    public const int CurrentFormat = 1;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public int Format { get; set; } = CurrentFormat;
    public string Topic { get; set; }
    public string ContentType { get; set; }
    public string Tags { get; set; }
    public Guid? NoteTypeId { get; set; }
    public Guid FolderId { get; set; }
    public int Priority { get; set; }
    public string Script { get; set; }
    public List<NoteVersionAttributeDto> Attributes { get; set; } = new();
    // Last, so that the small data is read first in the JSON.
    public string Description { get; set; }

    public static NoteVersionDto From(NoteDto note) => note == null ? null : new NoteVersionDto
    {
        Topic = note.Topic,
        ContentType = note.ContentType,
        Tags = note.Tags,
        NoteTypeId = note.NoteTypeId,
        FolderId = note.FolderId,
        Priority = note.Priority,
        Script = note.Script,
        Attributes = (note.KAttributesDto ?? new List<NoteKAttributeDto>())
            .Where(a => !string.IsNullOrEmpty(a.Value))
            .GroupBy(a => a.KAttributeId)
            .Select(g => new NoteVersionAttributeDto { Id = g.Key, Value = g.Last().Value })
            .ToList(),
        Description = note.Description
    };

    /// <summary>The value of an attribute, or null if it has none.</summary>
    public string AttributeValue(Guid kattributeId) =>
        Attributes?.FirstOrDefault(a => a.Id == kattributeId)?.Value;

    public NoteVersionDto Copy()
    {
        var copy = (NoteVersionDto)MemberwiseClone();
        copy.Attributes = (Attributes ?? new()).Select(a => new NoteVersionAttributeDto { Id = a.Id, Value = a.Value }).ToList();
        return copy;
    }

    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

    /// <summary>The version kept in a row, or null if it has none or it can't be read.</summary>
    public static NoteVersionDto FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;
        try
        {
            var version = JsonSerializer.Deserialize<NoteVersionDto>(json, JsonOptions);
            if (version != null)
                version.Attributes ??= new();
            return version;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

public class NoteVersionAttributeDto
{
    public Guid Id { get; set; }
    public string Value { get; set; }
}
