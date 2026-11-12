using KNote.Ai;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// AiDocumentSummarizer with a real local model (KNote Atlas, 4.4c), the way of production: a repository, the reader of the app,
/// the task's context and the conditional save. Only with KNOTE_RESOURCE_SMOKE_MODEL (an Ollama model, e.g. qwen3.5:9b; host:
/// OLLAMA_HOST or localhost): without it, Inconclusive, so that the usual run of this suite never calls a model.
/// </summary>
[TestClass]
[TestCategory("RequiresRealAiProvider")]
public class AiDocumentSummarizerSmokeTests
{
    [TestMethod]
    public async Task AnEnglishDocument_IsSummarized_InTheLanguageOfTheNote()
    {
        var model = Environment.GetEnvironmentVariable("KNOTE_RESOURCE_SMOKE_MODEL");
        if (string.IsNullOrWhiteSpace(model))
        {
            Assert.Inconclusive("KNOTE_RESOURCE_SMOKE_MODEL is not set - skipping.");
            return;
        }
        var provider = new AiProviderRef
        {
            Alias = "Smoke", Provider = EnumAiProvider.Ollama, Model = model,
            Host = Environment.GetEnvironmentVariable("OLLAMA_HOST") is { Length: > 0 } host ? host : "http://localhost:11434"
        };

        using var db = new RepositoryTestDatabase();
        using var service = new KntService(db.CreateRepository("EntityFramework"), activateMessageBroker: false) { DocumentReader = new AiDocumentReader() };
        var root = Path.Combine(Path.GetTempPath(), $"knote-smoke-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            service.Repository.RepositoryRef.ResourcesContainer = "KntResources";
            service.Repository.RepositoryRef.ResourcesContainerRootPath = root;
            service.Repository.RepositoryRef.ResourcesContainerRootUrl = "http://intranet/KNote";

            var folder = await service.Repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Manuales" });
            var noteId = Guid.NewGuid();
            await service.Repository.Notes.AddAsync(new NoteDto
            {
                NoteId = noteId, Topic = "Manuales del taller", FolderId = folder.Entity.FolderId,
                CreationDateTime = DateTime.Now, ModificationDateTime = DateTime.Now
            });
            var saved = await service.Notes.SaveResourceAsync(new ResourceDto
            {
                NoteId = noteId, Name = $"{Guid.NewGuid()}_zx81.pdf", Description = "zx81.pdf", FileType = "application/pdf",
                ContentArrayBytes = TestDocuments.Pdf(
                    "ZX81 SERVICE MANUAL\nSinclair Research, 1981.\nThis manual explains how to repair the ZX81 computer.",
                    "Chapter 1. The power supply.\nCheck the 9 V regulator before replacing the ULA.")
            });
            Assert.IsTrue(saved.IsValid, saved.ErrorMessage);

            var res = await AiDocumentSummarizer.SummarizeAsync(service, AiChatClientFactory.Create(provider), provider, saved.Entity.ResourceId, false);

            Assert.IsTrue(res.IsValid, res.ErrorMessage);
            StringAssert.Contains(res.Entity.Description, "ZX81");
            Assert.AreEqual(model, res.Entity.DescriptionModel);
            Console.WriteLine(res.Entity.Description);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }
}
