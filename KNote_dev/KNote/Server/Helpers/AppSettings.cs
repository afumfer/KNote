using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KNote.Server.Helpers
{
    public class AppSettings
    {
        public string Secret { get; set; }

        //TODO: Poner otras propiedades, aquí y en el json como
        //"ApplicationName": "MyApp",
        // "Version": "1.0.0"

        public string ApplicationName { get; set; }
        public string Version { get; set; }
    }
}
