using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KNote.Model.Dto;

/// <summary>
/// What a change of a note changed: KLogs.ChangeDetails, as JSON. Each member is null if that part didn't change. Values are
/// as people read them (the path of a folder, the name of a type), cut to a few hundred characters; the text, by lines and by
/// sections of Markdown. Built and read by NoteChangeDescriber, which also writes the line of the row (RegistryMessage) from it.
/// </summary>
public class NoteChangeDetailsDto
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public HistoryCreatedDto Created { get; set; }
    public HistoryDeletedDto Deleted { get; set; }
    /// <summary>A version restored (H23): the row ActionType Restored, with what it changed in the other members.</summary>
    public HistoryRestoredDto Restored { get; set; }

    public HistoryValueChangeDto Topic { get; set; }
    public HistoryTextChangeDto Text { get; set; }
    public List<HistoryAttributeChangeDto> Attributes { get; set; }
    public HistoryTagsChangeDto Tags { get; set; }
    public HistoryValueChangeDto Folder { get; set; }
    public HistoryValueChangeDto NoteType { get; set; }
    public HistoryValueChangeDto Priority { get; set; }
    // The content type of the note (NoteDto.ContentType, ContentTypeExt) by its parts, as read: never its JSON.
    /// <summary>How the description is shown (markdown, html, navigation).</summary>
    public HistoryValueChangeDto DescriptionFormat { get; set; }
    /// <summary>The language of the script (knt).</summary>
    public HistoryValueChangeDto ScriptFormat { get; set; }
    /// <summary>The description blocked (true) or unblocked (false).</summary>
    public bool? DescriptionBlocked { get; set; }
    /// <summary>True if the script changed (its text is only in the version).</summary>
    public bool? Script { get; set; }

    // The parts of the note (H3): not its own data, so never in its versions (H6). NotePartChangeDescriber.
    public List<HistoryPartChangeDto> Tasks { get; set; }
    public List<HistoryPartChangeDto> Resources { get; set; }
    public List<HistoryPartChangeDto> Alarms { get; set; }
    public List<HistoryPartChangeDto> Relations { get; set; }

    [JsonIgnore]
    public EnumHistoryFields Fields
    {
        get
        {
            var fields = EnumHistoryFields.None;
            if (Topic != null) fields |= EnumHistoryFields.Topic;
            if (Text != null) fields |= EnumHistoryFields.Description;
            if (Attributes?.Count > 0) fields |= EnumHistoryFields.Attributes;
            if (Tags != null) fields |= EnumHistoryFields.Tags;
            if (Folder != null) fields |= EnumHistoryFields.Folder;
            if (NoteType != null) fields |= EnumHistoryFields.NoteType;
            if (Priority != null) fields |= EnumHistoryFields.Priority;
            if (DescriptionFormat != null || ScriptFormat != null || DescriptionBlocked != null) fields |= EnumHistoryFields.ContentType;
            if (Script == true) fields |= EnumHistoryFields.Script;
            if (Tasks?.Count > 0) fields |= EnumHistoryFields.Tasks;
            if (Resources?.Count > 0) fields |= EnumHistoryFields.Resources;
            if (Alarms?.Count > 0) fields |= EnumHistoryFields.Alarms;
            if (Relations?.Count > 0) fields |= EnumHistoryFields.Relations;
            return fields;
        }
    }

    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

    public static NoteChangeDetailsDto FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new NoteChangeDetailsDto();
        try
        {
            return JsonSerializer.Deserialize<NoteChangeDetailsDto>(json, JsonOptions) ?? new NoteChangeDetailsDto();
        }
        catch (JsonException)
        {
            return new NoteChangeDetailsDto();
        }
    }
}

public class HistoryCreatedDto
{
    public string Folder { get; set; }
    public List<string> Sections { get; set; }
}

public class HistoryRestoredDto
{
    /// <summary>The row whose version was restored, and when that change happened (the last one of its session).</summary>
    public Guid? KLogId { get; set; }
    public DateTime? VersionOf { get; set; }
    /// <summary>A deleted note recovered (H24): KLogId and VersionOf are its row Deleted.</summary>
    public bool? Recovered { get; set; }
}

public class HistoryDeletedDto
{
    public string Folder { get; set; }

    // The parts deleted with the note, not noted one by one (H14).
    public int? Tasks { get; set; }
    public int? Resources { get; set; }
    public int? Alarms { get; set; }
    public int? Relations { get; set; }
}

/// <summary>What happened to a part of a note (HistoryPartActions).</summary>
public static class HistoryPartActions
{
    public const string Added = "Added";
    public const string Changed = "Changed";
    public const string Removed = "Removed";
}

/// <summary>
/// A part of a note added, changed or removed: a task, a resource, an alarm or a relation. Name is how people know it (the
/// first line of a task, the file of a resource, the date of an alarm, #12 Topic of the other note); Changes, the fields
/// changed (HistoryPartFields), with the value before and after when it is small.
/// </summary>
public class HistoryPartChangeDto
{
    public Guid Id { get; set; }
    public string Action { get; set; }
    public string Name { get; set; }
    public List<HistoryFieldChangeDto> Changes { get; set; }
    /// <summary>A relation removed because the other note was deleted (H14).</summary>
    public bool? OtherNoteDeleted { get; set; }
}

public class HistoryFieldChangeDto
{
    public string Field { get; set; }
    public string Before { get; set; }
    public string After { get; set; }
}

public class HistoryValueChangeDto
{
    public string Before { get; set; }
    public string After { get; set; }
}

public class HistoryAttributeChangeDto
{
    public System.Guid Id { get; set; }
    public string Name { get; set; }
    public string Before { get; set; }
    public string After { get; set; }
}

public class HistoryTagsChangeDto
{
    public List<string> Added { get; set; } = new();
    public List<string> Removed { get; set; } = new();
}

public class HistoryTextChangeDto
{
    public int Added { get; set; }
    public int Removed { get; set; }
    /// <summary>Too many changes to compare them line by line: every line counts as removed and added.</summary>
    public bool Replaced { get; set; }
    /// <summary>The sections with changes, in order; a null title is the text before the first heading.</summary>
    public List<HistorySectionChangeDto> Sections { get; set; } = new();
}

public class HistorySectionChangeDto
{
    public string Title { get; set; }
    public int Added { get; set; }
    public int Removed { get; set; }
}
