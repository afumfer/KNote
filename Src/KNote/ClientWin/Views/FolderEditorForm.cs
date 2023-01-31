using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

public partial class FolderEditorForm : Form, IEditorView<FolderDto>
{
    #region Private fields

    private readonly FolderEditorComponent _com;
    private Guid? _selectedParentFolderId;
    private FolderDto _selectedParentFolder;
    private bool _viewFinalized = false;
    private bool _formIsDisty = false;
    
    #endregion

    #region Constructor 

    public FolderEditorForm(FolderEditorComponent com)
    {
        InitializeComponent();
        _com = com;
    }

    #endregion

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

    public void ConfigureEmbededMode()
    {

    }

    public void ConfigureWindowMode()
    {

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
        var res = await _com.SaveModel();
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
        var folder = _com.GetFolder();
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
            if (MessageBox.Show("You have modified this entity, are you sure you want to exit without recording?", "KaNote", MessageBoxButtons.YesNo) == DialogResult.No)
                return false;
        }

        this.DialogResult = DialogResult.Cancel;
        _com.CancelEdition();
        return true;
    }

    private void ModelToControls()
    {
        textName.Text = _com.Model.Name;
        textNumber.Text = "#" + _com.Model.FolderNumber.ToString();
        textTags.Text = _com.Model.Tags;
        textOrder.Text = _com.Model.Order.ToString();
        textOrderNotes.Text = _com.Model.OrderNotes;
        textParentFolder.Text = (_com.Model.ParentFolderDto?.Name == null) ? "(root)" : _com.Model.ParentFolderDto?.Name;
        _selectedParentFolderId = _com.Model.ParentId;
        _selectedParentFolder = _com.Model.ParentFolderDto;            
    }

    private void ControlsToModel()
    {
        _com.Model.Name = textName.Text;
        _com.Model.Tags = textTags.Text;            
        int o;
        if (int.TryParse(textOrder.Text, out o))
            _com.Model.Order = o;

        _com.Model.OrderNotes = textOrderNotes.Text;
        _com.Model.ParentId = _selectedParentFolderId;
        _com.Model.ParentFolderDto = _selectedParentFolder;            
    }

    #endregion
}
