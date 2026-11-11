using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

public partial class ResourceEditorForm : KntEditorForm, IViewEditor<ResourceDto>
{
    #region Private fields

    private readonly ResourceEditorCtrl _ctrl;

    private string varFileType;
    private byte[] varContentArrayBytes;
    private string varName;
    private string varContainer;

    // The lock (ResourceDescriptionRules, D12): checked by itself while a person writes a text of their own, until the
    // person touches the checkbox; what is saved is what it says.
    private bool _loadedLock;
    private string _loadedModel;
    private bool _lockTouched;
    private bool _settingControls;

    #endregion

    #region Constructor

    public ResourceEditorForm(ResourceEditorCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;
    }

    #endregion

    #region IEditorView implementation

    public override async void RefreshView()
    {
        await ModelToControlsAsync();
    }

    #endregion

    #region Form event handlers

    private void ResourceEditorForm_Load(object sender, EventArgs e)
    {
        PersonalizeControls();
    }

    private async void buttonAccept_Click(object sender, EventArgs e)
    {
        var res = await _ctrl.SaveModel();
        if (res)
        {
            await kntView.ClearWebView();
            kntView.Refresh();
            FormIsDirty = false;
            this.DialogResult = DialogResult.OK;
        }
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
        TryCancelEdition();
    }

    protected override void CancelEdition()
        => _ctrl.CancelEdition();

    private async void buttonSelectFile_Click(object sender, EventArgs e)
    {
        openFileDialog.Title = "Select file for KeyNote resource";
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            var fileTmp = openFileDialog.FileName;
            // KNote Atlas, stage 4 (D6): the same limit as the Web.
            if (ResourceFileRules.SizeError(Path.GetFileName(fileTmp), new FileInfo(fileTmp).Length) is string sizeError)
            {
                ShowInfo(sizeError, KntConst.AppName);
                return;
            }
            varContentArrayBytes = File.ReadAllBytes(fileTmp);
            textFileName.Text = _ctrl.Service.ReplaceSpecialCharacters(Path.GetFileName(fileTmp));
            // Its name first: the lock (textDescription_TextChanged) compares the description with it, and the file's name
            // is not a text of a person (with the name before, a new resource was locked by itself).
            varName = _ctrl.Model.ResourceId.ToString() + "_" + textFileName.Text;
            varFileType = _ctrl.ExtensionFileToFileType(Path.GetExtension(fileTmp));
            // A locked description stays: the person wrote it (or decided to keep it).
            if (!checkLockDescription.Checked)
                textDescription.Text = textFileName.Text;
            await ShowPreview(fileTmp);
        }
    }

    private void textDescription_TextChanged(object sender, EventArgs e)
    {
        if (_settingControls)
            return;

        if (!_lockTouched)
        {
            _settingControls = true;
            checkLockDescription.Checked = ResourceDescriptionRules.LockWhileEditing(
                _ctrl.Model.LoadedDescription, _loadedLock, textDescription.Text, varName);
            _settingControls = false;
        }
        ShowAuthor();
    }

    private void checkLockDescription_CheckedChanged(object sender, EventArgs e)
    {
        if (_settingControls)
            return;

        _lockTouched = true;
        ShowAuthor();
    }

    #endregion

    #region Private methods

    private void PersonalizeControls()
    {
        kntView.ShowStatusInfo = false;
        kntView.ShowNavigationTools = false;
    }

    // Who wrote the description, as it would be saved now; a person's text that is not locked, as a warning.
    private void ShowAuthor()
    {
        var model = ResourceDescriptionRules.ModelWhileEditing(_ctrl.Model.LoadedDescription, _loadedModel, textDescription.Text);
        var locked = checkLockDescription.Checked;
        labelAuthor.Text = ResourceDescriptionRules.Author(textDescription.Text, locked, model, varName);
        labelAuthor.ForeColor = ResourceDescriptionRules.IsAtRisk(textDescription.Text, locked, model, varName)
            ? Color.DarkOrange : SystemColors.GrayText;
    }

    private async Task ModelToControlsAsync()
    {
        // The description this editor starts from: the first time it is opened (the note may be edited again before saving).
        _ctrl.Model.LoadedDescription ??= _ctrl.Model.Description ?? "";
        _loadedLock = _ctrl.Model.DescriptionByUser;
        _loadedModel = _ctrl.Model.DescriptionModel;
        _lockTouched = false;

        _settingControls = true;
        textFileName.Text = _ctrl.Model.NameOut;
        varName = _ctrl.Model.Name;
        textDescription.Text = _ctrl.Model.Description;
        checkLockDescription.Checked = _ctrl.Model.DescriptionByUser;
        _settingControls = false;
        ShowAuthor();
        textOrder.Text = _ctrl.Model.Order.ToString();
        varFileType = _ctrl.Model.FileType;
        varContainer = _ctrl.Model.Container;

        if (_ctrl.Model.ContentInDB)
            varContentArrayBytes = _ctrl.Model.ContentArrayBytes;
        else
        {
            var file = _ctrl.Service.Notes.UtilGetResourceFilePath(_ctrl.Model);
            if (!string.IsNullOrEmpty(file))
                if (File.Exists(file))
                    varContentArrayBytes = File.ReadAllBytes(file);
        }

        await ShowPreview(_ctrl.Model.FullUrl, false);
    }

    protected override void ControlsToModel()
    {
        _ctrl.Model.Name = varName;
        _ctrl.Model.Description = textDescription.Text;
        _ctrl.Model.DescriptionByUser = checkLockDescription.Checked;
        // The model that will keep the description when it is saved (the Service decides the same): with a text of a person,
        // none. Otherwise, until the note is saved, «Describe with AI» took it as done by the AI.
        _ctrl.Model.DescriptionModel = ResourceDescriptionRules.ModelWhileEditing(_ctrl.Model.LoadedDescription, _loadedModel, textDescription.Text);
        _ctrl.Model.Order = _ctrl.Store.KntTextUtils.TextToInt(textOrder.Text);
        _ctrl.Model.FileType = varFileType;
        _ctrl.Model.Container = varContainer;
        _ctrl.Model.ContentArrayBytes = varContentArrayBytes;

        _ctrl.SaveResourceFileAndRefreshDto();
    }

    private async Task ShowPreview(string file, bool includePdf = true)
    {
        if (file == null)
        {
            await kntView.ClearWebView();
            return;
        }

        var ext = Path.GetExtension(file);
        if (_ctrl.Store.KntTextUtils.ExtensionFileToFileType(ext) != "")
        {
            try
            {
                await kntView.ClearWebView();
                //await htmlPreview.Navigate(file);
                await kntView.ShowNavigationUrlContent(file);
            }
            catch (Exception ex)
            {
                ShowInfo(ex.Message, KntConst.AppName);
            }
        }
        else
            await kntView.ClearWebView();
    }

    #endregion

}
