using KNote.Model.Dto;
using KNote.Model;
using KNote.Service.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace KNote.Service.ServicesCommands;

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

public class KntNoteTypeSaveAsyncCommand : KntCommandServiceBase<NoteTypeDto, Result<NoteTypeDto>>
{
    public KntNoteTypeSaveAsyncCommand(IKntService service, NoteTypeDto entity) : base(service, entity)
    {

    }

    public override async Task<Result<NoteTypeDto>> Execute()
    {
        if (Param.NoteTypeId == Guid.Empty)
        {
            Param.NoteTypeId = Guid.NewGuid();
            return await Repository.NoteTypes.AddAsync(Param);
        }
        else
        {
            return await Repository.NoteTypes.UpdateAsync(Param);
        }
    }
}

public class KntNoteTypeDeleteAsyncCommand : KntCommandServiceBase<Guid, Result<NoteTypeDto>>
{
    public KntNoteTypeDeleteAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<NoteTypeDto>> Execute()
    {
        var result = new Result<NoteTypeDto>();

        var resGetEntity = await Repository.NoteTypes.GetAsync(Param);
        
        if (resGetEntity.IsValid)
        {
            var resDelEntity = await Repository.NoteTypes.DeleteAsync(Param);
            if (resDelEntity.IsValid)
                result.Entity = resGetEntity.Entity;
            else
                result.AddListErrorMessage(resDelEntity.ListErrorMessage);
        }
        else
        {
            result.AddListErrorMessage(resGetEntity.ListErrorMessage);
        }

        return result;
    }
}