using KNote.ClientWin.Views;

namespace KNote.ClientWin.Tests;

[TestClass]
public class KntEditorFormTests
{
    [TestMethod]
    [DataRow('a')]
    [DataRow('Z')]
    [DataRow('7')]
    [DataRow(' ')]
    [DataRow('\b')]
    public void ChangesContent_TypedCharacters_AreChanges(char key)
    {
        Assert.IsTrue(KntEditorForm.ChangesContent(key, focusIsMultilineText: false));
        Assert.IsTrue(KntEditorForm.ChangesContent(key, focusIsMultilineText: true));
    }

    [TestMethod]
    [DataRow('\t')]
    [DataRow('')]
    public void ChangesContent_TabAndEscape_AreNeverAChange(char key)
    {
        Assert.IsFalse(KntEditorForm.ChangesContent(key, focusIsMultilineText: false));
        Assert.IsFalse(KntEditorForm.ChangesContent(key, focusIsMultilineText: true));
    }

    [TestMethod]
    public void ChangesContent_Enter_IsAChangeOnlyInAMultilineTextBox()
    {
        Assert.IsFalse(KntEditorForm.ChangesContent('\r', focusIsMultilineText: false));
        Assert.IsTrue(KntEditorForm.ChangesContent('\r', focusIsMultilineText: true));
    }
}
