using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KNote.Model.Dto;

public class NotesSearchDto : SmartModelDtoBase
{
    public string TextSearch { get; set; }

    public bool SearchInDescription { get; set; }

    // Also matches each search token against the Tags/Description of the note's related
    // NoteTasks (1-to-n), not just the note's own Topic/Tags/Description.
    public bool SearchInNoteTasks { get; set; }

    // Leaves out the notes of this NoteType (e.g. the AI assistant's own sessions, see KntConst.ChatSessionsTag).
    public Guid? ExcludeNoteTypeId { get; set; }

    // Order of the notes: one of NoteOrderCriteria.SortableColumns (the note number breaks ties). Null, or
    // any other value, keeps the query's own order.
    public string OrderColumn { get; set; }

    public bool OrderAscending { get; set; } = true;

    public PageIdentifier PageIdentifier { get; set; } = new PageIdentifier();

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

        return results;
    }    
}

// TODO: tmp class, refactor
public class NotesSearchParam
{
    public string TextSearch { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string OrderColumn { get; set; }
    public bool OrderAscending { get; set; } = true;
}
