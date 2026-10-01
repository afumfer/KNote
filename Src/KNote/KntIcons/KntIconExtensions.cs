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
    public static void SetKntIcon(this ToolStripItem item, KntIcon icon, int logicalSize = KntIconProvider.DefaultSize)
    {
        int dpi = item.Owner?.DeviceDpi ?? KntIconProvider.SystemDpi;
        item.Image = KntIconProvider.GetBitmap(icon, logicalSize, dpi);
        item.ImageScaling = ToolStripItemImageScaling.None;
    }

    public static void SetKntIcon(this ButtonBase button, KntIcon icon, int logicalSize = KntIconProvider.DefaultSize)
    {
        button.Image = KntIconProvider.GetBitmap(icon, logicalSize, button.DeviceDpi);
    }

    public static void SetKntIcon(this PictureBox pictureBox, KntIcon icon, int logicalSize = KntIconProvider.DefaultSize)
    {
        pictureBox.Image = KntIconProvider.GetBitmap(icon, logicalSize, pictureBox.DeviceDpi);
        pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
    }
}
