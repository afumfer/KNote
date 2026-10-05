using System.Runtime.InteropServices;
using System.Runtime.ExceptionServices;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using NLog;
using KNote.ClientWin.Views;
using KNote.ClientWin.Core;
using KNote.ClientWin.Controllers;
using KNote.ClientWin.Utils;
using KNote.Model;
using KNote.Service.Core;
using NLog.Extensions.Logging;

namespace KNote.ClientWin;

static class Program
{
    // Command line argument of the instance started by a restart (see Store.RestartRequested).
    private const string RestartArgument = "--restart";

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>        
    [STAThread]
    static void Main(string[] args)
    {
#if RELEASE
        var currentProcess = Process.GetCurrentProcess();
        // Started by a restart: the previous instance may still be closing, and would otherwise make this
        // one quit below as a second instance.
        if (args.Contains(RestartArgument))
        {
            foreach (var previous in Process.GetProcessesByName(currentProcess.ProcessName).Where(p => p.Id != currentProcess.Id))
                previous.WaitForExit(15000);
        }

        Process[] instancias = Process.GetProcessesByName(currentProcess.ProcessName);
        if (instancias.Length > 1)
        {
            BringToFront();
            return;
        }
#endif
        // Visual styles, text rendering and high DPI mode (SystemAware) come from the csproj.
        ApplicationConfiguration.Initialize();
        Store appStore = new Store(new FactoryViewsWinForms());
        appStore.AccessDeniedNotifier = message =>
            KntMessageBox.Show(message, KntConst.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        RegisterGlobalExceptionHandlers(appStore);
        SplashForm splashForm = new SplashForm(appStore);
        Exception loadException = null;

        try
        {
            // LoadAppStore does real async I/O (repository access) and can show modal dialogs (sign-in,
            // Store.AuthenticateRepositoryAsync's registration). It must finish before KNoteManagementCtrl is
            // created. Kicking it off from SplashForm.Shown, under a real Application.Run(splashForm)
            // message loop, lets every "await" marshal its continuation back onto this UI thread the
            // normal WinForms way. A manual Application.DoEvents() polling loop here instead (run
            // before any Application.Run() call) cannot be trusted for that: nothing guarantees a
            // continuation resumes on this same thread once nested pumps are involved (ShowDialog's
            // own loop, plus SplashForm's own DoEvents() call in AppContext_AddedServiceRef), which is
            // exactly what caused a cross-thread control access as soon as repository loading had more
            // than one "await" in a row (e.g. right after cancelling the registration dialog).
            //
            // A single message loop for the whole session: its main form is the splash first, then the
            // management window, so the splash stays on screen until that window is complete (it starts
            // hidden (cloaked) and appears once its panels are built, see KNoteManagementForm_Shown) instead
            // of leaving a gap between both windows.
            var appContext = new ApplicationContext(splashForm);

            splashForm.Shown += async (s, e) =>
            {
                try
                {
                    if (!await LoadAppStore(appStore))
                    {
                        splashForm.Close();
                        return;
                    }

                    // Light/dark mode: only the windows created from here on follow it (see AppTheme). The
                    // splash and, on first run, the user registration dialog have already been shown in the
                    // default mode.
                    AppTheme.Apply(appStore.Settings.General.ColorMode);

                    // knoteManagement.Run() can end up displaying a note whose content uses WebView2 (e.g.
                    // it reactivates the last active folder, see Store.ChangeActiveFolderWithServiceRef):
                    // if its first note uses the WebView2 content mode, CoreWebView2Environment.CreateAsync
                    // needs this message loop pumping and the window shown. Run before that, WebView2's
                    // own marshaling can complete off the UI thread, so every await further up the call
                    // chain (up to NoteEditorForm.ModelToControls) then resumes off-thread too and throws
                    // a cross-thread InvalidOperationException. Hence the deferral to
                    // IViewKNoteManagement.ViewShown.
                    var knoteManagement = new KNoteManagementCtrl(appStore);
                    knoteManagement.View.ViewShown += (_, _) =>
                    {
                        try
                        {
                            knoteManagement.Run();
                        }
                        finally
                        {
                            // Posted: processed once the management window has become visible, right
                            // after this handler returns.
                            splashForm.BeginInvoke(new MethodInvoker(splashForm.Close));
                        }
                    };

                    var managementForm = (Form)knoteManagement.View;
                    managementForm.Show();
                    // From here on, closing the splash no longer ends the message loop: closing this does.
                    appContext.MainForm = managementForm;
                }
                catch (Exception ex)
                {
                    loadException = ex;
                    splashForm.Close();
                }
            };

            Application.Run(appContext);

            if (loadException != null)
                ExceptionDispatchInfo.Capture(loadException).Throw();

            appStore.Logger?.LogInformation("KNote finalized");

            if (appStore.RestartRequested)
                Process.Start(Environment.ProcessPath, RestartArgument);
        }
        catch (Exception ex)
        {
            // Not rethrown: that ended the process with no message at all for the user.
            appStore.Logger?.LogCritical(ex, "KNote has stopped because there was an exception.");
            KntMessageBox.Show($"{KntConst.AppName} could not start and will be closed. The error has been logged.\r\n\r\n{ex.Message}",
                KntConst.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            LogManager.Shutdown();
        }
    }

    // False: the user chose to close the application while signing in (no error to report).
    static async Task<bool> LoadAppStore(Store store)
    {
        var pathApp = Application.StartupPath;

        AppUserDataPath.EnsureExists();
        var appFileConfig = AppUserDataPath.ConfigFile;

        // Keep WebView2's browser profile/cache out of the app's binaries folder, consistent with
        // Data/ResourcesCache/config/log already living under AppUserDataPath.Directory.
        KntWebView.KntEditView.WebView2UserDataFolder = Path.Combine(AppUserDataPath.Directory, "WebView2Cache");

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
        if (!ConfigFileExists(store, appFileConfig))
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
                Alias = "Personal repository",                    
                ConnectionString = $"Data Source={dbFile}",
                Provider = "Microsoft.Data.Sqlite",
                Orm = "EntityFramework",
                ResourcesContainer = "KntCntResources",
                ResourcesContainerRootPath = pathResourcesCache,
                ResourcesContainerRootUrl = @"file:///" + pathResourcesCache.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            };

            // Linked below, with the rest of the configured repositories (there are none yet): the new
            // database only has its seeded users, so the user registers there and, as the first one after
            // adminKNote, becomes its Admin (see KntUsersRegisterAsyncCommand). A first run always signs in
            // with the Windows account (SecurityConfig's default).
            var initialServiceRef = new ServiceRef(r0, store.AppUserName, false, store.Logger);
            var resCreateDB = await initialServiceRef.Service.CreateDataBase();

            if (resCreateDB)
                store.Settings.Repositories.Items.Add(r0);
            store.SetAssistantServiceRef(null);
            store.Security.StartSession(AppAuthenticationMode.Windows);

            // Default values
            store.Settings.General.AutoSaveActivated = true;
            store.Settings.General.AutoSaveSeconds = 105;
            store.Settings.General.AlarmActivated = true;
            store.Settings.General.AlarmSeconds = 30;
            store.State.Session.LastDateTimeStart = DateTime.Now;
            store.State.Session.RunCounter = 1;
            store.Settings.General.LogFile = Path.Combine(AppUserDataPath.Directory, "KNoteWinApp.log");
            store.Settings.General.LogActivated = false;
        }
        // Load sevices references
        else
        {
            store.LoadConfig(appFileConfig);

            // Migrate LogFile away from the old default location next to the binaries, if still set to it.
            var legacyLogFile = Path.Combine(pathApp, "KNoteWinApp.log");
            if (store.Settings.General.LogFile == legacyLogFile)
                store.Settings.General.LogFile = Path.Combine(AppUserDataPath.Directory, "KNoteWinApp.log");

            if (!await SignInAsync(store))
                return false;

            // The Assistant repository is only read, as a catalog of templates, prompts and code
            // snippets (Store.GetCatalogItem/GetIncludeCode): the user isn't registered there, so it
            // runs without per-user authorization. Editing those notes goes through the repository's
            // own link, if it is also linked, which is authorized as usual.
            if (store.Settings.Repositories.Assistant?.ConnectionString != null)
                store.SetAssistantServiceRef(TryCreateServiceRef(store, store.Settings.Repositories.Assistant, enforceAuthorization: false));
            else
                store.SetAssistantServiceRef(null);
        }

        if (!await LinkRepositoriesAsync(store))
            return false;

        // No repository at all (the default one could not be created, or none of the configured ones
        // could be opened): stop before SaveConfig, which would overwrite the configuration with an
        // empty repository list. Main reports this to the user.
        var firstService = store.GetFirstServiceRef();
        if (firstService == null)
            throw new InvalidOperationException("No repository could be opened. The details have been logged.");

        store.State.Session.LastDateTimeStart = DateTime.Now;
        store.State.Session.RunCounter += 1;

        store.SaveConfig(appFileConfig);

        // default folder
        var folder = (await firstService.Service.Folders.GetHomeAsync()).Entity;
        store.DefaultFolderWithServiceRef = new FolderWithServiceRef { ServiceRef = firstService, FolderInfo = folder };

        return true;
    }

    // Starts the session's identity (Store.AppUserName, Store.Security): the Windows account, or, when the
    // application is set to sign in with a KNote user name and password, whatever the user types in the
    // sign-in dialog (which can also switch back to the Windows account). False: the user closed the
    // dialog, so the application must close.
    static async Task<bool> SignInAsync(Store store)
    {
        if (store.Settings.Security.AuthenticationMode != AppAuthenticationMode.Credentials)
        {
            store.AppUserName = SystemInformation.UserName;
            store.Security.StartSession(AppAuthenticationMode.Windows);
            return true;
        }

        var loginCtrl = new LoginCtrl(store);
        await loginCtrl.NewModel();
        return loginCtrl.RunModal().Entity == EControllerResult.Executed;
    }

    // Links every configured repository the session's user may use (see Store.AuthenticateRepositoryAsync).
    // A repository that can't be opened right now (e.g. its SQL Server is stopped or not reachable yet:
    // ServiceRef's constructor already connects, see KntRepositoryFactory) or that refuses the user (not
    // registered, wrong password, disabled) is skipped for this session only, instead of aborting the
    // whole application: it stays in the configuration and is tried again on the next startup. When every
    // repository could be opened but none accepted the user, the user can try again (signing in again,
    // with credentials) or close the application (false).
    static async Task<bool> LinkRepositoriesAsync(Store store)
    {
        while (true)
        {
            var refusals = new List<string>();
            var anyOpened = false;

            foreach (var r in store.Settings.Repositories.Items)
            {
                var serviceRef = TryCreateServiceRef(store, r);
                if (serviceRef == null)
                    continue;
                anyOpened = true;

                Result authentication;
                try
                {
                    authentication = await store.AuthenticateRepositoryAsync(serviceRef.Service);
                }
                catch (Exception ex)
                {
                    store.Logger?.LogError(ex, "Checking the current user in repository {alias} failed.", r.Alias);
                    authentication = new Result();
                    authentication.AddErrorMessage($"The user '{store.AppUserName}' could not be checked in the repository '{r.Alias}', so it is not available in this session.{Environment.NewLine}({ex.Message})");
                }

                if (authentication.IsValid)
                    store.AddServiceRef(serviceRef);
                else
                {
                    store.Logger?.LogWarning("Repository {alias} not linked: {reason}", r.Alias, authentication.ErrorMessage);
                    refusals.Add(authentication.ErrorMessage);
                }
            }

            // Linked something, or nothing could even be opened (reported by the caller): the refusals
            // are shown once the main window is up, through the config notices.
            if (store.GetFirstServiceRef() != null || !anyOpened)
            {
                foreach (var refusal in refusals)
                    store.AddConfigNotice(refusal);
                return true;
            }

            var answer = KntMessageBox.Show($"{string.Join(Environment.NewLine + Environment.NewLine, refusals)}"
                + $"{Environment.NewLine}{Environment.NewLine}{KntConst.AppName} needs at least one repository. Do you want to try again?",
                KntConst.AppName, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (answer != DialogResult.Yes)
                return false;

            if (store.Security.AuthenticationMode == AppAuthenticationMode.Credentials && !await SignInAsync(store))
                return false;
        }
    }

    // Only a real first run (neither the configuration nor its backup exist) may create the default
    // repository and a brand new configuration. KNoteData.config can be missing for a moment while
    // another KNote instance replaces it (File.Replace is not atomic for other processes), or be
    // missing with its backup left behind: treating either case as a first run used to overwrite the
    // user's configuration with an empty repository list.
    static bool ConfigFileExists(Store store, string configFile)
    {
        if (File.Exists(configFile))
            return true;

        var backupFile = configFile + XmlConfigFile.BackupExtension;
        if (!File.Exists(backupFile))
            return false;

        Thread.Sleep(500);
        if (File.Exists(configFile))
            return true;

        File.Copy(backupFile, configFile);
        store.Logger?.LogWarning("{configFile} was missing and has been restored from its backup.", configFile);
        store.AddConfigNotice($"'{Path.GetFileName(configFile)}' was missing, so it has been restored from its last backup.");
        return true;
    }

    // Returns null (logged, and reported to the user once the main window is shown, through the
    // config notices) when the repository can't be opened.
    static ServiceRef TryCreateServiceRef(Store store, RepositoryRef repositoryRef, bool enforceAuthorization = true)
    {
        try
        {
            return new ServiceRef(repositoryRef, store.AppUserName, store.Settings.Connectivity.MessageBroker.Activated, store.Logger,
                enforceAuthorization);
        }
        catch (Exception ex)
        {
            store.Logger?.LogError(ex, "Repository {alias} could not be opened.", repositoryRef.Alias);
            store.AddConfigNotice($"The repository '{repositoryRef.Alias}' could not be opened, so it is not available in this session. "
                + $"It will be tried again the next time {KntConst.AppName} starts.{Environment.NewLine}({ex.Message})");
            return null;
        }
    }

    // Last line of defense. Exceptions escaping an "async void" handler (alarm timers, PostIts
    // reopened at startup, WebView2 initialization, ...) or a fire-and-forget task used to end up in
    // WinForms' default "unhandled exception" dialog without leaving any trace in the log. They are
    // now logged, and UI thread ones are reported to the user while the app keeps running (same as
    // the default dialog's "Continue"). Store.Logger is only created inside LoadAppStore, so anything
    // failing before that point is still shown but not logged.
    static void RegisterGlobalExceptionHandlers(Store store)
    {
        // Must be called before the first window is created.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

        Application.ThreadException += (s, e) =>
        {
            store.Logger?.LogError(e.Exception, "Unhandled exception in the UI thread.");
            KntMessageBox.Show(
                $"An unexpected error has occurred and has been logged. {KntConst.AppName} will keep running.\r\n\r\n{e.Exception.Message}",
                KntConst.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
        };

        // Non-UI thread: the process is terminated by the runtime right after this handler, so the
        // only thing left to do is make sure the exception reaches the log file.
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            store.Logger?.LogCritical(e.ExceptionObject as Exception, "Unhandled exception in a non-UI thread. KNote will be terminated.");
            LogManager.Flush();
        };

        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            store.Logger?.LogWarning(e.Exception, "Unobserved task exception.");
            e.SetObserved();
        };

        KntWebView.KntEditView.WebView2ProcessFailed += (s, e) =>
            store.Logger?.LogWarning("WebView2 process failed. Kind: {kind}, reason: {reason}, exit code: {exitCode}, process: {description}.",
                e.ProcessFailedKind, e.Reason, e.ExitCode, e.ProcessDescription);

        KntWebView.KntEditView.WebView2ErrorOccurred += (s, ex) =>
            store.Logger?.LogError(ex, "WebView2 error.");
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
        IntPtr handle = FindWindow(null, $"{KntConst.AppName} Management");

        if (handle == IntPtr.Zero)
            return;

        ShowWindow(handle, 1);
        SetForegroundWindow(handle);
    }

    #endregion
}

