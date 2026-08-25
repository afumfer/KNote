using System.Runtime.InteropServices;
using System.Runtime.ExceptionServices;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using NLog;
using KNote.ClientWin.Views;
using KNote.ClientWin.Core;
using KNote.ClientWin.Controllers;
using KNote.Model;
using KNote.Service.Core;
using NLog.Extensions.Logging;

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
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        ApplicationConfiguration.Initialize();
        Store appStore = new Store(new FactoryViewsWinForms());
        SplashForm splashForm = new SplashForm(appStore);
        Exception loadException = null;

        try
        {
            // LoadAppStore does real async I/O (repository access) and can show a modal dialog
            // (Store.EnsureCurrentUserRegistered). It must finish before KNoteManagmentCtrl is
            // created. Kicking it off from SplashForm.Shown, under a real Application.Run(splashForm)
            // message loop, lets every "await" marshal its continuation back onto this UI thread the
            // normal WinForms way. A manual Application.DoEvents() polling loop here instead (run
            // before any Application.Run() call) cannot be trusted for that: nothing guarantees a
            // continuation resumes on this same thread once nested pumps are involved (ShowDialog's
            // own loop, plus SplashForm's own DoEvents() call in AppContext_AddedServiceRef), which is
            // exactly what caused a cross-thread control access as soon as repository loading had more
            // than one "await" in a row (e.g. right after cancelling the registration dialog).
            splashForm.Shown += async (s, e) =>
            {
                try
                {
                    await LoadAppStore(appStore);
                }
                catch (Exception ex)
                {
                    loadException = ex;
                }
                finally
                {
                    splashForm.Close();
                }
            };

            Application.Run(splashForm);

            if (loadException != null)
                ExceptionDispatchInfo.Capture(loadException).Throw();

            var knoteManagment = new KNoteManagmentCtrl(appStore);
            knoteManagment.Run();

            Application.Run((Form)knoteManagment.View);

            appStore.Logger?.LogInformation("KNote finalized");
        }
        catch (Exception ex)
        {
            appStore.Logger?.LogCritical(ex, "KNote has stopped because there was an exception.");
            throw;
        }
        finally
        {
            LogManager.Shutdown();
        }
    }

    static async Task LoadAppStore(Store store)
    {
        var pathApp = Application.StartupPath;

        AppUserDataPath.EnsureExists();
        var appFileConfig = AppUserDataPath.ConfigFile;

        // One-time migration: older versions kept KNoteData.config next to the application binaries.
        // If the user data folder doesn't have a config yet but a legacy one is found, copy it over;
        // once the copy is confirmed to have landed correctly, remove the legacy file so it doesn't
        // linger as an orphaned, no-longer-read duplicate.
        var legacyFileConfig = Path.Combine(pathApp, "KNoteData.config");
        if (!File.Exists(appFileConfig) && File.Exists(legacyFileConfig))
        {
            File.Copy(legacyFileConfig, appFileConfig);

            var copiedOk = File.Exists(appFileConfig)
                && new FileInfo(appFileConfig).Length == new FileInfo(legacyFileConfig).Length;
            if (copiedOk)
            {
                try
                {
                    File.Delete(legacyFileConfig);
                }
                catch (Exception)
                {
                    // Non-fatal: worst case the legacy file lingers as an unused duplicate.
                }
            }
        }

        // Set session values
        store.AppUserName = SystemInformation.UserName;
        store.ComputerName = SystemInformation.ComputerName;

        // Log configuration
        if (File.Exists(Path.Combine(pathApp, "NLog.config")))
        {
            LogManager.Setup().LoadConfigurationFromFile(Path.Combine(pathApp, "NLog.config"));
            store.Logger = new NLogLoggerFactory().CreateLogger<Store>();
        }
        else
            store.Logger = null;

        // Create default repository and add link
        if (!File.Exists(appFileConfig))
        {
            var pathData = Path.Combine(AppUserDataPath.Directory, "Data");
            if (!Directory.Exists(pathData))
                Directory.CreateDirectory(pathData);
            var dbFile = Path.Combine(pathData, $"knote_{SystemInformation.UserName}.db");

            var pathResourcesCache = Path.Combine(AppUserDataPath.Directory, "ResourcesCache");
            if (!Directory.Exists(pathResourcesCache))
                Directory.CreateDirectory(pathResourcesCache);

            var r0 = new RepositoryRef
            {
                Alias = "Personal respository",                    
                ConnectionString = $"Data Source={dbFile}",
                Provider = "Microsoft.Data.Sqlite",
                Orm = "EntityFramework",
                ResourcesContainer = "KntCntResources",
                ResourcesContainerRootPath = pathResourcesCache,
                ResourcesContainerRootUrl = @"file:///" + pathResourcesCache.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            };

            var initialServiceRef = new ServiceRef(r0, store.AppUserName, false, store.Logger);
            var resCreateDB = await initialServiceRef.Service.CreateDataBase(store.AppUserName);

            if (resCreateDB)
            {                    
                store.AddServiceRef(initialServiceRef);
                store.SetAssistantServiceRef(null);
                store.AppConfig.RespositoryRefs.Add(r0);
                store.AppConfig.AssistantRespositoryRef = null;
            }

            // Default values
            store.AppConfig.AutoSaveActivated = true;
            store.AppConfig.AutoSaveSeconds = 105;
            store.AppConfig.AlarmActivated = true;
            store.AppConfig.AlarmSeconds = 30;
            store.AppConfig.LastDateTimeStart = DateTime.Now;
            store.AppConfig.RunCounter = 1;
            store.AppConfig.LogFile = Path.Combine(AppUserDataPath.Directory, "KNoteWinApp.log");
            store.AppConfig.LogActivated = false;
        }
        // Load sevices references
        else
        {
            store.LoadConfig(appFileConfig);

            // Migrate LogFile away from the old default location next to the binaries, if still set to it.
            var legacyLogFile = Path.Combine(pathApp, "KNoteWinApp.log");
            if (store.AppConfig.LogFile == legacyLogFile)
                store.AppConfig.LogFile = Path.Combine(AppUserDataPath.Directory, "KNoteWinApp.log");

            foreach (var r in store.AppConfig.RespositoryRefs)
            {
                var serviceRef = new ServiceRef(r, store.AppUserName, store.AppConfig.ActivateMessageBroker, store.Logger);
                store.AddServiceRef(serviceRef);
                await store.EnsureCurrentUserRegistered(serviceRef.Service);
            }


            if (store.AppConfig.AssistantRespositoryRef?.ConnectionString != null)
                store.SetAssistantServiceRef(new ServiceRef(store.AppConfig.AssistantRespositoryRef, store.AppUserName, store.AppConfig.ActivateMessageBroker, store.Logger));
            else
                store.SetAssistantServiceRef(null);
        }

        store.AppConfig.LastDateTimeStart = DateTime.Now;
        store.AppConfig.RunCounter += 1;
        if (string.IsNullOrEmpty(store.AppConfig.ChatGPTDefaultModel))
            store.AppConfig.ChatGPTDefaultModel = "gpt-4o-mini";

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

    #endregion
}

