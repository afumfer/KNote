using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.Repository.EntityFramework;
using KNote.Repository.Entities;
using KNote.Model.Dto;
using KNote.Model;
using KNote.Repository.EntityFramework;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using KNote.Service;
using KNote.Repository;

namespace KNote.Service.Services
{
    public class KntFolderService : DomainActionBase, IKntFolderService
    {
        #region Fields

        private readonly IKntRepository _repository;

        #endregion

        #region Constructor

        protected internal KntFolderService(IKntRepository repository)
        {
            _repository = repository;
        }

        #endregion

        #region IKntFolderService

        public async Task<Result<List<FolderDto>>> GetAllAsync()
        {
            var resService = new Result<List<FolderDto>>();
            try
            {
                var resRep = await _repository.Folders.GetAllAsync();
                resService.Entity = resRep.Entity?.Select(f => f.GetSimpleDto<FolderDto>()).ToList();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task <Result<List<FolderDto>>> GetRootsAsync()
        {
            var resService = new Result<List<FolderDto>>();
            try
            {
                var resRep = await _repository.Folders.GetAllAsync(f => f.ParentId == null);
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
                var resRep = await _repository.Folders.GetAsync((object)folderId);
                // KNote template ... load here aditionals properties for FolderDto
                resRep = _repository.Folders.LoadReference(resRep.Entity, n => n.ParentFolder);

                // Map to dto
                resService.Entity = resRep.Entity?.GetSimpleDto<FolderDto>();                
                resService.Entity.ParentFolderDto = new FolderDto();
                resService.Entity.ParentFolderDto = resRep.Entity?.ParentFolder?.GetSimpleDto<FolderDto>();
               
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public int GetNextFolderNumber()
        {            
            // Emplear método LastOrDefault() en lugar de FirstOrDafault 
            var lastFolder = _repository.Folders
                .DbSet.OrderByDescending(f => f.FolderNumber).FirstOrDefault();

            return lastFolder != null ? lastFolder.FolderNumber + 1 : 1;
        }

        public async Task<Result<List<FolderDto>>> GetTreeAsync()
        {
            var result = new Result<List<FolderDto>>();

            var treeFolders = new List<FolderDto>();

            try
            {
                var allFolders = await _repository.Folders
                    .DbSet.ToListAsync();

                var allFoldersInfo = allFolders.Select(f => f.GetSimpleDto<FolderDto>()).ToList();

                treeFolders = allFoldersInfo.Where(fi => fi.ParentId == null)
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

        public async Task<Result<FolderDto>> SaveAsync(FolderDto entity)
        {
            Result<Folder> resRep = null;
            var resService = new Result<FolderDto>();

            try
            {
                if (entity.FolderId == Guid.Empty)
                {
                    entity.FolderId = Guid.NewGuid();
                    var newEntity = new Folder();
                    newEntity.SetSimpleDto(entity);

                    // TODO: update standard control values to newEntity
                    // ...
                    newEntity.FolderNumber = GetNextFolderNumber();

                    resRep = await _repository.Folders.AddAsync(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        ThrowKntException = false;
                    }

                    resRep = await _repository.Folders.GetAsync(entity.FolderId);
                    var entityForUpdate = resRep.Entity;

                    if (flagThrowKntException == true)
                        ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entity);
                        resRep = await _repository.Folders.UpdateAsync(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new Folder();
                        newEntity.SetSimpleDto(entity);

                        // TODO: update standard control values to newEntity
                        // ...
                        resRep = await _repository.Folders.AddAsync(newEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            // TODO: Valorar refactorizar los siguiente (este patrón está en varios sitios.
            resService.Entity = resRep.Entity?.GetSimpleDto<FolderDto>();
            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }

        public async Task<Result<FolderDto>> DeleteAsync(Guid id)
        {
            var resService = new Result<FolderDto>();
            try
            {
                var resRep = await _repository.Folders.GetAsync(id);
                if (resRep.IsValid)
                {
                    resRep = await _repository.Folders.DeleteAsync(resRep.Entity);
                    if (resRep.IsValid)
                        resService.Entity = resRep.Entity?.GetSimpleDto<FolderDto>();
                    else
                        resService.ErrorList = resRep.ErrorList;
                }
                else
                    resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
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

        #endregion

    }
}
