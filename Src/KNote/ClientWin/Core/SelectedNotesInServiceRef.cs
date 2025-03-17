using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Core;

public class SelectedNotesInServiceRef
{
    public ServiceRef ServiceRef { get; set; }
    public NotesSearchDto NotesSearch { get; set; }
    public NotesFilterDto NotesFilter { get; set; }
}
