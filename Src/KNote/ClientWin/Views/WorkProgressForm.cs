using KNote.Model;

namespace KNote.ClientWin.Views;

// A long work (a search or a filter of the notes list, KNote Atlas 4.6b): a small window over its owner with what is being done, a
// moving bar (the work doesn't say how much is left) and «Cancel». It is shown only if the work lasts more than ShowAfter (a quick
// search doesn't flash it), and the owner can't be used meanwhile. Start returns what ends it (Dispose): the work is awaited inside a
// using. «Cancel» cancels the CancellationTokenSource given; what to do then is the work's.
public class WorkProgressForm : KntForm
{
    public static readonly TimeSpan ShowAfter = TimeSpan.FromMilliseconds(400);

    private readonly Label _label;
    private readonly Button _cancel;

    private WorkProgressForm(string title, string text, CancellationTokenSource cancellation)
    {
        Text = string.IsNullOrEmpty(title) ? KntConst.AppName : title;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        ControlBox = false;
        MinimizeBox = MaximizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(LogicalToDeviceUnits(420), LogicalToDeviceUnits(126));
        var margin = LogicalToDeviceUnits(12);

        _label = new Label
        {
            Text = text, AutoEllipsis = true,
            Location = new Point(margin, margin), Size = new Size(ClientSize.Width - 2 * margin, LogicalToDeviceUnits(36))
        };
        var bar = new ProgressBar
        {
            Style = ProgressBarStyle.Marquee, MarqueeAnimationSpeed = 30,
            Location = new Point(margin, _label.Bottom + LogicalToDeviceUnits(4)), Size = new Size(ClientSize.Width - 2 * margin, LogicalToDeviceUnits(14))
        };
        _cancel = new Button
        {
            Text = "Cancel", Size = new Size(LogicalToDeviceUnits(90), LogicalToDeviceUnits(29)),
            Location = new Point(ClientSize.Width - margin - LogicalToDeviceUnits(90), bar.Bottom + LogicalToDeviceUnits(12))
        };
        _cancel.Click += (s, e) =>
        {
            _cancel.Enabled = false;
            _label.Text = "Canceling…";
            cancellation.Cancel();
        };
        CancelButton = _cancel;
        Controls.AddRange([_label, bar, _cancel]);
    }

    /// <summary>
    /// Starts showing the work over <paramref name="owner"/> (after ShowAfter, if it is still running). Disposing the result closes
    /// it (or stops it from being shown) and gives the owner back.
    /// </summary>
    public static IDisposable Start(Control owner, string title, string text, CancellationTokenSource cancellation) =>
        new Showing(owner?.FindForm(), title, text, cancellation);

    private sealed class Showing : IDisposable
    {
        private readonly Form _owner;
        private readonly System.Windows.Forms.Timer _timer;
        private WorkProgressForm _form;
        private bool _ended;

        public Showing(Form owner, string title, string text, CancellationTokenSource cancellation)
        {
            _owner = owner;
            _timer = new System.Windows.Forms.Timer { Interval = (int)ShowAfter.TotalMilliseconds };
            _timer.Tick += (s, e) =>
            {
                _timer.Stop();
                if (_ended || _owner == null || _owner.IsDisposed)
                    return;
                _form = new WorkProgressForm(title, text, cancellation);
                _owner.Enabled = false;
                _form.Show(_owner);
            };
            _timer.Start();
        }

        public void Dispose()
        {
            if (_ended)
                return;
            _ended = true;
            _timer.Stop();
            _timer.Dispose();
            if (_form != null)
            {
                if (_owner != null && !_owner.IsDisposed)
                    _owner.Enabled = true;
                _form.Close();
                _form.Dispose();
                _owner?.Activate();
            }
        }
    }
}
