using KNote.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KntRedmineApi
{
    public static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            
            Application.Run(new KntRedmineForm(CreateServiceFromConfig()));
        }

        static IPluginCommand CreateServiceFromConfig()
        {
            // TODO: Create new service object from app file config. 

            // cInfo = .... load cInfo from file config ...

            //var repositoryRef = new RepositoryRef
            //{

            //    Alias = cInfo.Alias,
            //    ConnectionString = cInfo.ConnectionString,
            //    Provider = cInfo.Provider,
            //    Orm = cInfo.Orm,
            //    ResourcesContainer = cInfo.ResourcesContainer,
            //    ResourcesContainerCacheRootPath = cInfo.ResourcesContainerCacheRootPath,
            //    ResourcesContainerCacheRootUrl = cInfo.ResourcesContainerCacheRootUrl
            //};
            
            //    var serviceRef = new ServiceRef(repositoryRef);
            //    retrun serviceRef.Service;

            return null;
        }
    }
}
