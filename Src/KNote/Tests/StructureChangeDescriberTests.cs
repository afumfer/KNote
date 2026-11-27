using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// StructureChangeDescriber (the history of the structure of a repository, H.2b): what changed in a folder, a note type, an
/// attribute and its values, a user or a setting; the private values without them (H15); how a session adds up; its line.
/// </summary>
[TestClass]
public class StructureChangeDescriberTests
{
    private static string Line(string entity, StructureChangeDetailsDto details) => StructureChangeDescriber.Render(entity, details);

    [TestMethod]
    public void AFolder_CreatedRenamedMovedDeleted()
    {
        var parent = Guid.NewGuid();
        var other = Guid.NewGuid();
        var folder = new FolderInfoDto { FolderId = Guid.NewGuid(), Name = "KNote", ParentId = parent };
        var renamed = new FolderInfoDto { FolderId = folder.FolderId, Name = "KNote Atlas", ParentId = other };

        Assert.AreEqual(@"Folder created in Home\Projects",
            Line(HistoryEntities.Folder, StructureChangeDescriber.Folder(null, folder, null, @"Home\Projects")));
        Assert.AreEqual(@"Renamed: KNote → KNote Atlas · Moved from Home\Projects to Home\Archive",
            Line(HistoryEntities.Folder, StructureChangeDescriber.Folder(folder, renamed, @"Home\Projects", @"Home\Archive")));
        Assert.AreEqual("Folder deleted", Line(HistoryEntities.Folder, StructureChangeDescriber.Folder(folder, null, null, null)));

        var reordered = new FolderInfoDto { FolderId = folder.FolderId, Name = "KNote", ParentId = parent, Order = 5, OrderNotes = "x" };
        Assert.IsTrue(StructureChangeDescriber.Folder(folder, reordered, null, null).IsEmpty, "the order is presentation (H17)");
    }

    [TestMethod]
    public void AnAttribute_AndItsValues()
    {
        var restoring = new KAttributeTabulatedValueDto { KAttributeTabulatedValueId = Guid.NewGuid(), Value = "Restoring" };
        var broken = new KAttributeTabulatedValueDto { KAttributeTabulatedValueId = Guid.NewGuid(), Value = "Broken" };
        var before = new KAttributeDto { KAttributeId = Guid.NewGuid(), Name = "Status", KAttributeDataType = EnumKAttributeDataType.TabulatedValue,
            KAttributeValues = new() { restoring, broken } };
        var after = new KAttributeDto { KAttributeId = before.KAttributeId, Name = "State", KAttributeDataType = EnumKAttributeDataType.TabulatedValue,
            RequiredValue = true, KAttributeValues = new()
            {
                new KAttributeTabulatedValueDto { KAttributeTabulatedValueId = restoring.KAttributeTabulatedValueId, Value = "In restoration" },
                new KAttributeTabulatedValueDto { Value = "Works" }
            } };

        Assert.AreEqual("Renamed: Status → State · Required: No → Yes · Values: +Works, −Broken, Restoring → In restoration",
            Line(HistoryEntities.KAttribute, StructureChangeDescriber.KAttribute(before, after, new HistoryNames())));
        Assert.AreEqual("Attribute created (TabulatedValue) · Values: +Restoring, +Broken",
            Line(HistoryEntities.KAttribute, StructureChangeDescriber.KAttribute(null, before, new HistoryNames())));
    }

    [TestMethod]
    public void AUser_WithoutItsEmail_NorEverAPassword()
    {
        var before = new UserDto { UserId = Guid.NewGuid(), UserName = "mlopez", FullName = "Marta López", EMail = "marta@old.test", RoleDefinition = "Guest" };
        var after = new UserDto { UserId = before.UserId, UserName = "mlopez", FullName = "Marta López Ruiz", EMail = "marta@new.test", RoleDefinition = "Staff", Disabled = true };

        var change = StructureChangeDescriber.User(before, after);

        Assert.AreEqual("Full name: Marta López → Marta López Ruiz · Email changed · Role: Guest → Staff · Disabled", Line(HistoryEntities.User, change));
        Assert.IsFalse(change.ToJson().Contains("marta@"), change.ToJson());
        Assert.AreEqual("User created (Guest)", Line(HistoryEntities.User, StructureChangeDescriber.User(null, before)));
        Assert.AreEqual("Password reset", Line(HistoryEntities.User, StructureChangeDescriber.Password(HistoryPasswordChanges.Reset)));
    }

    [TestMethod]
    public void ASetting_WithOrWithoutItsValues()
    {
        Assert.AreEqual("Value: Work → Work notes", Line(HistoryEntities.SystemValue, StructureChangeDescriber.Setting("Work", "Work notes", showValues: true)));
        var secret = StructureChangeDescriber.Setting("token-1", "token-2", showValues: false);
        Assert.AreEqual("Value changed", Line(HistoryEntities.SystemValue, secret));
        Assert.IsFalse(secret.ToJson().Contains("token"), secret.ToJson());
        Assert.AreEqual("Setting created", Line(HistoryEntities.SystemValue, StructureChangeDescriber.Setting(null, "token-1", showValues: false)));
        Assert.AreEqual("Setting deleted", Line(HistoryEntities.SystemValue, StructureChangeDescriber.Setting("token-1", null, showValues: false)));
    }

    [TestMethod]
    public void TheAiModels_AndTheNoisePolicy()
    {
        var models = StructureChangeDescriber.AiModels(
            new RepositoryAiModelsDto { EmbeddingModel = "bge-m3", QueryInstruction = "Represent this" },
            new RepositoryAiModelsDto { EmbeddingModel = "qwen3-embedding", QueryInstruction = "Find", LlmModel = "qwen3.5:9b" });
        Assert.AreEqual("Embedding model: bge-m3 → qwen3-embedding · Query instruction changed · Recommended LLM: (none) → qwen3.5:9b",
            Line(HistoryEntities.SystemValue, models));

        var task = Guid.NewGuid();
        var inbox = Guid.NewGuid();
        var policy = StructureChangeDescriber.NoisePolicy(new NoisePolicyDto { ExcludedFolderIds = new() { inbox } },
            new NoisePolicyDto { ExcludedNoteTypeIds = new() { task } },
            id => id == task ? "Task" : null, id => id == inbox ? @"Home\Inbox" : null);
        Assert.AreEqual(@"Excluded note types: +Task · Excluded folders: −Home\Inbox", Line(HistoryEntities.SystemValue, policy));
    }

    [TestMethod]
    public void ASession_AddsUp_AndBackToHowItWasIsNothing()
    {
        var a = new NoteTypeDto { NoteTypeId = Guid.NewGuid(), Name = "A" };
        var b = new NoteTypeDto { NoteTypeId = a.NoteTypeId, Name = "B" };
        var c = new NoteTypeDto { NoteTypeId = a.NoteTypeId, Name = "C" };

        var twice = StructureChangeDescriber.Merge(StructureChangeDescriber.NoteType(a, b, null), StructureChangeDescriber.NoteType(b, c, null));
        Assert.AreEqual("Renamed: A → C", Line(HistoryEntities.NoteType, twice));
        var back = StructureChangeDescriber.Merge(StructureChangeDescriber.NoteType(a, b, null), StructureChangeDescriber.NoteType(b, a, null));
        Assert.IsTrue(back.IsEmpty);

        var created = StructureChangeDescriber.Merge(StructureChangeDescriber.NoteType(null, a, null), StructureChangeDescriber.NoteType(a, b, null));
        Assert.AreEqual("Note type created · Renamed: A → B", Line(HistoryEntities.NoteType, created));

        var values = StructureChangeDescriber.Merge(
            new StructureChangeDetailsDto { Lists = new() { new HistoryListChangeDto { Field = HistoryStructureFields.Values, Added = new() { "X" } } } },
            new StructureChangeDetailsDto { Lists = new() { new HistoryListChangeDto { Field = HistoryStructureFields.Values, Removed = new() { "X" } } } });
        Assert.IsTrue(values.IsEmpty, "added and then removed");
    }

    [TestMethod]
    public void TheIdOfASetting_IsAlwaysTheSame()
    {
        Assert.AreEqual(HistoryRules.SettingId("AI", "AI_NOISE_POLICY"), HistoryRules.SettingId("ai", "ai_noise_policy"));
        Assert.AreNotEqual(HistoryRules.SettingId("AI", "AI_NOISE_POLICY"), HistoryRules.SettingId("AI", "MODELS"));
    }
}
