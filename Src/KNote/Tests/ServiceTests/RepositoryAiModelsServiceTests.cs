using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// The repository's AI models (KNote Atlas, SystemValues of scope AI) through the Service layer: anybody reads them,
/// only an Admin saves them, an empty value leaves no row, the generic SystemValues commands cannot touch them, and
/// reading them says whether this installation has the embedding model.
/// </summary>
[TestClass]
public class RepositoryAiModelsServiceTests
{
    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ARepository_WithoutModels_ReadsThemEmpty(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository(orm));

        var res = await service.SystemValues.GetRepositoryAiModelsAsync();

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        Assert.IsNull(res.Entity.EmbeddingModel);
        Assert.IsNull(res.Entity.EmbeddingDimensions);
        Assert.IsNull(res.Entity.QueryInstruction);
        Assert.IsNull(res.Entity.LlmModel);
        Assert.IsFalse(res.Entity.EmbeddingModelAvailable);
        Assert.IsFalse(res.Entity.IsDirty());
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task SaveRepositoryAiModels_KeepsThemTrimmed(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository(orm));

        var res = await service.SystemValues.SaveRepositoryAiModelsAsync(new RepositoryAiModelsDto
        {
            EmbeddingModel = "  qwen3-embedding:0.6b ",
            EmbeddingDimensions = 512,
            QueryInstruction = " Given a search query, retrieve the notes that answer it ",
            LlmModel = "qwen3.5:9b"
        });

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        var read = (await new KntService(db.CreateRepository(orm)).SystemValues.GetRepositoryAiModelsAsync()).Entity;
        Assert.AreEqual("qwen3-embedding:0.6b", read.EmbeddingModel);
        Assert.AreEqual(512, read.EmbeddingDimensions);
        Assert.AreEqual("Given a search query, retrieve the notes that answer it", read.QueryInstruction);
        Assert.AreEqual("qwen3.5:9b", read.LlmModel);
        Assert.AreEqual("qwen3-embedding:0.6b", (await SystemValueAsync(service, KntConst.AiEmbedModelKey))?.Value);
    }

    [TestMethod]
    public async Task SaveRepositoryAiModels_EmptyValues_LeaveNoRows()
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository("Dapper"));
        await service.SystemValues.SaveRepositoryAiModelsAsync(new RepositoryAiModelsDto
        {
            EmbeddingModel = "qwen3-embedding:0.6b", EmbeddingDimensions = 512, QueryInstruction = "Search", LlmModel = "qwen3.5:9b"
        });

        var res = await service.SystemValues.SaveRepositoryAiModelsAsync(new RepositoryAiModelsDto { LlmModel = "  " });

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        Assert.IsNull(res.Entity.EmbeddingModel);
        Assert.IsNull(res.Entity.LlmModel);
        var rows = (await service.SystemValues.GetAllAsync()).Entity.Where(v => v.Scope == KntConst.AiScope).ToList();
        Assert.AreEqual(0, rows.Count);
    }

    [TestMethod]
    public async Task SaveRepositoryAiModels_RefusesInvalidModels()
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository("Dapper"));

        var res = await service.SystemValues.SaveRepositoryAiModelsAsync(new RepositoryAiModelsDto { EmbeddingDimensions = 512 });
        var resNull = await service.SystemValues.SaveRepositoryAiModelsAsync(null!);

        Assert.IsFalse(res.IsValid);
        Assert.IsFalse(resNull.IsValid);
        Assert.IsNull(await SystemValueAsync(service, KntConst.AiEmbedDimsKey));
    }

    [TestMethod]
    public async Task GetRepositoryAiModels_SaysWhetherThisInstallationHasTheEmbeddingModel()
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository("Dapper"));
        await service.SystemValues.SaveRepositoryAiModelsAsync(new RepositoryAiModelsDto { EmbeddingModel = "qwen3-embedding:0.6b" });

        service.EmbeddingGenerators = null;
        Assert.IsFalse((await service.SystemValues.GetRepositoryAiModelsAsync()).Entity.EmbeddingModelAvailable);

        service.EmbeddingGenerators = new FakeEmbeddingGenerators("qwen3-embedding:4b");
        Assert.IsFalse((await service.SystemValues.GetRepositoryAiModelsAsync()).Entity.EmbeddingModelAvailable);

        service.EmbeddingGenerators = new FakeEmbeddingGenerators("QWEN3-EMBEDDING:0.6B");
        Assert.IsTrue((await service.SystemValues.GetRepositoryAiModelsAsync()).Entity.EmbeddingModelAvailable);
    }

    [TestMethod]
    [DataRow("nobody", null)]
    [DataRow("guest", "Guest")]
    public async Task GetRepositoryAiModels_IsReadableByAnyone(string userName, string? roleDefinition)
    {
        using var db = new RepositoryTestDatabase();
        var service = EnforcedService(db, userName);
        if (roleDefinition != null)
            await AddUserAsync(service, userName, roleDefinition);

        var res = await service.SystemValues.GetRepositoryAiModelsAsync();

        // Guest is the least role: an unregistered user is refused, as by every Guest command.
        Assert.AreEqual(roleDefinition != null, res.IsValid, res.ErrorMessage);
    }

    [TestMethod]
    [DataRow("pm", "ProjectManager", false)]
    [DataRow("admin", "Admin", true)]
    public async Task SaveRepositoryAiModels_NeedsAdmin(string userName, string roleDefinition, bool allowed)
    {
        using var db = new RepositoryTestDatabase();
        var service = EnforcedService(db, userName);
        await AddUserAsync(service, userName, roleDefinition);

        var res = await service.SystemValues.SaveRepositoryAiModelsAsync(new RepositoryAiModelsDto { EmbeddingModel = "qwen3-embedding:0.6b" });

        Assert.AreEqual(allowed, res.IsValid, res.ErrorMessage);
        Assert.AreEqual(!allowed, res.NotAuthorized);
        Assert.AreEqual(allowed ? "qwen3-embedding:0.6b" : null, (await SystemValueAsync(service, KntConst.AiEmbedModelKey))?.Value);
    }

    [TestMethod]
    public async Task GenericSystemValueCommands_CannotChangeOrDeleteTheAiModels()
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository("Dapper"));
        await service.SystemValues.SaveRepositoryAiModelsAsync(new RepositoryAiModelsDto { EmbeddingModel = "qwen3-embedding:0.6b" });
        var stored = await SystemValueAsync(service, KntConst.AiEmbedModelKey);

        var update = await service.SystemValues.SaveAsync(new SystemValueDto
        {
            SystemValueId = stored!.SystemValueId, Scope = KntConst.AiScope, Key = KntConst.AiEmbedModelKey, Value = "another-model"
        });
        var insertAnother = await service.SystemValues.SaveAsync(new SystemValueDto
        {
            Scope = "ai", Key = KntConst.AiLlmModelKey, Value = "another-model"
        });
        var delete = await service.SystemValues.DeleteAsync(stored.SystemValueId);
        Action script = () => service.SaveSystemVariable(KntConst.AiScope, KntConst.AiEmbedModelKey, "another-model");

        Assert.IsFalse(update.IsValid);
        Assert.IsFalse(insertAnother.IsValid);
        Assert.IsFalse(delete.IsValid);
        Assert.ThrowsExactly<KntServiceException>(script);
        Assert.AreEqual("qwen3-embedding:0.6b", (await SystemValueAsync(service, KntConst.AiEmbedModelKey))?.Value);
    }

    private static async Task<SystemValueDto?> SystemValueAsync(KntService service, string key)
    {
        var res = await service.Repository.SystemValues.GetAsync(KntConst.AiScope, key);
        return res.IsValid ? res.Entity : null;
    }

    private static KntService EnforcedService(RepositoryTestDatabase db, string userName)
        => new(db.CreateRepository("Dapper")) { UserIdentityName = userName, EnforceAuthorization = true };

    // Straight through the repository: commands would need an Admin to create users.
    private static async Task AddUserAsync(KntService service, string userName, string roleDefinition)
    {
        var res = await service.Repository.Users.AddAsync(new UserDto
        {
            UserId = Guid.NewGuid(),
            UserName = userName,
            EMail = $"{userName}-{Guid.NewGuid():N}@knote.tests",
            FullName = userName,
            RoleDefinition = roleDefinition
        });
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
    }
}
