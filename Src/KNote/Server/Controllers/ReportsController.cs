using System;
using System.Globalization;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Reports;
using KNote.Server.Helpers;
using KNote.Service.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KNote.Server.Controllers;

// Printable reports and exports of the web app's notes lists and notes (KNote.Reports' WebReports, the same
// generators as ClientWin): the HTML page of a report, for the client to show and print, or a CSV file. As in
// ClientWin, printing or exporting what the repository holds requires the Staff role.
[Authorize(Roles = "Admin, Staff, ProjectManager")]
[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IKntService _service;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IKntService service, IFileStore fileStore, IHttpContextAccessor httpContextAccessor, ILogger<ReportsController> logger)
    {
        _service = service;
        _service.Logger = logger;
        _logger = logger;

        _service.UserIdentityName = httpContextAccessor.HttpContext.User?.Identity?.Name;

        // As NotesController: the report loads the resources (images of the notes) from this host unless the
        // configuration says otherwise.
        if (string.IsNullOrEmpty(_service.RepositoryRef.ResourcesContainerRootPath))
            _service.RepositoryRef.ResourcesContainerRootPath = fileStore.GetResourcesContainerRootPath();
        if (string.IsNullOrEmpty(_service.RepositoryRef.ResourcesContainerRootUrl))
            _service.RepositoryRef.ResourcesContainerRootUrl = fileStore.GetResourcesContainerRootUrl();
    }

    [HttpPost("noteslist")]
    public async Task<IActionResult> NotesList([FromBody] NotesListReportRequestDto request)
    {
        try
        {
            UseCulture(request.Culture);
            var resApi = await new WebReports(_service, _logger).NotesListAsync(request);
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "NotesList at {dateTime}.", DateTime.Now);
            return BadRequest(Error<ReportDto>(ex));
        }
    }

    [HttpPost("notesbook")]
    public async Task<IActionResult> NotesBook([FromBody] NotesListReportRequestDto request)
    {
        try
        {
            UseCulture(request.Culture);
            var resApi = await new WebReports(_service, _logger).NotesBookAsync(request, HttpContext.RequestAborted);
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested)
        {
            // The user left: nobody is waiting for the book.
            return BadRequest(new Result<ReportDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "NotesBook at {dateTime}.", DateTime.Now);
            return BadRequest(Error<ReportDto>(ex));
        }
    }

    [HttpPost("noteslistcsv")]
    public async Task<IActionResult> NotesListCsv([FromBody] NotesListReportRequestDto request)
    {
        try
        {
            UseCulture(request.Culture);
            var resApi = await new WebReports(_service, _logger).NotesListCsvAsync(request);
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "NotesListCsv at {dateTime}.", DateTime.Now);
            return BadRequest(Error<ReportFileDto>(ex));
        }
    }

    [HttpGet("notes/{id}")]
    public async Task<IActionResult> NoteDetail(Guid id, [FromQuery] string culture)
    {
        try
        {
            UseCulture(culture);
            var resApi = await new WebReports(_service, _logger).NoteDetailAsync(id);
            return resApi.IsValid ? Ok(resApi) : BadRequest(resApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "NoteDetail {id} at {dateTime}.", id, DateTime.Now);
            return BadRequest(Error<ReportDto>(ex));
        }
    }

    // The report is written in the user's culture (dates, numbers, CSV separator), for this request only: an
    // async method's culture changes don't reach its caller. An unknown culture keeps the server's.
    private static void UseCulture(string culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
            return;

        try
        {
            var cultureInfo = CultureInfo.GetCultureInfo(culture);
            CultureInfo.CurrentCulture = cultureInfo;
            CultureInfo.CurrentUICulture = cultureInfo;
        }
        catch (CultureNotFoundException)
        {
        }
    }

    private static Result<T> Error<T>(Exception ex)
    {
        var resApi = new Result<T>();
        resApi.AddErrorMessage(ex.ToApiErrorMessage());
        return resApi;
    }
}
