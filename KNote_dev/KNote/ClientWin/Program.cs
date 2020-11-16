using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using KNote.ClientWin.Views;
using KNote.ClientWin.Core;
using KNote.ClientWin.Components;

namespace KNote.ClientWin
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static async Task Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ApplicationContext applicationContext = new ApplicationContext();
           
            Store appStore = new Store(new FactoryViewsWinForms());

            #region Splash

            //SplashForm splashForm = new SplashForm(appStore);
            //splashForm.Show();
            //Application.DoEvents();

            #endregion 

            await LoadAppStore(appStore);

            #region Demo & lab

            // applicationContext.MainForm = new LabForm(appStore);

            #endregion

            #region Normal start
            
            var knoteManagment = new KNoteManagmentComponent(appStore);
            knoteManagment.Run();
            applicationContext.MainForm = (Form)knoteManagment.View;

            #endregion

            //splashForm.Close(); 

            Application.Run(applicationContext);

            //Application.Run(new DemoForm());
        }

        static async Task LoadAppStore(Store store)
        {
            // Hard values for test ....

            store.LogFile = Application.StartupPath + @"\KNoteWinApp.log";
            store.LogActivated = false;

            var defaultServiceRef = new ServiceRef(
                "Test db1 (SQL Server Prod - Dapper",
                @"Data Source=.\sqlexpress;Initial Catalog=KNote02DB;User Id=userKNote;Password=SinclairQL1983;Connection Timeout=60;MultipleActiveResultSets=true;",
                "Microsoft.Data.SqlClient",
                "EntityFramework");  // Dapper

            //var defaultServiceRef = new ServiceRef(
            //    "Tasks db3 (Sqlite)",
            //    @"Data Source=D:\DBs\KNote02DB_Sqlite.db",
            //    "Microsoft.Data.Sqlite",
            //    "Dapper");


            var folder = (await defaultServiceRef.Service.Folders.GetHomeAsync()).Entity;

            store.AddServiceRef(defaultServiceRef);

            store.UpdateActiveFolder(new FolderWithServiceRef { FolderInfo = folder, ServiceRef = defaultServiceRef });

            #region Datos para repositorios - diferentes alternativas 

            //"_DefaultORM": "EntityFramework",
            //"DefaultORM": "Dapper",

            //"DefaultProvider": "Microsoft.Data.SqlClient",
            //"DefaultConnection": "Data Source=.\\sqlexpress;Initial Catalog=KNote02DB;User Id=userKNote;Password=SinclairQL1983;Connection Timeout=60;MultipleActiveResultSets=true;",

            //"_DefaultProvider": "Microsoft.Data.Sqlite",
            //"_DefaultConnection": "Data Source=D:\\DBs\\KNote02DB_Sqlite.db"

            #endregion 

            store.AddServiceRef(new ServiceRef
                ("Test db2 (SQL Server Desa - Dapper)",
                @"Data Source=.\sqlexpress;Initial Catalog=KNote02DesaDB;User Id=userKNote;Password=SinclairQL1983;Connection Timeout=60;MultipleActiveResultSets=true;",
                "Microsoft.Data.SqlClient",
                "Dapper"));


            // --------------------------

            store.AddServiceRef(new ServiceRef
                ("Tasks db3 (Sqlite)",
                @"Data Source=D:\DBs\KNote02DB_Sqlite.db",
                "Microsoft.Data.Sqlite",
                "EntityFramework"));

            //store.AddServiceRef(new ServiceRef(
            //    "Test db1 (SQL Server Prod - Dapper",
            //    @"Data Source=.\sqlexpress;Initial Catalog=KNote02DB;User Id=userKNote;Password=SinclairQL1983;Connection Timeout=60;MultipleActiveResultSets=true;",
            //    "Microsoft.Data.SqlClient",
            //    "Dapper"));

        }
    }
}
