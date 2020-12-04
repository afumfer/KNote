using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Repository.EntityFramework
{
    public class KntFolderRepository: DomainActionBase, IKntFolderRepository
    {
        private IGenericRepositoryEF<KntDbContext, Folder> _folders;

        public KntFolderRepository(KntDbContext context, bool throwKntException)
        {
            _folders = new GenericRepositoryEF<KntDbContext, Folder>(context, throwKntException);
            ThrowKntException = throwKntException;
        }

        public async Task<Result<List<FolderDto>>> GetAllAsync()
        {
            var resService = new Result<List<FolderDto>>();
            try
            {
                var resRep = await _folders.GetAllAsync();
                resService.Entity = resRep.Entity?.Select(f => f.GetSimpleDto<FolderDto>()).ToList();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<FolderDto>> GetAsync(Guid folderId)
        {
            var resService = new Result<FolderDto>();
            try
            {
                var resRep = await _folders.GetAsync((object)folderId);
                // KNote template ... load here aditionals properties for FolderDto
                resRep = _folders.LoadReference(resRep.Entity, n => n.ParentFolder);

                // Map to dto
                resService.Entity = resRep.Entity?.GetSimpleDto<FolderDto>();
                resService.Entity.ParentFolderDto = new FolderDto();
                resService.Entity.ParentFolderDto = resRep.Entity?.ParentFolder?.GetSimpleDto<FolderDto>();

                var resultChilds = await GetTreeAsync(folderId);
                resService.Entity.ChildFolders = resultChilds.Entity;

                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<List<FolderDto>>> GetTreeAsync(Guid? parentId = null)
        {
            var result = new Result<List<FolderDto>>();

            var treeFolders = new List<FolderDto>();

            try
            {
                var allFolders = await _folders.DbSet.ToListAsync();

                var allFoldersInfo = allFolders.Select(f => f.GetSimpleDto<FolderDto>()).ToList();

                treeFolders = allFoldersInfo.Where(fi => fi.ParentId == parentId)
                    .OrderBy(f => f.Order).ThenBy(f => f.Name).ToList();

                foreach (FolderDto f in treeFolders)
                    LoadChilds(f, allFoldersInfo);

                result.Entity = treeFolders;

            }
            catch (KntEntityValidationException ex)
            {
                AddDBEntityErrorsToErrorsList(ex, result.ErrorList);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }

            return ResultDomainAction<List<FolderDto>>(result);
        }
        
        public async Task<Result<FolderDto>> GetHomeAsync()
        {
            var resService = new Result<FolderDto>();

            try
            {
                var homeFolder = await _folders.DbSet
                    .Where(f => f.FolderNumber == 1)
                    .Select(f => f)
                    .FirstOrDefaultAsync();
                resService.Entity = homeFolder.GetSimpleDto<FolderDto>();
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            return ResultDomainAction(resService);
        }

        public async Task<Result<FolderDto>> AddAsync(FolderDto entity)
        {
            var response = new Result<FolderDto>();
            try
            {
                var newEntity = new Folder();
                newEntity.SetSimpleDto(entity);
                newEntity.FolderNumber = GetNextFolderNumber();
                newEntity.CreationDateTime = DateTime.Now;
                newEntity.ModificationDateTime = DateTime.Now;

                var resGenRep = await _folders.AddAsync(newEntity);

                response.Entity = resGenRep.Entity?.GetSimpleDto<FolderDto>();
                response.ErrorList = resGenRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);
        }

        public async Task<Result<FolderDto>> UpdateAsync(FolderDto entity)
        {
            var resGenRep = new Result<Folder>();
            var response = new Result<FolderDto>();

            try
            {
                bool flagThrowKntException = false;
                if (_folders.ThrowKntException == true)
                {
                    flagThrowKntException = true;
                    _folders.ThrowKntException = false;
                }

                var resGenRepGet = await _folders.GetAsync(entity.FolderId);
                Folder entityForUpdate;

                if (flagThrowKntException == true)
                    _folders.ThrowKntException = true;

                if (resGenRepGet.IsValid)
                {
                    entityForUpdate = resGenRepGet.Entity;
                    entityForUpdate.SetSimpleDto(entity);
                    entityForUpdate.ModificationDateTime = DateTime.Now;
                    resGenRep = await _folders.UpdateAsync(entityForUpdate);
                }
                else
                {
                    resGenRep.Entity = null;
                    resGenRep.AddErrorMessage("Can't find entity for update.");
                }

                response.Entity = resGenRep.Entity?.GetSimpleDto<FolderDto>();
                response.ErrorList = resGenRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }

            return ResultDomainAction(response);
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            var response = new Result();
            try
            {
                var resGenRep = await _folders.DeleteAsync(id);
                if (!resGenRep.IsValid)
                    response.ErrorList = resGenRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);
        }

        #region  IDisposable

        public virtual void Dispose()
        {
            _folders.Dispose();
        }

        #endregion

        #region Private methods

        private void LoadChilds(FolderDto folder, List<FolderDto> allFolders)
        {
            folder.ChildFolders = allFolders.Where(fi => fi.ParentId == folder.FolderId)
                .OrderBy(f => f.Order).ThenBy(f => f.Name).ToList();

            foreach (FolderDto f in folder.ChildFolders)
                LoadChilds(f, allFolders);
        }

        private int GetNextFolderNumber()
        {
            // Emplear método LastOrDefault() en lugar de FirstOrDafault 
            var lastFolder = _folders
                .DbSet.OrderByDescending(f => f.FolderNumber).FirstOrDefault();

            return lastFolder != null ? lastFolder.FolderNumber + 1 : 1;
        }

        #endregion

    }
}
