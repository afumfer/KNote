using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Net.Mail;
using System.Runtime.InteropServices;
using KNote.ClientWin.Controllers;
using KNote.Model.Dto;
using KNote.Model;
using KntScript;
using KNote.Service.Core;


namespace KNote.ClientWin.Core;

public class KNoteScriptLibrary: Library
{
    private readonly Store _store;
    
    private Dictionary<string, string> dictionaryVars;

    public KNoteScriptLibrary(Store store) 
    {
        _store = store;
    }

    #region Factory methods for KntScript 

    public Store GetStore()
    {
        return _store;
    }

    public FoldersSelectorCtrl GetFoldersSelectorCtrl()
    {
        return new FoldersSelectorCtrl(_store);
    }

    public NotesSelectorCtrl GetNotesSelectorCtrl()
    {
        return new NotesSelectorCtrl(_store);
    }

    public KNoteManagmentCtrl GetKNoteManagmentCtrl()
    {
        return new KNoteManagmentCtrl(_store);
    }

    public MonitorCtrl GetMonitorCtrl()
    {
        return new MonitorCtrl(_store);
    }

    public NoteEditorCtrl GetNewNoteEditorCtrl()
    {
        var noteEditor = new NoteEditorCtrl(_store);        
        Task.Run(() => noteEditor.NewModel(_store.GetActiveOrDefaultService()).Wait());
        return noteEditor;
    }

    public NoteEditorCtrl GetNoteEditorCtrl(int noteNumber)
    {
        var service = _store.GetActiveOrDefaultService();
        NoteExtendedDto resNoteExt = null;
        var resNoteInfo = Task.Run(() => service.Notes.GetAsync(noteNumber)).Result;
        if (resNoteInfo.IsValid)
        {
            resNoteExt = Task.Run(() => service.Notes.GetExtendedAsync(resNoteInfo.Entity.NoteId)).Result.Entity;
        }                
        var noteEditor = new NoteEditorCtrl(_store);

        if(resNoteExt != null)
            noteEditor.LoadModel(service, resNoteExt);
        else
            Task.Run(() => noteEditor.NewModel(_store.GetActiveOrDefaultService()).Wait());
        return noteEditor;
    }


    public KntScriptConsoleCtrl GetKntScriptConsoleCtrl()
    {
        return new KntScriptConsoleCtrl(_store);
    }

    public KntChatGPTCtrl GetKntChatGPTCtrl()
    {
        return new KntChatGPTCtrl(_store);
    }

    public string GetKntChatGPTMessage(string prompt)
    {
        var chatGPT =  new KntChatGPTCtrl(_store);
        chatGPT.Run();
        Task.Run(() => chatGPT.GetCompletionAsync(prompt)).Wait();        
        return chatGPT.ChatTextMessasges.ToString();
    }

    public KntChatCtrl GetKntChatCtrl()
    {
        return new KntChatCtrl(_store);
    }

    public void SendChatMessage(string message)
    {
        var chat = new KntChatCtrl(_store);
        var res = chat.Run();
        if(res.IsValid)
            Task.Run(() => chat.SendMessage(message)).Wait();        
    }

    public KntServerCOMCtrl GetServerCOMCtrl()
    {
        return new KntServerCOMCtrl(_store);
    }

    public MonitorCtrl GetKntMoniotorCtrl()
    {
        return new MonitorCtrl(_store);
    }

    public KntLabCtrl GetKntLabCtrl()
    {
        return new KntLabCtrl(_store);
    }

    public KntHttpClientCtrl GetHttpClientCtrl()
    {
        return new KntHttpClientCtrl(_store);
    }

    public bool CheckHttpRequest(string url, int timeOut)
    {
        try
        {
            var httpClient = new KntHttpClientCtrl(_store);
            httpClient.TimeOut = timeOut;
            httpClient.Run();
            var res = httpClient.Get(url);
            httpClient.Dispose();
            return res;
        }
        catch
        {
            return false;
        }
    }

    public bool CheckHttpRequest(string url)
    {
        return CheckHttpRequest(url, 30000);
    }


    #endregion

    #region Utils methods

    public void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
    {
        DirectoryCopyEx(sourceDirName, destDirName, copySubDirs, false, false);
    }

    public void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool showError)
    {
        DirectoryCopyEx(sourceDirName, destDirName, copySubDirs, false, showError);
    }

    public void DirectoryCopyDiff(string sourceDirName, string destDirName, bool copySubDirs)
    {
        DirectoryCopyEx(sourceDirName, destDirName, copySubDirs, true, false);
    }

    public void DirectoryCopyDiff(string sourceDirName, string destDirName, bool copySubDirs, bool showError)
    {
        DirectoryCopyEx(sourceDirName, destDirName, copySubDirs, true, showError);
    }

    private void DirectoryCopyEx(string sourceDirName, string destDirName, bool copySubDirs, bool diff, bool showError)
    {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);
        DirectoryInfo[] dirs = dir.GetDirectories();

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: "
                + sourceDirName);
        }

        // If the destination directory doesn't exist, create it.
        if (!Directory.Exists(destDirName))
        {
            Directory.CreateDirectory(destDirName);
        }

        // Get the files in the directory and copy them to the new location.
        //      with differential option 
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string temppath = Path.Combine(destDirName, file.Name);

            if (diff == true)
            {
                if (File.Exists(temppath))
                {
                    FileInfo ftmp = new FileInfo(temppath);
                    if (file.Length != ftmp.Length || file.LastWriteTime != ftmp.LastWriteTime || ftmp.IsReadOnly != true)
                        FileCopy(file, temppath, showError);
                }
                else
                    FileCopy(file, temppath, showError);

            }
            else
                FileCopy(file, temppath, showError);

        }

        // If copying subdirectories, copy them and their contents to new location.
        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopyEx(subdir.FullName, temppath, copySubDirs, diff, showError);
            }
        }
    }

    private void FileCopy(FileInfo file, string dest, bool showError)
    {
        try
        {
            file.CopyTo(dest, true);
        }
        catch (Exception ex)
        {
            if (showError == true)
                MessageBox.Show(dest.ToString() + " >> " + ex.Message.ToString());
            else
                throw;
        }
    }

    public bool SendGMailMessage(string fromEmail, string fromName, string fromPwd,
        List<object> toUsers, string subject, string body)
    {
        return SendMailMessage(fromEmail, fromName, fromPwd, toUsers, subject, body, false, 587, "smtp.gmail.com", true);
    }

    public bool SendMailMessage(string fromEmail, string fromName, string fromPwd,
        List<object> toUsers, string subject, string body, bool isBodyHtml,
        int port, string host, bool enbleSsl)
    {
        var msg = new MailMessage();
        var client = new SmtpClient();

        try
        {
            foreach (string s in toUsers)
                msg.To.Add(s.ToString());

            msg.From = new System.Net.Mail.MailAddress(fromEmail, fromName);
            msg.Subject = subject;
            msg.Body = body;
            msg.IsBodyHtml = isBodyHtml;

            client.Credentials = new System.Net.NetworkCredential(fromEmail, fromPwd);
            client.Port = port;
            client.Host = host;
            client.EnableSsl = enbleSsl;

            client.Send(msg);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public DbConnection GetSQLConnection(string connectionString)
    {
        var db =  new SqlConnection(connectionString);
        db.Open();
        return db;                        
    }

    public void SetParameter(SqlCommand cmd, SqlParameter par)
    {
        cmd.Parameters.Add(par);
    }

    public void Exec(string fileName, string arguments, string userName, System.Security.SecureString password, string domain)
    {
        Exec(fileName, arguments, userName, password, domain, false);
    }

    public void Exec(string fileName, string arguments, string userName, System.Security.SecureString password, string domain, bool showError)
    {
        try
        {
            Process.Start(fileName, arguments, userName, password, domain);
        }
        catch (Exception ex)
        {
            if (showError == true)
                MessageBox.Show("The following error has occurred: " + ex.Message, KntConst.AppName);
            else
                throw;
        }
    }

    public void Exec(string fileName, string arguments)
    {
        string url = fileName;

        try
        {
            Process.Start(url, arguments);
        }
        catch
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }

    public void Exec(string fileName)
    {       
        try
        {            
            var startInfo = new ProcessStartInfo(fileName);
            startInfo.WorkingDirectory = Path.GetDirectoryName(fileName);        

            var proc = Process.Start(startInfo);
        }
        catch
        {           
            throw;           
        }
    }


    public async Task<bool> LoadVarsFromRepository(string varIdentifier, string repositoryAlias)
    {
        ServiceRef serviceRef;
        if(string.IsNullOrEmpty(repositoryAlias))
            serviceRef = _store.GetFirstServiceRef();
        else
            serviceRef = _store.GetServiceRef(repositoryAlias);

        if (serviceRef == null || varIdentifier == null)
            throw new Exception("Invalid parameter.");

        NotesSearchDto search = new NotesSearchDto { TextSearch = $"{varIdentifier} " };
        var note = (await serviceRef.Service.Notes.GetSearch(search)).Entity.FirstOrDefault();

        string content = note?.Description;
        if (content != null)
        {
            //tagVars = note.Description;
            dictionaryVars = new Dictionary<string, string>();
            string[] lines = note.Description?.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
            );
            foreach(string s in lines)
            {
                var splitVar = s.Split('=');
                if (splitVar.Length == 2)
                {
                    splitVar[0] = splitVar[0].Trim();
                    splitVar[1] = splitVar[1].Trim();
                    if (dictionaryVars.ContainsKey(splitVar[0]))
                        dictionaryVars[splitVar[0]] = splitVar[1];
                    else 
                        dictionaryVars.Add(splitVar[0], splitVar[1]);
                }
            }
            return true;
        }
        else                            
            return false;
    }

    public string GetVar(string idVar)
    {
        if (dictionaryVars == null)
            return null;
        else
        {
            if (dictionaryVars.ContainsKey(idVar))
                return dictionaryVars[idVar];
            else
                return null;
        }
    }

    public async Task<bool> SaveInfoInRepository(string repositoryAlias, string subjet, string message, int? folderNumber)
    {
        ServiceRef serviceRef = GetServiceRef(repositoryAlias);

        NoteDto note = (await serviceRef.Service.Notes.NewAsync()).Entity;
        note.Topic = subjet;
        note.Description = message;
        if (folderNumber == null)
            note.FolderId = (await serviceRef.Service.Folders.GetHomeAsync()).Entity.FolderId;
        else
        {
            var folder = (await serviceRef.Service.Folders.GetAsync((int)folderNumber)).Entity;
            if(folder != null)
                note.FolderId = folder.FolderId;
            else
                throw new Exception("Invalid parameter.");
        }

        var res = (await serviceRef.Service.Notes.SaveAsync(note)).IsValid;

        return res;
    }

    public async Task<bool> SaveInfoInRepository(string repositoryAlias, string subjet, string message)
    {
        return await SaveInfoInRepository(repositoryAlias, subjet, message, null);
    }

    public FolderDto GetFolderByNum(string repositoryAlias, int folderNumber)
    {
        ServiceRef serviceRef = GetServiceRef(repositoryAlias);

        Task<Result<FolderDto>> task = serviceRef.Service.Folders.GetAsync(folderNumber);
        task.Wait();

        Result<FolderDto> result = task.Result;

        return result.Entity;            
    }

    public NoteDto GetNoteByNum(string repositoryAlias, int noteNumber)
    {
        ServiceRef serviceRef = GetServiceRef(repositoryAlias);

        Task<Result<NoteDto>> task = serviceRef.Service.Notes.GetAsync(noteNumber);
        task.Wait();

        Result<NoteDto> result = task.Result;

        return result.Entity;
    }

    #endregion 

    #region Tests and demos

    public Guid TestTaskToSecuential()
    {
        ServiceRef serviceRef = GetServiceRef(null);

        Task<Result<FolderDto>> task = serviceRef.Service.Folders.GetHomeAsync();
        task.Wait();

        Result<FolderDto> result = task.Result;

        var r = result.Entity.FolderId;

        return r;
    }


    public float DemoSumNum(float x, float y)
    {
        return x + y;
    }

    public object TestNull()
    {
        return null;
    }

    public void TestMsg()
    {
        MessageBox.Show("TEST KNoteScriptLibrary Method");
    }

    public static void TestStatic()
    {
        MessageBox.Show("Static");
    }

    #endregion

    #region Private methods

    private ServiceRef GetServiceRef(string repositoryAlias)
    {
        ServiceRef serviceRef;
        if (string.IsNullOrEmpty(repositoryAlias))
            serviceRef = _store.GetFirstServiceRef();
        else
            serviceRef = _store.GetServiceRef(repositoryAlias);

        if (serviceRef == null)
            throw new Exception("Invalid parameter.");

        return serviceRef;
    }

    #endregion 
}

