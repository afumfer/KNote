using KntIcons;

namespace KNote.ClientWin.Utils;

/// <summary>
/// The app's ToolStrip renderer (ToolStripManager.Renderer, see AppTheme.Apply): the professional one, with
/// the menu items' check mark drawn from the KntIcons glyph. WinForms' own check image is a bitmap copied
/// unscaled into the check rectangle (ToolStripRenderer.OnRenderItemCheck), so at high scaling, when that
/// rectangle is smaller than the bitmap, the check mark comes out cropped.
/// </summary>
internal class KntToolStripRenderer : ToolStripProfessionalRenderer
{
    protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
    {
        if (SystemInformation.HighContrast || e.ImageRectangle.IsEmpty)
        {
            base.OnRenderItemCheck(e);
            return;
        }

        // Without an image the base renderer only paints the check background.
        base.OnRenderItemCheck(new ToolStripItemImageRenderEventArgs(e.Graphics, e.Item, null, e.ImageRectangle));

        var item = e.Item;
        if (item is not ToolStripMenuItem menuItem || menuItem.CheckState == CheckState.Unchecked)
            return;

        // Same fill as the check background, to pick a check color that contrasts with it.
        var background = item.Pressed ? ColorTable.CheckPressedBackground
            : item.Selected ? ColorTable.CheckSelectedBackground
            : ColorTable.CheckBackground;
        var color = !item.Enabled ? SystemColors.GrayText
            : background.GetBrightness() < 0.5f ? Color.White : Color.Black;

        int dpi = e.ToolStrip?.DeviceDpi ?? KntIconProvider.SystemDpi;
        var glyph = KntIconProvider.GetBitmap(KntIcon.Check, KntIconProvider.DefaultSize, dpi, color);

        // Centered, and scaled down only if the rectangle is smaller than the glyph.
        var rect = e.ImageRectangle;
        int side = Math.Min(glyph.Width, Math.Min(rect.Width, rect.Height));
        var target = new Rectangle(rect.Left + (rect.Width - side) / 2, rect.Top + (rect.Height - side) / 2, side, side);
        e.Graphics.DrawImage(glyph, target);
    }
}
