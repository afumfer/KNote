namespace KNote.Server.Helpers;

public class AppSettings
{
    public string Secret { get; set; }
    public string ResourcesContainer { get; set; }
    public string ResourcesContainerRootPath { get; set; }
    public string ResourcesContainerRootUrl { get; set; }
    public bool ResourcesContentInDB { get; set; }
    public bool ActivateMessageBroker { get; set; }
}
