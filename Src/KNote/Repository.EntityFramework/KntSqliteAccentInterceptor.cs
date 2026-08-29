using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using KNote.Model;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace KNote.Repository.EntityFramework;

/// <summary>
/// Overrides SQLite's built-in "like" scalar function so every LIKE comparison on connections EF
/// opens becomes case- and accent-insensitive, mirroring the SQL Server COLLATE ... LIKE behavior
/// used for the same searches (see KntNoteRepository). Must be paired with EF.Functions.Like in
/// LINQ queries, since EF's SQLite provider translates string.Contains to instr(...), not LIKE.
/// </summary>
internal sealed class KntSqliteAccentInterceptor : DbConnectionInterceptor
{
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData) =>
        Register((SqliteConnection)connection);

    public override Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        Register((SqliteConnection)connection);
        return Task.CompletedTask;
    }

    private static void Register(SqliteConnection connection)
    {
        connection.CreateFunction<string, string, bool>(
            "like",
            (pattern, value) => AccentInsensitiveLike.IsMatch(pattern, value),
            isDeterministic: true);

        connection.CreateFunction<string, string, string, bool>(
            "like",
            (pattern, value, escapeChar) =>
                AccentInsensitiveLike.IsMatch(pattern, value, string.IsNullOrEmpty(escapeChar) ? null : escapeChar[0]),
            isDeterministic: true);
    }
}
