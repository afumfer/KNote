using System.Linq;
using System.Threading;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// Regression coverage for the first use of the features that keep their data as notes of a system note type (AI
/// assistant sessions, chat room threads) by several requests at once. KntSystemNotes used to check whether the note
/// type, its attributes and the folder existed and create them when missing, with nothing in between: concurrent
/// callers all saw "missing" and all created them. The note type and the attributes have unique indexes, so all but
/// one caller failed ("UNIQUE constraint failed"); the folder has none, so each caller created its own copy. On
/// Sqlite the concurrent writes also failed with "database is locked" (rollback journal mode).
///
/// Each caller has its own KntService and repository, as each request of the Server has (scoped services); Task.Run
/// and a shared start gate make their "check, then create" steps genuinely overlap, the same technique as
/// SaveSystemVariableConcurrencyTests.
/// </summary>
[TestClass]
public class SystemNotesConcurrencyTests
{
    private const int ConcurrentCalls = 20;
    // Each round on a new database: once warmed up, the calls can be quick enough not to overlap in a given round, so
    // a single round passed now and then even without the fix.
    private const int Rounds = 5;
    private const string UserName = "staff1";

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ConcurrentFirstUse_AiSessions_AllSucceed_AndTheInfrastructureIsCreatedOnce(string orm)
    {
        for (var round = 0; round < Rounds; round++)
        {
            using var db = new RepositoryTestDatabase();
            var services = await StaffServicesAsync(db, orm, ConcurrentCalls);

            var results = await RunAtOnceAsync(services.Select(s => Call(() => s.AiSessions.GetUserSessionsAsync())));

            AssertAllValid(results, round);
            await AssertCreatedOnceAsync(services[0], KntConst.ChatSessionsTag, KntConst.AiSessionsFolderName,
                KntConst.AiProviderAttributeName, KntConst.AiModelAttributeName);
        }
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ConcurrentFirstUse_ChatRooms_AllSucceed_AndTheInfrastructureIsCreatedOnce(string orm)
    {
        for (var round = 0; round < Rounds; round++)
        {
            using var db = new RepositoryTestDatabase();
            var services = await StaffServicesAsync(db, orm, ConcurrentCalls);

            var results = await RunAtOnceAsync(services.Select(s => Call(() => s.ChatRooms.GetThreadsAsync())));

            AssertAllValid(results, round);
            await AssertCreatedOnceAsync(services[0], KntConst.ChatRoomTag, KntConst.ChatRoomFolderName);
        }
    }

    // The web app's first page load: the AI assistant page and the chat room hub create their own infrastructure at
    // the same time (different note types and folders, but concurrent writes on the same database).
    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ConcurrentFirstUse_AiSessionsAndChatRooms_AllSucceed_AndEachInfrastructureIsCreatedOnce(string orm)
    {
        for (var round = 0; round < Rounds; round++)
        {
            using var db = new RepositoryTestDatabase();
            var services = await StaffServicesAsync(db, orm, ConcurrentCalls);

            var results = await RunAtOnceAsync(services.Select((s, i) => i % 2 == 0
                ? Call(() => s.AiSessions.GetUserSessionsAsync())
                : Call(() => s.ChatRooms.GetThreadsAsync())));

            AssertAllValid(results, round);
            await AssertCreatedOnceAsync(services[0], KntConst.ChatSessionsTag, KntConst.AiSessionsFolderName,
                KntConst.AiProviderAttributeName, KntConst.AiModelAttributeName);
            await AssertCreatedOnceAsync(services[0], KntConst.ChatRoomTag, KntConst.ChatRoomFolderName);
        }
    }

    // Several folders with the name (left by the old race): the oldest one is always used, so new notes don't spread
    // over the copies.
    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task DuplicatedFolders_TheOldestOneIsUsed(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = (await StaffServicesAsync(db, orm, 1))[0];
        // The newer copy goes first in the lists (lower Order, lower id), so the oldest one is not found by chance.
        var oldest = await AddFolderAsync(service, KntConst.AiSessionsFolderName, Guid.Parse("ffffffff-0000-0000-0000-000000000000"), 10);
        await AddFolderAsync(service, KntConst.AiSessionsFolderName, Guid.Parse("00000000-0000-0000-0000-000000000001"), 1);

        var res = await service.AiSessions.SaveAsync(new AiChatSessionDto
        {
            Provider = "OpenAI",
            Model = "gpt-test",
            Turns = { new AiChatTurnDto { Prompt = "Question", Answer = "Answer" } }
        });

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        var note = (await service.Repository.Notes.GetAsync(res.Entity.NoteId)).Entity;
        Assert.AreEqual(oldest.FolderId, note.FolderId);
    }

    private static async Task<List<KntService>> StaffServicesAsync(RepositoryTestDatabase db, string orm, int count)
    {
        var services = Enumerable.Range(0, count)
            .Select(_ => new KntService(db.CreateRepository(orm)) { UserIdentityName = UserName, EnforceAuthorization = true })
            .ToList();

        // Straight through the repository: commands would need an Admin to create users.
        var res = await services[0].Repository.Users.AddAsync(new UserDto
        {
            UserId = Guid.NewGuid(),
            UserName = UserName,
            EMail = $"{UserName}@knote.tests",
            FullName = UserName,
            RoleDefinition = nameof(EnumRoles.Staff)
        });
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        return services;
    }

    // A failed call, whether it returns an invalid result or throws (the Server answers both with a 400).
    private static Func<Task<ResultBase>> Call<TResult>(Func<Task<TResult>> call) where TResult : ResultBase =>
        async () =>
        {
            try
            {
                return await call();
            }
            catch (Exception ex)
            {
                var failed = new Result();
                failed.AddErrorMessage(ex.GetBaseException().Message);
                return failed;
            }
        };

    // The calls wait for the gate on thread-pool threads: the pool gets enough threads first, or only as many calls as
    // it has threads would start together, and the others once those finished.
    private static async Task<ResultBase[]> RunAtOnceAsync(IEnumerable<Func<Task<ResultBase>>> calls)
    {
        ThreadPool.GetMinThreads(out var workerThreads, out var ioThreads);
        ThreadPool.SetMinThreads(Math.Max(workerThreads, ConcurrentCalls * 2), ioThreads);
        try
        {
            using var startGate = new ManualResetEventSlim(false);
            var tasks = calls
                .Select(call => Task.Run(async () =>
                {
                    startGate.Wait();
                    return await call();
                }))
                .ToArray();

            startGate.Set();
            return await Task.WhenAll(tasks);
        }
        finally
        {
            ThreadPool.SetMinThreads(workerThreads, ioThreads);
        }
    }

    private static void AssertAllValid(ResultBase[] results, int round)
    {
        var errors = results.Where(r => !r.IsValid).Select(r => r.ErrorMessage).ToList();
        Assert.AreEqual(0, errors.Count,
            $"Round {round + 1}: {errors.Count} of {results.Length} calls failed: {string.Join(" | ", errors.Distinct())}");
    }

    private static async Task AssertCreatedOnceAsync(KntService service, string noteTypeName, string folderName,
        params string[] attributeNames)
    {
        var noteTypes = (await service.Repository.NoteTypes.GetAllAsync()).Entity.Where(t => t.Name == noteTypeName).ToList();
        Assert.AreEqual(1, noteTypes.Count, $"Note types {noteTypeName}");

        var attributes = (await service.Repository.KAttributes.GetAllAsync(noteTypes[0].NoteTypeId)).Entity
            .Where(a => a.NoteTypeId == noteTypes[0].NoteTypeId).Select(a => a.Name).ToList();
        CollectionAssert.AreEquivalent(attributeNames, attributes, $"Attributes of {noteTypeName}");

        var folders = (await service.Repository.Folders.GetAllAsync()).Entity.Where(f => f.Name == folderName).ToList();
        Assert.AreEqual(1, folders.Count, $"Folders {folderName}");
    }

    private static async Task<FolderDto> AddFolderAsync(KntService service, string name, Guid folderId, int order)
    {
        var res = await service.Repository.Folders.AddAsync(new FolderDto { FolderId = folderId, Name = name, Order = order });
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        return res.Entity;
    }
}
