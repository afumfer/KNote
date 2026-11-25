using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;
using Microsoft.Data.Sqlite;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The history of the structure of a repository, H.2b (03-Historial.md): folders, note types, attributes and their values,
/// types of relation, users and settings leave their row (EntityName = the entity), one per session; never a password, nor
/// the email of a user nor the value of a generic setting (H15); what KNote creates by itself has origin KNote (H16); the order
/// of the notes of a folder is not noted (H17). An attribute is noted in its transaction (if it can't be, it isn't saved); the
/// rest just after (if it can't be, the change stays). Dapper and EF.
/// </summary>
[TestClass]
public class StructureHistoryServiceTests
{
    private const string Password = "Retro-Computers-2026";

    private RepositoryTestDatabase db = null!;
    private KntService service = null!;
    private Guid homeId;

    private async Task OpenAsync(string orm)
    {
        db = new RepositoryTestDatabase();
        service = new KntService(db.CreateRepository(orm), activateMessageBroker: false)
        {
            UserIdentityName = "armando",
            HistoryOrigin = EnumHistoryOrigin.Desktop
        };
        var home = await service.Repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Equipment" });
        homeId = home.Entity.FolderId;
        var user = await service.Repository.Users.AddAsync(new UserDto
        {
            UserId = Guid.NewGuid(), UserName = "armando", EMail = "armando@test.local", FullName = "Armando", RoleDefinition = "Admin"
        });
        Assert.IsTrue(user.IsValid, user.ErrorMessage);
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
    public async Task AFolder_IsOneRow_NeverOnePerNote(string orm)
    {
        await OpenAsync(orm);
        var other = await SaveFolderAsync(new FolderDto { Name = "Archive", ParentId = homeId });
        var folder = await SaveFolderAsync(new FolderDto { Name = "Restored", ParentId = homeId });
        var note = new NoteDto { NoteId = Guid.NewGuid(), Topic = "CPC", Description = "x", FolderId = folder.FolderId, ContentType = "markdown" };
        Assert.IsTrue((await service.Repository.Notes.AddAsync(note)).IsValid);

        service.UserIdentityName = "marta";
        folder.Name = "Repaired";
        folder.ParentId = other.FolderId;
        await SaveFolderAsync(folder);
        await service.Folders.UpdateOrderNotesAsync(folder.FolderId, note.NoteId.ToString()); // presentation: no row (H17)

        var rows = await RowsAsync(HistoryEntities.Folder, folder.FolderId);
        Assert.HasCount(2, rows);
        Assert.AreEqual("Folder created in Equipment", rows.Single(r => r.UserName == "armando").RegistryMessage);
        var change = rows.Single(r => r.UserName == "marta");
        Assert.AreEqual(@"Renamed: Restored → Repaired · Moved from Equipment to Equipment\Archive", change.RegistryMessage);
        Assert.AreEqual(@"Equipment\Archive\Repaired", change.EntityTitle);
        Assert.IsEmpty(await RowsAsync(HistoryEntities.Note, note.NoteId), "its notes' paths changed: not noted one by one");

        Assert.IsTrue((await service.Repository.Notes.DeleteAsync(note.NoteId)).IsValid);
        Assert.IsTrue((await service.Folders.DeleteAsync(folder.FolderId)).IsValid);
        Assert.AreEqual("Folder deleted", (await RowsAsync(HistoryEntities.Folder, folder.FolderId)).Single(r => r.ActionType == "Deleted").RegistryMessage);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AnAttribute_WithItsValues_AndAllOfThemRemoved(string orm)
    {
        await OpenAsync(orm);
        var attribute = await SaveAttributeAsync(new KAttributeDto
        {
            Name = "Status", KAttributeDataType = EnumKAttributeDataType.TabulatedValue,
            KAttributeValues = new() { new KAttributeTabulatedValueDto { Value = "Restoring", Order = 1 }, new KAttributeTabulatedValueDto { Value = "Works", Order = 2 } }
        });
        Assert.AreEqual("Attribute created (TabulatedValue) · Values: +Restoring, +Works",
            (await RowsAsync(HistoryEntities.KAttribute, attribute.KAttributeId)).Single().RegistryMessage);

        service.UserIdentityName = "marta";
        var stored = (await service.KAttributes.GetAsync(attribute.KAttributeId)).Entity;
        stored.KAttributeValues.Clear();
        await SaveAttributeAsync(stored);

        Assert.IsEmpty((await service.Repository.KAttributes.GetKAttributeTabulatedValuesAsync(attribute.KAttributeId)).Entity,
            "emptying the list deletes its values");
        Assert.AreEqual("Values: −Restoring, −Works",
            (await RowsAsync(HistoryEntities.KAttribute, attribute.KAttributeId)).Single(r => r.UserName == "marta").RegistryMessage);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AnAttribute_ThatCantBeNoted_IsNotSaved_ButAFolderIs(string orm)
    {
        await OpenAsync(orm);
        NoHistory();

        try
        {
            var saved = await service.KAttributes.SaveAsync(new KAttributeDto
            {
                Name = "Not saved", KAttributeDataType = EnumKAttributeDataType.TabulatedValue,
                KAttributeValues = new() { new KAttributeTabulatedValueDto { Value = "X" } }
            });
            Assert.IsFalse(saved.IsValid);
        }
        catch (Exception)
        {
            // Failed: what matters is that it isn't there.
        }
        var folder = await service.Folders.SaveAsync(new FolderDto { Name = "Saved anyway", ParentId = homeId });

        Assert.IsFalse((await service.Repository.KAttributes.GetAllAsync()).Entity.Any(a => a.Name == "Not saved"));
        Assert.IsTrue(folder.IsValid, "the structure without a transaction of its own is noted just after: its change stays (H10)");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AUser_NeverLeavesItsPasswordNorItsEmail(string orm)
    {
        await OpenAsync(orm);
        var created = await service.Users.CreateAsync(new UserRegisterDto
        {
            UserName = "mlopez", EMail = "marta@old.test", FullName = "Marta", RoleDefinition = "Staff", Password = Password
        });
        Assert.IsTrue(created.IsValid, created.ErrorMessage);
        var userId = created.Entity.UserId;
        Assert.IsTrue((await service.Users.SetPasswordAsync(userId, Password + "-2")).IsValid);

        service.UserIdentityName = "mlopez";
        var profile = await service.Users.SaveProfileAsync(new UserProfileDto { EMail = "marta@new.test", FullName = "Marta López", CurrentPassword = Password + "-2" });
        Assert.IsTrue(profile.IsValid, profile.ErrorMessage);
        Assert.IsTrue((await service.Users.ChangePasswordAsync(new UserChangePasswordDto { CurrentPassword = Password + "-2", NewPassword = Password + "-3" })).IsValid);

        var rows = await RowsAsync(HistoryEntities.User, userId);
        CollectionAssert.AreEquivalent(new[] { "User created (Staff) · Password set", "Full name: Marta → Marta López · Email changed · Password changed" },
            rows.Select(r => r.RegistryMessage).ToArray());
        foreach (var row in rows)
            foreach (var text in new[] { row.RegistryMessage, row.ChangeDetails, row.EntityTitle })
            {
                StringAssert.DoesNotMatch(text, new System.Text.RegularExpressions.Regex("Retro-Computers|marta@"), text);
            }
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task WhoRegisters_IsTheNewUser(string orm)
    {
        await OpenAsync(orm);
        service.UserIdentityName = null;

        var registered = await service.Users.RegisterAsync(new UserRegisterDto { UserName = "jperez", EMail = "j@test.local", FullName = "J", Password = Password });
        Assert.IsTrue(registered.IsValid, registered.ErrorMessage);

        var row = (await RowsAsync(HistoryEntities.User, registered.Entity.UserId)).Single();
        Assert.AreEqual("jperez", row.UserName);
        StringAssert.StartsWith(row.RegistryMessage, "User created (");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task TheSettings_TheNameWithItsValues_AGenericOneWithout(string orm)
    {
        await OpenAsync(orm);
        var before = (await service.SystemValues.GetRepositoryInfoAsync()).Entity?.Name;

        Assert.IsTrue((await service.SystemValues.SaveRepositoryNameAsync("Retro computers")).IsValid);
        var generic = await service.SystemValues.SaveAsync(new SystemValueDto { Scope = "MINE", Key = "TOKEN", Value = "very-secret-value" });
        Assert.IsTrue(generic.IsValid, generic.ErrorMessage);

        var name = (await RowsAsync(HistoryEntities.SystemValue, HistoryRules.SettingId(KntConst.SystemScope, KntConst.RepositoryNameKey))).Single();
        Assert.AreEqual(KntHistory.RepositoryNameTitle, name.EntityTitle);
        Assert.AreEqual(before == null ? "Setting created" : $"Value: {before} → Retro computers", name.RegistryMessage);
        var secret = (await RowsAsync(HistoryEntities.SystemValue, generic.Entity.SystemValueId)).Single();
        Assert.AreEqual("MINE/TOKEN", secret.EntityTitle);
        Assert.IsFalse((secret.ChangeDetails + secret.RegistryMessage).Contains("very-secret"), secret.ChangeDetails);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task WhatKNoteCreatesByItself_HasOriginKNote(string orm)
    {
        await OpenAsync(orm);

        var profile = await service.AtlasContext.CreateProfileAsync();
        Assert.IsTrue(profile.IsValid, profile.ErrorMessage);

        var atlas = (await service.Repository.Folders.GetAllAsync()).Entity.Single(f => f.Name == KntConst.AtlasFolderName);
        var row = (await RowsAsync(HistoryEntities.Folder, atlas.FolderId)).Single();
        Assert.AreEqual("KNote", row.Origin);
        Assert.AreEqual("armando", row.UserName, "for whom it was done");
    }

    private void NoHistory()
    {
        SqliteConnection.ClearAllPools();
        using var connection = new SqliteConnection($"Data Source={db.DatabaseFilePath}");
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "CREATE TRIGGER NoHistory BEFORE INSERT ON KLogs BEGIN SELECT RAISE(ABORT, 'no history'); END;";
        cmd.ExecuteNonQuery();
    }

    private async Task<FolderDto> SaveFolderAsync(FolderDto folder)
    {
        var saved = await service.Folders.SaveAsync(folder);
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        return (await service.Repository.Folders.GetAsync(saved.Entity.FolderId)).Entity;
    }

    private async Task<KAttributeDto> SaveAttributeAsync(KAttributeDto attribute)
    {
        var saved = await service.KAttributes.SaveAsync(attribute);
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        return saved.Entity;
    }

    private async Task<List<KLogDto>> RowsAsync(string entityName, Guid entityId) =>
        (await service.Repository.History.GetEntityHistoryAsync(entityName, entityId)).Entity;
}
