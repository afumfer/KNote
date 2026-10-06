using System.ComponentModel;
using System.Reflection;

namespace KNote.ClientWin.Utils;

/// <summary>
/// ContextMenuStrip laid out for the screen's scaling. WinForms' ContextMenuStrip (through the public
/// ToolStripDropDownMenu constructor) sizes its image/check margin, default image size and paddings for 96 DPI
/// and only rescales them on a DPI change in PerMonitorV2 mode, which this app doesn't use (it's SystemAware).
/// So at 200 % the margin's gradient bar ends up narrower than the check box drawn over it. The menus' own
/// drop-downs (ToolStripMenuItem.DropDown) don't need this: WinForms creates them for the system DPI.
/// </summary>
public class KntContextMenuStrip : ContextMenuStrip
{
    // The private method that WinForms uses to rescale those values; there's no public way to do it. If a
    // future version renames it, the menu is simply laid out as before (a test in ClientWin.Tests warns of it).
    internal static readonly MethodInfo ScaleConstantsMethod = typeof(ToolStripDropDownMenu).GetMethod(
        "ScaleConstants", BindingFlags.Instance | BindingFlags.NonPublic, [typeof(int)]);

    public KntContextMenuStrip() => ScaleToDeviceDpi();

    public KntContextMenuStrip(IContainer container) : base(container) => ScaleToDeviceDpi();

    private void ScaleToDeviceDpi()
    {
        if (DeviceDpi != 96)
            ScaleConstantsMethod?.Invoke(this, [DeviceDpi]);
    }
}
