using System.Diagnostics;
using System.Text;
using KNote.Ai;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The measure of the summary of a long document (KNote Atlas, stage 2, 2.1a, D11): a real document of the owner, read as the apps
/// read it, summarized with a real model, timing each run (the pace of the estimates) and keeping each summary to judge it. Needs
/// KNOTE_LONGDOC_MEASURE_FILE (the document). KNOTE_LONGDOC_MEASURE_PROVIDER: Ollama (by default: a local model, KNOTE_LONGDOC_MEASURE_MODEL
/// or KNOTE_RESOURCE_SMOKE_MODEL, with each context of KNOTE_LONGDOC_MEASURE_CONTEXTS, tokens separated by |, 8192 by default), OpenAI
/// (KNOTE_LONGDOC_MEASURE_MODEL or gpt-4o-mini, with OPENAI_API_KEY) or Anthropic (or claude-haiku-4-5, with ANTHROPIC_API_KEY): a paid
/// model, run only when the owner asks, with the context KNote fixes (32,000). Optional KNOTE_LONGDOC_MEASURE_LENGTH (Short, Medium or
/// Long; Medium) and KNOTE_LONGDOC_MEASURE_LANGUAGE. The results go to KNOTE_RESOURCE_MEASURE_FOLDER\results\longdoc-*.md (or next to
/// the document), out of the repository: they hold what the document says.
/// </summary>
[TestClass]
[TestCategory("RequiresRealAiProvider")]
public class LongDocumentMeasure
{
    [TestMethod]
    public async Task Summarize()
    {
        var file = Environment.GetEnvironmentVariable("KNOTE_LONGDOC_MEASURE_FILE");
        var kind = Environment.GetEnvironmentVariable("KNOTE_LONGDOC_MEASURE_PROVIDER") is { Length: > 0 } p ? p.Trim() : EnumAiProvider.Ollama;
        var providerName = new[] { EnumAiProvider.Ollama, EnumAiProvider.OpenAI, EnumAiProvider.Anthropic }
            .FirstOrDefault(n => string.Equals(n, kind, StringComparison.OrdinalIgnoreCase));
        var model = Environment.GetEnvironmentVariable("KNOTE_LONGDOC_MEASURE_MODEL") is { Length: > 0 } m ? m
            : providerName == EnumAiProvider.OpenAI ? "gpt-4o-mini"
            : providerName == EnumAiProvider.Anthropic ? "claude-haiku-4-5"
            : Environment.GetEnvironmentVariable("KNOTE_RESOURCE_SMOKE_MODEL");
        var keyVariable = providerName == EnumAiProvider.OpenAI ? "OPENAI_API_KEY" : providerName == EnumAiProvider.Anthropic ? "ANTHROPIC_API_KEY" : null;
        if (string.IsNullOrWhiteSpace(file) || !File.Exists(file) || providerName == null || string.IsNullOrWhiteSpace(model)
            || (keyVariable != null && string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(keyVariable))))
        {
            Assert.Inconclusive("KNOTE_LONGDOC_MEASURE_FILE (an existing document) and a model (an Ollama one, or OpenAI/Anthropic with its key) are needed - skipping.");
            return;
        }
        // A paid model works with the context KNote fixes for it: one run.
        var contexts = providerName != EnumAiProvider.Ollama ? [AiSummaryContext.PaidTokens]
            : (Environment.GetEnvironmentVariable("KNOTE_LONGDOC_MEASURE_CONTEXTS") is { Length: > 0 } c ? c : "8192")
                .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(int.Parse).ToList();
        var length = Enum.TryParse<EnumSummaryLength>(Environment.GetEnvironmentVariable("KNOTE_LONGDOC_MEASURE_LENGTH"), true, out var l) ? l : EnumSummaryLength.Medium;
        var language = Environment.GetEnvironmentVariable("KNOTE_LONGDOC_MEASURE_LANGUAGE");
        var results = Environment.GetEnvironmentVariable("KNOTE_RESOURCE_MEASURE_FOLDER") is { Length: > 0 } folder
            ? Path.Combine(folder, "results")
            : Path.Combine(Path.GetDirectoryName(file)!, "results");
        Directory.CreateDirectory(results);

        using var db = new RepositoryTestDatabase();
        using var service = new KntService(db.CreateRepository("EntityFramework"), activateMessageBroker: false) { DocumentReader = new AiDocumentReader() };
        var root = Path.Combine(Path.GetTempPath(), $"knote-measure-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            service.Repository.RepositoryRef.ResourcesContainer = "KntResources";
            service.Repository.RepositoryRef.ResourcesContainerRootPath = root;
            service.Repository.RepositoryRef.ResourcesContainerRootUrl = "http://intranet/KNote";

            var noteFolder = await service.Repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Documents" });
            var noteId = Guid.NewGuid();
            await service.Repository.Notes.AddAsync(new NoteDto
            {
                NoteId = noteId, Topic = Path.GetFileNameWithoutExtension(file), FolderId = noteFolder.Entity.FolderId,
                CreationDateTime = DateTime.Now, ModificationDateTime = DateTime.Now
            });
            var name = Path.GetFileName(file);
            var saved = await service.Notes.SaveResourceAsync(new ResourceDto
            {
                NoteId = noteId, Name = $"{Guid.NewGuid()}_{name}", Description = name,
                FileType = Path.GetExtension(file).ToLowerInvariant() == ".pdf" ? "application/pdf" : "", ContentArrayBytes = await File.ReadAllBytesAsync(file)
            });
            Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
            var reading = Stopwatch.StartNew();
            var read = await service.ResourceContent.ReadDocumentAsync(saved.Entity.ResourceId);
            Assert.IsTrue(read.IsValid, read.ErrorMessage);
            var readSeconds = reading.Elapsed.TotalSeconds;

            var host = Environment.GetEnvironmentVariable("OLLAMA_HOST") is { Length: > 0 } h ? h : "http://localhost:11434";
            foreach (var context in contexts)
            {
                var provider = providerName == EnumAiProvider.Ollama
                    ? new AiProviderRef { Alias = "Measure", Provider = providerName, Model = model, Host = host, ContextWindow = context }
                    : new AiProviderRef { Alias = "Measure", Provider = providerName, Model = model };
                var request = new DocumentProcessRequestDto { ResourceId = saved.Entity.ResourceId, Length = length, Language = language };

                var estimate = await AiDocumentProcessor.EstimateAsync(service, provider, request);
                Assert.IsTrue(estimate.IsValid, estimate.ErrorMessage);
                var run = await AiDocumentProcessor.SummarizeAsync(service, p => AiChatClientFactory.Create(p), provider, request);
                Assert.IsTrue(run.IsValid, run.ErrorMessage);
                var summary = (await service.Notes.GetAsync(run.Entity.NoteId!.Value)).Entity.Description;
                var pace = (await service.Summaries.GetStateAsync()).Entity.SecondsPer1000Characters.GetValueOrDefault(model);

                var report = new StringBuilder();
                report.AppendLine($"# {name} · {model} · context {context} · {length}");
                report.AppendLine();
                report.AppendLine($"- Read in {readSeconds:0.0} s: {read.Entity.Pages} pages, {estimate.Entity.Characters:N0} characters; scanned without text: {estimate.Entity.MissingPages.Count}.");
                report.AppendLine($"- Estimated: {estimate.Entity.Batches} batches, {estimate.Entity.Calls} calls.");
                report.AppendLine($"- Done: {run.Entity.CallsDone} calls in {run.Entity.ElapsedSeconds:0} s ({run.Entity.ElapsedSeconds / Math.Max(1, run.Entity.CallsDone):0.0} s per call); pace {pace:0.00} s per 1,000 characters.");
                report.AppendLine();
                report.AppendLine("## The note");
                report.AppendLine();
                report.AppendLine(summary);
                var path = Path.Combine(results, $"longdoc-{Path.GetFileNameWithoutExtension(file)}-{model.Replace(':', '_')}-{context}-{length}.md");
                await File.WriteAllTextAsync(path, report.ToString());
                Console.WriteLine($"{context}: {run.Entity.CallsDone} calls, {run.Entity.ElapsedSeconds:0} s -> {path}");
            }
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }
}
