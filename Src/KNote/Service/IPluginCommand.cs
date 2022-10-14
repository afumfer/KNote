using KNote.Model;
using KNote.Service.Services;

namespace KNote.Service;

public interface IPluginCommand
{
    string Name { get; }
    string Description { get; }
    
    ServiceContext ServiceContext { get; }

    public int Execute();

    public void InjectRepositoryParam(ServiceContext serviceContext);    

}

public class ServiceContext
{
    public string Alias { get; set; }
    public string ConnectionString { get; set; }
    public string Provider { get; set; }
    public string Orm { get; set; }
    public string ResourcesContainer { get; set; }
    public string ResourcesContainerCacheRootPath { get; set; }
    public string ResourcesContainerCacheRootUrl { get; set; }
}
