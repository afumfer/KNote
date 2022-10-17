using KNote.Model;
using KNote.Repository;
using KNote.Service;

namespace KntRedmineApi;

public class KntRedminePluginCommand : IPluginCommand
{
    public string Name 
    { 
        get 
        { 
            return "KntRedminePlugin"; 
        } 
    } 

    public string Description
    {
        get
        {
            return "Redmine Plugin for KaNote";
        }
    }

    private PluginContext? _pluginContext;  
    public PluginContext? PluginContext
    {
        get { return _pluginContext; }
    }

    public void InjectRepositoryParam(PluginContext? pluginContext)
    {
        _pluginContext = pluginContext;
    }

    public int Execute()
    {
        var xx = _pluginContext;
        var f = new KntRedminePluginForm(_pluginContext);
        //var f = new KntRedminePluginForm();
        f.Show();

        //if (_pluginContext != null)
        //{

        //    var repositoryRef = new RepositoryRef
        //    {

        //        Alias = _pluginContext?.RepositoryInfo.Alias,
        //        ConnectionString = _pluginContext?.RepositoryInfo.ConnectionString,
        //        Provider = _pluginContext?.RepositoryInfo.Provider,
        //        Orm = _pluginContext?.RepositoryInfo.Orm,
        //        ResourcesContainer = _pluginContext?.RepositoryInfo.ResourcesContainer,
        //        ResourcesContainerCacheRootPath = _pluginContext?.RepositoryInfo.ResourcesContainerCacheRootPath,
        //        ResourcesContainerCacheRootUrl = _pluginContext?.RepositoryInfo.ResourcesContainerCacheRootUrl
        //    };


        //    var serviceRef = new ServiceRef();
        //    var serviceRef2 = new ServiceRef(repositoryRef);
        //    //_service = serviceRef.Service;            
        //}



        return 0;
    }

}

