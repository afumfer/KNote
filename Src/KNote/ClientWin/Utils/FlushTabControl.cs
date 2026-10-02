namespace KNote.ClientWin.Utils;

/// <summary>
/// TabControl whose pages, in dark mode, fill the whole inside of its frame. A TabControl keeps a few pixels
/// of margin between its pages and its frame; in dark mode that strip has the page's color, so next to a
/// darker scroll bar of the page's content (e.g. the folders tree) it looks like a gap. In light mode the
/// margin is left as is: there it matches the scroll bar, and without it the page would cover the frame.
/// </summary>
public class FlushTabControl : TabControl
{
    public override Rectangle DisplayRectangle
    {
        get
        {
            var pages = base.DisplayRectangle;
            if (!Application.IsDarkModeEnabled)
                return pages;

            // Only the frame's own pixel is left on the left, right and bottom; the tabs row is untouched.
            const int frame = 1;
            return Rectangle.FromLTRB(frame, pages.Top, Width - frame, Height - frame);
        }
    }
}
