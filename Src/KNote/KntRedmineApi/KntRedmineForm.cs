using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using System.Xml.Serialization;

namespace KntRedmineApi;

public partial class KntRedmineForm : Form
{
    #region Fields

    private IPluginCommand _pluginCommand;
    private KntRedmineManager _manager;

    #endregion

    #region Constructors

    public KntRedmineForm()
    {
        InitializeComponent();
    }

    public KntRedmineForm(IPluginCommand pluginCommand) : this()
    {
        if (pluginCommand == null)
            throw new Exception("Invalid KntRedmine plugin command.");

        _pluginCommand = pluginCommand;

        _manager = new KntRedmineManager(pluginCommand.Service);
    }

    #endregion

    #region Events handlers methods

    private async void KntRedmineForm_Load(object sender, EventArgs e)
    {
        if (_pluginCommand == null || _manager == null)
        {
            MessageBox.Show("Configuration context is invalid.");
            this.Close();
            return;
        }

        var isValidService = await _manager.IsValidService();
        if (!isValidService)
        {
            MessageBox.Show($"This database not support {KntConst.AppName} Redmine utils.");
            this.Close();
            return;
        }

        await _manager.InitManager();

        textHost.Text = _manager.Host;
        textApiKey.Text = _manager.ApiKey;
        textToolsPath.Text = _manager.ToolsPath;
        textIssuesImportFile.Text = _manager.ImportFile;
        textFolderNumForImportIssues.Text = _manager.RootFolderForImport?.ToString();
        textKNoteImportUser.Text = _manager.AppUserName;


        try
        {
            if (!string.IsNullOrEmpty(textIssuesImportFile.Text))
                textIssuesId.Text = File.ReadAllText(textIssuesImportFile.Text);
        }
        catch { }
    }

    private void KntRedmineForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (_pluginCommand == null || _manager == null)
            return;

        try
        {
            var configFile = Path.Combine(Application.StartupPath, "KntRedmine.config");

            var kntRedmineConfig = new KntRedmineConfig(_pluginCommand.Service.RepositoryRef);

            TextWriter w = new StreamWriter(configFile);
            XmlSerializer serializer = new XmlSerializer(typeof(KntRedmineConfig));
            serializer.Serialize(w, kntRedmineConfig);
            w.Close();
        }
        catch (Exception)
        {

        }
    }

    private async void buttonImportRedmineIssues_Click(object sender, EventArgs e)
    {
        try
        {
            UseWaitCursor = true;

            if (_manager == null || _pluginCommand == null)
            {
                MessageBox.Show("There is no correct context selected.", KntConst.AppName);
                return;
            }

            Guid? userId = null;
            var userDto = (await _manager.Service.Users.GetByUserNameAsync(_manager.AppUserName)).Entity;
            if (userDto != null)
                userId = userDto.UserId;

            if (userId == null)
            {
                MessageBox.Show("There is no valid user to import data ");
                return;
            }

            if (string.IsNullOrEmpty(textIssuesId.Text))
            {
                MessageBox.Show("Issues ID not selected.");
                return;
            }

            var filter = new NotesFilterDto();

            var hhuu = GetHUs(textIssuesId.Text);
            var i = 1;
            listInfoRedmine.Items.Clear();

            foreach (var hu in hhuu)
            {
                NoteExtendedDto note = (await _manager.Service.Notes.NewExtendedAsync(new NoteInfoDto { NoteTypeId = Guid.Parse("4A3E0AE2-005D-44F0-8BF0-7E0D2A60F6C7") })).Entity;

                filter.Tags = $"HU#{hu}";

                var notes = (await _manager.Service.Notes.GetFilter(filter)).Entity;

                if (notes != null)
                {
                    if (notes.Count > 0)
                        note = (await _manager.Service.Notes.GetExtendedAsync(notes[0].NoteId)).Entity;
                }

                var res = await _manager.IssueToNoteDto(hu, note);

                if (res)
                {
                    var resSaveNote = await _manager.Service.Notes.SaveExtendedAsync(note);
                    if(resSaveNote.IsValid)
                        listInfoRedmine.Items.Add($"{i++} - {note.Tags}: {note.Topic}");
                    else 
                        listInfoRedmine.Items.Add($"{i++} - {resSaveNote.ErrorMessage}");

                    listInfoRedmine.Refresh();
                }
                else
                {
                    listInfoRedmine.Items.Add($"{i++} - Error");
                    listInfoRedmine.Refresh();
                }
            }

            await _manager.SaveSystemPlugInVariable("IMPORTFILE", textIssuesImportFile.Text);
            await _manager.SaveSystemPlugInVariable("ROOTFOLDERFORIMPORT", textFolderNumForImportIssues.Text);

            UseWaitCursor = false;
            MessageBox.Show("End import", KntConst.AppName);
        }
        catch (Exception ex)
        {
            UseWaitCursor = false;
            MessageBox.Show($"The following error has occurred: {ex.Message}", KntConst.AppName);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private void buttonIssuesImportFile_Click(object sender, EventArgs e)
    {
        try
        {
            openFileDialog.Title = "Select file Issues ID file";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var fileTmp = openFileDialog.FileName;
                textIssuesImportFile.Text = fileTmp;
                textIssuesId.Text = File.ReadAllText(fileTmp);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"The following error has occurred: {ex.Message}", KntConst.AppName);
        }
    }

    private async void buttonFindIssue_Click(object sender, EventArgs e)
    {
        try
        {
            UseWaitCursor = true;
            if (_manager == null)
            {
                MessageBox.Show("There is no archive selected ");
                return;
            }

            textPredictSubject.Text = "";
            textPredictDescription.Text = "";
            textPredictCategory.Text = "";
            textPredictionGestion.Text = "";
            textPredictionPH.Text = "";

            NoteExtendedDto note = (await _manager.Service.Notes.NewExtendedAsync(new NoteInfoDto { NoteTypeId = Guid.Parse("4A3E0AE2-005D-44F0-8BF0-7E0D2A60F6C7") })).Entity;

            var res = _manager.IssueToNoteDto(textPredictFindIssue.Text, note, false);

            textPredictSubject.Text = note.Topic;
            textPredictDescription.Text = note.Description;
            textPredictCategory.Text = note.KAttributesDto[2].Value;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"The following error has occurred: {ex.Message}", KntConst.AppName);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private void buttonPredictGestion_Click(object sender, EventArgs e)
    {
        try
        {
            UseWaitCursor = true;
            Application.DoEvents();
            textPredictionGestion.Text = "";
            textPredictionGestion.Text = _manager?.PredictGestion(textPredictSubject.Text, textPredictDescription.Text);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"The following error has occurred: {ex.Message}", KntConst.AppName);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private void buttonPredictPH_Click(object sender, EventArgs e)
    {
        try
        {
            UseWaitCursor = true;
            Application.DoEvents();
            textPredictionPH.Text = "";
            textPredictionPH.Text = _manager?.PredictPH(textPredictCategory.Text, textPredictSubject.Text, textPredictDescription.Text);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"The following error has occurred: {ex.Message}", KntConst.AppName);
        }
        finally
        {
            UseWaitCursor = false;
        }
    }
    private async void buttonSaveParameters_Click(object sender, EventArgs e)
    {
        if (_manager == null)
        {
            MessageBox.Show("Manager is null, cann't save values.");
            return;
        }
        await _manager.SaveSystemPlugInVariable("HOST", textHost.Text);
        await _manager.SaveSystemPlugInVariable("APIKEY", textApiKey.Text);
        await _manager.SaveSystemPlugInVariable("TOOLSPATH", textToolsPath.Text);
        await _manager.SaveSystemPlugInVariable("APPUSERNAME", textKNoteImportUser.Text);
        await _manager.SaveSystemPlugInVariable("IMPORTFILE", textIssuesImportFile.Text);
        await _manager.SaveSystemPlugInVariable("ROOTFOLDERFORIMPORT", textFolderNumForImportIssues.Text);
    }

    #endregion

    #region Private methods

    private string[] GetHUs(string strIssuesId)
    {
        return strIssuesId.Split("\r\n");
    }

    #endregion

}
