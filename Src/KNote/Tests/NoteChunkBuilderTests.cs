using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// NoteChunkBuilder: the fragments of a note for the semantic index (KNote Atlas, stage 0) - data, text by sections,
/// tasks, described resources - and their hashes.
/// </summary>
[TestClass]
public class NoteChunkBuilderTests
{
    private const string Separator = NoteChunkBuilder.HeadingSeparator;

    private static NoteChunkSource Source(string description = "", string topic = "Amstrad CPC 464") => new()
    {
        Topic = topic,
        NoteType = "Computer",
        FolderPath = @"Home\Collection\Amstrad",
        Tags = "z80, 8-bit",
        Description = description
    };

    private static List<NoteChunkDraft> Of(NoteChunkSet set, NoteChunkSourceType type) =>
        set.Chunks.Where(c => c.SourceType == type).ToList();

    private static string Words(int count, string word = "word") =>
        string.Join(" ", Enumerable.Range(1, count).Select(i => $"{word}{i}"));

    #region Data

    [TestMethod]
    public void Data_OneFragment_WithAKeyValueLinePerData()
    {
        var source = Source();
        source.Attributes.Add(new NoteChunkAttribute("Processor", "Z80A"));
        source.Attributes.Add(new NoteChunkAttribute("Status", "  "));
        source.Attributes.Add(new NoteChunkAttribute("Notes", "Two\nlines"));
        source.Resources.Add(new NoteChunkResource(Guid.NewGuid(), "front.jpg", "image/jpeg", "front.jpg"));
        source.Resources.Add(new NoteChunkResource(Guid.NewGuid(), "manual.pdf", "application/pdf", ""));

        var data = Of(NoteChunkBuilder.Build(source), NoteChunkSourceType.Data).Single();

        Assert.AreEqual(
            "Topic: Amstrad CPC 464\nType: Computer\nFolder: Home\\Collection\\Amstrad\nTags: z80, 8-bit\n"
            + "Processor: Z80A\nNotes: Two lines\nResources: front.jpg, manual.pdf",
            data.Content);
        Assert.AreEqual(Guid.Empty, data.SourceId);
        Assert.AreEqual(0, data.ChunkIndex);
        Assert.AreEqual("", data.Heading);
    }

    [TestMethod]
    public void Data_WithoutOptionalData_HasTheTopic()
    {
        var source = new NoteChunkSource { Topic = "Only a topic" };

        var set = NoteChunkBuilder.Build(source);

        Assert.AreEqual(1, set.Chunks.Count);
        Assert.AreEqual("Topic: Only a topic", set.Chunks[0].Content);
    }

    [TestMethod]
    public void Data_ManyResources_ListsTheFirstOnes()
    {
        var source = Source();
        for (var i = 1; i <= 25; i++)
            source.Resources.Add(new NoteChunkResource(Guid.NewGuid(), $"photo{i}.jpg", "image/jpeg", ""));

        var data = Of(NoteChunkBuilder.Build(source), NoteChunkSourceType.Data).Single();

        var resources = data.Content.Split('\n').Last();
        StringAssert.StartsWith(resources, "Resources: photo1.jpg, photo2.jpg");
        StringAssert.EndsWith(resources, "photo20.jpg, \u2026 (5 more)");
        Assert.IsFalse(resources.Contains("photo21.jpg"));
    }

    #endregion

    #region Text

    [TestMethod]
    public void Text_EmptyDescription_HasNoTextFragments()
    {
        var set = NoteChunkBuilder.Build(Source(""));

        Assert.AreEqual(0, Of(set, NoteChunkSourceType.Text).Count);
    }

    [TestMethod]
    public void Text_BySections_WithThePathOfHeadings()
    {
        var intro = Words(60, "intro");
        var restoration = Words(60, "restoration");
        var keyboard = Words(60, "keyboard");
        var sources = Words(60, "sources");
        var description = $"{intro}\n\n# Restoration\n\n{restoration}\n\n## Keyboard\n\n{keyboard}\n\n# Sources\n\n{sources}";

        var text = Of(NoteChunkBuilder.Build(Source(description)), NoteChunkSourceType.Text);

        CollectionAssert.AreEqual(
            new[]
            {
                "Amstrad CPC 464",
                $"Amstrad CPC 464{Separator}Restoration",
                $"Amstrad CPC 464{Separator}Restoration{Separator}Keyboard",
                $"Amstrad CPC 464{Separator}Sources"
            },
            text.Select(c => c.Heading).ToArray());
        StringAssert.StartsWith(text[2].Content, "## Keyboard");
        CollectionAssert.AreEqual(new[] { 0, 1, 2, 3 }, text.Select(c => c.ChunkIndex).ToArray());
        Assert.IsTrue(text.All(c => c.SourceId == Guid.Empty));
    }

    [TestMethod]
    public void Text_HeadingInACodeBlock_IsNotASection()
    {
        var description = $"{Words(60)}\n\n```\n# not a heading\n```\n\n{Words(60, "more")}";

        var text = Of(NoteChunkBuilder.Build(Source(description)), NoteChunkSourceType.Text);

        Assert.AreEqual(1, text.Count);
        Assert.AreEqual("Amstrad CPC 464", text[0].Heading);
    }

    [TestMethod]
    public void Text_SmallSections_JoinTheNextOne()
    {
        var description = "# One\n\nShort.\n\n# Two\n\nAlso short.\n\n# Three\n\n" + Words(300);

        var text = Of(NoteChunkBuilder.Build(Source(description)), NoteChunkSourceType.Text);

        // One and Two together; Three does not fit with them and is cut in two.
        Assert.AreEqual(3, text.Count);
        Assert.AreEqual($"Amstrad CPC 464{Separator}One", text[0].Heading);
        Assert.AreEqual("# One\n\nShort.\n\n# Two\n\nAlso short.\n\n", text[0].Content);
        Assert.AreEqual($"Amstrad CPC 464{Separator}Three", text[1].Heading);
        Assert.AreEqual($"Amstrad CPC 464{Separator}Three", text[2].Heading);
    }

    [TestMethod]
    public void Text_LongSection_IsCutInPiecesThatTogetherAreTheCleanText()
    {
        var paragraphs = Enumerable.Range(1, 12).Select(i => Words(40, $"p{i}w"));
        var longLine = Words(400, "long");
        var description = "# Restoration\r\n\r\n" + string.Join("\r\n\r\n", paragraphs) + "\r\n\r\n" + longLine
            + "\r\n\r\n![photo](data:image/png;base64,AAAA)\r\n\r\n# End\r\n\r\n" + Words(80, "end");

        var text = Of(NoteChunkBuilder.Build(Source(description)), NoteChunkSourceType.Text);

        Assert.IsTrue(text.Count > 3, $"{text.Count} fragments");
        Assert.IsTrue(text.All(c => c.Content.Length <= NoteChunkBuilder.MaxChunkLength));
        Assert.AreEqual(NoteTextCleaner.Clean(description), string.Concat(text.Select(c => c.Content)));
        Assert.IsTrue(text.Where(c => c.Heading.EndsWith("Restoration")).Count() > 1);
        CollectionAssert.AreEqual(Enumerable.Range(0, text.Count).ToArray(), text.Select(c => c.ChunkIndex).ToArray());
    }

    [TestMethod]
    public void Text_PieceOfAParagraphList_IsCutAfterABlankLine()
    {
        var description = string.Join("\n\n", Enumerable.Range(1, 10).Select(i => Words(30, $"p{i}w")));

        var text = Of(NoteChunkBuilder.Build(Source(description)), NoteChunkSourceType.Text);

        Assert.IsTrue(text.Count > 1);
        Assert.IsTrue(text.Take(text.Count - 1).All(c => c.Content.EndsWith("\n\n")));
    }

    #endregion

    #region Tasks and resources

    [TestMethod]
    public void Task_OneFragment_WithItsStatusAndDescription()
    {
        var taskId = Guid.NewGuid();
        var source = Source();
        source.Tasks.Add(new NoteChunkTask(taskId, "Change the capacitors", "Change the capacitors\r\n\r\nOf the power supply.", false));
        source.Tasks.Add(new NoteChunkTask(Guid.NewGuid(), "Clean the keyboard", "", true));

        var tasks = Of(NoteChunkBuilder.Build(source), NoteChunkSourceType.Task);

        Assert.AreEqual(2, tasks.Count);
        Assert.AreEqual(taskId, tasks[0].SourceId);
        Assert.AreEqual("Amstrad CPC 464", tasks[0].Heading);
        Assert.AreEqual("Task: Change the capacitors\nStatus: pending\n\nChange the capacitors\n\nOf the power supply.", tasks[0].Content);
        Assert.AreEqual("Task: Clean the keyboard\nStatus: resolved", tasks[1].Content);
        Assert.IsTrue(tasks.All(c => c.ChunkIndex == 0));
    }

    [TestMethod]
    public void Task_LongDescription_IsCut()
    {
        var source = Source();
        source.Tasks.Add(new NoteChunkTask(Guid.NewGuid(), "Long", Words(500), false));

        var tasks = Of(NoteChunkBuilder.Build(source), NoteChunkSourceType.Task);

        Assert.IsTrue(tasks.Count > 1);
        CollectionAssert.AreEqual(Enumerable.Range(0, tasks.Count).ToArray(), tasks.Select(c => c.ChunkIndex).ToArray());
        StringAssert.StartsWith(tasks[0].Content, "Task: Long");
    }

    [TestMethod]
    public void Resource_WithADescriptionOfItsOwn_HasAFragment()
    {
        var resourceId = Guid.NewGuid();
        var source = Source();
        source.Resources.Add(new NoteChunkResource(resourceId, "cpc-board.jpg", "image/jpeg", "The board after changing the capacitors."));

        var resource = Of(NoteChunkBuilder.Build(source), NoteChunkSourceType.Resource).Single();

        Assert.AreEqual(resourceId, resource.SourceId);
        Assert.AreEqual("Amstrad CPC 464", resource.Heading);
        Assert.AreEqual("Resource: cpc-board.jpg\nType: image/jpeg\n\nThe board after changing the capacitors.", resource.Content);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("  ")]
    [DataRow("cpc-board.jpg")]
    [DataRow("CPC-BOARD.JPG")]
    [DataRow("cpc-board")]
    [DataRow("Image inserted from clipboard.")]
    public void Resource_WithoutADescriptionOfItsOwn_HasNoFragment(string? description)
    {
        var source = Source();
        source.Resources.Add(new NoteChunkResource(Guid.NewGuid(), "cpc-board.jpg", "image/jpeg", description!));

        var set = NoteChunkBuilder.Build(source);

        Assert.AreEqual(0, Of(set, NoteChunkSourceType.Resource).Count);
        StringAssert.Contains(Of(set, NoteChunkSourceType.Data).Single().Content, "Resources: cpc-board.jpg");
    }

    #endregion

    #region Hashes

    [TestMethod]
    public void Hashes_DoNotChangeWithTheLineEnds()
    {
        var description = "# Restoration\n\nCleaned the case.\n\n- Keyboard\n- Power supply";
        var lf = Source(description);
        lf.Tasks.Add(new NoteChunkTask(Guid.Empty, "A task", "A task\nwith two lines", false));
        var crlf = Source(description.Replace("\n", "\r\n"));
        crlf.Tasks.Add(new NoteChunkTask(Guid.Empty, "A task", "A task\r\nwith two lines", false));

        var a = NoteChunkBuilder.Build(lf);
        var b = NoteChunkBuilder.Build(crlf);

        Assert.AreEqual(a.SourceHash, b.SourceHash);
        CollectionAssert.AreEqual(a.Chunks.Select(c => c.ContentHash).ToArray(), b.Chunks.Select(c => c.ContentHash).ToArray());
        Assert.AreEqual(64, a.SourceHash.Length);
    }

    [TestMethod]
    public void ContentHash_IsTheHashOfTheTextForTheModel()
    {
        var chunk = NoteChunkBuilder.Build(Source("Some text.")).Chunks.Single(c => c.SourceType == NoteChunkSourceType.Text);

        Assert.AreEqual("Amstrad CPC 464\n\nSome text.", chunk.TextForEmbedding);
        Assert.AreEqual(NoteChunkBuilder.Hash(chunk.TextForEmbedding), chunk.ContentHash);
    }

    [TestMethod]
    public void RenamingTheTopic_ChangesTheHashesOfTheText()
    {
        var before = NoteChunkBuilder.Build(Source("Some text.", "Amstrad CPC 464"));
        var after = NoteChunkBuilder.Build(Source("Some text.", "Amstrad CPC 6128"));

        Assert.AreNotEqual(before.SourceHash, after.SourceHash);
        Assert.AreNotEqual(
            Of(before, NoteChunkSourceType.Text).Single().ContentHash,
            Of(after, NoteChunkSourceType.Text).Single().ContentHash);
    }

    [TestMethod]
    public void ChangeInACleanedPart_ChangesTheSourceHashOnly()
    {
        var before = NoteChunkBuilder.Build(Source("Text <!-- old comment --> end."));
        var after = NoteChunkBuilder.Build(Source("Text <!-- new comment --> end."));

        Assert.AreNotEqual(before.SourceHash, after.SourceHash);
        CollectionAssert.AreEqual(before.Chunks.Select(c => c.ContentHash).ToArray(), after.Chunks.Select(c => c.ContentHash).ToArray());
    }

    [TestMethod]
    public void DescribingAResource_ChangesTheSourceHash_AndAddsItsFragment()
    {
        var resourceId = Guid.NewGuid();
        var source = Source();
        source.Resources.Add(new NoteChunkResource(resourceId, "cpc-board.jpg", "image/jpeg", "cpc-board.jpg"));
        var before = NoteChunkBuilder.Build(source);

        source.Resources[0] = source.Resources[0] with { Description = "The board after changing the capacitors." };
        var after = NoteChunkBuilder.Build(source);

        Assert.AreNotEqual(before.SourceHash, after.SourceHash);
        Assert.AreEqual(0, Of(before, NoteChunkSourceType.Resource).Count);
        Assert.AreEqual(1, Of(after, NoteChunkSourceType.Resource).Count);
    }

    #endregion

    #region From a note

    [TestMethod]
    public void From_TakesWhatGoesToTheIndex()
    {
        var noteId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var note = new NoteExtendedDto
        {
            NoteId = noteId,
            Topic = "Amstrad CPC 464",
            Tags = "z80",
            Description = "<p>HTML</p>",
            NoteTypeDto = new NoteTypeDto { Name = "Computer" },
            KAttributesDto = new List<NoteKAttributeDto>
            {
                new() { Name = "Status", Value = "Working", Order = 2 },
                new() { Name = "Processor", Value = "Z80A", Order = 1 }
            },
            Tasks = new List<NoteTaskDto> { new() { NoteTaskId = Guid.NewGuid(), Description = "# Clean it\nAll of it", Resolved = true } },
            Resources = new List<ResourceDto>
            {
                new() { ResourceId = resourceId, Name = $"{noteId}_front.jpg", FileType = "image/jpeg", Description = "The front." }
            }
        };

        var source = NoteChunkSource.From(note, @"Home\Collection", "Markdown text");

        Assert.AreEqual("Computer", source.NoteType);
        Assert.AreEqual(@"Home\Collection", source.FolderPath);
        Assert.AreEqual("Markdown text", source.Description);
        CollectionAssert.AreEqual(new[] { "Processor", "Status" }, source.Attributes.Select(a => a.Name).ToArray());
        Assert.AreEqual("Clean it", source.Tasks.Single().Topic);
        Assert.IsTrue(source.Tasks.Single().Resolved);
        Assert.AreEqual(new NoteChunkResource(resourceId, "front.jpg", "image/jpeg", "The front."), source.Resources.Single());
    }

    #endregion
}
