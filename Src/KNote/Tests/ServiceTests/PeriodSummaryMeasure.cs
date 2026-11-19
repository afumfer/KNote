using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using KNote.Ai;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The measure of the summaries of a period (KNote Atlas stage 2, D23; Guia-evaluaciones.md, part 3): against a COPY of a real repository
/// (it writes in it: the answers of the AI about the doubtful notes and the summaries), for each period, the selection (and its keywords,
/// if given), the summary with a real local model, its calls, time and pace, its citations (read and not recognized) and its relations;
/// then the three sheets the owner rates in the browser: A, each summary (does it cover what matters?); B, 10 claims at random against
/// the notes they cite; C, 30 notes of the selections (10 in, 10 out and 10 decided by the AI) against the owner's criterion. And the
/// size of «Note relevance» and the time of the selection, that reads it. The results go to results\ under KNOTE_RESOURCE_MEASURE_FOLDER
/// (or next to the copy), out of the repository: they have what the notes say.
/// Variables: KNOTE_PERIOD_MEASURE_DB (a SQLite file or a SQL Server connection string); KNOTE_PERIOD_MEASURE_PERIODS
/// ("yyyy-MM-dd..yyyy-MM-dd", separated by |) or KNOTE_PERIOD_MEASURE_FROM and _TO (one period); optional _KEYWORDS, _WORDS (2000),
/// _LANGUAGE (Spanish), _ATTRIBUTES (the attributes of «By concept», separated by |: «Note type · Name» as the selector shows
/// them, or only the name) and _SEED (2, the samples of B and C); KNOTE_RESOURCE_SMOKE_MODEL (an Ollama model; host OLLAMA_HOST or
/// localhost). Without them, Inconclusive. Filter by name: never the category.
/// </summary>
[TestClass]
[TestCategory("RequiresRealAiProvider")]
public class PeriodSummaryMeasure
{
    private const int ClaimsToReview = 10;
    private const int NotesPerGroup = 10;
    private const int NoteTextShown = 800;
    private static readonly Regex Sentence = new(@"(?<=[.!?])\s+(?=[\p{Lu}¿¡\d#])", RegexOptions.Compiled);
    private static readonly Regex ListMarker = new(@"^\s*(?:[-*+]|\d+[.)])\s+", RegexOptions.Compiled);

    private sealed record Run(DateTime From, DateTime To, string Text, HashSet<int> Read, NoteSelectionResultDto Selection);

    private static string Env(string name, string fallback = "") => Environment.GetEnvironmentVariable(name) is { Length: > 0 } value ? value : fallback;

    private static string Database => Env("KNOTE_PERIOD_MEASURE_DB");
    private static bool IsFile => File.Exists(Database);
    private static string RepositoryLabel => IsFile ? Path.GetFileName(Database) : "SQL Server";

    [TestMethod]
    public async Task Summarize()
    {
        var model = Env("KNOTE_RESOURCE_SMOKE_MODEL");
        var periods = Periods();
        if (string.IsNullOrWhiteSpace(Database) || string.IsNullOrWhiteSpace(model) || periods.Count == 0)
        {
            Assert.Inconclusive("KNOTE_PERIOD_MEASURE_DB, KNOTE_PERIOD_MEASURE_PERIODS (or _FROM and _TO) and KNOTE_RESOURCE_SMOKE_MODEL are not set - skipping the measure.");
            return;
        }
        var provider = new AiProviderRef { Alias = "Measure", Provider = EnumAiProvider.Ollama, Model = model, Host = Env("OLLAMA_HOST", "http://localhost:11434") };
        var folder = Env("KNOTE_RESOURCE_MEASURE_FOLDER", IsFile ? Path.GetDirectoryName(Database)! : Path.GetTempPath());
        var results = Path.Combine(folder, "results");
        Directory.CreateDirectory(results);
        var seed = int.TryParse(Env("KNOTE_PERIOD_MEASURE_SEED"), out var s) ? s : 2;

        using var service = new KntService(KntRepositoryFactory.Create(new RepositoryRef
        {
            Orm = "EntityFramework",
            Provider = IsFile ? "Microsoft.Data.Sqlite" : "Microsoft.Data.SqlClient",
            ConnectionString = IsFile ? $"Data Source={Database}" : Database
        }), activateMessageBroker: false);
        // A user of the copy writes the answers of the AI and the summary (its first Admin).
        var users = (await service.Users.GetAllAsync()).Entity;
        service.UserIdentityName = users.FirstOrDefault(u => KntRoles.Parse(u.RoleDefinition).Contains(EnumRoles.Admin))?.UserName ?? users.First().UserName;

        var runs = new List<Run>();
        var overview = new StringBuilder($"# Stage 2 measure · {RepositoryLabel} · {model} · {DateTime.Now:yyyy-MM-dd HH:mm}\n\n");
        overview.Append("| Period | Notes in / doubtful / out | Selection | Calls (estimated) | Time | Words by the AI | Cited read | Not recognized | Relations |\n|---|---|---|---|---|---|---|---|---|\n");
        var failed = new List<string>();
        foreach (var (from, to) in periods)
        {
            var run = await SummarizeAsync(service, provider, from, to, results, overview);
            if (run == null)
                failed.Add($"{from:yyyy-MM-dd} – {to:yyyy-MM-dd}");
            else
                runs.Add(run);
        }

        overview.Append($"\n{await RelevanceNoteTextAsync(service, runs)}\n");
        await WriteSheetsAsync(service, runs, results, seed, overview);
        var file = Path.Combine(results, $"stage2-measure-{model.Replace(':', '_')}.md");
        await File.WriteAllTextAsync(file, overview.ToString());
        Console.WriteLine(overview);
        Assert.AreEqual(0, failed.Count, $"Not completed: {string.Join(", ", failed)} (see its period-*.md).");
    }

    private static List<(DateTime From, DateTime To)> Periods()
    {
        var periods = new List<(DateTime, DateTime)>();
        foreach (var period in Env("KNOTE_PERIOD_MEASURE_PERIODS").Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var dates = period.Split("..", StringSplitOptions.TrimEntries);
            if (dates.Length == 2 && DateTime.TryParse(dates[0], out var from) && DateTime.TryParse(dates[1], out var to))
                periods.Add((from, to));
        }
        if (periods.Count == 0 && DateTime.TryParse(Env("KNOTE_PERIOD_MEASURE_FROM"), out var f) && DateTime.TryParse(Env("KNOTE_PERIOD_MEASURE_TO"), out var t))
            periods.Add((f, t));
        return periods;
    }

    // The attributes of «By concept» of KNOTE_PERIOD_MEASURE_ATTRIBUTES: each one as the selector shows it («Note type · Name», «- · Name»)
    // or only by its name (every attribute of that name).
    private static async Task<List<Guid>> ConceptAttributeIdsAsync(KntService service)
    {
        var asked = Env("KNOTE_PERIOD_MEASURE_ATTRIBUTES").Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (asked.Length == 0)
            return new List<Guid>();
        var choices = PeriodConceptRules.Choices((await service.KAttributes.GetAllAsync()).Entity);
        return choices.Where(c => asked.Any(a => string.Equals(a, c.Label, StringComparison.CurrentCultureIgnoreCase)
            || string.Equals(a, c.Name, StringComparison.CurrentCultureIgnoreCase))).Select(c => c.KAttributeId).ToList();
    }

    // One summary, as the apps make it, and its report (period-*.md). Null if it was not completed.
    private static async Task<Run?> SummarizeAsync(KntService service, AiProviderRef provider, DateTime from, DateTime to, string results, StringBuilder overview)
    {
        var request = new DocumentProcessRequestDto
        {
            Kind = EnumDocumentProcessKind.PeriodSummary, Language = Env("KNOTE_PERIOD_MEASURE_LANGUAGE", "Spanish"),
            Period = new PeriodSummaryRequestDto
            {
                Words = int.TryParse(Env("KNOTE_PERIOD_MEASURE_WORDS"), out var words) ? words : PeriodSummaryRules.DefaultWords,
                Selection = new NoteSelectionRequestDto { From = from, To = to, Keywords = Env("KNOTE_PERIOD_MEASURE_KEYWORDS", null!) },
                ConceptAttributeIds = await ConceptAttributeIdsAsync(service)
            }
        };

        var selectionClock = Stopwatch.StartNew();
        var before = (await service.Summaries.PrepareSelectionAsync(request.Period.Selection)).Entity;
        selectionClock.Stop();
        var estimate = await AiDocumentProcessor.EstimateAsync(service, provider, request);
        var phases = new List<string>();
        var clock = Stopwatch.StartNew();
        var result = await AiDocumentProcessor.ProcessAsync(service, p => AiChatClientFactory.Create(p), provider, request,
            new Progress<DocumentProcessProgressDto>(p => phases.Add($"{clock.Elapsed.TotalSeconds:N0} s · {p.Phase} {p.CallsDone}/{p.CallsTotal}")));
        clock.Stop();
        // After the run, the doubtful notes have the answers of the AI: the selection the summary read.
        var after = (await service.Summaries.PrepareSelectionAsync(request.Period.Selection)).Entity ?? before;

        var report = new StringBuilder();
        report.Append($"# Period summary measure · {from:yyyy-MM-dd} – {to:yyyy-MM-dd} · {provider.Model}\n\n");
        report.Append($"- Selection before: {NoteRelevanceRules.CountText(before)} ({selectionClock.Elapsed.TotalMilliseconds:N0} ms)\n");
        report.Append($"- Selection after: {NoteRelevanceRules.CountText(after)}\n");
        if (estimate.IsValid)
            report.Append($"- Estimate: {estimate.Entity.Notes} notes, {estimate.Entity.DoubtfulToClassify} doubtful to classify, {estimate.Entity.Calls} calls, " +
                $"{estimate.Entity.Characters:N0} characters, {estimate.Entity.Sections} parts, context {estimate.Entity.ContextTokens} tokens\n");
        else
            report.Append($"- Estimate: {estimate.ErrorMessage}\n");
        report.Append($"- Result: {result.Entity?.Phase} in {clock.Elapsed.TotalSeconds:N0} s, {result.Entity?.CallsTotal} calls {result.Entity?.Message}\n");

        Run? run = null;
        var period = $"{from:yyyy-MM-dd} – {to:yyyy-MM-dd}";
        if (result.Entity?.Phase == EnumDocumentProcessPhase.Completed && result.Entity.NoteId is Guid noteId)
        {
            var note = (await service.Notes.GetExtendedAsync(noteId)).Entity;
            var text = note.Description ?? "";
            var written = WrittenByTheAi(text);
            var read = after.Notes.Where(n => n.IsIncluded).Select(n => n.NoteNumber).ToHashSet();
            var cited = NoteCitationLinker.CitedNumbers(written);
            var notRead = NotRecognized(written);
            var wordsByAi = NoteReadingCleaner.CountWords(written);
            var relations = note.TraceNotesTo?.Count ?? 0;
            report.Append($"- Parts of «What happened»: {estimate.Entity?.Sections}\n");
            report.Append($"- Citations: {cited.Count} notes read cited; not recognized ({notRead.Count}): {string.Join(", ", notRead)}\n");
            report.Append($"- Words written by the AI: {wordsByAi}\n- Relations @DerivedFrom: {relations}\n");
            report.Append("\n## Phases\n\n").Append(string.Join("\n", phases.Distinct().Select(p => $"- {p}"))).Append("\n\n## Summary\n\n").Append(text).Append('\n');
            overview.Append($"| {period} | {after.In} / {after.Doubtful} / {after.Out} | {selectionClock.Elapsed.TotalMilliseconds:N0} ms | {result.Entity.CallsTotal} ({estimate.Entity?.Calls}) " +
                $"| {clock.Elapsed.TotalSeconds:N0} s | {wordsByAi} | {cited.Count} | {notRead.Count} | {relations} |\n");
            run = new Run(from, to, text, read, after);
        }
        else
            overview.Append($"| {period} | {before.In} / {before.Doubtful} / {before.Out} | {selectionClock.Elapsed.TotalMilliseconds:N0} ms | {result.Entity?.Phase}: {result.Entity?.Message} | | | | | |\n");

        await File.WriteAllTextAsync(Path.Combine(results, $"period-{from:yyyyMMdd}-{to:yyyyMMdd}-{provider.Model.Replace(':', '_')}.md"), report.ToString());
        return run;
    }

    // What the AI wrote: the executive summary, what happened and what is open (the index, the statistics and the tables are KNote's).
    private static string WrittenByTheAi(string text)
    {
        string Part(string heading, string next)
        {
            var start = text.IndexOf(heading, StringComparison.Ordinal);
            if (start < 0)
                return "";
            var end = text.IndexOf(next, start + heading.Length, StringComparison.Ordinal);
            return text[(start + heading.Length)..(end < 0 ? text.Length : end)];
        }
        return Part(PeriodSummaryDocument.ExecutiveHeading, PeriodSummaryDocument.IndexHeading) + "\n"
            + Part(PeriodSummaryDocument.HappenedHeading, PeriodSummaryDocument.PendingHeading) + "\n"
            + Part(PeriodSummaryDocument.PendingHeading, PeriodSummaryDocument.StatisticsHeading);
    }

    private static List<string> NotRecognized(string text) =>
        Regex.Matches(text, $@"#{SummaryCitationRules.Unlinked}(\d+)").Select(m => m.Groups[1].Value).Distinct().ToList();

    // The sentences of what the AI wrote with a citation of a note it read.
    private static IEnumerable<(string Claim, List<int> Cited)> Claims(Run run) =>
        WrittenByTheAi(run.Text).Split('\n')
            .Where(line => !line.TrimStart().StartsWith('#') && !line.TrimStart().StartsWith('|'))
            .Select(line => ListMarker.Replace(line, "").Trim())
            .SelectMany(line => Sentence.Split(line))
            .Select(sentence => (Claim: sentence.Trim(), Cited: NoteCitationLinker.CitedNumbers(sentence).Where(run.Read.Contains).OrderBy(n => n).ToList()))
            .Where(c => c.Cited.Count > 0 && c.Claim.Length > 20);

    private static async Task WriteSheetsAsync(KntService service, List<Run> runs, string results, int seed, StringBuilder overview)
    {
        var model = Env("KNOTE_RESOURCE_SMOKE_MODEL");
        var suffix = model.Replace(':', '_');

        // A · Each summary.
        MeasureSheet.Write(Path.Combine(results, $"stage2-summaries-{suffix}.html"), $"Stage 2 · A · {runs.Count} period summaries · {model}",
            "Does each summary cover what matters of its period? Write in the comment what is missing or wrong.", RepositoryLabel,
            ["Covers what matters", "Misses something important", "Not useful"],
            runs.Select(r => new MeasureSheet.Item($"{r.From:yyyy-MM-dd} – {r.To:yyyy-MM-dd}", r.Text)).ToList());

        // B · Claims against their citations.
        var random = new Random(seed);
        var claims = runs.SelectMany(r => Claims(r).Select(c => (Run: r, c.Claim, c.Cited)))
            .OrderBy(_ => random.Next()).Take(ClaimsToReview).ToList();
        var claimItems = new List<MeasureSheet.Item>();
        foreach (var (run, claim, cited) in claims)
        {
            var text = new StringBuilder(claim).Append("\n\n— What the cited notes say —\n");
            foreach (var number in cited)
                text.Append('\n').Append(await NoteShownAsync(service, run.Selection.Notes.First(n => n.NoteNumber == number))).Append('\n');
            claimItems.Add(new MeasureSheet.Item($"{run.From:yyyy-MM-dd} – {run.To:yyyy-MM-dd} · cites {string.Join(", ", cited.Select(n => $"#{n}"))}", text.ToString()));
        }
        MeasureSheet.Write(Path.Combine(results, $"stage2-claims-{suffix}.html"), $"Stage 2 · B · {claimItems.Count} claims against their citations · {model}",
            $"Claims at random (seed {seed}) of what the AI wrote, each one with the beginning of the notes it cites. Is it what the notes say?", RepositoryLabel,
            ["Supported", "Partly", "Not supported"], claimItems);

        // C · Notes of the selections: 10 in, 10 out and 10 decided by the AI (the rest from the other groups, if one has fewer).
        var notes = runs.SelectMany(r => r.Selection.Notes).DistinctBy(n => n.NoteId).OrderBy(_ => random.Next()).ToList();
        var byAi = notes.Where(n => n.Source == EnumSelectionSource.Ai).Take(NotesPerGroup).ToList();
        var inNotes = notes.Where(n => n.Source != EnumSelectionSource.Ai && n.Decision == EnumSelectionDecision.In).Take(NotesPerGroup).ToList();
        var outNotes = notes.Where(n => n.Source != EnumSelectionSource.Ai && n.Decision == EnumSelectionDecision.Out).Take(NotesPerGroup).ToList();
        var sample = inNotes.Concat(outNotes).Concat(byAi).ToList();
        sample.AddRange(notes.Except(sample).Take(NotesPerGroup * 3 - sample.Count));
        var sampleItems = new List<MeasureSheet.Item>();
        foreach (var n in sample)
            sampleItems.Add(new MeasureSheet.Item($"KNote says: {n.Decision} ({n.Source}) — {n.Reason}", await NoteShownAsync(service, n)));
        MeasureSheet.Write(Path.Combine(results, $"stage2-selection-{suffix}.html"), $"Stage 2 · C · {sample.Count} notes of the selections · {model}",
            $"At random (seed {seed}): {inNotes.Count} in, {outNotes.Count} out and {byAi.Count} decided by the AI. Do you agree with what KNote decided?", RepositoryLabel,
            ["Agree", "Disagree"],
            sampleItems);

        overview.Append($"Sheets: A {runs.Count} summaries · B {claimItems.Count} claims · C {sample.Count} notes ({inNotes.Count} in, {outNotes.Count} out, {byAi.Count} by the AI), seed {seed}.\n");
    }

    private static async Task<string> NoteShownAsync(KntService service, NoteSelectionDto note)
    {
        var text = NoteReadingCleaner.Clean((await service.Notes.GetAsync(note.NoteId)).Entity?.Description ?? "");
        if (text.Length > NoteTextShown)
            text = text[..NoteTextShown] + " …";
        return $"#{note.NoteNumber} {note.Topic} · {note.FolderName} · modified {note.ModificationDateTime:yyyy-MM-dd}\n{text}";
    }

    // «Note relevance» after the runs: its rows and its size (the selection reads it all, and each answer of the AI rewrites it).
    private static async Task<string> RelevanceNoteTextAsync(KntService service, List<Run> runs)
    {
        if (runs.LastOrDefault()?.Selection.RelevanceNote is not { } reference)
            return "«Note relevance»: not created.";
        var text = (await service.Notes.GetAsync(reference.NoteId)).Entity?.Description ?? "";
        var (corrections, answers) = RelevanceNote.Parse(text);
        return $"«Note relevance» #{reference.NoteNumber}: {corrections.Count} corrections, {answers.Count} answers of the AI, {text.Length:N0} characters.";
    }
}
