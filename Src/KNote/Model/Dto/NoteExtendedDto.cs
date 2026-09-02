using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Model.Dto;

public class NoteExtendedDto : NoteDto
{
    private List<ResourceDto> _resources;
    public List<ResourceDto> Resources
    {
        get
        {
            if (_resources == null)
                _resources = new List<ResourceDto>();
            return _resources;
        }
        set
        {
            _resources = value;
        }
    }

    private List<NoteTaskDto> _tasks;
    public List<NoteTaskDto> Tasks
    {
        get
        {
            if (_tasks == null)
                _tasks = new List<NoteTaskDto>();
            return _tasks;
        }
        set
        {
            _tasks = value;
        }
    }

    private List<KMessageDto> _messages;
    public List<KMessageDto> Messages
    {
        get
        {
            if (_messages == null)
                _messages = new List<KMessageDto>();
            return _messages;
        }
        set
        {
            _messages = value;
        }
    }

    // Incoming relations (ToId == this note - "who points to me"). Fase 0, decision 4.
    private List<TraceNoteDto> _traceNotesFrom;
    public List<TraceNoteDto> TraceNotesFrom
    {
        get
        {
            if (_traceNotesFrom == null)
                _traceNotesFrom = new List<TraceNoteDto>();
            return _traceNotesFrom;
        }
        set
        {
            _traceNotesFrom = value;
        }
    }

    // Outgoing relations (FromId == this note - "who I point to"). Fase 0, decision 4.
    private List<TraceNoteDto> _traceNotesTo;
    public List<TraceNoteDto> TraceNotesTo
    {
        get
        {
            if (_traceNotesTo == null)
                _traceNotesTo = new List<TraceNoteDto>();
            return _traceNotesTo;
        }
        set
        {
            _traceNotesTo = value;
        }
    }
}
