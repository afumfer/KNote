using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model.Dto;
using KNote.Model;
using KNote.Service.Interfaces;
using KNote.Service.Core;
using KNote.Service.ServicesCommands;

namespace KNote.Service.Services;

public class KntFolderService : KntServiceBase, IKntFolderService
{
    #region Constructor

    public KntFolderService(IKntService service) : base(service)
    {
        
    }

    #endregion

    #region IKntFolderService

    public async Task<Result<List<FolderInfoDto>>> GetAllAsync()
    {        
        var command = new KntFoldersGetAllAsyncCommand(Service);
        return await ExecuteCommand(command);
    }
    
    public async Task<Result<FolderDto>> GetAsync(Guid folderId)
    {        
        var command = new KntFoldersGetAsyncCommand(Service, folderId);
        return await ExecuteCommand(command);
    }

    public async Task<Result<FolderDto>> GetAsync(int folderNumber)
    {        
        var command = new KntFoldersGetByNumAsyncCommand(Service, folderNumber);
        return await ExecuteCommand(command);
    }

    // The names of the folder's ancestors and its own, separated by a backslash (as ClientWin and the web app show it).
    // Each folder is read with GetAsync, so it needs the same role.
    public async Task<Result<string>> GetPathAsync(Guid folderId)
    {
        var names = new List<string>();
        Guid? id = folderId;
        // A loop in the parents (damaged data) can't hang it.
        while (id is Guid current && names.Count < MaxPathDepth)
        {
            var result = await GetAsync(current);
            if (!result.IsValid || result.Entity == null)
            {
                var error = new Result<string>();
                error.AddErrorMessage(result.ErrorMessage);
                return error;
            }

            names.Insert(0, result.Entity.Name);
            id = result.Entity.ParentId;
        }

        return new Result<string>(string.Join("\\", names));
    }

    private const int MaxPathDepth = 100;

    public async Task<Result<List<FolderDto>>> GetTreeAsync()
    {        
        var command = new KntFoldersGetTreeAsyncCommand(Service);
        return await ExecuteCommand(command);
    }

    public async Task<Result<FolderDto>> GetHomeAsync()
    {     
        var command = new KntFoldersGetHomeAsyncCommand(Service);
        return await ExecuteCommand(command);
    }

    public async Task<Result<List<FolderInfoDto>>> GetHomeChildrenAsync()
    {
        var command = new KntFoldersGetHomeChildrenAsyncCommand(Service);
        return await ExecuteCommand(command);
    }

    public async Task<Result<FolderDto>> SaveAsync(FolderDto entity)
    {
        var command = new KntFoldersSaveAsyncCommand(Service, entity);
        return await ExecuteCommand(command);
    }

    public async Task<Result> UpdateOrderNotesAsync(Guid folderId, string orderNotes)
    {
        var command = new KntFoldersUpdateOrderNotesAsyncCommand(Service, folderId, orderNotes);
        return await ExecuteCommand(command);
    }

    public async Task<Result<FolderDto>> DeleteAsync(Guid id)
    {
        var command = new KntFoldersDeleteAsyncCommand(Service, id);
        return await ExecuteCommand(command);
    }

    #endregion
}
