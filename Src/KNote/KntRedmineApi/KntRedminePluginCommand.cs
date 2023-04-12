using KNote.Model;
using KNote.Repository;
using KNote.Service.Core;

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
            return $"Redmine Plugin for {KntConst.AppName}";
        }
    }

    IKntService _service;
    public IKntService Service
    {
        get { return _service; }
        set { _service = value; }
    }

    public string AppUserName { get; set; }

    public string ToolsPath { get; set; }

    public int Execute()
    {        
        var f = new KntRedmineForm(this);
        f.Show();
       
        return 0;
    }


}

