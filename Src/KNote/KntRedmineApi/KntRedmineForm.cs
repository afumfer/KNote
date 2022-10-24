using KNote.Model;
using KNote.Model.Dto;
using KNote.Service;
using Pandoc;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KntRedmineApi
{
    public partial class KntRedmineForm : Form
    {
        private IPluginCommand? _pluginCommand;
        private IKntService? _service;
        private KntRedmineManager? _manager;

        public KntRedmineForm()
        {
            InitializeComponent();
        }

        public KntRedmineForm(IPluginCommand? pluginCommand) : this()
        {
            _pluginCommand = pluginCommand;
            _service = pluginCommand?.Service;
        }

        private async void KntRedmineForm_Load(object sender, EventArgs e)
        {
            var isValidService = await IsValidService();
            if (!isValidService)
            {
                MessageBox.Show("This database not support KaNote Redmine utils.");
                this.Close();
                return;
            }

            // RedMineAPI
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

        private async Task<bool> IsValidService()
        {
            var supportEducaRedmine = await GetSystemPlugInVariable("PLUGIN_SUPPORT");
                        
            if (supportEducaRedmine.ToLower() == "true")
                return true;
            else
                return false;                      
        }

        private async Task<string> GetSystemPlugInVariable(string variable)
        {
            // TODO: refactor this method
            if (_service == null)
                return "";

            var supportEducaRedmine = await _service.SystemValues.GetAsync("KNT_REDMINEEDUCA_PLUGIN", variable);
            if (supportEducaRedmine.IsValid)            
                return supportEducaRedmine.Entity.Value;                                
            else            
                return "";                        
        }

        private async void buttonImportRedmineIssues_Click(object sender, EventArgs e)
        {
            if (_service == null || _manager == null || _pluginCommand == null)
            {
                MessageBox.Show("There is no correct context selected.");
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

            //// TODO: Refactor this ....
            //_store.AppConfig.HostRedmine = textHost.Text;
            //_store.AppConfig.ApiKeyRedmine = textApiKey.Text;
            //_store.AppConfig.IssuesImportFile = textIssuesImportFile.Text;
            //_store.AppConfig.ToolsPath = Path.GetDirectoryName(_store.AppConfig.IssuesImportFile);
            
            //var pandocEngine = new PandocEngine($"{_store.AppConfig.ToolsPath}/pandoc.exe");        
            PandocInstance.SetPandocPath($"{_pluginCommand.ToolsPath}/pandoc.exe");

            //// ----

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

                    // iIefficient version
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

            MessageBox.Show("End import");

        }

        private string[] GetHUs(string strIssuesId)
        {
            return strIssuesId.Split("\r\n");
        }

        private string TextToMarkdown(string pathUtils, string text)
        {
            // TODO: refactor this method

            // pandoc -f textile -t markdown --wrap=preserve prueba1.text -o pruebaS1.md

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

        private void buttonIssuesImportFile_Click(object sender, EventArgs e)
        {
            openFileDialog.Title = "Select file Issues ID file";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var fileTmp = openFileDialog.FileName;
                textIssuesImportFile.Text = fileTmp;
                textIssuesId.Text = File.ReadAllText(fileTmp);
                // !!!
                //_store.AppConfig.IssuesImportFile = textIssuesImportFile.Text;
            }
        }

        private async void buttonFindIssue_Click(object sender, EventArgs e)
        {
            if (_service == null || _manager == null)
            {
                MessageBox.Show("There is no archive selected ");
                return;
            }

            NoteExtendedDto note = (await _service.Notes.NewExtendedAsync(new NoteInfoDto { NoteTypeId = Guid.Parse("4A3E0AE2-005D-44F0-8BF0-7E0D2A60F6C7") })).Entity;
            //var manager = new KntRedmineManager(_store.AppConfig.HostRedmine, _store.AppConfig.ApiKeyRedmine);

            textPredictSubject.Text = "";
            textPredictDescription.Text = "";
            textPredictCategory.Text = "";
            textPredictionGestion.Text = "";

            textPredictionPH.Text = "";

            var res = _manager.IssueToNoteDto(textPredictFindIssue.Text, note, false);

            textPredictSubject.Text = note.Topic;
            textPredictDescription.Text = note.Description;

            if (note.KAttributesDto.Count < 3)
                MessageBox.Show("You do not have the experimental database selected with the RedMine Educa import.");
            else
                textPredictCategory.Text = note.KAttributesDto[2].Value;
        }

        private void buttonPredictGestion_Click(object sender, EventArgs e)
        {
            textPredictionGestion.Text = "";
            //var manager = new KntRedmineManager(_store.AppConfig.HostRedmine, _store.AppConfig.ApiKeyRedmine);
            textPredictionGestion.Text = _manager?.PredictGestion(textPredictSubject.Text, textPredictDescription.Text);
        }

        private void buttonPredictPH_Click(object sender, EventArgs e)
        {
            textPredictionPH.Text = "";
            //var manager = new KntRedmineManager(_store.AppConfig.HostRedmine, _store.AppConfig.ApiKeyRedmine);
            textPredictionPH.Text = _manager?.PredictPH(textPredictCategory.Text, textPredictSubject.Text, textPredictDescription.Text);
        }
    }
}
