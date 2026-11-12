using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using KNote.Ai;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using Microsoft.Extensions.AI;

namespace KNote.ClientWin.Tests;

// KNote Atlas 4.0 (and the speed of the stage's measure): the owner's photos and pages, through a real local model, one pass per
// test. Each element is saved as it ends, so a pass that is stopped goes on where it was; results\report.html is rewritten each
// time. Only with KNOTE_RESOURCE_MEASURE_FOLDER set, so that running the whole RequiresRealAiProvider suite does not start hours of
// work. See ClientWin.Tests/CLAUDE.md. The results hold what the files say: they stay in that folder.
[TestClass]
[TestCategory("RequiresRealAiProvider")]
public class ResourceVisionMeasure
{
    private const string SmallModel = "qwen3.5:9b";
    private const string BiggerModel = "qwen3.6:35b-a3b";
    private const int DefaultSide = 1600;
    private const int LargerSide = 2200;
    private const int PagesPerPdf = 5;
    private const int DefaultContextWindow = 131072;
    private const double MaxSecondsPerPhoto = 15;
    private const double MaxSecondsPerPage = 30;

    private static readonly string[] PhotoExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = false };

    private sealed record Pass(string Name, string Model, bool Reasoning, int Side, bool Photos);

    private static readonly Pass[] Passes =
    [
        new("A", SmallModel, false, DefaultSide, true),
        new("B", SmallModel, true, DefaultSide, true),
        new("C", SmallModel, false, LargerSide, false),
        new("D", BiggerModel, false, DefaultSide, true)
    ];

    private sealed class Row
    {
        public string Pass { get; set; } = "";
        public string Kind { get; set; } = "";
        public string Item { get; set; } = "";
        public string Image { get; set; } = "";
        public long ImageBytes { get; set; }
        public double Seconds { get; set; }
        public double FirstTokenSeconds { get; set; }
        public long? InputTokens { get; set; }
        public long? OutputTokens { get; set; }
        public int ReasoningChars { get; set; }
        public bool Cut { get; set; }
        public bool HasNote { get; set; }
        public string TextLayer { get; set; } = "";
        public string Text { get; set; } = "";
        public string Error { get; set; } = "";
    }

    private sealed record PassInfo(string Name, string Model, bool Reasoning, int Side, int ContextWindow, double LoadSeconds, string Loaded);

    private static string Folder => Environment.GetEnvironmentVariable("KNOTE_RESOURCE_MEASURE_FOLDER") ?? "";
    private static string Results => Path.Combine(Folder, "results");

    private static bool Skip()
    {
        if (string.IsNullOrWhiteSpace(Folder))
        {
            Assert.Inconclusive("KNOTE_RESOURCE_MEASURE_FOLDER is not set - skipping the measure.");
            return true;
        }
        Directory.CreateDirectory(Path.Combine(Results, "images"));
        return false;
    }

    [TestMethod] public Task PassA_SmallModel_NoReasoning() => RunPassAsync("A");
    [TestMethod] public Task PassB_SmallModel_Reasoning() => RunPassAsync("B");
    [TestMethod] public Task PassC_SmallModel_NoReasoning_LargerPages() => RunPassAsync("C");
    [TestMethod] public Task PassD_BiggerModel_NoReasoning() => RunPassAsync("D");

    // Without a model: what PdfPig sees in each page of the PDFs, and the text of the documents.
    [TestMethod]
    public void Documents()
    {
        if (Skip())
            return;

        var report = new StringBuilder();

        report.Append("<h2>Pages of the PDFs</h2><table><tr><th>File</th><th>Pages</th><th>Seconds</th><th>Need transcription</th><th>With text</th><th>Letters in the pages that need it</th><th>Error</th></tr>");
        foreach (var path in Files("scans", ".pdf").Concat(Files("text", ".pdf")))
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var pages = ResourceProbe.InspectPdf(path);
                var scanned = pages.Where(p => p.NeedsTranscription).ToList();
                report.Append($"<tr><td>{H(Relative(path))}</td><td>{pages.Count}</td><td>{watch.Elapsed.TotalSeconds:0.0}</td><td>{scanned.Count}</td><td>{pages.Count - scanned.Count}</td>" +
                    $"<td>{string.Join(", ", scanned.Select(p => p.Letters).Distinct().OrderBy(n => n))}</td><td></td></tr>");
            }
            catch (Exception ex)
            {
                report.Append($"<tr><td>{H(Relative(path))}</td><td></td><td>{watch.Elapsed.TotalSeconds:0.0}</td><td></td><td></td><td></td><td>{H(ex.GetType().Name + ": " + ex.Message)}</td></tr>");
            }
        }
        report.Append("</table>");

        report.Append("<h2>Text of the documents</h2>");
        foreach (var path in Files("text", ".pdf", ".docx", ".odt", ".txt", ".md"))
            report.Append(DocumentSection(path));

        File.WriteAllText(Path.Combine(Results, "documents.html.part"), report.ToString());
        WriteReport();
    }

    private static string DocumentSection(string path)
    {
        var name = H(Relative(path));
        try
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            switch (Path.GetExtension(path).ToLowerInvariant())
            {
                case ".pdf":
                    var pages = ResourceProbe.ReadPdfText(path);
                    var seconds = watch.Elapsed.TotalSeconds;
                    var raw = ResourceProbe.ReadPdfRawText(path);
                    var page = Enumerable.Range(0, pages.Count).FirstOrDefault(i => pages[i].Length > 200);
                    return $"<h3>{name}</h3><p>{pages.Count} pages, {pages.Sum(p => p.Length):N0} characters, {seconds:0.0} s. Page {page + 1}:</p>" +
                        $"<div class=\"pair\"><div><h4>In reading order</h4><pre>{H(Start(pages.ElementAtOrDefault(page)))}</pre></div>" +
                        $"<div><h4>As it comes in the file</h4><pre>{H(Start(raw.ElementAtOrDefault(page)))}</pre></div></div>";
                case ".docx":
                    var zipped = ResourceProbe.ReadDocxFromZip(path);
                    var openXml = ResourceProbe.ReadDocxWithOpenXml(path);
                    var same = ResourceProbe.Comparable(zipped) == ResourceProbe.Comparable(openXml);
                    return $"<h3>{name}</h3><p>Reader of the zip: {zipped.Length:N0} characters. OpenXml: {openXml.Length:N0}. <b>{(same ? "The same text" : "They differ")}</b>.</p>" +
                        (same ? $"<pre>{H(Start(zipped))}</pre>"
                              : $"<div class=\"pair\"><div><h4>Zip</h4><pre>{H(Start(FirstDifference(zipped, openXml)))}</pre></div><div><h4>OpenXml</h4><pre>{H(Start(FirstDifference(openXml, zipped)))}</pre></div></div>");
                case ".odt":
                    var odt = ResourceProbe.ReadOdt(path);
                    return $"<h3>{name}</h3><p>{odt.Length:N0} characters, {watch.Elapsed.TotalSeconds:0.0} s.</p><pre>{H(Start(odt))}</pre>";
                default:
                    var text = File.ReadAllText(path);
                    return $"<h3>{name}</h3><p>{text.Length:N0} characters.</p><pre>{H(Start(text))}</pre>";
            }
        }
        catch (Exception ex)
        {
            return $"<h3>{name}</h3><p class=\"bad\">{H(ex.GetType().Name + ": " + ex.Message)}</p>";
        }
    }

    private static string Start(string? text) => text == null ? "" : text.Length <= 1500 ? text : text[..1500] + "…";

    private static string FirstDifference(string text, string other)
    {
        var a = ResourceProbe.Comparable(text);
        var b = ResourceProbe.Comparable(other);
        var i = 0;
        while (i < a.Length && i < b.Length && a[i] == b[i])
            i++;
        return a[Math.Max(0, i - 100)..];
    }

    private static async Task RunPassAsync(string name)
    {
        if (Skip())
            return;

        var pass = Passes.Single(p => p.Name == name);
        var settings = AiTestConfig.TryGetOllamaProviderRef();
        if (settings == null || !await AiTestConfig.IsOllamaReachableAsync(settings.Host))
        {
            Assert.Inconclusive("Ollama is not configured or not reachable - skipping the measure.");
            return;
        }

        var context = int.TryParse(Environment.GetEnvironmentVariable("KNOTE_RESOURCE_MEASURE_CONTEXT"), out var value) ? value : DefaultContextWindow;
        var provider = new AiProviderRef { Alias = "Measure", Provider = EnumAiProvider.Ollama, Model = pass.Model, Host = settings.Host, ContextWindow = context };
        var client = AiChatClientFactory.Create(provider);

        var done = ReadRows(pass.Name).Where(r => string.IsNullOrEmpty(r.Error)).Select(r => r.Item).ToHashSet();
        var loadSeconds = await LoadModelAsync(client);
        File.WriteAllText(Path.Combine(Results, $"pass-{pass.Name}.info.json"), JsonSerializer.Serialize(
            new PassInfo(pass.Name, pass.Model, pass.Reasoning, pass.Side, context, loadSeconds, await LoadedModelAsync(settings.Host)), Json));

        if (pass.Photos)
        {
            foreach (var photo in Files("photos", PhotoExtensions).Where(p => !done.Contains(Relative(p))))
                await MeasureAsync(pass, Relative(photo), "photo", async row =>
                {
                    var content = await File.ReadAllBytesAsync(photo);
                    var contentType = "image/" + Path.GetExtension(photo).TrimStart('.').Replace("jpg", "jpeg").ToLowerInvariant();
                    var reduced = AiImageReducer.Reduce(content, contentType) ?? throw new InvalidDataException("The image could not be read.");

                    row.Image = SaveImage($"{Path.GetFileNameWithoutExtension(photo)}.jpg", reduced.Content);
                    row.ImageBytes = reduced.Content.Length;

                    var notePath = photo + ".txt";
                    var note = File.Exists(notePath) ? await File.ReadAllTextAsync(notePath) : null;
                    row.HasNote = note != null;
                    return await ResourceVisionRun.DescribePhotoAsync(client, provider.Provider, reduced.Content, reduced.ContentType, note, "Spanish", pass.Reasoning);
                });
        }

        foreach (var (path, page) in PagesToTranscribe().Where(p => !done.Contains(PageItem(p.Path, p.Page))))
            await MeasureAsync(pass, PageItem(path, page), "page", async row =>
            {
                var image = ResourceProbe.DrawPage(path, page, pass.Side);
                row.Image = SaveImage($"{Path.GetFileNameWithoutExtension(path)}-p{page}-{pass.Side}.jpg", image);
                row.ImageBytes = image.Length;
                row.TextLayer = ResourceProbe.ReadPdfText(path)[page - 1];
                return await ResourceVisionRun.TranscribePageAsync(client, provider.Provider, image, pass.Reasoning);
            });
    }

    private static async Task MeasureAsync(Pass pass, string item, string kind, Func<Row, Task<ResourceVisionRun.Outcome>> run)
    {
        var row = new Row { Pass = pass.Name, Kind = kind, Item = item };
        try
        {
            var outcome = await run(row);
            row.Seconds = outcome.Seconds;
            row.FirstTokenSeconds = outcome.FirstTokenSeconds;
            row.InputTokens = outcome.InputTokens;
            row.OutputTokens = outcome.OutputTokens;
            row.ReasoningChars = outcome.ReasoningChars;
            row.Cut = outcome.Cut;
            row.Text = outcome.Text;
        }
        catch (Exception ex)
        {
            row.Error = ex.GetType().Name + ": " + ex.Message;
        }

        File.AppendAllText(Path.Combine(Results, $"pass-{pass.Name}.jsonl"), JsonSerializer.Serialize(row, Json) + "\n");
        WriteReport();
    }

    // The first call loads the model, and it must not count. It may take longer than HttpClient waits: Ollama goes on loading, so it is asked again.
    private static async Task<double> LoadModelAsync(IChatClient client)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await foreach (var _ in client.GetStreamingResponseAsync([new ChatMessage(ChatRole.User, "Reply with one word: OK.")],
                    new ChatOptions { MaxOutputTokens = 1, Reasoning = new ReasoningOptions { Effort = ReasoningEffort.None } }))
                {
                }
                return watch.Elapsed.TotalSeconds;
            }
            catch (Exception ex) when (attempt < 5 && ex is not OutOfMemoryException)
            {
            }
        }
    }

    // What Ollama says it has loaded: how much of it is in the GPU, and with what context.
    private static async Task<string> LoadedModelAsync(string host)
    {
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var ps = await http.GetFromJsonAsync<JsonElement>($"{host.TrimEnd('/')}/api/ps");
            return string.Join("; ", ps.GetProperty("models").EnumerateArray().Select(m =>
            {
                var size = m.GetProperty("size").GetInt64();
                var vram = m.GetProperty("size_vram").GetInt64();
                var contextLength = m.TryGetProperty("context_length", out var c) ? c.GetRawText() : "?";
                return $"{m.GetProperty("name").GetString()}: {size / 1e9:0.0} GB, {100.0 * vram / Math.Max(size, 1):0}% in the GPU, context {contextLength}";
            }));
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    // The same number of each PDF, spread along it: the first pages of a book are not its usual ones, and another PDF in the folder
    // does not change the pages that the passes already measured. Also the pages of a scan that has a text layer: the rule of D1
    // takes them as text, and comparing the layer with the transcription is what says if that is right.
    private static List<(string Path, int Page)> PagesToTranscribe() =>
        Files("scans", ".pdf").SelectMany(path => Spread(PagesWithImage(path), PagesPerPdf).Select(page => (path, page))).ToList();

    private static List<int> PagesWithImage(string path)
    {
        try
        {
            return ResourceProbe.InspectPdf(path).Where(p => p.Images > 0).Select(p => p.Page).ToList();
        }
        catch (Exception)
        {
            return [];
        }
    }

    private static List<int> Spread(List<int> pages, int count) =>
        pages.Count <= count ? pages : Enumerable.Range(0, count).Select(i => pages[(2 * i + 1) * pages.Count / (2 * count)]).ToList();

    private static string PageItem(string path, int page) => $"{Relative(path)} p.{page}";

    private static IEnumerable<string> Files(string folder, params string[] extensions)
    {
        var path = Path.Combine(Folder, folder);
        return Directory.Exists(path)
            ? Directory.EnumerateFiles(path).Where(f => extensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase)).OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            : [];
    }

    private static string Relative(string path) => Path.GetRelativePath(Folder, path).Replace('\\', '/');

    private static string SaveImage(string name, byte[] content)
    {
        File.WriteAllBytes(Path.Combine(Results, "images", name), content);
        return "images/" + name;
    }

    private static List<Row> ReadRows(string pass)
    {
        var path = Path.Combine(Results, $"pass-{pass}.jsonl");
        return File.Exists(path)
            ? File.ReadAllLines(path).Where(l => l.Length > 0).Select(l => JsonSerializer.Deserialize<Row>(l, Json)!).ToList()
            : [];
    }

    #region The report

    private static string H(string? text) => WebUtility.HtmlEncode(text ?? "");

    private static void WriteReport()
    {
        var passes = Passes.Select(p => (Pass: p, Rows: ReadRows(p.Name).GroupBy(r => r.Item).Select(g => g.Last()).ToList())).Where(p => p.Rows.Count > 0).ToList();
        var html = new StringBuilder("""
            <!doctype html><meta charset="utf-8"><title>KNote Atlas 4.0</title>
            <style>
            body{font:14px/1.45 system-ui,sans-serif;margin:24px;max-width:1500px}
            table{border-collapse:collapse;margin:8px 0}td,th{border:1px solid #ccc;padding:4px 8px;text-align:left;vertical-align:top}
            pre{white-space:pre-wrap;background:#f6f6f6;padding:8px;margin:4px 0}
            .item{display:flex;gap:16px;border-top:2px solid #999;padding:12px 0}.item img{max-width:560px;max-height:760px;flex:none;border:1px solid #ccc}
            .pair{display:flex;gap:16px}.pair>div{flex:1;min-width:0}.bad{color:#b00020}.meta{color:#555;font-size:12px}
            </style><h1>KNote Atlas 4.0: photos and pages</h1>
            """);

        html.Append("<h2>Speed</h2><table><tr><th>Pass</th><th>Model</th><th>Reasoning</th><th>Page side</th><th>Loaded</th><th>Kind</th><th>Done</th><th>Errors</th><th>Cut</th><th>Median s</th><th>Mean s</th><th>Slowest s</th><th>Median tokens out</th><th>Criterion</th></tr>");
        foreach (var (pass, rows) in passes)
        {
            var infoPath = Path.Combine(Results, $"pass-{pass.Name}.info.json");
            var info = File.Exists(infoPath) ? JsonSerializer.Deserialize<PassInfo>(File.ReadAllText(infoPath), Json) : null;
            foreach (var kind in new[] { "photo", "page" })
            {
                var all = rows.Where(r => r.Kind == kind).ToList();
                if (all.Count == 0)
                    continue;

                var good = all.Where(r => string.IsNullOrEmpty(r.Error)).ToList();
                var median = Median(good.Select(r => r.Seconds));
                var limit = kind == "photo" ? MaxSecondsPerPhoto : MaxSecondsPerPage;
                html.Append($"<tr><td>{pass.Name}</td><td>{H(pass.Model)}</td><td>{(pass.Reasoning ? "on" : "off")}</td><td>{pass.Side}</td><td>{H(info?.Loaded)} (load {info?.LoadSeconds:0} s)</td><td>{kind}</td><td>{good.Count}</td><td>{all.Count - good.Count}</td><td>{good.Count(r => r.Cut)}</td>" +
                    $"<td>{median:0.0}</td><td>{(good.Count > 0 ? good.Average(r => r.Seconds) : 0):0.0}</td><td>{(good.Count > 0 ? good.Max(r => r.Seconds) : 0):0.0}</td><td>{Median(good.Select(r => (double)(r.OutputTokens ?? 0))):0}</td>" +
                    $"<td>{(median <= limit ? "within" : "<span class=\"bad\">over</span>")} {limit:0} s</td></tr>");
            }
        }
        html.Append("</table>");

        var partPath = Path.Combine(Results, "documents.html.part");
        if (File.Exists(partPath))
            html.Append(File.ReadAllText(partPath));

        html.Append("<h2>Each element</h2>");
        foreach (var kind in new[] { "photo", "page" })
            foreach (var item in passes.SelectMany(p => p.Rows).Where(r => r.Kind == kind).Select(r => r.Item).Distinct())
            {
                var rows = passes.Select(p => (p.Pass, Row: p.Rows.FirstOrDefault(r => r.Item == item))).Where(p => p.Row != null).ToList();
                html.Append($"<div class=\"item\"><img loading=\"lazy\" src=\"{H(rows.Select(r => r.Row!.Image).FirstOrDefault(i => i.Length > 0))}\" alt=\"\"><div><h3>{H(item)}</h3>");
                var layer = rows.Select(r => r.Row!.TextLayer).FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));
                if (layer != null)
                    html.Append($"<div class=\"meta\"><b>Text layer of the PDF: what the rule of D1 would keep instead of a transcription</b></div><pre>{H(layer)}</pre>");
                foreach (var (pass, row) in rows)
                {
                    html.Append($"<div class=\"meta\">Pass {pass.Name}: {H(pass.Model)}, reasoning {(pass.Reasoning ? "on" : "off")}, {pass.Side} px: {row!.Seconds:0.0} s (first text at {row.FirstTokenSeconds:0.0} s), " +
                        $"{row.InputTokens} tokens in, {row.OutputTokens} out, {row.ReasoningChars:N0} characters of reasoning, image {row.ImageBytes / 1024:N0} KB{(row.HasNote ? ", with its note" : "")}{(row.Cut ? ", <span class=\"bad\">CUT</span>" : "")}</div>");
                    html.Append(string.IsNullOrEmpty(row.Error) ? $"<pre>{H(row.Text)}</pre>" : $"<p class=\"bad\">{H(row.Error)}</p>");
                }
                html.Append("</div></div>");
            }

        File.WriteAllText(Path.Combine(Results, "report.html"), html.ToString());
    }

    private static double Median(IEnumerable<double> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        return sorted.Count == 0 ? 0 : sorted.Count % 2 == 1 ? sorted[sorted.Count / 2] : (sorted[sorted.Count / 2 - 1] + sorted[sorted.Count / 2]) / 2;
    }

    #endregion
}
