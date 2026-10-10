using System.Runtime.CompilerServices;

// Lets ClientWin.Tests exercise AiChatClientFactory.ResolveApiKey without making it public API.
[assembly: InternalsVisibleTo("KNote.ClientWin.Tests")]
