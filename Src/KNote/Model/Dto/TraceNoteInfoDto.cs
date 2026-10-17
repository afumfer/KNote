using System;
using System.Collections.Generic;

namespace KNote.Model.Dto;

// A trace note (relation) of a note as the web app's "Trace notes" tab shows it: the relation itself plus the
// OTHER note in it (RelatedNote*) and the name of its type, resolved by the server (api/notes/{id}/tracenotes) with
// the same rules as ClientWin's note editor (TraceNoteRows). The display fields are read only: it is saved back as
// the TraceNoteDto it is.
public class TraceNoteInfoDto : TraceNoteDto
{
    public Guid RelatedNoteId { get; set; }

    // "#123", or "?" when the related note no longer exists.
    public string RelatedNoteNumber { get; set; }

    public string RelatedNoteTopic { get; set; }

    public string RelatedNoteTags { get; set; }

    // Empty for an untyped relation.
    public string TraceNoteTypeName { get; set; }
}

// The trace notes of a note, in both directions (as NoteExtendedDto keeps them).
public class NoteTraceNotesDto
{
    // Incoming relations: this note is ToId, the related note is FromId.
    public List<TraceNoteInfoDto> TraceNotesFrom { get; set; } = new();

    // Outgoing relations: this note is FromId, the related note is ToId.
    public List<TraceNoteInfoDto> TraceNotesTo { get; set; } = new();
}
