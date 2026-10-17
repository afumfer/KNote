using System.Globalization;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.Logging;

namespace KNote.Reports;

// The web app's reports (Server's api/reports): its notes lists (a folder, a quick search or a filter, see
// NotesListReportRequestDto) printed as a list or as a book or exported as CSV, and a note's details. The same
// generators as ClientWin, with the list's columns as the web shows them (NotesGridColumns) and the resources
// loaded from the web server (WebReportResources). Dates and numbers are written with the current culture.
public class WebReports
{
    private readonly IKntService _service;
    private readonly IReportResources _resources;
    private readonly ILogger _logger;

    public WebReports(IKntService service, ILogger logger = null)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _resources = new WebReportResources(service);
        _logger = logger;
    }

    public async Task<Result<ReportDto>> NotesListAsync(NotesListReportRequestDto request)
    {
        var notes = await LoadNotesAsync(request);
        if (!notes.IsValid)
            return Failed<ReportDto>(notes);

        var snapshot = NotesGridColumns.Snapshot(notes.Entity, request.Compact);
        var context = await GetContextAsync(request);
        var report = NotesListReport.Build(snapshot, context, DateTime.Now);

        return Report(report, snapshot.Rows.Count);
    }

    // One chapter per note: it reads every note of the list (the user is warned of a large one, see
    // LargeNotesBook).
    public async Task<Result<ReportDto>> NotesBookAsync(NotesListReportRequestDto request, CancellationToken cancellationToken = default)
    {
        var notes = await LoadNotesAsync(request);
        if (!notes.IsValid)
            return Failed<ReportDto>(notes);

        var context = await GetContextAsync(request);
        var chapters = await NotesBook.LoadChaptersAsync(_service, _resources, notes.Entity.Select(n => n.NoteId), cancellationToken);
        // A browser can't tell the pages the chapters land on: the contents keep their links, without numbers.
        var report = NotesBook.Build(context, chapters, DateTime.Now, CultureInfo.CurrentCulture.TwoLetterISOLanguageName, pageNumbers: false);

        return Report(report, chapters.Count);
    }

    // The list as printed (same columns and cell texts), in a file for spreadsheets (see NotesListCsv).
    public async Task<Result<ReportFileDto>> NotesListCsvAsync(NotesListReportRequestDto request)
    {
        var notes = await LoadNotesAsync(request);
        if (!notes.IsValid)
            return Failed<ReportFileDto>(notes);

        var snapshot = NotesGridColumns.Snapshot(notes.Entity, request.Compact);
        var context = await GetContextAsync(request);

        return new Result<ReportFileDto>(new ReportFileDto
        {
            FileName = ReportFileName.Sanitize(context.FileNameBase(DateTime.Now)) + ".csv",
            ContentType = "text/csv",
            // The BOM, so that Excel reads it as UTF-8 (see NotesListCsv.FileEncoding).
            Content = "﻿" + NotesListCsv.Build(snapshot)
        });
    }

    // Everything about a note, as saved.
    public async Task<Result<ReportDto>> NoteDetailAsync(Guid noteId)
    {
        var exists = await _service.Notes.GetAsync(noteId);
        if (!exists.IsValid)
            return Failed<ReportDto>(exists);
        if (exists.Entity == null)
        {
            var notFound = new Result<ReportDto>();
            notFound.AddErrorMessage("The note doesn't exist.");
            return notFound;
        }

        var note = await _service.Notes.GetExtendedAsync(noteId);
        if (!note.IsValid)
            return Failed<ReportDto>(note);

        var data = await NoteDetailReportData.CreateAsync(_service, _service.RepositoryRef, _resources, note.Entity, unsavedChanges: false, _logger);
        data.Repository = await GetRepositoryNameAsync();

        return Report(NoteDetailReport.Build(data, DateTime.Now), 1);
    }

    // The whole list, in the order requested: what the web's notes list loads page by page.
    public async Task<Result<List<NoteMinimalDto>>> LoadNotesAsync(NotesListReportRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        switch (request.Source)
        {
            case NotesListReportSource.Folder:
                if (request.FolderId == null)
                {
                    var noFolder = new Result<List<NoteMinimalDto>>();
                    noFolder.AddErrorMessage("No folder selected.");
                    return noFolder;
                }
                return await _service.Notes.GetFilterMinimalAsync(new NotesFilterDto
                {
                    FolderId = request.FolderId,
                    OrderColumn = request.OrderColumn,
                    OrderAscending = request.OrderAscending
                });

            case NotesListReportSource.Search:
                return await _service.Notes.GetSearchMinimalAsync(new NotesSearchDto
                {
                    TextSearch = request.TextSearch,
                    OrderColumn = request.OrderColumn,
                    OrderAscending = request.OrderAscending
                });

            default:
                var filter = (request.NotesFilter ?? new NotesFilterDto()).GetSimpleDto<NotesFilterDto>();
                filter.PageIdentifier = new PageIdentifier();
                filter.OrderColumn = request.OrderColumn;
                filter.OrderAscending = request.OrderAscending;
                return await _service.Notes.GetFilterMinimalAsync(filter);
        }
    }

    // Where the list comes from, with every name resolved, for the report's heading and the file name.
    public async Task<NotesListContext> GetContextAsync(NotesListReportRequestDto request)
    {
        var context = new NotesListContext { RepositoryName = await GetRepositoryNameAsync() };

        switch (request.Source)
        {
            case NotesListReportSource.Folder:
                context.Source = NotesListSource.Folder;
                if (request.FolderId != null)
                {
                    context.FolderPath = await NoteDetailReportData.GetFolderPathAsync(_service, request.FolderId.Value, _logger);
                    context.FolderNumber = (await _service.Folders.GetAsync(request.FolderId.Value)).Entity?.FolderNumber;
                }
                break;

            case NotesListReportSource.Search:
                context.Source = NotesListSource.Search;
                context.SearchText = request.TextSearch?.Trim();
                break;

            default:
                context.Source = NotesListSource.Filter;
                context.FilterCriteria = await NotesListContext.DescribeFilterCriteriaAsync(_service, request.NotesFilter ?? new NotesFilterDto(), _logger);
                break;
        }

        return context;
    }

    // The repository's name as the users see it in both apps (DB_REPOSITORYNAME); its database provider is
    // not shown on the web.
    private async Task<string> GetRepositoryNameAsync()
    {
        var info = await _service.SystemValues.GetRepositoryInfoAsync();
        return info.IsValid ? info.Entity?.Name : null;
    }

    private static Result<ReportDto> Report(ReportDocument report, int notesCount)
        => new(new ReportDto
        {
            Title = report.Title,
            Html = ReportHtml.Render(report),
            FileName = ReportFileName.Sanitize(report.FileNameBase ?? report.Title),
            NotesCount = notesCount
        });

    private static Result<T> Failed<T>(ResultBase failed)
    {
        var result = new Result<T>();
        result.AddListErrorMessage(failed.ListErrorMessage);
        result.NotAuthorized = failed.NotAuthorized;
        return result;
    }
}
