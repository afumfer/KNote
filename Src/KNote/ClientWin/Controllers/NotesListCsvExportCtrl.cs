using System.Diagnostics;
using KNote.ClientWin.Core;
using KNote.ClientWin.Core.Reports;
using KNote.Model;
using KNote.Service.Core;
using Microsoft.Extensions.Logging;

namespace KNote.ClientWin.Controllers;

// Exports the notes list as the user is seeing it to a CSV file (same content as the printed notes list),
// named after the folder or search/filter it comes from. Exporting what a repository holds requires the
// Staff role there, like printing it.
[KntAuthorize(EnumRoles.Staff)]
public class NotesListCsvExportCtrl : CtrlViewBase<IViewFileExport>
{
    #region Properties

    // The repository the notes come from: the export is authorized against the user's role there.
    public IKntService Service { get; set; }

    // The list as displayed (see INotesListSnapshotProvider).
    public NotesListSnapshot Snapshot { get; set; }

    // Where the listed notes come from (see KNoteManagementCtrl.GetNotesListContextAsync), for the file name.
    public Func<string, Task<NotesListContext>> GetContextAsync { get; set; }

    // Opens the exported file with the application Windows associates with CSV files.
    public Action<string> OpenFile { get; set; } = path => Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });

    protected override IKntService AuthorizationResource => Service;

    #endregion

    #region Constructor

    public NotesListCsvExportCtrl(Store store) : base(store)
    {
        ControllerName = "Export notes list to CSV";
    }

    #endregion

    #region Controller methods

    protected override IViewFileExport CreateView()
        => Store.FactoryViews.Registry.Resolve<NotesListCsvExportCtrl, IViewFileExport>(this);

    // The whole use case, without a window of its own (so not through Run()): checks the user's role, asks
    // for the file, writes it and offers to open it. False when nothing was exported.
    public async Task<bool> ExportAsync()
    {
        if (!CheckAccess())
            return false;

        try
        {
            if (Snapshot == null || Snapshot.Rows.Count == 0)
            {
                View.ShowInfo("There are no notes in the list to export.");
                return false;
            }

            var context = await GetContextAsync(Snapshot.TextFilter);
            var fileName = ReportFileName.Sanitize(context.FileNameBase(DateTime.Now)) + ".csv";

            var path = View.PromptForSaveFile("Export notes list to CSV", "CSV file (*.csv)|*.csv",
                ReportFileName.InitialFolder(Store.State.Reports.LastExportFolder), fileName);
            if (string.IsNullOrEmpty(path))
                return false;

            using (new WaitCursor())
                await File.WriteAllTextAsync(path, NotesListCsv.Build(Snapshot), NotesListCsv.FileEncoding);

            Store.State.Reports.LastExportFolder = Path.GetDirectoryName(path);
            NotifyMessage($"Notes list exported to CSV: {path}");

            if (View.ShowInfo($"Notes list exported to CSV ({Snapshot.Rows.Count} notes):\r\n{path}\r\n\r\nDo you want to open it now?", KntConst.AppName,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                OpenExportedFile(path);

            return true;
        }
        catch (Exception ex)
        {
            Store.Logger?.LogError(ex, "ExportNotesListToCsv: {message}", ex.Message);
            View.ShowInfo($"The notes list could not be exported: {ex.Message}");
            return false;
        }
        finally
        {
            Finalize();
        }
    }

    private void OpenExportedFile(string path)
    {
        try
        {
            OpenFile(path);
        }
        catch (Exception ex)
        {
            Store.Logger?.LogError(ex, "OpenFile {path}: {message}", path, ex.Message);
            View.ShowInfo($"The file could not be opened: {ex.Message}");
        }
    }

    #endregion
}
