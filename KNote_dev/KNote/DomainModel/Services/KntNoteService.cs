using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KNote.DomainModel.Repositories;
using KNote.Shared;
using KNote.DomainModel.Entities;
// TODO: Pendiente de eliminar
//using KNote.DomainModel.Dto;
using KNote.Shared.Dto;

using KNote.DomainModel.Infrastructure;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
//using System.Data.Entity.Validation;

namespace KNote.DomainModel.Services
{
    public class KntNoteService : DomainActionBase, IKntNoteService
    {
        #region Fields

        private readonly IKntRepository _repository;

        #endregion

        #region Constructor

        protected internal KntNoteService(IKntRepository repository)
        {
            _repository = repository;
        }

        #endregion

        #region IKntNoteService

        public Result<List<NoteInfoDto>> GetAll()
        {
            var resService = new Result<List<NoteInfoDto>>();
            try
            {
                var resRep = _repository.Notes.GetAll();
                resService.Entity = resRep.Entity?.Select(u => u.GetSimpleDto<NoteInfoDto>()).ToList();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        //RecentNotes
        public Result<List<NoteInfoDto>> RecentNotes()
        {
            var resService = new Result<List<NoteInfoDto>>();
            try
            {
                // TODO: !!!AAA
                var resRep = _repository.Notes.DbSet.OrderByDescending(n => n.NoteNumber).Take(25).ToList();
                resService.Entity = resRep.Select(u => u.GetSimpleDto<NoteInfoDto>()).ToList();
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public Result<NoteDto> Get(int noteNumber)
        {
            var resService = new Result<NoteDto>();
            try
            {
                var resRep = _repository.Notes.Get(n => n.NoteNumber == noteNumber);
                
                resService.Entity = resRep.Entity?.GetSimpleDto<NoteDto>();
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

        public async Task <Result<NoteDto>> GetAsync(Guid noteId)
        {
            var resService = new Result<NoteDto>();
            try
            {
                // Option 1 -----------------------
                var resRep = await _repository.Notes.GetAsync((object)noteId);
                // KNote template ... load here aditionals properties for UserDto
                resRep = _repository.Notes.LoadCollection(resRep.Entity, u => u.KAttributes);
                resRep = _repository.Notes.LoadReference(resRep.Entity, n => n.Folder);

                // Map to dto
                resService.Entity = resRep.Entity?.GetSimpleDto<NoteDto>();
                resService.Entity.KAttributesDto = resRep.Entity?.KAttributes.Select(_ => _.KAttribute.GetSimpleDto<NoteKAttributeDto>()).ToList() ;
                resService.Entity.FolderDto = resRep.Entity?.Folder.GetSimpleDto<FolderDto>();
                // ....

                resService.ErrorList = resRep.ErrorList;

                //// Option 2 ----------------------- 
                //var entity = _repository.Notes.DbSet.Where(n => n.NoteId == noteId)
                //    .Include(n => n.KAttributes).ThenInclude(n => n.KAttribute)
                //    .Include(n => n.NoteTasks)
                //    .Include(n => n.KMessages)
                //    .Include(n => n.Resources)
                //    .Include(n => n.Folder)
                //    .Include(n => n.NoteType)
                //    .FirstOrDefault();
                //resService.Entity = entity?.GetSimpleDto<NoteDto>();

            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }
        
        public int GetNextNoteNumber()
        {
            //var lastNote = _repository.Context.Notes.OrderByDescending(n => n.NoteNumber).FirstOrDefault();
            var lastNote = _repository.Notes
                .DbSet.OrderByDescending(n => n.NoteNumber).FirstOrDefault();
            return lastNote != null ? lastNote.NoteNumber + 1 : 1;
        }

        public Result<List<NoteInfoDto>> GetByFolder(Guid folderId)
        {
            var resService = new Result<List<NoteInfoDto>>();
            try
            {
                var resRep = _repository.Notes.GetAll(n => n.FolderId == folderId);
                resService.Entity = resRep.Entity?.Select(n => n.GetSimpleDto<NoteInfoDto>()).ToList();
                resService.ErrorList = resRep.ErrorList; ;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        // ==============
        // TODO: Sustituir esto por IQuerable
        // ==============
        public Result<List<NoteInfoDto>> GetFilter(int _page, int _limit, Guid folderId, string q) 
        {
            var resService = new Result<List<NoteInfoDto>>();
            Expression<Func<Note, bool>> predicate;

            if (folderId == new Guid("00000000-0000-0000-0000-000000000000"))
                if (string.IsNullOrEmpty(q))                    
                    predicate = null;
                else                    
                    predicate = (n) => (n.Topic.Contains(q) || n.Description.Contains(q));
            else
                if (string.IsNullOrEmpty(q))
                predicate = (n) => n.FolderId == folderId;
            else
                predicate = (n) => (n.FolderId == folderId) && (n.Topic.Contains(q) || n.Description.Contains(q));

            try
            {
                var totalEntities = _repository.Notes.GetAll(predicate).Entity.Count;
                var resRep = _repository.Notes.GetAllWithPagination(_page, _limit, predicate);
                if (resRep.IsValid)
                {                    
                    resService.Entity = resRep.Entity.Select(n => n.GetSimpleDto<NoteInfoDto>()).ToList();
                    resService.Tag = totalEntities.ToString();
                    return resService;
                }
                else
                {
                    resService.ErrorList = resRep.ErrorList;
                    return resService;
                }
            }
            catch (Exception ex)
            {
                resService.AddErrorMessage("Generic error: " + ex.Message);
                return resService;
            }
        }

        // ==============
        // TODO: Sustituir esto por IQuerable
        // ==============
        public Result<List<NoteItemDto>> GetNoteItemList(Guid? folderId)
        {            
            var result = new Result<List<NoteItemDto>>();

            try
            {
                if (folderId != null)
                {
                    result.Entity = _repository.Notes.DbSet
                        .Where(n => n.FolderId == folderId).Select(n => new NoteItemDto
                        {
                            NoteId = n.NoteId,
                            NoteNumber = n.NoteNumber,
                            Topic = n.Topic,
                            Priority = n.Priority,
                            Tags = n.Tags,
                            CreationDateTime = n.CreationDateTime,
                            ModificationDateTime = n.ModificationDateTime,
                            FolderId = n.FolderId
                        }).ToList();
                }
                else
                {
                    result.Entity = _repository.Notes.DbSet
                        .Select(n => new NoteItemDto
                        {
                            NoteId = n.NoteId,
                            NoteNumber = n.NoteNumber,
                            Topic = n.Topic,
                            Priority = n.Priority,
                            Tags = n.Tags,
                            CreationDateTime = n.CreationDateTime,
                            ModificationDateTime = n.ModificationDateTime,
                            FolderId = n.FolderId
                        }).ToList();
                }

            }
            catch (KntEntityValidationException ex)
            {
                AddDBEntityErrorsToErrorsList(ex, result.ErrorList);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }

            return ResultDomainAction<List<NoteItemDto>>(result);
        }

        public Result<NoteDto> New(NoteInfoDto entityInfo = null)
        {
            var resService = new Result<NoteDto>();
            NoteDto newNote;

            try
            {
                newNote = new NoteDto();
                if (entityInfo != null)
                    newNote.SetSimpleDto(entityInfo);

                // TODO: load default values
                // for newNote

                if (newNote.NoteId == Guid.Empty)
                    newNote.NoteId = Guid.NewGuid();

                // TODO: Pensar mejor de lo 
                //if (newNote.CreationDateTime == DateTime.MinValue)
                //    newNote.CreationDateTime = DateTime.Now;

                //if (newNote.ModificationDateTime == DateTime.MinValue)
                //    newNote.ModificationDateTime = DateTime.Now;

                // TODO: Cargar entidades dependientes
                //if (newNote.KAttributes == null)
                //    newNote.KAttributes = new List<NoteKAttribute>();

                //if (newNote.Resources == null)
                //    newNote.Resources = new List<Resource>();

                //if (newNote.Tasks == null)
                //    newNote.Tasks = new List<NoteTask>();

                //if (newNote.Windows == null)
                //    newNote.Windows = new List<Window>();

                //if (newNote.To == null)
                //    newNote.To = new List<TraceNote>();

                //if (newNote.From == null)
                //    newNote.From = new List<TraceNote>();

                resService.Entity = newNote;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            return ResultDomainAction(resService);
        }

        public Result<NoteDto> Save(NoteDto entity)
        {
            Result<Note> resRep = null;
            var resService = new Result<NoteDto>();

            try
            {
                if (entity.NoteId == Guid.Empty)
                {
                    entity.NoteId = Guid.NewGuid();
                    var newEntity = new Note();
                    newEntity.SetSimpleDto(entity);

                    // TODO: update standard control values to newEntity
                    // ...
                    newEntity.NoteNumber = GetNextNoteNumber();

                    if (newEntity.CreationDateTime == DateTime.MinValue)
                        newEntity.CreationDateTime = DateTime.Now;

                    if (newEntity.ModificationDateTime == DateTime.MinValue)
                        newEntity.ModificationDateTime = DateTime.Now;
                    // ...

                    resRep = _repository.Notes.Add(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_repository.Notes.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _repository.Notes.ThrowKntException = false;
                    }

                    var entityForUpdate = _repository.Notes.Get(entity.NoteId).Entity;

                    if (flagThrowKntException == true)
                        _repository.Notes.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entity);
                        resRep = _repository.Notes.Update(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new Note();
                        newEntity.SetSimpleDto(entity);

                        // TODO: update standard control values to newEntity
                        // ...
                        newEntity.NoteNumber = GetNextNoteNumber();

                        if (newEntity.CreationDateTime == DateTime.MinValue)
                            newEntity.CreationDateTime = DateTime.Now;

                        if (newEntity.ModificationDateTime == DateTime.MinValue)
                            newEntity.ModificationDateTime = DateTime.Now;
                        // ...


                        resRep = _repository.Notes.Add(newEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            // TODO: Valorar refactorizar los siguiente (este patrón está en varios sitios.
            resService.Entity = resRep.Entity?.GetSimpleDto<NoteDto>();
            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }

        public async Task<Result<NoteDto>> SaveAsync(NoteDto entity)
        {
            Result<Note> resRep = null;
            var resService = new Result<NoteDto>();

            try
            {
                if (entity.NoteId == Guid.Empty)
                {
                    entity.NoteId = Guid.NewGuid();
                    var newEntity = new Note();
                    newEntity.SetSimpleDto(entity);

                    // TODO: update standard control values to newEntity
                    // ...
                    newEntity.NoteNumber = GetNextNoteNumber();
                    if (newEntity.CreationDateTime == DateTime.MinValue)
                        newEntity.CreationDateTime = DateTime.Now;

                    if (newEntity.ModificationDateTime == DateTime.MinValue)
                        newEntity.ModificationDateTime = DateTime.Now;
                    // ...

                    resRep = await _repository.Notes.AddAsync(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_repository.Notes.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _repository.Notes.ThrowKntException = false;
                    }

                    resRep = await _repository.Notes.GetAsync(entity.NoteId);
                    var entityForUpdate = resRep.Entity;

                    if (flagThrowKntException == true)
                        _repository.Notes.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entity);
                        resRep = await _repository.Notes.UpdateAsync(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new Note();
                        newEntity.SetSimpleDto(entity);

                        // TODO: update standard control values to newEntity
                        // ...
                        newEntity.NoteNumber = GetNextNoteNumber();
                        if (newEntity.CreationDateTime == DateTime.MinValue)
                            newEntity.CreationDateTime = DateTime.Now;

                        if (newEntity.ModificationDateTime == DateTime.MinValue)
                            newEntity.ModificationDateTime = DateTime.Now;
                        // ...
                        resRep = await _repository.Notes.AddAsync(newEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            // TODO: Valorar refactorizar los siguiente (este patrón está en varios sitios.
            resService.Entity = resRep.Entity?.GetSimpleDto<NoteDto>();
            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }


        public Result<NoteKAttributeInfoDto> SaveAttrtibute(NoteKAttributeInfoDto entityInfo)
        {
            Result<NoteKAttribute> resRep = null;
            var resService = new Result<NoteKAttributeInfoDto>();

            try
            {
                if (entityInfo.NoteKAttributeId == Guid.Empty)
                {
                    entityInfo.NoteKAttributeId = Guid.NewGuid();
                    var newEntity = new NoteKAttribute();
                    newEntity.SetSimpleDto(entityInfo);

                    // TODO: update standard control values to newEntity
                    // ...
                                        
                    resRep = _repository.NoteKAttributes.Add(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_repository.NoteKAttributes.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _repository.NoteKAttributes.ThrowKntException = false;
                    }

                    var entityForUpdate = _repository.NoteKAttributes.Get(entityInfo.NoteKAttributeId).Entity;

                    if (flagThrowKntException == true)
                        _repository.NoteKAttributes.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entityInfo);
                        resRep = _repository.NoteKAttributes.Update(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new NoteKAttribute();
                        newEntity.SetSimpleDto(entityInfo);

                        // TODO: update standard control values to newEntity
                        // ...

                        resRep = _repository.NoteKAttributes.Add(newEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            // TODO: Valorar refactorizar los siguiente (este patrón está en varios sitios.
            resService.Entity = resRep.Entity?.GetSimpleDto<NoteKAttributeInfoDto>();
            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }

        public Result<ResourceInfoDto> SaveResource(ResourceInfoDto entityInfo)
        {
            Result<Resource> resRep = null;
            var resService = new Result<ResourceInfoDto>();

            try
            {
                if (entityInfo.ResourceId == Guid.Empty)
                {
                    entityInfo.ResourceId = Guid.NewGuid();
                    var newEntity = new Resource();
                    newEntity.SetSimpleDto(entityInfo);

                    // TODO: update standard control values to newEntity
                    // ...

                    resRep = _repository.Resources.Add(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_repository.Resources.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _repository.Resources.ThrowKntException = false;
                    }

                    var entityForUpdate = _repository.Resources.Get(entityInfo.ResourceId).Entity;

                    if (flagThrowKntException == true)
                        _repository.Resources.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entityInfo);
                        resRep = _repository.Resources.Update(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new Resource();
                        newEntity.SetSimpleDto(entityInfo);

                        // TODO: update standard control values to newEntity
                        // ...

                        resRep = _repository.Resources.Add(newEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            // TODO: Valorar refactorizar los siguiente (este patrón está en varios sitios.
            resService.Entity = resRep.Entity?.GetSimpleDto<ResourceInfoDto>();
            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }

        public Result<NoteTaskInfoDto> SaveNoteTask(NoteTaskInfoDto entityInfo)
        {
            Result<NoteTask> resRep = null;
            var resService = new Result<NoteTaskInfoDto>();

            try
            {
                if (entityInfo.NoteTaskId == Guid.Empty)
                {
                    entityInfo.NoteTaskId = Guid.NewGuid();
                    var newEntity = new NoteTask();
                    newEntity.SetSimpleDto(entityInfo);

                    // TODO: update standard control values to newEntity
                    // ...

                    resRep = _repository.NoteTasks.Add(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_repository.NoteTasks.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _repository.NoteTasks.ThrowKntException = false;
                    }

                    var entityForUpdate = _repository.NoteTasks.Get(entityInfo.NoteTaskId).Entity;

                    if (flagThrowKntException == true)
                        _repository.NoteTasks.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entityInfo);
                        resRep = _repository.NoteTasks.Update(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new NoteTask();
                        newEntity.SetSimpleDto(entityInfo);

                        // TODO: update standard control values to newEntity
                        // ...

                        resRep = _repository.NoteTasks.Add(newEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            // TODO: Valorar refactorizar los siguiente (este patrón está en varios sitios.
            resService.Entity = resRep.Entity?.GetSimpleDto<NoteTaskInfoDto>();
            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }

        public Result<WindowInfoDto> SaveWindow(WindowInfoDto entityInfo)
        {
            Result<Window> resRep = null;
            var resService = new Result<WindowInfoDto>();

            try
            {
                if (entityInfo.WindowId == Guid.Empty)
                {
                    entityInfo.WindowId = Guid.NewGuid();
                    var newEntity = new Window();
                    newEntity.SetSimpleDto(entityInfo);

                    // TODO: update standard control values to newEntity
                    // ...

                    resRep = _repository.Windows.Add(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_repository.Windows.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _repository.Windows.ThrowKntException = false;
                    }

                    var entityForUpdate = _repository.Windows.Get(entityInfo.WindowId).Entity;

                    if (flagThrowKntException == true)
                        _repository.Windows.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entityInfo);
                        resRep = _repository.Windows.Update(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new Window();
                        newEntity.SetSimpleDto(entityInfo);

                        // TODO: update standard control values to newEntity
                        // ...

                        resRep = _repository.Windows.Add(newEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            // TODO: Valorar refactorizar los siguiente (este patrón está en varios sitios.
            resService.Entity = resRep.Entity?.GetSimpleDto<WindowInfoDto>();
            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }

        public Result<TraceNoteInfoDto> SaveTraceNote(TraceNoteInfoDto entityInfo)
        {
            Result<TraceNote> resRep = null;
            var resService = new Result<TraceNoteInfoDto>();

            try
            {
                if (entityInfo.TraceNoteId == Guid.Empty)
                {
                    entityInfo.TraceNoteId = Guid.NewGuid();
                    var newEntity = new TraceNote();
                    newEntity.SetSimpleDto(entityInfo);

                    // TODO: update standard control values to newEntity
                    // ...

                    resRep = _repository.TraceNotes.Add(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_repository.TraceNotes.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _repository.TraceNotes.ThrowKntException = false;
                    }

                    var entityForUpdate = _repository.TraceNotes.Get(entityInfo.TraceNoteId).Entity;

                    if (flagThrowKntException == true)
                        _repository.TraceNotes.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entityInfo);
                        resRep = _repository.TraceNotes.Update(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new TraceNote();
                        newEntity.SetSimpleDto(entityInfo);

                        // TODO: update standard control values to newEntity
                        // ...

                        resRep = _repository.TraceNotes.Add(newEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            // TODO: Valorar refactorizar los siguiente (este patrón está en varios sitios.
            resService.Entity = resRep.Entity?.GetSimpleDto<TraceNoteInfoDto>();
            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }




        public Result<NoteInfoDto> Delete(Guid id)
        {
            var resService = new Result<NoteInfoDto>();
            try
            {
                var resRep = _repository.Notes.Get(id);
                if (resRep.IsValid)
                {
                    resRep = _repository.Notes.Delete(resRep.Entity);
                    if (resRep.IsValid)
                        resService.Entity = resRep.Entity?.GetSimpleDto<NoteInfoDto>();
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

        public async Task<Result<NoteInfoDto>> DeleteAsync(Guid id)
        {
            var resService = new Result<NoteInfoDto>();
            try
            {
                var resRep = await _repository.Notes.GetAsync(id);
                if (resRep.IsValid)
                {
                    resRep = await _repository.Notes.DeleteAsync(resRep.Entity);
                    if (resRep.IsValid)
                        resService.Entity = resRep.Entity?.GetSimpleDto<NoteInfoDto>();
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

        #region TODO: pendiente




        //public Result<List<NoteDto>> GetAllFull(Expression<Func<Note, bool>> predicate)
        //{
        //    throw new NotImplementedException();
        //}

        //public Result<NoteDto> LoadAllCollections(Note note)
        //{
        //    throw new NotImplementedException();
        //}

        #endregion 

    }
}
