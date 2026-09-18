#nullable enable

namespace KNote.ClientWin.Utils;

/// <summary>
/// Single entry point for every MessageBox KNote shows. A plain MessageBox.Show is a normal
/// (non-topmost) window, so a PostIt marked "always visible" (a TopMost form) covers it and the app
/// looks frozen while it waits for an answer nobody can see. When any TopMost window of this process
/// is visible, the dialog is shown TopMost too (MB_TOPMOST); otherwise it behaves exactly like
/// MessageBox.Show. Same overloads as MessageBox.Show, so call sites only change the class name.
/// </summary>
public static class KntMessageBox
{
    // MB_TOPMOST | MB_TASKMODAL: neither is exposed by MessageBoxOptions, but the value is passed
    // straight to the native call. TopMost alone is not enough - see ShowCore.
    private const MessageBoxOptions TopMostOption = (MessageBoxOptions)(0x00040000 | 0x00002000);

    // An owner-less window for MessageBox.Show: a null owner would make WinForms use the active
    // window as owner, and a non-null handle is what MB_TASKMODAL needs to be a NULL hWnd.
    private sealed class NoOwner : IWin32Window
    {
        public IntPtr Handle => IntPtr.Zero;
    }

    public static DialogResult Show(string? text)
        => ShowCore(null, text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None);

    public static DialogResult Show(string? text, string? caption)
        => ShowCore(null, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None);

    public static DialogResult Show(string? text, string? caption, MessageBoxButtons buttons)
        => ShowCore(null, text, caption, buttons, MessageBoxIcon.None);

    public static DialogResult Show(string? text, string? caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        => ShowCore(null, text, caption, buttons, icon);

    public static DialogResult Show(IWin32Window? owner, string? text)
        => ShowCore(owner, text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None);

    public static DialogResult Show(IWin32Window? owner, string? text, string? caption)
        => ShowCore(owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None);

    public static DialogResult Show(IWin32Window? owner, string? text, string? caption, MessageBoxButtons buttons)
        => ShowCore(owner, text, caption, buttons, MessageBoxIcon.None);

    public static DialogResult Show(IWin32Window? owner, string? text, string? caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        => ShowCore(owner, text, caption, buttons, icon);

    internal static MessageBoxOptions BuildOptions(bool anyTopMostWindowVisible)
        => anyTopMostWindowVisible ? TopMostOption : 0;

    private static DialogResult ShowCore(IWin32Window? owner, string? text, string? caption, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
        if (!TopMostWindows.AnyVisible())
            return MessageBox.Show(owner, text, caption, buttons, icon, MessageBoxDefaultButton.Button1, BuildOptions(false));

        // With an owner, clicking the (disabled) owner window while the box is open makes Windows
        // re-stack the box's owner chain, and the box drops below the TopMost PostIt again. Owner-less
        // + task modal keeps the box on top and all of the app's windows disabled instead.
        return MessageBox.Show(new NoOwner(), text, caption, buttons, icon, MessageBoxDefaultButton.Button1, BuildOptions(true));
    }
}
