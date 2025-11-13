using KntScript;

namespace KNote.ClientWin.Views;

public partial class ReadVarForm : Form
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
        AutoScaleMode = AutoScaleMode.Dpi;

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

    private void GenControls()
    {
        TextBox textBox;
        Label label;
        int topText = 15;
        int leftText = 210;
        int topLabel = 15;
        int leftLabel = 10;
        int widthLabel = 200;
        int widthText = 350;
        int inc = 30;
        int i = 0;

        foreach (ReadVarItem var in _readVarItems)
        {
            label = new Label();
            label.Font = new Font(new FontFamily("Courier New"), 10);
            label.Text = var.Label.ToString();
            label.Top = topLabel;
            label.Left = leftLabel;
            label.Width = widthLabel;

            textBox = new TextBox();
            textBox.Tag = var.VarIdent;
            textBox.Text = var.VarValue.ToString();
            textBox.Top = topText;
            textBox.Left = leftText;
            textBox.Width = widthText;
            textBox.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right)));

            panelControls.Controls.Add(label);
            panelControls.Controls.Add(textBox);

            if (i == 0)
                textFirst = textBox;

            topText += inc;
            topLabel += inc;
            i++;
        }
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
