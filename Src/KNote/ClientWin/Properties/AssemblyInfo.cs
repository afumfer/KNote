using System.Runtime.CompilerServices;

// Lets ClientWin.Tests exercise a few internal test seams (KNoteAIAssistantCtrl.SetChatClientForTesting,
// AppInfoAlarmRowMaintenance.PruneAsync) without making them public API.
[assembly: InternalsVisibleTo("KNote.ClientWin.Tests")]
