using System.Data.Common;
using System.Diagnostics;
using System.Text;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The measure of the note history (H30, 03-Historial.md): against a COPY of a real repository (it writes in it), with Dapper and with
/// EF. It never touches the notes of the copy: it copies some of them (the biggest ones, one of a median size and a random sample) into
/// a folder of its own, works on those and, at the end, deletes them, removes them from the deleted notes and deletes the folder (their
/// rows of the history stay, without copies).
/// A · the cost of saving: with the history (the first save of a session: row and copy; the autosaves of the same session: the row
///     updated) against the same saves without it (KntHistory.NotNoted).
/// B · the reads of the apps with a load (4 sessions of 4 users on each note of the sample): the «History» tab, «See this version»,
///     «Compare», «Repository changes» (its first page and a later one) and «Recover deleted notes».
/// C · what KLogs takes (rows, copies and bytes, by entity), before and with the load.
/// D · what the retention would empty, in the copy (now, in 30, 90 and 365 days) and simulated for a note edited every working day
///     and ten times a day for two years.
/// E · the autosave: how many rows a session of many saves leaves.
/// The report goes to results\history-measure-*.md under KNOTE_RESOURCE_MEASURE_FOLDER (or the temporary folder), out of the
/// repository. Variables: KNOTE_HISTORY_MEASURE_DB (a SQLite file or a SQL Server connection string); optional
/// KNOTE_HISTORY_MEASURE_NOTES (the sample, 250) and _SEED (1). Without the database, Inconclusive. Filter by name.
/// </summary>
[TestClass]
public class HistoryMeasure
{
    private const int Samples = 8;
    private const int Autosaves = 20;
    private static readonly string[] Users = { "measure-a", "measure-b", "measure-c", "measure-d" };

    private static string Env(string name, string fallback = "") => Environment.GetEnvironmentVariable(name) is { Length: > 0 } value ? value : fallback;
    private static string Database => Env("KNOTE_HISTORY_MEASURE_DB");
    private static bool IsFile => File.Exists(Database);

    [TestMethod]
    public async Task Measure()
    {
        if (string.IsNullOrWhiteSpace(Database))
        {
            Assert.Inconclusive("KNOTE_HISTORY_MEASURE_DB is not set - skipping the measure of the history.");
            return;
        }
        var folder = Env("KNOTE_RESOURCE_MEASURE_FOLDER", IsFile ? Path.GetDirectoryName(Database)! : Path.GetTempPath());
        var results = Path.Combine(folder, "results");
        Directory.CreateDirectory(results);
        var sampleSize = int.TryParse(Env("KNOTE_HISTORY_MEASURE_NOTES"), out var n) ? n : 250;
        var seed = int.TryParse(Env("KNOTE_HISTORY_MEASURE_SEED"), out var s) ? s : 1;

        var report = new StringBuilder($"# History measure · {(IsFile ? Path.GetFileName(Database) : new SqlConnectionStringBuilder(Database).InitialCatalog)} · {DateTime.Now:yyyy-MM-dd HH:mm}\n\n");
        using (var first = Open("EntityFramework"))
        {
            report.Append($"Notes in the copy: {await ScalarAsync("SELECT COUNT(*) FROM Notes"):N0}. ");
            var since = (await first.History.GetRetentionAsync()).Entity?.Since;
            report.Append($"History since: {(since is DateTime d ? d.ToString("yyyy-MM-dd HH:mm") : "-")}.\n\n");
            report.Append("## C · What KLogs takes, before the measure\n\n").Append(await SpaceAsync()).Append('\n');
            report.Append("## D · What the retention would empty in the copy\n\n").Append(await RetentionInTheCopyAsync(first)).Append('\n');
        }
        report.Append("## D · The retention, simulated\n\n").Append(SimulatedRetention()).Append('\n');

        foreach (var orm in new[] { "Dapper", "EntityFramework" })
        {
            report.Append($"## {orm}\n\n");
            using var service = Open(orm);
            await RunAsync(service, orm, sampleSize, seed, report);
        }

        var file = Path.Combine(results, $"history-measure-{DateTime.Now:yyyyMMdd-HHmm}.md");
        await File.WriteAllTextAsync(file, report.ToString());
        Console.WriteLine(report);
        Console.WriteLine($"Report: {file}");
    }

    private static KntService Open(string orm) => new(KntRepositoryFactory.Create(new RepositoryRef
    {
        Orm = orm,
        Provider = IsFile ? "Microsoft.Data.Sqlite" : "Microsoft.Data.SqlClient",
        ConnectionString = IsFile ? $"Data Source={Database}" : Database
    }), activateMessageBroker: false) { HistoryOrigin = EnumHistoryOrigin.Desktop };

    // The whole measure with one ORM, on its own copies of the notes.
    private static async Task RunAsync(KntService service, string orm, int sampleSize, int seed, StringBuilder report)
    {
        var admin = (await service.Users.GetAllAsync()).Entity
            .FirstOrDefault(u => KntRoles.Parse(u.RoleDefinition).Contains(EnumRoles.Admin))?.UserName ?? Users[0];
        As(service, admin);
        var home = (await service.Repository.Folders.GetAsync(KntConst.DefaultFolderNumber)).Entity;
        var measureFolder = (await service.Folders.SaveAsync(new FolderDto
        {
            FolderId = Guid.Empty, Name = $"__History measure {orm} {DateTime.Now:yyyyMMdd-HHmmss}__", ParentId = home.FolderId
        })).Entity;
        Assert.IsNotNull(measureFolder, "the folder of the measure could not be created");

        var biggest = await NoteIdsAsync("ORDER BY LEN_DESC DESC", 3);
        var median = await MedianNoteIdAsync();
        // The same seed gives the same sample.
        var random = new Random(seed);
        var sample = (await NoteIdsAsync("ORDER BY 1", int.MaxValue)).OrderBy(_ => random.Next()).Take(sampleSize).ToList();

        var copies = new List<Guid>();
        try
        {
            // A · the cost of saving.
            report.Append("### A · Saving a note (ms: median / max)\n\n| Note | Size | Without history | First save of a session | Autosave (same session) | Rows of the session |\n|---|---|---|---|---|---|\n");
            foreach (var sourceId in biggest.Append(median))
            {
                var copy = await CopyAsync(service, sourceId, measureFolder, admin);
                copies.Add(copy.Id);
                var without = new List<double>();
                using (KntHistory.NotNoted())
                    for (int i = 0; i < Samples; i++)
                        without.Add(await EditAsync(service, copy.Id, $"without {i}"));
                var sessions = new List<double>();
                for (int i = 0; i < Samples; i++)
                {
                    As(service, Users[i % Users.Length]);
                    sessions.Add(await EditAsync(service, copy.Id, $"session {i}"));
                }
                // E · the autosave: the same user, many saves in a row.
                As(service, admin);
                var rowsBefore = await RowsOfAsync(service, copy.Id);
                var autosaves = new List<double>();
                for (int i = 0; i < Autosaves; i++)
                    autosaves.Add(await EditAsync(service, copy.Id, $"autosave {i}"));
                var rowsAdded = await RowsOfAsync(service, copy.Id) - rowsBefore;
                report.Append($"| #{copy.Number} (from #{copy.SourceNumber}) | {copy.Size / 1024.0:N0} KB | {Stats(without)} | {Stats(sessions)} | {Stats(autosaves.Skip(1))} | {rowsAdded} row(s) for {Autosaves} saves |\n");
            }

            // B · the reads, with a load: 4 sessions of 4 users on each note of the sample.
            var loadClock = Stopwatch.StartNew();
            var loaded = new List<Guid>();
            foreach (var sourceId in sample)
            {
                var copy = await CopyAsync(service, sourceId, measureFolder, admin);
                loaded.Add(copy.Id);
                foreach (var user in Users)
                {
                    As(service, user);
                    await EditAsync(service, copy.Id, user);
                }
            }
            loadClock.Stop();
            copies.AddRange(loaded);
            As(service, admin);
            report.Append($"\nLoad: {loaded.Count} notes copied with {loaded.Count * Users.Length} sessions in {loadClock.Elapsed.TotalSeconds:N0} s.\n\n");

            var big = copies[0];
            var bigRows = (await service.History.GetNoteHistoryAsync(big)).Entity.Rows;
            var oldestWithCopy = bigRows.Last(r => r.HasPreviousContent);
            var reads = new List<(string, double)>
            {
                ("«History» of the biggest note (first page)", await TimeAsync(() => service.History.GetNoteHistoryAsync(big))),
                ("«See this version» (its oldest version)", await TimeAsync(() => service.History.GetNoteVersionAsync(big, oldestWithCopy.KLogId))),
                ("«Compare» (that change)", await TimeAsync(() => service.History.GetNoteComparisonAsync(big, oldestWithCopy.KLogId))),
                ("«Compare» with the current note", await TimeAsync(() => service.History.GetNoteComparisonAsync(big, oldestWithCopy.KLogId, true))),
                ("«Repository changes», last 7 days (first page)", await TimeAsync(() => service.History.GetRepositoryChangesAsync(new RepositoryChangesQueryDto()))),
                ("«Repository changes», last 90 days (first page)", await TimeAsync(() => service.History.GetRepositoryChangesAsync(
                    new RepositoryChangesQueryDto { From = DateTime.Today.AddDays(-89) }))),
                ("«Repository changes», 7 days, rows 901-1000", await TimeAsync(() => service.History.GetRepositoryChangesAsync(new RepositoryChangesQueryDto { Skip = 900 }))),
                ("Its users (the filter «Who»)", await TimeAsync(() => service.History.GetRepositoryChangesUsersAsync(new RepositoryChangesQueryDto())))
            };

            report.Append("### C · What KLogs takes, with the load\n\n").Append(await SpaceAsync()).Append('\n');

            // The deleted notes: all the copies of the measure.
            var deleteClock = Stopwatch.StartNew();
            foreach (var id in copies)
                Assert.IsTrue((await service.Notes.DeleteExtendedAsync(id)).IsValid);
            deleteClock.Stop();
            reads.Add(($"«Recover deleted notes» (with {copies.Count} more)", await TimeAsync(() => service.History.GetDeletedNotesAsync())));
            report.Append("### B · Reads (ms, the median of 5)\n\n| Read | Time |\n|---|---|\n");
            foreach (var (what, ms) in reads)
                report.Append($"| {what} | {ms:N0} |\n");
            report.Append($"\nDeleting the {copies.Count} notes of the measure (each with its last version): {deleteClock.Elapsed.TotalMilliseconds / copies.Count:N0} ms per note.\n\n");
        }
        finally
        {
            As(service, admin);
            foreach (var id in copies)
            {
                if ((await service.Notes.GetAsync(id)).IsValid)
                    await service.Notes.DeleteExtendedAsync(id);
                await service.History.RemoveDeletedNoteAsync(id);
            }
            await service.Folders.DeleteAsync(measureFolder.FolderId);
        }
    }

    private sealed record Copy(Guid Id, int Number, int SourceNumber, int Size);

    // A copy of a note of the repository, in the folder of the measure, without its type (its attributes) or its parts.
    private static async Task<Copy> CopyAsync(KntService service, Guid sourceId, FolderDto folder, string admin)
    {
        As(service, admin);
        var source = (await service.Notes.GetAsync(sourceId)).Entity;
        var note = new NoteDto
        {
            NoteId = Guid.NewGuid(), Topic = "[history measure] " + source.Topic, Description = source.Description, Tags = source.Tags ?? "",
            ContentType = source.ContentType, FolderId = folder.FolderId, FolderDto = folder,
            CreationDateTime = DateTime.Now, ModificationDateTime = DateTime.Now
        };
        var saved = await service.Notes.SaveAsync(note, false);
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        return new Copy(saved.Entity.NoteId, saved.Entity.NoteNumber, source.NoteNumber, Encoding.UTF8.GetByteCount(source.Description ?? ""));
    }

    // One save with a change of its text; the time of the save only.
    private static async Task<double> EditAsync(KntService service, Guid noteId, string mark)
    {
        var note = (await service.Notes.GetAsync(noteId)).Entity;
        note.Description = (note.Description ?? "") + $"\r\n<!-- {mark} -->";
        var clock = Stopwatch.StartNew();
        var saved = await service.Notes.SaveAsync(note, false);
        clock.Stop();
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        return clock.Elapsed.TotalMilliseconds;
    }

    private static async Task<int> RowsOfAsync(KntService service, Guid noteId) =>
        (await service.Repository.History.GetEntityHistoryAsync(HistoryEntities.Note, noteId)).Entity?.Count ?? 0;

    private static void As(KntService service, string user)
    {
        service.UserIdentityName = user;
        service.ResetCurrentUser();
    }

    private static async Task<double> TimeAsync<T>(Func<Task<T>> read) where T : ResultBase
    {
        var times = new List<double>();
        for (int i = 0; i < 5; i++)
        {
            var clock = Stopwatch.StartNew();
            var result = await read();
            clock.Stop();
            Assert.IsTrue(result.IsValid, result.ErrorMessage);
            times.Add(clock.Elapsed.TotalMilliseconds);
        }
        return Median(times);
    }

    private static double Median(IEnumerable<double> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        return sorted.Count == 0 ? 0 : sorted[sorted.Count / 2];
    }

    private static string Stats(IEnumerable<double> values)
    {
        var list = values.ToList();
        return list.Count == 0 ? "-" : $"{Median(list):N0} / {list.Max():N0}";
    }

    #region The copy, read directly (only what the Service doesn't give: sizes and the notes to copy)

    private static DbConnection Connection() => IsFile ? new SqliteConnection($"Data Source={Database}") : new SqlConnection(Database);

    private static string LengthOf(string column) => IsFile ? $"LENGTH({column})" : $"DATALENGTH({column})";

    private static async Task<long> ScalarAsync(string sql)
    {
        await using var connection = Connection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        return Convert.ToInt64(await command.ExecuteScalarAsync() ?? 0);
    }

    private static async Task<List<object[]>> RowsAsync(string sql)
    {
        await using var connection = Connection();
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        var rows = new List<object[]>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var values = new object[reader.FieldCount];
            reader.GetValues(values);
            rows.Add(values);
        }
        return rows;
    }

    // The notes to copy: not of the types KNote writes (they are not noted).
    private static string NotesOfUsers => "FROM Notes n LEFT JOIN NoteTypes t ON t.NoteTypeId = n.NoteTypeId WHERE (t.Name IS NULL OR t.Name NOT LIKE '@%')";

    private static async Task<List<Guid>> NoteIdsAsync(string orderBy, int take)
    {
        var top = take == int.MaxValue ? "" : IsFile ? "" : $"TOP {take} ";
        var limit = take == int.MaxValue || !IsFile ? "" : $" LIMIT {take}";
        var rows = await RowsAsync($"SELECT {top}n.NoteId, {LengthOf("n.Description")} AS LEN_DESC {NotesOfUsers} {orderBy}{limit}");
        return rows.Select(r => r[0] is Guid g ? g : Guid.Parse(r[0].ToString()!)).ToList();
    }

    private static async Task<Guid> MedianNoteIdAsync()
    {
        var count = await ScalarAsync($"SELECT COUNT(*) {NotesOfUsers}");
        var page = IsFile ? $"LIMIT 1 OFFSET {count / 2}" : $"OFFSET {count / 2} ROWS FETCH NEXT 1 ROWS ONLY";
        var rows = await RowsAsync($"SELECT n.NoteId, {LengthOf("n.Description")} AS LEN_DESC {NotesOfUsers} ORDER BY LEN_DESC {page}");
        return rows[0][0] is Guid g ? g : Guid.Parse(rows[0][0].ToString()!);
    }

    private static async Task<string> SpaceAsync()
    {
        var rows = await RowsAsync($"SELECT EntityName, COUNT(*), COUNT(PreviousContentSize), COALESCE(SUM(CAST(PreviousContentSize AS BIGINT)), 0), " +
            $"COALESCE(SUM(CAST({LengthOf("PreviousContent")} AS BIGINT)), 0), " +
            $"COALESCE(SUM(CAST({LengthOf("ChangeDetails")} AS BIGINT)), 0) + COALESCE(SUM(CAST({LengthOf("RegistryMessage")} AS BIGINT)), 0) " +
            "FROM KLogs GROUP BY EntityName ORDER BY EntityName");
        var text = new StringBuilder("| Entity | Rows | Rows with a copy | Copies (UTF-8) | Copies (stored) | Details and lines (stored) |\n|---|---|---|---|---|---|\n");
        foreach (var r in rows)
            text.Append($"| {r[0]} | {Convert.ToInt64(r[1]):N0} | {Convert.ToInt64(r[2]):N0} | {Size(Convert.ToInt64(r[3]))} | {Size(Convert.ToInt64(r[4]))} | {Size(Convert.ToInt64(r[5]))} |\n");
        if (rows.Count == 0)
            text.Append("| (no rows) | | | | | |\n");
        var notes = await RowsAsync("SELECT EntityId, COUNT(*) FROM KLogs WHERE EntityName = 'Note' GROUP BY EntityId");
        if (notes.Count > 0)
        {
            var perNote = notes.Select(r => Convert.ToDouble(r[1])).ToList();
            text.Append($"\nNotes with history: {notes.Count:N0}; rows per note: median {Median(perNote):N0}, max {perNote.Max():N0}.\n");
        }
        return text.ToString();
    }

    private static string Size(long bytes) => bytes >= 1024 * 1024 ? $"{bytes / 1024.0 / 1024.0:N1} MB" : $"{bytes / 1024.0:N0} KB";

    #endregion

    #region D · The retention

    // What the retention of the copy would empty now and later, without emptying anything.
    private static async Task<string> RetentionInTheCopyAsync(KntService service)
    {
        var retention = (await service.History.GetRetentionAsync()).Entity?.Retention ?? new HistoryRetentionDto();
        var entities = await RowsAsync("SELECT DISTINCT EntityName, EntityId FROM KLogs WHERE PreviousContentSize IS NOT NULL");
        var histories = new List<List<KLogDto>>();
        foreach (var e in entities)
            histories.Add((await service.Repository.History.GetEntityHistoryAsync(e[0].ToString()!, e[1] is Guid g ? g : Guid.Parse(e[1].ToString()!))).Entity ?? new());

        var text = new StringBuilder($"Retention: all {retention.KeepAllMonths} months, one a day until {retention.DailyUntilMonths}, then one a month; " +
            $"{retention.MaxContentSizeMB} MB and {retention.MaxCopiesPerNote} copies per note at most; deleted notes {retention.DeletedNotesDays} days.\n\n" +
            "| When | Copies emptied | Bytes emptied | Copies kept |\n|---|---|---|---|\n");
        foreach (var days in new[] { 0, 30, 90, 365 })
        {
            var now = DateTime.Now.AddDays(days);
            var emptied = histories.SelectMany(h => h.Where(r => HistoryRules.CopiesToEmpty(h, retention, now).Contains(r.KLogId))).ToList();
            var all = histories.Sum(h => h.Count(r => r.HasPreviousContent));
            text.Append($"| {(days == 0 ? "Now" : $"In {days} days")} | {emptied.Count:N0} | {Size(emptied.Sum(r => (long)(r.PreviousContentSize ?? 0)))} | {all - emptied.Count:N0} |\n");
        }
        return text.ToString();
    }

    // A note edited every working day, and one edited ten times a day, for two years: the copies the default retention keeps.
    private static string SimulatedRetention()
    {
        var retention = new HistoryRetentionDto();
        var now = DateTime.Today.AddHours(18);
        var text = new StringBuilder("With the default retention, a note of 100 KB:\n\n| Edited | Sessions in two years | Copies kept | Its copies take |\n|---|---|---|---|\n");
        foreach (var (what, perDay) in new[] { ("Every working day, once", 1), ("Every working day, ten times", 10) })
        {
            var rows = new List<KLogDto>();
            for (var day = now.Date.AddYears(-2); day <= now.Date; day = day.AddDays(1))
            {
                if (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                    continue;
                for (int i = 0; i < perDay; i++)
                {
                    var when = day.AddHours(9).AddMinutes(i * 45);
                    rows.Add(new KLogDto { KLogId = Guid.NewGuid(), RegistryDateTime = when, LastDateTime = when.AddMinutes(5), PreviousContentSize = 100 * 1024 });
                }
            }
            var kept = rows.Count - HistoryRules.CopiesToEmpty(rows, retention, now).Count;
            text.Append($"| {what} | {rows.Count:N0} | {kept:N0} | {Size(kept * 100L * 1024)} |\n");
        }
        return text.ToString();
    }

    #endregion
}
