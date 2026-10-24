using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Tests.Helpers;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// The alarms of a note (api/notes/{id}/alarms, api/notes/alarms), saved at once as the web note editor does: a new
/// one with the Service's default values, saved, read, changed and deleted; any user reads them, only Staff or higher
/// writes them; and an alarm of another note is never touched.
/// </summary>
[TestClass]
public class NoteAlarmsInProcessTests
{
    private static KNoteWebApplicationFactory _factory = null!;
    private static HttpClient _httpClient = null!;
    private static FolderInfoDto _folder = null!;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        (_factory, _httpClient) = await InProcessTestHost.CreateAuthenticatedClientAsync();
        var folders = await (await _httpClient.GetAsync("api/folders")).Content.ReadFromJsonAsync<Result<List<FolderInfoDto>>>();
        _folder = folders!.Entity!.First();
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        _httpClient.Dispose();
        await _factory.DisposeAsync();
    }

    [TestMethod]
    public async Task Alarms_New_Save_Read_Change_Delete()
    {
        var note = await CreateNoteAsync("__TEST ALARMS CRUD__");

        var newAlarm = await ReadAsync<KMessageDto>(await _httpClient.GetAsync($"api/notes/{note.NoteId}/alarms/new"));
        Assert.AreEqual(note.NoteId, newAlarm.NoteId);
        Assert.AreEqual(EnumNotificationType.AppInfo, newAlarm.NotificationType);
        Assert.IsTrue(newAlarm.AlarmActivated);
        Assert.IsNotNull(newAlarm.UserId, "For the user signed in.");
        Assert.AreEqual(Guid.Empty, newAlarm.KMessageId);

        newAlarm.Comment = "Call the customer";
        var saved = await ReadAsync<KMessageDto>(await _httpClient.PostAsJsonAsync("api/notes/alarms", newAlarm));
        Assert.AreNotEqual(Guid.Empty, saved.KMessageId);

        var listed = await ReadAsync<List<KMessageDto>>(await _httpClient.GetAsync($"api/notes/{note.NoteId}/alarms"));
        Assert.AreEqual("Call the customer", listed.Single().Comment);
        Assert.AreEqual(newAlarm.UserFullName, listed.Single().UserFullName);

        saved.AlarmType = EnumAlarmType.InMinutes;
        saved.AlarmMinutes = 15;
        saved.AlarmActivated = false;
        await ReadAsync<KMessageDto>(await _httpClient.PutAsJsonAsync("api/notes/alarms", saved));
        var changed = await ReadAsync<KMessageDto>(await _httpClient.GetAsync($"api/notes/alarms/{saved.KMessageId}"));
        Assert.AreEqual(EnumAlarmType.InMinutes, changed.AlarmType);
        Assert.AreEqual(15, changed.AlarmMinutes);
        Assert.IsFalse(changed.AlarmActivated);

        await ReadAsync<KMessageDto>(await _httpClient.DeleteAsync($"api/notes/alarms/{saved.KMessageId}"));
        Assert.IsEmpty(await ReadAsync<List<KMessageDto>>(await _httpClient.GetAsync($"api/notes/{note.NoteId}/alarms")));
    }

    [TestMethod]
    public async Task Alarms_OfAnotherNote_AreNotTouched()
    {
        var note = await CreateNoteAsync("__TEST ALARMS NOTE__");
        var other = await CreateNoteAsync("__TEST ALARMS OTHER NOTE__");
        var otherAlarm = await SaveNewAlarmAsync(other.NoteId, "Of the other note");
        var alarm = await SaveNewAlarmAsync(note.NoteId, "Of this note");

        await ReadAsync<KMessageDto>(await _httpClient.DeleteAsync($"api/notes/alarms/{alarm.KMessageId}"));

        Assert.IsEmpty(await ReadAsync<List<KMessageDto>>(await _httpClient.GetAsync($"api/notes/{note.NoteId}/alarms")));
        var otherAlarms = await ReadAsync<List<KMessageDto>>(await _httpClient.GetAsync($"api/notes/{other.NoteId}/alarms"));
        Assert.AreEqual(otherAlarm.KMessageId, otherAlarms.Single().KMessageId);
    }

    [TestMethod]
    public async Task Alarms_AGuestReadsThem_ButCanNotWriteThem()
    {
        var note = await CreateNoteAsync("__TEST ALARMS GUEST__");
        var alarm = await SaveNewAlarmAsync(note.NoteId, "Read by a guest");
        using var guest = await GuestClientAsync();

        var list = await guest.GetAsync($"api/notes/{note.NoteId}/alarms");
        var one = await guest.GetAsync($"api/notes/alarms/{alarm.KMessageId}");
        var newOne = await guest.GetAsync($"api/notes/{note.NoteId}/alarms/new");
        var save = await guest.PutAsJsonAsync("api/notes/alarms", alarm);
        var delete = await guest.DeleteAsync($"api/notes/alarms/{alarm.KMessageId}");

        Assert.AreEqual(HttpStatusCode.OK, list.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, one.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, newOne.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, save.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, delete.StatusCode);
    }

    [TestMethod]
    public async Task Alarms_InMinutesWithoutMinutes_IsRefused()
    {
        var note = await CreateNoteAsync("__TEST ALARMS INVALID__");
        var alarm = await ReadAsync<KMessageDto>(await _httpClient.GetAsync($"api/notes/{note.NoteId}/alarms/new"));
        alarm.AlarmType = EnumAlarmType.InMinutes;
        alarm.AlarmMinutes = 0;

        var httpRes = await _httpClient.PostAsJsonAsync("api/notes/alarms", alarm);

        Assert.AreEqual(HttpStatusCode.BadRequest, httpRes.StatusCode);
        Assert.IsEmpty(await ReadAsync<List<KMessageDto>>(await _httpClient.GetAsync($"api/notes/{note.NoteId}/alarms")));
    }

    private static async Task<KMessageDto> SaveNewAlarmAsync(Guid noteId, string comment)
    {
        var alarm = await ReadAsync<KMessageDto>(await _httpClient.GetAsync($"api/notes/{noteId}/alarms/new"));
        alarm.Comment = comment;
        return await ReadAsync<KMessageDto>(await _httpClient.PostAsJsonAsync("api/notes/alarms", alarm));
    }

    // Registered after the class' Admin: a Guest (KntUsersRegisterAsyncCommand).
    private static async Task<HttpClient> GuestClientAsync()
    {
        var client = _factory.CreateClient();
        var token = await (await client.PostAsJsonAsync("api/users/register", new UserRegisterDto
        {
            UserName = $"guest-{Guid.NewGuid():N}"[..24],
            EMail = $"{Guid.NewGuid():N}@knote.tests",
            FullName = "Alarms guest",
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

    private static async Task<NoteDto> CreateNoteAsync(string topic)
    {
        var note = new NoteDto
        {
            NoteId = Guid.Empty,
            Topic = topic,
            Description = "Created by NoteAlarmsInProcessTests.",
            FolderId = _folder.FolderId,
            FolderDto = _folder,
            CreationDateTime = DateTime.Now,
            ModificationDateTime = DateTime.Now
        };
        return await ReadAsync<NoteDto>(await _httpClient.PostAsJsonAsync("api/notes", note));
    }
}
