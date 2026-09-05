using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// Schema revision 3 widened KAttributes' unique index from (Name) to (Name, NoteTypeId) - see
/// ModelBuilderExtensions/KntSchemaUpdater - which stopped rejecting duplicate Names among *global*
/// attributes (NoteTypeId == null), since SQL Server/Sqlite treat each NULL as distinct in a unique
/// index. KntKAttributesSaveAsyncCommand closes that one gap itself, at the Service layer, mirroring
/// how KntCommandSaveServiceBase.ValidateParam()/TraceNoteDto.Validate() reject a self-loop before it
/// ever reaches the repository - here the check needs a repository read, so it lives in the command's
/// Execute() instead of the ORM-agnostic DTO.Validate().
/// </summary>
[TestClass]
public class KAttributesServiceTests
{
    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task SaveAsync_DuplicateNameAmongGlobalAttributes_IsRejected(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var repo = db.CreateRepository(orm);
        var service = new KntService(repo, activateMessageBroker: false);

        var first = await service.KAttributes.SaveAsync(new KAttributeDto { Name = "Priority" });
        Assert.IsTrue(first.IsValid, first.ErrorMessage);

        var second = await service.KAttributes.SaveAsync(new KAttributeDto { Name = "Priority" });

        Assert.IsFalse(second.IsValid);
        Assert.IsTrue(second.ErrorMessage.Contains("already exists", StringComparison.OrdinalIgnoreCase), second.ErrorMessage);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task SaveAsync_UpdatingTheSameGlobalAttributeWithItsOwnName_IsAllowed(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var repo = db.CreateRepository(orm);
        var service = new KntService(repo, activateMessageBroker: false);

        var added = await service.KAttributes.SaveAsync(new KAttributeDto { Name = "Priority" });
        Assert.IsTrue(added.IsValid, added.ErrorMessage);

        added.Entity.Description = "Updated description";
        var updated = await service.KAttributes.SaveAsync(added.Entity); // same Name, same KAttributeId

        Assert.IsTrue(updated.IsValid, updated.ErrorMessage);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task SaveAsync_SameNameScopedToDifferentNoteTypes_IsAllowed(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var repo = db.CreateRepository(orm);
        var service = new KntService(repo, activateMessageBroker: false);

        var typeA = await service.Repository.NoteTypes.AddAsync(new NoteTypeDto { NoteTypeId = Guid.NewGuid(), Name = $"Type A {Guid.NewGuid():N}" });
        Assert.IsTrue(typeA.IsValid, typeA.ErrorMessage);
        var typeB = await service.Repository.NoteTypes.AddAsync(new NoteTypeDto { NoteTypeId = Guid.NewGuid(), Name = $"Type B {Guid.NewGuid():N}" });
        Assert.IsTrue(typeB.IsValid, typeB.ErrorMessage);

        var first = await service.KAttributes.SaveAsync(new KAttributeDto { Name = "Priority", NoteTypeId = typeA.Entity.NoteTypeId });
        Assert.IsTrue(first.IsValid, first.ErrorMessage);

        // Same Name, but scoped to a different NoteTypeId - this is exactly what revision 3's index
        // change was meant to allow.
        var second = await service.KAttributes.SaveAsync(new KAttributeDto { Name = "Priority", NoteTypeId = typeB.Entity.NoteTypeId });
        Assert.IsTrue(second.IsValid, second.ErrorMessage);
    }
}
