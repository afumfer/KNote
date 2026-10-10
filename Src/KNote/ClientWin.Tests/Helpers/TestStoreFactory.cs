using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;

namespace KNote.ClientWin.Tests.Helpers;

/// <summary>Builds a minimal real Store (TestFactoryViews, no ServiceRef/DefaultFolderWithServiceRef
/// set) - enough to build a KNoteAiToolsHost for tests that don't exercise KNoteAiTools.create_task
/// (which is the only thing that touches Store.DefaultFolderWithServiceRef).</summary>
internal static class TestStoreFactory
{
    public static Store CreateEmpty() => new(new TestFactoryViews());

    /// <summary>
    /// Signs the store's user in as an Admin of the given repositories (of an unrelated fake one when none
    /// is given, which is enough for the application-wide role): tests that aren't about authorization
    /// exercise their controllers as a user allowed to do everything (see CtrlBase.CheckAccess,
    /// Store.CanRunScripts and NoteEditorCtrl.ConsultMode).
    /// </summary>
    public static Store GrantAdmin(this Store store, params KNote.Service.Core.IKntService[] repositories)
    {
        if (repositories.Length == 0)
            repositories = new KNote.Service.Core.IKntService[] { new FakeKntService() };

        foreach (var repository in repositories)
            store.Security.SetRepositoryRole(repository, KNote.Model.EnumRoles.Admin);

        return store;
    }
}
