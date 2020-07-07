using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.Repository.EntityFramework;
using KNote.Model;
using KNote.Repository.Entities;
using KNote.Model.Dto;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using KNote.Service;
using KNote.Repository;

namespace KNote.Service.Services
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
            return await _repository.NoteTypes.GetAllAsync();
        }

        public async Task<Result<NoteTypeDto>> GetAsync(Guid id)
        {
            return await _repository.NoteTypes.GetAsync(id);
        }

        public async Task<Result<NoteTypeDto>> SaveAsync(NoteTypeDto entity)
        {
            return await _repository.NoteTypes.SaveAsync(entity);
        }

        public async Task<Result<NoteTypeDto>> DeleteAsync(Guid id)
        {
            return await _repository.NoteTypes.DeleteAsync(id);
        }

        #endregion

    }
}
