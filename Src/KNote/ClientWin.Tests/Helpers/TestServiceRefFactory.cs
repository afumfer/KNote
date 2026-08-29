using KNote.Model;
using KNote.Service.Core;
using KNote.Service.Interfaces;

namespace KNote.ClientWin.Tests.Helpers;

/// <summary>
/// Builds a real ServiceRef (Dapper + SQLite :memory:) purely to satisfy AiChatClientFactory.Create's
/// signature. Building it is lazy and doesn't open a DB connection until a repository method is
/// actually called - see ServiceRefRegistryTests.cs for the same recipe. Fine even for the
/// tool-calling smoke test: an in-memory DB with no schema still lets search_notes run (and, if the
/// model invokes it, Microsoft.Extensions.AI's function-invocation pipeline reports the resulting
/// error back to the model rather than throwing - it does not fail the chat completion itself).
/// </summary>
internal static class TestServiceRefFactory
{
    public static ServiceRef CreateInMemorySqlite(string alias = "test") => new(
        new RepositoryRef
        {
            Alias = alias,
            Orm = "Dapper",
            Provider = "Microsoft.Data.Sqlite",
            ConnectionString = "Data Source=:memory:"
        },
        userIdentityName: "test-user");

    /// <summary>
    /// A ServiceRef whose .Service resolves to the given fake instead of a real, repository-backed
    /// KntService. ServiceRef's constructor already touches its own Service getter once (to set
    /// Logger), which lazily builds a real KntService over the in-memory SQLite repository - this
    /// overwrites that via ServiceRef's public "_service" field before anything else can observe it,
    /// so every later ServiceRef.Service access returns the fake. Used for tests that need
    /// Store.DefaultFolderWithServiceRef.ServiceRef.Service to be a fake (e.g.
    /// KNoteAiToolsTests' create_task tests), where a real database is neither needed nor wanted.
    /// </summary>
    public static ServiceRef CreateWithFakeService(IKntService fakeService)
    {
        var serviceRef = CreateInMemorySqlite();
        serviceRef._service = fakeService;
        return serviceRef;
    }
}
