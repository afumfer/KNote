using System.Drawing.Drawing2D;

namespace KNote.ClientWin.Utils;

/// <summary>
/// KNote's application icon (Resources\Icons\KNote.ico, also the executable's icon), loaded once for every
/// window, the tray icon and the splash/about pictures. Its frames go from 16 to 256 px so Windows has a
/// sharp one at any scale.
/// </summary>
public static class AppIcon
{
    private const string ResourceName = "KNote.ClientWin.Resources.Icons.KNote.ico";

    // System.Drawing.Icon can't select the 256 px frame (its size is stored as 0 in the .ico directory, which
    // only the Windows shell reads as 256), so 255 picks the largest frame it can load (192 px).
    private const int LargestLoadableSize = 255;

    private static readonly Lazy<Icon> LazyIcon = new(Load);

    /// <summary>Shared instance: assign it freely, never dispose it.</summary>
    public static Icon Icon => LazyIcon.Value;

    /// <summary>The icon as a bitmap of exactly <paramref name="pixelSize"/> pixels; the caller owns it.</summary>
    public static Bitmap GetBitmap(int pixelSize)
    {
        using var frame = new Icon(Icon, LargestLoadableSize, LargestLoadableSize);
        using var source = frame.ToBitmap();

        var bitmap = new Bitmap(pixelSize, pixelSize);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.DrawImage(source, 0, 0, pixelSize, pixelSize);
        return bitmap;
    }

    /// <summary>Shows the icon centered in <paramref name="pictureBox"/>, as large as the box allows.</summary>
    public static void ShowIn(PictureBox pictureBox)
    {
        pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
        pictureBox.Image = GetBitmap(Math.Min(pictureBox.Width, pictureBox.Height));
    }

    private static Icon Load()
    {
        using var stream = typeof(AppIcon).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded resource {ResourceName} not found.");
        return new Icon(stream);
    }
}
