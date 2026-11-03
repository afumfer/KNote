using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// RepositoryAiModelsDto (KNote Atlas): what a repository can declare, and when two declarations have the same
/// embedding model (another one means building the semantic index again).
/// </summary>
[TestClass]
public class RepositoryAiModelsDtoTests
{
    [TestMethod]
    public void NoModels_IsValid()
    {
        Assert.IsTrue(new RepositoryAiModelsDto().IsValid());
    }

    [TestMethod]
    public void AModelWithItsSettings_IsValid()
    {
        var models = new RepositoryAiModelsDto
        {
            EmbeddingModel = "qwen3-embedding:0.6b",
            EmbeddingDimensions = 512,
            QueryInstruction = "Given a search query, retrieve the notes that answer it",
            LlmModel = "qwen3.5:9b"
        };

        Assert.IsTrue(models.IsValid(), models.GetErrorMessage());
    }

    [TestMethod]
    [DataRow(RepositoryAiModelsDto.MinEmbeddingDimensions - 1)]
    [DataRow(RepositoryAiModelsDto.MaxEmbeddingDimensions + 1)]
    public void DimensionsOutOfRange_AreRefused(int dimensions)
    {
        var models = new RepositoryAiModelsDto { EmbeddingModel = "qwen3-embedding:0.6b", EmbeddingDimensions = dimensions };

        Assert.IsFalse(models.IsValid());
    }

    [TestMethod]
    public void DimensionsOrAQueryInstruction_NeedAModel()
    {
        Assert.IsFalse(new RepositoryAiModelsDto { EmbeddingDimensions = 512 }.IsValid());
        Assert.IsFalse(new RepositoryAiModelsDto { QueryInstruction = "Search" }.IsValid());
        Assert.IsTrue(new RepositoryAiModelsDto { QueryInstruction = "   " }.IsValid());
    }

    [TestMethod]
    public void TooLongTexts_AreRefused()
    {
        Assert.IsFalse(new RepositoryAiModelsDto { EmbeddingModel = new string('x', RepositoryAiModelsDto.MaxModelLength + 1) }.IsValid());
        Assert.IsFalse(new RepositoryAiModelsDto { EmbeddingModel = "m", QueryInstruction = new string('x', RepositoryAiModelsDto.MaxQueryInstructionLength + 1) }.IsValid());
    }

    [TestMethod]
    public void SameEmbeddingModel_ComparesTheModelAndItsDimensions()
    {
        var saved = new RepositoryAiModelsDto { EmbeddingModel = "qwen3-embedding:0.6b", LlmModel = "a" };

        Assert.IsTrue(new RepositoryAiModelsDto { EmbeddingModel = " QWEN3-embedding:0.6b ", LlmModel = "b" }.SameEmbeddingModel(saved));
        Assert.IsFalse(new RepositoryAiModelsDto { EmbeddingModel = "qwen3-embedding:4b" }.SameEmbeddingModel(saved));
        Assert.IsFalse(new RepositoryAiModelsDto { EmbeddingModel = "qwen3-embedding:0.6b", EmbeddingDimensions = 512 }.SameEmbeddingModel(saved));
        Assert.IsFalse(new RepositoryAiModelsDto().SameEmbeddingModel(saved));
    }

    [TestMethod]
    public void ParseIndexKey_GivesTheModelAndTheDimensions()
    {
        Assert.AreEqual(("qwen3-embedding:4b", (int?)1024), RepositoryAiModelsDto.ParseIndexKey("qwen3-embedding:4b@1024"));
        Assert.AreEqual(("qwen3-embedding:0.6b", (int?)null), RepositoryAiModelsDto.ParseIndexKey(" qwen3-embedding:0.6b "));
        Assert.AreEqual(("hf.co/user/model@main", (int?)null), RepositoryAiModelsDto.ParseIndexKey("hf.co/user/model@main"));
        Assert.AreEqual(((string)null, (int?)null), RepositoryAiModelsDto.ParseIndexKey(" "));
    }
}
