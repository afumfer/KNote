namespace KNote.Client.Shared;

/// <summary>A section of the app: menu or tab text, route (relative to the base) and Material Symbols icon.</summary>
public record KntSection(string Text, string Path, string Icon);

/// <summary>
/// The app sections, shared by the side menu (MainLayoutNavMenu), the admin panel tabs and the header of each
/// section page (KntIndexHeader), so a section shows the same icon everywhere.
/// </summary>
public static class KntSections
{
    public static readonly KntSection Home = new("Home", "index", "home");
    public static readonly KntSection QuickSearch = new("Quick search", "notes/search", "search");
    public static readonly KntSection TreePanel = new("Tree panel", "notes/tree", "topic");
    public static readonly KntSection NotesFilter = new("Notes filter", "notes/filter", "filter_alt");
    public static readonly KntSection TasksCalendar = new("Tasks calendar", "notes/taskscalendar", "calendar_today");
    public static readonly KntSection Alarms = new("Alarms", "notes/alarmscalendar", "alarm");
    public static readonly KntSection ChatRoom = new("Chat room", "chat", "chat_bubble");
    public static readonly KntSection AiAssistant = new("AI Assistant", "aiassistant", "smart_toy");
    public static readonly KntSection Admin = new("Admin panel", "admin", "admin_panel_settings");

    // Admin panel tabs
    public static readonly KntSection Users = new("Users management", "users", "person");
    public static readonly KntSection Folders = new("Folders management", "folders", "folder");
    public static readonly KntSection NoteTypes = new("Notes types", "types", "edit_attributes");
    public static readonly KntSection Attributes = new("Attributes management", "attributes", "format_list_bulleted");
}
