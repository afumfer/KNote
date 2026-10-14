using System.Xml.Serialization;
using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Tests;

/// <summary>
/// The "Application info" rows saved when repositories were known by their alias get the RepositoryId of
/// the repository that had that alias in this installation's configuration, once it is open; until then
/// they keep the alias (also in the state file).
/// </summary>
[TestClass]
public class AppInfoAlarmRowRepositoryMigrationTests
{
    [TestMethod]
    public void RowOfAnOldAlias_GetsTheIdOfTheRepositoryThatHadIt_EvenIfTheDatabaseIsNamedOtherwise()
    {
        // A shared database that another installation named first: its name is not this alias.
        var work = OpenRepository(legacyAlias: "My work", name: "Team notes");
        var row = OldRow("My work");

        var changed = AppInfoAlarmRowRepositoryMigration.Apply([row], [work]);

        Assert.IsTrue(changed);
        Assert.AreEqual(work.RepositoryId, row.RepositoryId);
    }

    [TestMethod]
    public void RowOfAnOldAlias_FallsBackToTheName_WhenTheConfigurationHasNoAliasAnymore()
    {
        var work = OpenRepository(legacyAlias: null, name: "Work");
        var row = OldRow("Work");

        AppInfoAlarmRowRepositoryMigration.Apply([row], [work]);

        Assert.AreEqual(work.RepositoryId, row.RepositoryId);
    }

    [TestMethod]
    public void TheAliasOfTheConfiguration_WinsOverTheNameOfAnotherRepository()
    {
        var aliasOwner = OpenRepository(legacyAlias: "Work", name: "Team notes");
        var sameName = OpenRepository(legacyAlias: null, name: "Work");
        var row = OldRow("Work");

        AppInfoAlarmRowRepositoryMigration.Apply([row], [sameName, aliasOwner]);

        Assert.AreEqual(aliasOwner.RepositoryId, row.RepositoryId);
    }

    [TestMethod]
    public void RowOfARepositoryNotOpen_KeepsItsAlias()
    {
        var notOpened = new RepositoryRef { LegacyAlias = "Offline" }; // no RepositoryId: it couldn't be opened
        var row = OldRow("Offline");

        var changed = AppInfoAlarmRowRepositoryMigration.Apply([row], [notOpened]);

        Assert.IsFalse(changed);
        Assert.AreEqual(Guid.Empty, row.RepositoryId);
        Assert.AreEqual("Offline", row.RepositoryAlias);
    }

    [TestMethod]
    public void RowWithAnId_IsLeftAlone()
    {
        var work = OpenRepository(legacyAlias: "Work", name: "Work");
        var id = Guid.NewGuid();
        var row = new AppInfoAlarmRow { KMessageId = Guid.NewGuid(), RepositoryId = id };

        var changed = AppInfoAlarmRowRepositoryMigration.Apply([row], [work]);

        Assert.IsFalse(changed);
        Assert.AreEqual(id, row.RepositoryId);
    }

    [TestMethod]
    public void StateFile_KeepsTheAliasOnlyWhileTheRowHasNoId()
    {
        var unresolved = OldRow("Offline");
        var resolved = OldRow("Work");
        resolved.RepositoryId = Guid.NewGuid();
        var state = new AppUserState();
        state.AppInfoAlarmsWindow.Rows.AddRange([unresolved, resolved]);

        var rows = RoundTrip(state).AppInfoAlarmsWindow.Rows;

        Assert.AreEqual("Offline", rows[0].RepositoryAlias);
        Assert.AreEqual(Guid.Empty, rows[0].RepositoryId);
        Assert.IsNull(rows[1].RepositoryAlias);
        Assert.AreEqual(resolved.RepositoryId, rows[1].RepositoryId);
    }

    private static RepositoryRef OpenRepository(string? legacyAlias, string name)
    {
        var repositoryRef = new RepositoryRef();
        if (legacyAlias != null)
            repositoryRef.LegacyAlias = legacyAlias;
        repositoryRef.Name = name;
        repositoryRef.RepositoryId = Guid.NewGuid();
        return repositoryRef;
    }

    // A row as saved before repositories had an id.
    private static AppInfoAlarmRow OldRow(string alias) => new()
    {
        KMessageId = Guid.NewGuid(),
        RepositoryAlias = alias,
        NotifiedAt = new DateTime(2026, 1, 1, 10, 0, 0)
    };

    private static AppUserState RoundTrip(AppUserState state)
    {
        var serializer = new XmlSerializer(typeof(AppUserState));
        using var stream = new MemoryStream();
        serializer.Serialize(stream, state);
        stream.Position = 0;
        return (AppUserState)serializer.Deserialize(stream)!;
    }
}
