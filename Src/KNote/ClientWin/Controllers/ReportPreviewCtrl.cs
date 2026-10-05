using KNote.ClientWin.Core;
using KNote.ClientWin.Core.Reports;
using KNote.Model;
using KNote.Service.Core;
using Microsoft.Extensions.Logging;

namespace KNote.ClientWin.Controllers;

// Shows a printable report (ReportDocument) in a preview window from which the user prints it or saves
// it as PDF. The report itself is built by whoever launches this use case (notes list, note details...);
// printing and PDF generation are done by the view's browser engine (WebView2).
[KntAuthorize(EnumRoles.Staff)]
public class ReportPreviewCtrl : CtrlViewBase<IViewBase>
{
    #region Properties

    public ReportDocument Report { get; private set; }

    // The repository the report comes from: printing it is authorized against the user's role there.
    public IKntService Service { get; set; }

    protected override IKntService AuthorizationResource => Service;

    public string Html { get; private set; }

    // Local folder served as KntConst.VirtualHostNameToFolderMapping, so the report can show the
    // repository's resources (e.g. images embedded in a note description). Optional.
    public string ResourcesRootPath { get; set; }

    public string SuggestedPdfFileName
        => ReportFileName.Sanitize(Report?.FileNameBase ?? Report?.Title) + ".pdf";

    public string LastExportFolder
    {
        get { return ReportFileName.InitialFolder(Store.State.Reports.LastExportFolder); }
        set { Store.State.Reports.LastExportFolder = value; }
    }

    #endregion

    #region Constructor

    public ReportPreviewCtrl(Store store) : base(store)
    {
        ControllerName = "Report preview";
    }

    #endregion

    #region Controller methods

    protected override IViewBase CreateView()
        => Store.FactoryViews.Registry.Resolve<ReportPreviewCtrl, IViewBase>(this);

    // Opens a new preview window with the report built by buildReport: the entry point of every print use
    // case. Printing what a repository holds requires the Staff role there (see the class attribute), so
    // that is checked first, before building a report that can take a while (e.g. a notes book). Refused,
    // the user is told and the result is Canceled (no error left to report by the caller).
    public static async Task<Result<EControllerResult>> ShowAsync(Store store, IKntService service,
        Func<Task<ReportDocument>> buildReport, string resourcesRootPath = null)
    {
        var reportPreviewCtrl = new ReportPreviewCtrl(store) { Service = service };
        if (!reportPreviewCtrl.CheckAccess())
            return new Result<EControllerResult>(EControllerResult.Canceled);

        ReportDocument report;
        try
        {
            report = await buildReport();
        }
        catch
        {
            reportPreviewCtrl.Finalize();
            throw;
        }

        reportPreviewCtrl.LoadReport(report, resourcesRootPath);
        return reportPreviewCtrl.Run();
    }

    public void LoadReport(ReportDocument report, string resourcesRootPath = null)
    {
        Report = report ?? throw new ArgumentNullException(nameof(report));
        Html = ReportHtml.Render(report);
        ResourcesRootPath = resourcesRootPath;
    }

    // Second pass of a report with page references (ReportDocument.ResolvePageReferences): the view has
    // printed it once and read where each target landed (see PdfNamedDestinations).
    public void ApplyPageNumbers(IReadOnlyDictionary<string, int> pages)
    {
        Html = ReportHtml.FillPageReferences(Html, pages);
        if (pages == null || pages.Count == 0)
            Store.Logger?.LogWarning("Report preview - page numbers could not be resolved for \"{title}\".", Report?.Title);
    }

    public void PdfSaved(string pdfPath)
    {
        LastExportFolder = Path.GetDirectoryName(pdfPath);
        NotifyMessage($"Report saved as PDF: {pdfPath}");
    }

    public void ReportError(string action, Exception ex)
    {
        Store.Logger?.LogError(ex, "Report preview - {action}", action);
        View.ShowInfo($"{action}: {ex.Message}", "KNote - Report", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    #endregion
}
