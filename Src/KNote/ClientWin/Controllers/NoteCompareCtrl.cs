using System.Net;
using System.Text;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Controllers;

/// <summary>
/// «Compare» (the history, H12, H22), in a window of its own: what a change did (the note before that row and after it) or, with
/// «Compare with the current note», that version against the note now; the only one possible when the copy before the change is
/// gone (the Service says so). Shown as HTML in a web view (Html: every text escaped, no scripts). Included in the note editor (no
/// [KntAuthorize]): the command checks the role (Guest).
/// </summary>
public class NoteCompareCtrl : CtrlViewBase<IViewNoteCompare>
{
    #region Constructor

    public NoteCompareCtrl(Store store, IKntService service, Guid noteId, KLogDto row, string noteTitle) : base(store)
    {
        ControllerName = "Compare note versions";
        Service = service;
        NoteId = noteId;
        Row = row;
        NoteTitle = noteTitle;
    }

    #endregion

    #region Properties

    public IKntService Service { get; }
    public Guid NoteId { get; }
    public KLogDto Row { get; }
    public string NoteTitle { get; }

    public NoteComparisonDto Comparison { get; private set; }
    public string Error { get; private set; }

    public string Title => string.IsNullOrEmpty(NoteTitle) ? "Compare versions of the note" : $"Compare versions of {NoteTitle}";

    public string Subtitle => $"The change of {HistoryRules.WhenText(Row)}"
        + (string.IsNullOrEmpty(Row?.UserName) ? "" : $" by {Row.UserName}");

    #endregion

    #region Controller implementation

    protected override IViewNoteCompare CreateView() =>
        Store.FactoryViews.Registry.Resolve<NoteCompareCtrl, IViewNoteCompare>(this);

    #endregion

    #region Public methods

    /// <summary>Reads the comparison; false (with its error in Error) if it can't be read.</summary>
    public async Task<bool> LoadAsync(bool withCurrentNote)
    {
        Error = null;
        var result = await Service.History.GetNoteComparisonAsync(NoteId, Row.KLogId, withCurrentNote);
        if (!result.IsValid)
        {
            Error = result.ErrorMessage;
            return false;
        }
        Comparison = result.Entity;
        return true;
    }

    /// <summary>The comparison as HTML for the web view: the notice, the fields that differ and the text by lines.</summary>
    public string Html() => Html(Comparison);

    public static string Html(NoteComparisonDto comparison)
    {
        if (comparison == null)
            return "";

        string E(string text) => WebUtility.HtmlEncode(text ?? "");
        var html = new StringBuilder();
        html.Append(Styles);

        if (!string.IsNullOrEmpty(comparison.Notice))
            html.Append("<p class=\"knt-cmp-notice\">").Append(E(comparison.Notice)).Append("</p>");
        if (comparison.Same)
            html.Append("<p><i>No differences.</i></p>");

        if (comparison.Fields.Count > 0)
        {
            html.Append("<table class=\"knt-cmp-fields\"><thead><tr><th></th><th>").Append(E(comparison.BeforeLabel))
                .Append("</th><th>").Append(E(comparison.AfterLabel)).Append("</th></tr></thead><tbody>");
            foreach (var field in comparison.Fields)
                html.Append("<tr><th>").Append(E(field.Name)).Append("</th><td class=\"knt-cmp-before\">")
                    .Append(field.Before == null ? "—" : E(field.Before)).Append("</td><td class=\"knt-cmp-after\">")
                    .Append(field.After == null ? "—" : E(field.After)).Append("</td></tr>");
            html.Append("</tbody></table>");
        }

        var text = comparison.Text;
        if (text != null && !text.Same)
        {
            var counts = new List<string>();
            if (text.Added > 0) counts.Add($"+{text.Added}");
            if (text.Removed > 0) counts.Add($"−{text.Removed}");
            var lines = text.Added + text.Removed == 1 ? "line" : "lines";
            html.Append("<p class=\"knt-cmp-text-header\"><b>Text: ").Append(text.Replaced ? "replaced " : "")
                .Append(E($"({string.Join(" ", counts)} {lines})")).Append("</b> <span class=\"knt-cmp-legend\">− ")
                .Append(E(comparison.BeforeLabel)).Append(" · + ").Append(E(comparison.AfterLabel)).Append("</span></p>");

            html.Append("<table class=\"knt-cmp-text\">");
            foreach (var line in text.Lines)
            {
                if (line.Kind == EnumComparisonLine.Folded)
                {
                    html.Append("<tr class=\"knt-cmp-folded\"><td></td><td></td><td></td><td>")
                        .Append(line.Skipped == 1 ? "… 1 unchanged line" : $"… {line.Skipped} unchanged lines").Append("</td></tr>");
                    continue;
                }
                var (css, sign) = line.Kind switch
                {
                    EnumComparisonLine.Removed => ("knt-cmp-removed", "−"),
                    EnumComparisonLine.Added => ("knt-cmp-added", "+"),
                    _ => ("knt-cmp-equal", "")
                };
                html.Append("<tr class=\"").Append(css).Append("\"><td class=\"knt-cmp-num\">")
                    .Append(line.BeforeNumber > 0 ? line.BeforeNumber.ToString() : "").Append("</td><td class=\"knt-cmp-num\">")
                    .Append(line.AfterNumber > 0 ? line.AfterNumber.ToString() : "").Append("</td><td class=\"knt-cmp-sign\">")
                    .Append(sign).Append("</td><td class=\"knt-cmp-code\">");
                if (line.Segments == null)
                    html.Append(E(line.Text));
                else
                    foreach (var segment in line.Segments)
                        html.Append(segment.Changed ? "<mark>" + E(segment.Text) + "</mark>" : E(segment.Text));
                html.Append("</td></tr>");
            }
            if (text.Truncated)
                html.Append("<tr class=\"knt-cmp-folded\"><td></td><td></td><td></td><td>… too many lines: the rest are not shown.</td></tr>");
            html.Append("</table>");
        }

        return html.ToString();
    }

    #endregion

    // Translucent colours: they read on a light or a dark page (KNoteWebViewStyle sets the colours of the text).
    private const string Styles = """
        <style>
        .knt-cmp-notice { padding: 6px 8px; background: rgba(255, 193, 7, 0.18); border-radius: 3px; }
        .knt-cmp-fields { border-collapse: collapse; width: 100%; margin-bottom: 12px; }
        .knt-cmp-fields th, .knt-cmp-fields td { padding: 3px 8px; text-align: left; vertical-align: top; border-bottom: 1px solid rgba(128, 128, 128, 0.3); white-space: pre-wrap; overflow-wrap: anywhere; }
        .knt-cmp-fields tbody th { width: 10em; font-weight: 500; opacity: 0.8; }
        .knt-cmp-before { background: rgba(220, 53, 69, 0.12); }
        .knt-cmp-after { background: rgba(40, 167, 69, 0.12); }
        .knt-cmp-legend { opacity: 0.7; font-weight: normal; }
        .knt-cmp-text { border-collapse: collapse; width: 100%; font-family: Consolas, 'Cascadia Mono', monospace; font-size: 0.9em; }
        .knt-cmp-text td { padding: 0 4px; vertical-align: top; }
        .knt-cmp-num { width: 3em; text-align: right; opacity: 0.55; user-select: none; }
        .knt-cmp-sign { width: 1em; text-align: center; user-select: none; }
        .knt-cmp-code { white-space: pre-wrap; overflow-wrap: anywhere; }
        .knt-cmp-removed { background: rgba(220, 53, 69, 0.14); }
        .knt-cmp-added { background: rgba(40, 167, 69, 0.14); }
        .knt-cmp-removed mark { background: rgba(220, 53, 69, 0.4); color: inherit; }
        .knt-cmp-added mark { background: rgba(40, 167, 69, 0.4); color: inherit; }
        .knt-cmp-folded td { opacity: 0.65; font-style: italic; background: rgba(128, 128, 128, 0.08); }
        </style>
        """;
}
