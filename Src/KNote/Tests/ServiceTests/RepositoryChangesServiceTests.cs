using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// «Repository changes» (H.5a, H28, H29): the rows of a period, the newest first and in pages, with their filters; the changes of
/// the structure only for ProjectManager and up (checked by the command with the user's role); which notes of the page exist; and
/// the users of the period. Dapper and EF.
/// </summary>
[TestClass]
public class RepositoryChangesServiceTests
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
        foreach (var (name, role) in new[] { ("armando", "Admin"), ("staff", "Staff"), ("pm", "ProjectManager") })
            Assert.IsTrue((await service.Repository.Users.AddAsync(new UserDto
            {
                UserId = Guid.NewGuid(), UserName = name, EMail = $"{name}@knote.tests", FullName = name, RoleDefinition = role
            })).IsValid);
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
    public async Task TheChangesOfToday_TheNewestFirst_InPages(string orm)
    {
        await OpenAsync(orm);
        var first = await StoredNoteAsync("First", "armando");
        var second = await StoredNoteAsync("Second", "marta");
        var third = await StoredNoteAsync("Third", "armando");

        var all = (await service.History.GetRepositoryChangesAsync(new RepositoryChangesQueryDto())).Entity;
        var page = (await service.History.GetRepositoryChangesAsync(new RepositoryChangesQueryDto { Skip = 1, Take = 1 })).Entity;
        var past = (await service.History.GetRepositoryChangesAsync(new RepositoryChangesQueryDto
        {
            From = DateTime.Today.AddDays(-10), To = DateTime.Today.AddDays(-1)
        })).Entity;

        CollectionAssert.AreEqual(new[] { third.NoteId, second.NoteId, first.NoteId }, all.Rows.Select(r => r.EntityId).ToArray());
        Assert.IsTrue(all.StructureIncluded, "armando is an Admin");
        Assert.AreEqual(second.NoteId, page.Rows.Single().EntityId);
        Assert.AreEqual(3, page.Total);
        Assert.IsTrue(page.HasMore);
        Assert.AreEqual(0, past.Total, "nothing before today");
        Assert.IsTrue(all.Rows.All(all.NoteExists));
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task TheFilters_ByWhatActionUserAndOrigin(string orm)
    {
        await OpenAsync(orm);
        var note = await StoredNoteAsync("To delete", "armando");
        await StoredNoteAsync("By the assistant", "marta", EnumHistoryOrigin.Assistant);
        service.UserIdentityName = "armando";
        Assert.IsTrue((await service.Notes.DeleteExtendedAsync(note.NoteId)).IsValid);

        var deleted = (await service.History.GetRepositoryChangesAsync(new RepositoryChangesQueryDto { ActionType = "Deleted" })).Entity;
        var byMarta = (await service.History.GetRepositoryChangesAsync(new RepositoryChangesQueryDto { UserName = "marta" })).Entity;
        var assistant = (await service.History.GetRepositoryChangesAsync(new RepositoryChangesQueryDto { Origin = "Assistant" })).Entity;
        var users = (await service.History.GetRepositoryChangesUsersAsync(new RepositoryChangesQueryDto())).Entity;

        var deletedRow = deleted.Rows.Single();
        Assert.AreEqual(note.NoteId, deletedRow.EntityId);
        Assert.IsFalse(deleted.NoteExists(deletedRow), "no «Open note»: it is recovered from «Recover deleted notes»");
        Assert.AreEqual("By the assistant", HistoryRulesTopic(byMarta.Rows.Single()));
        Assert.AreEqual("marta", assistant.Rows.Single().UserName);
        CollectionAssert.AreEqual(new[] { "armando", "marta" }, users);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task TheStructure_IsOnlyForAProjectManagerAndUp(string orm)
    {
        await OpenAsync(orm);
        await StoredNoteAsync("A note", "armando");
        service.UserIdentityName = "armando";
        var folder = await service.Folders.SaveAsync(new FolderDto { FolderId = Guid.Empty, Name = "Projects", ParentId = home.FolderId });
        Assert.IsTrue(folder.IsValid, folder.ErrorMessage);

        service.UserIdentityName = "staff";
        service.ResetCurrentUser();
        var forStaff = (await service.History.GetRepositoryChangesAsync(new RepositoryChangesQueryDto())).Entity;
        var foldersForStaff = (await service.History.GetRepositoryChangesAsync(new RepositoryChangesQueryDto { EntityName = HistoryEntities.Folder })).Entity;

        service.UserIdentityName = "pm";
        service.ResetCurrentUser();
        var forPm = (await service.History.GetRepositoryChangesAsync(new RepositoryChangesQueryDto())).Entity;
        var foldersForPm = (await service.History.GetRepositoryChangesAsync(new RepositoryChangesQueryDto { EntityName = HistoryEntities.Folder })).Entity;

        Assert.IsFalse(forStaff.StructureIncluded);
        Assert.IsTrue(forStaff.Rows.All(r => r.EntityName == HistoryEntities.Note), "a Staff sees only the notes");
        Assert.AreEqual(0, foldersForStaff.Total);
        Assert.IsTrue(forPm.StructureIncluded);
        Assert.IsTrue(forPm.Rows.Any(r => r.EntityName == HistoryEntities.Folder));
        StringAssert.EndsWith(foldersForPm.Rows.Single().EntityTitle, "Projects", "the title of a folder is its path");
    }

    // The topic of a note's row: its title without the number.
    private static string HistoryRulesTopic(KLogDto row) => KNote.Model.Core.HistoryRules.NoteTopicOf(row.EntityTitle);

    private async Task<NoteDto> StoredNoteAsync(string topic, string user, EnumHistoryOrigin origin = EnumHistoryOrigin.Desktop)
    {
        service.UserIdentityName = user;
        service.HistoryOrigin = origin;
        var note = new NoteDto
        {
            NoteId = Guid.NewGuid(), Topic = topic, Description = "Text", Tags = "", FolderId = home.FolderId, FolderDto = home,
            ContentType = "markdown", CreationDateTime = DateTime.Now, ModificationDateTime = DateTime.Now
        };
        var saved = await service.Notes.SaveAsync(note, false);
        service.HistoryOrigin = EnumHistoryOrigin.Desktop;
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        return saved.Entity;
    }
}
