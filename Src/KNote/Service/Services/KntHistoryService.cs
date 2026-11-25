using System;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Service.Interfaces;
using KNote.Service.ServicesCommands;

namespace KNote.Service.Services;

public class KntHistoryService : KntServiceBase, IKntHistoryService
{
    public KntHistoryService(IKntService service) : base(service)
    {

    }

    public async Task<Result<NoteHistoryPageDto>> GetNoteHistoryAsync(Guid noteId, int skip = 0, int take = 0) =>
        await ExecuteCommand(new KntHistoryGetNoteHistoryAsyncCommand(Service, noteId, skip, take));

    public async Task<Result<NoteVersionViewDto>> GetNoteVersionAsync(Guid noteId, Guid klogId) =>
        await ExecuteCommand(new KntHistoryGetNoteVersionAsyncCommand(Service, noteId, klogId));
}
