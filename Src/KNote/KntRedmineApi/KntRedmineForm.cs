using KNote.Model;
using KNote.Model.Dto;
using KNote.Service;
using Pandoc;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace KntRedmineApi;

public partial class KntRedmineForm : Form
{
    #region Fields

    private IPluginCommand? _pluginCommand;
    private IKntService? _service;
    private KntRedmineManager? _manager;

    #endregion

    #region Constructors

    public KntRedmineForm()
    {
        InitializeComponent();
    }

    public KntRedmineForm(IPluginCommand? pluginCommand) : this()
    {
        _pluginCommand = pluginCommand;
        _service = pluginCommand?.Service;
    }

    #endregion

    #region Events handlers methods

    private async void KntRedmineForm_Load(object sender, EventArgs e)
    {
        if(_pluginCommand == null || _service == null)
        {
            MessageBox.Show("Configuration context is invalid.");
            this.Close();
            return;
        }

        var isValidService = await IsValidService();
        if (!isValidService)
        {
            MessageBox.Show("This database not support KaNote Redmine utils.");
            this.Close();
            return;
        }

        // RedMineAPI variables
        var host = await GetSystemPlugInVariable("HOST"); 
        var apiKey = await GetSystemPlugInVariable("APIKEY");
        var importFile = await GetSystemPlugInVariable("IMPORTFILE");

        textHost.Text = host;
        textApiKey.Text = apiKey;
        textIssuesImportFile.Text = importFile;

        _manager = new KntRedmineManager(host, apiKey);

        try
        {
            if(!string.IsNullOrEmpty(textIssuesImportFile.Text))
                textIssuesId.Text = File.ReadAllText(textIssuesImportFile.Text);
        }
        catch { }
    }

    private void KntRedmineForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (_pluginCommand == null)
            return;

        try
        {
            var configFile = Path.Combine(Application.StartupPath, "KntRedmine.config");

            var kntRedmineConfig = new KntRedmineConfig
            {
                AppUserName = _pluginCommand.AppUserName,
                ToolsPath = _pluginCommand.ToolsPath,
                RepositoryRef = _pluginCommand.Service.RepositoryRef
            };

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

            if (_service == null || _manager == null || _pluginCommand == null)
            {
                MessageBox.Show("There is no correct context selected.", "KaNote");
                return;
            }

            Guid? userId = null;
            var userDto = (await _service.Users.GetByUserNameAsync(_pluginCommand.AppUserName)).Entity;
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

            //var pandocEngine = new PandocEngine($"{_store.AppConfig.ToolsPath}/pandoc.exe");        
            PandocInstance.SetPandocPath($"{_pluginCommand.ToolsPath}/pandoc.exe");

            var filter = new NotesFilterDto();

            int rootFolNum = 1;
            if (!string.IsNullOrEmpty(textFolderNumForImportIssues.Text))
                int.TryParse(textFolderNumForImportIssues.Text, out rootFolNum);
            var parentFolder = (await _service.Folders.GetAsync(rootFolNum)).Entity;
            var folders = (await _service.Folders.GetAllAsync()).Entity;

            var hhuu = GetHUs(textIssuesId.Text);
            var i = 1;
            listInfoRedmine.Items.Clear();

            foreach (var hu in hhuu)
            {
                string folderName = "";
                NoteExtendedDto note = (await _service.Notes.NewExtendedAsync(new NoteInfoDto { NoteTypeId = Guid.Parse("4A3E0AE2-005D-44F0-8BF0-7E0D2A60F6C7") })).Entity;

                filter.Tags = $"HU#{hu}";

                var notes = (await _service.Notes.GetFilter(filter)).Entity;

                if (notes != null)
                {
                    if (notes.Count > 0)
                        note = (await _service.Notes.GetExtendedAsync(notes[0].NoteId)).Entity;

                }

                var res = _manager.IssueToNoteDto(hu, note);

                if (res)
                {
                    note.Tags = filter.Tags;
                    folderName = note.KAttributesDto[2].Value;

                    foreach (var r in note.Resources)
                    {
                        r.FileType = ExtensionFileToFileType(Path.GetExtension(r.Name));
                    }

                    var folder = folders.FirstOrDefault(f => f.Name == folderName);

                    if (folder != null)
                    {
                        note.FolderId = folder.FolderId;
                    }
                    else
                    {
                        FolderDto newFolder = new FolderDto
                        {
                            Name = folderName,
                            ParentId = parentFolder.FolderId
                        };

                        var resSave = await _service.Folders.SaveAsync(newFolder);

                        if (resSave.IsValid)
                        {
                            folders = (await _service.Folders.GetAllAsync()).Entity;
                            note.FolderId = resSave.Entity.FolderId;
                        }
                        else
                            note.FolderId = parentFolder.FolderId;
                    }

                    foreach (var r in note.Resources)
                    {
                        if (string.IsNullOrEmpty(r.Container))
                        {
                            r.Container = $"{_service.RepositoryRef.ResourcesContainer}/{DateTime.Now.Year.ToString()}";
                        }

                        r.Container = r.Container.Replace('\\', '/');

                        var org = $"!{r.NameOut}!";
                        var dest = $"![alt text]({r.Container}/{r.Name})";

                        note.Description = note.Description.Replace(org, dest, true, CultureInfo.CurrentCulture);
                    }

                    // Inefficient version
                    //note.Description = TextToMarkdown(_store.AppConfig.ToolsPath, note.Description);

                    // Other version
                    note.Description = await TextToMarkdown2(_pluginCommand.ToolsPath, note.Description);

                    // This version, pending encoding issue ...
                    //note.Description = await pandocEngine.ConvertToText<TextileIn, CommonMarkOut>(note.Description);                
                    //note.Description = note.Description.Replace("\\[", "[");
                    //note.Description = note.Description.Replace("\\]", "]");

                    var resSaveNote = await _service.Notes.SaveExtendedAsync(note);
                    listInfoRedmine.Items.Add($"{i++} - #{note.Tags}: {note.Topic}");
                    listInfoRedmine.Refresh();
                }
                else
                {
                    listInfoRedmine.Items.Add($"Error");
                    listInfoRedmine.Refresh();
                }
            }

            // Save variables
            await SaveSystemPlugInVariable("HOST", textHost.Text);
            await SaveSystemPlugInVariable("APIKEY", textApiKey.Text);
            await SaveSystemPlugInVariable("IMPORTFILE", textIssuesImportFile.Text);

            UseWaitCursor = false;
            MessageBox.Show("End import", "KaNote");

        }
        catch (Exception ex)
        {
            UseWaitCursor = false;
            MessageBox.Show($"The following error has occurred: {ex.Message}", "KaNote");                
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
            MessageBox.Show($"The following error has occurred: {ex.Message}", "KaNote");
        }            
    }

    private async void buttonFindIssue_Click(object sender, EventArgs e)
    {
        try
        {
            UseWaitCursor = true;
            if (_service == null || _manager == null)
            {
                MessageBox.Show("There is no archive selected ");
                return;
            }

            textPredictSubject.Text = "";
            textPredictDescription.Text = "";
            textPredictCategory.Text = "";
            textPredictionGestion.Text = "";
            textPredictionPH.Text = "";

            NoteExtendedDto note = (await _service.Notes.NewExtendedAsync(new NoteInfoDto { NoteTypeId = Guid.Parse("4A3E0AE2-005D-44F0-8BF0-7E0D2A60F6C7") })).Entity;

            var res = _manager.IssueToNoteDto(textPredictFindIssue.Text, note, false);

            textPredictSubject.Text = note.Topic;
            textPredictDescription.Text = note.Description;
            textPredictCategory.Text = note.KAttributesDto[2].Value;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"The following error has occurred: {ex.Message}", "KaNote");
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
            MessageBox.Show($"The following error has occurred: {ex.Message}", "KaNote");
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
            MessageBox.Show($"The following error has occurred: {ex.Message}", "KaNote");
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    #endregion

    #region Private methods

    private string[] GetHUs(string strIssuesId)
    {
        return strIssuesId.Split("\r\n");
    }

    private string TextToMarkdown(string pathUtils, string text)
    {
        // TODO: refactor this method

        var textOut = "";

        if (!Directory.Exists(pathUtils))
            return text;

        string fileIn = Path.Combine(pathUtils, "input.text");
        string fileOut = Path.Combine(pathUtils, "output.md");
        string exPandoc = Path.Combine(pathUtils, "pandoc.exe");
        string param = $" -f textile -t markdown --wrap=preserve {fileIn} -o {fileOut}";

        if (System.IO.File.Exists(fileIn))
            System.IO.File.Delete(fileIn);

        if (System.IO.File.Exists(fileOut))
            System.IO.File.Delete(fileOut);

        System.IO.File.WriteAllText(fileIn, text);

        var process = Process.Start(exPandoc, param);
        process.WaitForExit();
        var exitCode = process.ExitCode;

        if (System.IO.File.Exists(fileOut))
            textOut = System.IO.File.ReadAllText(fileOut);

        textOut = textOut.Replace("\\[", "[");
        textOut = textOut.Replace("\\]", "]");
        return textOut;

    }

    private async Task<string> TextToMarkdown2(string pathUtils, string text)
    {
        // TODO: refactor this method
        var textOut = "";

        if (!Directory.Exists(pathUtils))
            return text;

        string fileIn = Path.Combine(pathUtils, "__input.text");
        string fileOut = Path.Combine(pathUtils, "__output.md");

        if (System.IO.File.Exists(fileIn))
            System.IO.File.Delete(fileIn);

        if (System.IO.File.Exists(fileOut))
            System.IO.File.Delete(fileOut);

        System.IO.File.WriteAllText(fileIn, text);

        await PandocInstance.Convert<TextileIn, CommonMarkOut>(fileIn, fileOut, new TextileIn { }, new CommonMarkOut { Wrap = Wrap.Preserve }, default);

        if (System.IO.File.Exists(fileOut))
            textOut = System.IO.File.ReadAllText(fileOut);

        textOut = textOut.Replace("\\[", "[");
        textOut = textOut.Replace("\\]", "]");
        return textOut;
    }

    public string ExtensionFileToFileType(string extension)
    {
        // TODO: Refactor this method

        var ext = extension.ToLower();

        if (ext == ".jpg")
            return @"image/jpeg";
        else if (ext == ".jpeg")
            return @"image/jpeg";
        else if (ext == ".png")
            return "image/png";
        else if (ext == ".pdf")
            return "application/pdf";
        else if (ext == ".mp4")
            return "video/mp4";
        else if (ext == ".mp3")
            return "audio/mp3";
        else if (ext == ".txt")
            return "text/plain";
        else if (ext == ".text")
            return "text/plain";
        else if (ext == ".htm")
            return "text/plain";
        else if (ext == ".html")
            return "text/plain";
        else
            return "";
    }

    private async Task<bool> IsValidService()
    {
        var value = await GetSystemPlugInVariable("PLUGIN_SUPPORT");

        if (value.ToLower() == "true")
            return true;
        else
            return false;
    }

    private async Task<string> GetSystemPlugInVariable(string variable)
    {            
        if (_service == null)
            return "";

        var valueDto = await _service.SystemValues.GetAsync("KNT_REDMINEEDUCA_PLUGIN", variable);
        if (valueDto.IsValid)
            return valueDto.Entity.Value;
        else
            return "";
    }

    private async Task<string> SaveSystemPlugInVariable(string variable, string value)
    {
        // TODO: refactor this method
        if (_service == null)
            return "";

        var valueDto = await _service.SystemValues.GetAsync("KNT_REDMINEEDUCA_PLUGIN", variable);
        if (valueDto.IsValid)
        {
            valueDto.Entity.Value = value;
            var resSave = await _service.SystemValues.SaveAsync(valueDto.Entity);
            if(resSave.IsValid)
                return resSave.Entity.Value;
            else
                return "";
        }
        else
            return "";
    }

    #endregion

}
