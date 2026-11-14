using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// NoteInfoDto.SetEmptyTextsForEditing (KNote Atlas, stage 2, D6): the texts with no value become empty texts, what has a value
/// stays, and the editors of both apps then see no change in an empty box.
/// </summary>
[TestClass]
public class NoteEmptyTextsTests
{
    [TestMethod]
    public void TheTextsWithNoValue_BecomeEmpty_AndTheOthersStay()
    {
        var note = new NoteDto { Topic = "QL", Description = null, Tags = null, InternalTags = null, Script = "run" };

        note.SetEmptyTextsForEditing();

        Assert.AreEqual(("QL", "", "", "", "run"), (note.Topic, note.Description, note.Tags, note.InternalTags, note.Script));
    }

    [TestMethod]
    public void AfterIt_AnEmptyBox_IsNotAChange()
    {
        var note = new NoteDto { Topic = "QL" };
        note.SetEmptyTextsForEditing();
        note.SetIsDirty(false);

        note.Tags = "";
        note.Script = "";
        note.Description = "";
        note.InternalTags = "";

        Assert.IsFalse(note.IsDirty());
    }
}
