using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Service.Interfaces;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The semantic index of KNote Atlas through the Service layer (IKntSemanticSearchService, KntSemanticIndexer), with a
/// fake, deterministic embedding model: what it indexes, the vectors it reuses, the notes that become pending, the
/// rebuild, the cancellation, the errors, the dimensions and the state of the index.
/// </summary>
[TestClass]
public class SemanticIndexServiceTests
{
    private const string Model = "fake-embed";

    private sealed class Fixture : IDisposable
    {
        public RepositoryTestDatabase Db { get; } = new();
        public FakeEmbeddingGenerators Models { get; } = new(Model);
        public KntService Service { get; private set; } = null!;
        public Guid HomeId { get; private set; }
        public Guid UserId { get; private set; }

        public static async Task<Fixture> CreateAsync(string orm = "Dapper", int? dimensions = null, string model = Model)
        {
            var fixture = new Fixture();
            fixture.Service = new KntService(fixture.Db.CreateRepository(orm)) { EmbeddingGenerators = fixture.Models };
            await fixture.SetModelAsync(model, dimensions);
            fixture.HomeId = (await fixture.Service.Folders.GetHomeAsync()).Entity.FolderId;
            fixture.UserId = (await fixture.Service.Users.GetAllAsync()).Entity.First().UserId;
            return fixture;
        }

        public async Task SetModelAsync(string model, int? dimensions = null)
        {
            var res = await Service.SystemValues.SaveRepositoryAiModelsAsync(new RepositoryAiModelsDto
            {
                EmbeddingModel = model, EmbeddingDimensions = dimensions
            });
            Assert.IsTrue(res.IsValid, res.ErrorMessage);
        }

        public async Task<NoteDto> AddNoteAsync(string topic, string description = "", Guid? folderId = null, Guid? noteTypeId = null,
            string contentType = null!)
        {
            var note = (await Service.Notes.NewAsync()).Entity;
            note.Topic = topic;
            note.Description = description;
            note.FolderId = folderId ?? HomeId;
            note.NoteTypeId = noteTypeId;
            if (contentType != null)
                note.ContentType = contentType;
            var res = await Service.Notes.SaveAsync(note);
            Assert.IsTrue(res.IsValid, res.ErrorMessage);
            return res.Entity;
        }

        public async Task<SemanticIndexProgressDto> RunAsync(bool rebuild = false, Action<SemanticIndexProgressDto>? onProgress = null,
            CancellationToken cancellationToken = default)
        {
            var res = await Service.SemanticSearch.RunIndexAsync(new SemanticIndexRunRequest
            {
                Rebuild = rebuild,
                Progress = onProgress == null ? null : new SyncProgress(onProgress),
                CancellationToken = cancellationToken
            });
            Assert.IsNotNull(res.Entity);
            return res.Entity;
        }

        public async Task<SemanticIndexStatusDto> StatusAsync()
        {
            var res = await Service.SemanticSearch.GetIndexStatusAsync();
            Assert.IsTrue(res.IsValid, res.ErrorMessage);
            return res.Entity;
        }

        public async Task<List<NoteChunkDto>> ChunksAsync(Guid noteId, string indexKey = Model) =>
            (await Service.Repository.SemanticIndex.GetNoteChunksAsync(noteId, indexKey)).Entity;

        public async Task<bool> IsPendingAsync(Guid noteId) =>
            (await Service.Repository.SemanticIndex.GetPendingNoteIdsAsync(Model, 10000)).Entity.Contains(noteId);

        public void Dispose() => Db.Dispose();
    }

    // Reports in the same call (Progress<T> posts to the thread pool).
    private sealed class SyncProgress : IProgress<SemanticIndexProgressDto>
    {
        private readonly Action<SemanticIndexProgressDto> _report;
        public SyncProgress(Action<SemanticIndexProgressDto> report) => _report = report;
        public void Report(SemanticIndexProgressDto value) => _report(value);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task RunIndex_IndexesEveryNote_ButTheSystemOnes(string orm)
    {
        using var fixture = await Fixture.CreateAsync(orm);
        var note = await fixture.AddNoteAsync("Amstrad CPC 464", "# Restoration\n\nCleaned the case.");
        var sessionsType = (await fixture.Service.NoteTypes.SaveAsync(new NoteTypeDto { Name = KntConst.ChatSessionsTag })).Entity;
        var session = await fixture.AddNoteAsync("A session", "A private conversation.", noteTypeId: sessionsType.NoteTypeId);
        var before = await fixture.StatusAsync();

        var progress = await fixture.RunAsync();

        Assert.AreEqual(EnumSemanticIndexPhase.Completed, progress.Phase);
        Assert.AreEqual(before.PendingNotes, progress.NotesDone);
        Assert.AreEqual(0, progress.ErrorsCount);
        var status = await fixture.StatusAsync();
        Assert.AreEqual(EnumSemanticIndexStatus.Ready, status.Status);
        Assert.AreEqual(0, status.PendingNotes);
        Assert.AreEqual(status.TotalNotes, status.IndexedNotes);
        Assert.IsNotNull(status.CompletedDateTime);
        Assert.AreEqual(0, (await fixture.ChunksAsync(session.NoteId)).Count, "system notes are not indexed");

        var chunks = await fixture.ChunksAsync(note.NoteId);
        StringAssert.StartsWith(chunks.Single(c => c.SourceType == NoteChunkSourceType.Data).Content, "Topic: Amstrad CPC 464");
        Assert.AreEqual("# Restoration\n\nCleaned the case.", chunks.Single(c => c.SourceType == NoteChunkSourceType.Text).Content);
        Assert.IsTrue(chunks.All(c => c.Dimensions == FakeEmbeddingGenerators.Dimensions));
        Assert.AreEqual(1f, EmbeddingVector.Dot(chunks[0].Vector, chunks[0].Vector), 1e-5f, "normalized");
    }

    [TestMethod]
    public async Task RunIndex_WithoutModel_OrWithoutItsGenerator_Fails()
    {
        using var fixture = await Fixture.CreateAsync(model: "not-here");

        var res = await fixture.Service.SemanticSearch.RunIndexAsync(new SemanticIndexRunRequest());

        Assert.IsFalse(res.IsValid);
        Assert.AreEqual(EnumSemanticIndexPhase.Failed, res.Entity.Phase);
        StringAssert.Contains(res.ErrorMessage, "not-here");

        await fixture.SetModelAsync(null!);
        var resWithoutModel = await fixture.Service.SemanticSearch.RunIndexAsync(new SemanticIndexRunRequest());
        Assert.IsFalse(resWithoutModel.IsValid);
        Assert.AreEqual(EnumSemanticIndexStatus.None, (await fixture.StatusAsync()).Status);
    }

    [TestMethod]
    public async Task ChangingATask_CalculatesOnlyItsFragment()
    {
        using var fixture = await Fixture.CreateAsync();
        var note = await fixture.AddNoteAsync("Amstrad CPC 464", "Cleaned the case.");
        await fixture.RunAsync();
        fixture.Models.Reset();

        var task = (await fixture.Service.Notes.SaveNoteTaskAsync(new NoteTaskDto
        {
            NoteId = note.NoteId, UserId = fixture.UserId, Description = "Change the capacitors", CreationDateTime = DateTime.Now, ModificationDateTime = DateTime.Now
        })).Entity;
        Assert.IsTrue(await fixture.IsPendingAsync(note.NoteId));
        var progress = await fixture.RunAsync();

        Assert.AreEqual(1, progress.NotesDone);
        Assert.AreEqual(1, progress.ChunksEmbedded);
        Assert.AreEqual(2, progress.ChunksReused);
        StringAssert.StartsWith(fixture.Models.GeneratedTexts.Single(), "Amstrad CPC 464\n\nTask: Change the capacitors");
        Assert.AreEqual(task.NoteTaskId, (await fixture.ChunksAsync(note.NoteId)).Single(c => c.SourceType == NoteChunkSourceType.Task).SourceId);
        Assert.IsFalse(await fixture.IsPendingAsync(note.NoteId));
    }

    [TestMethod]
    public async Task TheCommandsThatChangeANote_LeaveItPending()
    {
        using var fixture = await Fixture.CreateAsync();
        var service = fixture.Service;
        var folder = (await service.Folders.SaveAsync(new FolderDto { Name = "Collection", ParentId = fixture.HomeId })).Entity;
        var subfolder = (await service.Folders.SaveAsync(new FolderDto { Name = "Amstrad", ParentId = folder.FolderId })).Entity;
        var noteType = (await service.NoteTypes.SaveAsync(new NoteTypeDto { Name = "Computer" })).Entity;
        var attribute = (await service.KAttributes.SaveAsync(new KAttributeDto { Name = "Processor", KAttributeDataType = EnumKAttributeDataType.String })).Entity;
        var note = await fixture.AddNoteAsync("Amstrad CPC 464", "Text.", subfolder.FolderId, noteType.NoteTypeId);
        var other = await fixture.AddNoteAsync("Other", "Text.");
        var withAttribute = (await service.Notes.GetAsync(other.NoteId)).Entity;
        withAttribute.KAttributesDto.Single(a => a.KAttributeId == attribute.KAttributeId).Value = "Z80A";
        Assert.IsTrue((await service.Notes.SaveAsync(withAttribute)).IsValid);

        async Task AssertPendingAfterAsync(string what, Guid noteId, Func<Task<bool>> change)
        {
            await fixture.RunAsync();
            Assert.IsFalse(await fixture.IsPendingAsync(noteId), $"{what}: indexed before");
            Assert.IsTrue(await change(), $"{what}: changed");
            Assert.IsTrue(await fixture.IsPendingAsync(noteId), $"{what}: pending after");
        }

        await AssertPendingAfterAsync("save", note.NoteId, async () =>
        {
            var edited = (await service.Notes.GetAsync(note.NoteId)).Entity;
            edited.Description = "New text.";
            return (await service.Notes.SaveAsync(edited)).IsValid;
        });
        NoteTaskDto task = null!;
        await AssertPendingAfterAsync("new task", note.NoteId, async () =>
        {
            var res = await service.Notes.SaveNoteTaskAsync(new NoteTaskDto
            {
                NoteId = note.NoteId, UserId = fixture.UserId, Description = "A task", CreationDateTime = DateTime.Now, ModificationDateTime = DateTime.Now
            });
            task = res.Entity;
            return res.IsValid;
        });
        await AssertPendingAfterAsync("delete task", note.NoteId, async () => (await service.Notes.DeleteNoteTaskAsync(task.NoteTaskId)).IsValid);
        ResourceDto resource = null!;
        await AssertPendingAfterAsync("new resource", note.NoteId, async () =>
        {
            var res = await service.Notes.SaveResourceAsync(new ResourceDto
            {
                NoteId = note.NoteId, Container = "KntResources", Name = $"{note.NoteId}_front.png", FileType = "image/png",
                Description = "The front.",
                ContentInDB = true, ContentArrayBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 }
            });
            resource = res.Entity;
            Assert.IsTrue(res.IsValid, res.ErrorMessage);
            return res.IsValid;
        });
        await AssertPendingAfterAsync("delete resource", note.NoteId, async () => (await service.Notes.DeleteResourceAsync(resource.ResourceId)).IsValid);
        await AssertPendingAfterAsync("tags", note.NoteId, async () => (await service.Notes.UtilPatchChangeTagsAsync(note.NoteId, "", "z80")).IsValid);
        await AssertPendingAfterAsync("rename the parent folder", note.NoteId, async () =>
        {
            folder.Name = "Computers";
            return (await service.Folders.SaveAsync(folder)).IsValid;
        });
        await AssertPendingAfterAsync("rename the note type", note.NoteId, async () =>
        {
            noteType.Name = "Microcomputer";
            return (await service.NoteTypes.SaveAsync(noteType)).IsValid;
        });
        await AssertPendingAfterAsync("rename the attribute", other.NoteId, async () =>
        {
            attribute.Name = "CPU";
            return (await service.KAttributes.SaveAsync(attribute)).IsValid;
        });

        await fixture.RunAsync();
        var data = (await fixture.ChunksAsync(note.NoteId)).Single(c => c.SourceType == NoteChunkSourceType.Data).Content;
        StringAssert.Contains(data, @"Folder: Home\Computers\Amstrad");
        StringAssert.Contains(data, "Type: Microcomputer");
        StringAssert.Contains((await fixture.ChunksAsync(other.NoteId)).Single(c => c.SourceType == NoteChunkSourceType.Data).Content, "CPU: Z80A");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task SavingANoteAsClientWinDoes_LeavesItPending(string orm)
    {
        using var fixture = await Fixture.CreateAsync(orm);
        var note = await fixture.AddNoteAsync("Amstrad CPC 464", "Text.");
        await fixture.RunAsync();
        Assert.AreEqual(0, (await fixture.StatusAsync()).PendingNotes);

        var extended = (await fixture.Service.Notes.GetExtendedAsync(note.NoteId)).Entity;
        extended.Description = "New text.";
        var saved = await fixture.Service.Notes.SaveExtendedAsync(extended);

        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        Assert.IsTrue(await fixture.IsPendingAsync(note.NoteId));
        Assert.AreEqual(1, (await fixture.StatusAsync()).PendingNotes);
    }

    [TestMethod]
    public async Task Rebuild_CalculatesEveryVectorAgain()
    {
        using var fixture = await Fixture.CreateAsync();
        await fixture.AddNoteAsync("One", "Text one.");
        await fixture.RunAsync();
        var chunks = (await fixture.StatusAsync()).Chunks;
        fixture.Models.Reset();

        var progress = await fixture.RunAsync(rebuild: true);

        Assert.AreEqual(EnumSemanticIndexPhase.Completed, progress.Phase);
        Assert.AreEqual(chunks, fixture.Models.GeneratedTexts.Count);
        Assert.AreEqual(0, progress.ChunksReused);
        var status = await fixture.StatusAsync();
        Assert.AreEqual(EnumSemanticIndexStatus.Ready, status.Status);
        Assert.IsFalse(status.Rebuilding);
        Assert.AreEqual(0, status.PendingNotes);
    }

    [TestMethod]
    public async Task ACanceledRebuild_GoesOnWithoutReusingTheOldVectors()
    {
        using var fixture = await Fixture.CreateAsync();
        for (var i = 1; i <= 4; i++)
            await fixture.AddNoteAsync($"Note {i}", $"Text {i}.");
        await fixture.RunAsync();
        var chunks = (await fixture.StatusAsync()).Chunks;
        fixture.Models.Reset();

        using var cancel = new CancellationTokenSource();
        var canceled = await fixture.RunAsync(rebuild: true, p => { if (p.NotesDone >= 2) cancel.Cancel(); }, cancel.Token);

        Assert.AreEqual(EnumSemanticIndexPhase.Canceled, canceled.Phase);
        var during = await fixture.StatusAsync();
        Assert.IsTrue(during.Rebuilding);
        Assert.IsTrue(during.PendingNotes > 0);
        Assert.AreEqual(during.TotalNotes, during.IndexedNotes, "the old vectors still serve");

        var resumed = await fixture.RunAsync();

        Assert.AreEqual(EnumSemanticIndexPhase.Completed, resumed.Phase);
        Assert.AreEqual(0, resumed.ChunksReused);
        Assert.AreEqual(chunks, fixture.Models.GeneratedTexts.Count);
        Assert.IsFalse((await fixture.StatusAsync()).Rebuilding);
    }

    [TestMethod]
    public async Task ACanceledCreation_KeepsWhatIsDone_AndTheNextRunGoesOn()
    {
        using var fixture = await Fixture.CreateAsync();
        for (var i = 1; i <= 4; i++)
            await fixture.AddNoteAsync($"Note {i}", $"Text {i}.");
        var total = (await fixture.StatusAsync()).TotalNotes;

        using var cancel = new CancellationTokenSource();
        var canceled = await fixture.RunAsync(onProgress: p => { if (p.NotesDone >= 2) cancel.Cancel(); }, cancellationToken: cancel.Token);

        Assert.AreEqual(EnumSemanticIndexPhase.Canceled, canceled.Phase);
        var during = await fixture.StatusAsync();
        Assert.AreEqual(EnumSemanticIndexStatus.Building, during.Status);
        Assert.AreEqual(canceled.NotesDone, during.IndexedNotes);

        var resumed = await fixture.RunAsync();

        Assert.AreEqual(total - canceled.NotesDone, resumed.NotesDone);
        var status = await fixture.StatusAsync();
        Assert.AreEqual(EnumSemanticIndexStatus.Ready, status.Status);
        Assert.AreEqual(total, status.IndexedNotes);
    }

    [TestMethod]
    public async Task ANoteThatFails_IsCounted_AndTriedAgainUpToThreeTimes()
    {
        using var fixture = await Fixture.CreateAsync();
        var broken = await fixture.AddNoteAsync("BROKEN note", "Text.");
        fixture.Models.FailWhen = text => text.Contains("BROKEN");

        var first = await fixture.RunAsync();

        Assert.AreEqual(EnumSemanticIndexPhase.Completed, first.Phase);
        Assert.AreEqual(1, first.ErrorsCount);
        StringAssert.Contains(first.Errors.Single(), "BROKEN note");
        Assert.IsTrue(await fixture.IsPendingAsync(broken.NoteId));
        Assert.AreEqual(1, (await fixture.StatusAsync()).PendingNotes);

        Assert.AreEqual(1, (await fixture.RunAsync()).ErrorsCount);
        Assert.AreEqual(1, (await fixture.RunAsync()).ErrorsCount);
        var fourth = await fixture.RunAsync();
        Assert.AreEqual(0, fourth.ErrorsCount, "not tried again in this process");
        Assert.AreEqual(0, fourth.NotesDone);
    }

    [TestMethod]
    public async Task FewerDimensions_CutAndNormalizeTheVectors_InAnIndexOfTheirOwn()
    {
        using var fixture = await Fixture.CreateAsync(dimensions: 32);
        var note = await fixture.AddNoteAsync("One", "Text one.");

        await fixture.RunAsync();

        var status = await fixture.StatusAsync();
        Assert.AreEqual($"{Model}@32", status.IndexKey);
        var chunks = await fixture.ChunksAsync(note.NoteId, $"{Model}@32");
        Assert.IsTrue(chunks.Count > 0);
        Assert.IsTrue(chunks.All(c => c.Dimensions == 32 && c.Vector.Length == 32));
        Assert.AreEqual(1f, EmbeddingVector.Dot(chunks[0].Vector, chunks[0].Vector), 1e-5f);
        Assert.AreEqual(0, (await fixture.ChunksAsync(note.NoteId)).Count);
    }

    [TestMethod]
    public async Task AnHtmlNote_IsIndexedAsMarkdown()
    {
        using var fixture = await Fixture.CreateAsync();
        var contentType = ContentTypeExtJsonHelper.Serialize(new ContentTypeExt { ForDescription = "html" });
        var note = await fixture.AddNoteAsync("Html", "<p><strong>Hello</strong> world</p>", contentType: contentType);

        await fixture.RunAsync();

        var text = (await fixture.ChunksAsync(note.NoteId)).Single(c => c.SourceType == NoteChunkSourceType.Text).Content;
        StringAssert.Contains(text, "**Hello** world");
        Assert.IsFalse(text.Contains("<p>"));
    }

    [TestMethod]
    public async Task RefreshPending_DoesSomeNotes_WithoutTouchingTheState()
    {
        using var fixture = await Fixture.CreateAsync();
        for (var i = 1; i <= 4; i++)
            await fixture.AddNoteAsync($"Note {i}", $"Text {i}.");
        var before = await fixture.StatusAsync();

        var res = await fixture.Service.SemanticSearch.RefreshPendingAsync(new SemanticIndexRefreshRequest { MaxNotes = 2 });

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        Assert.AreEqual(2, res.Entity.NotesDone);
        var after = await fixture.StatusAsync();
        Assert.AreEqual(before.PendingNotes - 2, after.PendingNotes);
        Assert.AreEqual(EnumSemanticIndexStatus.Building, after.Status, "rows without a completed creation");
        Assert.IsNull(after.CompletedDateTime);
    }

    [TestMethod]
    public async Task RefreshPending_WhileAnotherRunGoesOn_DoesNothing()
    {
        using var fixture = await Fixture.CreateAsync();
        await fixture.AddNoteAsync("One", "Text one.");
        var started = new TaskCompletionSource();
        var release = new TaskCompletionSource();
        fixture.Models.BeforeGenerate = async (_, _) =>
        {
            started.TrySetResult();
            await release.Task;
        };

        var run = fixture.RunAsync();
        await started.Task;
        var refresh = await fixture.Service.SemanticSearch.RefreshPendingAsync(new SemanticIndexRefreshRequest());
        release.SetResult();
        fixture.Models.BeforeGenerate = null;
        await run;

        Assert.IsTrue(refresh.IsValid, refresh.ErrorMessage);
        Assert.AreEqual(0, refresh.Entity.NotesDone);
        Assert.IsNotNull(refresh.Entity.Message);
    }

    [TestMethod]
    public async Task OtherModelsIndexes_AreListed_AndOnlyTheyCanBeRemoved()
    {
        using var fixture = await Fixture.CreateAsync();
        await fixture.AddNoteAsync("One", "Text one.");
        await fixture.RunAsync();
        await fixture.SetModelAsync(Model, 32);

        var status = await fixture.StatusAsync();

        Assert.AreEqual(EnumSemanticIndexStatus.None, status.Status);
        Assert.AreEqual(Model, status.OtherIndexes.Single().IndexKey);
        Assert.IsFalse((await fixture.Service.SemanticSearch.DeleteModelIndexAsync($"{Model}@32")).IsValid, "the current one is rebuilt");
        Assert.IsTrue((await fixture.Service.SemanticSearch.DeleteModelIndexAsync(Model)).IsValid);
        Assert.AreEqual(0, (await fixture.StatusAsync()).OtherIndexes.Count);
    }
}
