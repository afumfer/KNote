using KNote.Model;
using KNote.Model.Dto;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The noise policy with a repository (KNote Atlas, stage 2, 2.2, D3, D4): without one, an empty policy; kept in SystemValues and
/// read back; without the types and folders that don't exist nor KNote's own; the generic commands of SystemValues don't touch it;
/// and the recount of a policy not saved yet. Dapper and EF.
/// </summary>
[TestClass]
public class NoisePolicyServiceTests
{
    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ThePolicy_IsKeptAndReadBack_WithoutWhatDoesNotExistNorKNotesOwn(string orm)
    {
        using var fixture = await SemanticIndexFixture.CreateAsync(orm);
        var service = fixture.Service;
        Assert.AreEqual(0, (await service.Summaries.GetNoisePolicyAsync()).Entity.ExcludedNoteTypeIds.Count, "none yet: empty");
        var meeting = await AddTypeAsync(fixture, "Meeting");
        var sessions = await AddTypeAsync(fixture, KntConst.ChatSessionsTag);
        var inbox = await AddFolderAsync(fixture, "Inbox");
        var atlas = await AddFolderAsync(fixture, KntConst.AtlasFolderName);

        var saved = await service.Summaries.SaveNoisePolicyAsync(new NoisePolicyDto
        {
            ExcludedNoteTypeIds = { meeting, sessions, Guid.NewGuid() },
            ExcludedFolderIds = { inbox, atlas, fixture.HomeId }
        });

        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        CollectionAssert.AreEqual(new[] { meeting }, saved.Entity.ExcludedNoteTypeIds, "KNote's own and the unknown ones are not kept");
        CollectionAssert.AreEquivalent(new[] { inbox, fixture.HomeId }, saved.Entity.ExcludedFolderIds, "Home is a folder like any other");
        var read = (await service.Summaries.GetNoisePolicyAsync()).Entity;
        CollectionAssert.AreEqual(new[] { meeting }, read.ExcludedNoteTypeIds);
        CollectionAssert.AreEquivalent(new[] { inbox, fixture.HomeId }, read.ExcludedFolderIds);

        // A folder deleted afterwards is not in the policy any more.
        Assert.IsTrue((await service.Folders.DeleteAsync(inbox)).IsValid);
        CollectionAssert.AreEqual(new[] { fixture.HomeId }, (await service.Summaries.GetNoisePolicyAsync()).Entity.ExcludedFolderIds);
    }

    [TestMethod]
    public async Task TheGenericCommandsOfSystemValues_DoNotTouchIt()
    {
        using var fixture = await SemanticIndexFixture.CreateAsync();
        var service = fixture.Service;
        var meeting = await AddTypeAsync(fixture, "Meeting");
        Assert.IsTrue((await service.Summaries.SaveNoisePolicyAsync(new NoisePolicyDto { ExcludedNoteTypeIds = { meeting } })).IsValid);
        var stored = (await service.Repository.SystemValues.GetAsync(KntConst.AiScope, KntConst.AiNoisePolicyKey)).Entity;
        StringAssert.Contains(stored.Value, meeting.ToString(), "the ids, in a JSON");

        stored.Value = "{}";
        Assert.IsFalse((await service.SystemValues.SaveAsync(stored)).IsValid);
        Assert.IsFalse((await service.SystemValues.DeleteAsync(stored.SystemValueId)).IsValid);
        CollectionAssert.AreEqual(new[] { meeting }, (await service.Summaries.GetNoisePolicyAsync()).Entity.ExcludedNoteTypeIds);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task TheRecount_OfAPolicyNotSavedYet_SaysWhy(string orm)
    {
        using var fixture = await SemanticIndexFixture.CreateAsync(orm);
        var service = fixture.Service;
        var meeting = await AddTypeAsync(fixture, "Meeting");
        var inbox = await AddFolderAsync(fixture, "Inbox");
        var child = await AddFolderAsync(fixture, "Inbox child", inbox);
        var sessions = await AddTypeAsync(fixture, KntConst.ChatSessionsTag);
        await fixture.AddNoteAsync("A meeting", noteTypeId: meeting);
        await fixture.AddNoteAsync("A quick note", folderId: inbox);
        await fixture.AddNoteAsync("In a subfolder", folderId: child);
        await AddTaggedNoteAsync(fixture, "Kept", inbox, "#knowledge");
        await AddTaggedNoteAsync(fixture, "Left out", fixture.HomeId, "draft #NoKnowledge");
        await fixture.AddNoteAsync("A conversation", noteTypeId: sessions);
        var before = (await service.Summaries.CountNoiseAsync(new NoisePolicyDto())).Entity;

        var count = (await service.Summaries.CountNoiseAsync(new NoisePolicyDto { ExcludedNoteTypeIds = { meeting }, ExcludedFolderIds = { inbox } })).Entity;

        Assert.AreEqual(before.Notes, count.Notes);
        Assert.AreEqual(1, count.ExcludedNoteType);
        Assert.AreEqual(1, count.ExcludedFolder, "the quick note; not the one of the subfolder");
        Assert.AreEqual(1, count.NoKnowledgeTag);
        Assert.AreEqual(1, count.KNoteNotes);
        Assert.AreEqual(1, count.KeptByKnowledgeTag);
        Assert.AreEqual(before.Out + 2, count.Out);
        Assert.IsEmpty((await service.Summaries.GetNoisePolicyAsync()).Entity.ExcludedNoteTypeIds, "counting saves nothing");
    }

    private static async Task<Guid> AddTypeAsync(SemanticIndexFixture fixture, string name)
    {
        var saved = await fixture.Service.NoteTypes.SaveAsync(new NoteTypeDto { Name = name });
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        return saved.Entity.NoteTypeId;
    }

    private static async Task<Guid> AddFolderAsync(SemanticIndexFixture fixture, string name, Guid? parentId = null)
    {
        var saved = await fixture.Service.Folders.SaveAsync(new FolderDto { Name = name, ParentId = parentId ?? fixture.HomeId });
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
        return saved.Entity.FolderId;
    }

    private static async Task AddTaggedNoteAsync(SemanticIndexFixture fixture, string topic, Guid folderId, string tags)
    {
        var note = (await fixture.Service.Notes.NewAsync()).Entity;
        note.Topic = topic;
        note.FolderId = folderId;
        note.Tags = tags;
        var saved = await fixture.Service.Notes.SaveAsync(note);
        Assert.IsTrue(saved.IsValid, saved.ErrorMessage);
    }
}
