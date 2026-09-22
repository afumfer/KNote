using KNote.Model;
using KNote.Service.Core;
using Microsoft.Extensions.Logging;

namespace KNote.ClientWin.Core;

/// <summary>
/// Startup-time sanitation for the "Application info" rows, independent of AppInfoAlarmsCtrl and its
/// view. KNoteManagementCtrl must decide whether to show the panel *before* creating it - creating
/// AppInfoAlarmsCtrl shows its window immediately, via the generic CtrlViewBase.Run() shared by every
/// Ctrl (see AppInfoAlarmsCtrl's own constructor/OnInitialized) - so the same "is this row still valid"
/// check (AppInfoRowRefresh.IsAddressedToActiveUser) needs to run here first, without a view, instead of
/// deciding from the raw persisted row count and only pruning afterwards.
///
/// A row for a repository that is not currently configured/reachable, or that errors out answering
/// either question below, is left untouched - unreachable or erroring right now isn't evidence of
/// orphaned - so a single run is not guaranteed to remove everything that eventually turns out invalid.
/// Every such failure is isolated to its own alias/row (never aborts the rest of the pass) and reported
/// through onError, since this runs unattended (fire-and-forget from KNoteManagementCtrl's startup) with
/// nothing else to surface it.
/// </summary>
public static class AppInfoAlarmRowMaintenance
{
    public static async Task<bool> PruneAndHasAnyRowAsync(Store store)
    {
        try
        {
            var hasAnyConfirmedRow = await PruneAsync(
                store.State.AppInfoAlarmsWindow.Rows,
                alias => store.GetServiceRef(alias)?.Service,
                store.GetUserId,
                ex => store.Logger?.LogError(ex, "AppInfoAlarmRowMaintenance: {message}", ex.Message));

            store.SaveConfig();
            return hasAnyConfirmedRow;
        }
        catch (Exception ex)
        {
            // Belt and braces: PruneAsync itself isolates every per-alias/per-row failure already, so
            // this is only reached by something outside that (e.g. SaveConfig failing to write the
            // file) - still must not let a startup-time, fire-and-forget task crash unobserved.
            store.Logger?.LogError(ex, "AppInfoAlarmRowMaintenance.PruneAndHasAnyRowAsync: {message}", ex.Message);
            return false;
        }
    }

    // The testable core, kept free of Store/ServiceRef (ServiceRef always builds a real repository
    // connection as soon as it's constructed, with no seam left to inject a test double - see
    // ClientWin.Tests/AppInfoAlarmRowMaintenanceTests.cs) - a resolveService/getActiveUserId pair
    // stands in for Store.GetServiceRef(alias)?.Service / Store.GetUserId, same idea as
    // AppInfoRowRefresh's own activeUserIdOf delegate.
    internal static async Task<bool> PruneAsync(
        IList<AppInfoAlarmRow> rows,
        Func<string, IKntService> resolveService,
        Func<IKntService, Task<Guid?>> getActiveUserId,
        Action<Exception> onError = null)
    {
        if (rows.Count == 0)
            return false;

        // Absent from this dictionary covers both "not configured" (resolveService returned null) and
        // "configured but errored just now" (getActiveUserId threw) - deliberately the same outcome
        // below: rows for that alias are left alone either way, not judged.
        var activeUsers = new Dictionary<string, Guid?>();
        foreach (var alias in rows.Select(r => r.RepositoryAlias).Distinct().ToList())
        {
            var service = resolveService(alias);
            if (service == null)
                continue;

            try
            {
                activeUsers[alias] = await getActiveUserId(service);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
            }
        }

        // Not the same thing as "rows.Count > 0" once pruning is done: a row whose repository doesn't
        // currently resolve is deliberately left alone (see the class doc comment) but is exactly as
        // unreachable to AppInfoAlarmsCtrl.LoadPersistedRows - it will never get to View.AddOrUpdateRow
        // either, so it must not count as "there is something to show". Only a row confirmed valid here
        // (reachable repository, still qualifies) does.
        var hasAnyConfirmedRow = false;

        foreach (var row in rows.ToList())
        {
            if (!activeUsers.TryGetValue(row.RepositoryAlias, out var activeUserId))
                continue;

            try
            {
                if (await IsStillValidAsync(resolveService(row.RepositoryAlias), row, activeUserId))
                    hasAnyConfirmedRow = true;
                else
                    rows.Remove(row);
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
            }
        }

        return hasAnyConfirmedRow;
    }

    private static async Task<bool> IsStillValidAsync(IKntService service, AppInfoAlarmRow row, Guid? activeUserId)
    {
        var message = await service.Notes.GetMessageAsync(row.KMessageId);
        return AppInfoRowRefresh.IsAddressedToActiveUser(message.IsValid ? message.Entity : null, activeUserId)
            && message.Entity.NoteId != null;
    }
}
