using KNote.Model;
using KNote.Service;
using SQLitePCL;

namespace KntRedminePlugin;

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

    private ServiceContext _serviceContext;  
    public ServiceContext ServiceContext
    {
        get { return _serviceContext; }
    }

    public void InjectRepositoryParam(ServiceContext serviceContext)
    {
        _serviceContext = serviceContext;
    }

    public int Execute()
    {
        var f = new RedminePluginForm();
        f.Text = _serviceContext?.Alias;
        f.Show();
        return 0;
    }

}

