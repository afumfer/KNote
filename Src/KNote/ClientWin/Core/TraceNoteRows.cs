using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Core;

// One relation of a note's "Trace notes", described by the OTHER note in it (Number/Topic/Tags) plus the
// relation's own Type/Order/Weight. Type is empty for untyped relations (a valid, common case).
public record TraceNoteRow(Guid TraceNoteId, string Number, string Topic, string Tags, string Type, int Order, double Weight);

// Resolves trace notes into displayable rows - shared by the note editor's "Trace notes" tab and the
// note details report.
public static class TraceNoteRows
{
    // Fetched once and reused for every row - avoids re-fetching the (small, catalog-like) trace note
    // types list once per trace note.
    public static async Task<Dictionary<Guid, string>> GetTypeNamesAsync(IKntService service)
        => (await service.TraceNoteTypes.GetAllAsync()).Entity?
            .ToDictionary(t => t.TraceNoteTypeId, t => t.Name) ?? new Dictionary<Guid, string>();

    // One lookup per row (trace lists are small, per-note; not worth a batch endpoint yet). A related
    // note that no longer exists shows as "?".
    public static async Task<TraceNoteRow> ResolveAsync(IKntService service, TraceNoteDto traceNote, Guid relatedNoteId, Dictionary<Guid, string> typeNames)
    {
        var relatedNote = (await service.Notes.GetAsync(relatedNoteId)).Entity;
        var type = traceNote.TraceNoteTypeId.HasValue && typeNames.TryGetValue(traceNote.TraceNoteTypeId.Value, out var typeName) ? typeName : "";

        return new TraceNoteRow(traceNote.TraceNoteId, relatedNote != null ? "#" + relatedNote.NoteNumber : "?",
            relatedNote?.Topic, relatedNote?.Tags, type, traceNote.Order, traceNote.Weight);
    }

    // Rows for the "from" (related note = FromId) or "to" (related note = ToId) list of a note, skipping
    // relations deleted in the editor but not saved yet.
    public static async Task<List<TraceNoteRow>> ResolveAllAsync(IKntService service, IEnumerable<TraceNoteDto> traceNotes, bool fromSide, Dictionary<Guid, string> typeNames = null)
    {
        typeNames ??= await GetTypeNamesAsync(service);

        var rows = new List<TraceNoteRow>();
        foreach (var traceNote in traceNotes ?? Enumerable.Empty<TraceNoteDto>())
            if (!traceNote.IsDeleted())
                rows.Add(await ResolveAsync(service, traceNote, fromSide ? traceNote.FromId : traceNote.ToId, typeNames));
        return rows;
    }
}
