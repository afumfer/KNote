using KNote.ClientWin.Controllers;
using KNote.Model;
using KNote.Service.Core;

namespace KNote.ClientWin.Core;

// A click on a citation (#123, a link of NoteCitationLinker) in the views of ClientWin: the WebView2 is kept where it is and
// the note opens in the note editor. Like KNoteAiToolsHost, a Core -> Controllers reference, to launch a Ctrl and its view.
public static class NoteCitations
{
    // For KntEditView.LinkHandler: true if the link is a citation. The note opens out of the WebView2 callback: it may
    // show a message box (see the WebView2 notes of ClientWin/CLAUDE.md).
    public static bool TryOpen(string url, Control view, Store store, IKntService service, Action<string> showMessage)
    {
        if (!NoteCitationLinker.TryGetDesktopNumber(url, out var noteNumber))
            return false;

        view.BeginInvoke(async () => await OpenAsync(store, service, noteNumber, showMessage));
        return true;
    }

    public static async Task OpenAsync(Store store, IKntService service, int noteNumber, Action<string> showMessage)
    {
        try
        {
            var note = await service.Notes.GetCitedNoteAsync(noteNumber);
            if (!note.IsValid)
                showMessage(note.ErrorMessage);
            else if (note.Entity == null)
                showMessage($"Note #{noteNumber} not found.");
            else
            {
                var noteEditor = new NoteEditorCtrl(store);
                if (await noteEditor.LoadModelById(service, note.Entity.NoteId, false))
                    noteEditor.Run();
                else
                    noteEditor.Dispose();
            }
        }
        catch (Exception ex)
        {
            showMessage(ex.Message);
        }
    }
}
