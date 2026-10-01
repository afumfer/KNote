using System.Drawing;
using KNote.ClientWin.Utils;

namespace KNote.ClientWin.Tests;

[TestClass]
public class WindowPlacementTests
{
    // Primary 1920x1040 working area, plus a second monitor on its left (negative coordinates).
    private static readonly Rectangle Primary = new(0, 0, 1920, 1040);
    private static readonly Rectangle LeftMonitor = new(-1280, 0, 1280, 984);
    private static readonly IReadOnlyList<Rectangle> Screens = new[] { Primary, LeftMonitor };

    [TestMethod]
    public void EnsureVisible_WindowOnScreen_IsUnchanged()
    {
        var bounds = new Rectangle(300, 200, 800, 350);

        Assert.AreEqual(bounds, WindowPlacement.EnsureVisible(bounds, Screens));
    }

    [TestMethod]
    public void EnsureVisible_WindowOnMonitorWithNegativeCoordinates_IsUnchanged()
    {
        var bounds = new Rectangle(-1000, 100, 800, 350);

        Assert.AreEqual(bounds, WindowPlacement.EnsureVisible(bounds, Screens));
    }

    [TestMethod]
    public void EnsureVisible_WindowPartlyOffScreenButCaptionReachable_IsUnchanged()
    {
        var bounds = new Rectangle(1700, 900, 800, 350);

        Assert.AreEqual(bounds, WindowPlacement.EnsureVisible(bounds, Screens));
    }

    [TestMethod]
    public void EnsureVisible_WindowBeyondAllScreens_MovesIntoNearestScreenKeepingSize()
    {
        // Saved when the desktop was larger (e.g. primary monitor at 100 % instead of 200 %).
        var bounds = new Rectangle(3000, 1500, 800, 350);

        var result = WindowPlacement.EnsureVisible(bounds, Screens);

        Assert.AreEqual(new Rectangle(1120, 690, 800, 350), result);
    }

    [TestMethod]
    public void EnsureVisible_CaptionAboveScreen_MovesDown()
    {
        var bounds = new Rectangle(300, -500, 800, 350);

        var result = WindowPlacement.EnsureVisible(bounds, Screens);

        Assert.AreEqual(new Rectangle(300, 0, 800, 350), result);
    }

    [TestMethod]
    public void EnsureVisible_WindowLargerThanNearestScreen_IsShrunkToFit()
    {
        var bounds = new Rectangle(5000, 0, 2500, 1500);

        var result = WindowPlacement.EnsureVisible(bounds, Screens);

        Assert.AreEqual(Primary, result);
    }
}
