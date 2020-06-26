﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.ServiceEF.Repositories;
using KNote.Model;
using KNote.Model.Entities;
using KNote.Model.Dto;
using KNote.Model.Dto.Info;
using KNote.ServiceEF.Infrastructure;
using Microsoft.EntityFrameworkCore;

using System.Linq.Expressions;
using KNote.Model.Services;


namespace KNote.ServiceEF.Services
{
    public class KntNoteTypeService : DomainActionBase, IKntNoteTypeService
    {
        #region Fields

        private readonly IKntRepository _repository;

        #endregion

        #region Constructor

        public KntNoteTypeService(IKntRepository repository)
        {
            _repository = repository;
        }

        #endregion

        #region IKntNoteTypes

        public Result<List<NoteTypeInfoDto>> GetAll()
        {
            var resService = new Result<List<NoteTypeInfoDto>>();
            try
            {
                var resRep = _repository.NoteTypes.GetAll();
                resService.Entity = resRep.Entity?
                    .Select(t => t.GetSimpleDto<NoteTypeInfoDto>())
                    .OrderBy(t => t.Name)
                    .ToList();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<NoteTypeDto>> GetAsync(Guid id)
        {
            var resService = new Result<NoteTypeDto>();
            try
            {
                var resRep = await _repository.NoteTypes.GetAsync((object)id);
                
                resService.Entity = resRep.Entity?.GetSimpleDto<NoteTypeDto>();
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

        public async Task<Result<NoteTypeDto>> SaveAsync(NoteTypeDto entityInfo)
        {
            Result<NoteType> resRep = null;
            var resService = new Result<NoteTypeDto>();

            try
            {
                if (entityInfo.NoteTypeId == Guid.Empty)
                {
                    entityInfo.NoteTypeId = Guid.NewGuid();
                    var newEntity = new NoteType();
                    newEntity.SetSimpleDto(entityInfo);

                    // TODO: update standard control values to newEntity
                    // ...

                    resRep = await _repository.NoteTypes.AddAsync(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_repository.Users.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _repository.Users.ThrowKntException = false;
                    }

                    //var entityForUpdate = _repository.Users.Get(entityInfo.UserId).Entity;
                    resRep = await _repository.NoteTypes.GetAsync(entityInfo.NoteTypeId);
                    var entityForUpdate = resRep.Entity;

                    if (flagThrowKntException == true)
                        _repository.Users.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entityInfo);
                        resRep = await _repository.NoteTypes.UpdateAsync(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new NoteType();
                        newEntity.SetSimpleDto(entityInfo);

                        // TODO: update standard control values to newEntity
                        // ...
                        resRep = await _repository.NoteTypes.AddAsync(newEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            // TODO: Valorar refactorizar los siguiente (este patrón está en varios sitios).
            resService.Entity = resRep.Entity?.GetSimpleDto<NoteTypeDto>();
            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }

        public async Task<Result<NoteTypeInfoDto>> DeleteAsync(Guid id)
        {
            var resService = new Result<NoteTypeInfoDto>();
            try
            {
                var resRep = await _repository.NoteTypes.GetAsync(id);
                if (resRep.IsValid)
                {
                    resRep = await _repository.NoteTypes.DeleteAsync(resRep.Entity);
                    if (resRep.IsValid)
                        resService.Entity = resRep.Entity?.GetSimpleDto<NoteTypeInfoDto>();
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

    }
}
