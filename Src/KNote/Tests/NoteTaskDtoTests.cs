using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// NoteTaskDto.GetTopic: the topic the lists of tasks show (ClientWin and the web), its Tags or, without them, the
/// first line of its description.
/// </summary>
[TestClass]
public class NoteTaskDtoTests
{
    [TestMethod]
    [DataRow(" Review the API ", "Some text", "Review the API")]
    [DataRow(null, "## Migrate the database\r\n\r\nCopy the tables.", "Migrate the database")]
    [DataRow("", "\r\n\r\n- [ ] first step\r\n- second", "[ ] first step")]
    [DataRow("  ", "> quoted line", "quoted line")]
    [DataRow(null, null, "")]
    [DataRow("", "  \r\n  ", "")]
    public void GetTopic_IsTheTagsOrTheFirstLineOfTheDescription(string? tags, string? description, string expected)
    {
        var task = new NoteTaskDto { Tags = tags!, Description = description! };

        Assert.AreEqual(expected, task.GetTopic());
    }

    [TestMethod]
    public void GetTopic_CutsALongFirstLine()
    {
        var task = new NoteTaskDto { Description = new string('a', 150) };

        Assert.AreEqual(new string('a', 100) + "…", task.GetTopic());
    }
}
