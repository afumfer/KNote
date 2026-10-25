using KNote.Repository.EntityFramework.Entities;

namespace KNote.Server.Helpers;

public class AppSettings
{
    public string Secret { get; set; }
    public bool ActivateMessageBroker { get; set; }
    public bool MountResourceContainerOnStartup { get; set; }
    // HSTS and the redirection to HTTPS (off in KNote Local, which only listens on http://127.0.0.1).
    public bool UseHttpsRedirection { get; set; } = true;
}
