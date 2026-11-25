using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using Microsoft.Extensions.Logging;

namespace KNote.ClientWin.Views;

// A version of a note (NoteVersionCtrl, the history, H.3): a window of its own, read only, built here and not in the Designer. On
// top, what changed and the note as it was (topic, folder, type, tags, priority, attributes) and, if copies in between were not
// kept, the notice; below, its description in a web view with the same rules as the note editor (made safe, no scripts but the
// ones of its diagrams). Shown when the version was read (NoteEditorCtrl.ShowVersionAsync).
public class NoteVersionForm : KntForm, IViewNoteVersion
{
    #region Private fields

    private readonly NoteVersionCtrl _ctrl;
    private readonly KntWebView.KntEditView _description;
    private readonly Label _notice;
    private readonly Label _change;
    private readonly Label _topic;
    private readonly Label _meta;
    private readonly Label _attributes;

    #endregion

    #region Constructor

    public NoteVersionForm(NoteVersionCtrl ctrl)
    {
        _ctrl = ctrl;

        SuspendLayout();
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(820, 640);
        MinimumSize = new Size(480, 360);
        ShowInTaskbar = true;

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 1,
            Padding = new Padding(10, 8, 10, 6)
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _notice = NewLabel();
        _notice.BackColor = SystemColors.Info;
        _notice.ForeColor = SystemColors.InfoText;
        _notice.Padding = new Padding(6, 4, 6, 4);
        _change = NewLabel();
        _change.ForeColor = SystemColors.GrayText;
        _topic = NewLabel();
        _topic.Font = new Font(Font.FontFamily, Font.Size * 1.25f, FontStyle.Bold);
        _meta = NewLabel();
        _meta.ForeColor = SystemColors.GrayText;
        _attributes = NewLabel();
        foreach (var label in new[] { _notice, _change, _topic, _meta, _attributes })
            header.Controls.Add(label);

        _description = new KntWebView.KntEditView
        {
            Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, ContentScriptsEnabled = false, RenderDiagrams = true
        };

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Padding = new Padding(6) };
        var close = new Button { Text = "Close", AutoSize = true, UseVisualStyleBackColor = true, DialogResult = DialogResult.Cancel };
        close.Click += (s, e) => Close();
        buttons.Controls.Add(close);
        CancelButton = close;

        Controls.Add(_description);
        Controls.Add(buttons);
        Controls.Add(header);
        ResumeLayout(true);
    }

    #endregion

    #region IViewNoteVersion implementation

    public override void RefreshView()
    {
        var version = _ctrl.Version;
        Text = $"{_ctrl.Title} · {_ctrl.Subtitle}";
        if (version == null)
            return;

        _notice.Text = version.Notice ?? "";
        _notice.Visible = !string.IsNullOrEmpty(version.Notice);
        _change.Text = version.Change ?? "";
        _topic.Text = string.IsNullOrWhiteSpace(version.Topic) ? "Untitled note" : version.Topic;
        var meta = new List<string> { $"Folder: {version.FolderPath}" };
        if (!string.IsNullOrEmpty(version.NoteTypeName))
            meta.Add($"Type: {version.NoteTypeName}");
        if (!string.IsNullOrWhiteSpace(version.Tags))
            meta.Add($"Tags: {version.Tags}");
        meta.Add($"Priority: {version.Priority}");
        _meta.Text = string.Join("    ", meta);
        _attributes.Text = string.Join(Environment.NewLine, version.Attributes.Select(a => $"{a.Name}: {a.Value}"));
        _attributes.Visible = version.Attributes.Count > 0;
    }

    #endregion

    #region Form event handlers

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        RefreshView();
    }

    // The web view starts when the window is shown, not inside its own callbacks (nothing modal after them).
    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        try
        {
            if (!string.IsNullOrEmpty(_ctrl.ResourcesFolder))
                await _description.SetVirtualHostNameToFolderMapping(_ctrl.ResourcesFolder);
            var html = _ctrl.DescriptionHtml();
            await _description.ShowNavigationContent((html ?? "<p><i>No description.</i></p>") + _ctrl.Store.KNoteWebViewStyle);
        }
        catch (Exception ex)
        {
            _ctrl.Store.Logger?.LogError(ex, "The description of a version of a note could not be shown.");
        }
    }

    protected override void OnUserClosing(FormClosingEventArgs e) => _ctrl.Finalize();

    #endregion

    private static Label NewLabel() => new()
    {
        AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, 2, 0, 2), MaximumSize = new Size(1600, 0)
    };
}
