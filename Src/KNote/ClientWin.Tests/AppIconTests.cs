using KNote.ClientWin.Utils;

namespace KNote.ClientWin.Tests;

[TestClass]
public class AppIconTests
{
    [TestMethod]
    [DataRow(16)]
    [DataRow(32)]
    [DataRow(48)]
    [DataRow(64)]
    [DataRow(96)]
    [DataRow(128)]
    [DataRow(192)]
    public void Icon_HasFrameForEveryStandardSize(int size)
    {
        using var frame = new Icon(AppIcon.Icon, size, size);

        Assert.AreEqual(size, frame.Width);
    }

    [TestMethod]
    public void GetBitmap_ReturnsRequestedSize()
    {
        using var bitmap = AppIcon.GetBitmap(152);

        Assert.AreEqual(new Size(152, 152), bitmap.Size);
    }
}
