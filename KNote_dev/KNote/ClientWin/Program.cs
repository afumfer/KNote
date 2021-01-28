using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using KNote.ClientWin.Views;
using KNote.ClientWin.Core;
using KNote.ClientWin.Components;
using KNote.Model.Dto;
using System.IO;

namespace KNote.ClientWin
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>        
        [STAThread]
        //static async Task Main()
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ApplicationContext applicationContext = new ApplicationContext();
           
            Store appStore = new Store(new FactoryViewsWinForms());

            #region Splash

            SplashForm splashForm = new SplashForm(appStore);
            splashForm.Show();
            Application.DoEvents();

            #endregion 
            
            LoadAppStore(appStore);

            #region Demo & lab

            // applicationContext.MainForm = new LabForm(appStore);

            #endregion

            #region Normal start

            var knoteManagment = new KNoteManagmentComponent(appStore);
            knoteManagment.Run();
            applicationContext.MainForm = (Form)knoteManagment.View;

            #endregion

            splashForm.Close(); 

            Application.Run(applicationContext);            
        }

        static async Task LoadAppStore(Store store)
        //static void LoadAppStore(Store store)
        {            
            var appFileConfig = Path.Combine(Application.StartupPath, "KNoteData.config");

            var appConfig = store.LoadConfig(appFileConfig);
           
            if (appConfig == null)
            {
                // Add some repository for development environment ...

                var r3 = new RepositoryRef
                {
                    Alias = "Tasks db3 (Sqlite)",
                    ConnectionString = @"Data Source=D:\DBs\KNote02DB_Sqlite.db",
                    Provider = "Microsoft.Data.Sqlite",
                    Orm = "EntityFramework"
                };
                store.AddServiceRef(new ServiceRef(r3));


                var r1 = new RepositoryRef
                {
                    Alias = "Test db1 (SQL Server Prod - Dapper)",                    
                    // ConnectionString = @"Data Source=.\sqlexpress;Initial Catalog=KNote02DB;User Id=userKNote;Password=SinclairQL1983;Connection Timeout=60;MultipleActiveResultSets=true;",
                    ConnectionString = @"Data Source=.\sqlexpress;Initial Catalog=KNote02DB;Trusted_Connection=True;Connection Timeout=60;MultipleActiveResultSets=true;",
                    Provider = "Microsoft.Data.SqlClient",
                    Orm = "Dapper"  // Dapper / EntityFramework
                };             
                store.AddServiceRef(new ServiceRef(r1));

                var r2 = new RepositoryRef
                {
                    Alias = "Test db2 (SQL Server Desa - Dapper)",                    
                    ConnectionString = @"Data Source=.\sqlexpress;Initial Catalog=KNote02DesaDB;Trusted_Connection=True;Connection Timeout=60;MultipleActiveResultSets=true;",
                    Provider = "Microsoft.Data.SqlClient",
                    Orm = "Dapper" 
                };
                store.AddServiceRef(new ServiceRef(r2));

                appConfig = new AppConfig();
                appConfig.RespositoryRefs.Add(r1);
                appConfig.RespositoryRefs.Add(r2);
                appConfig.RespositoryRefs.Add(r3);
                appConfig.AutoSaveMinutes = 10;
                appConfig.LastDateTimeStart = DateTime.Now;
                appConfig.RunCounter = 1;
            }
            else
            {
                foreach(var r in appConfig.RespositoryRefs)
                {
                    store.AddServiceRef(new ServiceRef(r));
                }

            }

            // TODO: add default values
            store.AppUserName = SystemInformation.UserName;
            store.ComputerName = SystemInformation.ComputerName;
            store.LogFile = Application.StartupPath + @"\KNoteWinApp.log";
            store.LogActivated = false;

            store.SaveConfig(appConfig, appFileConfig);

            // default folder
            var firstService = store.GetFirstServiceRef();
            var folder = (await firstService.Service.Folders.GetHomeAsync()).Entity;
            store.DefaultFolderWithServiceRef = new FolderWithServiceRef { ServiceRef = firstService, FolderInfo = folder };

            #region Doc data for repositories

            //"_DefaultORM": "EntityFramework",
            //"DefaultORM": "Dapper",

            //"DefaultProvider": "Microsoft.Data.SqlClient",
            //"DefaultConnection": "Data Source=.\\sqlexpress;Initial Catalog=KNote02DB;User Id=userKNote;Password=SinclairQL1983;Connection Timeout=60;MultipleActiveResultSets=true;",

            //"_DefaultProvider": "Microsoft.Data.Sqlite",
            //"_DefaultConnection": "Data Source=D:\\DBs\\KNote02DB_Sqlite.db"

            #endregion
        }
    }
}
