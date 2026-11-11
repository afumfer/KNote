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
    public async Task TraceNoteType_WrittenByOneOrm_IsReadableByTheOther(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);

        var traceNoteTypeId = Guid.NewGuid();
        var traceNoteType = new TraceNoteTypeDto { TraceNoteTypeId = traceNoteTypeId, Name = "Parity TraceNoteType", Description = "Parity description" };

        var addRes = await writer.TraceNoteTypes.AddAsync(traceNoteType);
        Assert.IsTrue(addRes.IsValid, addRes.ErrorMessage);
        Assert.AreEqual(traceNoteTypeId, addRes.Entity.TraceNoteTypeId);

        var getRes = await reader.TraceNoteTypes.GetAsync(traceNoteTypeId);
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

    // Who wrote the description of a resource (KntSchemaUpdater revision 9): written and changed by one ORM, read by the other.
    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task ResourceDescriptionLock_WrittenByOneOrm_IsReadByTheOther(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);

        var folderRes = await writer.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Parity Resources Folder" });
        var noteId = Guid.NewGuid();
        var noteRes = await writer.Notes.AddAsync(new NoteDto { NoteId = noteId, Topic = "Parity resources", FolderId = folderRes.Entity.FolderId, CreationDateTime = DateTime.Now, ModificationDateTime = DateTime.Now });
        Assert.IsTrue(noteRes.IsValid, noteRes.ErrorMessage);

        var resource = new ResourceDto { ResourceId = Guid.NewGuid(), NoteId = noteId, Name = "photo.png", Container = "R", Description = "A QL.", FileType = "image/png" };
        var added = await writer.Notes.AddResourceAsync(resource);
        Assert.IsTrue(added.IsValid, added.ErrorMessage);

        var read = (await reader.Notes.GetResourceAsync(resource.ResourceId)).Entity;
        Assert.IsFalse(read.DescriptionByUser);
        Assert.IsNull(read.DescriptionModel);

        resource.DescriptionByUser = true;
        resource.DescriptionModel = "qwen3.5:9b";
        Assert.IsTrue((await writer.Notes.UpdateResourceAsync(resource)).IsValid);

        var list = (await reader.Notes.GetResourcesAsync(noteId)).Entity;
        Assert.HasCount(1, list);
        Assert.IsTrue(list[0].DescriptionByUser);
        Assert.AreEqual("qwen3.5:9b", list[0].DescriptionModel);
    }

    // The description of the AI (KNote Atlas, 4.2) is written only if the resource is as it was read and not locked.
    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ResourceAiDescription_IsWrittenOnlyIfNothingChanged(string orm)
    {
        using var db = new RepositoryTestDatabase();
        using var repository = db.CreateRepository(orm);

        var folderRes = await repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "AI descriptions" });
        var noteId = Guid.NewGuid();
        Assert.IsTrue((await repository.Notes.AddAsync(new NoteDto { NoteId = noteId, Topic = "AI descriptions", FolderId = folderRes.Entity.FolderId, CreationDateTime = DateTime.Now, ModificationDateTime = DateTime.Now })).IsValid);

        async Task<Guid> AddAsync(string description, bool locked)
        {
            var resource = new ResourceDto { ResourceId = Guid.NewGuid(), NoteId = noteId, Name = $"{Guid.NewGuid()}.png", Container = "R", Description = description, DescriptionByUser = locked, FileType = "image/png" };
            Assert.IsTrue((await repository.Notes.AddResourceAsync(resource)).IsValid);
            return resource.ResourceId;
        }
        async Task<ResourceDto> GetAsync(Guid id) => (await repository.Notes.GetResourceAsync(id)).Entity;

        var free = await AddAsync("photo.png", false);
        var empty = await AddAsync(null, false);
        var locked = await AddAsync("photo.png", true);
        var changed = await AddAsync("A person's text.", false);

        Assert.IsTrue((await repository.Notes.SaveResourceAiDescriptionAsync(free, "A QL.", "qwen3.5:9b", "photo.png", (await GetAsync(free)).Name)).Entity);
        Assert.IsTrue((await repository.Notes.SaveResourceAiDescriptionAsync(empty, "A QL.", "qwen3.5:9b", null, (await GetAsync(empty)).Name)).Entity);
        Assert.IsFalse((await repository.Notes.SaveResourceAiDescriptionAsync(locked, "A QL.", "qwen3.5:9b", "photo.png", (await GetAsync(locked)).Name)).Entity);
        Assert.IsFalse((await repository.Notes.SaveResourceAiDescriptionAsync(changed, "A QL.", "qwen3.5:9b", "photo.png", (await GetAsync(changed)).Name)).Entity);
        Assert.IsFalse((await repository.Notes.SaveResourceAiDescriptionAsync(changed, "A QL.", "qwen3.5:9b", "A person's text.", "another-file.png")).Entity);

        Assert.AreEqual("qwen3.5:9b", (await GetAsync(free)).DescriptionModel);
        Assert.AreEqual("A QL.", (await GetAsync(empty)).Description);
        Assert.AreEqual("photo.png", (await GetAsync(locked)).Description);
        Assert.IsNull((await GetAsync(locked)).DescriptionModel);
        Assert.AreEqual("A person's text.", (await GetAsync(changed)).Description);
    }

    // The text read of a document (KNote Atlas, 4.4a, D2): what one ORM writes the other reads, only while the resource has
    // that file; saving the resource (as a note with its resources does) keeps it, and forgetting it leaves it unread.
    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task TheTextOfADocument_IsKeptApart_AndSavingTheResourceKeepsIt(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);

        var folderRes = await writer.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Documents" });
        var noteId = Guid.NewGuid();
        Assert.IsTrue((await writer.Notes.AddAsync(new NoteDto { NoteId = noteId, Topic = "A manual", FolderId = folderRes.Entity.FolderId,
            CreationDateTime = DateTime.Now, ModificationDateTime = DateTime.Now })).IsValid);
        var resource = new ResourceDto { ResourceId = Guid.NewGuid(), NoteId = noteId, Name = $"{Guid.NewGuid()}_manual.pdf", Container = "R",
            Description = "manual.pdf", FileType = "application/pdf" };
        Assert.IsTrue((await writer.Notes.AddResourceAsync(resource)).IsValid);
        var text = "Page one\f\u0001OCR of page two";

        Assert.IsFalse((await writer.Notes.SaveResourceTextAsync(resource.ResourceId, text, EnumExtractedState.Partial, "another.pdf")).Entity,
            "its file was replaced meanwhile");
        Assert.AreEqual(EnumExtractedState.NotRead, (await reader.Notes.GetResourceTextAsync(resource.ResourceId)).Entity.State);
        Assert.IsTrue((await writer.Notes.SaveResourceTextAsync(resource.ResourceId, text, EnumExtractedState.Partial, resource.Name)).Entity);

        var read = (await reader.Notes.GetResourceTextAsync(resource.ResourceId)).Entity;
        Assert.AreEqual((resource.ResourceId, noteId, "application/pdf", EnumExtractedState.Partial, text),
            (read.ResourceId, read.NoteId, read.FileType, read.State, read.Text));
        Assert.AreEqual(text, (await reader.Notes.GetResourceTextsAsync(noteId)).Entity.Single().Text);

        // Saving the resource with the other ORM, as saving its note does: the text stays.
        var stored = (await reader.Notes.GetResourceAsync(resource.ResourceId)).Entity;
        stored.Description = "The manual of the QL.";
        Assert.IsTrue((await reader.Notes.UpdateResourceAsync(stored)).IsValid);
        Assert.AreEqual(text, (await writer.Notes.GetResourceTextAsync(resource.ResourceId)).Entity.Text);

        Assert.IsTrue((await writer.Notes.ClearResourceTextAsync(resource.ResourceId)).IsValid);
        var cleared = (await reader.Notes.GetResourceTextAsync(resource.ResourceId)).Entity;
        Assert.AreEqual((EnumExtractedState.NotRead, (string?)null), (cleared.State, cleared.Text));
        Assert.IsNull((await reader.Notes.GetResourceTextAsync(Guid.NewGuid())).Entity);
    }

    // «Process pending resources» (KNote Atlas, 4.3): the resources the AI could describe, without their content nor the ones of
    // the excluded note types, and locking a description only if it is still the same, not locked and not the AI's.
    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ResourcesToDescribe_AndLockingADescription(string orm)
    {
        using var db = new RepositoryTestDatabase();
        using var repository = db.CreateRepository(orm);

        var folderRes = await repository.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Resources to describe" });
        var excludedTypeId = Guid.NewGuid();
        Assert.IsTrue((await repository.NoteTypes.AddAsync(new NoteTypeDto { NoteTypeId = excludedTypeId, Name = "@Excluded" })).IsValid);
        async Task<Guid> AddNoteAsync(Guid? noteTypeId)
        {
            var noteId = Guid.NewGuid();
            Assert.IsTrue((await repository.Notes.AddAsync(new NoteDto
            {
                NoteId = noteId, Topic = "Resources", FolderId = folderRes.Entity.FolderId, NoteTypeId = noteTypeId,
                CreationDateTime = DateTime.Now, ModificationDateTime = DateTime.Now
            })).IsValid);
            return noteId;
        }
        var noteId = await AddNoteAsync(null);
        var excludedNoteId = await AddNoteAsync(excludedTypeId);

        async Task<Guid> AddAsync(Guid note, string description, bool locked = false, string? model = null)
        {
            var resource = new ResourceDto
            {
                ResourceId = Guid.NewGuid(), NoteId = note, Name = $"{Guid.NewGuid()}.png", Container = "R", Description = description,
                DescriptionByUser = locked, DescriptionModel = model, FileType = "image/png", ContentInDB = true, ContentArrayBytes = [1, 2, 3]
            };
            Assert.IsTrue((await repository.Notes.AddResourceAsync(resource)).IsValid);
            return resource.ResourceId;
        }

        var pending = await AddAsync(noteId, "photo.png");
        var person = await AddAsync(noteId, "A person's text.");
        await AddAsync(noteId, "Mine.", locked: true);
        var byTheAi = await AddAsync(noteId, "A QL.", model: "qwen3.5:9b");
        var ofExcluded = await AddAsync(excludedNoteId, "photo.png");

        var all = (await repository.Notes.GetResourcesToDescribeAsync()).Entity;
        var notExcluded = (await repository.Notes.GetResourcesToDescribeAsync([excludedTypeId])).Entity;

        CollectionAssert.AreEquivalent(new[] { pending, person, ofExcluded }, all.Select(r => r.ResourceId).ToList());
        CollectionAssert.AreEquivalent(new[] { pending, person }, notExcluded.Select(r => r.ResourceId).ToList());
        var read = notExcluded.Single(r => r.ResourceId == person);
        Assert.AreEqual("A person's text.", read.Description);
        Assert.AreEqual(noteId, read.NoteId);
        Assert.AreEqual("image/png", read.FileType);
        Assert.IsInstanceOfType<ResourceInfoDto>(read);
        Assert.IsNotInstanceOfType<ResourceDto>(read, "without the content");

        Assert.IsFalse((await repository.Notes.LockResourceDescriptionAsync(person, "Another text.")).Entity, "it changed");
        Assert.IsFalse((await repository.Notes.LockResourceDescriptionAsync(byTheAi, "A QL.")).Entity, "the AI's is not a person's");
        Assert.IsTrue((await repository.Notes.LockResourceDescriptionAsync(person, "A person's text.")).Entity);
        Assert.IsFalse((await repository.Notes.LockResourceDescriptionAsync(person, "A person's text.")).Entity, "already locked");
        Assert.IsTrue((await repository.Notes.GetResourceAsync(person)).Entity.DescriptionByUser);
        Assert.IsFalse((await repository.Notes.GetResourceAsync(byTheAi)).Entity.DescriptionByUser);
    }

    // An alarm of a user (with the user's name) and one without a user: both listed and read by either ORM.
    [TestMethod]
    [DataRow("Dapper", "EntityFramework")]
    [DataRow("EntityFramework", "Dapper")]
    public async Task Alarms_WithAndWithoutAUser_AreReadByTheOther(string writerOrm, string readerOrm)
    {
        using var db = new RepositoryTestDatabase();
        using var writer = db.CreateRepository(writerOrm);
        using var reader = db.CreateRepository(readerOrm);

        var folderRes = await writer.Folders.AddAsync(new FolderDto { FolderId = Guid.NewGuid(), FolderNumber = 0, Name = "Parity Alarms Folder" });
        var noteId = Guid.NewGuid();
        var noteRes = await writer.Notes.AddAsync(new NoteDto { NoteId = noteId, Topic = "Parity alarms", FolderId = folderRes.Entity.FolderId, CreationDateTime = DateTime.Now, ModificationDateTime = DateTime.Now });
        Assert.IsTrue(noteRes.IsValid, noteRes.ErrorMessage);
        var user = (await writer.Users.GetAllAsync()).Entity.First();

        var withUser = new KMessageDto { KMessageId = Guid.NewGuid(), NoteId = noteId, UserId = user.UserId, Comment = "With user", AlarmActivated = true, AlarmDateTime = DateTime.Now };
        var withoutUser = new KMessageDto { KMessageId = Guid.NewGuid(), NoteId = noteId, UserId = null, Comment = "Without user", AlarmActivated = true, AlarmDateTime = DateTime.Now.AddHours(1) };
        Assert.IsTrue((await writer.Notes.AddMessageAsync(withUser)).IsValid);
        Assert.IsTrue((await writer.Notes.AddMessageAsync(withoutUser)).IsValid);

        var list = await reader.Notes.GetMessagesAsync(noteId);
        var one = await reader.Notes.GetMessageAsync(withUser.KMessageId);
        var other = await reader.Notes.GetMessageAsync(withoutUser.KMessageId);

        Assert.IsTrue(list.IsValid, list.ErrorMessage);
        Assert.HasCount(2, list.Entity);
        Assert.AreEqual(user.FullName, list.Entity.Single(m => m.KMessageId == withUser.KMessageId).UserFullName);
        Assert.IsNull(list.Entity.Single(m => m.KMessageId == withoutUser.KMessageId).UserFullName);
        Assert.AreEqual(user.FullName, one.Entity.UserFullName);
        Assert.IsTrue(other.IsValid, other.ErrorMessage);
        Assert.IsNull(other.Entity.UserId);
    }
}
