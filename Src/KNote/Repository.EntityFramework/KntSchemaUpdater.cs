using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using KNote.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace KNote.Repository.EntityFramework;

/// <summary>
/// Brings an existing database up to date with schema changes that are additive-only
/// (new tables/indexes, no ALTER of existing tables) and were never captured by an EF
/// Core migration. EnsureCreated() (triggered by the KntDbContext constructor below)
/// remains the only mechanism that creates a brand-new database; this class only
/// covers databases that already exist with an older schema.
///
/// A new schema change is added here as a new entry in "Steps" plus a bump of
/// CurrentSchemaRevision, at the same time - ModelBuilderExtensions.Seed() reads
/// CurrentSchemaRevision directly for the seeded DB_VERSION, so a brand-new install (created
/// via EnsureCreated from the current model) and an upgraded install (brought up to date by
/// this class) end up seeded at the same DB_VERSION and this loop is then a no-op for both.
/// The SQL below is NOT regenerated from CurrentSchemaRevision - keep it byte-for-byte in sync
/// with ModelBuilderExtensions/the entity it targets by hand, every time either one changes.
/// </summary>
public static class KntSchemaUpdater
{
    public const int CurrentSchemaRevision = 2;

    private static readonly List<(int Revision, Action<KntDbContext, RepositoryRef> Apply)> Steps = new()
    {
        (2, AddTraceNotes)
    };

    // KntRepositoryFactory.Create runs EnsureUpToDate on every repository construction, which for
    // Server means every scoped-per-request IKntRepository - without this cache that would mean one
    // extra DB roundtrip per HTTP request, forever, just to re-confirm what was already confirmed.
    // Keyed by connection string because ClientWin can have several RepositoryRefs (several open
    // databases) in the same process. A benign race on first use (two threads both missing the cache
    // and both running the check) is acceptable: EnsureUpToDate is idempotent by construction.
    private static readonly ConcurrentDictionary<string, bool> VerifiedConnectionStrings = new();

    public static void EnsureUpToDate(RepositoryRef repositoryRef)
    {
        if (VerifiedConnectionStrings.ContainsKey(repositoryRef.ConnectionString))
            return;

        using var ctx = CreateContext(repositoryRef);

        // Keep one physical connection open for the whole check, opened before EnsureCreated: EF's
        // default (non-kept-open) connection handling closes the connection between commands, which
        // for a non-shared-cache Sqlite "Data Source=:memory:" (used by some test doubles) would
        // destroy the database created by EnsureCreated before the read/write below ever ran.
        ctx.Database.OpenConnection();
        ctx.Database.EnsureCreated();

        var stored = ReadStoredRevision(ctx);

        foreach (var step in Steps.OrderBy(s => s.Revision))
        {
            if (step.Revision <= stored)
                continue;

            step.Apply(ctx, repositoryRef);
            WriteStoredRevision(ctx, step.Revision);
            stored = step.Revision;
        }

        VerifiedConnectionStrings[repositoryRef.ConnectionString] = true;
    }

    private static KntDbContext CreateContext(RepositoryRef repositoryRef)
    {
        var optionsBuilder = new DbContextOptionsBuilder<KntDbContext>();

        if (repositoryRef.Provider == "Microsoft.Data.SqlClient")
            optionsBuilder.UseSqlServer(repositoryRef.ConnectionString);
        else if (repositoryRef.Provider == "Microsoft.Data.Sqlite")
        {
            optionsBuilder.UseSqlite(repositoryRef.ConnectionString);
            optionsBuilder.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
        }
        else
            throw new Exception("Data provider not suported (KntSchemaUpdater)");

        // EnsureCreated is called explicitly by EnsureUpToDate, after opening the connection - see
        // the comment there. This is still the only path in KNote that creates a brand-new database.
        return new KntDbContext(optionsBuilder.Options, ensureCreated: false);
    }

    private static int ReadStoredRevision(KntDbContext ctx)
    {
        var value = ctx.SystemValues
            .Where(v => v.Scope == "SYSTEM" && v.Key == "DB_VERSION")
            .Select(v => v.Value)
            .FirstOrDefault();

        // Every step's Revision is >= 1, so any value that isn't already one of our own integer
        // revisions - including every pre-existing "x.x.x.x" APP_VERSION-style value ever seeded,
        // which int.TryParse simply fails on - collapses to 0 here and is intentionally treated as
        // "older than every step, run them all". There is no need to figure out which legacy value
        // means what: every step is itself idempotent (IF NOT EXISTS/IF EXISTS guards), so running a
        // step that turns out to already be applied is a safe no-op, not just an unreachable case.
        return int.TryParse(value, out var revision) ? revision : 0;
    }

    private static void WriteStoredRevision(KntDbContext ctx, int revision)
    {
        var row = ctx.SystemValues.FirstOrDefault(v => v.Scope == "SYSTEM" && v.Key == "DB_VERSION");

        if (row == null)
        {
            ctx.SystemValues.Add(new Entities.SystemValue
            {
                SystemValueId = Guid.NewGuid(),
                Scope = "SYSTEM",
                Key = "DB_VERSION",
                Value = revision.ToString()
            });
        }
        else
        {
            row.Value = revision.ToString();
        }

        ctx.SaveChanges();
    }

    // Revision 2 - adds TraceNoteTypes/TraceNotes, widens TraceNotes' unique index from
    // (FromId, ToId) to (FromId, ToId, TraceNoteTypeId) (Fase 0, decision 1: several relations of
    // different TraceNoteTypeId are now allowed between the same pair of notes), and deletes the
    // APP_VERSION SystemValues row - dead data since it was first seeded, never read anywhere in the
    // codebase (confirmed with a full-repo search); ModelBuilderExtensions.Seed() no longer writes it
    // either, so this step is what removes it from every pre-existing database.
    //
    // Both entities have been part of the EF model since 2020/2021 (git blame), long before any real
    // KNote installation - EnsureCreated() already created TraceNoteTypes/TraceNotes, WITH THE OLD
    // (FromId, ToId) unique index, for every database that was first created since then. So for
    // virtually every real database the CREATE TABLE below is a no-op (kept only as a defensive
    // fallback for a database that genuinely predates 2020), and the DROP INDEX of the old
    // IX_TraceNotes_FromId_ToId is the step that actually matters: without it, the old, narrower
    // unique constraint would keep rejecting same-pair/different-type relations right alongside the
    // new index, silently defeating decision 1.
    //
    // DDL generated once from the EF model (Database.GenerateCreateScript(), one per provider) at the
    // time this step was written, then hand-adapted to be idempotent (IF NOT EXISTS/IF EXISTS). Must
    // match ModelBuilderExtensions' TraceNote/TraceNoteType configuration and the Entities/TraceNote.cs,
    // TraceNoteType.cs shape.
    private static void AddTraceNotes(KntDbContext ctx, RepositoryRef repositoryRef)
    {
        var sql = repositoryRef.Provider == "Microsoft.Data.SqlClient" ? AddTraceNotesSqlServer : AddTraceNotesSqlite;
        ctx.Database.ExecuteSqlRaw(sql);
    }

    private const string AddTraceNotesSqlServer = @"
DELETE FROM [SystemValues] WHERE [Scope] = 'SYSTEM' AND [Key] = 'APP_VERSION';

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TraceNoteTypes')
BEGIN
    CREATE TABLE [TraceNoteTypes] (
        [TraceNoteTypeId] uniqueidentifier NOT NULL,
        [Name] nvarchar(256) NOT NULL,
        [Description] nvarchar(max) NULL,
        [Timestamp] rowversion NULL,
        CONSTRAINT [PK_TraceNoteTypes] PRIMARY KEY ([TraceNoteTypeId])
    );
END;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TraceNoteTypes_Name')
    CREATE UNIQUE INDEX [IX_TraceNoteTypes_Name] ON [TraceNoteTypes] ([Name]);

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TraceNotes')
BEGIN
    CREATE TABLE [TraceNotes] (
        [TraceNoteId] uniqueidentifier NOT NULL,
        [FromId] uniqueidentifier NOT NULL,
        [ToId] uniqueidentifier NOT NULL,
        [Order] int NOT NULL,
        [Weight] float NOT NULL,
        [TraceNoteTypeId] uniqueidentifier NULL,
        [Timestamp] rowversion NULL,
        CONSTRAINT [PK_TraceNotes] PRIMARY KEY ([TraceNoteId]),
        CONSTRAINT [FK_TraceNotes_Notes_FromId] FOREIGN KEY ([FromId]) REFERENCES [Notes] ([NoteId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TraceNotes_Notes_ToId] FOREIGN KEY ([ToId]) REFERENCES [Notes] ([NoteId]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TraceNotes_TraceNoteTypes_TraceNoteTypeId] FOREIGN KEY ([TraceNoteTypeId]) REFERENCES [TraceNoteTypes] ([TraceNoteTypeId])
    );
END;

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TraceNotes_FromId_ToId')
    DROP INDEX [IX_TraceNotes_FromId_ToId] ON [TraceNotes];

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TraceNotes_FromId_ToId_TraceNoteTypeId')
    CREATE UNIQUE INDEX [IX_TraceNotes_FromId_ToId_TraceNoteTypeId] ON [TraceNotes] ([FromId], [ToId], [TraceNoteTypeId]) WHERE [TraceNoteTypeId] IS NOT NULL;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TraceNotes_ToId')
    CREATE INDEX [IX_TraceNotes_ToId] ON [TraceNotes] ([ToId]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_TraceNotes_TraceNoteTypeId')
    CREATE INDEX [IX_TraceNotes_TraceNoteTypeId] ON [TraceNotes] ([TraceNoteTypeId]);
";

    private const string AddTraceNotesSqlite = @"
DELETE FROM ""SystemValues"" WHERE ""Scope"" = 'SYSTEM' AND ""Key"" = 'APP_VERSION';

CREATE TABLE IF NOT EXISTS ""TraceNoteTypes"" (
    ""TraceNoteTypeId"" TEXT NOT NULL CONSTRAINT ""PK_TraceNoteTypes"" PRIMARY KEY,
    ""Name"" TEXT NOT NULL,
    ""Description"" TEXT NULL,
    ""Timestamp"" BLOB NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ""IX_TraceNoteTypes_Name"" ON ""TraceNoteTypes"" (""Name"");

CREATE TABLE IF NOT EXISTS ""TraceNotes"" (
    ""TraceNoteId"" TEXT NOT NULL CONSTRAINT ""PK_TraceNotes"" PRIMARY KEY,
    ""FromId"" TEXT NOT NULL,
    ""ToId"" TEXT NOT NULL,
    ""Order"" INTEGER NOT NULL,
    ""Weight"" REAL NOT NULL,
    ""TraceNoteTypeId"" TEXT NULL,
    ""Timestamp"" BLOB NULL,
    CONSTRAINT ""FK_TraceNotes_Notes_FromId"" FOREIGN KEY (""FromId"") REFERENCES ""Notes"" (""NoteId"") ON DELETE RESTRICT,
    CONSTRAINT ""FK_TraceNotes_Notes_ToId"" FOREIGN KEY (""ToId"") REFERENCES ""Notes"" (""NoteId"") ON DELETE RESTRICT,
    CONSTRAINT ""FK_TraceNotes_TraceNoteTypes_TraceNoteTypeId"" FOREIGN KEY (""TraceNoteTypeId"") REFERENCES ""TraceNoteTypes"" (""TraceNoteTypeId"")
);

DROP INDEX IF EXISTS ""IX_TraceNotes_FromId_ToId"";
CREATE UNIQUE INDEX IF NOT EXISTS ""IX_TraceNotes_FromId_ToId_TraceNoteTypeId"" ON ""TraceNotes"" (""FromId"", ""ToId"", ""TraceNoteTypeId"");
CREATE INDEX IF NOT EXISTS ""IX_TraceNotes_ToId"" ON ""TraceNotes"" (""ToId"");
CREATE INDEX IF NOT EXISTS ""IX_TraceNotes_TraceNoteTypeId"" ON ""TraceNotes"" (""TraceNoteTypeId"");
";
}
