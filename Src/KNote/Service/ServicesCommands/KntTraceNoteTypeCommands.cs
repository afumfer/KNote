using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model.Dto;
using KNote.Model;
using KNote.Model.Core;
using KNote.Service.Core;

namespace KNote.Service.ServicesCommands;

[KntAuthorize(EnumRoles.Guest)]
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

[KntAuthorize(EnumRoles.Guest)]
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

[KntAuthorize(EnumRoles.Admin)]
public class KntTraceNoteTypeSaveAsyncCommand : KntCommandSaveServiceBase<TraceNoteTypeDto, Result<TraceNoteTypeDto>>
{
    public KntTraceNoteTypeSaveAsyncCommand(IKntService service, TraceNoteTypeDto entity) : base(service, entity)
    {

    }

    public override async Task<Result<TraceNoteTypeDto>> Execute()
    {
        TraceNoteTypeDto before = null;
        Result<TraceNoteTypeDto> result;
        if (Param.TraceNoteTypeId == Guid.Empty)
        {
            Param.TraceNoteTypeId = Guid.NewGuid();
            result = await Repository.TraceNoteTypes.AddAsync(Param);
        }
        else
        {
            var resOld = await Repository.TraceNoteTypes.GetAsync(Param.TraceNoteTypeId);
            before = resOld.IsValid ? resOld.Entity : null;
            result = await Repository.TraceNoteTypes.UpdateAsync(Param);
        }

        if (result.IsValid)
            await KntHistory.NoteStructureAsync(Service, HistoryEntities.TraceNoteType, Param.TraceNoteTypeId, Param.Name,
                StructureChangeDescriber.TraceNoteType(before, Param));
        return result;
    }
}

[KntAuthorize(EnumRoles.Admin)]
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
        {
            result.Entity = resGetEntity.Entity;
            await KntHistory.NoteStructureAsync(Service, HistoryEntities.TraceNoteType, Param, resGetEntity.Entity.Name,
                StructureChangeDescriber.TraceNoteType(resGetEntity.Entity, null));
        }
        else
            result.AddListErrorMessage(resDelEntity.ListErrorMessage);

        return result;
    }
}
