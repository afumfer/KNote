using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

public partial class ResourceEditorForm : Form, IViewEditor<ResourceDto>
{
    #region Private fields

    private readonly ResourceEditorCtrl _ctrl;
    private bool _viewFinalized = false;
    private bool _formIsDisty = false;
    
    private string varFileType;        
    private byte[] varContentArrayBytes;
    private string varName;
    private string varContainer;

    #endregion

    #region Constructor

    public ResourceEditorForm(ResourceEditorCtrl ctrl)
    {
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        _ctrl = ctrl;
    }

    #endregion

    #region IEditorView implementation

    public void ShowView()
    {
        this.Show();
    }

    public Result<EComponentResult> ShowModalView()
    {
        var res = _ctrl.DialogResultToComponentResult(this.ShowDialog());
        return res;
    }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        return MessageBox.Show(info, caption, buttons, icon);
    }

    public async void RefreshView()
    {
        await ModelToControls();
    }

    public void RefreshModel()
    {
        ControlsToModel();
    }

    public async void CleanView()
    {
        textDescription.Text = "";
        textOrder.Text = "";            
        await htmlPreview.NavigateToString(" ");
        textFileName.Text = "";
    }

    public void OnClosingView()
    {
        _viewFinalized = true;
        this.Close();
    }

    #endregion

    #region Form event handlers

    private void ResourceEditorForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
        {
            var confirmExit = OnCandelEdition();
            if (!confirmExit)
                e.Cancel = true;
        }
    }

    private void ResourceEditorForm_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            _formIsDisty = true;
    }

    private void ResourceEditorForm_KeyPress(object sender, KeyPressEventArgs e)
    {
        _formIsDisty = true;
    }

    private async void buttonAccept_Click(object sender, EventArgs e)
    {            
        var res = await _ctrl.SaveModel();
        if (res)
        {                
            await htmlPreview.NavigateToString(" ");
            htmlPreview.Refresh();
            _formIsDisty = false;
            this.DialogResult = DialogResult.OK;
        }
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
        OnCandelEdition();
    }

    private async void buttonSelectFile_Click(object sender, EventArgs e)
    {
        openFileDialog.Title = "Select file for KeyNote resource";
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            var fileTmp = openFileDialog.FileName;
            varContentArrayBytes = File.ReadAllBytes(fileTmp);                
            textFileName.Text = Path.GetFileName(fileTmp);
            textDescription.Text = textFileName.Text;
            varName = _ctrl.Model.ResourceId.ToString() + "_" + textFileName.Text;                
            varFileType = _ctrl.ExtensionFileToFileType(Path.GetExtension(fileTmp));
            await ShowPreview(fileTmp);
        }
    }

    #endregion

    #region Private methods

    private bool OnCandelEdition()
    {
        if (_formIsDisty)
        {
            if (MessageBox.Show("You have modified this entity, are you sure you want to exit without recording?", "KNote", MessageBoxButtons.YesNo) == DialogResult.No)
                return false;
        }

        this.DialogResult = DialogResult.Cancel;
        _ctrl.CancelEdition();
        return true;
    }

    private async Task ModelToControls()
    {            
        textFileName.Text = _ctrl.Model.NameOut;
        varName = _ctrl.Model.Name;
        textDescription.Text = _ctrl.Model.Description;
        textOrder.Text = _ctrl.Model.Order.ToString();
        varFileType = _ctrl.Model.FileType;            
        varContainer = _ctrl.Model.Container;                  

        if (_ctrl.Model.ContentInDB)             
            varContentArrayBytes = _ctrl.Model.ContentArrayBytes;            
        else
        {
            var file = _ctrl.Service.Notes.UtilGetResourceFilePath(_ctrl.Model);
            if(!string.IsNullOrEmpty(file))
                if(File.Exists(file))
                    varContentArrayBytes = File.ReadAllBytes(file);
        } 
        
        await ShowPreview(_ctrl.Model.FullUrl, false);
    }

    private void ControlsToModel()
    {
        _ctrl.Model.Name = varName;
        _ctrl.Model.Description = textDescription.Text;
        _ctrl.Model.Order = _ctrl.Store.TextToInt(textOrder.Text);
        _ctrl.Model.FileType = varFileType;
        _ctrl.Model.Container = varContainer;            
        _ctrl.Model.ContentArrayBytes = varContentArrayBytes;

        _ctrl.SaveResourceFileAndRefreshDto();            
    }

    private async Task ShowPreview(string file, bool includePdf = true) 
    {
        if (file == null)
        {                
            await htmlPreview.NavigateToString(" ");
            return;
        }
                        
        var ext = Path.GetExtension(file);          
        if(_ctrl.Store.ExtensionFileToFileType(ext) != "")
        {
            try
            {
                await htmlPreview.NavigateToString(" ");
                await htmlPreview.Navigate(file);
            }
            catch (Exception ex)
            {
                ShowInfo(ex.Message, KntConst.AppName);
            }
        }
        else
            await htmlPreview.NavigateToString(" ");
    }

    #endregion

}
