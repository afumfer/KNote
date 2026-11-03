using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using KNote.Model.Dto;

namespace KNote.Model.Core;

/// <summary>
/// Sorts a notes list in memory, as the queries of the repository sort the paged ones: by one of
/// NoteOrderCriteria.SortableColumns or, for the notes found by meaning, by Similarity; texts without case or accents;
/// the note number breaks ties. Used where the whole list is already loaded (the search by meaning in both apps).
/// </summary>
public static class NoteListSorter
{
    public const string SimilarityColumn = nameof(NoteSearchHitDto.Similarity);

    private static readonly CompareInfo Compare = CultureInfo.InvariantCulture.CompareInfo;
    private const CompareOptions TextOptions = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

    public static bool IsSortableColumn(string column) =>
        column == SimilarityColumn || NoteOrderCriteria.IsSortableColumn(column);

    /// <summary>The notes in the order asked; with an unknown column (or none), in their own order.</summary>
    public static List<T> Sort<T>(IEnumerable<T> notes, string column, bool ascending) where T : NoteMinimalDto
    {
        var list = notes?.ToList() ?? new List<T>();
        if (!IsSortableColumn(column))
            return list;

        var comparer = Comparer<T>.Create((a, b) =>
        {
            var order = CompareBy(a, b, column);
            if (!ascending)
                order = -order;
            return order != 0 ? order : a.NoteNumber.CompareTo(b.NoteNumber);
        });
        // A stable sort: equal notes keep their order.
        return list.Select((note, index) => (note, index))
            .OrderBy(x => x.note, comparer)
            .ThenBy(x => x.index)
            .Select(x => x.note)
            .ToList();
    }

    private static int CompareBy(NoteMinimalDto a, NoteMinimalDto b, string column) => column switch
    {
        "NoteNumber" => a.NoteNumber.CompareTo(b.NoteNumber),
        "Topic" => CompareText(a.Topic, b.Topic),
        "Priority" => a.Priority.CompareTo(b.Priority),
        "Tags" => CompareText(a.Tags, b.Tags),
        "InternalTags" => CompareText(a.InternalTags, b.InternalTags),
        "ModificationDateTime" => a.ModificationDateTime.CompareTo(b.ModificationDateTime),
        "CreationDateTime" => a.CreationDateTime.CompareTo(b.CreationDateTime),
        SimilarityColumn => SimilarityOf(a).CompareTo(SimilarityOf(b)),
        _ => 0
    };

    private static float SimilarityOf(NoteMinimalDto note) => note is NoteSearchHitDto hit ? hit.Similarity : 0;

    // Nulls first, as in the databases.
    private static int CompareText(string a, string b) =>
        a == null ? (b == null ? 0 : -1) : b == null ? 1 : Compare.Compare(a, b, TextOptions);
}
