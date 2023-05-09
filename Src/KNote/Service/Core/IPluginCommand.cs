namespace KNote.Service.Core;

public interface IPluginCommand
{
    string Name { get; }
    string Description { get; }

    IKntService Service { get; set; }

    public int Execute();

}
