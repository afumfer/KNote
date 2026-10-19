namespace KNote.ClientWin.Utils;

/// <summary>
/// A tooltip for an item of a bar at the bottom of a window (a status bar), shown above the item. A ToolStrip shows
/// its items' tooltips below the cursor: at the bottom of the screen (a maximized window) there is no room, Windows
/// moves them over the cursor, which hides them, and they flash. The text is read each time it is shown, as it may
/// change. The item's own ToolTipText must stay empty.
/// </summary>
public sealed class ToolStripItemToolTip : IDisposable
{
    // Room between the tooltip and the item, and the tooltip's own border and margins around its text.
    private const int Gap = 4;
    private static readonly Size Chrome = new(16, 10);

    private readonly ToolStripItem _item;
    private readonly Func<string> _text;
    private readonly ToolTip _toolTip = new();

    public ToolStripItemToolTip(ToolStripItem item, Func<string> text)
    {
        _item = item;
        _text = text;

        _item.MouseHover += Item_MouseHover;
        _item.MouseLeave += Item_MouseLeave;
    }

    public void Show()
    {
        var owner = _item.Owner;
        var text = _text();
        if (owner == null || string.IsNullOrEmpty(text))
            return;

        // Tooltips are drawn with the system's status font.
        var size = TextRenderer.MeasureText(text, SystemFonts.StatusFont) + Chrome;
        var bounds = _item.Bounds;
        var x = Math.Max(0, bounds.Right - size.Width);
        var y = bounds.Top - size.Height - Gap;

        _toolTip.Show(text, owner, x, y);
    }

    public void Hide()
    {
        if (_item.Owner != null)
            _toolTip.Hide(_item.Owner);
    }

    private void Item_MouseHover(object sender, EventArgs e) => Show();

    private void Item_MouseLeave(object sender, EventArgs e) => Hide();

    public void Dispose()
    {
        _item.MouseHover -= Item_MouseHover;
        _item.MouseLeave -= Item_MouseLeave;
        _toolTip.Dispose();
    }
}
