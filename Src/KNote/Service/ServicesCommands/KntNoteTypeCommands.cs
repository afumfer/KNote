using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model.Dto;
using KNote.Model;
using KNote.Model.Core;
using KNote.Service.Core;

namespace KNote.Service.ServicesCommands;

[KntAuthorize(EnumRoles.Guest)]
public class KntNoteTypeGetAllAsyncCommand : KntCommandServiceBase<Result<List<NoteTypeDto>>>
{
    public KntNoteTypeGetAllAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<List<NoteTypeDto>>> Execute()
    {
        return await Repository.NoteTypes.GetAllAsync();
    }
}

[KntAuthorize(EnumRoles.Guest)]
public class KntNoteTypeGetAsyncCommand : KntCommandServiceBase<Guid, Result<NoteTypeDto>>
{
    public KntNoteTypeGetAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<NoteTypeDto>> Execute()
    {
        return await Repository.NoteTypes.GetAsync(Param);
    }
}

[KntAuthorize(EnumRoles.Admin)]
public class KntNoteTypeSaveAsyncCommand : KntCommandSaveServiceBase<NoteTypeDto, Result<NoteTypeDto>>
{
    public KntNoteTypeSaveAsyncCommand(IKntService service, NoteTypeDto entity) : base(service, entity)
    {

    }

    public override async Task<Result<NoteTypeDto>> Execute()
    {
        if (Param.NoteTypeId == Guid.Empty)
        {
            Param.NoteTypeId = Guid.NewGuid();
            var added = await Repository.NoteTypes.AddAsync(Param);
            if (added.IsValid)
                await NoteAsync(null);
            return added;
        }
        else
        {
            var resOld = await Repository.NoteTypes.GetAsync(Param.NoteTypeId);
            var res = await Repository.NoteTypes.UpdateAsync(Param);
            // Its name is in the fragment of data of its notes.
            if (res.IsValid && resOld.Entity is { } old && old.Name != Param.Name)
                await KntSemanticIndexer.MarkStaleAsync(Service, r => r.MarkStaleByNoteTypeAsync(Param.NoteTypeId));
            if (res.IsValid)
                await NoteAsync(resOld.IsValid ? resOld.Entity : null);
            return res;
        }
    }

    private async Task NoteAsync(NoteTypeDto before) =>
        await KntHistory.NoteStructureAsync(Service, HistoryEntities.NoteType, Param.NoteTypeId, Param.Name,
            StructureChangeDescriber.NoteType(before, Param, await KntHistory.NoteTypeNamesForHistoryAsync(Service)));
}

[KntAuthorize(EnumRoles.Admin)]
public class KntNoteTypeDeleteAsyncCommand : KntCommandServiceBase<Guid, Result<NoteTypeDto>>
{
    public KntNoteTypeDeleteAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<NoteTypeDto>> Execute()
    {
        var result = new Result<NoteTypeDto>();

        var resGetEntity = await Repository.NoteTypes.GetAsync(Param);

        if (!resGetEntity.IsValid)
        {
            result.AddListErrorMessage(resGetEntity.ListErrorMessage);
            return result;
        }

        // Business rule, checked here instead of letting it surface as a raw FK-constraint DB
        // error: a note type still in use by existing notes can't be deleted. Living in the command
        // (not in a caller like ClientWin's NoteTypeEditorCtrl) means every consumer of this service
        // - ClientWin and Server/Blazor's NoteTypesController alike - gets the same clear message.
        var notesUsingType = await Repository.Notes.GetFilterMinimalAsync(new NotesFilterDto { NoteTypeId = Param });
        if (notesUsingType.IsValid && notesUsingType.Entity?.Count > 0)
        {
            result.AddErrorMessage($"Can't delete this note type: {notesUsingType.Entity.Count} note(s) still use it.");
            return result;
        }

        var resDelEntity = await Repository.NoteTypes.DeleteAsync(Param);
        if (resDelEntity.IsValid)
        {
            result.Entity = resGetEntity.Entity;
            await KntHistory.NoteStructureAsync(Service, HistoryEntities.NoteType, Param, resGetEntity.Entity.Name,
                StructureChangeDescriber.NoteType(resGetEntity.Entity, null, null));
        }
        else
            result.AddListErrorMessage(resDelEntity.ListErrorMessage);

        return result;
    }
}