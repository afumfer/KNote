using System;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Service.Interfaces;
using KNote.Service.ServicesCommands;

namespace KNote.Service.Services;

public class KntSummaryService : KntServiceBase, IKntSummaryService
{
    public KntSummaryService(IKntService service) : base(service)
    {

    }

    public async Task<Result<DocumentToSummarizeDto>> PrepareDocumentAsync(Guid resourceId) =>
        await ExecuteCommand(new KntSummaryPrepareDocumentAsyncCommand(Service, resourceId));

    public async Task<Result<SummaryStateDto>> GetStateAsync() =>
        await ExecuteCommand(new KntSummaryGetStateAsyncCommand(Service));

    public async Task<Result<NoteDto>> SaveDocumentSummaryAsync(DocumentSummarySaveDto summary) =>
        await ExecuteCommand(new KntSummarySaveDocumentSummaryAsyncCommand(Service, summary));
}
