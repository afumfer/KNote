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

        static async void LoadAppStore(Store store)
        {
            var rootAppPath = Application.StartupPath;

            var appFileConfig = Path.Combine(rootAppPath, "KNoteData.config");

            store.LoadConfig(appFileConfig);

            if (store.AppConfig == null)
            {
                // Create default repository and add link

                // TODO: 
                // - Crear si no existe el directroio por defecto para la base de datos
                // - Crear el directorio por defecto para la caché de recursos. 
                // - Crear la base de datos con el nombre de usuario
                // - Añadir a la lista de repositorios
                // - OJO: añadir por defecto el usuario que crea el repositorio como usuario-admin

                var r0 = new RepositoryRef
                {
                    Alias = "Personal respository",
                    ConnectionString = @"Data Source=D:\DBs\KNote05DB_Sqlite.db",
                    Provider = "Microsoft.Data.Sqlite",
                    Orm = "EntityFramework"
                };
                store.AddServiceRef(new ServiceRef(r0));
                store.AppConfig.RespositoryRefs.Add(r0);


                // Default values
                store.AppConfig.AutoSaveSeconds = 105;
                store.AppConfig.AlarmSeconds = 30;
                store.AppConfig.LastDateTimeStart = DateTime.Now;
                store.AppConfig.RunCounter = 1;
                store.AppConfig.LogFile = rootAppPath + @"\KNoteWinApp.log";
                store.AppConfig.LogActivated = false;
                store.AppConfig.CacheResources = @"D:\Resources\knt";
                store.AppConfig.CacheUrlResources = @"http://afx.hopto.org/kntres/NotesResources";

                #region Examples info for repositories

                // For debug and tests

                //var r3 = new RepositoryRef
                //{
                //    Alias = "Tasks db3 (Sqlite)",
                //    ConnectionString = @"Data Source=D:\DBs\KNote05DB_Sqlite.db",
                //    Provider = "Microsoft.Data.Sqlite",
                //    Orm = "EntityFramework"
                //};
                //store.AddServiceRef(new ServiceRef(r3));

                //var r1 = new RepositoryRef
                //{
                //    Alias = "Test db1 (SQL Server Prod - Dapper)",
                //    // ConnectionString = @"Data Source=.\sqlexpress;Initial Catalog=KNote05DB;User Id=userKNote;Password=SinclairQL1983;Connection Timeout=60;MultipleActiveResultSets=true;",
                //    ConnectionString = @"Data Source=.\sqlexpress;Initial Catalog=KNote05DB;Trusted_Connection=True;Connection Timeout=60;MultipleActiveResultSets=true;",
                //    Provider = "Microsoft.Data.SqlClient",
                //    Orm = "Dapper"  // Dapper / EntityFramework
                //};
                //store.AddServiceRef(new ServiceRef(r1));

                //var r2 = new RepositoryRef
                //{
                //    Alias = "Test db2 (SQL Server Desa - Dapper)",
                //    ConnectionString = @"Data Source=.\sqlexpress;Initial Catalog=KNote05DesaDB;Trusted_Connection=True;Connection Timeout=60;MultipleActiveResultSets=true;",
                //    Provider = "Microsoft.Data.SqlClient",
                //    Orm = "EntityFramework"
                //};
                //store.AddServiceRef(new ServiceRef(r2));

                //"_DefaultORM": "EntityFramework",
                //"DefaultORM": "Dapper",

                //"DefaultProvider": "Microsoft.Data.SqlClient",
                //"DefaultConnection": "Data Source=.\\sqlexpress;Initial Catalog=KNote02DB;User Id=userKNote;Password=SinclairQL1983;Connection Timeout=60;MultipleActiveResultSets=true;",

                //"_DefaultProvider": "Microsoft.Data.Sqlite",
                //"_DefaultConnection": "Data Source=D:\\DBs\\KNote02DB_Sqlite.db"

                //store.AppConfig.RespositoryRefs.Add(r1);
                //store.AppConfig.RespositoryRefs.Add(r2);
                //store.AppConfig.RespositoryRefs.Add(r3);

                #endregion
            }
            else
            {
                foreach (var r in store.AppConfig.RespositoryRefs)                
                    store.AddServiceRef(new ServiceRef(r));                
            }

            // Set session values
            store.AppUserName = SystemInformation.UserName;
            store.ComputerName = SystemInformation.ComputerName;
            store.AppConfig.LastDateTimeStart = DateTime.Now;
            store.AppConfig.RunCounter += 1;

            store.SaveConfig(appFileConfig);

            // default folder
            var firstService = store.GetFirstServiceRef();
            var folder = (await firstService.Service.Folders.GetHomeAsync()).Entity;
            store.DefaultFolderWithServiceRef = new FolderWithServiceRef { ServiceRef = firstService, FolderInfo = folder };

        }

    }
}
