using KNote.ClientWin.Utils;

namespace KNote.ClientWin.Tests;

[TestClass]
public class PostItCaptionIconTests
{
    private static readonly Color SoftYellow = Color.FromArgb(0xFF, 0xE6, 0x80);

    [TestMethod]
    [DataRow("#202020")]
    [DataRow("#000080")]
    [DataRow("#800000")]
    public void GetColor_DarkCaption_IsTheSoftYellow(string caption)
    {
        Assert.AreEqual(SoftYellow.ToArgb(), PostItCaptionIcon.GetColor(ColorTranslator.FromHtml(caption)).ToArgb());
    }

    [TestMethod]
    [DataRow("#FFFF80")]   // the default Post-It caption
    [DataRow("#FFFFC0")]
    [DataRow("#FFFFFF")]
    [DataRow("#C0FFC0")]
    public void GetColor_LightCaption_IsADeeperYellowWithEnoughContrast(string caption)
    {
        var background = ColorTranslator.FromHtml(caption);

        var color = PostItCaptionIcon.GetColor(background);

        Assert.IsTrue(PostItCaptionIcon.RelativeLuminance(color) < PostItCaptionIcon.RelativeLuminance(SoftYellow));
        Assert.IsTrue(Contrast(color, background) >= PostItCaptionIcon.MinContrast, $"{caption}: {color}");
    }

    [TestMethod]
    [DataRow("#FFFF80")]
    [DataRow("#FFFFFF")]
    [DataRow("#808080")]
    public void GetColor_AnyCaption_IsAYellow(string caption)
    {
        var color = PostItCaptionIcon.GetColor(ColorTranslator.FromHtml(caption));

        Assert.AreEqual(48f, color.GetHue(), 1f, $"{caption}: {color}");
        Assert.AreEqual(1f, color.GetSaturation(), 0.01f, $"{caption}: {color}");
    }

    private static double Contrast(Color a, Color b)
        => PostItCaptionIcon.ContrastRatio(PostItCaptionIcon.RelativeLuminance(a), PostItCaptionIcon.RelativeLuminance(b));
}
