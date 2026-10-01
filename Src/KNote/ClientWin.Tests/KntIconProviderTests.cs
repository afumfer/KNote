using System.Drawing;
using KntIcons;

namespace KNote.ClientWin.Tests;

[TestClass]
public class KntIconProviderTests
{
    public static IEnumerable<object[]> AllIcons => Enum.GetValues<KntIcon>().Select(icon => new object[] { icon });

    [TestMethod]
    [DynamicData(nameof(AllIcons))]
    public void GetBitmap_EveryIcon_IsMappedAndDrawsSomething(KntIcon icon)
    {
        foreach (var logicalSize in new[] { 16, 20 })
        {
            var bitmap = KntIconProvider.GetBitmap(icon, logicalSize, 96);

            Assert.IsTrue(HasVisiblePixels(bitmap), $"{icon} at {logicalSize} px draws nothing (wrong codepoint?)");
        }
    }

    [TestMethod]
    [DataRow(16, 96, 16)]
    [DataRow(16, 120, 20)]
    [DataRow(16, 144, 24)]
    [DataRow(16, 192, 32)]
    [DataRow(20, 120, 25)]
    [DataRow(20, 168, 35)]
    public void GetBitmap_SizeFollowsDpi(int logicalSize, int dpi, int expectedPixels)
    {
        var bitmap = KntIconProvider.GetBitmap(KntIcon.Save, logicalSize, dpi);

        Assert.AreEqual(new Size(expectedPixels, expectedPixels), bitmap.Size);
    }

    [TestMethod]
    public void GetBitmap_SameArguments_ReturnsSharedInstance()
    {
        var first = KntIconProvider.GetBitmap(KntIcon.Folder, 16, 144);
        var second = KntIconProvider.GetBitmap(KntIcon.Folder, 16, 144);

        Assert.AreSame(first, second);
    }

    [TestMethod]
    public void GetBitmap_DifferentDpi_ReturnsDifferentBitmaps()
    {
        var at100 = KntIconProvider.GetBitmap(KntIcon.Folder, 16, 96);
        var at200 = KntIconProvider.GetBitmap(KntIcon.Folder, 16, 192);

        Assert.AreNotSame(at100, at200);
    }

    [TestMethod]
    public void CreateImageList_KeepsOrderAndDeviceSize()
    {
        using var imageList = KntIconProvider.CreateImageList(16, 192, KntIcon.FolderOpen, KntIcon.Folder, KntIcon.Repository);

        Assert.AreEqual(ColorDepth.Depth32Bit, imageList.ColorDepth);
        Assert.AreEqual(new Size(32, 32), imageList.ImageSize);
        Assert.AreEqual(3, imageList.Images.Count);
        Assert.AreEqual(0, imageList.Images.IndexOfKey(nameof(KntIcon.FolderOpen)));
        Assert.AreEqual(2, imageList.Images.IndexOfKey(nameof(KntIcon.Repository)));
    }

    [TestMethod]
    public void SetKntIcon_ToolStripItem_UsesOwnerDpiWithoutStretching()
    {
        using var toolStrip = new ToolStrip();
        var button = new ToolStripButton();
        toolStrip.Items.Add(button);

        button.SetKntIcon(KntIcon.Save);

        int expectedPixels = KntIconProvider.ToDevicePixels(KntIconProvider.DefaultSize, toolStrip.DeviceDpi);
        Assert.AreEqual(new Size(expectedPixels, expectedPixels), button.Image!.Size);
        Assert.AreEqual(ToolStripItemImageScaling.None, button.ImageScaling);
    }

    private static bool HasVisiblePixels(Bitmap bitmap)
    {
        for (int y = 0; y < bitmap.Height; y++)
            for (int x = 0; x < bitmap.Width; x++)
                if (bitmap.GetPixel(x, y).A > 0)
                    return true;
        return false;
    }
}
