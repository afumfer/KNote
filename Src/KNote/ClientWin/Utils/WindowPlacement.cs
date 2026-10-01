namespace KNote.ClientWin.Utils;

/// <summary>
/// Keeps a restored window reachable when the saved bounds no longer fall on any screen: a monitor was
/// unplugged, the primary monitor changed, or its Windows scale changed (e.g. 100 % -> 200 %, which
/// shrinks the coordinate space a SystemAware app sees).
/// </summary>
public static class WindowPlacement
{
    // How much of the window's top edge (its caption) must lie on a screen for the user to grab it.
    private const int MinVisibleWidth = 100;
    private const int MinVisibleHeight = 20;

    /// <summary>
    /// Returns <paramref name="bounds"/> unchanged while its caption can still be grabbed on one of the
    /// current screens; otherwise the window moved (and shrunk, if it doesn't fit) into the working area of
    /// the nearest screen.
    /// </summary>
    public static Rectangle EnsureVisible(Rectangle bounds) =>
        EnsureVisible(bounds, Screen.AllScreens.Select(s => s.WorkingArea).ToList());

    internal static Rectangle EnsureVisible(Rectangle bounds, IReadOnlyList<Rectangle> workingAreas)
    {
        var caption = new Rectangle(bounds.X, bounds.Y, bounds.Width, MinVisibleHeight);
        int minVisibleWidth = Math.Min(MinVisibleWidth, bounds.Width);
        foreach (var area in workingAreas)
        {
            var visible = Rectangle.Intersect(caption, area);
            if (visible.Width >= minVisibleWidth && visible.Height >= MinVisibleHeight)
                return bounds;
        }

        var center = new Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);
        var target = workingAreas.MinBy(area => DistanceSquared(center, area));

        int width = Math.Min(bounds.Width, target.Width);
        int height = Math.Min(bounds.Height, target.Height);
        int x = Math.Clamp(bounds.X, target.Left, target.Right - width);
        int y = Math.Clamp(bounds.Y, target.Top, target.Bottom - height);
        return new Rectangle(x, y, width, height);
    }

    private static long DistanceSquared(Point point, Rectangle area)
    {
        long dx = point.X < area.Left ? area.Left - point.X : point.X > area.Right ? point.X - area.Right : 0;
        long dy = point.Y < area.Top ? area.Top - point.Y : point.Y > area.Bottom ? point.Y - area.Bottom : 0;
        return dx * dx + dy * dy;
    }
}
