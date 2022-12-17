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

namespace KNote.Service.Services
{
    public class KntNoteTypeService : KntServiceBase, IKntNoteTypeService
    {
        #region Fields

        //private readonly IKntService _parentService;

        //private IKntRepository Repository
        //{
        //    get { return _parentService.Repository; }
        //}

        #endregion

        #region Constructor

        

        //public KntNoteTypeService(IKntRepository repository)
        //{
        //    Repository = repository;
        //}

        public KntNoteTypeService(IKntService service) : base(service)
        {
            //_parentService = service;
        }


        #endregion

        #region IKntNoteTypes

        public async Task<Result<List<NoteTypeDto>>> GetAllAsync()
        {
            return await Repository.NoteTypes.GetAllAsync();
        }

        public async Task<Result<NoteTypeDto>> GetAsync(Guid id)
        {
            return await Repository.NoteTypes.GetAsync(id);
        }

        public async Task<Result<NoteTypeDto>> SaveAsync(NoteTypeDto entity)
        {
            if (entity.NoteTypeId == Guid.Empty)
            {
                entity.NoteTypeId = Guid.NewGuid();
                return await Repository.NoteTypes.AddAsync(entity);
            }
            else
            {
                return await Repository.NoteTypes.UpdateAsync(entity);
            }
        }

        public async Task<Result<NoteTypeDto>> DeleteAsync(Guid id)
        {
            var result = new Result<NoteTypeDto>();

            var resGetEntity = await GetAsync(id);

            if (resGetEntity.IsValid) 
            { 
                var resDelEntity = await Repository.NoteTypes.DeleteAsync(id);
                if (resDelEntity.IsValid)                
                    result.Entity = resGetEntity.Entity;                                    
                else                
                    result.AddListErrorMessage(resDelEntity.ListErrorMessage);                                    
            }
            else
            {
                result.AddListErrorMessage(resGetEntity.ListErrorMessage);
            }

            return result;
        }

        #endregion

    }
}
