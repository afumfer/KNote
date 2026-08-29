using KNote.Model;
using Microsoft.Data.Sqlite;

namespace KNote.Repository.Dapper;

/// <summary>
/// Overrides SQLite's built-in "like" scalar function so every LIKE comparison on the connection
/// becomes case- and accent-insensitive, mirroring the SQL Server COLLATE ... LIKE behavior used
/// for the same searches (see KntNoteRepository).
/// </summary>
internal static class KntSqliteAccentFunctions
{
    public static void Register(SqliteConnection connection)
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
