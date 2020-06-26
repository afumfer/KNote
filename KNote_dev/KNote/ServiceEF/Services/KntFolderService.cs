using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.ServiceEF.Repositories;
using KNote.Model.Entities;
using KNote.Model.Dto.Info;
using KNote.Model.Dto;
using KNote.Model;
using KNote.ServiceEF.Infrastructure;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using KNote.Model.Services;

namespace KNote.ServiceEF.Services
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

        public Result<List<FolderDto>> GetAll()
        {
            var resService = new Result<List<FolderDto>>();
            try
            {
                var resRep = _repository.Folders.GetAll();
                resService.Entity = resRep.Entity?.Select(f => f.GetSimpleDto<FolderDto>()).ToList();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public Result<List<FolderDto>> GetRoots()
        {
            var resService = new Result<List<FolderDto>>();
            try
            {
                var resRep = _repository.Folders.GetAll(f => f.ParentId == null);
                resService.Entity = resRep.Entity?.Select(f => f.GetSimpleDto<FolderDto>()).ToList();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public Result<FolderDto> Get(int folerNumber)
        {
            var resService = new Result<FolderDto>();
            try
            {
                var resRep = _repository.Folders.Get(f => f.FolderNumber == folerNumber);
                
                resService.Entity = resRep.Entity?.GetSimpleDto<FolderDto>();
                // KNote template ... load here aditionals properties for UserDto
                // ... 

                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }
        
        public Result<FolderDto> Get(Guid folderId)
        {
            var resService = new Result<FolderDto>();
            try
            {
                var resRep = _repository.Folders.Get((object)folderId);
                // KNote template ... load here aditionals properties for FolderDto
                resRep = _repository.Folders.LoadReference(resRep.Entity, n => n.ParentFolder);

                // Map to dto
                resService.Entity = resRep.Entity?.GetSimpleDto<FolderDto>();
                //resService.Entity.FolderDto = resRep.Entity?.Folder.GetSimpleDto<FolderDto>();
                resService.Entity.ParentFolderDto = new FolderDto();
                resService.Entity.ParentFolderDto = resRep.Entity?.ParentFolder?.GetSimpleDto<FolderDto>();

                // ... 

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
            var lastFolder = _repository.Folders
                .DbSet.OrderByDescending(f => f.FolderNumber).FirstOrDefault();

            return lastFolder != null ? lastFolder.FolderNumber + 1 : 1;
        }

        public Result<List<FolderDto>> GetTree()
        {
            var result = new Result<List<FolderDto>>();

            var treeFolders = new List<FolderDto>();

            try
            {
                var allFolders = _repository.Folders
                    .DbSet.ToList();

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

        public Result<FolderDto> New(FolderInfoDto entityInfo = null)
        {
            var resService = new Result<FolderDto>();
            FolderDto newFolder;

            try
            {
                newFolder = new FolderDto();
                if (entityInfo != null)
                    newFolder.SetSimpleDto(entityInfo);

                // TODO: load default values
                // for newFolder

                if (newFolder.FolderId == Guid.Empty)
                    newFolder.FolderId = Guid.NewGuid();

                // TODO: !!! repensar, se han eliminado estas fechas del dto de mantenimiento
                //if (newFolder.CreationDateTime == DateTime.MinValue)
                //    newFolder.CreationDateTime = DateTime.Now;

                //if (newFolder.ModificationDateTime == DateTime.MinValue)
                //    newFolder.ModificationDateTime = DateTime.Now;

                resService.Entity = newFolder;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            return ResultDomainAction(resService);
        }

        public Result<FolderDto> Save(FolderDto entity)
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

                    resRep = _repository.Folders.Add(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_repository.Folders.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _repository.Folders.ThrowKntException = false;
                    }

                    var entityForUpdate = _repository.Folders.Get(entity.FolderId).Entity;

                    if (flagThrowKntException == true)
                        _repository.Folders.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entity);
                        resRep = _repository.Folders.Update(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new Folder();
                        newEntity.SetSimpleDto(entity);

                        // TODO: update standard control values to newEntity
                        // ...
                        resRep = _repository.Folders.Add(newEntity);
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

        public Result<FolderDto> Delete(Guid id)
        {
            var resService = new Result<FolderDto>();
            try
            {
                var resRep = _repository.Folders.Get(id);
                if (resRep.IsValid)
                {
                    resRep = _repository.Folders.Delete(resRep.Entity);
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
