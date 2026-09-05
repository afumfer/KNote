using System;
using System.Linq;

namespace KNote.Model.Core;

public enum NoteOrderMode
{
    Default,
    Fixed,
    Auto
}

// Parses/formats Folder.OrderNotes. The field stays a free-form string (no schema change) with an
// embedded mode indicator: "" -> Default, "FIXED|<Column>|<ASC|DESC>" -> Fixed,
// "AUTO" or "AUTO|<Column>|<ASC|DESC>" -> Auto (the second form caches the last order actually
// applied by the user in the notes selector, so it survives across sessions/reopens).
public class NoteOrderCriteria
{
    public const string DefaultColumn = "NoteNumber";

    public static readonly string[] SortableColumns =
    {
        "NoteNumber", "Topic", "Priority", "Tags", "InternalTags", "ModificationDateTime", "CreationDateTime"
    };

    public NoteOrderMode Mode { get; }
    public string Column { get; }
    public bool Ascending { get; }

    public string EffectiveColumn => Column ?? DefaultColumn;

    public NoteOrderCriteria(NoteOrderMode mode, string column, bool ascending)
    {
        Mode = mode;
        Column = column;
        Ascending = ascending;
    }

    public static NoteOrderCriteria Parse(string orderNotes)
    {
        if (string.IsNullOrWhiteSpace(orderNotes))
            return new NoteOrderCriteria(NoteOrderMode.Default, DefaultColumn, true);

        // The bare "AUTO" marker (no cached column yet) is the one value that isn't three parts.
        if (orderNotes == "AUTO")
            return new NoteOrderCriteria(NoteOrderMode.Auto, null, true);

        var parts = orderNotes.Split('|');

        // Anything else must match our format exactly - a known mode, a known column, a known
        // direction - or it's treated as unrecognized. This is deliberately strict rather than
        // "recognize the mode token and shrug off the rest": data imported from another tool, or
        // free text a user typed into this field before this feature existed, can legitimately
        // start with the literal text "AUTO" or "FIXED" by coincidence without meaning our format
        // at all. Partially matching such a value (e.g. keeping "Auto" mode with no column) would
        // silently misclassify it AND, for Auto, arm the auto-persist-on-click behavior - overwriting
        // that imported value the next time the user sorts the folder in the selector. Falling all
        // the way back to Default for anything that doesn't fully parse avoids both problems.
        if (parts.Length == 3 && SortableColumns.Contains(parts[1]) && (parts[2] == "ASC" || parts[2] == "DESC"))
        {
            if (parts[0] == "AUTO")
                return new NoteOrderCriteria(NoteOrderMode.Auto, parts[1], parts[2] == "ASC");

            if (parts[0] == "FIXED")
                return new NoteOrderCriteria(NoteOrderMode.Fixed, parts[1], parts[2] == "ASC");
        }

        return new NoteOrderCriteria(NoteOrderMode.Default, DefaultColumn, true);
    }

    public string Format()
    {
        return Mode switch
        {
            NoteOrderMode.Fixed => $"FIXED|{EffectiveColumn}|{(Ascending ? "ASC" : "DESC")}",
            NoteOrderMode.Auto when Column != null => $"AUTO|{Column}|{(Ascending ? "ASC" : "DESC")}",
            NoteOrderMode.Auto => "AUTO",
            _ => ""
        };
    }
}
