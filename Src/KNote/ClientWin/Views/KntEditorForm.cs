using KNote.Model;
using KNote.ClientWin.Utils;

namespace KNote.ClientWin.Views;

// Base class for the modal editors that edit one entity with Accept/Cancel buttons: keeps track of
// unsaved changes and asks before discarding them. The buttons themselves are wired in each editor's
// Designer (buttonAccept_Click / buttonCancel_Click just call AcceptEditionAsync / TryCancelEdition).
//
// Same designer-safety rules as KntForm: no constructor logic, no interfaces, no controller reference.
public class KntEditorForm : KntForm
{
    // Set by typing in the form itself: it only sees keys when KeyPreview is true (set in each
    // editor's Designer, not here) or when no child control has the focus.
    protected bool FormIsDirty { get; set; }

    protected override void OnKeyPress(KeyPressEventArgs e)
    {
        FormIsDirty = true;
        base.OnKeyPress(e);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            FormIsDirty = true;

        base.OnKeyUp(e);
    }

    // Hooks each editor overrides to reach its controller.
    protected virtual Task<bool> SaveModelAsync()
    {
        return Task.FromResult(true);
    }

    protected virtual void CancelEdition()
    {
    }

    protected async Task AcceptEditionAsync()
    {
        var saved = await SaveModelAsync();
        if (saved)
        {
            FormIsDirty = false;
            DialogResult = DialogResult.OK;
        }
    }

    // Returns false (and does nothing) when there are unsaved changes and the user chooses to keep
    // editing; otherwise cancels the edition.
    protected bool TryCancelEdition()
    {
        if (FormIsDirty)
        {
            if (KntMessageBox.Show("You have modified this entity, are you sure you want to exit without recording?", KntConst.AppName, MessageBoxButtons.YesNo) == DialogResult.No)
                return false;
        }

        DialogResult = DialogResult.Cancel;
        CancelEdition();
        return true;
    }

    protected override void OnUserClosing(FormClosingEventArgs e)
    {
        if (!TryCancelEdition())
            e.Cancel = true;
    }
}
