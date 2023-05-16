using System.Runtime.InteropServices;
using System.Diagnostics;
using KNote.ClientWin.Views;
using KNote.ClientWin.Core;
using KNote.ClientWin.Components;
using KNote.Model;
using KNote.Service.Core;

namespace KNote.ClientWin;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>        
    [STAThread]
    static void Main()        
    {

#if RELEASE
        Process[] instancias = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);

        if (instancias.Length > 1)
        {
            BringToFront();
            return;
        }
#endif

        ApplicationConfiguration.Initialize();

        ApplicationContext applicationContext = new ApplicationContext();
           
        Store appStore = new Store(new FactoryViewsWinForms());

        #region Splash

        SplashForm splashForm = new SplashForm(appStore);
        splashForm.Show();
        Application.DoEvents();

        #endregion
            
        LoadAppStore(appStore);

        #region Demo & lab ... for debug

        // applicationContext.MainForm = new LabForm(appStore);

        #endregion

        #region Normal start

        var knoteManagment = new KNoteManagmentComponent(appStore);
        knoteManagment.Run();
        applicationContext.MainForm = (Form)knoteManagment.View;

        #endregion

        splashForm.Close();

        if (Environment.OSVersion.Version.Major >= 6)
        {
            SetProcessDPIAware();
        }

        Application.Run(applicationContext);            
    }

    static async void LoadAppStore(Store store)
    {
        var pathApp = Application.StartupPath;
        var appFileConfig = Path.Combine(pathApp, "KNoteData.config");

        // Set session values
        store.AppUserName = SystemInformation.UserName;
        store.ComputerName = SystemInformation.ComputerName;
        store.AppConfig.LastDateTimeStart = DateTime.Now;
        store.AppConfig.RunCounter += 1;

        if (!File.Exists(appFileConfig))
        {
            // Create default repository and add link
                                
            var pathData = Path.Combine(pathApp, "Data");
            if (!Directory.Exists(pathData))
                Directory.CreateDirectory(pathData);
            var dbFile = Path.Combine(pathData, $"knote_{SystemInformation.UserName}.db");
                
            var pathResourcesCache = Path.Combine(pathApp, "ResourcesCache");
            if (!Directory.Exists(pathResourcesCache))
                Directory.CreateDirectory(pathResourcesCache);

            var r0 = new RepositoryRef
            {
                Alias = "Personal respository",                    
                ConnectionString = $"Data Source={dbFile}",
                Provider = "Microsoft.Data.Sqlite",
                Orm = "EntityFramework",
                ResourcesContainer = "NotesResources",
                ResourcesContainerRootPath = pathResourcesCache,
                ResourcesContainerRootUrl = @"file:///" + pathResourcesCache.Replace(@"\", @"/")
            };

            var initialServiceRef = new ServiceRef(r0, store.AppUserName);
            var resCreateDB = await initialServiceRef.Service.CreateDataBase(store.AppUserName);

            if (resCreateDB)
            {                    
                store.AddServiceRef(initialServiceRef);
                store.AppConfig.RespositoryRefs.Add(r0);
            }

            // Default values
            store.AppConfig.AutoSaveActivated = true;
            store.AppConfig.AutoSaveSeconds = 105;
            store.AppConfig.AlarmActivated = true;
            store.AppConfig.AlarmSeconds = 30;
            store.AppConfig.LastDateTimeStart = DateTime.Now;
            store.AppConfig.RunCounter = 1;
            store.AppConfig.LogFile = pathApp + @"\KNoteWinApp.log";
            store.AppConfig.LogActivated = false;
        }
        else
        {
            store.LoadConfig(appFileConfig);
            foreach (var r in store.AppConfig.RespositoryRefs)                
                store.AddServiceRef(new ServiceRef(r, store.AppUserName, store.AppConfig.ActivateMessageBroker));
        }

        store.SaveConfig(appFileConfig);

        // default folder
        var firstService = store.GetFirstServiceRef();
        var folder = (await firstService.Service.Folders.GetHomeAsync()).Entity;
        store.DefaultFolderWithServiceRef = new FolderWithServiceRef { ServiceRef = firstService, FolderInfo = folder };
    }

    #region Utils

    [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
    public static extern IntPtr FindWindow(String lpClassName, String lpWindowName);

    [DllImport("USER32.DLL")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("USER32.DLL")]
    public static extern bool ShowWindow(IntPtr hWnd, int i);

    public static void BringToFront()
    {
        IntPtr handle = FindWindow(null, $"{KntConst.AppName} Managment");

        if (handle == IntPtr.Zero)
            return;

        ShowWindow(handle, 1);
        SetForegroundWindow(handle);
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetProcessDPIAware();
    #endregion
}

