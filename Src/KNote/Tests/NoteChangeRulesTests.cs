using KNote.Model.Core;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// NoteChangeRules (KNote Atlas, stage 2, D6): which saves modify a note. Its own data against what is stored, with its texts
/// as the editors give them back; its status doesn't count.
/// </summary>
[TestClass]
public class NoteChangeRulesTests
{
    private static readonly Guid FolderId = Guid.NewGuid();
    private static readonly Guid AttributeId = Guid.NewGuid();

    private static NoteDto Stored() => new()
    {
        NoteId = Guid.NewGuid(), Topic = "QL", Description = "Line one\r\nLine two\r\n", Tags = null, Script = null,
        FolderId = FolderId, Priority = 1, ContentType = "markdown", InternalTags = "",
        KAttributesDto = { new NoteKAttributeDto { KAttributeId = AttributeId, Value = "" } }
    };

    [TestMethod]
    public void TheSameData_AsAnEditorGivesItBack_IsNoChange()
    {
        var edited = Stored();
        edited.Description = "Line one\nLine two";
        edited.Tags = "";
        edited.Script = "";
        edited.InternalTags = "Alarms pending";
        edited.KAttributesDto.Clear();

        Assert.IsNull(NoteChangeRules.ChangedField(Stored(), edited));
    }

    [TestMethod]
    [DataRow(nameof(NoteDto.Topic))]
    [DataRow(nameof(NoteDto.Description))]
    [DataRow(nameof(NoteDto.Tags))]
    [DataRow(nameof(NoteDto.FolderId))]
    [DataRow(nameof(NoteDto.NoteTypeId))]
    [DataRow(nameof(NoteDto.Priority))]
    [DataRow(nameof(NoteDto.Script))]
    [DataRow(nameof(NoteDto.ContentType))]
    [DataRow(nameof(NoteDto.KAttributesDto))]
    public void EachOfItsOwnData_IsAChange(string field)
    {
        var edited = Stored();
        switch (field)
        {
            case nameof(NoteDto.Topic): edited.Topic = "QL 2"; break;
            case nameof(NoteDto.Description): edited.Description = "Line one\r\nLine 2"; break;
            case nameof(NoteDto.Tags): edited.Tags = "ql"; break;
            case nameof(NoteDto.FolderId): edited.FolderId = Guid.NewGuid(); break;
            case nameof(NoteDto.NoteTypeId): edited.NoteTypeId = Guid.NewGuid(); break;
            case nameof(NoteDto.Priority): edited.Priority = 2; break;
            case nameof(NoteDto.Script): edited.Script = "run"; break;
            case nameof(NoteDto.ContentType): edited.ContentType = "html"; break;
            case nameof(NoteDto.KAttributesDto): edited.KAttributesDto[0].Value = "ACME"; break;
        }

        Assert.AreEqual(field, NoteChangeRules.ChangedField(Stored(), edited));
    }

    [TestMethod]
    public void SameText_NoValueAndEmpty_CrLfAndLf_AndTheEnd()
    {
        Assert.IsTrue(NoteChangeRules.SameText(null, ""));
        Assert.IsTrue(NoteChangeRules.SameText("a\r\nb", "a\nb"));
        Assert.IsTrue(NoteChangeRules.SameText("a\r\n\r\n", "a "));
        Assert.IsFalse(NoteChangeRules.SameText(" a", "a"), "the spaces at the start count");
        Assert.IsFalse(NoteChangeRules.SameText("a\nb", "a b"));
    }
}
