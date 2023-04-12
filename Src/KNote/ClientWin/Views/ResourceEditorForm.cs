using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

public partial class ResourceEditorForm : Form, IViewEditor<ResourceDto>
{
    #region Private fields

    private readonly ResourceEditorComponent _com;
    private bool _viewFinalized = false;
    private bool _formIsDisty = false;
    
    private string varFileType;        
    private byte[] varContentArrayBytes;
    private string varName;
    private string varContainer;

    #endregion

    #region Constructor

    public ResourceEditorForm(ResourceEditorComponent com)
    {
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        _com = com;
    }

    #endregion

    #region IEditorView implementation

    public void ShowView()
    {
        this.Show();
    }

    public Result<EComponentResult> ShowModalView()
    {
        var res = _com.DialogResultToComponentResult(this.ShowDialog());
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
        var res = await _com.SaveModel();
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
            varName = _com.Model.ResourceId.ToString() + "_" + textFileName.Text;                
            varFileType = _com.ExtensionFileToFileType(Path.GetExtension(fileTmp));
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
        _com.CancelEdition();
        return true;
    }

    private async Task ModelToControls()
    {            
        textFileName.Text = _com.Model.NameOut;
        varName = _com.Model.Name;
        textDescription.Text = _com.Model.Description;
        textOrder.Text = _com.Model.Order.ToString();
        varFileType = _com.Model.FileType;            
        varContainer = _com.Model.Container;                  

        if (_com.Model.ContentInDB)             
            varContentArrayBytes = _com.Model.ContentArrayBytes;            
        else
        {
            var file = _com.Service.Notes.UtilGetResourcePath(_com.Model);
            if(!string.IsNullOrEmpty(file))
                if(File.Exists(file))
                    varContentArrayBytes = File.ReadAllBytes(file);
        } 
        
        await ShowPreview(_com.Model.FullUrl, false);
    }

    private void ControlsToModel()
    {
        _com.Model.Name = varName;
        _com.Model.Description = textDescription.Text;
        _com.Model.Order = _com.TextToInt(textOrder.Text);
        _com.Model.FileType = varFileType;
        _com.Model.Container = varContainer;            
        _com.Model.ContentArrayBytes = varContentArrayBytes;

        _com.SaveResourceFileAndRefreshDto();            
    }

    private async Task ShowPreview(string file, bool includePdf = true)  // !!!
    {
        if (file == null)
        {                
            await htmlPreview.NavigateToString(" ");
            return;
        }
                        
        var ext = Path.GetExtension(file);          
        if(_com.Store.ExtensionFileToFileType(ext) != "")
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
