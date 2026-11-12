using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// The semantic index through the API (api/semantic, KNote Atlas 0.3), with a fake embedding model in the Server: an
/// Admin creates it in the background (KntAtlasJobs) and follows its progress, any user reads its status, and a
/// Guest can neither create, cancel nor remove indexes.
/// </summary>
[TestClass]
public class SemanticIndexInProcessTests
{
    private const string Model = "fake-embed";

    private static KNoteWebApplicationFactory _factory = null!;
    private static HttpClient _admin = null!;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        (_factory, _admin) = await InProcessTestHost.CreateAuthenticatedClientAsync(factory =>
            factory.TestServices = services => services.AddSingleton<IKntEmbeddingGenerators>(new FakeEmbeddingGenerators(Model)));
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        _admin.Dispose();
        await _factory.DisposeAsync();
    }

    [TestMethod]
    public async Task AnAdmin_CreatesTheIndexInTheBackground_AndFollowsItsProgress()
    {
        await ReadAsync<RepositoryAiModelsDto>(await _admin.PostAsJsonAsync("api/semantic/models", new RepositoryAiModelsDto { EmbeddingModel = Model }));
        var before = await ReadAsync<SemanticIndexStatusDto>(await _admin.GetAsync("api/semantic/status"));
        Assert.AreEqual(EnumSemanticIndexStatus.None, before.Status);
        Assert.IsTrue(before.EmbeddingModelAvailable);

        var started = await ReadAsync<SemanticIndexProgressDto>(await _admin.PostAsJsonAsync("api/semantic/index", new SemanticIndexRunDto()));
        Assert.IsFalse(started.Rebuild);
        var progress = started;
        for (var i = 0; i < 200 && !progress.IsFinished; i++)
        {
            await Task.Delay(50);
            progress = await ReadAsync<SemanticIndexProgressDto>(await _admin.GetAsync("api/semantic/index/progress"));
        }

        Assert.AreEqual(EnumSemanticIndexPhase.Completed, progress.Phase, progress.Message);
        Assert.AreEqual(before.PendingNotes, progress.NotesDone);
        Assert.IsNotNull(progress.RequestedBy);
        var after = await ReadAsync<SemanticIndexStatusDto>(await _admin.GetAsync("api/semantic/status"));
        Assert.AreEqual(EnumSemanticIndexStatus.Ready, after.Status);
        Assert.AreEqual(0, after.PendingNotes);
        Assert.AreEqual(after.TotalNotes, after.IndexedNotes);
        Assert.IsFalse(await ReadAsync<bool>(await _admin.PostAsync("api/semantic/index/cancel", null)), "nothing to cancel");
    }

    [TestMethod]
    public async Task AGuest_ReadsTheStatus_ButCannotRunCancelOrRemove()
    {
        using var guest = await GuestClientAsync();

        var status = await guest.GetAsync("api/semantic/status");
        var run = await guest.PostAsJsonAsync("api/semantic/index", new SemanticIndexRunDto());
        var progress = await guest.GetAsync("api/semantic/index/progress");
        var cancel = await guest.PostAsync("api/semantic/index/cancel", null);
        var delete = await guest.DeleteAsync("api/semantic/index?key=other");

        Assert.AreEqual(HttpStatusCode.OK, status.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, run.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, progress.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, cancel.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, delete.StatusCode);
    }

    [TestMethod]
    public async Task RemovingTheCurrentIndex_IsABadRequest()
    {
        await ReadAsync<RepositoryAiModelsDto>(await _admin.PostAsJsonAsync("api/semantic/models", new RepositoryAiModelsDto { EmbeddingModel = Model }));

        var httpRes = await _admin.DeleteAsync($"api/semantic/index?key={Uri.EscapeDataString(Model)}");
        var other = await _admin.DeleteAsync($"api/semantic/index?key={Uri.EscapeDataString("other-model@512")}");

        Assert.AreEqual(HttpStatusCode.BadRequest, httpRes.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, other.StatusCode);
    }

    // Registered after the class' Admin: a Guest (KntUsersRegisterAsyncCommand).
    private static async Task<HttpClient> GuestClientAsync()
    {
        var client = _factory.CreateClient();
        var token = await (await client.PostAsJsonAsync("api/users/register", new UserRegisterDto
        {
            UserName = $"guest-{Guid.NewGuid():N}"[..24],
            EMail = $"{Guid.NewGuid():N}@knote.tests",
            FullName = "Atlas guest",
            Password = "InProcess-Test-Password-1!"
        })).Content.ReadFromJsonAsync<UserTokenDto>();
        Assert.IsTrue(token!.success, token.error);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token.token);
        return client;
    }

    private static async Task<T> ReadAsync<T>(HttpResponseMessage httpRes)
    {
        var result = await httpRes.Content.ReadFromJsonAsync<Result<T>>();
        Assert.IsNotNull(result);
        Assert.IsTrue(httpRes.IsSuccessStatusCode && result.IsValid, $"{httpRes.StatusCode}: {result.ErrorMessage}");
        return result.Entity;
    }
}
