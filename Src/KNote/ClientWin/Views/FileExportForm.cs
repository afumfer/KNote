using KNote.ClientWin.Core;

namespace KNote.ClientWin.Views;

/// <summary>
/// View of the file export use cases (e.g. NotesListCsvExportCtrl): they have no window of their own, only
/// the save file dialog and messages, so this form is never shown.
/// </summary>
[System.ComponentModel.DesignerCategory("Code")]
public class FileExportForm : KntForm, IViewFileExport
{
    public string PromptForSaveFile(string title, string filter, string initialDirectory, string fileName)
    {
        using var dialog = new SaveFileDialog
        {
            Title = title,
            Filter = filter,
            AddExtension = true,
            OverwritePrompt = true,
            InitialDirectory = initialDirectory,
            FileName = fileName
        };

        return dialog.ShowDialog() == DialogResult.OK ? dialog.FileName : null;
    }
}
