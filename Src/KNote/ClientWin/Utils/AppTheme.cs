using KNote.Model;

namespace KNote.ClientWin.Utils;

/// <summary>
/// Light/dark color mode of the app. WinForms themes its controls itself (Application.SetColorMode), but
/// only for the windows created after the mode is set: switching it with windows already open leaves them
/// half painted in each mode. So the mode is applied once, at startup (Program.Main, right after the
/// configuration is loaded and before the main window exists), and changing it takes a restart.
/// </summary>
public static class AppTheme
{
    public static void Apply(AppColorMode mode)
    {
        Application.SetColorMode(ToSystemColorMode(mode));

        // Menus and toolbars (those with the default ManagerRenderMode) use it in any color mode.
        ToolStripManager.Renderer = new KntToolStripRenderer();
    }

    // True when the app is running in dark mode (chosen, or followed from Windows with AppColorMode.System).
    public static bool IsDark => Application.IsDarkModeEnabled;

    // Links over a dark background: a LinkLabel's default blue (#0000FF) is barely readable there. Same
    // color as the links of the Markdown viewer's dark style sheet (KNoteWebViewStyle.css).
    internal static readonly Color DarkLinkColor = Color.FromArgb(0x4D, 0xAA, 0xFC);

    // Fixes what WinForms' dark mode leaves light, all of them designer defaults: a TabPage with
    // UseVisualStyleBackColor (the light visual style background around its content), the Fixed3D border
    // (the default of TextBox, ListBox, ListView, TreeView...), which becomes the thin gray FixedSingle one,
    // and the LinkLabel's dark blue. Called by KntForm on load, so it only reaches the controls that exist by
    // then. Safe to call again on the same controls: what it has already changed no longer matches.
    public static void AdjustControlsForDarkMode(Control root)
    {
        if (IsDark)
            AdjustControls(root);
    }

    internal static void AdjustControls(Control root)
    {
        foreach (Control control in root.Controls)
        {
            if (control is TabPage tabPage)
                tabPage.UseVisualStyleBackColor = false;

            if (control is LinkLabel linkLabel)
            {
                linkLabel.LinkColor = DarkLinkColor;
                linkLabel.ActiveLinkColor = DarkLinkColor;
                linkLabel.VisitedLinkColor = DarkLinkColor;
            }

            switch (control)
            {
                case TextBoxBase textBox when textBox.BorderStyle == BorderStyle.Fixed3D:
                    textBox.BorderStyle = BorderStyle.FixedSingle;
                    if (!textBox.Multiline && textBox.AutoSize)
                        KeepSingleLineTextCentered(textBox);
                    break;
                case ListBox listBox when listBox.BorderStyle == BorderStyle.Fixed3D:
                    listBox.BorderStyle = BorderStyle.FixedSingle;
                    break;
                case ListView listView when listView.BorderStyle == BorderStyle.Fixed3D:
                    listView.BorderStyle = BorderStyle.FixedSingle;
                    break;
                case TreeView treeView when treeView.BorderStyle == BorderStyle.Fixed3D:
                    treeView.BorderStyle = BorderStyle.FixedSingle;
                    break;
                case UpDownBase upDown when upDown.BorderStyle == BorderStyle.Fixed3D:
                    upDown.BorderStyle = BorderStyle.FixedSingle;
                    break;
                case DataGridView grid when grid.BorderStyle == BorderStyle.Fixed3D:
                    grid.BorderStyle = BorderStyle.FixedSingle;
                    break;
                case Panel panel when panel.BorderStyle == BorderStyle.Fixed3D:
                    panel.BorderStyle = BorderStyle.FixedSingle;
                    break;
                case UserControl userControl when userControl.BorderStyle == BorderStyle.Fixed3D:
                    userControl.BorderStyle = BorderStyle.FixedSingle;
                    break;
                case SplitContainer split when split.BorderStyle == BorderStyle.Fixed3D:
                    split.BorderStyle = BorderStyle.FixedSingle;
                    break;
                case Label label when label.BorderStyle == BorderStyle.Fixed3D:
                    label.BorderStyle = BorderStyle.FixedSingle;
                    break;
                case PictureBox pictureBox when pictureBox.BorderStyle == BorderStyle.Fixed3D:
                    pictureBox.BorderStyle = BorderStyle.FixedSingle;
                    break;
            }

            AdjustControls(control);
        }
    }

    // A single-line text box can't center its text vertically: the text sits right under the top border.
    // FixedSingle's border is 1 px per side instead of Fixed3D's 2, but the automatic height is the same, so
    // the text ends up 1 px too high with 2 spare pixels below it. Shrinking the box by 1 px on each side
    // centers the text again and keeps the box centered on its row (labels, combos and buttons beside it).
    // From PreferredHeight, not Height: until its handle exists (e.g. in a hidden panel) the box still has the
    // Designer's height, and 2 px less than that is too short for its font: once focused, the text line is
    // painted over the top border.
    private static void KeepSingleLineTextCentered(TextBoxBase textBox)
    {
        textBox.AutoSize = false;
        textBox.SetBounds(textBox.Left, textBox.Top + 1, textBox.Width, textBox.PreferredHeight - 2);
    }

    internal static SystemColorMode ToSystemColorMode(AppColorMode mode) => mode switch
    {
        AppColorMode.Dark => SystemColorMode.Dark,
        AppColorMode.System => SystemColorMode.System,
        _ => SystemColorMode.Classic
    };
}
