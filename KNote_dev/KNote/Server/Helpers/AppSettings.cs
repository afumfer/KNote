using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KNote.Server.Helpers
{
    public class AppSettings
    {
        public string Secret { get; set; }
        public string ApplicationName { get; set; }
        public string Version { get; set; }
        public string ContainerResources { get; set; }
        public string ContainerResourcesRootPath { get; set; }        
    }
}
