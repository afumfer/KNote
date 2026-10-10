using System;
using System.Collections.Generic;

namespace KNote.Model.Dto;

// The Web AI assistant API (Server's AiAssistantController).

// A configured AI provider as the client sees it: never its API key or host.
public class AiProviderInfoDto
{
    public string Alias { get; set; }

    // EnumAiProvider value.
    public string Provider { get; set; }

    public string Model { get; set; }

    // The provider used when none is chosen (the first one configured).
    public bool IsDefault { get; set; }
}

// One question to the assistant, with the conversation so far (the server keeps no conversation state).
public class AiChatRequestDto
{
    // Configured provider to use; the default one when empty.
    public string ProviderAlias { get; set; }

    public List<AiChatTurnDto> History { get; set; } = new();

    public string Prompt { get; set; }
}

// Each server-sent event of a streamed answer (its SSE event type is Type, see AiChatStreamEventTypes).
public class AiChatStreamEventDto
{
    public string Type { get; set; }

    // Delta: the next piece of the answer. Tool: the name of the tool being called. Error: the message.
    public string Text { get; set; }

    // NoteCreated: the note the create_task tool has just saved.
    public Guid? NoteId { get; set; }

    public int? NoteNumber { get; set; }

    // Completed: the whole turn, with its usage.
    public AiChatTurnDto Turn { get; set; }
}

public static class AiChatStreamEventTypes
{
    public const string Delta = "delta";
    public const string Tool = "tool";
    public const string NoteCreated = "noteCreated";
    public const string Completed = "completed";
    public const string Error = "error";
}
