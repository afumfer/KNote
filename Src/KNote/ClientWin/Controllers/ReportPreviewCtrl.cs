using KNote.ClientWin.Core;
using KNote.ClientWin.Core.Reports;
using Microsoft.Extensions.Logging;

namespace KNote.ClientWin.Controllers;

// Shows a printable report (ReportDocument) in a preview window from which the user prints it or saves
// it as PDF. The report itself is built by whoever launches this use case (notes list, note details...);
// printing and PDF generation are done by the view's browser engine (WebView2).
public class ReportPreviewCtrl : CtrlViewBase<IViewBase>
{
    #region Properties

    public ReportDocument Report { get; private set; }

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

    public void LoadReport(ReportDocument report, string resourcesRootPath = null)
    {
        Report = report ?? throw new ArgumentNullException(nameof(report));
        Html = ReportHtml.Render(report);
        ResourcesRootPath = resourcesRootPath;
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
