using KNote.Model;
using KNote.Service.Core;

namespace KNote.ClientWin.Core;

/// <summary>
/// Startup-time sanitation for the "Application info" rows, independent of AppInfoAlarmsCtrl and its
/// view. KNoteManagementCtrl must decide whether to show the panel *before* creating it - creating
/// AppInfoAlarmsCtrl shows its window immediately, via the generic CtrlViewBase.Run() shared by every
/// Ctrl (see AppInfoAlarmsCtrl's own constructor/OnInitialized) - so the same "is this row still valid"
/// check (AppInfoRowRefresh.IsAddressedToActiveUser) needs to run here first, without a view, instead of
/// deciding from the raw persisted row count and only pruning afterwards.
///
/// A row for a repository that is not currently configured/reachable is left untouched - unreachable
/// right now isn't evidence of orphaned - so a single run is not guaranteed to remove everything that
/// eventually turns out invalid.
/// </summary>
public static class AppInfoAlarmRowMaintenance
{
    public static async Task<bool> PruneAndHasAnyRowAsync(Store store)
    {
        var rows = store.State.AppInfoAlarmsWindow.Rows;
        if (rows.Count == 0)
            return false;

        var activeUsers = new Dictionary<string, Guid?>();
        foreach (var alias in rows.Select(r => r.RepositoryAlias).Distinct().ToList())
        {
            var serviceRef = store.GetServiceRef(alias);
            activeUsers[alias] = serviceRef == null ? null : await store.GetUserId(serviceRef.Service);
        }

        foreach (var row in rows.ToList())
        {
            var serviceRef = store.GetServiceRef(row.RepositoryAlias);
            if (serviceRef == null)
                continue;

            if (!await IsStillValidAsync(serviceRef.Service, row, activeUsers[row.RepositoryAlias]))
                rows.Remove(row);
        }

        store.SaveConfig();
        return rows.Count > 0;
    }

    private static async Task<bool> IsStillValidAsync(IKntService service, AppInfoAlarmRow row, Guid? activeUserId)
    {
        var message = await service.Notes.GetMessageAsync(row.KMessageId);
        return AppInfoRowRefresh.IsAddressedToActiveUser(message.IsValid ? message.Entity : null, activeUserId)
            && message.Entity.NoteId != null;
    }
}
