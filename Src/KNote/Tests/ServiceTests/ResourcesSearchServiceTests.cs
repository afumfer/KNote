using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// «Search in resources» through the service (KNote Atlas, stage 4, D18, 4.6a): the search and the filter find a note by the text of
/// its resources, but never by the resources of the system notes (the attachments of the conversations of the assistant): the
/// service leaves them out by itself (ResourcesExcludedNoteTypeIds), whoever calls it. Those notes are found by their topic, as before.
/// </summary>
[TestClass]
public class ResourcesSearchServiceTests
{
    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task TheResourcesOfTheSystemNotes_AreNotSearched(string orm)
    {
        using var db = new RepositoryTestDatabase();
        using var service = new KntService(db.CreateRepository(orm), activateMessageBroker: false);
        var folder = (await service.Repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Search" })).Entity;
        var sessionsType = (await service.Repository.NoteTypes.AddAsync(new NoteTypeDto
        {
            NoteTypeId = Guid.NewGuid(), Name = KntConst.SystemNoteTypeNames[0]
        })).Entity;
        var marker = $"SECRET{Guid.NewGuid():N}"[..16];

        var note = await AddNoteAsync(service, folder.FolderId, "A manual", null);
        await AddResourceAsync(service, note, $"The text with {marker}.");
        var session = await AddNoteAsync(service, folder.FolderId, "A conversation", sessionsType.NoteTypeId);
        await AddResourceAsync(service, session, $"An attachment with {marker}.");

        var searched = await service.Notes.GetSearchMinimalAsync(new NotesSearchDto { TextSearch = marker, SearchInResources = true });
        var filtered = await service.Notes.GetFilterMinimalAsync(new NotesFilterDto { Description = marker, SearchInResources = true });
        var byTopic = await service.Notes.GetSearchMinimalAsync(new NotesSearchDto { TextSearch = "conversation", SearchInResources = true });

        Assert.IsTrue(searched.IsValid, searched.ErrorMessage);
        CollectionAssert.AreEqual(new[] { note }, searched.Entity.Select(n => n.NoteId).ToList());
        CollectionAssert.AreEqual(new[] { note }, filtered.Entity.Select(n => n.NoteId).ToList());
        CollectionAssert.Contains(byTopic.Entity.Select(n => n.NoteId).ToList(), session);
    }

    private static async Task<Guid> AddNoteAsync(KntService service, Guid folderId, string topic, Guid? noteTypeId)
    {
        var added = await service.Repository.Notes.AddAsync(new NoteDto
        {
            NoteId = Guid.NewGuid(), Topic = topic, FolderId = folderId, NoteTypeId = noteTypeId, CreationDateTime = DateTime.Now, ModificationDateTime = DateTime.Now
        });
        Assert.IsTrue(added.IsValid, added.ErrorMessage);
        return added.Entity.NoteId;
    }

    private static async Task AddResourceAsync(KntService service, Guid noteId, string text)
    {
        var resource = await service.Repository.Notes.AddResourceAsync(new ResourceDto
        {
            ResourceId = Guid.NewGuid(), NoteId = noteId, Name = $"{Guid.NewGuid()}_doc.pdf", Description = "doc.pdf", ContentInDB = true,
            ContentArrayBytes = [1], FileType = "application/pdf", Container = "KntResources"
        });
        Assert.IsTrue(resource.IsValid, resource.ErrorMessage);
        Assert.IsTrue((await service.Repository.Notes.SaveResourceTextAsync(resource.Entity.ResourceId, text, EnumExtractedState.Complete, resource.Entity.Name)).Entity);
    }
}
