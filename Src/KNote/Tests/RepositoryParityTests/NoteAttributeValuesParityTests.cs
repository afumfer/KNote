using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository;
using KNote.Tests.Helpers;

namespace KNote.Tests.RepositoryParityTests;

/// <summary>
/// GetAttributeValuesAsync (the index «By concept» of a period summary): the values, not empty, of the attributes asked for in every
/// note, the same with both ORMs, written with one and read with the other.
/// </summary>
[TestClass]
public class NoteAttributeValuesParityTests
{
    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task TheValuesOfTheAttributesAskedFor_NotEmpty(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);
        var folder = await writer.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = $"Concepts {Guid.NewGuid():N}" });
        Assert.IsTrue(folder.IsValid, folder.ErrorMessage);
        var status = await AddAttributeAsync(writer, "Status");
        var planning = await AddAttributeAsync(writer, "Planning");
        var other = await AddAttributeAsync(writer, "Other");
        var first = await AddNoteAsync(writer, folder.Entity.FolderId, (status, "Open"), (planning, "Q4"), (other, "x"));
        var second = await AddNoteAsync(writer, folder.Entity.FolderId, (status, ""), (planning, "Q1"));

        var rows = (await reader.Notes.GetAttributeValuesAsync(new[] { status, planning })).Entity;

        Assert.AreEqual("Open, Q4", string.Join(", ", rows.Where(r => r.NoteId == first).OrderBy(r => r.Value).Select(r => r.Value)));
        Assert.AreEqual(planning, rows.Single(r => r.NoteId == second).KAttributeId, "the empty value is left out");
        Assert.IsFalse(rows.Any(r => r.KAttributeId == other));
        Assert.IsEmpty((await reader.Notes.GetAttributeValuesAsync(Array.Empty<Guid>())).Entity);
    }

    private static async Task<Guid> AddAttributeAsync(IKntRepository repository, string name)
    {
        var attribute = await repository.KAttributes.AddAsync(new KAttributeDto
        {
            KAttributeId = Guid.NewGuid(), Name = $"{name} {Guid.NewGuid():N}", KAttributeDataType = EnumKAttributeDataType.String
        });
        Assert.IsTrue(attribute.IsValid, attribute.ErrorMessage);
        return attribute.Entity.KAttributeId;
    }

    private static async Task<Guid> AddNoteAsync(IKntRepository repository, Guid folderId, params (Guid Attribute, string Value)[] values)
    {
        var noteId = Guid.NewGuid();
        var note = await repository.Notes.AddAsync(new NoteDto
        {
            NoteId = noteId, Topic = $"Note {noteId:N}", FolderId = folderId,
            KAttributesDto = values.Select(v => new NoteKAttributeDto { NoteKAttributeId = Guid.NewGuid(), NoteId = noteId, KAttributeId = v.Attribute, Value = v.Value }).ToList()
        });
        Assert.IsTrue(note.IsValid, note.ErrorMessage);
        return noteId;
    }
}
