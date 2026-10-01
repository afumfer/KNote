using KNote.ClientWin.Utils;

namespace KNote.ClientWin.Tests;

[TestClass]
public class ControlLayoutTests
{
    [TestMethod]
    public void AlignToTextBox_TakesTextBoxTopAndHeight_KeepsHorizontalPlacement()
    {
        using var textBox = new TextBox { Location = new Point(10, 32), Width = 200 };
        using var button = new Button { Location = new Point(216, 28), Size = new Size(27, 26) };

        button.AlignToTextBox(textBox);

        Assert.AreEqual(textBox.Top, button.Top);
        Assert.AreEqual(textBox.Height, button.Height);
        Assert.AreEqual(216, button.Left);
        Assert.AreEqual(27, button.Width);
    }
}
