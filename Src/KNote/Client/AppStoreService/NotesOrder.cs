using KNote.Model.Core;

namespace KNote.Client.AppStoreService;

/// <summary>Order of a notes list (Pages/Notes/NotesGrid): one of NoteOrderCriteria.SortableColumns and its direction.</summary>
public record NotesOrder(string Column, bool Ascending)
{
    public static NotesOrder From(NoteOrderCriteria criteria) => new(criteria.EffectiveColumn, criteria.Ascending);
}
