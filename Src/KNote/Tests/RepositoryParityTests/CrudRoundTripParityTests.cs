using KNote.Model.Dto;
using KNote.Tests.Helpers;

namespace KNote.Tests.RepositoryParityTests;

/// <summary>
/// Cross-ORM round-trip tests: write an entity with one repository implementation, read it back
/// with the other, against the SAME physical Sqlite file. This targets persistence-level/wire-
/// format drift (dates, GUIDs, nulls, defaults) - not just "does each ORM independently satisfy
/// the contract" (see NotesQueryParityTests for that kind of behavioral parity instead).
///
/// Each [DataRow] pair is run in both directions (Dapper writes/EF reads, and vice versa) because
/// a mismatch could plausibly be one-directional.
///
/// Every entity's id is assigned explicitly before AddAsync: neither repository implementation
/// generates it (confirmed by writing this suite - both silently persist Guid.Empty otherwise).
/// Id generation is the Service layer's responsibility (see e.g. KntFolderCommands.cs: "if
/// (Param.FolderId == Guid.Empty) Param.FolderId = Guid.NewGuid();"), which these tests
/// deliberately bypass to exercise the Repository layer directly.
/// </summary>
[TestClass]
public class CrudRoundTripParityTests
{
    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task Folder_WrittenByOneOrm_IsReadableByTheOther(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);

        var folderId = Guid.NewGuid();
        var folder = new FolderDto { FolderId = folderId, FolderNumber = 0, Name = "Parity Folder", ParentId = null };

        var addRes = await writer.Folders.AddAsync(folder);
        Assert.IsTrue(addRes.IsValid, addRes.ErrorMessage);
        Assert.AreEqual(folderId, addRes.Entity.FolderId);

        var getRes = await reader.Folders.GetAsync(folderId);
        Assert.IsTrue(getRes.IsValid, getRes.ErrorMessage);
        Assert.AreEqual(addRes.Entity.Name, getRes.Entity.Name);
        Assert.AreEqual(addRes.Entity.FolderNumber, getRes.Entity.FolderNumber);
    }

    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task NoteType_WrittenByOneOrm_IsReadableByTheOther(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);

        var noteTypeId = Guid.NewGuid();
        var noteType = new NoteTypeDto { NoteTypeId = noteTypeId, Name = "Parity NoteType", Description = "Parity description" };

        var addRes = await writer.NoteTypes.AddAsync(noteType);
        Assert.IsTrue(addRes.IsValid, addRes.ErrorMessage);
        Assert.AreEqual(noteTypeId, addRes.Entity.NoteTypeId);

        var getRes = await reader.NoteTypes.GetAsync(noteTypeId);
        Assert.IsTrue(getRes.IsValid, getRes.ErrorMessage);
        Assert.AreEqual(addRes.Entity.Name, getRes.Entity.Name);
        Assert.AreEqual(addRes.Entity.Description, getRes.Entity.Description);
    }

    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task KAttribute_WrittenByOneOrm_IsReadableByTheOther(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);

        var kAttributeId = Guid.NewGuid();
        var kAttribute = new KAttributeDto { KAttributeId = kAttributeId, Name = "Parity KAttribute", Description = "Parity description" };

        var addRes = await writer.KAttributes.AddAsync(kAttribute);
        Assert.IsTrue(addRes.IsValid, addRes.ErrorMessage);
        Assert.AreEqual(kAttributeId, addRes.Entity.KAttributeId);

        var getRes = await reader.KAttributes.GetAsync(kAttributeId);
        Assert.IsTrue(getRes.IsValid, getRes.ErrorMessage);
        Assert.AreEqual(addRes.Entity.Name, getRes.Entity.Name);
        Assert.AreEqual(addRes.Entity.Description, getRes.Entity.Description);
    }

    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task SystemValue_WrittenByOneOrm_IsReadableByTheOther(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);

        var systemValueId = Guid.NewGuid();
        var systemValue = new SystemValueDto { SystemValueId = systemValueId, Scope = "PARITY_SCOPE", Key = "PARITY_KEY", Value = "parity-value" };

        var addRes = await writer.SystemValues.AddAsync(systemValue);
        Assert.IsTrue(addRes.IsValid, addRes.ErrorMessage);
        Assert.AreEqual(systemValueId, addRes.Entity.SystemValueId);

        var getRes = await reader.SystemValues.GetAsync(systemValueId);
        Assert.IsTrue(getRes.IsValid, getRes.ErrorMessage);
        Assert.AreEqual(addRes.Entity.Value, getRes.Entity.Value);
    }

    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task Note_WrittenByOneOrm_IsReadableByTheOther(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);

        // The folder FK is created with the writer ORM too, so the note's FolderId is guaranteed
        // to exist regardless of which ORM's repository ends up reading it.
        var folderRes = await writer.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Parity Note Folder" });
        Assert.IsTrue(folderRes.IsValid, folderRes.ErrorMessage);

        var noteId = Guid.NewGuid();
        var note = new NoteDto
        {
            NoteId = noteId,
            Topic = "Parity Note",
            Description = "Parity note description",
            FolderId = folderRes.Entity.FolderId,
            CreationDateTime = DateTime.Now,
            ModificationDateTime = DateTime.Now
        };

        var addRes = await writer.Notes.AddAsync(note);
        Assert.IsTrue(addRes.IsValid, addRes.ErrorMessage);
        Assert.AreEqual(noteId, addRes.Entity.NoteId);

        var getRes = await reader.Notes.GetAsync(noteId);
        Assert.IsTrue(getRes.IsValid, getRes.ErrorMessage);
        Assert.AreEqual(addRes.Entity.Topic, getRes.Entity.Topic);
        Assert.AreEqual(addRes.Entity.Description, getRes.Entity.Description);
        Assert.AreEqual(addRes.Entity.FolderId, getRes.Entity.FolderId);
    }
}
