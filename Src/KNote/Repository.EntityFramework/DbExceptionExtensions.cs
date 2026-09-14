using System;
using Microsoft.Data.Sqlite;
using Microsoft.Data.SqlClient;

namespace KNote.Repository.EntityFramework;

internal static class DbExceptionExtensions
{
    // SQLite's primary result code for any constraint violation. See
    // https://www.sqlite.org/rescode.html#constraint. Matching only the primary code (not the more
    // specific extended code, e.g. SQLITE_CONSTRAINT_UNIQUE) keeps this resilient to library
    // updates: a false positive here just costs one extra, harmless retry in the
    // NoteNumber/FolderNumber generation loop (see KntNoteRepository/KntFolderRepository.AddAsync)
    // instead of masking a different bug.
    private const int SqliteConstraintViolation = 19;

    // SQL Server error numbers for a duplicate-key insert. 2601: duplicate row in a unique index;
    // 2627: primary key or UNIQUE constraint violation. See
    // https://learn.microsoft.com/sql/relational-databases/errors-events/database-engine-events-and-errors-2000-to-2999
    private const int SqlServerDuplicateKeyRow = 2601;
    private const int SqlServerUniqueOrPrimaryKeyViolation = 2627;

    // GenericRepositoryEF.AddAsync catches the real provider exception (wrapped by EF Core in a
    // DbUpdateException) and re-throws it as a KntRepositoryException via ResultDomainAction; that
    // wrapping preserves the original exception as InnerException (see
    // GenericRepositoryEF.AddExecptionsMessagesToResult/ResultDomainAction), so walking the chain
    // here finds the concrete SqliteException/SqlException regardless of how many wrapper
    // exceptions sit in between.
    public static bool IsUniqueConstraintViolation(this Exception ex)
    {
        for (var current = ex; current != null; current = current.InnerException)
        {
            if (current is SqliteException sqliteEx && sqliteEx.SqliteErrorCode == SqliteConstraintViolation)
                return true;

            if (current is SqlException sqlEx && sqlEx.Number is SqlServerDuplicateKeyRow or SqlServerUniqueOrPrimaryKeyViolation)
                return true;
        }

        return false;
    }
}
