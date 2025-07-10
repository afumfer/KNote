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

    public async Task<Result<FolderDto>> SaveAsync(FolderDto entity)
    {
        var command = new KntFoldersSaveAsyncCommand(Service, entity);
        return await ExecuteCommand(command);
    }

    public async Task<Result<FolderDto>> DeleteAsync(Guid id)
    {
        var command = new KntFoldersDeleteAsyncCommand(Service, id);
        return await ExecuteCommand(command);
    }

    #endregion
}
