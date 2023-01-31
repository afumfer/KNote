using KNote.Service.Core;

namespace KNote.ClientWin.Core;

public class ServiceWithNoteId
{
    public IKntService Service { get; set; }
    public Guid NoteId { get; set; }

}
