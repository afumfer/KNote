using KNote.Model;
using KNote.Service.Core;

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
}
