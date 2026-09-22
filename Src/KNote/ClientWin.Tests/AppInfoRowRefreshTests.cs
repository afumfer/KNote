using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

[TestClass]
public class AppInfoRowRefreshTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid NoteId = Guid.NewGuid();
    private const string Alias = "repo";

    private static Guid? ActiveUserOf(string alias) => alias == Alias ? UserId : null;

    private static AppInfoAlarmRow NewRow(Guid? noteId = null) => new()
    {
        KMessageId = Guid.NewGuid(),
        RepositoryAlias = Alias,
        NoteId = noteId ?? NoteId,
        NoteTopic = "old topic",
        Comment = "old comment"
    };

    private static KMessageDto NewMessage(AppInfoAlarmRow row, EnumNotificationType type = EnumNotificationType.AppInfo, Guid? userId = null) => new()
    {
        KMessageId = row.KMessageId,
        NotificationType = type,
        UserId = userId ?? UserId,
        Comment = row.Comment
    };

    private static NoteExtendedDto NewNote(string topic, params KMessageDto[] messages) => new()
    {
        NoteId = NoteId,
        Topic = topic,
        Messages = messages.ToList()
    };

    [TestMethod]
    public void ApplyUserSaved_FullNameChanged_UpdatesRowsOfThatUserOnly()
    {
        var mine = NewRow();
        mine.UserFullName = "old name";
        var otherRepo = NewRow();
        otherRepo.RepositoryAlias = "other";
        otherRepo.UserFullName = "old name";

        var changes = AppInfoRowRefresh.ApplyUserSaved([mine, otherRepo], new UserDto { UserId = UserId, FullName = "new name" }, ActiveUserOf);

        Assert.AreEqual(1, changes.Count);
        Assert.AreSame(mine, changes[0].Row);
        Assert.AreEqual(AppInfoRowRefresh.ChangeKind.Updated, changes[0].Kind);
        Assert.AreEqual("new name", mine.UserFullName);
        Assert.AreEqual("old name", otherRepo.UserFullName);
    }

    [TestMethod]
    public void ApplyUserSaved_FullNameUnchanged_ReportsNoChanges()
    {
        var row = NewRow();
        row.UserFullName = "same";

        var changes = AppInfoRowRefresh.ApplyUserSaved([row], new UserDto { UserId = UserId, FullName = "same" }, ActiveUserOf);

        Assert.AreEqual(0, changes.Count);
    }

    [TestMethod]
    public void ApplyUserSaved_AnotherUser_ReportsNoChanges()
    {
        var row = NewRow();
        row.UserFullName = "old name";

        var changes = AppInfoRowRefresh.ApplyUserSaved([row], new UserDto { UserId = Guid.NewGuid(), FullName = "new name" }, ActiveUserOf);

        Assert.AreEqual(0, changes.Count);
        Assert.AreEqual("old name", row.UserFullName);
    }

    [TestMethod]
    public void ApplyNoteSaved_TopicAndCommentChanged_UpdatesRow()
    {
        var row = NewRow();
        var message = NewMessage(row);
        message.Comment = "new comment";

        var changes = AppInfoRowRefresh.ApplyNoteSaved([row], NewNote("new topic", message), ActiveUserOf);

        Assert.AreEqual(1, changes.Count);
        Assert.AreEqual(AppInfoRowRefresh.ChangeKind.Updated, changes[0].Kind);
        Assert.AreEqual("new topic", row.NoteTopic);
        Assert.AreEqual("new comment", row.Comment);
    }

    [TestMethod]
    public void ApplyNoteSaved_NothingChanged_ReportsNoChanges()
    {
        var row = NewRow();

        var changes = AppInfoRowRefresh.ApplyNoteSaved([row], NewNote("old topic", NewMessage(row)), ActiveUserOf);

        Assert.AreEqual(0, changes.Count);
    }

    [TestMethod]
    public void ApplyNoteSaved_MessageDeleted_RemovesRow()
    {
        var row = NewRow();

        var changes = AppInfoRowRefresh.ApplyNoteSaved([row], NewNote("old topic"), ActiveUserOf);

        Assert.AreEqual(1, changes.Count);
        Assert.AreEqual(AppInfoRowRefresh.ChangeKind.Removed, changes[0].Kind);
    }

    [TestMethod]
    public void ApplyNoteSaved_MessagesListNull_RemovesRow()
    {
        var row = NewRow();
        var note = new NoteExtendedDto { NoteId = NoteId, Topic = "old topic", Messages = null };

        var changes = AppInfoRowRefresh.ApplyNoteSaved([row], note, ActiveUserOf);

        Assert.AreEqual(AppInfoRowRefresh.ChangeKind.Removed, changes.Single().Kind);
    }

    [TestMethod]
    [DataRow(EnumNotificationType.PostIt)]
    [DataRow(EnumNotificationType.Email)]
    [DataRow(EnumNotificationType.ExecuteKntScript)]
    public void ApplyNoteSaved_TypeNoLongerAppInfo_RemovesRow(EnumNotificationType type)
    {
        var row = NewRow();

        var changes = AppInfoRowRefresh.ApplyNoteSaved([row], NewNote("old topic", NewMessage(row, type)), ActiveUserOf);

        Assert.AreEqual(AppInfoRowRefresh.ChangeKind.Removed, changes.Single().Kind);
    }

    [TestMethod]
    public void ApplyNoteSaved_RecipientChangedToAnotherUser_RemovesRow()
    {
        var row = NewRow();

        var changes = AppInfoRowRefresh.ApplyNoteSaved([row], NewNote("old topic", NewMessage(row, userId: Guid.NewGuid())), ActiveUserOf);

        Assert.AreEqual(AppInfoRowRefresh.ChangeKind.Removed, changes.Single().Kind);
    }

    [TestMethod]
    public void ApplyNoteSaved_RecipientCleared_RemovesRow()
    {
        var row = NewRow();
        var message = NewMessage(row);
        message.UserId = null;

        var changes = AppInfoRowRefresh.ApplyNoteSaved([row], NewNote("old topic", message), ActiveUserOf);

        Assert.AreEqual(AppInfoRowRefresh.ChangeKind.Removed, changes.Single().Kind);
    }

    [TestMethod]
    public void ApplyNoteSaved_UnknownActiveUserForRepository_RemovesRow()
    {
        var row = NewRow();
        row.RepositoryAlias = "other";

        var changes = AppInfoRowRefresh.ApplyNoteSaved([row], NewNote("old topic", NewMessage(row)), ActiveUserOf);

        Assert.AreEqual(AppInfoRowRefresh.ChangeKind.Removed, changes.Single().Kind);
    }

    [TestMethod]
    public void ApplyNoteSaved_OnlyAffectsRowsOfThatNote()
    {
        var mine = NewRow();
        var other = NewRow(Guid.NewGuid());
        var note = NewNote("new topic", NewMessage(mine));

        var changes = AppInfoRowRefresh.ApplyNoteSaved([mine, other], note, ActiveUserOf);

        Assert.AreEqual(1, changes.Count);
        Assert.AreSame(mine, changes[0].Row);
        Assert.AreEqual("old topic", other.NoteTopic);
    }

    [TestMethod]
    public void ApplyNoteSaved_TwoRowsSameNote_EachResolvedByItsOwnMessage()
    {
        var kept = NewRow();
        var dropped = NewRow();

        var changes = AppInfoRowRefresh.ApplyNoteSaved([kept, dropped], NewNote("old topic", NewMessage(kept)), ActiveUserOf);

        Assert.AreEqual(1, changes.Count);
        Assert.AreSame(dropped, changes[0].Row);
        Assert.AreEqual(AppInfoRowRefresh.ChangeKind.Removed, changes[0].Kind);
    }
}
