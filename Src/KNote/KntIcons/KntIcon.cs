namespace KntIcons;

/// <summary>
/// The icons available to the KNote UI, named by what they mean in the app (not by their glyph), so a view
/// asks for <see cref="Repository"/> and the catalog decides how a repository looks.
/// </summary>
public enum KntIcon
{
    // General
    NewNote,
    Edit,
    Delete,
    Save,
    Undo,
    Redo,
    Print,
    Settings,
    Search,
    FilterClear,
    Check,
    Tools,
    Add,
    Remove,
    More,
    Menu,

    // Folders and repositories
    Folder,
    FolderOpen,
    Repository,

    // Note editor
    PostIt,
    Navigate,
    BasicData,
    Attributes,
    Resources,
    Activities,
    Alarm,
    Script,
    TraceNotes,
    Html,
    Markdown,
    UploadResource,
    PasteFromClipboard,

    // Scripts and devices
    NewDocument,
    Run,
    Stop,

    // Web view
    Back,
    Forward,
    Refresh,

    // Rich text editor
    Cut,
    Copy,
    Bold,
    Italic,
    Underline,
    Font,
    ClearFormatting,
    FontColor,
    FontIncrease,
    FontDecrease,
    AlignLeft,
    AlignCenter,
    AlignRight,
    IndentIncrease,
    IndentDecrease,
    NumberedList,
    BulletedList,
    HorizontalLine,
    Table,
    Image,
    Link
}
