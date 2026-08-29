using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;

namespace KNote.ClientWin.Tests.Helpers;

/// <summary>Builds a minimal real Store (TestFactoryViews, no ServiceRef/DefaultFolderWithServiceRef
/// set) - enough to satisfy AiChatClientFactory.Create's signature for tests that don't exercise
/// KNoteAiTools.create_task (which is the only thing that touches Store.DefaultFolderWithServiceRef).</summary>
internal static class TestStoreFactory
{
    public static Store CreateEmpty() => new(new TestFactoryViews());
}
