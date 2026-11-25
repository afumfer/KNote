using System;

namespace KNote.Model.Dto;

/// <summary>
/// One write of the history, which the repository does in the same transaction as the change it notes (H10). Either a new
/// row (Row, with its version in PreviousContent), or the merge of this change into the row of its session (MergeIntoKLogId,
/// H5): Row then has the values of the merged row, and PreviousContent, if given, is set only if that row has none. If that
/// row is no longer as it was read (its LastDateTime is not ExpectedLastDateTime: another write came first), Standalone is
/// inserted instead, with StandalonePreviousContent: this change alone, as a new row. Prepared by the Service (KntHistory).
/// </summary>
public class KLogWriteDto
{
    public KLogDto Row { get; set; }
    public string PreviousContent { get; set; }

    public Guid? MergeIntoKLogId { get; set; }
    public DateTime? ExpectedLastDateTime { get; set; }

    public KLogDto Standalone { get; set; }
    public string StandalonePreviousContent { get; set; }

    /// <summary>The row of a note just created is titled by the repository, once it has its number.</summary>
    public bool TitleFromNewNote { get; set; }

    public bool WritesPreviousContent => PreviousContent != null;

    /// <summary>Titles the rows of a note just created with its number (TitleFromNewNote), once the repository gave it one.</summary>
    public void TitleNewNote(int noteNumber, string topic)
    {
        if (!TitleFromNewNote)
            return;
        var title = Core.HistoryRules.NoteTitle(noteNumber, topic);
        if (Row != null)
            Row.EntityTitle = title;
        if (Standalone != null)
            Standalone.EntityTitle = title;
    }
}
