using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Service.ServicesCommands;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// «Recover deleted notes» (H.4b, H24, H26, H27) and the retention of the history (H25): the deleted notes that keep their last
/// version; recovering one (the same id and its history, its number if free, its folder or Home, its creation date, a row Restored);
/// «Remove permanently» (Admin); and keeping the retention (Admin, validated, noted as a setting). Dapper and EF.
/// </summary>
[TestClass]
public class NoteHistoryRecoverServiceTests
{
    private RepositoryTestDatabase db = null!;
    private KntService service = null!;
    private FolderDto home = null!;

    private async Task OpenAsync(string orm)
    {
        db = new RepositoryTestDatabase();
        service = new KntService(db.CreateRepository(orm), activateMessageBroker: false)
        {
            UserIdentityName = "armando",
            HistoryOrigin = EnumHistoryOrigin.Desktop
        };
        home = (await service.Repository.Folders.GetAsync(KntConst.DefaultFolderNumber)).Entity;
        Assert.IsNotNull(home, "the test database has its Home folder");
    }

    [TestCleanup]
    public void TestCleanup()
    {
        service?.Dispose();
        db?.Dispose();
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ADeletedNote_IsRecovered_WithItsIdNumberAndHistory(string orm)
    {
        await OpenAsync(orm);
        var computer = await service.Repository.NoteTypes.AddAsync(new NoteTypeDto { NoteTypeId = Guid.NewGuid(), Name = "Computer" });
        var status = await service.Repository.KAttributes.AddAsync(new KAttributeDto
        {
            KAttributeId = Guid.NewGuid(), Name = "Status", NoteTypeId = computer.Entity.NoteTypeId, KAttributeDataType = EnumKAttributeDataType.String
        });
        var note = await StoredNoteAsync("Amstrad CPC 464", home, computer.Entity.NoteTypeId,
            new NoteKAttributeDto { NoteKAttributeId = Guid.NewGuid(), KAttributeId = status.Entity.KAttributeId, Name = "Status", Value = "Works" });
        var created = (await service.History.GetNoteHistoryAsync(note.NoteId)).Entity.Rows.Single();
        service.UserIdentityName = "marta";
        Assert.IsTrue((await service.Notes.DeleteExtendedAsync(note.NoteId)).IsValid);

        var deleted = (await service.History.GetDeletedNotesAsync()).Entity.Single();
        Assert.AreEqual(note.NoteId, deleted.NoteId);
        Assert.AreEqual(note.NoteNumber, deleted.NoteNumber);
        Assert.AreEqual("Amstrad CPC 464", deleted.Topic);
        Assert.AreEqual("Home", deleted.FolderPath);
        Assert.AreEqual("marta", deleted.DeletedBy);

        service.UserIdentityName = "armando";
        var result = await service.History.RecoverNoteAsync(note.NoteId);

        Assert.IsTrue(result.IsValid, result.ErrorMessage);
        Assert.AreEqual(note.NoteNumber, result.Entity.NoteNumber);
        CollectionAssert.Contains(result.Entity.Notices, KntHistoryRecoverNoteAsyncCommand.PartsNotRecovered);
        var recovered = (await service.Notes.GetAsync(note.NoteId)).Entity;
        Assert.AreEqual("Amstrad CPC 464", recovered.Topic);
        Assert.AreEqual("Text of the note", recovered.Description);
        Assert.AreEqual(home.FolderId, recovered.FolderId);
        Assert.AreEqual(computer.Entity.NoteTypeId, recovered.NoteTypeId);
        Assert.AreEqual("Works", recovered.KAttributesDto.Single(a => a.KAttributeId == status.Entity.KAttributeId).Value);
        Assert.AreEqual(created.RegistryDateTime, recovered.CreationDateTime, "created when its row Created says (H26)");

        var rows = (await service.History.GetNoteHistoryAsync(note.NoteId)).Entity.Rows;
        CollectionAssert.AreEqual(new[] { "Restored", "Deleted", "Created" }, rows.Select(r => r.ActionType).ToArray(), "its history goes on");
        StringAssert.StartsWith(rows.First().RegistryMessage, "Recovered from the deleted notes (deleted ");
        Assert.AreEqual(0, (await service.History.GetDeletedNotesAsync()).Entity.Count);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ANumberTaken_GivesTheNoteAnotherOne(string orm)
    {
        await OpenAsync(orm);
        var note = await StoredNoteAsync("The last note", home);
        Assert.IsTrue((await service.Notes.DeleteExtendedAsync(note.NoteId)).IsValid);
        var other = await StoredNoteAsync("A new note", home);
        Assert.AreEqual(note.NoteNumber, other.NoteNumber, "the number of the last note deleted is given again (MAX + 1)");

        var result = await service.History.RecoverNoteAsync(note.NoteId);

        Assert.IsTrue(result.IsValid, result.ErrorMessage);
        Assert.AreNotEqual(note.NoteNumber, result.Entity.NoteNumber);
        Assert.AreEqual($"Its number #{note.NoteNumber} is now another note's: it is #{result.Entity.NoteNumber}.", result.Entity.Notices.First());
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AFolderThatNoLongerExists_PutsItInHome(string orm)
    {
        await OpenAsync(orm);
        var projects = (await service.Repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Projects" })).Entity;
        var note = await StoredNoteAsync("In projects", projects);
        Assert.IsTrue((await service.Notes.DeleteExtendedAsync(note.NoteId)).IsValid);
        Assert.IsTrue((await service.Repository.Folders.DeleteAsync(projects.FolderId)).IsValid);

        var result = await service.History.RecoverNoteAsync(note.NoteId);

        Assert.IsTrue(result.IsValid, result.ErrorMessage);
        Assert.AreEqual(home.FolderId, (await service.Notes.GetAsync(note.NoteId)).Entity.FolderId);
        CollectionAssert.Contains(result.Entity.Notices, "Its folder no longer exists: it is in Home.");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task RemovedPermanently_ItCanNoLongerBeRecovered(string orm)
    {
        await OpenAsync(orm);
        var note = await StoredNoteAsync("Secret", home);
        Assert.IsTrue((await service.Notes.DeleteExtendedAsync(note.NoteId)).IsValid);

        Assert.IsTrue((await service.History.RemoveDeletedNoteAsync(note.NoteId)).IsValid);

        Assert.AreEqual(0, (await service.History.GetDeletedNotesAsync()).Entity.Count);
        var result = await service.History.RecoverNoteAsync(note.NoteId);
        Assert.IsFalse(result.IsValid);
        StringAssert.Contains(result.ErrorMessage, KntHistoryRecoverNoteAsyncCommand.NotDeleted);
        var rows = (await service.History.GetNoteHistoryAsync(note.NoteId)).Entity.Rows;
        CollectionAssert.AreEqual(new[] { "Removed", "Deleted", "Created" }, rows.Select(r => r.ActionType).ToArray(),
            "its rows stay, and one more says who removed it");
        Assert.AreEqual("Removed permanently from the deleted notes", rows[0].RegistryMessage);
        Assert.AreEqual("armando", rows[0].UserName);
        Assert.IsTrue(rows.All(r => !r.HasPreviousContent));
        var version = await service.History.GetNoteVersionAsync(note.NoteId, rows[1].KLogId);
        StringAssert.Contains(version.ErrorMessage, KNote.Model.Core.HistoryRules.VersionCopyNotKept, "«See this version» says why");
        Assert.IsFalse((await service.History.RemoveDeletedNoteAsync(note.NoteId)).IsValid, "once");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ANoteThatExists_IsNotRecovered(string orm)
    {
        await OpenAsync(orm);
        var note = await StoredNoteAsync("Alive", home);

        var result = await service.History.RecoverNoteAsync(note.NoteId);

        Assert.IsFalse(result.IsValid);
        Assert.IsFalse((await service.History.RemoveDeletedNoteAsync(note.NoteId)).IsValid);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task TheRetention_IsKeptAndNoted(string orm)
    {
        await OpenAsync(orm);
        var before = (await service.History.GetRetentionAsync()).Entity;
        Assert.AreEqual(3, before.Retention.KeepAllMonths, "the default one");

        var saved = await service.History.SaveRetentionAsync(new HistoryRetentionDto { KeepAllMonths = 1, DailyUntilMonths = 6, DeletedNotesDays = 10 });

        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        var after = (await service.History.GetRetentionAsync()).Entity.Retention;
        Assert.AreEqual(1, after.KeepAllMonths);
        Assert.AreEqual(6, after.DailyUntilMonths);
        Assert.AreEqual(10, after.DeletedNotesDays);
        var row = (await service.Repository.History.GetEntityHistoryAsync(HistoryEntities.SystemValue,
            HistoryRules.SettingId(KntConst.SystemScope, KntConst.HistoryRetentionKey))).Entity.Single();
        Assert.AreEqual(KntHistory.HistoryRetentionTitle, row.EntityTitle);
        StringAssert.Contains(row.RegistryMessage, "Months with all the versions: 3 → 1");
        StringAssert.Contains(row.RegistryMessage, "Days in the deleted notes: 30 → 10");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ARetentionOutOfItsLimits_IsNotKept(string orm)
    {
        await OpenAsync(orm);

        var saved = await service.History.SaveRetentionAsync(new HistoryRetentionDto { KeepAllMonths = 6, DailyUntilMonths = 2 });

        Assert.IsFalse(saved.IsValid);
        StringAssert.Contains(saved.ErrorMessage, "can't be fewer than the months with all of them");
        Assert.AreEqual(3, (await service.History.GetRetentionAsync()).Entity.Retention.KeepAllMonths);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AStaffUser_RecoversButDoesntRemoveNorKeepTheRetention(string orm)
    {
        await OpenAsync(orm);
        var note = await StoredNoteAsync("Deleted by staff", home);
        Assert.IsTrue((await service.Notes.DeleteExtendedAsync(note.NoteId)).IsValid);
        Assert.IsTrue((await service.Repository.Users.AddAsync(new UserDto
        {
            UserId = Guid.NewGuid(), UserName = "staff", EMail = "staff@knote.tests", FullName = "Staff", RoleDefinition = "Staff"
        })).IsValid);
        service.UserIdentityName = "staff";
        service.EnforceAuthorization = true;

        Assert.IsTrue((await service.History.RemoveDeletedNoteAsync(note.NoteId)).NotAuthorized);
        Assert.IsTrue((await service.History.SaveRetentionAsync(new HistoryRetentionDto())).NotAuthorized);
        Assert.IsTrue((await service.History.GetDeletedNotesAsync()).IsValid);
        var recovered = await service.History.RecoverNoteAsync(note.NoteId);
        Assert.IsTrue(recovered.IsValid, recovered.ErrorMessage);
    }

    private async Task<NoteDto> StoredNoteAsync(string topic, FolderDto folder, Guid? type = null, params NoteKAttributeDto[] attributes)
    {
        var note = new NoteDto
        {
            NoteId = Guid.NewGuid(), Topic = topic, Description = "Text of the note", Tags = "", FolderId = folder.FolderId, FolderDto = folder,
            NoteTypeId = type, ContentType = "markdown", CreationDateTime = new DateTime(2026, 1, 1), ModificationDateTime = new DateTime(2026, 1, 1)
        };
        foreach (var attribute in attributes)
        {
            attribute.NoteId = note.NoteId;
            note.KAttributesDto.Add(attribute);
        }
        var saved = await service.Notes.SaveAsync(note, false);
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        return saved.Entity;
    }
}
