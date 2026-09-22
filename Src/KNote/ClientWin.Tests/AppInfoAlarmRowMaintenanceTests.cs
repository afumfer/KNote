using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Tests;

// Regression coverage for a bug found while testing: a row whose repository doesn't currently resolve
// (deliberately left untouched - see AppInfoAlarmRowMaintenance's doc comment) used to count towards
// "there is something to show", even though AppInfoAlarmsCtrl.LoadPersistedRows can never actually
// display it either - so the panel opened anyway, empty, and the unresolved row was never removed.
//
// Exercises the internal PruneAsync core directly (see its own doc comment for why: ServiceRef always
// builds a real repository connection as soon as it's constructed, leaving no seam to inject a fake
// IKntService through a real Store/ServiceRef).
[TestClass]
public class AppInfoAlarmRowMaintenanceTests
{
    private const string Alias = "repo";
    private const string UnknownAlias = "not configured";
    private static readonly Guid UserId = Guid.NewGuid();

    private static AppInfoAlarmRow NewRow(string alias = Alias) => new()
    {
        KMessageId = Guid.NewGuid(),
        RepositoryAlias = alias,
        NotifiedAt = DateTime.Now
    };

    private static Func<string, IKntService> Resolve(FakeKntService service) => alias => alias == Alias ? service : null;

    private static Func<IKntService, Task<Guid?>> ActiveUser(Guid? userId) => _ => Task.FromResult(userId);

    private static FakeKntService NewFakeService(Func<Guid, Task<Result<KMessageDto>>> getMessage = null)
    {
        var service = new FakeKntService();
        if (getMessage != null)
            service.NotesFake.GetMessageAsyncImpl = getMessage;
        return service;
    }

    private static Result<KMessageDto> ValidAppInfoMessage(Guid id, Guid userId) => new()
    {
        Entity = new KMessageDto { KMessageId = id, NoteId = Guid.NewGuid(), NotificationType = EnumNotificationType.AppInfo, UserId = userId }
    };

    private static Result<KMessageDto> NotFound() => new() { ListErrorMessage = new List<string> { "not found" } };

    [TestMethod]
    public async Task PruneAsync_NoRows_ReturnsFalse()
    {
        var hasAny = await AppInfoAlarmRowMaintenance.PruneAsync(new List<AppInfoAlarmRow>(), _ => null, ActiveUser(null));

        Assert.IsFalse(hasAny);
    }

    [TestMethod]
    public async Task PruneAsync_ValidRow_IsKeptAndReturnsTrue()
    {
        var rows = new List<AppInfoAlarmRow> { NewRow() };
        var service = NewFakeService(id => Task.FromResult(ValidAppInfoMessage(id, UserId)));

        var hasAny = await AppInfoAlarmRowMaintenance.PruneAsync(rows, Resolve(service), ActiveUser(UserId));

        Assert.IsTrue(hasAny);
        Assert.AreEqual(1, rows.Count);
    }

    [TestMethod]
    public async Task PruneAsync_MessageNoLongerExists_RemovesRowAndReturnsFalse()
    {
        var rows = new List<AppInfoAlarmRow> { NewRow() };
        var service = NewFakeService(_ => Task.FromResult(NotFound()));

        var hasAny = await AppInfoAlarmRowMaintenance.PruneAsync(rows, Resolve(service), ActiveUser(UserId));

        Assert.IsFalse(hasAny);
        Assert.AreEqual(0, rows.Count);
    }

    [TestMethod]
    public async Task PruneAsync_MessageNoLongerAddressedToActiveUser_RemovesRow()
    {
        var rows = new List<AppInfoAlarmRow> { NewRow() };
        var service = NewFakeService(id => Task.FromResult(ValidAppInfoMessage(id, Guid.NewGuid())));

        var hasAny = await AppInfoAlarmRowMaintenance.PruneAsync(rows, Resolve(service), ActiveUser(UserId));

        Assert.IsFalse(hasAny);
        Assert.AreEqual(0, rows.Count);
    }

    // The bug: a row for a repository alias that isn't currently configured must not make the panel
    // pop up empty, even though (by design) the row itself is left alone rather than removed.
    [TestMethod]
    public async Task PruneAsync_UnknownRepositoryAlias_IsKeptButReturnsFalse()
    {
        var rows = new List<AppInfoAlarmRow> { NewRow(UnknownAlias) };

        var hasAny = await AppInfoAlarmRowMaintenance.PruneAsync(rows, resolveService: _ => null, ActiveUser(null));

        Assert.IsFalse(hasAny);
        Assert.AreEqual(1, rows.Count, "unreachable now isn't evidence of orphaned");
    }

    [TestMethod]
    public async Task PruneAsync_OneUnknownRepositoryAndOneValidRow_ReturnsTrue()
    {
        var unknown = NewRow(UnknownAlias);
        var valid = NewRow();
        var rows = new List<AppInfoAlarmRow> { unknown, valid };
        var service = NewFakeService(id => Task.FromResult(ValidAppInfoMessage(id, UserId)));

        var hasAny = await AppInfoAlarmRowMaintenance.PruneAsync(rows, Resolve(service), ActiveUser(UserId));

        Assert.IsTrue(hasAny);
        CollectionAssert.Contains(rows, unknown, "the unresolved row is left alone, not removed");
        CollectionAssert.Contains(rows, valid);
    }

    // Regression: a repository that is configured but currently errors out (stale connection string,
    // moved/missing database file after restoring old config files, ...) used to abort pruning for
    // every other repository too, silently - nothing got cleaned up and nothing was logged.
    [TestMethod]
    public async Task PruneAsync_GetActiveUserIdThrows_LeavesRowAloneAndReportsError()
    {
        var rows = new List<AppInfoAlarmRow> { NewRow() };
        var service = NewFakeService();
        var errors = new List<Exception>();

        var hasAny = await AppInfoAlarmRowMaintenance.PruneAsync(
            rows, Resolve(service), _ => throw new InvalidOperationException("db unreachable"), errors.Add);

        Assert.IsFalse(hasAny);
        Assert.AreEqual(1, rows.Count, "couldn't judge - left alone, not removed");
        Assert.AreEqual(1, errors.Count);
    }

    [TestMethod]
    public async Task PruneAsync_GetMessageThrows_LeavesRowAloneAndReportsError()
    {
        var rows = new List<AppInfoAlarmRow> { NewRow() };
        var service = NewFakeService(_ => throw new InvalidOperationException("db unreachable"));
        var errors = new List<Exception>();

        var hasAny = await AppInfoAlarmRowMaintenance.PruneAsync(rows, Resolve(service), ActiveUser(UserId), errors.Add);

        Assert.IsFalse(hasAny);
        Assert.AreEqual(1, rows.Count, "couldn't judge - left alone, not removed");
        Assert.AreEqual(1, errors.Count);
    }

    [TestMethod]
    public async Task PruneAsync_OneRepositoryErrorsAndAnotherIsValid_TheValidOneStillShows()
    {
        const string brokenAlias = "broken";
        var broken = NewRow(brokenAlias);
        var valid = NewRow();
        var rows = new List<AppInfoAlarmRow> { broken, valid };
        var healthyService = NewFakeService(id => Task.FromResult(ValidAppInfoMessage(id, UserId)));
        var brokenService = new FakeKntService(); // configured, but its active-user lookup fails below
        var errors = new List<Exception>();

        IKntService ResolveEither(string alias) => alias switch
        {
            Alias => healthyService,
            brokenAlias => brokenService,
            _ => null
        };

        Task<Guid?> GetActiveUserId(IKntService service) => service == healthyService
            ? Task.FromResult<Guid?>(UserId)
            : throw new InvalidOperationException("repository is unreachable");

        var hasAny = await AppInfoAlarmRowMaintenance.PruneAsync(rows, ResolveEither, GetActiveUserId, errors.Add);

        Assert.IsTrue(hasAny);
        CollectionAssert.Contains(rows, broken, "the broken repository's row is left alone");
        CollectionAssert.Contains(rows, valid);
        Assert.AreEqual(1, errors.Count);
    }
}
