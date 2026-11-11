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

    // KNote Atlas, stage 4 (D18): also matches each token against the description and the text read of the note's resources
    // (also the transcriptions of the scanned pages), but not of the notes of these types (ResourcesExcludedNoteTypeIds, the
    // system ones, set by the Service): their resources are the attachments of the conversations.
    public bool SearchInResources { get; set; }

    public List<Guid> ResourcesExcludedNoteTypeIds { get; set; }

    // By words (this search) or by meaning (KNote Atlas, IKntSemanticSearchService.SearchAsync with TextSearch): who
    // searches chooses which one to call. The repository's search by words ignores it.
    public EnumNotesSearchMode Mode { get; set; }

    // Leaves out the notes of these NoteTypes (e.g. the AI assistant's own sessions and the chat room threads, see
    // KntConst.ChatSessionsTag and KntConst.ChatRoomTag).
    public List<Guid> ExcludeNoteTypeIds { get; set; }

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
    // KNote Atlas, stage 4 (4.6a): the options of the search of the Web, as ClientWin's.
    public bool SearchInDescription { get; set; }
    public bool SearchInNoteTasks { get; set; }
    public bool SearchInResources { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public string OrderColumn { get; set; }
    public bool OrderAscending { get; set; } = true;
}
