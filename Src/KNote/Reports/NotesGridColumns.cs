using KNote.Model.Dto;

namespace KNote.Reports;

// The columns of the web app's notes list (Client's NotesGrid), in its order, and each cell as the list shows
// it (dates with the current culture): the web's notes list printed or exported as it is seen. The compact
// view shows the number, topic, priority and tags. A list found by meaning (NoteSearchHitDto) also has the
// similarity, after the topic (not the fragment, which would make each row long).
public static class NotesGridColumns
{
    private record Column(NotesListColumn Definition, bool InCompactView, Func<NoteMinimalDto, string> Text);

    // Widths: the grid's, with Topic (which takes the room left in the grid) as the widest.
    private static readonly Column[] Columns =
    {
        new(new NotesListColumn("NoteNumber", "#", true, 64), true, n => n.NoteNumber.ToString()),
        new(new NotesListColumn("Topic", "Topic", false, 400), true, n => n.Topic ?? ""),
        new(new NotesListColumn("Priority", "Priority", false, 96), true, n => n.Priority.ToString()),
        new(new NotesListColumn("Tags", "Tags", false, 120), true, n => n.Tags ?? ""),
        new(new NotesListColumn("InternalTags", "Status", false, 110), false, n => n.InternalTags ?? ""),
        new(new NotesListColumn("ModificationDateTime", "Modified", true, 100), false, n => n.ModificationDateTime.ToShortDateString()),
        new(new NotesListColumn("CreationDateTime", "Created", true, 100), false, n => n.CreationDateTime.ToShortDateString())
    };

    private static readonly Column SimilarityColumn =
        new(new NotesListColumn("Similarity", "Similarity", true, 80), true, n => n is NoteSearchHitDto hit ? hit.Similarity.ToString("0.00") : "");

    public static NotesListSnapshot Snapshot(IEnumerable<NoteMinimalDto> notes, bool compact)
    {
        ArgumentNullException.ThrowIfNull(notes);

        var list = notes.ToList();
        var columns = Columns.Where(c => !compact || c.InCompactView).ToList();
        if (list.Any(n => n is NoteSearchHitDto))
            columns.Insert(columns.FindIndex(c => c.Definition.Name == "Topic") + 1, SimilarityColumn);

        var snapshot = new NotesListSnapshot();
        snapshot.Columns.AddRange(columns.Select(c => c.Definition));
        foreach (var note in list)
        {
            snapshot.Rows.Add(columns.Select(c => c.Text(note)).ToArray());
            snapshot.NoteIds.Add(note.NoteId);
        }
        snapshot.LoadedCount = snapshot.Rows.Count;

        return snapshot;
    }
}
