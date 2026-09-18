#nullable enable

using System.Runtime.InteropServices;

namespace KNote.ClientWin.Utils;

/// <summary>
/// Detects whether any TopMost window of this process is visible (typically a PostIt marked "always
/// visible"). Native window enumeration instead of Application.OpenForms: callers can run on non-UI
/// threads (e.g. a KntScript running in its own thread), where touching Form/Control properties is
/// not safe.
/// </summary>
public static class TopMostWindows
{
    public static bool AnyVisible()
    {
        var processId = (uint)Environment.ProcessId;
        var found = false;

        EnumWindows((hWnd, _) =>
        {
            GetWindowThreadProcessId(hWnd, out var windowProcessId);
            if (windowProcessId == processId && IsWindowVisible(hWnd) && (GetWindowLong(hWnd, GwlExStyle) & WsExTopMost) != 0)
            {
                found = true;
                return false;
            }
            return true;
        }, IntPtr.Zero);

        return found;
    }

    /// <summary>
    /// Removes the native owner of a window. A TopMost dialog whose owner is a normal window drops
    /// below other TopMost windows (PostIts) as soon as that owner is clicked; without an owner it stays
    /// on top. WinForms still re-enables and re-activates the previous window when the dialog closes.
    /// </summary>
    public static void ClearNativeOwner(IntPtr hWnd)
    {
        SetWindowLongPtr(hWnd, GwlpHwndParent, IntPtr.Zero);
    }

    private const int GwlpHwndParent = -8;
    private const int GwlExStyle = -20;
    private const int WsExTopMost = 0x00000008;

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
}
