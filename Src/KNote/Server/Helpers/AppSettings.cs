using KNote.Repository.EntityFramework.Entities;

namespace KNote.Server.Helpers;

public class AppSettings
{
    public string Secret { get; set; }
    public bool ActivateMessageBroker { get; set; }
    public bool MountResourceContainerOnStartup { get; set; }
}
