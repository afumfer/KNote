using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace KntIcons;

/// <summary>
/// Renders <see cref="KntIcon"/>s from the embedded Fluent UI System Icons font into bitmaps of the exact device
/// size for a DPI, so they stay sharp at any Windows scale instead of stretching a fixed-size PNG.
/// </summary>
/// <remarks>
/// Bitmaps are cached and shared between every control that shows the same icon: never dispose them.
/// </remarks>
public static class KntIconProvider
{
    /// <summary>Logical (100 %) size of toolbar, menu, tab and tree icons.</summary>
    public const int DefaultSize = 16;

    private const string FontResourceName = "KntIcons.FluentSystemIcons-Regular.ttf";

    private static readonly PrivateFontCollection Fonts = new();
    private static readonly Lazy<FontFamily> IconFontFamily = new(LoadFontFamily);
    private static readonly Lazy<int> LazySystemDpi = new(ReadSystemDpi);
    private static readonly Dictionary<(KntIcon Icon, int LogicalSize, int PixelSize), Bitmap> Cache = new();

    /// <summary>
    /// DPI the process renders at (the app is SystemAware): fallback for callers with no control at hand.
    /// </summary>
    public static int SystemDpi => LazySystemDpi.Value;

    public static int ToDevicePixels(int logicalSize, int dpi) =>
        (int)Math.Round(logicalSize * dpi / 96.0, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Returns the icon drawn in a square of <paramref name="logicalSize"/> logical pixels scaled to
    /// <paramref name="dpi"/>. The same arguments always return the same (shared) bitmap.
    /// </summary>
    public static Bitmap GetBitmap(KntIcon icon, int logicalSize, int dpi)
    {
        int pixelSize = ToDevicePixels(logicalSize, dpi);
        var key = (icon, logicalSize, pixelSize);

        lock (Cache)
        {
            if (!Cache.TryGetValue(key, out var bitmap))
            {
                bitmap = Render(KntIconCatalog.Get(icon), logicalSize, pixelSize);
                Cache.Add(key, bitmap);
            }
            return bitmap;
        }
    }

    /// <summary>
    /// Creates a 32-bit <see cref="ImageList"/> with <paramref name="icons"/> in that order (index i is
    /// icons[i], and each image's key is its <see cref="KntIcon"/> name). The caller owns the list.
    /// </summary>
    public static ImageList CreateImageList(int logicalSize, int dpi, params KntIcon[] icons)
    {
        int pixelSize = ToDevicePixels(logicalSize, dpi);
        var imageList = new ImageList
        {
            ColorDepth = ColorDepth.Depth32Bit,
            ImageSize = new Size(pixelSize, pixelSize)
        };
        foreach (var icon in icons)
            imageList.Images.Add(icon.ToString(), GetBitmap(icon, logicalSize, dpi));
        return imageList;
    }

    private static Bitmap Render(KntIconGlyph glyph, int logicalSize, int pixelSize)
    {
        // Each design size is drawn on its own pixel grid; use the 16 px one wherever it fits.
        int codepoint = logicalSize <= 16 && glyph.Glyph16 != 0 ? glyph.Glyph16 : glyph.Glyph20;

        var family = IconFontFamily.Value;
        float emHeight = family.GetEmHeight(FontStyle.Regular);
        float cellAscent = family.GetCellAscent(FontStyle.Regular);

        // Fluent glyphs fill the em square from the baseline up, but AddString places the origin at the top
        // of the (taller) cell ascent: move up by the difference so the em square matches the bitmap.
        float top = -(cellAscent - emHeight) / emHeight * pixelSize;

        using var path = new GraphicsPath();
        path.AddString(char.ConvertFromUtf32(codepoint), family, (int)FontStyle.Regular, pixelSize,
            new PointF(0, top), StringFormat.GenericTypographic);

        var bitmap = new Bitmap(pixelSize, pixelSize, PixelFormat.Format32bppArgb);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        // Pixel (x, y) covers [x, x + 1]: edges on whole coordinates stay crisp instead of half-covered.
        graphics.PixelOffsetMode = PixelOffsetMode.Half;
        using var brush = new SolidBrush(glyph.Color);
        graphics.FillPath(brush, path);

        return bitmap;
    }

    private static FontFamily LoadFontFamily()
    {
        using var stream = typeof(KntIconProvider).Assembly.GetManifestResourceStream(FontResourceName)
            ?? throw new InvalidOperationException($"Embedded resource {FontResourceName} not found.");
        var data = new byte[stream.Length];
        stream.ReadExactly(data);

        // AddMemoryFont needs the font data to stay valid for as long as the font is used, which is the whole
        // process lifetime here: this memory is deliberately never freed.
        IntPtr fontData = Marshal.AllocCoTaskMem(data.Length);
        Marshal.Copy(data, 0, fontData, data.Length);
        Fonts.AddMemoryFont(fontData, data.Length);

        return Fonts.Families[0];
    }

    private static int ReadSystemDpi()
    {
        using var graphics = Graphics.FromHwnd(IntPtr.Zero);
        return (int)Math.Round(graphics.DpiX);
    }
}
