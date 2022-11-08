using KNote.Model;
using KNote.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace KntRedmineApi;

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

    static IPluginCommand? CreateServiceFromConfig()
    {
        var configFile = Path.Combine(Application.StartupPath, "KntRedmine.config");
        try
        {
            if (!File.Exists(configFile))
                return null;

            TextReader reader = new StreamReader(configFile);
            XmlSerializer serializer = new XmlSerializer(typeof(KntRedmineConfig));
            var appConfig = (KntRedmineConfig)serializer.Deserialize(reader);
            reader.Close();

            if (appConfig == null)
                return null;

            var serviceRef = new ServiceRef(appConfig.RepositoryRef);

            IPluginCommand plugin = new KntRedminePluginCommand
            {
                Service = serviceRef.Service
            };

            return plugin;
        }
        catch (Exception)
        {
            return null;
        }
    }

}
