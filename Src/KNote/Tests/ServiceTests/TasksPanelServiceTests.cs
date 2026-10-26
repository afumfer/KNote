using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;
using Microsoft.Data.Sqlite;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The tasks panel of the web (Notes.GetTasksPanelAsync), in both ORMs: the tasks of a window by column, as
/// TaskPanelRules classifies them (the repositories' SQL is its version), their order, paging, filters, the tasks out of
/// the window, the nearest one when the window has none, and never the tasks of the system notes. Today is Saturday
/// 24 Oct 2026; the default window (3 months) goes from 1 Sep to 30 Nov.
/// </summary>
[TestClass]
public class TasksPanelServiceTests
{
    private static readonly DateTime Today = new(2026, 10, 24);
    private readonly List<RepositoryTestDatabase> _databases = new();

    [TestCleanup]
    public void Cleanup() => _databases.ForEach(db => db.Dispose());

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Panel_TheTasksOfTheWindowByColumn(string orm)
    {
        var data = await SeedAsync(orm);

        var res = await data.Service.Notes.GetTasksPanelAsync(Filter());

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        var panel = res.Entity;
        Assert.AreEqual(new DateTime(2026, 9, 1), panel.WindowStart);
        Assert.AreEqual(new DateTime(2026, 11, 30), panel.WindowEnd);
        Assert.AreEqual(Today, panel.CutOff);
        CollectionAssert.AreEqual(new[] { "t2" }, Topics(panel, EnumTaskPanelColumn.Resolved));
        CollectionAssert.AreEqual(new[] { "t8", "t6", "t7", "t1" }, Topics(panel, EnumTaskPanelColumn.InProgress), "Oldest first.");
        CollectionAssert.AreEqual(new[] { "t4" }, Topics(panel, EnumTaskPanelColumn.Pending));
        Assert.AreEqual(4, panel.Columns.Single(c => c.Column == EnumTaskPanelColumn.InProgress).TotalCount);
        Assert.AreEqual(2, panel.OutsideCount, "t3 and t5; never the task of the system note.");
        Assert.IsNull(panel.NearestTaskDate);

        var t1 = panel.Columns.SelectMany(c => c.Tasks).Single(t => t.Tags == "t1");
        Assert.AreEqual("Portal", t1.NoteTopic);
        Assert.AreEqual("Project", t1.NoteTypeName);
        Assert.AreEqual(data.Other.FullName, t1.UserFullName);
        AssertAsTheRules(panel);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Panel_PastWindow_TheStateOfThen(string orm)
    {
        var data = await SeedAsync(orm);

        var res = await data.Service.Notes.GetTasksPanelAsync(Filter(f => (f.Size, f.ReferenceDate) = (1, new DateTime(2026, 8, 15))));

        var panel = res.Entity;
        Assert.AreEqual(new DateTime(2026, 8, 31), panel.CutOff);
        CollectionAssert.AreEqual(new[] { "t3" }, Topics(panel, EnumTaskPanelColumn.Resolved));
        CollectionAssert.AreEqual(new[] { "t8", "t2" }, Topics(panel, EnumTaskPanelColumn.InProgress), "t2 was resolved later.");
        Assert.AreEqual(0, Topics(panel, EnumTaskPanelColumn.Pending).Length);
        AssertAsTheRules(panel);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Panel_Filters(string orm)
    {
        var data = await SeedAsync(orm);

        async Task<string[]> All(Action<TasksPanelFilterDto> change)
            => (await data.Service.Notes.GetTasksPanelAsync(Filter(change))).Entity.Columns.SelectMany(c => c.Tasks).Select(t => t.Tags).OrderBy(t => t).ToArray();

        CollectionAssert.AreEqual(new[] { "t1" }, await All(f => f.UserId = data.Other.UserId));
        CollectionAssert.AreEqual(new[] { "t1", "t4", "t6" }, await All(f => f.NoteTypeId = data.ProjectTypeId));
        CollectionAssert.AreEqual(new[] { "t1", "t2", "t4", "t6", "t7", "t8" }, await All(f => f.Tags = new() { "client", "legal" }));
        CollectionAssert.AreEqual(new[] { "t1", "t4", "t6" }, await All(f => f.Tags = new() { "urgent" }));
        CollectionAssert.AreEqual(new[] { "t1", "t4", "t6" }, await All(f => f.Text = "portal"));
        CollectionAssert.AreEqual(new[] { "t2", "t7", "t8" }, await All(f => f.Text = $"#{data.BillingNoteNumber}"));
        CollectionAssert.AreEqual(new[] { "t1", "t4", "t6" }, await All(f => (f.FolderId, f.IncludeChildFolders) = (data.ChildFolderId, false)));
        CollectionAssert.AreEqual(new[] { "t2", "t7", "t8" }, await All(f => (f.FolderId, f.IncludeChildFolders) = (data.RootFolderId, false)));
        Assert.AreEqual(6, (await All(f => (f.FolderId, f.IncludeChildFolders) = (data.RootFolderId, true))).Length);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Panel_LoadMore_TheNextPageOfAColumn(string orm)
    {
        var data = await SeedAsync(orm);

        var res = await data.Service.Notes.GetTasksPanelAsync(Filter(f => (f.Column, f.Skip, f.PageSize) = (EnumTaskPanelColumn.InProgress, 2, 2)));

        var column = res.Entity.Columns.Single();
        Assert.AreEqual(EnumTaskPanelColumn.InProgress, column.Column);
        Assert.AreEqual(4, column.TotalCount);
        CollectionAssert.AreEqual(new[] { "t7", "t1" }, column.Tasks.Select(t => t.Tags).ToArray());
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Panel_EmptyWindow_TheNearestTask(string orm)
    {
        var data = await SeedAsync(orm);

        var future = await data.Service.Notes.GetTasksPanelAsync(Filter(f => (f.Unit, f.Size, f.ReferenceDate) = (EnumTasksWindowUnit.Days, 1, new DateTime(2026, 12, 20))));
        var past = await data.Service.Notes.GetTasksPanelAsync(Filter(f => (f.Unit, f.Size, f.ReferenceDate) = (EnumTasksWindowUnit.Days, 1, new DateTime(2024, 6, 1))));

        Assert.IsTrue(future.Entity.Columns.All(c => c.TotalCount == 0));
        Assert.AreEqual(new DateTime(2026, 12, 5), future.Entity.NearestTaskDate, "The start of t5, the last one before.");
        Assert.AreEqual(8, future.Entity.OutsideCount);
        Assert.AreEqual(new DateTime(2025, 1, 1), past.Entity.NearestTaskDate, "The start of t8, the first one after.");
    }

    // Every task in its column on the cut-off date and in the window, as TaskPanelRules says.
    private static void AssertAsTheRules(TasksPanelDto panel)
    {
        foreach (var column in panel.Columns)
            foreach (var task in column.Tasks)
            {
                Assert.AreEqual(column.Column, task.Column);
                Assert.AreEqual(TaskPanelRules.Classify(task, panel.CutOff), task.Column, task.Tags);
                Assert.IsTrue(TaskPanelRules.Overlaps(task, panel.WindowStart, panel.WindowEnd, panel.Today), task.Tags);
            }
    }

    private static string[] Topics(TasksPanelDto panel, EnumTaskPanelColumn column)
        => panel.Columns.Single(c => c.Column == column).Tasks.Select(t => t.Tags).ToArray();

    private static TasksPanelFilterDto Filter(Action<TasksPanelFilterDto>? change = null)
    {
        var filter = new TasksPanelFilterDto { Today = Today, ReferenceDate = Today };
        change?.Invoke(filter);
        return filter;
    }

    private sealed record Data(KntService Service, UserDto User, UserDto Other, Guid ProjectTypeId, Guid RootFolderId, Guid ChildFolderId, int BillingNoteNumber);

    private async Task<Data> SeedAsync(string orm)
    {
        var db = new RepositoryTestDatabase();
        _databases.Add(db);
        var service = new KntService(db.CreateRepository(orm), activateMessageBroker: false);
        var (user, other) = (await NewUserAsync(service), await NewUserAsync(service));

        var projectType = await service.Repository.NoteTypes.AddAsync(new NoteTypeDto { NoteTypeId = Guid.NewGuid(), Name = "Project" });
        var systemType = await service.Repository.NoteTypes.AddAsync(new NoteTypeDto { NoteTypeId = Guid.NewGuid(), Name = KntConst.ChatSessionsTag });
        var root = await service.Repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Tasks root" });
        var child = await service.Repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Tasks child", ParentId = root.Entity.FolderId });

        var portal = await NewNoteAsync(service, "Portal", "urgent client", child.Entity.FolderId, projectType.Entity.NoteTypeId);
        var billing = await NewNoteAsync(service, "Billing", "legal", root.Entity.FolderId, null);
        var session = await NewNoteAsync(service, "AI session", "", root.Entity.FolderId, systemType.Entity.NoteTypeId);

        await AddTaskAsync(service, portal.NoteId, other.UserId, "t1", "2026-10-10");
        await AddTaskAsync(service, billing.NoteId, user.UserId, "t2", "2026-08-20", "2026-09-05");
        await AddTaskAsync(service, billing.NoteId, user.UserId, "t3", "2026-08-01", "2026-08-31");
        await AddTaskAsync(service, portal.NoteId, user.UserId, "t4", "2026-11-10");
        await AddTaskAsync(service, billing.NoteId, user.UserId, "t5", "2026-12-05");
        await AddTaskAsync(service, portal.NoteId, user.UserId, "t6", "2026-10-01", "2026-10-30");
        var t7 = await AddTaskAsync(service, billing.NoteId, user.UserId, "t7", null);
        await AddTaskAsync(service, billing.NoteId, user.UserId, "t8", "2025-01-01");
        await AddTaskAsync(service, session.NoteId, user.UserId, "system", "2026-10-05");
        SetTaskCreation(db, t7, new DateTime(2026, 10, 2, 11, 0, 0));

        return new Data(service, user, other, projectType.Entity.NoteTypeId, root.Entity.FolderId, child.Entity.FolderId, billing.NoteNumber);
    }

    private static async Task<UserDto> NewUserAsync(KntService service)
    {
        var id = Guid.NewGuid();
        var res = await service.Users.CreateAsync(new UserRegisterDto
        {
            UserName = $"tsk-{id:N}"[..24],
            EMail = $"{id:N}@knote.tests",
            FullName = $"Tasks user {id:N}"[..24],
            RoleDefinition = nameof(EnumRoles.Staff),
            Password = "Tasks-Password-1!"
        });
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        return res.Entity;
    }

    private static async Task<NoteDto> NewNoteAsync(KntService service, string topic, string tags, Guid folderId, Guid? noteTypeId)
    {
        var note = await service.Repository.Notes.AddAsync(new NoteDto
        {
            NoteId = Guid.NewGuid(), Topic = topic, Tags = tags, FolderId = folderId, NoteTypeId = noteTypeId,
            CreationDateTime = DateTime.Now, ModificationDateTime = DateTime.Now
        });
        Assert.IsTrue(note.IsValid, note.ErrorMessage);
        return note.Entity;
    }

    // The task's Tags (its topic) name it in the tests.
    private static async Task<Guid> AddTaskAsync(KntService service, Guid noteId, Guid userId, string name, string? start, string? end = null)
    {
        var task = new NoteTaskDto
        {
            NoteTaskId = Guid.NewGuid(), NoteId = noteId, UserId = userId, Tags = name, Description = $"Task {name}",
            StartDate = start == null ? null : DateTime.Parse(start).AddHours(9),
            EndDate = end == null ? null : DateTime.Parse(end).AddHours(18),
            Resolved = end != null
        };
        var res = await service.Repository.Notes.AddNoteTaskAsync(task);
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        return task.NoteTaskId;
    }

    // The repositories give a new task the current date; t7, without start, needs another one.
    private static void SetTaskCreation(RepositoryTestDatabase db, Guid noteTaskId, DateTime creation)
        => Exec(db, "UPDATE NoteTasks SET CreationDateTime = @value WHERE NoteTaskId = @id;", creation, noteTaskId.ToString().ToUpperInvariant());

    private static void Exec(RepositoryTestDatabase db, string sql, object value, string id)
    {
        using var connection = new SqliteConnection($"Data Source={db.DatabaseFilePath}");
        connection.Open();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        cmd.Parameters.AddWithValue("@value", value);
        cmd.Parameters.AddWithValue("@id", id);
        Assert.AreEqual(1, cmd.ExecuteNonQuery());
    }
}
