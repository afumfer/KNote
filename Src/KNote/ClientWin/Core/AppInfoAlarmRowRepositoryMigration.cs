using KNote.Model;

namespace KNote.ClientWin.Core;

/// <summary>
/// LEGACY(repository-alias): remove together with AppInfoAlarmRow.RepositoryAlias and RepositoryRef.LegacyAlias.
/// The rows of the "Application info" panel saved before repositories had an id identify their repository
/// by the alias it had in this installation's configuration. Once the repositories are open, each row is
/// given the RepositoryId of the repository that had that alias (its LegacyAlias, still in memory; or else
/// its name, which is the alias when this installation named the database). A row whose repository can't be
/// opened now keeps its alias for the next startup.
/// </summary>
public static class AppInfoAlarmRowRepositoryMigration
{
    // True when some row changed (the state file must be saved).
    public static bool Apply(IEnumerable<AppInfoAlarmRow> rows, IEnumerable<RepositoryRef> openRepositories)
    {
        var repositories = openRepositories.Where(r => r.RepositoryId != Guid.Empty).ToList();
        var changed = false;

        foreach (var row in rows.Where(r => r.RepositoryId == Guid.Empty && !string.IsNullOrEmpty(r.RepositoryAlias)))
        {
            var repository = repositories.FirstOrDefault(r => r.LegacyAlias == row.RepositoryAlias)
                ?? repositories.FirstOrDefault(r => r.Name == row.RepositoryAlias);
            if (repository == null)
                continue;

            row.RepositoryId = repository.RepositoryId;
            changed = true;
        }

        return changed;
    }
}
