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
/// SemanticSearchController (api/semantic), delivery 0.1 of KNote Atlas: the repository's AI models, read by any user
/// with a session (with whether this server has the embedding model) and saved only by an Admin; the server's models
/// for an Admin; the AI assistant keeps seeing only the chat providers.
/// </summary>
[TestClass]
public class SemanticSearchInProcessTests
{
    private static KNoteWebApplicationFactory _factory = null!;
    private static HttpClient _admin = null!;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        (_factory, _admin) = await InProcessTestHost.CreateAuthenticatedClientAsync(factory =>
        {
            // The two first providers of appsettings.json are replaced by these ones.
            factory.AppConfiguration["ai:providers:0:alias"] = "Embeddings";
            factory.AppConfiguration["ai:providers:0:provider"] = EnumAiProvider.Ollama;
            factory.AppConfiguration["ai:providers:0:model"] = "qwen3-embedding:0.6b";
            factory.AppConfiguration["ai:providers:0:host"] = "http://localhost:11434";
            factory.AppConfiguration["ai:providers:0:kind"] = EnumAiProviderKind.Embeddings;
            factory.AppConfiguration["ai:providers:1:alias"] = "Chat";
            factory.AppConfiguration["ai:providers:1:provider"] = EnumAiProvider.Ollama;
            factory.AppConfiguration["ai:providers:1:model"] = "qwen3.5:9b";
            factory.AppConfiguration["ai:providers:1:host"] = "http://localhost:11434";
            factory.AppConfiguration["ai:providers:1:kind"] = EnumAiProviderKind.Chat;
        });
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        _admin.Dispose();
        await _factory.DisposeAsync();
    }

    [TestMethod]
    public async Task Models_SavedByAnAdmin_ReadByAGuest_WithTheirAvailability()
    {
        var saved = await ReadAsync<RepositoryAiModelsDto>(await _admin.PostAsJsonAsync("api/semantic/models",
            new RepositoryAiModelsDto { EmbeddingModel = "qwen3-embedding:0.6b", LlmModel = "qwen3.5:9b" }));
        Assert.IsTrue(saved.EmbeddingModelAvailable, "This server has an embeddings provider of the model.");

        using var guest = await GuestClientAsync();
        var read = await ReadAsync<RepositoryAiModelsDto>(await guest.GetAsync("api/semantic/models"));
        Assert.AreEqual("qwen3-embedding:0.6b", read.EmbeddingModel);
        Assert.AreEqual("qwen3.5:9b", read.LlmModel);

        var notHere = await ReadAsync<RepositoryAiModelsDto>(await _admin.PostAsJsonAsync("api/semantic/models",
            new RepositoryAiModelsDto { EmbeddingModel = "text-embedding-3-small" }));
        Assert.IsFalse(notHere.EmbeddingModelAvailable);
    }

    [TestMethod]
    public async Task Models_NotSavedByAGuest_NorReadWithoutASession()
    {
        using var guest = await GuestClientAsync();
        using var anonymous = _factory.CreateClient();

        var byGuest = await guest.PostAsJsonAsync("api/semantic/models", new RepositoryAiModelsDto { EmbeddingModel = "other-model" });
        var installationByGuest = await guest.GetAsync("api/semantic/installation-models");
        var byAnonymous = await anonymous.GetAsync("api/semantic/models");

        Assert.AreEqual(HttpStatusCode.Forbidden, byGuest.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, installationByGuest.StatusCode);
        Assert.AreEqual(HttpStatusCode.Unauthorized, byAnonymous.StatusCode);
    }

    [TestMethod]
    public async Task Models_Invalid_AreABadRequest_AndNotSaved()
    {
        // The DTO's own validation: refused by [ApiController] before the command (which refuses it too).
        var httpRes = await _admin.PostAsJsonAsync("api/semantic/models", new RepositoryAiModelsDto { EmbeddingDimensions = 512 });
        var read = await ReadAsync<RepositoryAiModelsDto>(await _admin.GetAsync("api/semantic/models"));

        Assert.AreEqual(HttpStatusCode.BadRequest, httpRes.StatusCode);
        Assert.AreNotEqual(512, read.EmbeddingDimensions);
    }

    [TestMethod]
    public async Task InstallationModels_AreTheServersProviders_WithoutHosts()
    {
        var httpRes = await _admin.GetAsync("api/semantic/installation-models");
        var body = await httpRes.Content.ReadAsStringAsync();
        var models = await ReadAsync<AiInstallationModelsDto>(httpRes);

        Assert.AreEqual("qwen3-embedding:0.6b", models.EmbeddingModels.Single().Model);
        Assert.IsTrue(models.ChatModels.Any(m => m.Model == "qwen3.5:9b"));
        Assert.IsFalse(models.ChatModels.Any(m => m.Model == "qwen3-embedding:0.6b"));
        Assert.DoesNotContain("localhost:11434", body, "Hosts never leave the server.");
    }

    [TestMethod]
    public async Task TheAssistant_SeesOnlyTheChatProviders()
    {
        var providers = await ReadAsync<List<AiProviderInfoDto>>(await _admin.GetAsync("api/aiassistant/providers"));

        Assert.IsFalse(providers.Any(p => p.Alias == "Embeddings"));
        Assert.AreEqual("Chat", providers.Single(p => p.IsDefault).Alias);
    }

    [TestMethod]
    public void TheServiceOfEachRequest_HasTheServersEmbeddingModels()
    {
        var generators = _factory.Services.GetRequiredService<IKntEmbeddingGenerators>();

        Assert.IsNotNull(generators.Find("QWEN3-EMBEDDING:0.6B"));
        Assert.IsNull(generators.Find("qwen3.5:9b"), "A chat model is not an embedding model.");
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
        var body = await httpRes.Content.ReadAsStringAsync();
        Assert.IsTrue(httpRes.IsSuccessStatusCode, $"{httpRes.StatusCode}: {body}");
        var res = System.Text.Json.JsonSerializer.Deserialize<Result<T>>(body, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web));
        Assert.IsTrue(res!.IsValid, res.ErrorMessage);
        return res.Entity!;
    }
}
