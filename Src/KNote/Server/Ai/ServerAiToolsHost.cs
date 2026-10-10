using System.Collections.Concurrent;
using System.Threading.Tasks;
using KNote.Ai;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.Server.Ai;

// Server's side of KNoteAiTools.create_task: the note goes to the repository's home folder, and the notes created
// are queued for AiAssistantController, which tells the Web client about them (AiChatStreamEventTypes.NoteCreated)
// in the answer's stream. One per chat request.
public class ServerAiToolsHost : IKNoteAiToolsHost
{
    private readonly IKntService _service;

    public ServerAiToolsHost(IKntService service)
    {
        _service = service;
    }

    public ConcurrentQueue<NoteExtendedDto> CreatedNotes { get; } = new();

    public async Task<AiNoteDestination> GetNewNoteDestinationAsync()
    {
        var home = await _service.Folders.GetHomeAsync();
        if (!home.IsValid || home.Entity == null)
            return null;

        return new AiNoteDestination(_service, home.Entity);
    }

    public void OnNoteCreated(IKntService service, NoteExtendedDto note) => CreatedNotes.Enqueue(note);
}
