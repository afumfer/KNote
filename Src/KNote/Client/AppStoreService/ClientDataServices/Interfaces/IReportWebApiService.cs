using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

// Printable reports (the HTML page of each one, Shared/KntReportPreview shows and prints it) and the CSV export of
// the notes lists (api/reports). Written in the user's culture (dates, numbers, CSV separator).
// cancellationToken (all of them): the caller no longer wants the answer; see BaseService.SendAsync.
public interface IReportWebApiService
{
    Task<Result<ReportDto>> GetNotesListAsync(NotesListReportRequestDto request, CancellationToken cancellationToken = default);

    Task<Result<ReportDto>> GetNotesBookAsync(NotesListReportRequestDto request, CancellationToken cancellationToken = default);

    Task<Result<ReportFileDto>> GetNotesListCsvAsync(NotesListReportRequestDto request, CancellationToken cancellationToken = default);

    Task<Result<ReportDto>> GetNoteDetailAsync(Guid noteId, CancellationToken cancellationToken = default);
}
