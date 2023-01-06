using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Repository.EntityFramework;

public class KntFolderRepository: KntRepositoryBase, IKntFolderRepository
{
    public KntFolderRepository(KntDbContext singletonContext, RepositoryRef repositoryRef)
        : base (singletonContext, repositoryRef)
    {            
    }

    public KntFolderRepository(RepositoryRef repositoryRef)
        : base(repositoryRef)
    {
    }

    public async Task<Result<List<FolderInfoDto>>> GetAllAsync()
    {
        var result = new Result<List<FolderInfoDto>>();

        try
        {
            var ctx = GetOpenConnection();
            var folders = new GenericRepositoryEF<KntDbContext, Folder>(ctx) ;

            var resRep = await folders.GetAllAsync();
            result.Entity = resRep.Entity?.Select(f => f.GetSimpleDto<FolderInfoDto>()).ToList();
            result.AddListErrorMessage(resRep.ListErrorMessage);

            await CloseIsTempConnection(ctx);
        }
        catch (Exception ex)
        {
            AddExecptionsMessagesToResult(ex, result);
        }

        return ResultDomainAction(result);
    }

    public async Task<Result<FolderDto>> GetAsync(Guid folderId)
    {
        return await GetAsync(folderId, null);
    }

    public async Task<Result<FolderDto>> GetAsync(int folderNumber)
    {
        return await GetAsync(null, folderNumber);
    }

    public async Task<Result<FolderDto>> GetAsync(Guid? folderId, int? folderNumber)
    {
        var result = new Result<FolderDto>();

        try
        {
            var ctx = GetOpenConnection();
            var folders = new GenericRepositoryEF<KntDbContext, Folder>(ctx);

            Result<Folder> resRep;
            if(folderId != null)
                resRep = await folders.GetAsync((object)folderId);
            else 
                resRep = await folders.GetAsync(f => f.FolderNumber == folderNumber);
            
            // KNote template ... load here aditionals properties for FolderDto
            resRep = folders.LoadReference(resRep.Entity, n => n.ParentFolder);

            // Map to dto
            result.Entity = resRep.Entity?.GetSimpleDto<FolderDto>();
            result.Entity.ParentFolderDto = new FolderDto();
            result.Entity.ParentFolderDto = resRep.Entity?.ParentFolder?.GetSimpleDto<FolderDto>();

            var resultChilds = await GetTreeAsync(result.Entity.FolderId);
            result.Entity.ChildFolders = resultChilds.Entity;

            result.AddListErrorMessage(resRep.ListErrorMessage);

            await CloseIsTempConnection(ctx);
        }
        catch (Exception ex)
        {
            AddExecptionsMessagesToResult(ex, result);
        }

        return ResultDomainAction(result);
    }

    public async Task<Result<List<FolderDto>>> GetTreeAsync(Guid? parentId = null)
    {
        var result = new Result<List<FolderDto>>();
        var treeFolders = new List<FolderDto>();

        try
        {
            var ctx = GetOpenConnection();
            var folders = new GenericRepositoryEF<KntDbContext, Folder>(ctx);

            var allFolders = await folders.DbSet.ToListAsync();

            var allFoldersInfo = allFolders.Select(f => f.GetSimpleDto<FolderDto>()).ToList();

            treeFolders = allFoldersInfo.Where(fi => fi.ParentId == parentId)
                .OrderBy(f => f.Order).ThenBy(f => f.Name).ToList();

            foreach (FolderDto f in treeFolders)
                LoadChilds(f, allFoldersInfo);

            result.Entity = treeFolders;

            await CloseIsTempConnection(ctx);
        }
        catch (Exception ex)
        {
            AddExecptionsMessagesToResult(ex, result);
        }

        return ResultDomainAction<List<FolderDto>>(result);
    }
    
    public async Task<Result<FolderDto>> GetHomeAsync()
    {
        var result = new Result<FolderDto>();

        try
        {
            var ctx = GetOpenConnection();
            var folders = new GenericRepositoryEF<KntDbContext, Folder>(ctx);

            var homeFolder = await folders.DbSet
                .Where(f => f.FolderNumber == 1)
                .Select(f => f)
                .FirstOrDefaultAsync();
            result.Entity = homeFolder.GetSimpleDto<FolderDto>();
            
            await CloseIsTempConnection(ctx);
        }
        catch (Exception ex)
        {
            AddExecptionsMessagesToResult(ex, result);
        }

        return ResultDomainAction(result);
    }

    public async Task<Result<FolderDto>> AddAsync(FolderDto entity)
    {
        var result = new Result<FolderDto>();

        try
        {
            var ctx = GetOpenConnection();
            var folders = new GenericRepositoryEF<KntDbContext, Folder>(ctx);

            var newEntity = new Folder();
            newEntity.SetSimpleDto(entity);
            newEntity.FolderNumber = GetNextFolderNumber(folders);
            newEntity.CreationDateTime = DateTime.Now;
            newEntity.ModificationDateTime = DateTime.Now;

            var resGenRep = await folders.AddAsync(newEntity);

            result.Entity = resGenRep.Entity?.GetSimpleDto<FolderDto>();
            result.AddListErrorMessage(resGenRep.ListErrorMessage);

            await CloseIsTempConnection(ctx);
        }
        catch (Exception ex)
        {
            AddExecptionsMessagesToResult(ex, result);
        }

        return ResultDomainAction(result);
    }

    public async Task<Result<FolderDto>> UpdateAsync(FolderDto entity)
    {
        var result = new Result<FolderDto>();
        var resGenRep = new Result<Folder>();

        try
        {
            var ctx = GetOpenConnection();
            var folders = new GenericRepositoryEF<KntDbContext, Folder>(ctx);

            var resGenRepGet = await folders.GetAsync(entity.FolderId);
            Folder entityForUpdate;

            if (resGenRepGet.IsValid)
            {
                entityForUpdate = resGenRepGet.Entity;
                entityForUpdate.SetSimpleDto(entity);
                entityForUpdate.ModificationDateTime = DateTime.Now;
                resGenRep = await folders.UpdateAsync(entityForUpdate);
            }
            else
            {
                resGenRep.Entity = null;
                resGenRep.AddErrorMessage("Can't find entity for update.");
            }

            result.Entity = resGenRep.Entity?.GetSimpleDto<FolderDto>();
            result.AddListErrorMessage(resGenRep.ListErrorMessage);

            await CloseIsTempConnection(ctx);
        }
        catch (Exception ex)
        {
            AddExecptionsMessagesToResult(ex, result);
        }

        return ResultDomainAction(result);
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var result = new Result();

        try
        {
            var ctx = GetOpenConnection();
            var folders = new GenericRepositoryEF<KntDbContext, Folder>(ctx);

            var resGenRep = await folders.DeleteAsync(id);
            if (!resGenRep.IsValid)
                result.AddListErrorMessage(resGenRep.ListErrorMessage);

            await CloseIsTempConnection(ctx);
        }
        catch (Exception ex)
        {
            AddExecptionsMessagesToResult(ex, result);
        }

        return ResultDomainAction(result);
    }

    public async Task<Result<int>> GetNextFolderNumber()
    {
        var result = new Result<int>();

        try
        {
            var ctx = GetOpenConnection();
            var folders = new GenericRepositoryEF<KntDbContext, Folder>(ctx);
            result.Entity = GetNextFolderNumber(folders);
            await CloseIsTempConnection(ctx);
        }
        catch (Exception ex)
        {
            AddExecptionsMessagesToResult(ex, result);
        }

        return ResultDomainAction(result);
    }

    #region Private methods

    private int GetNextFolderNumber(GenericRepositoryEF<KntDbContext, Folder> folders)
    {
        // Emplear método LastOrDefault() en lugar de FirstOrDafault 
        var lastFolder = folders
            .DbSet.OrderByDescending(f => f.FolderNumber).FirstOrDefault();

        return lastFolder != null ? lastFolder.FolderNumber + 1 : 1;
    }

    private void LoadChilds(FolderDto folder, List<FolderDto> allFolders)
    {
        folder.ChildFolders = allFolders.Where(fi => fi.ParentId == folder.FolderId)
            .OrderBy(f => f.Order).ThenBy(f => f.Name).ToList();

        foreach (FolderDto f in folder.ChildFolders)
            LoadChilds(f, allFolders);
    }

    #endregion

}
