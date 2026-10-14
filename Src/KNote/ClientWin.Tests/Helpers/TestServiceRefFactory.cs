using KNote.Model;
using KNote.Service.Core;
using KNote.Service.Interfaces;
using Microsoft.Data.Sqlite;

namespace KNote.ClientWin.Tests.Helpers;

/// <summary>
/// Builds a real ServiceRef (Dapper + an in-memory SQLite database of its own) purely to give KNoteAiTools
/// a service to work against (see TestAiChatClientFactory), or ServiceRefRegistry/Store something to hold.
/// Building a ServiceRef opens its database: KntSchemaUpdater creates the schema there and ServiceRef then
/// reads the repository's identity from it, through another connection. A plain "Data Source=:memory:"
/// database disappears with the connection that created it, so each one here is a named, shared-cache
/// in-memory database kept alive by a connection left open for the whole test run.
/// </summary>
internal static class TestServiceRefFactory
{
    private static readonly List<SqliteConnection> KeptOpen = new();

    public static ServiceRef CreateInMemorySqlite(string name = "test", string? connectionString = null) => new(
        new RepositoryRef
        {
            Name = name,
            Orm = "Dapper",
            Provider = "Microsoft.Data.Sqlite",
            ConnectionString = connectionString ?? NewInMemoryDatabase()
        },
        userIdentityName: "test-user");

    public static string NewInMemoryDatabase()
    {
        var connectionString = $"Data Source=knote-tests-{Guid.NewGuid():N};Mode=Memory;Cache=Shared";

        var connection = new SqliteConnection(connectionString);
        connection.Open();
        lock (KeptOpen)
            KeptOpen.Add(connection);

        return connectionString;
    }

    /// <summary>
    /// A ServiceRef whose .Service resolves to the given fake instead of a real, repository-backed
    /// KntService. ServiceRef's constructor already touches its own Service getter (to set Logger and
    /// read the repository's identity), which builds a real KntService over the in-memory SQLite repository - this
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
