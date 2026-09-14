using System.Linq;
using System.Threading;
using KNote.Service.Core;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// Regression coverage for the SaveSystemVariable race condition: it used to decide insert-vs-
/// update from a stale read (GetAsync), so concurrent callers writing the same Scope+Key could all
/// see "no row yet" and all try to insert, racing on the unique index over (Scope, Key). Fixed to
/// retry as an update when a concurrent caller wins the insert (see
/// KntUniqueConstraintViolationException / KntService.SaveSystemVariable).
///
/// SaveSystemVariable used to be genuinely fire-and-forget (Task.Run(...) with no await/.Result),
/// so a plain sequential loop over calls created no real concurrency and, worse, let the test's own
/// read race the background saves regardless of overlap. Task.Run + a shared start gate dispatches
/// the calls to separate thread-pool threads and releases them together, forcing their "read
/// current row, decide insert-vs-update" steps to genuinely overlap - the same technique, and the
/// same reason for it, as ConcurrentAddParityTests. Verified both ways by temporarily reverting the
/// fix (git stash): the EntityFramework case then reliably failed ("found 0 rows" - the pending
/// fire-and-forget saves hadn't landed yet when this test's own read ran); the Dapper case did not
/// reliably fail pre-fix in that same run, since its writes are fast enough to usually land before
/// this test's own read regardless. Both cases pass deterministically post-fix, because
/// SaveSystemVariable now genuinely blocks until its save completes (or throws) instead of
/// returning before the write lands.
/// </summary>
[TestClass]
public class SaveSystemVariableConcurrencyTests
{
    private const int ConcurrentWrites = 10;

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ConcurrentSaveSystemVariable_SameScopeAndKey_NoDuplicateRowsAndNoExceptions(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var repo = db.CreateRepository(orm);
        var service = new KntService(repo, activateMessageBroker: false);

        const string scope = "TEST_SCOPE";
        const string key = "TEST_KEY";

        using var startGate = new ManualResetEventSlim(false);

        var tasks = Enumerable.Range(0, ConcurrentWrites)
            .Select(i => Task.Run(() =>
            {
                startGate.Wait();
                service.SaveSystemVariable(scope, key, $"value-{i}");
            }))
            .ToArray();

        startGate.Set();

        // Should not throw, and should not return before every write has actually landed.
        await Task.WhenAll(tasks);

        var all = await service.Repository.SystemValues.GetAllAsync();
        Assert.IsTrue(all.IsValid, all.ErrorMessage);

        var matching = all.Entity.Where(sv => sv.Scope == scope && sv.Key == key).ToList();
        Assert.AreEqual(1, matching.Count,
            $"Expected exactly one row for {scope}/{key}, found {matching.Count}: {string.Join(",", matching.Select(sv => sv.Value))}");
    }
}
