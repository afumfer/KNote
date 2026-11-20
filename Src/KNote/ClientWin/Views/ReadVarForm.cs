using KntScript;

namespace KNote.ClientWin.Views;

public partial class ReadVarForm : KntForm
{
    #region Private members

    TextBox textFirst;

    #endregion 

    #region Properties

    private List<ReadVarItem> _readVarItems;
    internal List<ReadVarItem> ReadVarItems
    {
        get { return _readVarItems; }
    }

    #endregion

    #region Constructor

    public ReadVarForm(List<ReadVarItem> readVarItems)
    {
        InitializeComponent();

        _readVarItems = readVarItems;

        GenControls();
    }

    #endregion

    #region Form events handler

    private void ReadVarForm_Shown(object sender, EventArgs e)
    {
        if (textFirst != null)        
            textFirst.Focus();        
    }

    private void buttonAccept_Click(object sender, EventArgs e)
    {
        CaptureVars();
        DialogResult = System.Windows.Forms.DialogResult.OK;
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
        DialogResult = System.Windows.Forms.DialogResult.Cancel;
    }

    #endregion

    #region Private methods

    private readonly List<(Label Label, TextBox Text)> _rows = new();

    // One row per variable: its label and its box. Placed in OnLoad (LayoutRows), not here.
    private void GenControls()
    {
        foreach (ReadVarItem var in _readVarItems)
        {
            var label = new Label { AutoSize = true, Text = var.Label?.ToString() ?? "" };
            var textBox = new TextBox
            {
                Tag = var.VarIdent,
                Text = var.VarValue?.ToString() ?? "",
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            panelControls.Controls.Add(label);
            panelControls.Controls.Add(textBox);
            _rows.Add((label, textBox));
        }

        textFirst = _rows.Count > 0 ? _rows[0].Text : null;
    }

    protected override void OnLoad(EventArgs e)
    {
        // Before base.OnLoad, which centers the dialog with its size.
        LayoutRows();
        base.OnLoad(e);
    }

    // The rows are created in code, so WinForms does not scale them: they are measured once the form has its final
    // scale (fixed pixels left a cut label and a tiny box above 100 %). The margin is the one of the buttons in the
    // Designer; the boxes start after the widest label and fill the width; the dialog is as tall as its rows.
    private void LayoutRows()
    {
        if (_rows.Count == 0)
            return;

        var margin = panelButtons.ClientSize.Width - buttonCancel.Right;
        var labelsWidth = _rows.Max(r => r.Label.PreferredWidth);
        var textLeft = 2 * margin + labelsWidth;
        var minTextWidth = 2 * labelsWidth;
        if (ClientSize.Width < textLeft + minTextWidth + margin)
            ClientSize = new Size(textLeft + minTextWidth + margin, ClientSize.Height);

        var top = 2 * margin;
        foreach (var (label, textBox) in _rows)
        {
            textBox.Bounds = new Rectangle(textLeft, top, panelControls.ClientSize.Width - textLeft - margin, textBox.Height);
            label.Location = new Point(margin, top + (textBox.Height - label.Height) / 2);
            top = textBox.Bottom + margin;
        }

        // Many rows (a KntScript readvar) scroll instead of making the dialog taller than the screen.
        var rowsHeight = top + margin;
        var maxHeight = Screen.FromControl(this).WorkingArea.Height * 3 / 4 - (Height - ClientSize.Height) - panelButtons.Height;
        panelControls.AutoScroll = rowsHeight > maxHeight;
        ClientSize = new Size(ClientSize.Width, Math.Min(rowsHeight, maxHeight) + panelButtons.Height);
    }

    private void CaptureVars()
    {
        foreach (Control c in panelControls.Controls)
        {
            foreach (ReadVarItem var in _readVarItems)
            {
                if (var.VarIdent == (string)c.Tag)
                {
                    var.VarNewValueText = c.Text;
                    break;
                }
            }
        }
    }

    #endregion

}
