using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using System.Linq.Expressions;
using KNote.Repository;
using KNote.Service.Interfaces;
using KNote.Service.Core;
using KNote.Service.ServicesCommands;

namespace KNote.Service.Services
{
    public class KntNoteTypeService : KntServiceBase, IKntNoteTypeService
    {
        #region Constructor

        public KntNoteTypeService(IKntService service) : base(service)
        {

        }

        #endregion

        #region IKntNoteTypes

        public async Task<Result<List<NoteTypeDto>>> GetAllAsync()
        {            
            var getAllCommand = new KntNoteTypeGetAllAsyncCommand(Service);
            return await ExecuteCommand(getAllCommand);
        }

        public async Task<Result<NoteTypeDto>> GetAsync(Guid id)
        {            
            var getAllCommand = new KntNoteTypeGetAsyncCommand(Service, id);
            return await ExecuteCommand(getAllCommand);
        }

        public async Task<Result<NoteTypeDto>> SaveAsync(NoteTypeDto entity)
        {
            //if (entity.NoteTypeId == Guid.Empty)
            //{
            //    entity.NoteTypeId = Guid.NewGuid();
            //    return await Repository.NoteTypes.AddAsync(entity);
            //}
            //else
            //{
            //    return await Repository.NoteTypes.UpdateAsync(entity);
            //}
            var getAllCommand = new KntNoteTypeSaveAsyncCommand(Service, entity);
            return await ExecuteCommand(getAllCommand);
        }

        public async Task<Result<NoteTypeDto>> DeleteAsync(Guid id)
        {
            //var result = new Result<NoteTypeDto>();

            //var resGetEntity = await GetAsync(id);

            //if (resGetEntity.IsValid) 
            //{ 
            //    var resDelEntity = await Repository.NoteTypes.DeleteAsync(id);
            //    if (resDelEntity.IsValid)                
            //        result.Entity = resGetEntity.Entity;                                    
            //    else                
            //        result.AddListErrorMessage(resDelEntity.ListErrorMessage);                                    
            //}
            //else
            //{
            //    result.AddListErrorMessage(resGetEntity.ListErrorMessage);
            //}

            //return result;

            var getAllCommand = new KntNoteTypeDeleteAsyncCommand(Service, id);
            return await ExecuteCommand(getAllCommand);
        }

        #endregion

    }
}
