using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Service.ServicesCommands;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// What the «History» tab shows (H.3): a page of the rows of a note, the newest first, and «See this version», the note as it was
/// after a change (H18): the copy of the next row that has one or, without any, the note now; with the names resolved, and saying
/// when copies in between were not kept. Dapper and EF.
/// </summary>
[TestClass]
public class NoteHistoryViewServiceTests
{
    private RepositoryTestDatabase db = null!;
    private KntService service = null!;
    private Guid folderId;

    private async Task OpenAsync(string orm)
    {
        db = new RepositoryTestDatabase();
        service = new KntService(db.CreateRepository(orm), activateMessageBroker: false)
        {
            UserIdentityName = "armando",
            HistoryOrigin = EnumHistoryOrigin.Desktop
        };
        var home = await service.Repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Equipment" });
        folderId = home.Entity.FolderId;
    }

    [TestCleanup]
    public void TestCleanup()
    {
        service?.Dispose();
        db?.Dispose();
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task TheList_ComesInPages_TheNewestFirst(string orm)
    {
        await OpenAsync(orm);
        var noteId = await StoredNoteAsync("V0");
        foreach (var user in new[] { "u1", "u2", "u3" })
            await EditAsync(noteId, user, n => n.Topic = "After " + user);

        var first = (await service.History.GetNoteHistoryAsync(noteId, 0, 2)).Entity;
        var rest = (await service.History.GetNoteHistoryAsync(noteId, 2, 2)).Entity;

        Assert.AreEqual(3, first.Total);
        CollectionAssert.AreEqual(new[] { "u3", "u2" }, first.Rows.Select(r => r.UserName).ToArray());
        Assert.IsTrue(first.HasMore);
        CollectionAssert.AreEqual(new[] { "u1" }, rest.Rows.Select(r => r.UserName).ToArray());
        Assert.IsFalse(rest.HasMore);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AVersion_IsTheNoteAfterTheChange(string orm)
    {
        await OpenAsync(orm);
        var noteId = await StoredNoteAsync("A");
        await EditAsync(noteId, "u1", n => n.Topic = "B");
        await EditAsync(noteId, "u2", n => { n.Topic = "C"; n.Tags = "restored"; });
        var rows = (await service.History.GetNoteHistoryAsync(noteId)).Entity.Rows;

        var afterB = (await service.History.GetNoteVersionAsync(noteId, rows.Single(r => r.UserName == "u1").KLogId)).Entity;
        var afterC = (await service.History.GetNoteVersionAsync(noteId, rows.Single(r => r.UserName == "u2").KLogId)).Entity;

        Assert.AreEqual("B", afterB.Topic, "the copy the next change kept: how the note was before it");
        Assert.IsTrue(afterB.Exact);
        Assert.IsNull(afterB.Notice);
        Assert.AreEqual("C", afterC.Topic, "the last change: the note now");
        Assert.AreEqual("restored", afterC.Tags);
        Assert.AreEqual("Equipment", afterC.FolderPath);
        Assert.AreEqual("u2", afterC.UserName);
        Assert.AreEqual("markdown", afterC.DescriptionFormat);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task WithoutACopyInBetween_TheVersionSaysSo(string orm)
    {
        await OpenAsync(orm);
        var noteId = await StoredNoteAsync("A");
        foreach (var (user, topic) in new[] { ("u1", "B"), ("u2", "C"), ("u3", "D") })
            await EditAsync(noteId, user, n => n.Topic = topic);
        var rows = (await service.History.GetNoteHistoryAsync(noteId)).Entity.Rows;
        // The retention emptied the copy of u2's row (how the note was after u1).
        await service.Repository.History.ClearPreviousContentAsync(new List<Guid> { rows.Single(r => r.UserName == "u2").KLogId });

        var version = (await service.History.GetNoteVersionAsync(noteId, rows.Single(r => r.UserName == "u1").KLogId)).Entity;

        Assert.AreEqual("C", version.Topic, "the next copy kept: it includes u2's change");
        Assert.IsFalse(version.Exact);
        StringAssert.StartsWith(version.Notice, "Includes later changes, up to ");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AVersion_HasTheNamesOfItsTypeAndAttributes(string orm)
    {
        await OpenAsync(orm);
        var type = await service.Repository.NoteTypes.AddAsync(new NoteTypeDto { NoteTypeId = Guid.NewGuid(), Name = "Computer" });
        var attribute = await service.Repository.KAttributes.AddAsync(new KAttributeDto
        {
            KAttributeId = Guid.NewGuid(), Name = "Status", NoteTypeId = type.Entity.NoteTypeId, KAttributeDataType = EnumKAttributeDataType.String
        });
        var noteId = await StoredNoteAsync("CPC", type.Entity.NoteTypeId,
            new NoteKAttributeDto { NoteKAttributeId = Guid.NewGuid(), KAttributeId = attribute.Entity.KAttributeId, Name = "Status", Value = "Works" });
        await EditAsync(noteId, "u1", n => n.Topic = "CPC 464");
        var row = (await service.History.GetNoteHistoryAsync(noteId)).Entity.Rows.Single();

        var version = (await service.History.GetNoteVersionAsync(noteId, row.KLogId)).Entity;

        Assert.AreEqual("Computer", version.NoteTypeName);
        Assert.AreEqual("Status: Works", string.Join("; ", version.Attributes.Select(a => $"{a.Name}: {a.Value}")));
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AChangeOfAnotherNote_IsNotInItsHistory(string orm)
    {
        await OpenAsync(orm);
        var noteId = await StoredNoteAsync("A");
        var otherId = await StoredNoteAsync("Other");
        await EditAsync(otherId, "u1", n => n.Topic = "Other B");
        var otherRow = (await service.History.GetNoteHistoryAsync(otherId)).Entity.Rows.Single();

        var version = await service.History.GetNoteVersionAsync(noteId, otherRow.KLogId);

        Assert.IsFalse(version.IsValid);
        StringAssert.Contains(version.ErrorMessage, KntHistoryGetNoteVersionAsyncCommand.NotInHistory);
    }

    private async Task<Guid> StoredNoteAsync(string topic, Guid? noteTypeId = null, params NoteKAttributeDto[] attributes)
    {
        var note = new NoteDto
        {
            NoteId = Guid.NewGuid(), Topic = topic, Description = "Text", Tags = "", FolderId = folderId, NoteTypeId = noteTypeId,
            ContentType = "markdown", CreationDateTime = new DateTime(2026, 1, 1), ModificationDateTime = new DateTime(2026, 1, 1)
        };
        foreach (var attribute in attributes)
        {
            attribute.NoteId = note.NoteId;
            note.KAttributesDto.Add(attribute);
        }
        Assert.IsTrue((await service.Repository.Notes.AddAsync(note)).IsValid);
        return note.NoteId;
    }

    private async Task EditAsync(Guid noteId, string user, Action<NoteDto> change)
    {
        service.UserIdentityName = user;
        var note = (await service.Notes.GetAsync(noteId)).Entity;
        change(note);
        var saved = await service.Notes.SaveAsync(note, false);
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
    }
}
