using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Core;

public class NotesFilterWithServiceRef
{
    public NotesFilterDto NotesFilter { get; set; }
    public ServiceRef ServiceRef { get; set; }
}
