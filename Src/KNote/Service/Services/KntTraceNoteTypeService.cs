using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Interfaces;
using KNote.Service.Core;
using KNote.Service.ServicesCommands;

namespace KNote.Service.Services;

public class KntTraceNoteTypeService : KntServiceBase, IKntTraceNoteTypeService
{
    #region Constructor

    public KntTraceNoteTypeService(IKntService service) : base(service)
    {

    }

    #endregion

    #region IKntTraceNoteTypeService

    public async Task<Result<List<TraceNoteTypeDto>>> GetAllAsync()
    {
        var command = new KntTraceNoteTypeGetAllAsyncCommand(Service);
        return await ExecuteCommand(command);
    }

    public async Task<Result<TraceNoteTypeDto>> GetAsync(Guid id)
    {
        var command = new KntTraceNoteTypeGetAsyncCommand(Service, id);
        return await ExecuteCommand(command);
    }

    public async Task<Result<TraceNoteTypeDto>> SaveAsync(TraceNoteTypeDto entity)
    {
        var command = new KntTraceNoteTypeSaveAsyncCommand(Service, entity);
        return await ExecuteCommand(command);
    }

    public async Task<Result<TraceNoteTypeDto>> DeleteAsync(Guid id)
    {
        var command = new KntTraceNoteTypeDeleteAsyncCommand(Service, id);
        return await ExecuteCommand(command);
    }

    #endregion

}
