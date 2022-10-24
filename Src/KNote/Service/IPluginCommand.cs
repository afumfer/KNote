using KNote.Model;
using KNote.Service.Services;

namespace KNote.Service;

public interface IPluginCommand
{
    string Name { get; }
    string Description { get; }

    string AppUserName { get; set; }

    string ToolsPath { get; set; }

    IKntService Service { get; set; }

    public int Execute();

    //public void InjectService(IKntService? service);    

}
