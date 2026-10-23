using System.ComponentModel;
using System.Runtime.InteropServices;

namespace KntWebView
{
    /// <summary>
    /// Multiline TextBox with space between its border and its text (TextPadding), which a TextBox has no
    /// property for: the text rectangle of the edit control is set instead (EM_SETRECT), inside its client area.
    /// </summary>
    public class KntPaddedTextBox : TextBox
    {
        private Padding _textPadding = Padding.Empty;
        /// <summary>The space around the text, in logical pixels (scaled with the DPI). Empty: the edit control's own.</summary>
        [DefaultValue(typeof(Padding), "0, 0, 0, 0")]
        public Padding TextPadding
        {
            get { return _textPadding; }
            set
            {
                if (_textPadding == value)
                    return;
                var wasSet = _textPadding != Padding.Empty;
                _textPadding = value;
                if (_textPadding != Padding.Empty || wasSet)
                    ApplyTextPadding();
            }
        }

        // The edit control goes back to its own text rectangle on every WM_SIZE and WM_SETFONT, and WinForms
        // doesn't always raise SizeChanged after the WM_SIZE (it depends on when the handle is created and the
        // control reparented), so the padding is set again right after the edit control handles them.
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if ((m.Msg == WM_SIZE || m.Msg == WM_SETFONT) && _textPadding != Padding.Empty)
                ApplyTextPadding();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (_textPadding != Padding.Empty)
                ApplyTextPadding();
        }

        private void ApplyTextPadding()
        {
            if (!IsHandleCreated || !Multiline)
                return;

            if (_textPadding == Padding.Empty)
            {
                // A null rectangle gives the edit control back its own.
                SendMessage(Handle, EM_SETRECT, IntPtr.Zero, IntPtr.Zero);
                return;
            }

            var padding = new Padding(
                LogicalToDeviceUnits(_textPadding.Left), LogicalToDeviceUnits(_textPadding.Top),
                LogicalToDeviceUnits(_textPadding.Right), LogicalToDeviceUnits(_textPadding.Bottom));
            // Not ClientRectangle: during WM_SIZE WinForms still has the previous size, it updates it afterwards.
            GetClientRect(Handle, out var client);
            var rect = new RECT
            {
                Left = client.Left + padding.Left,
                Top = client.Top + padding.Top,
                Right = Math.Max(client.Left + padding.Left, client.Right - padding.Right),
                Bottom = Math.Max(client.Top + padding.Top, client.Bottom - padding.Bottom)
            };
            SendMessage(Handle, EM_SETRECT, IntPtr.Zero, ref rect);
        }

        private const int WM_SIZE = 0x0005;
        private const int WM_SETFONT = 0x0030;
        private const int EM_SETRECT = 0x00B3;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, ref RECT lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);
    }
}
