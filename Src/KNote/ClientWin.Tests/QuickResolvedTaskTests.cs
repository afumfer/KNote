using KNote.ClientWin.Core;

namespace KNote.ClientWin.Tests;

/// <summary>
/// "Add quick resolved task" (notes list and post-it): a resolved task from the note's creation until now, and "Resolved task - "
/// with the note's topic, shortened when long, as its Topic/Tags.
/// </summary>
[TestClass]
public class QuickResolvedTaskTests
{
    private static readonly Guid NoteId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    [TestMethod]
    public void TheTask_GoesFromTheNotesCreationUntilNow_Resolved()
    {
        var created = new DateTime(2026, 11, 2, 9, 30, 0);
        var now = new DateTime(2026, 11, 20, 18, 5, 0);

        var task = QuickResolvedTask.Create(NoteId, UserId, "Install the server", created, now);

        Assert.AreEqual(NoteId, task.NoteId);
        Assert.AreEqual(UserId, task.UserId);
        Assert.AreEqual(created, task.StartDate);
        Assert.AreEqual(now, task.EndDate);
        Assert.IsTrue(task.Resolved);
        Assert.AreEqual(1, task.Priority);
        Assert.AreEqual("Resolved task - Install the server", task.Tags);
        Assert.AreEqual("Install the server", task.Description);
        Assert.IsTrue(string.IsNullOrEmpty(task.GetErrorMessage()), task.GetErrorMessage());
    }

    [TestMethod]
    public void ANoteDatedAheadOfTheClock_StartsNow()
    {
        var now = new DateTime(2026, 11, 20, 18, 5, 0);

        var task = QuickResolvedTask.Create(NoteId, UserId, "Topic", now.AddMinutes(3), now);

        Assert.AreEqual(now, task.StartDate);
        Assert.AreEqual(now, task.EndDate);
    }

    [TestMethod]
    public void ALongTopic_IsCutAtAWord()
    {
        var topic = "Review the configuration of the backup server after the migration of the main database to the new cluster";

        var tags = QuickResolvedTask.TagsFor(topic);

        Assert.AreEqual("Resolved task - Review the configuration of...", tags);
        Assert.IsTrue(tags.Length <= QuickResolvedTask.Title.Length + 3 + QuickResolvedTask.MaxTopicLength + 3);
    }

    [TestMethod]
    public void ATopicWithoutSpaces_IsCutInTheWord()
    {
        var topic = new string('x', 120);

        Assert.AreEqual($"Resolved task - {new string('x', QuickResolvedTask.MaxTopicLength)}...", QuickResolvedTask.TagsFor(topic));
    }

    [TestMethod]
    [DataRow("  Two\r\nlines   here ", "Resolved task - Two lines here")]
    [DataRow("", "Resolved task")]
    [DataRow(null, "Resolved task")]
    public void TheTopic_IsOneLine_AndWithoutOneOnlyTheTitle(string? topic, string expected)
    {
        Assert.AreEqual(expected, QuickResolvedTask.TagsFor(topic!));
    }
}
