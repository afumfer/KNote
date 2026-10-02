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
    public void GetBitmap_ColorOverride_DrawsInThatColor()
    {
        var light = KntIconProvider.GetBitmap(KntIcon.FolderOpen, 16, 96, Color.White);
        var catalog = KntIconProvider.GetBitmap(KntIcon.FolderOpen, 16, 96);

        var opaque = OpaquePixels(light).ToList();
        Assert.AreNotSame(catalog, light);
        Assert.IsTrue(opaque.Count > 0);
        Assert.IsTrue(opaque.All(c => c.R == 255 && c.G == 255 && c.B == 255));
    }

    [TestMethod]
    [DynamicData(nameof(AllIcons))]
    public void GetColor_DarkBackground_KeepsEnoughContrastOverTheDarkModeBackground(KntIcon icon)
    {
        // WCAG minimum for graphical objects (3:1), over the darkest background of dark mode.
        var background = Color.FromArgb(0x1F, 0x1F, 0x1F);

        var ratio = ContrastRatio(KntIconProvider.GetColor(icon, darkBackground: true), background);

        Assert.IsTrue(ratio >= 3, $"{icon}: contrast {ratio:0.00} over the dark background");
    }

    [TestMethod]
    [DynamicData(nameof(AllIcons))]
    public void GetColor_DarkBackground_IsLighterThanTheLightModeColor(KntIcon icon)
    {
        var light = KntIconProvider.GetColor(icon, darkBackground: false);
        var dark = KntIconProvider.GetColor(icon, darkBackground: true);

        Assert.IsTrue(RelativeLuminance(dark) > RelativeLuminance(light), $"{icon}: dark mode color is not lighter");
    }

    [TestMethod]
    [DataRow(96, 16, 32)]
    [DataRow(192, 32, 64)]
    public void CreateIcon_HasFramesForSmallAndLargeIconSizes(int dpi, int smallPixels, int largePixels)
    {
        using var icon = KntIconProvider.CreateIcon(KntIcon.Alarm, dpi);
        using var small = new Icon(icon, smallPixels, smallPixels);
        using var large = new Icon(icon, largePixels, largePixels);

        Assert.AreEqual(smallPixels, small.Width);
        Assert.AreEqual(largePixels, large.Width);
        Assert.AreNotEqual(IntPtr.Zero, large.Handle);
    }

    [TestMethod]
    public void SetKntIcons_ImageList_KeepsOrderAndDeviceSize()
    {
        using var imageList = new ImageList { ColorDepth = ColorDepth.Depth8Bit };

        imageList.SetKntIcons(16, 192, KntIcon.FolderOpen, KntIcon.Folder, KntIcon.Repository);

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

    private static double ContrastRatio(Color a, Color b)
    {
        double la = RelativeLuminance(a), lb = RelativeLuminance(b);
        return (Math.Max(la, lb) + 0.05) / (Math.Min(la, lb) + 0.05);
    }

    private static double RelativeLuminance(Color color)
    {
        static double Channel(byte value)
        {
            double c = value / 255.0;
            return c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        }
        return 0.2126 * Channel(color.R) + 0.7152 * Channel(color.G) + 0.0722 * Channel(color.B);
    }

    private static bool HasVisiblePixels(Bitmap bitmap)
    {
        for (int y = 0; y < bitmap.Height; y++)
            for (int x = 0; x < bitmap.Width; x++)
                if (bitmap.GetPixel(x, y).A > 0)
                    return true;
        return false;
    }

    private static IEnumerable<Color> OpaquePixels(Bitmap bitmap)
    {
        for (int y = 0; y < bitmap.Height; y++)
            for (int x = 0; x < bitmap.Width; x++)
            {
                var pixel = bitmap.GetPixel(x, y);
                if (pixel.A == 255)
                    yield return pixel;
            }
    }
}
