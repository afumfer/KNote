using KNote.Ai;
using KNote.ClientWin.Core;
using KNote.Model;
using Microsoft.Extensions.AI;

namespace KNote.ClientWin.Tests.Helpers;

/// <summary>
/// Builds the assistant's chat client the same way KNoteAIAssistantCtrl.ApplyProvider does - KNote.Ai's
/// AiChatClientFactory with KNoteAiTools hosted by ClientWin (KNoteAiToolsHost) - over an in-memory SQLite
/// ServiceRef and an empty Store, for the smoke tests that go through the production path.
/// </summary>
internal static class TestAiChatClientFactory
{
    public static IChatClient Create(AiProviderRef providerRef)
    {
        var tools = new KNoteAiTools(TestServiceRefFactory.CreateInMemorySqlite().Service,
            new KNoteAiToolsHost(TestStoreFactory.CreateEmpty()));

        return AiChatClientFactory.Create(providerRef, tools.GetTools());
    }
}
