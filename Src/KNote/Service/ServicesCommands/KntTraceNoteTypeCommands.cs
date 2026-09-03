using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model.Dto;
using KNote.Model;
using KNote.Service.Core;

namespace KNote.Service.ServicesCommands;

public class KntTraceNoteTypeGetAllAsyncCommand : KntCommandServiceBase<Result<List<TraceNoteTypeDto>>>
{
    public KntTraceNoteTypeGetAllAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<List<TraceNoteTypeDto>>> Execute()
    {
        return await Repository.TraceNoteTypes.GetAllAsync();
    }
}

public class KntTraceNoteTypeGetAsyncCommand : KntCommandServiceBase<Guid, Result<TraceNoteTypeDto>>
{
    public KntTraceNoteTypeGetAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<TraceNoteTypeDto>> Execute()
    {
        return await Repository.TraceNoteTypes.GetAsync(Param);
    }
}

public class KntTraceNoteTypeSaveAsyncCommand : KntCommandSaveServiceBase<TraceNoteTypeDto, Result<TraceNoteTypeDto>>
{
    public KntTraceNoteTypeSaveAsyncCommand(IKntService service, TraceNoteTypeDto entity) : base(service, entity)
    {

    }

    public override async Task<Result<TraceNoteTypeDto>> Execute()
    {
        if (Param.TraceNoteTypeId == Guid.Empty)
        {
            Param.TraceNoteTypeId = Guid.NewGuid();
            return await Repository.TraceNoteTypes.AddAsync(Param);
        }
        else
        {
            return await Repository.TraceNoteTypes.UpdateAsync(Param);
        }
    }
}

public class KntTraceNoteTypeDeleteAsyncCommand : KntCommandServiceBase<Guid, Result<TraceNoteTypeDto>>
{
    public KntTraceNoteTypeDeleteAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<TraceNoteTypeDto>> Execute()
    {
        var result = new Result<TraceNoteTypeDto>();

        var resGetEntity = await Repository.TraceNoteTypes.GetAsync(Param);

        if (!resGetEntity.IsValid)
        {
            result.AddListErrorMessage(resGetEntity.ListErrorMessage);
            return result;
        }

        // Unlike KntNoteTypeDeleteAsyncCommand's "notes still use it" pre-check, a type still
        // referenced by TraceNotes is deliberately left to the DB's FK constraint (no ON DELETE
        // configured on TraceNotes.TraceNoteTypeId - see ModelBuilderExtensions) and unwrapped at
        // the ClientWin ctrl level (TraceNoteTypeEditorCtrl.DeleteModel), the same way a NoteType
        // still referenced by KAttributes is handled today - not every FK dependency gets its own
        // pre-check, just the most common one per entity.
        var resDelEntity = await Repository.TraceNoteTypes.DeleteAsync(Param);
        if (resDelEntity.IsValid)
            result.Entity = resGetEntity.Entity;
        else
            result.AddListErrorMessage(resDelEntity.ListErrorMessage);

        return result;
    }
}
