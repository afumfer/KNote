using KNote.Model.Dto;
using KNote.Model;
using KNote.Service.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.Repository.EntityFramework.Entities;


namespace KNote.Service.ServicesCommands;

public class KntNotesGetAllAsyncCommand : KntCommandServiceBase<Result<List<NoteInfoDto>>>
{
    public KntNotesGetAllAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<List<NoteInfoDto>>> Execute()
    {
        return await Repository.Notes.GetAllAsync();
    }
}

public class KntNotesHomeAllAsyncCommand : KntCommandServiceBase<Result<List<NoteInfoDto>>>
{
    public KntNotesHomeAllAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<List<NoteInfoDto>>> Execute()
    {
        return await Repository.Notes.HomeNotesAsync();
    }
}

public class KntNotesGetAsyncCommand : KntCommandServiceBase<Guid, Result<NoteDto>>
{
    public KntNotesGetAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<NoteDto>> Execute()
    {
        return await Repository.Notes.GetAsync(Param);
    }
}

public class KntNotesGetByNumberAsyncCommand : KntCommandServiceBase<int, Result<NoteDto>>
{
    public KntNotesGetByNumberAsyncCommand(IKntService service, int id) : base(service, id)
    {

    }

    public override async Task<Result<NoteDto>> Execute()
    {
        return await Repository.Notes.GetAsync(Param);
    }
}

public class KntNotesGetExtendedAsyncCommand : KntCommandServiceBase<Guid, Result<NoteExtendedDto>>
{
    public KntNotesGetExtendedAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<NoteExtendedDto>> Execute()
    {
        var result = new Result<NoteExtendedDto>();

        var entity = (await Service.Notes.GetAsync(Param)).Entity.GetSimpleDto<NoteExtendedDto>();
        entity.Resources = (await Service.Notes.GetResourcesAsync(Param)).Entity;
        entity.Tasks = (await Service.Notes.GetNoteTasksAsync(Param)).Entity;
        entity.Messages = (await Service.Notes.GetMessagesAsync(Param)).Entity;

        result.Entity = entity;
        return result;
    }
}

public class KntNotesGetByFolderAsyncCommand : KntCommandServiceBase<Guid, Result<List<NoteInfoDto>>>
{
    public KntNotesGetByFolderAsyncCommand(IKntService service, Guid folderId) : base(service, folderId)
    {

    }

    public override async Task<Result<List<NoteInfoDto>>> Execute()
    {
        return await Repository.Notes.GetByFolderAsync(Param);
    }
}

//public class KntUsersSaveAsyncCommand : KntCommandServiceBase<UserDto, Result<UserDto>>
//{
//    public KntUsersSaveAsyncCommand(IKntService service, UserDto entity) : base(service, entity)
//    {

//    }

//    public override async Task<Result<UserDto>> Execute()
//    {
//        if (Param.UserId == Guid.Empty)
//        {
//            Param.UserId = Guid.NewGuid();
//            return await Repository.Users.AddAsync(Param);
//        }
//        else
//        {
//            return await Repository.Users.UpdateAsync(Param);
//        }
//    }
//}

//public class KntUsersDeleteAsyncCommand : KntCommandServiceBase<Guid, Result<UserDto>>
//{
//    public KntUsersDeleteAsyncCommand(IKntService service, Guid id) : base(service, id)
//    {

//    }

//    public override async Task<Result<UserDto>> Execute()
//    {
//        var result = new Result<UserDto>();

//        var resGetEntity = await Repository.Users.GetAsync(Param);

//        if (resGetEntity.IsValid)
//        {
//            var resDelEntity = await Repository.Users.DeleteAsync(Param);
//            if (resDelEntity.IsValid)
//                result.Entity = resGetEntity.Entity;
//            else
//                result.AddListErrorMessage(resDelEntity.ListErrorMessage);
//        }
//        else
//        {
//            result.AddListErrorMessage(resGetEntity.ListErrorMessage);
//        }

//        return result;
//    }
//}


