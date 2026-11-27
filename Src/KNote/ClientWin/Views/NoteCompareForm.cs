using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using Microsoft.Extensions.Logging;

namespace KNote.ClientWin.Views;

// «Compare» (NoteCompareCtrl, the history, H22): a window of its own, built here and not in the Designer. On top, «Compare with the
// current note»; below, the comparison in a web view without scripts (NoteCompareCtrl.Html, every text escaped). Shown when the
// comparison was read (NoteEditorCtrl.CompareAsync).
public class NoteCompareForm : KntForm, IViewNoteCompare
{
    #region Private fields

    private readonly NoteCompareCtrl _ctrl;
    private readonly KntWebView.KntEditView _comparison;
    private readonly CheckBox _withCurrentNote;
    private bool _shown;
    private int _version;

    #endregion

    #region Constructor

    public NoteCompareForm(NoteCompareCtrl ctrl)
    {
        _ctrl = ctrl;

        SuspendLayout();
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(900, 640);
        MinimumSize = new Size(480, 360);
        ShowInTaskbar = true;

        _withCurrentNote = new CheckBox { Text = "Compare with the current note", AutoSize = true };
        _withCurrentNote.CheckedChanged += async (s, e) => await ReloadAsync();
        var header = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(10, 6, 10, 4) };
        header.Controls.Add(_withCurrentNote);

        _comparison = new KntWebView.KntEditView
        {
            Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, ContentScriptsEnabled = false
        };

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Padding = new Padding(6) };
        var close = FooterButtons.Create("Close");
        close.DialogResult = DialogResult.Cancel;
        close.Click += (s, e) => Close();
        buttons.Controls.Add(close);
        CancelButton = close;

        Controls.Add(_comparison);
        Controls.Add(buttons);
        Controls.Add(header);
        ResumeLayout(true);
    }

    #endregion

    #region IViewNoteCompare implementation

    public override void RefreshView()
    {
        Text = $"{_ctrl.Title} · {_ctrl.Subtitle}";
        var comparison = _ctrl.Comparison;
        if (comparison == null)
            return;
        // Without the copy before the change it was compared with the note now: nothing else to choose.
        _withCurrentNote.Enabled = comparison.CanCompareChange;
        if (_withCurrentNote.Checked != comparison.WithCurrentNote)
        {
            _shown = false;
            _withCurrentNote.Checked = comparison.WithCurrentNote;
            _shown = true;
        }
    }

    #endregion

    #region Form event handlers

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        FooterButtons.FitTexts(this);
        RefreshView();
    }

    // The web view starts when the window is shown, not inside its own callbacks (nothing modal after them).
    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        _shown = true;
        await ShowComparisonAsync();
    }

    protected override void OnUserClosing(FormClosingEventArgs e) => _ctrl.Finalize();

    #endregion

    private async Task ReloadAsync()
    {
        if (!_shown)
            return;
        var version = ++_version;
        _withCurrentNote.Enabled = false;
        var loaded = await _ctrl.LoadAsync(_withCurrentNote.Checked);
        if (IsDisposed || version != _version)
            return;
        if (!loaded)
        {
            ShowInfo(_ctrl.Error);
            _withCurrentNote.Enabled = true;
            return;
        }
        RefreshView();
        await ShowComparisonAsync();
    }

    private async Task ShowComparisonAsync()
    {
        try
        {
            await _comparison.ShowNavigationContent(_ctrl.Html() + _ctrl.Store.KNoteWebViewStyle);
        }
        catch (Exception ex)
        {
            _ctrl.Store.Logger?.LogError(ex, "The comparison of two versions of a note could not be shown.");
        }
    }
}
