using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.DomainModel.Repositories;
using KNote.DomainModel.Entities;
using KNote.Shared.Dto.Info;
using KNote.Shared.Dto;
using KNote.Shared;
using KNote.DomainModel.Infrastructure;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace KNote.DomainModel.Services
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

        public Result<List<FolderInfoDto>> GetAll()
        {
            var resService = new Result<List<FolderInfoDto>>();
            try
            {
                var resRep = _repository.Folders.GetAll();
                resService.Entity = resRep.Entity?.Select(f => f.GetSimpleDto<FolderInfoDto>()).ToList();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public Result<List<FolderInfoDto>> GetRoots()
        {
            var resService = new Result<List<FolderInfoDto>>();
            try
            {
                var resRep = _repository.Folders.GetAll(f => f.ParentId == null);
                resService.Entity = resRep.Entity?.Select(f => f.GetSimpleDto<FolderInfoDto>()).ToList();
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

        public Result<List<FolderInfoDto>> GetTree()
        {
            var result = new Result<List<FolderInfoDto>>();

            var treeFolders = new List<FolderInfoDto>();

            try
            {                
                var allFolders = _repository.Folders
                    .DbSet.ToList();

                var allFoldersInfo = allFolders.Select(f => f.GetSimpleDto<FolderInfoDto>()).ToList();

                treeFolders = allFoldersInfo.Where(fi => fi.ParentId == null)
                    .OrderBy(f => f.Order).ThenBy(f => f.Name).ToList();

                foreach (FolderInfoDto f in treeFolders)
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

            return ResultDomainAction<List<FolderInfoDto>>(result);
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

        public Result<FolderInfoDto> Delete(Guid id)
        {
            var resService = new Result<FolderInfoDto>();
            try
            {
                var resRep = _repository.Folders.Get(id);
                if (resRep.IsValid)
                {
                    resRep = _repository.Folders.Delete(resRep.Entity);
                    if (resRep.IsValid)
                        resService.Entity = resRep.Entity?.GetSimpleDto<FolderInfoDto>();
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

        public async Task<Result<FolderInfoDto>> DeleteAsync(Guid id)
        {
            var resService = new Result<FolderInfoDto>();
            try
            {
                var resRep = await _repository.Folders.GetAsync(id);
                if (resRep.IsValid)
                {
                    resRep = await _repository.Folders.DeleteAsync(resRep.Entity);
                    if (resRep.IsValid)
                        resService.Entity = resRep.Entity?.GetSimpleDto<FolderInfoDto>();
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

        private void LoadChilds(FolderInfoDto folder, List<FolderInfoDto> allFolders)
        {
            folder.ChildFolders = allFolders.Where(fi => fi.ParentId == folder.FolderId)
                .OrderBy(f => f.Order).ThenBy(f => f.Name).ToList();

            foreach (FolderInfoDto f in folder.ChildFolders)
                LoadChilds(f, allFolders);
        }

        #endregion

        #region TODO

        //public Result<List<Folder>> GetAllFull(Expression<Func<Folder, bool>> predicate)
        //{
        //    var result = new Result<List<Folder>>();

        //    try
        //    {
        //        result.Entity = _repository.Context.Folders
        //            .Where(predicate)
        //            .Include(f => f.ChildsFolders)
        //            .Include(f => f.Notes)
        //            .ToList();
        //    }
        //    catch (KntEntityValidationException ex)
        //    {
        //        AddDBEntityErrorsToErrorsList(ex, result.ErrorList);
        //    }
        //    catch (Exception ex)
        //    {
        //        AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
        //    }

        //    return ResultDomainAction<List<Folder>>(result);
        //}

        #endregion 

    }
}
