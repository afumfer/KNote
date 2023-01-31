using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

public partial class AttributeEditorForm : Form, IEditorView<KAttributeDto>
{
    private readonly AttributeEditorComponent _com;
    private bool _viewFinalized = false;
    private bool _formIsDisty = false;

    public AttributeEditorForm(AttributeEditorComponent com)
    {
        InitializeComponent();
        _com = com;
    }

    #region IEditorView implementation

    public Control PanelView()
    {
        return panelForm;
    }

    public void ShowView()
    {
        this.Show();
    }

    public Result<EComponentResult> ShowModalView()
    {
        var res = _com.DialogResultToComponentResult(this.ShowDialog());
        return res;
    }

    public DialogResult ShowInfo(string info, string caption = "KaNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        return MessageBox.Show(info, caption, buttons, icon);
    }

    public void RefreshView()
    {
        //ModelToControls();
    }

    public void RefreshModel()
    {
        //ControlsToModel();
    }


    public void CleanView()
    {
        //textXxxxx.Text = "";
    }

    public void ConfigureEmbededMode()
    {
        
    }

    public void ConfigureWindowMode()
    {
        
    }

    public void OnClosingView()
    {
        _viewFinalized = true;
        this.Close();
    }

    #endregion 

    private void AttributeEditorForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
        {
            var confirmExit = OnCandelEdition();
            if (!confirmExit)
                e.Cancel = true;
        }
    }

    private bool OnCandelEdition()
    {
        if (_formIsDisty)
        {
            if (MessageBox.Show("You have modified this entity, are you sure you want to exit without recording?", "KaNote", MessageBoxButtons.YesNo) == DialogResult.No)
                return false;
        }

        this.DialogResult = DialogResult.Cancel;
        _com.CancelEdition();
        return true;
    }
}
