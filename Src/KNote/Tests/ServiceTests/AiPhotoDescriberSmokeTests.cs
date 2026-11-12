using KNote.Ai;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// AiPhotoDescriber with a real local model (KNote Atlas, 4.2), the way of production: a repository, the task's context and
/// the conditional save. Only with KNOTE_RESOURCE_SMOKE_MODEL (an Ollama model that sees images, e.g. qwen3.5:9b; host:
/// OLLAMA_HOST or localhost): without it, Inconclusive, so that the usual run of this suite never calls a model.
/// </summary>
[TestClass]
[TestCategory("RequiresRealAiProvider")]
public class AiPhotoDescriberSmokeTests
{
    [TestMethod]
    public async Task APhotoWithText_IsDescribed_InTheLanguageOfTheNote()
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
        var (chosen, describer) = await AiPhotoDescriber.ChooseAsync([provider], provider, null);
        if (!describer.Available)
        {
            Assert.Inconclusive(describer.Message);
            return;
        }

        using var db = new RepositoryTestDatabase();
        using var service = new KntService(db.CreateRepository("EntityFramework"), activateMessageBroker: false);
        var root = Path.Combine(Path.GetTempPath(), $"knote-smoke-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            service.Repository.RepositoryRef.ResourcesContainer = "KntResources";
            service.Repository.RepositoryRef.ResourcesContainerRootPath = root;
            service.Repository.RepositoryRef.ResourcesContainerRootUrl = "http://intranet/KNote";

            var folder = await service.Repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Fotos" });
            var noteId = Guid.NewGuid();
            await service.Repository.Notes.AddAsync(new NoteDto
            {
                NoteId = noteId, Topic = "Etiquetas del taller", Description = "Fotos de las etiquetas de los equipos que reparamos en el taller.",
                FolderId = folder.Entity.FolderId, CreationDateTime = DateTime.Now, ModificationDateTime = DateTime.Now
            });
            var saved = await service.Notes.SaveResourceAsync(new ResourceDto
            {
                NoteId = noteId, Name = $"{Guid.NewGuid()}_label.jpg", Description = "label.jpg", FileType = "image/jpeg", ContentArrayBytes = PhotoWithText("SERIAL 7351")
            });
            Assert.IsTrue(saved.IsValid, saved.ErrorMessage);

            var res = await AiPhotoDescriber.DescribeAsync(service, AiChatClientFactory.Create(chosen), chosen, saved.Entity.ResourceId, false);

            Assert.IsTrue(res.IsValid, res.ErrorMessage);
            StringAssert.Contains(res.Entity.Description, "7351");
            Assert.AreEqual(model, res.Entity.DescriptionModel);
            Console.WriteLine(res.Entity.Description);
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    private static byte[] PhotoWithText(string text)
    {
        using var surface = SkiaSharp.SKSurface.Create(new SkiaSharp.SKImageInfo(1200, 800));
        surface.Canvas.Clear(SkiaSharp.SKColors.LightGray);
        using var paint = new SkiaSharp.SKPaint { Color = SkiaSharp.SKColors.Black };
        using var font = new SkiaSharp.SKFont(SkiaSharp.SKTypeface.FromFamilyName("Arial"), 90);
        surface.Canvas.DrawRect(new SkiaSharp.SKRect(150, 250, 1050, 550), new SkiaSharp.SKPaint { Color = SkiaSharp.SKColors.White });
        surface.Canvas.DrawText(text, 220, 430, SkiaSharp.SKTextAlign.Left, font, paint);
        using var image = surface.Snapshot();
        using var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Jpeg, 85);
        return data.ToArray();
    }
}
