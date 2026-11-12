using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using KNote.Ai;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using Microsoft.Extensions.AI;

namespace KNote.ClientWin.Tests;

// KNote Atlas 4.5: the transcription of the owner's scanned pages (the scans\ folder of the measure, the same pages as 4.0: 5 with an
// image spread along each PDF) through a real local model, with the task, the request, the drawing, the options and the cleaning
// of production. It counts the images the model invents (4.0: «![Image](https://i.imgur.com/...)»), the code blocks, the pages
// cut and the seconds. Each page is saved as it ends (results\transcriptions.jsonl, results\transcriptions.html), so a run that is
// stopped goes on where it was. Only with KNOTE_RESOURCE_MEASURE_FOLDER set. The results hold what the files say: they stay in
// that folder.
[TestClass]
[TestCategory("RequiresRealAiProvider")]
public class ResourceTranscriptionMeasure
{
    private const string Model = "qwen3.5:9b";
    private const int DefaultContextWindow = 16384;
    private const int PagesPerPdf = 5;
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = false };

    private sealed class Row
    {
        public string Item { get; set; } = "";
        public string File { get; set; } = "";
        public int Page { get; set; }
        public int Pages { get; set; }
        public double DrawSeconds { get; set; }
        public double Seconds { get; set; }
        public long? InputTokens { get; set; }
        public long? OutputTokens { get; set; }
        public bool Cut { get; set; }
        public int InventedImages { get; set; }
        public bool CodeBlock { get; set; }
        public string Raw { get; set; } = "";
        public string Text { get; set; } = "";
        public string Error { get; set; } = "";
    }

    private static string Folder => Environment.GetEnvironmentVariable("KNOTE_RESOURCE_MEASURE_FOLDER") ?? "";
    private static string Results => Path.Combine(Folder, "results");
    private static string RowsFile => Path.Combine(Results, "transcriptions.jsonl");

    [TestMethod]
    public async Task Transcriptions()
    {
        if (string.IsNullOrWhiteSpace(Folder))
        {
            Assert.Inconclusive("KNOTE_RESOURCE_MEASURE_FOLDER is not set - skipping the measure.");
            return;
        }
        var settings = AiTestConfig.TryGetOllamaProviderRef();
        if (settings == null || !await AiTestConfig.IsOllamaReachableAsync(settings.Host))
        {
            Assert.Inconclusive("Ollama is not configured or not reachable - skipping the measure.");
            return;
        }
        Directory.CreateDirectory(Results);

        var model = Environment.GetEnvironmentVariable("KNOTE_RESOURCE_SMOKE_MODEL") ?? Model;
        var context = int.TryParse(Environment.GetEnvironmentVariable("KNOTE_RESOURCE_MEASURE_CONTEXT"), out var value) ? value : DefaultContextWindow;
        var provider = new AiProviderRef { Alias = "Measure", Provider = EnumAiProvider.Ollama, Model = model, Host = settings.Host, ContextWindow = context };
        var client = AiChatClientFactory.Create(provider);
        var system = AiPromptComposer.Compose(new AiPromptComposer.Parts { Task = AiTaskCatalog.TranscribePage, RepositoryName = "Measure" }).Text;
        var reader = new AiDocumentReader();

        var done = ReadRows().Where(r => string.IsNullOrEmpty(r.Error)).Select(r => r.Item).ToHashSet();
        await WarmUpAsync(client);

        foreach (var path in Files())
        {
            var fileName = Path.GetFileName(path);
            List<int> pages;
            int count;
            try
            {
                var inspected = ResourceProbe.InspectPdf(path);
                count = inspected.Count;
                pages = Spread(inspected.Where(p => p.Images > 0).Select(p => p.Page).ToList(), PagesPerPdf);
            }
            catch (Exception ex)
            {
                Append(new Row { Item = fileName, File = fileName, Error = ex.GetType().Name + ": " + ex.Message });
                continue;
            }

            foreach (var page in pages)
            {
                var item = $"{fileName} p.{page}";
                if (done.Contains(item))
                    continue;
                var row = new Row { Item = item, File = fileName, Page = page, Pages = count };
                try
                {
                    var draw = Stopwatch.StartNew();
                    byte[] image;
                    using (var stream = File.OpenRead(path))
                        image = reader.DrawPage(stream, page, AiTranscriptionRules.PageImageSide);
                    row.DrawSeconds = draw.Elapsed.TotalSeconds;
                    await RunAsync(client, system, AiTranscriptionRules.Request(fileName, page, count), image, row);
                }
                catch (Exception ex)
                {
                    row.Error = ex.GetType().Name + ": " + ex.Message;
                }
                Append(row);
            }
        }
        WriteReport(model, context);
    }

    // The options of production for Ollama (AiResourceTask with AiTranscriptionRules.MaxOutputTokens).
    private static async Task RunAsync(IChatClient client, string system, string request, byte[] image, Row row)
    {
        var options = new ChatOptions
        {
            Temperature = 0.2f, MaxOutputTokens = AiTranscriptionRules.MaxOutputTokens, Reasoning = new ReasoningOptions { Effort = ReasoningEffort.None }
        };
        var answer = new StringBuilder();
        var watch = Stopwatch.StartNew();
        List<ChatMessage> messages = [new(ChatRole.System, system), new(ChatRole.User, [new TextContent(request), new DataContent(image, "image/jpeg")])];
        await foreach (var update in client.GetStreamingResponseAsync(messages, options))
        {
            if (update.FinishReason == ChatFinishReason.Length)
                row.Cut = true;
            foreach (var content in update.Contents)
                if (content is TextContent text)
                    answer.Append(text.Text);
                else if (content is UsageContent usage)
                    (row.InputTokens, row.OutputTokens) = (usage.Details.InputTokenCount, usage.Details.OutputTokenCount);
        }
        row.Seconds = watch.Elapsed.TotalSeconds;
        row.Raw = answer.ToString();
        row.InventedImages = System.Text.RegularExpressions.Regex.Matches(row.Raw, @"!\[[^\]\n]*\]\([^)\n]*\)").Count;
        row.CodeBlock = row.Raw.TrimStart().StartsWith("```", StringComparison.Ordinal);
        row.Text = AiTranscriptionRules.CleanTranscription(row.Raw);
    }

    // The first call loads the model: it doesn't count.
    private static async Task WarmUpAsync(IChatClient client)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await foreach (var _ in client.GetStreamingResponseAsync([new ChatMessage(ChatRole.User, "Reply with one word: OK.")],
                    new ChatOptions { MaxOutputTokens = 1, Reasoning = new ReasoningOptions { Effort = ReasoningEffort.None } }))
                {
                }
                return;
            }
            catch (Exception) when (attempt < 5)
            {
            }
        }
    }

    private static IEnumerable<string> Files()
    {
        var path = Path.Combine(Folder, "scans");
        return Directory.Exists(path)
            ? Directory.EnumerateFiles(path, "*.pdf").OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            : [];
    }

    // As 4.0 (ResourceVisionMeasure): the same number of each PDF, spread along it.
    private static List<int> Spread(List<int> pages, int count) =>
        pages.Count <= count ? pages : Enumerable.Range(0, count).Select(i => pages[(2 * i + 1) * pages.Count / (2 * count)]).ToList();

    private static List<Row> ReadRows() =>
        File.Exists(RowsFile)
            ? File.ReadAllLines(RowsFile).Where(l => l.Length > 0).Select(l => JsonSerializer.Deserialize<Row>(l, Json)!).ToList()
            : new List<Row>();

    private static void Append(Row row) => File.AppendAllText(RowsFile, JsonSerializer.Serialize(row, Json) + "\n");

    private static void WriteReport(string model, int context)
    {
        static string H(string? text) => WebUtility.HtmlEncode(text ?? "");
        var rows = ReadRows().GroupBy(r => r.Item).Select(g => g.Last()).OrderBy(r => r.File).ThenBy(r => r.Page).ToList();
        var ok = rows.Where(r => string.IsNullOrEmpty(r.Error)).ToList();
        var seconds = ok.Select(r => r.Seconds).Order().ToList();
        var median = seconds.Count == 0 ? 0 : seconds[seconds.Count / 2];
        var html = new StringBuilder();
        html.Append("<!doctype html><meta charset=\"utf-8\"><title>Transcriptions</title><style>body{font-family:Segoe UI,sans-serif;margin:2rem}" +
            "table{border-collapse:collapse}td,th{border:1px solid #ccc;padding:.4rem;vertical-align:top}pre{white-space:pre-wrap;max-width:60rem;margin:0}</style>");
        html.Append($"<h1>Transcriptions of the scanned pages</h1><p>Model {H(model)}, context {context}. {ok.Count} pages, median {median:0.0} s " +
            $"(max. {(seconds.Count == 0 ? 0 : seconds[^1]):0.0}), {ok.Sum(r => r.InventedImages)} images invented, {ok.Count(r => r.CodeBlock)} in a code block, " +
            $"{ok.Count(r => r.Cut)} cut, {ok.Count(r => r.Text.Length == 0)} empty.</p>");
        html.Append("<table><tr><th>Page</th><th>Seconds</th><th>Tokens in / out</th><th>Images</th><th>Transcription (as kept)</th></tr>");
        foreach (var r in rows)
            html.Append($"<tr><td>{H(r.Item)}</td><td>{r.Seconds:0.0} (+{r.DrawSeconds:0.0})</td><td>{r.InputTokens} / {r.OutputTokens}{(r.Cut ? " (cut)" : "")}</td>" +
                $"<td>{r.InventedImages}{(r.CodeBlock ? ", block" : "")}</td><td><pre>{H(string.IsNullOrEmpty(r.Error) ? r.Text : "ERROR " + r.Error)}</pre></td></tr>");
        html.Append("</table>");
        File.WriteAllText(Path.Combine(Results, "transcriptions.html"), html.ToString());
    }
}
