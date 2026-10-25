using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Server.Helpers;
using KNote.Server.Local;
using KNote.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// The Server in KNote Local mode (a data folder): created the first time with a secret of its own (signing the sessions
/// works with it) and its own SQLite repository; /health says it is this KNote; the capabilities say so; the PID and the
/// control token are in the data folder while it runs, and the token is needed to stop it.
/// </summary>
[TestClass]
public class LocalModeInProcessTests
{
    private static string _dataPath = null!;
    private static KNoteWebApplicationFactory _factory = null!;
    private static HttpClient _client = null!;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        _dataPath = Path.Combine(Path.GetTempPath(), $"knote-local-{Guid.NewGuid():N}");
        _factory = new KNoteWebApplicationFactory(localDataPath: _dataPath);
        _client = _factory.CreateClient();
        await _client.GetAsync("api/server/capabilities");
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        try
        {
            Directory.Delete(_dataPath, true);
        }
        catch (IOException)
        {
            // Best effort.
        }
    }

    [TestMethod]
    public async Task FirstStart_ItsOwnRepositoryAndSecret_SignInWorks()
    {
        var localData = new KntLocalData(_dataPath);
        Assert.IsTrue(File.Exists(localData.ConfigFile));
        Assert.IsTrue(File.Exists(Path.Combine(localData.DataFolder, "KNote.db")), "The repository of the data folder, not the factory's.");

        var register = await _client.PostAsJsonAsync("api/users/register", new UserRegisterDto
        {
            UserName = $"local-{Guid.NewGuid():N}"[..20],
            EMail = $"{Guid.NewGuid():N}@knote.tests",
            FullName = "Local user",
            Password = "Local-Password-1!"
        });
        var token = await register.Content.ReadFromJsonAsync<UserTokenDto>();
        Assert.IsTrue(token!.success, token.error);

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token.token);
        Assert.AreEqual(HttpStatusCode.OK, (await client.GetAsync("api/users/me")).StatusCode, "Signed with the data folder's secret.");
    }

    [TestMethod]
    public async Task Health_SaysItIsThisKNote()
    {
        var httpRes = await _client.GetAsync("health");
        var health = JsonDocument.Parse(await httpRes.Content.ReadAsStringAsync()).RootElement;

        Assert.AreEqual(HttpStatusCode.OK, httpRes.StatusCode);
        Assert.AreEqual(KntConst.AppName, health.GetProperty("app").GetString());
        var instanceId = new KntLocalData(_dataPath).ReadConfig()["Local"]!["InstanceId"]!.GetValue<string>();
        Assert.AreEqual(instanceId, health.GetProperty("instanceId").GetString());
    }

    [TestMethod]
    public async Task Resources_ServedFromTheRepositorysFolder_NothingOutsideIt()
    {
        var repository = _factory.Services.GetRequiredService<KntRepositories>().Default;
        var repositoryRef = repository.RepositoryRef;
        var folder = Path.Combine(repositoryRef.ResourcesContainerRootPath, repositoryRef.ResourcesContainer, "2026");
        Directory.CreateDirectory(folder);
        await File.WriteAllTextAsync(Path.Combine(folder, "note.txt"), "Resource of a note");

        var resource = await _client.GetAsync($"{repositoryRef.ResourcesContainerRootUrl!.TrimStart('/')["KNote/".Length..]}/{repositoryRef.ResourcesContainer}/2026/note.txt");
        var outside = await _client.GetAsync($"Resources/{repository.RepositoryId}/..%2F..%2Fknote-local.json");
        var otherRepository = await _client.GetAsync($"Resources/{Guid.NewGuid()}/{repositoryRef.ResourcesContainer}/2026/note.txt");

        Assert.AreEqual($"/KNote/Resources/{repository.RepositoryId}", repositoryRef.ResourcesContainerRootUrl);
        Assert.AreEqual("Resource of a note", await resource.Content.ReadAsStringAsync());
        Assert.AreEqual("text/plain", resource.Content.Headers.ContentType?.MediaType);
        Assert.AreEqual(HttpStatusCode.NotFound, outside.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, otherRepository.StatusCode);
    }

    [TestMethod]
    public async Task Capabilities_LocalAdmin()
    {
        var res = await _client.GetFromJsonAsync<Result<ServerCapabilitiesDto>>("api/server/capabilities");

        Assert.IsTrue(res!.Entity!.LocalAdmin);
    }

    [TestMethod]
    public async Task PidAndControlToken_InTheDataFolder_TheTokenIsNeededToStop()
    {
        var localData = new KntLocalData(_dataPath);
        Assert.AreEqual(Environment.ProcessId.ToString(), File.ReadAllText(localData.PidFile));
        Assert.IsFalse(string.IsNullOrEmpty(File.ReadAllText(localData.ControlTokenFile)));

        var withoutToken = await _client.PostAsync("local/stop", null);
        using var wrong = new HttpRequestMessage(HttpMethod.Post, "local/stop");
        wrong.Headers.Add(KntLocal.ControlHeader, "not-the-token");
        var withWrongToken = await _client.SendAsync(wrong);

        Assert.AreEqual(HttpStatusCode.Forbidden, withoutToken.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, withWrongToken.StatusCode);
    }
}

/// <summary>
/// The Server stopped with its control token (a host of its own: it stops): the request is accepted, and the PID and
/// control token files are deleted.
/// </summary>
[TestClass]
public class LocalModeStopInProcessTests
{
    [TestMethod]
    public async Task Stop_WithTheControlToken_StopsAndDeletesItsFiles()
    {
        var dataPath = Path.Combine(Path.GetTempPath(), $"knote-local-{Guid.NewGuid():N}");
        var factory = new KNoteWebApplicationFactory(localDataPath: dataPath);
        try
        {
            using var client = factory.CreateClient();
            await client.GetAsync("api/server/capabilities");
            var localData = new KntLocalData(dataPath);
            using var request = new HttpRequestMessage(HttpMethod.Post, "local/stop");
            request.Headers.Add(KntLocal.ControlHeader, File.ReadAllText(localData.ControlTokenFile));

            var httpRes = await client.SendAsync(request);

            Assert.AreEqual(HttpStatusCode.Accepted, httpRes.StatusCode);
            var deadline = DateTime.Now.AddSeconds(10);
            while (File.Exists(localData.PidFile) && DateTime.Now < deadline)
                await Task.Delay(100);
            Assert.IsFalse(File.Exists(localData.PidFile));
            Assert.IsFalse(File.Exists(localData.ControlTokenFile));
        }
        finally
        {
            await factory.DisposeAsync();
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            try { Directory.Delete(dataPath, true); } catch (IOException) { }
        }
    }
}

/// <summary>The Server as a server (no data folder): nothing of KNote Local.</summary>
[TestClass]
public class ServerModeCapabilitiesInProcessTests
{
    [TestMethod]
    public async Task ServerMode_NoLocalAdmin_NoHealth_NoStop_NoResourcesByRepository()
    {
        await using var factory = new KNoteWebApplicationFactory();
        using var client = factory.CreateClient();

        var capabilities = await client.GetFromJsonAsync<Result<ServerCapabilitiesDto>>("api/server/capabilities");
        var health = await client.GetAsync("health");
        var stop = await client.PostAsync("local/stop", null);
        var resources = await client.GetAsync($"Resources/{Guid.NewGuid()}/container/file.txt");

        Assert.IsFalse(capabilities!.Entity!.LocalAdmin);
        Assert.AreNotEqual(HttpStatusCode.OK, health.StatusCode);
        Assert.AreNotEqual("application/json", health.Content.Headers.ContentType?.MediaType, "Not the health answer (the app's index.html, at most).");
        Assert.AreNotEqual(HttpStatusCode.Accepted, stop.StatusCode);
        Assert.AreNotEqual("text/plain", resources.Content.Headers.ContentType?.MediaType, "No resources served by repository (the app's index.html, at most).");
    }
}
