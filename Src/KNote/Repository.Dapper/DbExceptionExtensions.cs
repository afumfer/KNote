using System;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;

namespace KNote.Repository.Dapper;

internal static class DbExceptionExtensions
{
    // SQLite's primary result code for any constraint violation (unique index, primary key, not
    // null, foreign key, check...). See https://www.sqlite.org/rescode.html#constraint. Matching
    // only the primary code (not the more specific extended code, e.g. SQLITE_CONSTRAINT_UNIQUE)
    // keeps this resilient to library updates: a false positive here just costs one extra,
    // harmless retry in the NoteNumber/FolderNumber generation loop (see
    // KntNoteRepository/KntFolderRepository.AddAsync) instead of masking a different bug.
    private const int SqliteConstraintViolation = 19;

    // SQL Server error numbers for a duplicate-key insert. 2601: duplicate row in a unique index;
    // 2627: primary key or UNIQUE constraint violation. See
    // https://learn.microsoft.com/sql/relational-databases/errors-events/database-engine-events-and-errors-2000-to-2999
    private const int SqlServerDuplicateKeyRow = 2601;
    private const int SqlServerUniqueOrPrimaryKeyViolation = 2627;

    // Provider-specific, error-code based detection (not string matching on ex.Message, which is
    // both localized and free to change wording between provider versions).
    public static bool IsUniqueConstraintViolation(this Exception ex) => ex switch
    {
        SqliteException sqliteEx => sqliteEx.SqliteErrorCode == SqliteConstraintViolation,
        SqlException sqlEx => sqlEx.Number is SqlServerDuplicateKeyRow or SqlServerUniqueOrPrimaryKeyViolation,
        _ => false
    };
}
