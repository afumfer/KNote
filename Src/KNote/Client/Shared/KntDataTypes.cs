using KNote.Model;

namespace KNote.Client.Shared;

/// <summary>A Material Symbols icon for each data type of an attribute (the attribute's dialog, the notes filter).</summary>
public static class KntDataTypes
{
    public static string Icon(EnumKAttributeDataType dataType) => dataType switch
    {
        EnumKAttributeDataType.String => "short_text",
        EnumKAttributeDataType.TextArea => "notes",
        EnumKAttributeDataType.Int => "pin",
        EnumKAttributeDataType.Double => "decimal_increase",
        EnumKAttributeDataType.DateTime => "event",
        EnumKAttributeDataType.Bool => "toggle_on",
        EnumKAttributeDataType.TabulatedValue => "list",
        EnumKAttributeDataType.TagsValue => "sell",
        _ => "label"
    };
}
