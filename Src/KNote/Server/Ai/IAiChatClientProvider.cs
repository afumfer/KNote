using System.Collections.Generic;
using KNote.Ai;
using KNote.Model;
using Microsoft.Extensions.AI;

namespace KNote.Server.Ai;

// Builds the IChatClient of a provider for AiAssistantController. Registered in DI (singleton) so the in-process
// tests can replace it with a scripted client instead of calling a real provider.
public interface IAiChatClientProvider
{
    IChatClient Create(AiProviderRef providerRef, IEnumerable<AITool> tools);
}

public class AiChatClientProvider : IAiChatClientProvider
{
    public IChatClient Create(AiProviderRef providerRef, IEnumerable<AITool> tools) =>
        AiChatClientFactory.Create(providerRef, tools);
}
