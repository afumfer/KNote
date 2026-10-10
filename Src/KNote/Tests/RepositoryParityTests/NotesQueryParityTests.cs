using System.Linq;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Tests.Helpers;

namespace KNote.Tests.RepositoryParityTests;

/// <summary>
/// Behavioral parity: the same query is run against a repository built for ONE ORM at a time
/// (each against its own freshly provisioned database), and the SAME precise assertions are
/// checked - the assertions themselves are the contract both Dapper's hand-written SQL and EF's
/// LINQ translation must satisfy identically. See CrudRoundTripParityTests for the complementary
/// cross-ORM read/write technique.
///
/// This targets NotesController.Filter/Search/HomeNotes specifically because they are the
/// highest-risk area for silent drift: Dapper builds SQL by hand (Repository.Dapper/
/// KntNoteRepository.cs, GetWhereFilterNotesInfoDto/GetFilterPrivateAsync) while EF translates
/// LINQ (Repository.EntityFramework/KntNoteRepository.cs, GetFilterPrivateAsync) - two independent
/// implementations of the same pagination/ordering/matching contract.
/// </summary>
[TestClass]
public class NotesQueryParityTests
{
    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task HomeNotes_ReturnsSeededHomeFolderNotes_OrderedByPriorityThenTopic(string orm)
    {
        using var db = new RepositoryTestDatabase();
        using var repo = db.CreateRepository(orm);

        var res = await repo.Notes.HomeNotesAsync();

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        // ModelBuilderExtensions.Seed() seeds exactly 2 notes in the Home folder: priority 90
        // ("... documentation") and priority 100 ("Wellcome to KNote").
        Assert.AreEqual(2, res.Entity.Count);
        Assert.IsTrue(res.Entity[0].Priority < res.Entity[1].Priority);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task GetFilterMinimalAsync_MatchesTopicCaseInsensitively(string orm)
    {
        using var db = new RepositoryTestDatabase();
        using var repo = db.CreateRepository(orm);

        var folderRes = await repo.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Filter Parity Folder" });
        Assert.IsTrue(folderRes.IsValid, folderRes.ErrorMessage);

        string uniqueTopic = $"PARITY_FILTER_{Guid.NewGuid():N}";
        var noteRes = await repo.Notes.AddAsync(new NoteDto
        {
            NoteId = Guid.NewGuid(),
            Topic = uniqueTopic,
            FolderId = folderRes.Entity.FolderId,
            CreationDateTime = DateTime.Now,
            ModificationDateTime = DateTime.Now
        });
        Assert.IsTrue(noteRes.IsValid, noteRes.ErrorMessage);

        // Both Dapper (SQL LIKE, case-insensitive by default under Sqlite) and EF
        // (.ToLower().Contains(...)) are expected to match regardless of case.
        var filter = new NotesFilterDto { Topic = uniqueTopic.ToLowerInvariant(), PageIdentifier = new PageIdentifier() };
        var filterRes = await repo.Notes.GetFilterMinimalAsync(filter);

        Assert.IsTrue(filterRes.IsValid, filterRes.ErrorMessage);
        Assert.IsTrue(filterRes.Entity.Any(n => n.NoteId == noteRes.Entity.NoteId));
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task GetFilterMinimalAsync_RespectsPageSize(string orm)
    {
        using var db = new RepositoryTestDatabase();
        using var repo = db.CreateRepository(orm);

        var folderRes = await repo.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Pagination Parity Folder" });
        Assert.IsTrue(folderRes.IsValid, folderRes.ErrorMessage);

        string sharedTag = $"PARITY_PAGE_{Guid.NewGuid():N}";
        for (int i = 0; i < 3; i++)
        {
            var addRes = await repo.Notes.AddAsync(new NoteDto
            {
                NoteId = Guid.NewGuid(),
                Topic = $"{sharedTag}_{i}",
                FolderId = folderRes.Entity.FolderId,
                CreationDateTime = DateTime.Now,
                ModificationDateTime = DateTime.Now
            });
            Assert.IsTrue(addRes.IsValid, addRes.ErrorMessage);
        }

        var filter = new NotesFilterDto { Topic = sharedTag, PageIdentifier = new PageIdentifier { PageNumber = 1, PageSize = 2 } };
        var filterRes = await repo.Notes.GetFilterMinimalAsync(filter);

        Assert.IsTrue(filterRes.IsValid, filterRes.ErrorMessage);
        Assert.AreEqual(2, filterRes.Entity.Count);
        Assert.AreEqual(3, filterRes.TotalCount);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task GetFilterMinimalAsync_MatchesTopicIgnoringAccents(string orm)
    {
        using var db = new RepositoryTestDatabase();
        using var repo = db.CreateRepository(orm);

        var folderRes = await repo.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Accent Filter Parity Folder" });
        Assert.IsTrue(folderRes.IsValid, folderRes.ErrorMessage);

        string uniqueToken = $"PARITYACCENT{Guid.NewGuid():N}";
        var noteRes = await repo.Notes.AddAsync(new NoteDto
        {
            NoteId = Guid.NewGuid(),
            Topic = $"José {uniqueToken}",
            FolderId = folderRes.Entity.FolderId,
            CreationDateTime = DateTime.Now,
            ModificationDateTime = DateTime.Now
        });
        Assert.IsTrue(noteRes.IsValid, noteRes.ErrorMessage);

        // Search without the accent must still match "José" - accent-insensitive search
        // (SQLite: overridden "like" function; SQL Server: COLLATE ..._AI in production).
        var filter = new NotesFilterDto { Topic = $"jose {uniqueToken}", PageIdentifier = new PageIdentifier() };
        var filterRes = await repo.Notes.GetFilterMinimalAsync(filter);

        Assert.IsTrue(filterRes.IsValid, filterRes.ErrorMessage);
        Assert.IsTrue(filterRes.Entity.Any(n => n.NoteId == noteRes.Entity.NoteId));
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task GetSearchMinimalAsync_FindsNoteByTopicTokenIgnoringAccents(string orm)
    {
        using var db = new RepositoryTestDatabase();
        using var repo = db.CreateRepository(orm);

        var folderRes = await repo.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Accent Search Parity Folder" });
        Assert.IsTrue(folderRes.IsValid, folderRes.ErrorMessage);

        string uniqueToken = $"PARITYSEARCHACCENT{Guid.NewGuid():N}";
        var noteRes = await repo.Notes.AddAsync(new NoteDto
        {
            NoteId = Guid.NewGuid(),
            Topic = $"Café {uniqueToken}",
            FolderId = folderRes.Entity.FolderId,
            CreationDateTime = DateTime.Now,
            ModificationDateTime = DateTime.Now
        });
        Assert.IsTrue(noteRes.IsValid, noteRes.ErrorMessage);

        var searchRes = await repo.Notes.GetSearchMinimalAsync(new NotesSearchDto { TextSearch = $"cafe {uniqueToken}" });

        Assert.IsTrue(searchRes.IsValid, searchRes.ErrorMessage);
        Assert.IsTrue(searchRes.Entity.Any(n => n.NoteId == noteRes.Entity.NoteId));
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task GetSearchMinimalAsync_FindsNoteByTopicToken(string orm)
    {
        using var db = new RepositoryTestDatabase();
        using var repo = db.CreateRepository(orm);

        var folderRes = await repo.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Search Parity Folder" });
        Assert.IsTrue(folderRes.IsValid, folderRes.ErrorMessage);

        string uniqueToken = $"PARITYSEARCH{Guid.NewGuid():N}";
        var noteRes = await repo.Notes.AddAsync(new NoteDto
        {
            NoteId = Guid.NewGuid(),
            Topic = $"Note about {uniqueToken}",
            FolderId = folderRes.Entity.FolderId,
            CreationDateTime = DateTime.Now,
            ModificationDateTime = DateTime.Now
        });
        Assert.IsTrue(noteRes.IsValid, noteRes.ErrorMessage);

        var searchRes = await repo.Notes.GetSearchMinimalAsync(new NotesSearchDto { TextSearch = uniqueToken });

        Assert.IsTrue(searchRes.IsValid, searchRes.ErrorMessage);
        Assert.IsTrue(searchRes.Entity.Any(n => n.NoteId == noteRes.Entity.NoteId));
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task GetFilterMinimalAsync_TaskUserId_MatchesOnlyNotesWithATaskOfThatUser(string orm)
    {
        using var db = new RepositoryTestDatabase();
        using var repo = db.CreateRepository(orm);

        // Seeded users (ModelBuilderExtensions.Seed()).
        var userA = (await repo.Users.GetByUserNameAsync("adminKNote")).Entity;
        var userB = (await repo.Users.GetByUserNameAsync("user1")).Entity;
        Assert.IsNotNull(userA);
        Assert.IsNotNull(userB);

        var folderRes = await repo.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Task User Parity Folder" });
        Assert.IsTrue(folderRes.IsValid, folderRes.ErrorMessage);
        var folderId = folderRes.Entity.FolderId;

        var noteOfA = await AddNoteAsync(repo, folderId, "Note with a task of A");
        var noteOfB = await AddNoteAsync(repo, folderId, "Note with a task of B");
        var noteOfBoth = await AddNoteAsync(repo, folderId, "Note with tasks of A and B");
        await AddNoteAsync(repo, folderId, "Note without tasks");
        await AddTaskAsync(repo, noteOfA, userA.UserId);
        await AddTaskAsync(repo, noteOfB, userB.UserId);
        await AddTaskAsync(repo, noteOfBoth, userA.UserId);
        await AddTaskAsync(repo, noteOfBoth, userB.UserId);

        var filterRes = await repo.Notes.GetFilterMinimalAsync(new NotesFilterDto { FolderId = folderId, TaskUserId = userA.UserId });

        Assert.IsTrue(filterRes.IsValid, filterRes.ErrorMessage);
        CollectionAssert.AreEquivalent(new[] { noteOfA, noteOfBoth }, filterRes.Entity.Select(n => n.NoteId).ToList());
        Assert.AreEqual(2, filterRes.TotalCount);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task GetSearchMinimalAsync_ExcludeNoteTypeId_LeavesOutOnlyThatType(string orm)
    {
        using var db = new RepositoryTestDatabase();
        using var repo = db.CreateRepository(orm);

        var excludedType = await repo.NoteTypes.AddAsync(new NoteTypeDto { NoteTypeId = Guid.NewGuid(), Name = $"Excluded {Guid.NewGuid():N}" });
        var otherType = await repo.NoteTypes.AddAsync(new NoteTypeDto { NoteTypeId = Guid.NewGuid(), Name = $"Other {Guid.NewGuid():N}" });
        Assert.IsTrue(excludedType.IsValid, excludedType.ErrorMessage);
        Assert.IsTrue(otherType.IsValid, otherType.ErrorMessage);

        var folderRes = await repo.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Exclude Type Parity Folder" });
        Assert.IsTrue(folderRes.IsValid, folderRes.ErrorMessage);
        var folderId = folderRes.Entity.FolderId;

        string uniqueToken = $"PARITYEXCLUDE{Guid.NewGuid():N}";
        var excluded = await AddNoteAsync(repo, folderId, $"{uniqueToken} excluded", excludedType.Entity.NoteTypeId);
        var ofOtherType = await AddNoteAsync(repo, folderId, $"{uniqueToken} other type", otherType.Entity.NoteTypeId);
        var withoutType = await AddNoteAsync(repo, folderId, $"{uniqueToken} without type");

        var searchRes = await repo.Notes.GetSearchMinimalAsync(new NotesSearchDto
        {
            TextSearch = uniqueToken,
            ExcludeNoteTypeId = excludedType.Entity.NoteTypeId
        });

        Assert.IsTrue(searchRes.IsValid, searchRes.ErrorMessage);
        CollectionAssert.AreEquivalent(new[] { ofOtherType, withoutType }, searchRes.Entity.Select(n => n.NoteId).ToList());

        // Also when looking a note up by its number.
        var excludedNumber = (await repo.Notes.GetAsync(excluded)).Entity.NoteNumber;
        var byNumberRes = await repo.Notes.GetSearchMinimalAsync(new NotesSearchDto
        {
            TextSearch = $"#{excludedNumber}",
            ExcludeNoteTypeId = excludedType.Entity.NoteTypeId
        });
        Assert.IsTrue(byNumberRes.IsValid, byNumberRes.ErrorMessage);
        Assert.AreEqual(0, byNumberRes.Entity.Count);
    }

    private static async Task<Guid> AddNoteAsync(KNote.Repository.IKntRepository repo, Guid folderId, string topic, Guid? noteTypeId = null)
    {
        var noteRes = await repo.Notes.AddAsync(new NoteDto
        {
            NoteId = Guid.NewGuid(),
            Topic = topic,
            FolderId = folderId,
            NoteTypeId = noteTypeId,
            CreationDateTime = DateTime.Now,
            ModificationDateTime = DateTime.Now
        });
        Assert.IsTrue(noteRes.IsValid, noteRes.ErrorMessage);
        return noteRes.Entity.NoteId;
    }

    private static async Task AddTaskAsync(KNote.Repository.IKntRepository repo, Guid noteId, Guid userId)
    {
        var taskRes = await repo.Notes.AddNoteTaskAsync(new NoteTaskDto
        {
            NoteTaskId = Guid.NewGuid(),
            NoteId = noteId,
            UserId = userId,
            Description = "Parity task",
            Tags = ""
        });
        Assert.IsTrue(taskRes.IsValid, taskRes.ErrorMessage);
    }
}
