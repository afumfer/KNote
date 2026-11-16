using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// The noise policy (KNote Atlas, stage 2, 2.2, chapter 27, D2-D4): KNote's own notes always out; #NoKnowledge out, also with
/// #Knowledge; #Knowledge in although its type or its folder are left out; a folder left out on its own, not its subfolders; the
/// tags whole and without case; the recount by reason; and the policy kept without what doesn't exist nor KNote's own.
/// </summary>
[TestClass]
public class NoiseRulesTests
{
    private static readonly Guid Meeting = Guid.NewGuid();
    private static readonly Guid ChatSessions = Guid.NewGuid();
    private static readonly Guid Inbox = Guid.NewGuid();
    private static readonly Guid InboxChild = Guid.NewGuid();
    private static readonly Guid Projects = Guid.NewGuid();
    private static readonly Guid AtlasFolder = Guid.NewGuid();

    private static readonly HashSet<Guid> KNoteTypes = new() { ChatSessions };
    private static readonly HashSet<Guid> KNoteFolders = new() { AtlasFolder };
    private static readonly NoisePolicyDto Policy = new() { ExcludedNoteTypeIds = { Meeting }, ExcludedFolderIds = { Inbox } };

    private static EnumNoiseReason Classify(Guid folder, Guid? type = null, string? tags = null) =>
        NoiseRules.Classify(new NoteClassificationDto { NoteId = Guid.NewGuid(), FolderId = folder, NoteTypeId = type, Tags = tags }, Policy, KNoteTypes, KNoteFolders);

    [TestMethod]
    public void TheOrder_KNoteNotes_ThenNoKnowledge_ThenKnowledge_ThenThePolicy()
    {
        Assert.AreEqual(EnumNoiseReason.In, Classify(Projects));
        Assert.AreEqual(EnumNoiseReason.ExcludedNoteType, Classify(Projects, Meeting));
        Assert.AreEqual(EnumNoiseReason.ExcludedFolder, Classify(Inbox));
        Assert.AreEqual(EnumNoiseReason.NoKnowledgeTag, Classify(Projects, tags: "#NoKnowledge"));
        Assert.AreEqual(EnumNoiseReason.KnowledgeTag, Classify(Inbox, tags: "#Knowledge"), "in, although its folder is left out");
        Assert.AreEqual(EnumNoiseReason.KnowledgeTag, Classify(Projects, Meeting, "#Knowledge"), "in, although its type is left out");
        Assert.AreEqual(EnumNoiseReason.In, Classify(Projects, tags: "#Knowledge"), "nothing to keep it from: just in");
        Assert.AreEqual(EnumNoiseReason.NoKnowledgeTag, Classify(Projects, tags: "#Knowledge #NoKnowledge"), "with both, out");
        Assert.AreEqual(EnumNoiseReason.KNoteNote, Classify(Projects, ChatSessions, "#Knowledge"), "KNote's own notes, always out");
        Assert.AreEqual(EnumNoiseReason.KNoteNote, Classify(AtlasFolder, tags: "#Knowledge"));
    }

    [TestMethod]
    public void AFolderLeftOut_DoesNotLeaveOutItsSubfolders()
    {
        Assert.AreEqual(EnumNoiseReason.ExcludedFolder, Classify(Inbox));
        Assert.AreEqual(EnumNoiseReason.In, Classify(InboxChild), "each folder on its own");
    }

    [TestMethod]
    public void TheTags_AreReadWhole_AndWithoutCase()
    {
        Assert.IsTrue(NoiseRules.HasTag("work, #noknowledge; draft", KntConst.NoKnowledgeTag));
        Assert.IsTrue(NoiseRules.HasTag("#KNOWLEDGE", KntConst.KnowledgeTag));
        Assert.IsFalse(NoiseRules.HasTag("#NoKnowledgeBase", KntConst.NoKnowledgeTag), "the whole tag");
        Assert.IsFalse(NoiseRules.HasTag("@NoKnowledge", KntConst.NoKnowledgeTag), "with '#', not '@'");
        Assert.IsFalse(NoiseRules.HasTag(null, KntConst.KnowledgeTag));
    }

    [TestMethod]
    public void TheRecount_SaysHowManyAreOutAndWhy()
    {
        var notes = new[]
        {
            new NoteClassificationDto { FolderId = Projects },
            new NoteClassificationDto { FolderId = Projects, NoteTypeId = Meeting },
            new NoteClassificationDto { FolderId = Inbox },
            new NoteClassificationDto { FolderId = Inbox, Tags = "#Knowledge" },
            new NoteClassificationDto { FolderId = Projects, Tags = "#NoKnowledge" },
            new NoteClassificationDto { FolderId = AtlasFolder }
        };

        var count = NoiseRules.Count(notes, Policy, KNoteTypes, KNoteFolders);

        Assert.AreEqual(6, count.Notes);
        Assert.AreEqual(4, count.Out);
        Assert.AreEqual(2, count.In);
        Assert.AreEqual((1, 1, 1, 1, 1), (count.ExcludedNoteType, count.ExcludedFolder, count.NoKnowledgeTag, count.KNoteNotes, count.KeptByKnowledgeTag));
        var lines = NoiseRules.CountLines(count);
        StringAssert.StartsWith(lines[0], "Of 6 notes, 4 notes would be left out of the summaries and 2 notes would go in.");
        Assert.HasCount(6, lines);
    }

    [TestMethod]
    public void ThePolicyKept_HasOnlyWhatExists_AndNotKNotesOwn()
    {
        var gone = Guid.NewGuid();
        var policy = new NoisePolicyDto
        {
            ExcludedNoteTypeIds = { Meeting, Meeting, ChatSessions, gone },
            ExcludedFolderIds = { Inbox, AtlasFolder, gone }
        };

        var kept = NoiseRules.Clean(policy, new HashSet<Guid> { Meeting, ChatSessions }, new HashSet<Guid> { Inbox, InboxChild, Projects, AtlasFolder },
            KNoteTypes, KNoteFolders);

        CollectionAssert.AreEqual(new[] { Meeting }, kept.ExcludedNoteTypeIds);
        CollectionAssert.AreEqual(new[] { Inbox }, kept.ExcludedFolderIds);
    }

    [TestMethod]
    public void KNotesOwnTypes_AreTheSystemOnes_AndWhatKNoteAtlasWrites()
    {
        CollectionAssert.IsSubsetOf(KntConst.SystemNoteTypeNames.ToList(), NoiseRules.KNoteNoteTypeNames.ToList());
        CollectionAssert.Contains(NoiseRules.KNoteNoteTypeNames.ToList(), KntConst.SummaryTag);
        CollectionAssert.Contains(NoiseRules.KNoteNoteTypeNames.ToList(), KntConst.TranslationTag);
    }
}
