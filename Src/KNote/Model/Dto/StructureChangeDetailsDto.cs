using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KNote.Model.Dto;

/// <summary>
/// What a change of the structure of a repository changed (KLogs.ChangeDetails of a folder, a note type, an attribute, a type of
/// relation, a user or a setting, H.2b), as JSON: created or deleted, its fields with the value before and after (none for the
/// private ones: the email of a user, the value of a generic setting, H15; never a password), and the lists that changed (the
/// values of an attribute, the note types and folders of the noise policy). Built and read by StructureChangeDescriber.
/// </summary>
public class StructureChangeDetailsDto
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public bool? Created { get; set; }
    public bool? Deleted { get; set; }
    public List<HistoryFieldChangeDto> Fields { get; set; }
    public List<HistoryListChangeDto> Lists { get; set; }

    [JsonIgnore]
    public bool IsEmpty => Created != true && Deleted != true && !(Fields?.Count > 0) && !(Lists?.Count > 0);

    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

    public static StructureChangeDetailsDto FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new StructureChangeDetailsDto();
        try
        {
            return JsonSerializer.Deserialize<StructureChangeDetailsDto>(json, JsonOptions) ?? new StructureChangeDetailsDto();
        }
        catch (JsonException)
        {
            return new StructureChangeDetailsDto();
        }
    }
}

/// <summary>A list that changed, by the names of its items: added, removed and renamed (before → after).</summary>
public class HistoryListChangeDto
{
    public string Field { get; set; }
    public List<string> Added { get; set; }
    public List<string> Removed { get; set; }
    public List<HistoryValueChangeDto> Renamed { get; set; }
}
