using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using KNote.Ai;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The measure of the summary of a period (KNote Atlas, 2.5a): against a COPY of a real repository (it writes in it: the answers of the
/// AI about the doubtful notes and the summary), the selection of a period (and its keywords, if given), its counts, the summary with a
/// real local model, its calls, time and pace, and which citations are of notes it read. The result goes to results\period-*.md under
/// KNOTE_RESOURCE_MEASURE_FOLDER (or next to the copy), outside the repository: it has what the notes say.
/// Variables: KNOTE_PERIOD_MEASURE_DB (a SQLite file or a SQL Server connection string), KNOTE_PERIOD_MEASURE_FROM and _TO (yyyy-MM-dd),
/// optional _KEYWORDS, _WORDS (2000) and _LANGUAGE (Spanish); KNOTE_RESOURCE_SMOKE_MODEL (an Ollama model; host OLLAMA_HOST or localhost).
/// Without them, Inconclusive. Filter by name: never the category.
/// </summary>
[TestClass]
[TestCategory("RequiresRealAiProvider")]
public class PeriodSummaryMeasure
{
    private static string Env(string name, string fallback = "") => Environment.GetEnvironmentVariable(name) is { Length: > 0 } value ? value : fallback;

    private static string Database => Env("KNOTE_PERIOD_MEASURE_DB");
    private static bool IsFile => File.Exists(Database);

    [TestMethod]
    public async Task Summarize()
    {
        var model = Env("KNOTE_RESOURCE_SMOKE_MODEL");
        if (string.IsNullOrWhiteSpace(Database) || string.IsNullOrWhiteSpace(model)
            || !DateTime.TryParse(Env("KNOTE_PERIOD_MEASURE_FROM"), out var from) || !DateTime.TryParse(Env("KNOTE_PERIOD_MEASURE_TO"), out var to))
        {
            Assert.Inconclusive("KNOTE_PERIOD_MEASURE_DB, KNOTE_PERIOD_MEASURE_FROM, KNOTE_PERIOD_MEASURE_TO and KNOTE_RESOURCE_SMOKE_MODEL are not set - skipping the measure.");
            return;
        }
        var provider = new AiProviderRef { Alias = "Measure", Provider = EnumAiProvider.Ollama, Model = model, Host = Env("OLLAMA_HOST", "http://localhost:11434") };
        var folder = Env("KNOTE_RESOURCE_MEASURE_FOLDER", IsFile ? Path.GetDirectoryName(Database)! : Path.GetTempPath());
        var results = Path.Combine(folder, "results");
        Directory.CreateDirectory(results);

        using var service = new KntService(KntRepositoryFactory.Create(new RepositoryRef
        {
            Orm = "EntityFramework",
            Provider = IsFile ? "Microsoft.Data.Sqlite" : "Microsoft.Data.SqlClient",
            ConnectionString = IsFile ? $"Data Source={Database}" : Database
        }), activateMessageBroker: false);
        // A user of the copy writes the answers of the AI and the summary (its first Admin).
        var users = (await service.Users.GetAllAsync()).Entity;
        service.UserIdentityName = users.FirstOrDefault(u => KntRoles.Parse(u.RoleDefinition).Contains(EnumRoles.Admin))?.UserName ?? users.First().UserName;

        var request = new DocumentProcessRequestDto
        {
            Kind = EnumDocumentProcessKind.PeriodSummary, Language = Env("KNOTE_PERIOD_MEASURE_LANGUAGE", "Spanish"),
            Period = new PeriodSummaryRequestDto
            {
                Words = int.TryParse(Env("KNOTE_PERIOD_MEASURE_WORDS"), out var words) ? words : PeriodSummaryRules.DefaultWords,
                Selection = new NoteSelectionRequestDto { From = from, To = to, Keywords = Env("KNOTE_PERIOD_MEASURE_KEYWORDS", null!) }
            }
        };

        var before = (await service.Summaries.PrepareSelectionAsync(request.Period.Selection)).Entity;
        var estimate = await AiDocumentProcessor.EstimateAsync(service, provider, request);
        var phases = new List<string>();
        var clock = Stopwatch.StartNew();
        var result = await AiDocumentProcessor.ProcessAsync(service, p => AiChatClientFactory.Create(p), provider, request,
            new Progress<DocumentProcessProgressDto>(p => phases.Add($"{clock.Elapsed.TotalSeconds:N0} s · {p.Phase} {p.CallsDone}/{p.CallsTotal}")));
        clock.Stop();

        var report = new StringBuilder();
        report.Append($"# Period summary measure · {from:yyyy-MM-dd} – {to:yyyy-MM-dd} · {model}\n\n");
        report.Append($"- Selection before: {NoteRelevanceRules.CountText(before)}\n");
        if (estimate.IsValid)
            report.Append($"- Estimate: {estimate.Entity.Notes} notes, {estimate.Entity.DoubtfulToClassify} doubtful to classify, {estimate.Entity.Calls} calls, " +
                $"{estimate.Entity.Characters:N0} characters, context {estimate.Entity.ContextTokens} tokens\n");
        else
            report.Append($"- Estimate: {estimate.ErrorMessage}\n");
        report.Append($"- Result: {result.Entity?.Phase} in {clock.Elapsed.TotalSeconds:N0} s, {result.Entity?.CallsTotal} calls {result.Entity?.Message}\n");

        if (result.Entity?.NoteId is Guid noteId)
        {
            var text = (await service.Notes.GetAsync(noteId)).Entity.Description;
            // What the AI wrote is before the statistics; the citations of notes not read, without a link (SummaryCitationRules, D16).
            var written = text[..Math.Max(0, text.IndexOf(PeriodSummaryDocument.StatisticsHeading, StringComparison.Ordinal))];
            var cited = NoteCitationLinker.CitedNumbers(written);
            var notRead = Regex.Matches(written, $@"#{SummaryCitationRules.Unlinked}(\d+)").Select(m => m.Groups[1].Value).Distinct().ToList();
            report.Append($"- Parts of «What happened»: {estimate.Entity?.Sections}\n");
            report.Append($"- Citations: {cited.Count} notes read cited; not recognized ({notRead.Count}): {string.Join(", ", notRead)}\n");
            report.Append($"- Words written by the AI: {NoteReadingCleaner.CountWords(written)}\n");
            report.Append("\n## Phases\n\n").Append(string.Join("\n", phases.Distinct().Select(p => $"- {p}"))).Append("\n\n## Summary\n\n").Append(text).Append('\n');
        }

        var file = Path.Combine(results, $"period-{from:yyyyMMdd}-{to:yyyyMMdd}-{model.Replace(':', '_')}.md");
        await File.WriteAllTextAsync(file, report.ToString());
        Console.WriteLine(report);
        Assert.AreEqual(EnumDocumentProcessPhase.Completed, result.Entity?.Phase, result.Entity?.Message);
    }
}
