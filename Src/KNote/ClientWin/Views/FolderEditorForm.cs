using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

public partial class FolderEditorForm : Form, IViewEditor<FolderDto>
{
    #region Private fields

    private readonly FolderEditorCtrl _ctrl;
    private Guid? _selectedParentFolderId;
    private FolderDto _selectedParentFolder;
    private bool _viewFinalized = false;
    private bool _formIsDisty = false;
    
    #endregion

    #region Constructor 

    public FolderEditorForm(FolderEditorCtrl ctrl)
    {
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        _ctrl = ctrl;
    }

    #endregion

    #region IView implementation 
    
    public void ShowView()
    {
        this.Show();
    }

    public Result<EControllerResult> ShowModalView()
    {
        var res = _ctrl.DialogResultToControllerResult(this.ShowDialog());
        return res;
    }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        return MessageBox.Show(info, caption, buttons, icon);
    }

    public void RefreshView()
    {            
        ModelToControls();
    }

    public void RefreshModel()
    {
        ControlsToModel();
    }

    public void CleanView()
    {
        textName.Text = "";
        textNumber.Text = "";
        textTags.Text = "";
        textOrder.Text = "";
        textOrderNotes.Text = "";
        textParentFolder.Text = "";
    }

    public void OnClosingView()
    {
        _viewFinalized = true;
        this.Close();
    }

    #endregion

    #region Form events handler

    private void FolderEditorForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
        {
            var confirmExit = OnCancelEdition();
            if (!confirmExit)
                e.Cancel = true;
        }
    }

    private async void buttonAccept_Click(object sender, EventArgs e)
    {            
        var res = await _ctrl.SaveModel();
        if (res)
        {
            _formIsDisty = false;
            this.DialogResult = DialogResult.OK;
        }
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
        OnCancelEdition();
    }

    private void FolderEditorForm_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            _formIsDisty = true;
    }

    private void FolderEditorForm_KeyPress(object sender, KeyPressEventArgs e)
    {
        _formIsDisty = true;
    }

    private void buttonFolderSearch_Click(object sender, EventArgs e)
    {
        var folder = _ctrl.GetFolder();
        if (folder != null)
        {
            _selectedParentFolderId = folder.FolderId;
            _selectedParentFolder = folder.GetSimpleDto<FolderDto>();
            textParentFolder.Text = folder?.Name;                
        }
    }

    private void textParentFolder_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete)
        {
            _selectedParentFolder = null;
            _selectedParentFolderId = null;
            textParentFolder.Text = "(root)";
        }
    }

    #endregion

    #region Private methods

    private bool OnCancelEdition()
    {
        if (_formIsDisty)
        {
            if (MessageBox.Show("You have modified this entity, are you sure you want to exit without recording?", KntConst.AppName, MessageBoxButtons.YesNo) == DialogResult.No)
                return false;
        }

        this.DialogResult = DialogResult.Cancel;
        _ctrl.CancelEdition();
        return true;
    }

    private void ModelToControls()
    {
        textName.Text = _ctrl.Model.Name;
        textNumber.Text = "#" + _ctrl.Model.FolderNumber.ToString();
        textTags.Text = _ctrl.Model.Tags;
        textOrder.Text = _ctrl.Model.Order.ToString();
        textOrderNotes.Text = _ctrl.Model.OrderNotes;
        textParentFolder.Text = (_ctrl.Model.ParentFolderDto?.Name == null) ? "(root)" : _ctrl.Model.ParentFolderDto?.Name;
        _selectedParentFolderId = _ctrl.Model.ParentId;
        _selectedParentFolder = _ctrl.Model.ParentFolderDto;            
    }

    private void ControlsToModel()
    {
        _ctrl.Model.Name = textName.Text;
        _ctrl.Model.Tags = textTags.Text;            
        int o;
        if (int.TryParse(textOrder.Text, out o))
            _ctrl.Model.Order = o;

        _ctrl.Model.OrderNotes = textOrderNotes.Text;
        _ctrl.Model.ParentId = _selectedParentFolderId;
        _ctrl.Model.ParentFolderDto = _selectedParentFolder;            
    }

    #endregion
}
