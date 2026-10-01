using System.Drawing;

namespace KntIcons;

/// <summary>
/// Assigns <see cref="KntIcon"/>s to WinForms controls at the DPI of the control that shows them.
/// </summary>
public static class KntIconExtensions
{
    /// <summary>
    /// Sets the item's image. <see cref="ToolStripItemImageScaling.None"/> because the bitmap already has the
    /// final device size: letting the ToolStrip stretch it to ImageScalingSize would blur it again.
    /// </summary>
    public static void SetKntIcon(this ToolStripItem item, KntIcon icon,
        int logicalSize = KntIconProvider.DefaultSize, Color? color = null)
    {
        int dpi = item.Owner?.DeviceDpi ?? KntIconProvider.SystemDpi;
        item.Image = KntIconProvider.GetBitmap(icon, logicalSize, dpi, color);
        item.ImageScaling = ToolStripItemImageScaling.None;
    }

    public static void SetKntIcon(this ButtonBase button, KntIcon icon,
        int logicalSize = KntIconProvider.DefaultSize, Color? color = null)
    {
        button.Image = KntIconProvider.GetBitmap(icon, logicalSize, button.DeviceDpi, color);
    }

    public static void SetKntIcon(this PictureBox pictureBox, KntIcon icon,
        int logicalSize = KntIconProvider.DefaultSize, Color? color = null)
    {
        pictureBox.Image = KntIconProvider.GetBitmap(icon, logicalSize, pictureBox.DeviceDpi, color);
        pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
    }

    /// <summary>
    /// Replaces the list's images with <paramref name="icons"/>, in that order (index i is icons[i], and each
    /// image's key is its <see cref="KntIcon"/> name), as 32-bit images of the device size for
    /// <paramref name="dpi"/>. Meant for an ImageList created by the designer, so its form still disposes it.
    /// </summary>
    public static void SetKntIcons(this ImageList imageList, int logicalSize, int dpi, params KntIcon[] icons)
    {
        int pixelSize = KntIconProvider.ToDevicePixels(logicalSize, dpi);

        // Changing ColorDepth/ImageSize recreates the native list (dropping its images): set them first.
        imageList.Images.Clear();
        imageList.ColorDepth = ColorDepth.Depth32Bit;
        imageList.ImageSize = new Size(pixelSize, pixelSize);
        foreach (var icon in icons)
            imageList.Images.Add(icon.ToString(), KntIconProvider.GetBitmap(icon, logicalSize, dpi));
    }
}
