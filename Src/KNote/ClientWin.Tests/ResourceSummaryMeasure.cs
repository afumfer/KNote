using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using KNote.Ai;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Model.Dto;
using Microsoft.Extensions.AI;

namespace KNote.ClientWin.Tests;

// KNote Atlas 4.4c: the short summary of the owner's documents (the text\ folder of the measure) through a real local model, with
// the task, the request and the options of production, with 4,000, 8,000 and 16,000 characters of each document. Each element is
// saved as it ends (results\summaries.jsonl, results\summaries.html), so a run that is stopped goes on where it was. Only with
// KNOTE_RESOURCE_MEASURE_FOLDER set; the note is KNOTE_RESOURCE_MEASURE_NOTE (by default a Spanish topic, as the owner's notes).
// The results hold what the files say: they stay in that folder.
[TestClass]
[TestCategory("RequiresRealAiProvider")]
public class ResourceSummaryMeasure
{
    private const string Model = "qwen3.5:9b";
    private const int DefaultContextWindow = 16384;
    private const string DefaultNote = "Documentación del Sinclair QL";
    private static readonly int[] Lengths = [4_000, 8_000, 16_000];
    private static readonly string[] Extensions = [".pdf", ".docx", ".odt", ".txt", ".md"];
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = false };

    private sealed class Row
    {
        public string Item { get; set; } = "";
        public string File { get; set; } = "";
        public int Length { get; set; }
        public int Pages { get; set; }
        public int CharsSent { get; set; }
        public double Seconds { get; set; }
        public long? InputTokens { get; set; }
        public long? OutputTokens { get; set; }
        public bool Cut { get; set; }
        public string Summary { get; set; } = "";
        public string Error { get; set; } = "";
    }

    private static string Folder => Environment.GetEnvironmentVariable("KNOTE_RESOURCE_MEASURE_FOLDER") ?? "";
    private static string Results => Path.Combine(Folder, "results");
    private static string RowsFile => Path.Combine(Results, "summaries.jsonl");

    [TestMethod]
    public async Task Summaries()
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
        var note = Environment.GetEnvironmentVariable("KNOTE_RESOURCE_MEASURE_NOTE") ?? DefaultNote;
        var provider = new AiProviderRef { Alias = "Measure", Provider = EnumAiProvider.Ollama, Model = model, Host = settings.Host, ContextWindow = context };
        var client = AiChatClientFactory.Create(provider);
        var system = AiPromptComposer.Compose(new AiPromptComposer.Parts { Task = AiTaskCatalog.SummarizeDocument, RepositoryName = "Measure" }).Text;

        var done = ReadRows().Where(r => string.IsNullOrEmpty(r.Error)).Select(r => r.Item).ToHashSet();
        await WarmUpAsync(client);

        foreach (var path in Directory.GetFiles(Path.Combine(Folder, "text")).Where(f => Extensions.Contains(Path.GetExtension(f).ToLowerInvariant())).Order())
        {
            var fileName = Path.GetFileName(path);
            var format = DocumentTextRules.FormatOf("", fileName);
            string extracted;
            int pages;
            try
            {
                using var stream = File.OpenRead(path);
                var read = new AiDocumentReader().Read(stream, format);
                extracted = DocumentTextRules.Compose(read, DocumentTextRules.IsPaged(format));
                pages = read.Count;
            }
            catch (Exception ex)
            {
                Append(new Row { Item = fileName, File = fileName, Error = ex.GetType().Name + ": " + ex.Message });
                continue;
            }

            foreach (var length in Lengths)
            {
                var item = $"{fileName} · {length}";
                if (done.Contains(item))
                    continue;
                var row = new Row { Item = item, File = fileName, Length = length, Pages = pages };
                try
                {
                    var documentText = AiDocumentRules.DocumentText(extracted, DocumentTextRules.IsPaged(format), length);
                    row.CharsSent = documentText.Length;
                    var request = AiDocumentRules.Request(note, "", fileName, pages, documentText);
                    await RunAsync(client, system, request, row);
                }
                catch (Exception ex)
                {
                    row.Error = ex.GetType().Name + ": " + ex.Message;
                }
                Append(row);
            }
        }
        WriteReport(model, context, note);
    }

    // The language of the summary with 16,000 characters: (a) the request of production; (b) its last line naming the topic of the
    // note; (c) the request of production with a line in the instructions of the task (Add), as the owner did with the photos.
    [TestMethod]
    public async Task Languages()
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
        var note = Environment.GetEnvironmentVariable("KNOTE_RESOURCE_MEASURE_NOTE") ?? DefaultNote;
        var provider = new AiProviderRef { Alias = "Measure", Provider = EnumAiProvider.Ollama, Model = model, Host = settings.Host, ContextWindow = DefaultContextWindow };
        var client = AiChatClientFactory.Create(provider);
        var plain = AiPromptComposer.Compose(new AiPromptComposer.Parts { Task = AiTaskCatalog.SummarizeDocument, RepositoryName = "Measure" }).Text;
        var withLine = AiPromptComposer.Compose(new AiPromptComposer.Parts
        {
            Task = AiTaskCatalog.SummarizeDocument, RepositoryName = "Measure", TaskPromptText = "Escribe siempre los resúmenes en español."
        }).Text;
        await WarmUpAsync(client);

        var output = Path.Combine(Results, "summaries-language.jsonl");
        foreach (var path in Directory.GetFiles(Path.Combine(Folder, "text")).Where(f => Extensions.Contains(Path.GetExtension(f).ToLowerInvariant())).Order())
        {
            var fileName = Path.GetFileName(path);
            var format = DocumentTextRules.FormatOf("", fileName);
            using var stream = File.OpenRead(path);
            var read = new AiDocumentReader().Read(stream, format);
            var text = AiDocumentRules.DocumentText(DocumentTextRules.Compose(read, DocumentTextRules.IsPaged(format)), DocumentTextRules.IsPaged(format));
            var request = AiDocumentRules.Request(note, "", fileName, read.Count, text);
            var named = request[..request.LastIndexOf("Summarize this document", StringComparison.Ordinal)] +
                $"Summarize this document in two or three sentences, without mentioning the note. Write in the language of the topic of the note («{note}»).";

            foreach (var (variant, system, user) in new[] { ("a", plain, request), ("b", plain, named), ("c", withLine, request) })
            {
                var row = new Row { Item = $"{fileName} · {variant}", File = fileName, Length = AiDocumentRules.MaxDocumentTextLength, Pages = read.Count, CharsSent = text.Length };
                try
                {
                    await RunAsync(client, system, user, row);
                }
                catch (Exception ex)
                {
                    row.Error = ex.GetType().Name + ": " + ex.Message;
                }
                File.AppendAllText(output, JsonSerializer.Serialize(row, Json) + "\n");
            }
        }
    }

    // The options of production for Ollama (AiPhotoDescriber): no reasoning, a cap and a low temperature.
    private static async Task RunAsync(IChatClient client, string system, string request, Row row)
    {
        var options = new ChatOptions { Temperature = 0.2f, MaxOutputTokens = 500, Reasoning = new ReasoningOptions { Effort = ReasoningEffort.None } };
        var answer = new StringBuilder();
        var watch = Stopwatch.StartNew();
        await foreach (var update in client.GetStreamingResponseAsync([new ChatMessage(ChatRole.System, system), new ChatMessage(ChatRole.User, request)], options))
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
        row.Summary = AiPhotoRules.CleanDescription(answer.ToString());
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

    private static List<Row> ReadRows() =>
        File.Exists(RowsFile)
            ? File.ReadAllLines(RowsFile).Where(l => l.Length > 0).Select(l => JsonSerializer.Deserialize<Row>(l, Json)!).ToList()
            : new List<Row>();

    private static void Append(Row row) => File.AppendAllText(RowsFile, JsonSerializer.Serialize(row, Json) + "\n");

    private static void WriteReport(string model, int context, string note)
    {
        static string H(string? text) => WebUtility.HtmlEncode(text ?? "");
        // The last row of each item (a run that failed and was repeated).
        var rows = ReadRows().GroupBy(r => r.Item).Select(g => g.Last()).OrderBy(r => r.File).ThenBy(r => r.Length).ToList();
        var html = new StringBuilder();
        html.Append("<!doctype html><meta charset=\"utf-8\"><title>Summaries</title><style>body{font-family:Segoe UI,sans-serif;margin:2rem}" +
            "table{border-collapse:collapse}td,th{border:1px solid #ccc;padding:.4rem;vertical-align:top}td.s{max-width:46rem}</style>");
        html.Append($"<h1>Summaries of the documents</h1><p>Model {H(model)}, context {context}, note «{H(note)}».</p>");
        html.Append("<table><tr><th>File</th><th>Chars</th><th>Pages</th><th>Sent</th><th>Seconds</th><th>Tokens in / out</th><th>Summary</th></tr>");
        foreach (var r in rows)
            html.Append($"<tr><td>{H(r.File)}</td><td>{r.Length}</td><td>{r.Pages}</td><td>{r.CharsSent}</td><td>{r.Seconds:0.0}</td>" +
                $"<td>{r.InputTokens} / {r.OutputTokens}{(r.Cut ? " (cut)" : "")}</td><td class=\"s\">{H(string.IsNullOrEmpty(r.Error) ? r.Summary : "ERROR " + r.Error)}</td></tr>");
        html.Append("</table>");
        File.WriteAllText(Path.Combine(Results, "summaries.html"), html.ToString());
    }
}
