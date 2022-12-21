using KNote.Model.Dto;
using KNote.Model;
using KNote.Service.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KNote.Service.ServicesCommands;

public class KntFoldersGetAllAsyncCommand : KntCommandServiceBase<Result<List<FolderInfoDto>>>
{
    public KntFoldersGetAllAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<List<FolderInfoDto>>> Execute()
    {
        return await Repository.Folders.GetAllAsync();
    }
}

public class KntFoldersGetAsyncCommand : KntCommandServiceBase<Guid, Result<FolderDto>>
{
    public KntFoldersGetAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<FolderDto>> Execute()
    {
        return await Repository.Folders.GetAsync(Param);
    }
}

public class KntFoldersGetByNumAsyncCommand : KntCommandServiceBase<int, Result<FolderDto>>
{
    public KntFoldersGetByNumAsyncCommand(IKntService service, int id) : base(service, id)
    {

    }

    public override async Task<Result<FolderDto>> Execute()
    {
        return await Repository.Folders.GetAsync(Param);
    }
}

public class KntFoldersGetTreeAsyncCommand : KntCommandServiceBase<Result<List<FolderDto>>>
{
    public KntFoldersGetTreeAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<List<FolderDto>>> Execute()
    {
        return await Repository.Folders.GetTreeAsync();
    }
}

public class KntFoldersGetHomeAsyncCommand : KntCommandServiceBase<Result<FolderDto>>
{
    public KntFoldersGetHomeAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<FolderDto>> Execute()
    {
        return await Repository.Folders.GetHomeAsync();
    }
}

public class KntFoldersSaveAsyncCommand : KntCommandServiceBase<FolderDto, Result<FolderDto>>
{
    public KntFoldersSaveAsyncCommand(IKntService service, FolderDto entity) : base(service, entity)
    {

    }

    public override async Task<Result<FolderDto>> Execute()
    {
        if (Param.FolderId == Guid.Empty)
        {
            Param.FolderId = Guid.NewGuid();
            var res = await Repository.Folders.AddAsync(Param);
            return res;
        }
        else
        {
            var res = await Repository.Folders.UpdateAsync(Param);
            return res;
        }
    }
}

public class KntFoldersDeleteAsyncCommand : KntCommandServiceBase<Guid, Result<FolderDto>>
{
    public KntFoldersDeleteAsyncCommand(IKntService service, Guid id) : base(service, id)
    {

    }

    public override async Task<Result<FolderDto>> Execute()
    {
        var result = new Result<FolderDto>();

        var resGetEntity = await Service.Folders.GetAsync(Param);

        if (resGetEntity.IsValid)
        {
            result.Entity = resGetEntity.Entity;

            // Check rules
            if (result.Entity.FolderNumber == 1)
                result.AddErrorMessage($"{result.Entity.Name} is the default folder. It cannot be erased.");

            if (resGetEntity.Entity.ChildFolders.Count > 0)
                result.AddErrorMessage("This folder has child folders. Delete is not possible.");

            if ((await Repository.Notes.CountNotesInFolder(Param)).Entity > 0)
                result.AddErrorMessage("This folder has notes. Delete is not possible.");

            if (!result.IsValid)
                return result;

            // Is OK then delete entity
            var resDelEntity = await Repository.Folders.DeleteAsync(Param);
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