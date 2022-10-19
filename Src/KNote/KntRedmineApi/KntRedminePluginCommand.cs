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

    IKntService? _service;
    public IKntService? Service
    {
        get { return _service; }
    }

    public void InjectService(IKntService? service)
    {
        _service = service;
    }

    public int Execute()
    {        
        var f = new KntRedmineForm(_service);
        f.Show();
       
        return 0;
    }


}

