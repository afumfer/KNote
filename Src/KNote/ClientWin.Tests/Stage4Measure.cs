using System.Data.Common;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using KNote.Ai;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;

namespace KNote.ClientWin.Tests;

// KNote Atlas 4.7b: the measure of the stage (§ 6 of 03-Etapa-4), that the owner runs in local against a COPY of his biggest
// repository, already processed (photos described, documents read, scanned pages transcribed, index up to date). Opening it brings
// its schema up to date, as the apps do; nothing else is written. Variables:
//   KNOTE_STAGE4_MEASURE_DB         the copy: the path of a SQLite file, or a SQL Server connection string;
//   KNOTE_STAGE4_MEASURE_RESOURCES  the root folder of its resources (ResourcesContainerRootPath; only read);
//   KNOTE_RESOURCE_MEASURE_FOLDER   where the results go (results\stage4-*): they hold what the photos and documents say, so they
//                                   stay there, out of the repository;
//   KNOTE_STAGE4_MEASURE_QUERIES    (Index, optional) the searches by meaning, separated by '|';
//   KNOTE_STAGE4_MEASURE_SEED       (optional) the seed of the samples: the same one gives the same photos and pages.
// Photos: 30 photos described by the AI, at random, in a sheet to rate them (acceptable / could be better / bad). Pages: 20 scanned
// pages transcribed by the AI, at random, the page next to its transcription (understood / partly / no). Index: the size of what
// the stage keeps, the load of the cache of vectors and the time of the search by meaning (All, Photos, Documents), with the
// embeddings provider of the tests (AiProviderSmokeTests:Ollama, the model of the repository). Filter by name: never the category.
[TestClass]
[TestCategory("RequiresRealAiProvider")]
public class Stage4Measure
{
    private const int PhotosToReview = 30;
    private const int PagesToReview = 20;
    private const int Runs = 5;
    private const int PhotoSide = 480;
    private static readonly string[] DefaultQueries =
        ["a photo of an old computer", "how to repair the power supply", "a family trip to the beach", "the keyboard does not work", "a technical manual"];
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    private static string Database => Environment.GetEnvironmentVariable("KNOTE_STAGE4_MEASURE_DB") ?? "";
    private static string ResourcesRoot => Environment.GetEnvironmentVariable("KNOTE_STAGE4_MEASURE_RESOURCES") ?? "";
    private static string Folder => Environment.GetEnvironmentVariable("KNOTE_RESOURCE_MEASURE_FOLDER") ?? "";
    private static string Results => Path.Combine(Folder, "results");
    private static int Seed => int.TryParse(Environment.GetEnvironmentVariable("KNOTE_STAGE4_MEASURE_SEED"), out var seed) ? seed : 4;
    private static bool IsFile => File.Exists(Database);
    private static string RepositoryLabel => IsFile ? Path.GetFileName(Database) : "SQL Server";

    // A resource of the copy, read straight from its tables: the services give the resources of a note, not all of them.
    private sealed record ResourceRow(Guid ResourceId, string Name, string FileType, string? Description, string? DescriptionModel,
        bool DescriptionByUser, int ExtractedState, bool HasText, long NoteNumber, string? Topic, string? NoteTypeName);

    private sealed record SheetItem(string Title, string? Image, string? Text, string? Error);

    [TestMethod]
    public async Task Photos()
    {
        if (!Ready(out var reason))
        {
            Assert.Inconclusive(reason!);
            return;
        }
        using var service = Open();
        var resources = ReadResources();
        var photos = resources.Where(r => AiPhotoRules.IsPhoto(r.FileType, r.Name)).ToList();
        var described = photos.Where(r => !r.DescriptionByUser && !string.IsNullOrEmpty(r.DescriptionModel)).ToList();

        var items = new List<SheetItem>();
        foreach (var photo in Sample(described, PhotosToReview))
        {
            string? image = null, error = null;
            var file = await service.ResourceContent.GetPhotoFileAsync(photo.ResourceId);
            if (!file.IsValid)
                error = file.ErrorMessage;
            else if (AiImageReducer.Reduce(file.Entity.ContentArrayBytes, AiPhotoRules.ContentType(photo.FileType, photo.Name), PhotoSide) is { } reduced)
                image = $"data:{reduced.ContentType};base64,{Convert.ToBase64String(reduced.Content)}";
            else
                error = "The photo could not be read.";
            items.Add(new SheetItem($"#{photo.NoteNumber} {photo.Topic} · {FileName(photo.Name)} · {photo.DescriptionModel}", image, photo.Description, error));
        }

        var models = described.GroupBy(r => r.DescriptionModel).Select(g => $"{g.Key}: {g.Count()}");
        WriteSheet(Path.Combine(Results, "stage4-photos.html"), "Stage 4 · 30 photos described by the AI",
            $"{photos.Count} photos; {described.Count} described by the AI ({string.Join(", ", models)}), " +
            $"{photos.Count(r => r.DescriptionByUser)} by a person, {photos.Count(r => !r.DescriptionByUser && string.IsNullOrEmpty(r.DescriptionModel))} " +
            $"without a description of the AI. {items.Count} at random (seed {Seed}).",
            ["Acceptable", "Could be better", "Bad"], items);
    }

    [TestMethod]
    public async Task Pages()
    {
        if (!Ready(out var reason))
        {
            Assert.Inconclusive(reason!);
            return;
        }
        using var service = Open();
        var documents = ReadResources().Where(r => r.HasText && DocumentTextRules.IsDocument(r.FileType, r.Name)).ToList();

        var transcribed = new List<(ResourceRow Document, DocumentPageDto Page)>();
        var scanned = 0;
        foreach (var document in documents)
        {
            var text = await service.Repository.Notes.GetResourceTextAsync(document.ResourceId);
            var pages = DocumentTextRules.Pages(text.Entity?.Text);
            scanned += pages.Count(p => p.NeedsTranscription || p.Transcribed);
            transcribed.AddRange(pages.Where(p => p.Transcribed).Select(p => (document, p)));
        }

        var items = new List<SheetItem>();
        foreach (var (document, page) in Sample(transcribed, PagesToReview))
        {
            var drawn = await service.ResourceContent.DrawDocumentPageAsync(document.ResourceId, page.Number);
            items.Add(new SheetItem($"#{document.NoteNumber} {document.Topic} · {FileName(document.Name)} · page {page.Number}",
                drawn.IsValid ? drawn.Entity.DataUrl : null, page.Text, drawn.IsValid ? null : drawn.ErrorMessage));
        }

        WriteSheet(Path.Combine(Results, "stage4-pages.html"), "Stage 4 · 20 scanned pages transcribed by the AI",
            $"{documents.Count} documents with text; {scanned} scanned pages, {transcribed.Count} transcribed. {items.Count} at random (seed {Seed}). " +
            "Can you find and understand what the page says?", ["Understood", "Partly", "No"], items, sideBySide: true);
    }

    [TestMethod]
    public async Task Index()
    {
        if (!Ready(out var reason))
        {
            Assert.Inconclusive(reason!);
            return;
        }
        var embeddings = AiTestConfig.TryGetOllamaEmbeddingsProviderRef();
        if (embeddings == null || !await AiTestConfig.IsOllamaReachableAsync(embeddings.Host))
        {
            Assert.Inconclusive("Ollama is not configured or not reachable - skipping the measure.");
            return;
        }
        using var service = Open();
        var models = (await service.SystemValues.GetRepositoryAiModelsAsync()).Entity;
        if (string.IsNullOrEmpty(models?.EmbeddingModel))
        {
            Assert.Inconclusive("The repository has no embedding model - skipping the measure.");
            return;
        }
        embeddings.Model = models.EmbeddingModel;
        service.EmbeddingGenerators = new AiEmbeddingGenerators(() => [embeddings]);
        var key = models.GetIndexKey();
        var report = new StringBuilder();
        report.AppendLine("# Stage 4 · index and search").AppendLine();
        report.AppendLine($"Repository: {RepositoryLabel}; index `{key}`; {DateTime.Now:yyyy-MM-dd HH:mm}.").AppendLine();

        // What the stage keeps.
        var resources = ReadResources();
        var photos = resources.Where(r => AiPhotoRules.IsPhoto(r.FileType, r.Name)).ToList();
        var documents = resources.Where(r => DocumentTextRules.IsDocument(r.FileType, r.Name)).ToList();
        long textCharacters = 0;
        int pages = 0, pending = 0, transcribed = 0;
        foreach (var document in documents.Where(d => d.HasText))
        {
            var text = (await service.Repository.Notes.GetResourceTextAsync(document.ResourceId)).Entity?.Text ?? "";
            textCharacters += text.Length;
            var documentPages = DocumentTextRules.Pages(text);
            pages += documentPages.Count;
            pending += documentPages.Count(p => p.NeedsTranscription);
            transcribed += documentPages.Count(p => p.Transcribed);
        }
        report.AppendLine("## What is kept").AppendLine();
        report.AppendLine("| | |").AppendLine("|---|---|");
        if (IsFile)
            report.AppendLine($"| Database file | {Mb(new FileInfo(Database).Length)} MB{(File.Exists(Database + "-wal") ? $" (+ {Mb(new FileInfo(Database + "-wal").Length)} MB of WAL)" : "")} |");
        report.AppendLine($"| Photos | {photos.Count}: {photos.Count(p => !p.DescriptionByUser && !string.IsNullOrEmpty(p.DescriptionModel))} by the AI, " +
            $"{photos.Count(p => p.DescriptionByUser)} by a person |");
        report.AppendLine($"| Documents | {documents.Count}: {string.Join(", ", documents.GroupBy(d => (EnumExtractedState)d.ExtractedState).Select(g => $"{g.Key} {g.Count()}"))} |");
        report.AppendLine($"| Text of the documents | {textCharacters:N0} characters ({Mb(textCharacters * 2)} MB in memory as text), {pages} pages |");
        report.AppendLine($"| Scanned pages | {transcribed} transcribed, {pending} waiting |");

        var summary = (await service.SemanticSearch.GetIndexStatusAsync()).Entity;
        report.AppendLine($"| Index | {summary?.IndexedNotes} notes indexed, {summary?.PendingNotes} pending, {summary?.Chunks} fragments |").AppendLine();
        if (summary?.PendingNotes > 0)
            report.AppendLine($"> {summary.PendingNotes} notes are pending: each search puts some up to date first, and the times below include it. " +
                "Update the index of the copy and run it again.").AppendLine();

        // The first search, cold: it loads the cache of vectors of the process (it stays loaded, so the memory after it is the cache's).
        var queries = (Environment.GetEnvironmentVariable("KNOTE_STAGE4_MEASURE_QUERIES") is { Length: > 0 } list ? list.Split('|') : DefaultQueries)
            .Select(q => q.Trim()).Where(q => q.Length > 0).ToList();
        var before = GC.GetTotalMemory(true);
        var watch = Stopwatch.StartNew();
        var first = await service.SemanticSearch.SearchAsync(new SemanticSearchRequestDto { Text = queries[0] });
        watch.Stop();
        var after = GC.GetTotalMemory(true);
        Assert.IsTrue(first.IsValid, first.ErrorMessage);

        // What is in it, read again from the repository (and let go).
        var readWatch = Stopwatch.StartNew();
        var vectors = (await service.Repository.SemanticIndex.GetVectorsAsync(key)).Entity ?? new List<NoteChunkVectorDto>();
        readWatch.Stop();
        var fragments = vectors.Count;
        var dimensions = vectors.FirstOrDefault()?.Vector?.Length ?? 0;
        var byKind = string.Join(", ", vectors.GroupBy(c => c.SourceType).OrderBy(g => g.Key).Select(g => $"{g.Key} {g.Count():N0}"));
        vectors = null;
        report.AppendLine("## The cache of vectors").AppendLine();
        report.AppendLine("| | |").AppendLine("|---|---|");
        report.AppendLine($"| Fragments | {fragments:N0}: {byKind} |");
        report.AppendLine($"| Dimensions | {dimensions} |");
        report.AppendLine($"| Vectors | {Mb((long)fragments * dimensions * 4)} MB (float32) |");
        report.AppendLine($"| Memory of the cache | {Mb(after - before)} MB (managed memory before and after the first search) |");
        report.AppendLine($"| First search, cold | {watch.Elapsed.TotalMilliseconds:N0} ms (loads the cache; reading the vectors alone: {readWatch.Elapsed.TotalMilliseconds:N0} ms) |").AppendLine();

        // The search by meaning: the embedding of the question on its own, and the whole search (embedding, ranking and reading the
        // notes), warm, the median and the maximum of Runs after one to warm up.
        var generator = service.EmbeddingGenerators.Find(models.EmbeddingModel);
        report.AppendLine("## Search by meaning").AppendLine();
        report.AppendLine($"Median and maximum of {Runs} runs after one to warm up, in ms. Rest ≈ search − embedding: the ranking and reading the notes found.").AppendLine();
        report.AppendLine("| Query | Kind | Results | Embedding | Search (median) | Search (max) | Rest ≈ |").AppendLine("|---|---|---|---|---|---|---|");
        var rows = new List<object>();
        foreach (var query in queries)
        {
            var embedding = await TimeAsync(() => generator.GenerateAsync([query]));
            foreach (var kind in new[] { EnumResourceKind.All, EnumResourceKind.Photos, EnumResourceKind.Documents })
            {
                var found = 0;
                string? error = null;
                var search = await TimeAsync(async () =>
                {
                    var result = await service.SemanticSearch.SearchAsync(new SemanticSearchRequestDto { Text = query, ResourceKind = kind });
                    error = result.IsValid ? null : result.ErrorMessage;
                    found = kind == EnumResourceKind.All ? result.Entity?.Notes.Count ?? 0 : result.Entity?.Resources.Count ?? 0;
                });
                report.AppendLine(error != null
                    ? $"| {query} | {kind} | ERROR {error} | | | | |"
                    : $"| {query} | {kind} | {found} | {embedding.Median:N0} | {search.Median:N0} | {search.Max:N0} | {Math.Max(0, search.Median - embedding.Median):N0} |");
                rows.Add(new { query, kind = kind.ToString(), found, embeddingMs = embedding.Median, searchMs = search.Median, searchMaxMs = search.Max, error });
            }
        }

        Directory.CreateDirectory(Results);
        File.WriteAllText(Path.Combine(Results, "stage4-index.md"), report.ToString());
        File.WriteAllText(Path.Combine(Results, "stage4-index.json"), JsonSerializer.Serialize(new
        {
            key, dimensions, fragments, firstSearchMs = watch.Elapsed.TotalMilliseconds, readVectorsMs = readWatch.Elapsed.TotalMilliseconds, cacheBytes = after - before,
            textCharacters, pages, transcribed, pending, searches = rows
        }, Json));
    }

    private static bool Ready(out string? reason)
    {
        reason = string.IsNullOrWhiteSpace(Database) || string.IsNullOrWhiteSpace(ResourcesRoot) || string.IsNullOrWhiteSpace(Folder)
            ? "KNOTE_STAGE4_MEASURE_DB, KNOTE_STAGE4_MEASURE_RESOURCES and KNOTE_RESOURCE_MEASURE_FOLDER are not set - skipping the measure."
            : null;
        if (reason == null)
            Directory.CreateDirectory(Results);
        return reason == null;
    }

    private static RepositoryRef CopyRef() => new()
    {
        Orm = "EntityFramework",
        Provider = IsFile ? "Microsoft.Data.Sqlite" : "Microsoft.Data.SqlClient",
        ConnectionString = IsFile ? $"Data Source={Database}" : Database,
        ResourcesContainerRootPath = ResourcesRoot
    };

    private static KntService Open() =>
        new(KntRepositoryFactory.Create(CopyRef()), activateMessageBroker: false) { DocumentReader = new AiDocumentReader() };

    // All the resources of the copy but those of the system notes (conversations, chat room...), as the stage leaves them out.
    private static List<ResourceRow> ReadResources()
    {
        using DbConnection connection = IsFile ? new SqliteConnection(CopyRef().ConnectionString) : new SqlConnection(Database);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = @"SELECT r.ResourceId, r.[Name], r.FileType, r.[Description], r.DescriptionModel, r.DescriptionByUser, r.ExtractedState,
                CASE WHEN r.ExtractedText IS NULL THEN 0 ELSE 1 END, n.NoteNumber, n.Topic, t.[Name]
            FROM Resources r INNER JOIN Notes n ON n.NoteId = r.NoteId LEFT JOIN NoteTypes t ON t.NoteTypeId = n.NoteTypeId
            ORDER BY n.NoteNumber, r.[Name]";
        using var reader = command.ExecuteReader();
        var rows = new List<ResourceRow>();
        while (reader.Read())
        {
            string? Text(int i) => reader.IsDBNull(i) ? null : Convert.ToString(reader.GetValue(i));
            var row = new ResourceRow(Guid.Parse(Text(0)!), Text(1) ?? "", Text(2) ?? "", Text(3), Text(4), Convert.ToBoolean(reader.GetValue(5)),
                reader.IsDBNull(6) ? 0 : Convert.ToInt32(reader.GetValue(6)), Convert.ToInt32(reader.GetValue(7)) == 1,
                Convert.ToInt64(reader.GetValue(8)), Text(9), Text(10));
            if (!KntConst.IsSystemNoteType(row.NoteTypeName))
                rows.Add(row);
        }
        return rows;
    }

    private static List<T> Sample<T>(List<T> items, int count)
    {
        var random = new Random(Seed);
        return items.Select(item => (item, order: random.Next())).OrderBy(x => x.order).Take(count).Select(x => x.item).ToList();
    }

    private static async Task<(double Median, double Max)> TimeAsync(Func<Task> run)
    {
        var times = new List<double>();
        for (var i = 0; i <= Runs; i++)
        {
            var watch = Stopwatch.StartNew();
            await run();
            watch.Stop();
            if (i > 0)
                times.Add(watch.Elapsed.TotalMilliseconds);
        }
        times.Sort();
        return (times[times.Count / 2], times[^1]);
    }

    // A resource is kept as «{guid}_{file name}».
    private static string FileName(string name) =>
        name.Length > 37 && name[36] == '_' && Guid.TryParse(name[..36], out _) ? name[37..] : name;

    private static string Mb(long bytes) => (bytes / 1024d / 1024d).ToString("N1");

    // A page to rate each item, in the owner's browser: the choices are counted at the top, kept in the browser (localStorage, if it
    // can) and «Copy the results» puts them in the clipboard as text.
    private static void WriteSheet(string path, string title, string summary, string[] options, List<SheetItem> items, bool sideBySide = false)
    {
        static string H(string? text) => WebUtility.HtmlEncode(text ?? "");
        var html = new StringBuilder();
        html.Append($"<!doctype html><html><head><meta charset=\"utf-8\"><title>{H(title)}</title><style>")
            .Append("body{font-family:Segoe UI,sans-serif;margin:1.5rem;background:#fafafa;color:#222}")
            .Append(".item{background:#fff;border:1px solid #ddd;border-radius:6px;padding:1rem;margin:1rem 0}")
            .Append(".body{display:flex;gap:1rem;flex-wrap:wrap}.body img{max-width:" + (sideBySide ? "48%" : $"{PhotoSide}px") + ";border:1px solid #ccc}")
            .Append(".text{flex:1;min-width:280px;white-space:pre-wrap}.title{font-weight:600;margin-bottom:.5rem}.error{color:#b00}")
            .Append(".choices label{margin-right:1rem}#counts{position:sticky;top:0;background:#fafafa;padding:.5rem 0;font-weight:600}")
            .Append("</style></head><body>");
        html.Append($"<h1>{H(title)}</h1><p>Repository: {H(RepositoryLabel)} · {DateTime.Now:yyyy-MM-dd HH:mm}. {H(summary)}</p><div id=\"counts\"></div><button id=\"copy\">Copy the results</button>");
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            html.Append($"<div class=\"item\"><div class=\"title\">{i + 1}. {H(item.Title)}</div><div class=\"body\">");
            html.Append(item.Image != null ? $"<img src=\"{item.Image}\" alt=\"\">" : $"<div class=\"error\">{H(item.Error)}</div>");
            html.Append($"<div class=\"text\">{H(item.Text)}</div></div><div class=\"choices\">");
            foreach (var option in options)
                html.Append($"<label><input type=\"radio\" name=\"i{i}\" value=\"{H(option)}\"> {H(option)}</label>");
            html.Append($"<input type=\"text\" data-note=\"{i}\" placeholder=\"Comment\" size=\"50\"></div></div>");
        }
        html.Append("<script>")
            .Append($"const options={JsonSerializer.Serialize(options)},count={items.Count},key='knote-'+location.pathname;")
            .Append("function load(){try{return JSON.parse(localStorage.getItem(key)||'{}')}catch(e){return {}}}")
            .Append("function save(s){try{localStorage.setItem(key,JSON.stringify(s))}catch(e){}}")
            .Append("function state(){const s={};for(let i=0;i<count;i++){const c=document.querySelector(`input[name=i${i}]:checked`);")
            .Append("const n=document.querySelector(`input[data-note=\"${i}\"]`).value;s[i]={choice:c?c.value:null,note:n}}return s}")
            .Append("function counts(){const s=state();const t=options.map(o=>`${o}: ${Object.values(s).filter(x=>x.choice===o).length}`);")
            .Append("const left=Object.values(s).filter(x=>!x.choice).length;document.getElementById('counts').textContent=t.join(' · ')+` · not rated: ${left}`;save(s)}")
            .Append("const saved=load();for(const [i,v] of Object.entries(saved)){if(v.choice){const r=document.querySelector(`input[name=i${i}][value=\"${v.choice}\"]`);if(r)r.checked=true}")
            .Append("const n=document.querySelector(`input[data-note=\"${i}\"]`);if(n)n.value=v.note||''}")
            .Append("document.addEventListener('input',counts);counts();")
            .Append("document.getElementById('copy').onclick=()=>{const s=state();const lines=[document.getElementById('counts').textContent];")
            .Append("for(let i=0;i<count;i++)lines.push(`${i+1}. ${s[i].choice||'-'}${s[i].note?' · '+s[i].note:''}`);navigator.clipboard.writeText(lines.join('\\n'))};")
            .Append("</script></body></html>");
        File.WriteAllText(path, html.ToString());
    }
}
