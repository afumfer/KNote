﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.ServiceEF.Repositories;
using KNote.Model;
using KNote.Model.Entities;
using KNote.Model.Dto;
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

        public async Task<Result<List<NoteTypeDto>>> GetAllAsync()
        {
            var resService = new Result<List<NoteTypeDto>>();
            try
            {
                var resRep = await _repository.NoteTypes.GetAllAsync();
                resService.Entity = resRep.Entity?
                    .Select(t => t.GetSimpleDto<NoteTypeDto>())
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

        public async Task<Result<NoteTypeDto>> SaveAsync(NoteTypeDto entity)
        {
            Result<NoteType> resRep = null;
            var resService = new Result<NoteTypeDto>();

            try
            {
                if (entity.NoteTypeId == Guid.Empty)
                {
                    entity.NoteTypeId = Guid.NewGuid();
                    var newEntity = new NoteType();
                    newEntity.SetSimpleDto(entity);

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
                    resRep = await _repository.NoteTypes.GetAsync(entity.NoteTypeId);
                    var entityForUpdate = resRep.Entity;

                    if (flagThrowKntException == true)
                        _repository.Users.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entity);
                        resRep = await _repository.NoteTypes.UpdateAsync(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new NoteType();
                        newEntity.SetSimpleDto(entity);

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

        public async Task<Result<NoteTypeDto>> DeleteAsync(Guid id)
        {
            var resService = new Result<NoteTypeDto>();
            try
            {
                var resRep = await _repository.NoteTypes.GetAsync(id);
                if (resRep.IsValid)
                {
                    resRep = await _repository.NoteTypes.DeleteAsync(resRep.Entity);
                    if (resRep.IsValid)
                        resService.Entity = resRep.Entity?.GetSimpleDto<NoteTypeDto>();
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
