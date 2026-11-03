using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Service.Interfaces;

namespace KNote.Tests.Helpers;

/// <summary>
/// A repository of the tests of KNote Atlas (a Sqlite database of its own) with the fake embedding model as the
/// repository's one (FakeEmbeddingGenerators): notes, runs of the index and its state through the Service layer.
/// </summary>
internal sealed class SemanticIndexFixture : IDisposable
{
    public const string Model = "fake-embed";

    public RepositoryTestDatabase Db { get; } = new();
    public FakeEmbeddingGenerators Models { get; } = new(Model);
    public KntService Service { get; private set; } = null!;
    public Guid HomeId { get; private set; }
    public Guid UserId { get; private set; }

    public static async Task<SemanticIndexFixture> CreateAsync(string orm = "Dapper", int? dimensions = null, string model = Model)
    {
        var fixture = new SemanticIndexFixture();
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
internal sealed class SyncProgress : IProgress<SemanticIndexProgressDto>
{
    private readonly Action<SemanticIndexProgressDto> _report;
    public SyncProgress(Action<SemanticIndexProgressDto> report) => _report = report;
    public void Report(SemanticIndexProgressDto value) => _report(value);
}
