using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Tests;

// KNote Atlas 4.6a (D18): how long the search by words takes with «Search in resources» (a LIKE over the description and the text read
// of every resource) in a real repository, against the same search without it, and the filter with «Also in the resources». The owner
// runs it in local against a COPY of his biggest repository: opening it brings its schema up to date, as the apps do. Only with
// KNOTE_SEARCH_MEASURE_DB (the path of a SQLite file, or a SQL Server connection string) and KNOTE_RESOURCE_MEASURE_FOLDER (where the
// results go: results\search-resources.*). The words are KNOTE_SEARCH_MEASURE_WORDS (separated by '|'); by default, a few common
// ones. Both ORMs, each search 5 times (the first one, to warm up, is not counted). The results hold only the words, the counts and
// the times.
[TestClass]
[TestCategory("RequiresRealAiProvider")]
public class ResourceSearchMeasure
{
    private const int Runs = 5;
    private static readonly string[] DefaultWords = ["the", "manual", "error", "QL", "Spectrum"];
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = false };

    private sealed class Row
    {
        public string Orm { get; set; } = "";
        public string Search { get; set; } = "";
        public string Words { get; set; } = "";
        public int Notes { get; set; }
        public double MedianMs { get; set; }
        public double MaxMs { get; set; }
        public string Error { get; set; } = "";
    }

    private static string Database => Environment.GetEnvironmentVariable("KNOTE_SEARCH_MEASURE_DB") ?? "";
    private static string Folder => Environment.GetEnvironmentVariable("KNOTE_RESOURCE_MEASURE_FOLDER") ?? "";

    [TestMethod]
    public async Task SearchInResources()
    {
        if (string.IsNullOrWhiteSpace(Database) || string.IsNullOrWhiteSpace(Folder))
        {
            Assert.Inconclusive("KNOTE_SEARCH_MEASURE_DB and KNOTE_RESOURCE_MEASURE_FOLDER are not set - skipping the measure.");
            return;
        }
        var results = Path.Combine(Folder, "results");
        Directory.CreateDirectory(results);
        var words = (Environment.GetEnvironmentVariable("KNOTE_SEARCH_MEASURE_WORDS") is { Length: > 0 } list ? list.Split('|') : DefaultWords)
            .Select(w => w.Trim()).Where(w => w.Length > 0).ToList();

        var isFile = File.Exists(Database);
        var rows = new List<Row>();
        var resources = new Dictionary<string, (int Resources, long Characters)>();
        foreach (var orm in new[] { "Dapper", "EntityFramework" })
        {
            var repositoryRef = new RepositoryRef
            {
                Orm = orm,
                Provider = isFile ? "Microsoft.Data.Sqlite" : "Microsoft.Data.SqlClient",
                ConnectionString = isFile ? $"Data Source={Database}" : Database
            };
            using var service = new KntService(KntRepositoryFactory.Create(repositoryRef), activateMessageBroker: false);
            var states = await service.Repository.Notes.GetResourceTextStatesAsync();
            resources[orm] = (states.Entity?.Count ?? 0, 0);

            foreach (var word in words)
            {
                rows.Add(await MeasureAsync(orm, "Words", word, () => service.Notes.GetSearchMinimalAsync(
                    new NotesSearchDto { TextSearch = word, SearchInDescription = true, PageIdentifier = new PageIdentifier { PageSize = 25 } })));
                rows.Add(await MeasureAsync(orm, "Words + resources", word, () => service.Notes.GetSearchMinimalAsync(
                    new NotesSearchDto { TextSearch = word, SearchInDescription = true, SearchInResources = true, PageIdentifier = new PageIdentifier { PageSize = 25 } })));
                rows.Add(await MeasureAsync(orm, "Filter Description + resources", word, () => service.Notes.GetFilterMinimalAsync(
                    new NotesFilterDto { Description = word, SearchInResources = true, PageIdentifier = new PageIdentifier { PageSize = 25 } })));
            }
        }

        File.WriteAllLines(Path.Combine(results, "search-resources.jsonl"), rows.Select(r => JsonSerializer.Serialize(r, Json)));
        WriteReport(Path.Combine(results, "search-resources.html"), rows, resources);
    }

    private static async Task<Row> MeasureAsync(string orm, string search, string words, Func<Task<Result<List<NoteMinimalDto>>>> run)
    {
        var row = new Row { Orm = orm, Search = search, Words = words };
        try
        {
            var times = new List<double>();
            for (var i = 0; i <= Runs; i++)
            {
                var watch = Stopwatch.StartNew();
                var result = await run();
                watch.Stop();
                if (!result.IsValid)
                {
                    row.Error = result.ErrorMessage;
                    return row;
                }
                row.Notes = (int)result.TotalCount;
                if (i > 0)
                    times.Add(watch.Elapsed.TotalMilliseconds);
            }
            times.Sort();
            row.MedianMs = times[times.Count / 2];
            row.MaxMs = times[^1];
        }
        catch (Exception ex)
        {
            row.Error = ex.GetType().Name + ": " + ex.Message;
        }
        return row;
    }

    private static void WriteReport(string path, List<Row> rows, Dictionary<string, (int Resources, long Characters)> resources)
    {
        static string H(string? text) => WebUtility.HtmlEncode(text ?? "");
        var html = new StringBuilder();
        html.Append("<!doctype html><meta charset=\"utf-8\"><title>Search in resources</title><style>body{font-family:Segoe UI,sans-serif;margin:2rem}" +
            "table{border-collapse:collapse}td,th{border:1px solid #ccc;padding:.4rem;text-align:right}td:first-child,td:nth-child(2),td:nth-child(3){text-align:left}</style>");
        html.Append($"<h1>Search in resources (D18)</h1><p>{H(string.Join(", ", resources.Select(r => $"{r.Key}: {r.Value.Resources} resources")))}. " +
            $"Median and maximum of {Runs} runs after one to warm up; the notes found are the total of the search (the first page of 25 is read).</p>");
        html.Append("<table><tr><th>ORM</th><th>Search</th><th>Words</th><th>Notes</th><th>Median ms</th><th>Max ms</th></tr>");
        foreach (var r in rows)
            html.Append($"<tr><td>{H(r.Orm)}</td><td>{H(r.Search)}</td><td>{H(r.Words)}</td><td>{r.Notes}</td>" +
                (string.IsNullOrEmpty(r.Error) ? $"<td>{r.MedianMs:0}</td><td>{r.MaxMs:0}</td>" : $"<td colspan=\"2\">ERROR {H(r.Error)}</td>") + "</tr>");
        html.Append("</table>");
        File.WriteAllText(path, html.ToString());
    }
}
