using System.Runtime.CompilerServices;

// Lets ClientWin.Tests exercise a couple of internal test seams (AiChatClientFactory.ResolveApiKey,
// KNoteAIAssistantCtrl.SetChatClientForTesting) without making them public API.
[assembly: InternalsVisibleTo("KNote.ClientWin.Tests")]
