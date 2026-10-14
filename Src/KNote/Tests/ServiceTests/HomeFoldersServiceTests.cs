using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The folders the Web's Home shows beside Home's notes, and their notes (readable without signing in):
/// only the folders directly inside Home, by Order and Name, and their notes with Home's own criteria
/// (priority and topic, 25 at most). Any other folder is refused, so that public page can't read it.
/// </summary>
[TestClass]
public class HomeFoldersServiceTests
{
    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task GetHomeChildren_AreTheFoldersDirectlyInsideHome_ByOrderAndName(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository(orm));
        var homeId = (await service.Folders.GetHomeAsync()).Entity.FolderId;

        var second = await AddFolderAsync(service, "B second", homeId, order: 2);
        var first = await AddFolderAsync(service, "Z first", homeId, order: 1);
        var sameOrderA = await AddFolderAsync(service, "A same order", homeId, order: 2);
        await AddFolderAsync(service, "Grandchild", second, order: 0);
        await AddFolderAsync(service, "Outside Home", parentId: null, order: 0);

        var res = await service.Folders.GetHomeChildrenAsync();

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        CollectionAssert.AreEqual(new[] { first, sameOrderA, second }, res.Entity.Select(f => f.FolderId).ToArray());
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task HomeNotes_OfAFolderInsideHome_ByPriorityAndTopic_25AtMost(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository(orm));
        var homeId = (await service.Folders.GetHomeAsync()).Entity.FolderId;
        var folder = await AddFolderAsync(service, "Projects", homeId, order: 1);

        for (var i = 0; i < 30; i++)
            await AddNoteAsync(service, $"Note {i:00}", folder, priority: 30 - i);
        await AddNoteAsync(service, "B same priority", folder, priority: 1);
        await AddNoteAsync(service, "A same priority", folder, priority: 1);

        var res = await service.Notes.HomeNotesAsync(folder);

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        Assert.AreEqual(25, res.Entity.Count);
        CollectionAssert.AreEqual(new[] { "A same priority", "B same priority", "Note 29", "Note 28" },
            res.Entity.Take(4).Select(n => n.Topic).ToArray());
        Assert.IsTrue(res.Entity.All(n => n.FolderId == folder));
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task HomeNotes_OfHomeItself_AreHomeNotes(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository(orm));
        var homeId = (await service.Folders.GetHomeAsync()).Entity.FolderId;

        var byId = await service.Notes.HomeNotesAsync(homeId);
        var home = await service.Notes.HomeNotesAsync();

        Assert.IsTrue(byId.IsValid, byId.ErrorMessage);
        CollectionAssert.AreEqual(home.Entity.Select(n => n.NoteId).ToArray(), byId.Entity.Select(n => n.NoteId).ToArray());
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task HomeNotes_OfAnyOtherFolder_AreRefused(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository(orm));
        var homeId = (await service.Folders.GetHomeAsync()).Entity.FolderId;
        var child = await AddFolderAsync(service, "Projects", homeId, order: 1);
        var grandchild = await AddFolderAsync(service, "Private", child, order: 1);
        var outside = await AddFolderAsync(service, "Outside Home", parentId: null, order: 1);
        await AddNoteAsync(service, "Private note", grandchild, priority: 1);
        await AddNoteAsync(service, "Outside note", outside, priority: 1);

        foreach (var folderId in new[] { grandchild, outside, Guid.NewGuid() })
        {
            var res = await service.Notes.HomeNotesAsync(folderId);

            Assert.IsFalse(res.IsValid);
            Assert.IsNull(res.Entity);
        }
    }

    private static async Task<Guid> AddFolderAsync(KntService service, string name, Guid? parentId, int order)
    {
        var res = await service.Repository.Folders.AddAsync(new FolderDto
        {
            FolderId = Guid.NewGuid(), FolderNumber = 0, Name = name, ParentId = parentId, Order = order
        });
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        return res.Entity.FolderId;
    }

    private static async Task AddNoteAsync(KntService service, string topic, Guid folderId, int priority)
    {
        var res = await service.Repository.Notes.AddAsync(new NoteDto
        {
            NoteId = Guid.NewGuid(),
            Topic = topic,
            FolderId = folderId,
            Priority = priority,
            CreationDateTime = DateTime.Now,
            ModificationDateTime = DateTime.Now
        });
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
    }
}
