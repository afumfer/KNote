using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Tests.Helpers;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// api/tracenotetypes (the web admin panel's "Trace note types" tab) - see FoldersInProcessTests for the
/// general pattern (fresh per-class Sqlite database, self-registered Admin user). Deleting a type still
/// used by trace notes is covered by NotesInProcessTests, next to the trace notes endpoints.
/// </summary>
[TestClass]
public class TraceNoteTypesInProcessTests
{
    private static KNoteWebApplicationFactory _factory = null!;
    private static HttpClient _httpClient = null!;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        (_factory, _httpClient) = await InProcessTestHost.CreateAuthenticatedClientAsync();
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        _httpClient.Dispose();
        await _factory.DisposeAsync();
    }

    [TestMethod]
    public async Task Execute_BasicCRUD()
    {
        // Create
        TraceNoteTypeDto traceNoteType = new() { TraceNoteTypeId = Guid.Empty, Name = "__TEST_TRACENOTETYPE__", Description = "__TEST_TRACENOTETYPE_DESCRIPTION__" };

        var httpRes = await _httpClient.PostAsJsonAsync("api/tracenotetypes", traceNoteType);
        var res = await httpRes.Content.ReadFromJsonAsync<Result<TraceNoteTypeDto>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsNotNull(res?.Entity);
        Assert.AreNotEqual(Guid.Empty, res!.Entity!.TraceNoteTypeId);
        Assert.AreEqual(traceNoteType.Name, res.Entity.Name);
        Assert.AreEqual(traceNoteType.Description, res.Entity.Description);

        var traceNoteTypeId = res.Entity.TraceNoteTypeId;

        // Get_All
        httpRes = await _httpClient.GetAsync("api/tracenotetypes");
        var resList = await httpRes.Content.ReadFromJsonAsync<Result<List<TraceNoteTypeDto>>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.IsTrue(resList!.Entity!.Any(t => t.TraceNoteTypeId == traceNoteTypeId));

        // Get
        httpRes = await _httpClient.GetAsync($"api/tracenotetypes/{traceNoteTypeId}");
        res = await httpRes.Content.ReadFromJsonAsync<Result<TraceNoteTypeDto>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.AreEqual(traceNoteTypeId, res!.Entity!.TraceNoteTypeId);

        // Update
        var updated = res.Entity;
        updated.Description = "__TEST_TRACENOTETYPE_DESCRIPTION__ UPDATED";
        httpRes = await _httpClient.PutAsJsonAsync("api/tracenotetypes", updated);
        res = await httpRes.Content.ReadFromJsonAsync<Result<TraceNoteTypeDto>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.AreEqual(updated.Description, res!.Entity!.Description);

        // Delete
        httpRes = await _httpClient.DeleteAsync($"api/tracenotetypes/{traceNoteTypeId}");
        res = await httpRes.Content.ReadFromJsonAsync<Result<TraceNoteTypeDto>>();

        Assert.IsTrue(httpRes.IsSuccessStatusCode);
        Assert.AreEqual(traceNoteTypeId, res!.Entity!.TraceNoteTypeId);

        httpRes = await _httpClient.GetAsync("api/tracenotetypes");
        resList = await httpRes.Content.ReadFromJsonAsync<Result<List<TraceNoteTypeDto>>>();
        Assert.IsFalse(resList!.Entity!.Any(t => t.TraceNoteTypeId == traceNoteTypeId));
    }

    [TestMethod]
    public async Task Post_WithoutName_IsRejected()
    {
        // Rejected by [ApiController]'s model validation (Name is [Required]) before reaching the service, as for
        // the other admin entities - so the body is a ProblemDetails, not a Result.
        var httpRes = await _httpClient.PostAsJsonAsync("api/tracenotetypes", new TraceNoteTypeDto { Description = "No name" });

        Assert.AreEqual(HttpStatusCode.BadRequest, httpRes.StatusCode);
    }

    [TestMethod]
    public async Task AGuest_ReadsButDoesNotChangeTypes()
    {
        // Registered after the suite's Admin, so the service makes this user a Guest.
        using var guestClient = _factory.CreateClient();
        var register = await guestClient.PostAsJsonAsync("api/users/register", new UserRegisterDto
        {
            UserName = $"guest-{Guid.NewGuid():N}"[..24],
            EMail = $"{Guid.NewGuid():N}@knote.tests",
            FullName = "Guest user",
            Password = "InProcess-Test-Password-1!"
        });
        var token = await register.Content.ReadFromJsonAsync<UserTokenDto>();
        guestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token!.token);

        var list = await guestClient.GetAsync("api/tracenotetypes");
        var post = await guestClient.PostAsJsonAsync("api/tracenotetypes", new TraceNoteTypeDto { Name = "__TEST_GUEST__" });
        var delete = await guestClient.DeleteAsync($"api/tracenotetypes/{Guid.NewGuid()}");

        Assert.IsTrue(list.IsSuccessStatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, post.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, delete.StatusCode);
    }
}
