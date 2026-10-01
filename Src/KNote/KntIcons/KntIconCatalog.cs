using System.Drawing;

namespace KntIcons;

/// <summary>
/// A glyph of the embedded Fluent UI System Icons font (Regular) in its two design sizes, plus its color.
/// <see cref="Glyph16"/> is 0 when the font has no 16 px design for that icon (the 20 px one is used instead).
/// </summary>
internal readonly record struct KntIconGlyph(int Glyph16, int Glyph20, Color Color);

/// <summary>
/// Maps every <see cref="KntIcon"/> to its glyph. Codepoints come from FluentSystemIcons-Regular.json in
/// https://github.com/microsoft/fluentui-system-icons (fonts/); the trailing comment of each entry is the
/// Fluent icon name, so the "ic_fluent_{name}_16_regular" / "_20_regular" keys can be looked up again there.
/// </summary>
internal static class KntIconCatalog
{
    // Monochrome icons; color only carries meaning, and only for a few of them.
    private static readonly Color Neutral = Color.FromArgb(0x42, 0x42, 0x42);
    private static readonly Color Folder = Color.FromArgb(0xC2, 0x82, 0x00);
    private static readonly Color PostIt = Color.FromArgb(0xD8, 0xA8, 0x00);
    private static readonly Color Repository = Color.FromArgb(0x0F, 0x6C, 0xBD);
    private static readonly Color Alarm = Color.FromArgb(0xCA, 0x50, 0x10);
    private static readonly Color Success = Color.FromArgb(0x10, 0x7C, 0x10);
    private static readonly Color Danger = Color.FromArgb(0xC5, 0x0F, 0x1F);

    private static readonly Dictionary<KntIcon, KntIconGlyph> Glyphs = new()
    {
        // General
        [KntIcon.NewNote] = new(0xF56D, 0xF56E, Neutral),             // note_add
        [KntIcon.Edit] = new(0xF3DC, 0xF3DD, Neutral),                // edit
        [KntIcon.Delete] = new(0xE47B, 0xF34C, Danger),               // delete
        [KntIcon.Save] = new(0xEA43, 0xF67F, Neutral),                // save
        [KntIcon.Undo] = new(0xE126, 0xF199, Neutral),                // arrow_undo
        [KntIcon.Redo] = new(0xE0E4, 0xF16E, Neutral),                // arrow_redo
        [KntIcon.Print] = new(0xF6FA, 0xF62A, Neutral),               // print
        [KntIcon.Settings] = new(0xF6A8, 0xF6A9, Neutral),            // settings
        [KntIcon.Search] = new(0xEA7C, 0xF68F, Neutral),              // search
        [KntIcon.FilterClear] = new(0xE60C, 0xE60D, Neutral),         // filter_dismiss
        [KntIcon.Check] = new(0xE305, 0xF294, Neutral),               // checkmark
        [KntIcon.Tools] = new(0xEE85, 0xEE86, Neutral),               // wrench
        [KntIcon.Add] = new(0xF108, 0xF109, Neutral),                 // add
        [KntIcon.Remove] = new(0xEBCF, 0xEBD0, Neutral),              // subtract
        [KntIcon.More] = new(0xE823, 0xE824, Neutral),                // more_horizontal
        [KntIcon.Menu] = new(0xF0764, 0xF4E1, Neutral),               // line_horizontal_3

        // Folders and repositories
        [KntIcon.Folder] = new(0xE643, 0xF418, Folder),               // folder
        [KntIcon.FolderOpen] = new(0xF42D, 0xF42E, Folder),           // folder_open
        [KntIcon.Repository] = new(0xF0D7, 0xE466, Repository),       // database

        // Note editor
        [KntIcon.PostIt] = new(0xF663, 0xF56B, PostIt),               // note
        [KntIcon.ResizeGrip] = new(0xE2A4, 0xE2A5, Neutral),          // caret_down_right
        [KntIcon.Navigate] = new(0xE6B1, 0xF45A, Neutral),            // globe
        [KntIcon.BasicData] = new(0xEEED, 0xE557, Neutral),           // document_text
        [KntIcon.Attributes] = new(0xF01AD, 0xECE2, Neutral),         // text_bullet_list_square
        [KntIcon.Resources] = new(0xF1A8, 0xF1A9, Neutral),           // attach
        [KntIcon.Activities] = new(0, 0xEC92, Neutral),             // task_list_ltr
        [KntIcon.Alarm] = new(0xE36F, 0xF2E1, Alarm),                 // clock_alarm
        [KntIcon.Script] = new(0xF1DB, 0xF0239, Neutral),             // script
        [KntIcon.TraceNotes] = new(0xF0276, 0xF0277, Neutral),        // link_multiple
        [KntIcon.Html] = new(0xF339, 0xF2EF, Neutral),                // code
        [KntIcon.Markdown] = new(0xF0C2A, 0xE7DB, Neutral),           // markdown
        [KntIcon.UploadResource] = new(0xE131, 0xF1A4, Neutral),      // arrow_upload
        [KntIcon.PasteFromClipboard] = new(0xE35A, 0xF2D5, Neutral),  // clipboard_paste

        // Scripts and devices
        [KntIcon.NewDocument] = new(0xE4DA, 0xE4DB, Neutral),         // document_add
        [KntIcon.Run] = new(0xE990, 0xF605, Success),                 // play
        [KntIcon.Stop] = new(0xF729, 0xF72A, Danger),                 // stop

        // AI assistant
        [KntIcon.Send] = new(0xEA8E, 0xF699, Neutral),                // send
        [KntIcon.Restart] = new(0xE0B3, 0xF13F, Neutral),             // arrow_counterclockwise

        // Web view
        [KntIcon.Back] = new(0xF184, 0xF15B, Neutral),                // arrow_left
        [KntIcon.Forward] = new(0xE0EB, 0xF181, Neutral),             // arrow_right
        [KntIcon.Refresh] = new(0xE0AA, 0xF13D, Neutral),             // arrow_clockwise

        // Rich text editor
        [KntIcon.Cut] = new(0xF038C, 0xF33A, Neutral),                // cut
        [KntIcon.Copy] = new(0xF32A, 0xF32B, Neutral),                // copy
        [KntIcon.Bold] = new(0xECD2, 0xF7A4, Neutral),                // text_bold
        [KntIcon.Italic] = new(0xED36, 0xF7F4, Neutral),              // text_italic
        [KntIcon.Underline] = new(0xED67, 0xF80A, Neutral),           // text_underline
        [KntIcon.Font] = new(0xF7E3, 0xF7E4, Neutral),                // text_font
        [KntIcon.ClearFormatting] = new(0xECF3, 0xF7BC, Neutral),     // text_clear_formatting
        [KntIcon.FontColor] = new(0xECF5, 0xF7BF, Neutral),           // text_color
        [KntIcon.FontIncrease] = new(0, 0xF439, Neutral),           // font_increase
        [KntIcon.FontDecrease] = new(0, 0xF437, Neutral),           // font_decrease
        [KntIcon.AlignLeft] = new(0xECC3, 0xF79F, Neutral),           // text_align_left
        [KntIcon.AlignCenter] = new(0xECB2, 0xF799, Neutral),         // text_align_center
        [KntIcon.AlignRight] = new(0xECCA, 0xF7A1, Neutral),          // text_align_right
        [KntIcon.IndentIncrease] = new(0xF029A, 0xF029B, Neutral),    // text_indent_increase
        [KntIcon.IndentDecrease] = new(0xF0297, 0xF0298, Neutral),    // text_indent_decrease
        [KntIcon.NumberedList] = new(0xED3A, 0xF7F9, Neutral),        // text_number_list_ltr
        [KntIcon.BulletedList] = new(0xECD8, 0xECD9, Neutral),        // text_bullet_list_ltr
        [KntIcon.HorizontalLine] = new(0xF02B7, 0xF4E0, Neutral),     // line_horizontal_1
        [KntIcon.Table] = new(0xEBF4, 0xF75D, Neutral),               // table
        [KntIcon.Image] = new(0xF487, 0xF488, Neutral),               // image
        [KntIcon.Link] = new(0xF4E3, 0xF4E4, Neutral),                // link
    };

    public static KntIconGlyph Get(KntIcon icon) =>
        Glyphs.TryGetValue(icon, out var glyph)
            ? glyph
            : throw new ArgumentOutOfRangeException(nameof(icon), icon, "Icon not mapped in KntIconCatalog.");
}
