using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Server.Helpers;
using KNote.Server.Local;
using KNote.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// The computer's settings of KNote Local (api/local), for an Admin and applied at once: repositories added (new or
/// existing SQLite), refused when already linked, renamed, made the default one and unlinked (never the last one or the
/// session's, never deleting the database); AI providers with their API keys encrypted and never sent back; the e-mail
/// account and the Email alarms switch; the port. Not for a Guest, an anonymous request or another computer.
/// </summary>
[TestClass]
public class LocalSettingsInProcessTests
{
    private const string Password = "Local-Settings-1!";

    private static string _dataPath = null!;
    private static KNoteWebApplicationFactory _factory = null!;
    private static HttpClient _admin = null!;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        _dataPath = Path.Combine(Path.GetTempPath(), $"knote-local-{Guid.NewGuid():N}");
        _factory = new KNoteWebApplicationFactory(localDataPath: _dataPath);
        _admin = await SignedInClientAsync();
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        _admin.Dispose();
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

    private static KntLocalData LocalData => new(_dataPath);

    private static KntRepositories Repositories => _factory.Services.GetRequiredService<KntRepositories>();

    [TestMethod]
    public async Task Repositories_New_Rename_Default_Unlink_AppliedAtOnce()
    {
        var initial = await ReadAsync<List<LocalRepositoryDto>>(await _admin.GetAsync("api/local/repositories"));
        Assert.HasCount(1, initial);
        Assert.IsTrue(initial[0].IsOpen && initial[0].IsDefault);

        var added = await ReadAsync<List<LocalRepositoryDto>>(await _admin.PostAsJsonAsync("api/local/repositories", new LocalRepositoryAddDto { Name = "Work notes" }));

        var work = added.Single(r => r.Index == 1);
        Assert.IsTrue(work.IsOpen);
        Assert.AreEqual("Work notes", work.Name, "The name given to the new database.");
        Assert.AreEqual(Path.Combine(LocalData.DataFolder, "Work_notes.db"), work.Database);
        Assert.AreEqual(Path.Combine(LocalData.ResourcesFolder, "Work_notes"), work.ResourcesFolder);
        Assert.IsNotNull(Repositories.Find(work.RepositoryId!.Value), "Served without restarting.");

        var renamed = await ReadAsync<List<LocalRepositoryDto>>(await _admin.PutAsJsonAsync("api/local/repositories/1/name", "Work"));
        Assert.AreEqual("Work", renamed[1].Name);

        await ReadAsync<List<LocalRepositoryDto>>(await _admin.PutAsync("api/local/repositories/1/default", null));
        Assert.AreEqual(work.RepositoryId, Repositories.Default.RepositoryId);
        await ReadAsync<List<LocalRepositoryDto>>(await _admin.PutAsync("api/local/repositories/0/default", null));

        var unlinked = await ReadAsync<List<LocalRepositoryDto>>(await _admin.DeleteAsync("api/local/repositories/1"));
        Assert.HasCount(1, unlinked);
        Assert.IsNull(Repositories.Find(work.RepositoryId.Value));
        Assert.IsTrue(File.Exists(work.Database), "Unlinking never deletes the database.");

        // An existing database (as one of the desktop app): linked by its path, with its own name.
        var linked = await ReadAsync<List<LocalRepositoryDto>>(await _admin.PostAsJsonAsync("api/local/repositories",
            new LocalRepositoryAddDto { DatabasePath = work.Database, ResourcesFolder = Path.Combine(_dataPath, "OtherResources") }));
        Assert.AreEqual("Work", linked[1].Name);
        Assert.AreEqual(work.RepositoryId, linked[1].RepositoryId);
        await ReadAsync<List<LocalRepositoryDto>>(await _admin.DeleteAsync("api/local/repositories/1"));
    }

    [TestMethod]
    public async Task Repositories_Refused_AlreadyLinked_Missing_TheSessionsAndTheLastOne()
    {
        var sessionDatabase = (await ReadAsync<List<LocalRepositoryDto>>(await _admin.GetAsync("api/local/repositories")))[0].Database;

        var again = await _admin.PostAsJsonAsync("api/local/repositories", new LocalRepositoryAddDto { DatabasePath = sessionDatabase });
        var missing = await _admin.PostAsJsonAsync("api/local/repositories", new LocalRepositoryAddDto { DatabasePath = Path.Combine(_dataPath, "none.db") });
        var session = await _admin.DeleteAsync("api/local/repositories/0");

        StringAssert.Contains(await ErrorAsync(again), "already linked");
        StringAssert.Contains(await ErrorAsync(missing), "no database file");
        StringAssert.Contains(await ErrorAsync(session), "last repository");
        Assert.HasCount(1, Repositories.All);
    }

    [TestMethod]
    public async Task Repositories_TheSessionsOne_CanNotBeUnlinked()
    {
        await ReadAsync<List<LocalRepositoryDto>>(await _admin.PostAsJsonAsync("api/local/repositories", new LocalRepositoryAddDto { Name = $"Other {Guid.NewGuid():N}"[..12] }));
        try
        {
            var session = await _admin.DeleteAsync("api/local/repositories/0");

            StringAssert.Contains(await ErrorAsync(session), "repository of your session");
        }
        finally
        {
            await ReadAsync<List<LocalRepositoryDto>>(await _admin.DeleteAsync("api/local/repositories/1"));
        }
    }

    [TestMethod]
    public async Task AiProviders_KeyEncryptedAndNeverSentBack_OnlyTheComputersOnes_EmptyKeyKeepsIt()
    {
        var httpRes = await _admin.PutAsJsonAsync("api/local/ai", new List<LocalAiProviderDto>
        {
            new() { Alias = "Local OpenAI", Provider = EnumAiProvider.OpenAI, Model = "gpt-test", ApiKey = "sk-secret-1" }
        });
        var body = await httpRes.Content.ReadAsStringAsync();
        Assert.AreEqual(HttpStatusCode.OK, httpRes.StatusCode, body);
        Assert.DoesNotContain("sk-secret-1", body, "Never sent back.");

        var stored = LocalData.ReadConfig()["Local"]!["ai"]!["providers"]![0]!["apiKey"]!.GetValue<string>();
        Assert.IsTrue(KntSecretProtector.IsProtected(stored), "Encrypted in the file.");
        var providers = await ReadAsync<List<AiProviderInfoDto>>(await _admin.GetAsync("api/aiassistant/providers"));
        Assert.AreEqual("Local OpenAI", providers.Single().Alias, "Applied at once, without appsettings.json's samples.");

        var kept = await ReadAsync<List<LocalAiProviderDto>>(await _admin.PutAsJsonAsync("api/local/ai", new List<LocalAiProviderDto>
        {
            new() { Index = 0, Alias = "Local OpenAI", Provider = EnumAiProvider.OpenAI, Model = "gpt-test-2" }
        }));
        Assert.IsTrue(kept.Single().HasApiKey);
        Assert.AreEqual(stored, LocalData.ReadConfig()["Local"]!["ai"]!["providers"]![0]!["apiKey"]!.GetValue<string>());

        var invalid = await _admin.PutAsJsonAsync("api/local/ai", new List<LocalAiProviderDto> { new() { Alias = "No host", Provider = EnumAiProvider.Ollama, Model = "llama" } });
        Assert.AreEqual(HttpStatusCode.BadRequest, invalid.StatusCode);
    }

    [TestMethod]
    public async Task Email_PasswordEncrypted_AndTheAlarmsSwitch_AppliedAtOnce()
    {
        var saved = await ReadAsync<LocalEmailSettingsDto>(await _admin.PutAsJsonAsync("api/local/email", new LocalEmailSettingsDto
        {
            Host = "smtp.local.tests", Port = 2525, EnableSsl = false, FromAddress = "knote@local.tests", Username = "user", Password = "smtp-secret", SendEmailAlarms = true
        }));

        Assert.IsTrue(saved.HasPassword);
        Assert.IsNull(saved.Password);
        Assert.IsTrue(KntSecretProtector.IsProtected(LocalData.ReadConfig()["Email"]!["Password"]!.GetValue<string>()));
        var email = _factory.Services.GetRequiredService<IOptions<EmailConfig>>().Value;
        Assert.AreEqual("smtp.local.tests", email.Host);
        Assert.AreEqual("smtp-secret", email.Password);
        Assert.IsTrue(_factory.Services.GetRequiredService<IOptions<AlarmsConfig>>().Value.Email.Enabled);

        var withoutServer = await _admin.PutAsJsonAsync("api/local/email", new LocalEmailSettingsDto { SendEmailAlarms = true });
        Assert.AreEqual(HttpStatusCode.BadRequest, withoutServer.StatusCode);

        await ReadAsync<LocalEmailSettingsDto>(await _admin.PutAsJsonAsync("api/local/email", new LocalEmailSettingsDto { Host = "smtp.local.tests", FromAddress = "knote@local.tests" }));
        Assert.AreEqual("smtp-secret", _factory.Services.GetRequiredService<IOptions<EmailConfig>>().Value.Password, "An empty password keeps it.");
        Assert.IsFalse(_factory.Services.GetRequiredService<IOptions<AlarmsConfig>>().Value.Email.Enabled);
    }

    [TestMethod]
    public async Task Port_SavedForTheNextStart_AndValidated()
    {
        var saved = await ReadAsync<LocalServerSettingsDto>(await _admin.PutAsJsonAsync("api/local/server/port", 6123));
        var invalid = await _admin.PutAsJsonAsync("api/local/server/port", 80);

        Assert.AreEqual(6123, saved.Port);
        Assert.AreEqual("http://127.0.0.1:6123", saved.Url);
        Assert.AreEqual(_dataPath, saved.DataPath);
        Assert.AreEqual(HttpStatusCode.BadRequest, invalid.StatusCode);
    }

    [TestMethod]
    public async Task NotForAGuest_NorAnonymous()
    {
        using var guest = await SignedInClientAsync();
        using var anonymous = _factory.CreateClient();

        Assert.AreEqual(HttpStatusCode.Forbidden, (await guest.GetAsync("api/local/repositories")).StatusCode);
        Assert.AreEqual(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("api/local/repositories")).StatusCode);
    }

    [TestMethod]
    public void OnlyRequestsFromTheComputer()
    {
        var fromNetwork = new DefaultHttpContext();
        fromNetwork.Connection.RemoteIpAddress = IPAddress.Parse("192.168.1.20");
        var fromComputer = new DefaultHttpContext();
        fromComputer.Connection.RemoteIpAddress = IPAddress.IPv6Loopback;

        Assert.IsFalse(KntLocal.IsLocalRequest(fromNetwork));
        Assert.IsTrue(KntLocal.IsLocalRequest(fromComputer));
    }

    // The first one registered in the new database is an Admin; the next ones, Guests.
    private static async Task<HttpClient> SignedInClientAsync()
    {
        var client = _factory.CreateClient();
        var token = await (await client.PostAsJsonAsync("api/users/register", new UserRegisterDto
        {
            UserName = $"local-{Guid.NewGuid():N}"[..20],
            EMail = $"{Guid.NewGuid():N}@knote.tests",
            FullName = "Local settings user",
            Password = Password
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

    private static async Task<string> ErrorAsync(HttpResponseMessage httpRes)
    {
        Assert.AreEqual(HttpStatusCode.BadRequest, httpRes.StatusCode);
        return (await httpRes.Content.ReadFromJsonAsync<Result<List<LocalRepositoryDto>>>())!.ErrorMessage;
    }
}
