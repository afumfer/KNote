using KNote.ClientWin.Core;
using KNote.ClientWin.Utils;
using KNote.Model;

namespace KNote.ClientWin.Views;

// Base class for KNote's forms: holds behavior that is identical in every view instead of repeating
// it in each one.
//
// Keep it designer-safe: the Visual Studio WinForms designer instantiates this class when it opens a
// derived form, so it must stay in this same project, have no constructor logic, implement no
// interfaces (each derived form still declares its own IView* contract - a public method here
// satisfies the interface member by inheritance) and reference no Store/controller.
public class KntForm : Form
{
    // True once the controller has asked the view to close (OnClosingView), as opposed to the user
    // closing the window itself: closing handlers use it to tell the two apart.
    protected bool ViewFinalized { get; set; }

    // IViewBase / IViewEmbeddable plumbing common to every view. Virtual for the views that do more
    // (or something else) than the default.
    public virtual void ShowView()
    {
        Show();
    }

    public virtual Result<EControllerResult> ShowModalView()
    {
        return ShowDialog().ToControllerResult();
    }

    public virtual void OnClosingView()
    {
        ViewFinalized = true;
        Close();
    }

    public virtual void ConfigureEmbededMode()
    {
    }

    public virtual void ConfigureWindowMode()
    {
    }

    // Virtual for the few views with special needs (e.g. MonitorForm marshals to the UI thread).
    public virtual DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        return KntMessageBox.Show(info, caption, buttons, icon);
    }

    // A modal form is a normal (non-topmost) window, so a PostIt marked "always visible" would cover
    // it and the app would look frozen (same problem KntMessageBox solves for message boxes).
    // Checked here, right before the dialog is shown, so no ShowDialog call site has to change.
    protected override void OnLoad(EventArgs e)
    {
        if (!DesignMode && Modal && TopMostWindows.AnyVisible())
        {
            TopMost = true;
            TopMostWindows.ClearNativeOwner(Handle);
        }

        base.OnLoad(e);
    }
}
