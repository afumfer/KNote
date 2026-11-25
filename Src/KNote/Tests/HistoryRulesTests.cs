using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// HistoryRules (the history of the notes): when a change goes into the row of its session (H5), which versions the retention
/// empties (H7), the trash of the deleted notes (H8), and the tags of PatchChangeTags (NoteChangeRules.ChangeTag).
/// </summary>
[TestClass]
public class HistoryRulesTests
{
    private static readonly DateTime Now = new(2026, 11, 25, 12, 0, 0);

    private static KLogDto Last(string action = "Updated", string user = "armando", string origin = "Desktop", int minutesAgo = 3) => new()
    {
        KLogId = Guid.NewGuid(), ActionType = action, UserName = user, Origin = origin, LastDateTime = Now.AddMinutes(-minutesAgo)
    };

    [TestMethod]
    public void TheSameUserOriginAndNote_WithinTenMinutes_Group()
    {
        Assert.IsTrue(HistoryRules.CanGroup(Last(), "Armando", EnumHistoryOrigin.Desktop, Now));
        Assert.IsTrue(HistoryRules.CanGroup(Last("Created"), "armando", EnumHistoryOrigin.Desktop, Now));
        Assert.IsTrue(HistoryRules.CanGroup(Last(minutesAgo: 9), "armando", EnumHistoryOrigin.Desktop, Now));
    }

    [TestMethod]
    public void AnotherUserOriginOrAction_OrTenMinutesLater_DontGroup()
    {
        Assert.IsFalse(HistoryRules.CanGroup(null, "armando", EnumHistoryOrigin.Desktop, Now));
        Assert.IsFalse(HistoryRules.CanGroup(Last(user: "marta"), "armando", EnumHistoryOrigin.Desktop, Now));
        Assert.IsFalse(HistoryRules.CanGroup(Last(), "armando", EnumHistoryOrigin.Web, Now));
        Assert.IsFalse(HistoryRules.CanGroup(Last(minutesAgo: 10), "armando", EnumHistoryOrigin.Desktop, Now));
        Assert.IsFalse(HistoryRules.CanGroup(Last(minutesAgo: -1), "armando", EnumHistoryOrigin.Desktop, Now));
        Assert.IsFalse(HistoryRules.CanGroup(Last("Deleted"), "armando", EnumHistoryOrigin.Desktop, Now));
        Assert.IsFalse(HistoryRules.CanGroup(Last("Restored"), "armando", EnumHistoryOrigin.Desktop, Now));
    }

    [TestMethod]
    public void Timestamp_IsToTheMillisecond()
    {
        var moment = new DateTime(2026, 11, 25, 10, 31, 12, 987).AddTicks(1234);
        Assert.AreEqual(new DateTime(2026, 11, 25, 10, 31, 12, 987), HistoryRules.Timestamp(moment));
    }

    [TestMethod]
    public void Retention_KeepsAllForThreeMonths_ThenOneADay_ThenOneAMonth()
    {
        var rows = new List<KLogDto>
        {
            Copy(Now.AddDays(-1)), Copy(Now.AddDays(-1).AddHours(1)),                       // all of them
            Copy(Now.AddMonths(-4)), Copy(Now.AddMonths(-4).AddHours(2)),                  // one that day: the first
            Copy(Now.AddMonths(-4).AddDays(1)),                                             // another day
            Copy(new DateTime(2025, 6, 3)), Copy(new DateTime(2025, 6, 20)),               // one that month: the first
            Copy(new DateTime(2025, 5, 2)),
            new KLogDto { KLogId = Guid.NewGuid(), RegistryDateTime = Now.AddMonths(-5) }   // no version
        };

        var empty = HistoryRules.CopiesToEmpty(rows, new HistoryRetentionDto(), Now);

        CollectionAssert.AreEquivalent(new[] { rows[3].KLogId, rows[6].KLogId }, empty);
    }

    [TestMethod]
    public void Retention_AboveTheMaximum_EmptiesTheOldest()
    {
        var rows = Enumerable.Range(0, 6).Select(i => Copy(Now.AddDays(-i))).ToList();

        var empty = HistoryRules.CopiesToEmpty(rows, new HistoryRetentionDto { MaxCopiesPerNote = 4 }, Now);

        CollectionAssert.AreEquivalent(new[] { rows[5].KLogId, rows[4].KLogId }, empty);
    }

    [TestMethod]
    public void TheRetention_ReadsWithItsDefaults()
    {
        var defaults = HistoryRetentionDto.FromJson(null);
        Assert.AreEqual(3, defaults.KeepAllMonths);
        Assert.AreEqual(12, defaults.DailyUntilMonths);
        Assert.AreEqual(2, defaults.MaxContentSizeMB);
        Assert.AreEqual(500, defaults.MaxCopiesPerNote);
        Assert.AreEqual(30, defaults.DeletedNotesDays);
        Assert.AreEqual(30, HistoryRetentionDto.FromJson("not json").DeletedNotesDays);

        var stored = HistoryRetentionDto.FromJson("{\"keepAllMonths\":6,\"dailyUntilMonths\":2,\"deletedNotesDays\":7}");
        Assert.AreEqual(6, stored.KeepAllMonths);
        Assert.AreEqual(6, stored.DailyUntilMonths, "never shorter than the period with all of them");
        Assert.AreEqual(Now.AddDays(-7), HistoryRules.DeletedNotesLimit(stored, Now));
    }

    [TestMethod]
    public void TheRetention_ValidatesItsLimits_AndItsJsonHasOnlyItsValues()
    {
        Assert.IsTrue(new HistoryRetentionDto().IsValid());
        Assert.IsFalse(new HistoryRetentionDto { KeepAllMonths = -1 }.IsValid());
        Assert.IsFalse(new HistoryRetentionDto { MaxCopiesPerNote = 0 }.IsValid());
        Assert.IsFalse(new HistoryRetentionDto { DeletedNotesDays = HistoryRetentionDto.MaxDeletedNotesDays + 1 }.IsValid());
        Assert.IsFalse(new HistoryRetentionDto { KeepAllMonths = 6, DailyUntilMonths = 3 }.IsValid());

        Assert.AreEqual("{\"keepAllMonths\":3,\"dailyUntilMonths\":12,\"maxContentSizeMB\":2,\"maxCopiesPerNote\":500,\"deletedNotesDays\":30}",
            new HistoryRetentionDto().ToJson());
    }

    [TestMethod]
    public void ABigNote_IsNotedWithoutItsCopy()
    {
        var retention = new HistoryRetentionDto { MaxContentSizeMB = 1 };
        Assert.IsTrue(HistoryRules.FitsCopy(1024 * 1024, retention));
        Assert.IsFalse(HistoryRules.FitsCopy(1024 * 1024 + 1, retention));
        Assert.AreEqual(5, HistoryRules.ContentSize("ñañ"));
    }

    [TestMethod]
    public void NoteTitle()
    {
        Assert.AreEqual("#340 Amstrad CPC 464", HistoryRules.NoteTitle(340, "Amstrad CPC 464"));
        Assert.AreEqual(KLogDto.MaxEntityTitleLength, HistoryRules.NoteTitle(1, new string('x', 1000)).Length);
    }

    [TestMethod]
    public void ChangeTag_AsTheRepositoriesDidIt()
    {
        Assert.AreEqual("a b", NoteChangeRules.ChangeTag("a", null, "b"));
        Assert.AreEqual("a", NoteChangeRules.ChangeTag("a", "", "a"));
        Assert.AreEqual("b", NoteChangeRules.ChangeTag(null, null, "b"));
        Assert.AreEqual("x c", NoteChangeRules.ChangeTag("a c", "a", "x"));
        Assert.AreEqual("c", NoteChangeRules.ChangeTag("a c", "a", null));
    }

    [TestMethod]
    public void TheVersionAfterAChange_IsTheCopyOfTheNextRowWithOne_OrTheNoteNow()
    {
        var created = Row(1, "Created", copy: false, EnumHistoryFields.None);
        var edited = Row(2, "Updated", copy: true, EnumHistoryFields.Description);
        var taskOnly = Row(3, "Updated", copy: false, EnumHistoryFields.Tasks);
        var renamed = Row(4, "Updated", copy: true, EnumHistoryFields.Topic);
        var rows = new[] { renamed, created, taskOnly, edited };

        Assert.AreEqual(edited, HistoryRules.VersionAfter(rows, created.KLogId).CopyRow, "how the note was before the next change");
        var afterEdit = HistoryRules.VersionAfter(rows, edited.KLogId);
        Assert.AreEqual(renamed, afterEdit.CopyRow, "a row of its tasks only has no copy and doesn't count");
        Assert.IsTrue(afterEdit.Exact);
        var last = HistoryRules.VersionAfter(rows, renamed.KLogId);
        Assert.IsTrue(last.CurrentNote);
        Assert.IsTrue(last.Exact);
        Assert.IsNull(HistoryRules.VersionAfter(rows, Guid.NewGuid()));
    }

    [TestMethod]
    public void WithoutACopyInBetween_TheVersionIncludesLaterChanges()
    {
        var first = Row(1, "Updated", copy: true, EnumHistoryFields.Topic);
        var emptied = Row(2, "Updated", copy: false, EnumHistoryFields.Description); // its copy emptied by the retention
        var next = Row(3, "Updated", copy: true, EnumHistoryFields.Topic);

        var version = HistoryRules.VersionAfter(new[] { first, emptied, next }, first.KLogId);

        Assert.AreEqual(next, version.CopyRow);
        Assert.IsFalse(version.Exact);
        Assert.AreEqual(emptied.LastDateTime, version.IncludesUntil);
        StringAssert.StartsWith(HistoryRules.VersionNotice(version.IncludesUntil), "Includes later changes, up to ");
        Assert.IsNull(HistoryRules.VersionNotice(null));
    }

    [TestMethod]
    public void WhenText_IsAMomentOrASession()
    {
        var culture = System.Globalization.CultureInfo.GetCultureInfo("es-ES");
        var moment = new KLogDto { RegistryDateTime = new DateTime(2026, 11, 25, 10, 5, 0), LastDateTime = new DateTime(2026, 11, 25, 10, 5, 30) };
        var session = new KLogDto { RegistryDateTime = new DateTime(2026, 11, 25, 10, 5, 0), LastDateTime = new DateTime(2026, 11, 25, 10, 31, 0) };

        Assert.AreEqual("25/11/2026 10:05", HistoryRules.WhenText(moment, culture));
        Assert.AreEqual("25/11/2026 10:05–10:31", HistoryRules.WhenText(session, culture));
    }

    private static KLogDto Row(int minute, string action, bool copy, EnumHistoryFields fields) => new()
    {
        KLogId = Guid.NewGuid(), ActionType = action, Fields = fields, PreviousContentSize = copy ? 10 : null,
        RegistryDateTime = Now.AddMinutes(minute), LastDateTime = Now.AddMinutes(minute).AddSeconds(20)
    };

    private static KLogDto Copy(DateTime when) => new()
    {
        KLogId = Guid.NewGuid(), RegistryDateTime = when, LastDateTime = when, PreviousContentSize = 100
    };
}
