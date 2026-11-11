using KNote.ClientWin.Controllers;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// KNote Atlas, stage 4 (4.4b): the text of a document is read from the repository, so only a document already saved and
/// without changes of the editor is offered (NoteEditorCtrl.CanReadNow).
/// </summary>
[TestClass]
public class NoteEditorCtrlDocumentsTests
{
    private static ResourceDto Saved(string name, string fileType)
    {
        var resource = new ResourceDto { ResourceId = Guid.NewGuid(), Name = $"{Guid.NewGuid()}_{name}", FileType = fileType };
        resource.SetIsNew(false);
        resource.SetIsDirty(false);
        return resource;
    }

    [TestMethod]
    public void ADocumentSaved_CanBeRead()
    {
        Assert.IsTrue(NoteEditorCtrl.CanReadNow(Saved("manual.pdf", "application/pdf")));
        Assert.IsTrue(NoteEditorCtrl.CanReadNow(Saved("letter.docx", "")));
    }

    [TestMethod]
    public void APhoto_ANewDocument_OrOneWithChanges_CanNot()
    {
        var changed = Saved("manual.pdf", "application/pdf");
        changed.Description = "Changed here";
        changed.SetIsDirty(true);
        var added = Saved("new.pdf", "application/pdf");
        added.SetIsNew(true);

        Assert.IsFalse(NoteEditorCtrl.CanReadNow(Saved("photo.png", "image/png")));
        Assert.IsFalse(NoteEditorCtrl.CanReadNow(changed));
        Assert.IsFalse(NoteEditorCtrl.CanReadNow(added));
        Assert.IsFalse(NoteEditorCtrl.CanReadNow(null!));
    }
}
