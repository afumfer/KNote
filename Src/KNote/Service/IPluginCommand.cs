using KNote.Model;
using KNote.Service.Services;

namespace KNote.Service;

public interface IPluginCommand
{
    string Name { get; }
    string Description { get; }
    
    IKntService? Service { get; }

    public int Execute();

    public void InjectService(IKntService? service);    

}
