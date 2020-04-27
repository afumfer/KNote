using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.DomainModel.Repositories;
using KNote.Shared;
using KNote.DomainModel.Entities;
using KNote.Shared.Dto;
using KNote.DomainModel.Infrastructure;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

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
        
        public Result<List<NoteInfoDto>> HomeNotes()
        {
            var resService = new Result<List<NoteInfoDto>>();
            try
            {
                // TODO: Repensar esto 

                var idFolderHome = _repository.Folders.DbSet
                    .Where(f => f.FolderNumber == 1)
                    .Select(f => f.FolderId)
                    .SingleOrDefault();

                var resRep = _repository.Notes.DbSet
                    .Include(n => n.Folder)
                    //.Where( n => n.Folder.FolderNumber == 1)
                    .Where(n => n.FolderId == idFolderHome)
                    .OrderBy(n => n.NoteNumber).Take(25).ToList();

                resService.Entity = resRep.Select(u => u.GetSimpleDto<NoteInfoDto>()).ToList();
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
                var entity = await _repository.Notes.DbSet.Where(n => n.NoteId == noteId)
                    .Include(n => n.KAttributes).ThenInclude(n => n.KAttribute)                    
                    .Include(n => n.Folder)
                    .Include(n => n.NoteType)
                    .SingleOrDefaultAsync();

                // Map to dto
                resService.Entity = entity?.GetSimpleDto<NoteDto>();
                resService.Entity.FolderDto = entity?.Folder.GetSimpleDto<FolderDto>();                
                resService.Entity.KAttributesDto = entity?.KAttributes.Select(_ => _.GetSimpleDto<NoteKAttributeDto>()).ToList();                

                // Complete Attributes list
                resService.Entity.KAttributesDto = CompleteNoteAttributes(resService.Entity.KAttributesDto, entity.NoteId);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }
        
        private List<NoteKAttributeDto> CompleteNoteAttributes(List<NoteKAttributeDto> attributesNotes, Guid noteId)
        {
            var attributes = _repository.KAttributes.GetAll().Entity;
            foreach (KAttribute a in attributes)
            {
                var atrTmp = attributesNotes.Where(na => na.KAttributeId == a.KAttributeId).Select(at => at).SingleOrDefault();
                if (atrTmp == null)
                {
                    attributesNotes.Add(new NoteKAttributeDto
                    {
                        KAttributeId = a.KAttributeId,
                        NoteId = noteId,
                        Value = "",
                        KAttributeName = a.Name,
                        Key = a.Key,
                        KAttributeDataType = a.KAttributeDataType,
                        RequiredValue = a.RequiredValue,
                        Order = a.Order,
                        Script = a.Script,
                        Disabled = a.Disabled
                    });
                }
                else
                {
                    atrTmp.KAttributeName = a.Name;
                    atrTmp.Key = a.Key;
                    atrTmp.KAttributeDataType = a.KAttributeDataType;
                    atrTmp.RequiredValue = a.RequiredValue;
                    atrTmp.Order = a.Order;
                    atrTmp.Script = a.Script;
                    atrTmp.Disabled = a.Disabled;
                }
            }
            return attributesNotes.OrderBy(_ => _.Order).ToList();
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

        public async Task<Result<List<NoteInfoDto>>> GetFilter(NotesFilterDto notesFilter)
        {
            // TODO: !!! pendiente de completar

            var resService = new Result<List<NoteInfoDto>>();
            try
            {
                var query = _repository.Notes.Queryable;

                // Filters 

                if (notesFilter.FolderId != null)
                    query = query.Where(n => n.FolderId == notesFilter.FolderId);

                if (notesFilter.NoteTypeId != null)
                    query = query.Where(n => n.NoteTypeId == notesFilter.NoteTypeId);

                if (!string.IsNullOrEmpty(notesFilter.Topic))
                    query = query.Where(n => n.Topic.ToLower().Contains(notesFilter.Topic.ToLower()));

                if (!string.IsNullOrEmpty(notesFilter.Tags))
                    query = query.Where(n => n.Tags.ToLower().Contains(notesFilter.Tags.ToLower()));

                if (!string.IsNullOrEmpty(notesFilter.Description))
                    query = query.Where(n => n.Description.ToLower().Contains(notesFilter.Description.ToLower()));

                foreach(var f in notesFilter.AttributesFilter)
                {
                    //query = query.Where(n => n.KAttributes.Select(a => a.Value).Contains(f.Value));
                    query = query.Where(n => n.KAttributes.Where(_ => _.KAttributeId == f.AtrId).Select(a => a.Value).Contains(f.Value));
                }

                resService.CountEntity = await query.CountAsync();

                // Order by and pagination
                query = query
                    .OrderBy(n => n.Topic)
                    .Pagination(notesFilter.Pagination);

                // Get content
                resService.Entity = await query
                    .Select(u => u.GetSimpleDto<NoteInfoDto>())
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<List<NoteInfoDto>>> GetSearch(NotesSearchDto notesSearch)
        {
            var resService = new Result<List<NoteInfoDto>>();
            try
            {
                var query = _repository.Notes.Queryable;
                
                // TODO: !!! pendiente de analizar la cadéna de búsqueda para aplicar la lógica.

                //if (notesSearch.NoteNumber > 0 )
                //    query = query.Where(n => n.NoteNumber == notesSearch.NoteNumber);
                //else 
                
                if (!string.IsNullOrEmpty(notesSearch.TextSearch))
                    query = query.Where(n => n.Topic.ToLower().Contains(notesSearch.TextSearch.ToLower()));

                resService.CountEntity = await query.CountAsync();

                // Order by and pagination
                query = query
                    .OrderBy(n => n.Topic)
                    .Pagination(notesSearch.Pagination);

                // Get content
                resService.Entity = await query
                    .Select(u => u.GetSimpleDto<NoteInfoDto>())
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
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

                // Load default values
                // for newNote

                if (newNote.NoteId == Guid.Empty)
                    newNote.NoteId = Guid.NewGuid();
                               
                newNote.KAttributesDto = new List<NoteKAttributeDto>();
                newNote.KAttributesDto = CompleteNoteAttributes(newNote.KAttributesDto, newNote.NoteId);

                resService.Entity = newNote;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

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

                resService.Entity = resRep.Entity?.GetSimpleDto<NoteDto>();

                foreach (NoteKAttributeDto atr in entity.KAttributesDto)
                {                    
                    var res = await SaveAttrtibuteAsync(atr);
                    resService.Entity.KAttributesDto.Add(res.Entity);

                    // TODO: !!! pendiente de volcar errores en resRep
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            // TODO: Valorar refactorizar los siguiente (este patrón está en varios sitios.            
            resService.ErrorList = resRep.ErrorList;
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

        public async Task<Result<NoteKAttributeDto>> SaveAttrtibuteAsync(NoteKAttributeDto entity)
        {
            Result<NoteKAttribute> resRep = null;
            var resService = new Result<NoteKAttributeDto>();

            try
            {
                if (entity.NoteKAttributeId == Guid.Empty)
                {
                    entity.NoteKAttributeId = Guid.NewGuid();
                    var newEntity = new NoteKAttribute();
                    newEntity.SetSimpleDto(entity);

                    // TODO: update standard control values to newEntity
                    // ...
                                        
                    resRep = await _repository.NoteKAttributes.AddAsync(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_repository.NoteKAttributes.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _repository.NoteKAttributes.ThrowKntException = false;
                    }

                    var entityForUpdate = (await _repository.NoteKAttributes.GetAsync(entity.NoteKAttributeId)).Entity;

                    if (flagThrowKntException == true)
                        _repository.NoteKAttributes.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entity);
                        resRep = await _repository.NoteKAttributes.UpdateAsync(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new NoteKAttribute();
                        newEntity.SetSimpleDto(entity);

                        // TODO: update standard control values to newEntity
                        // ...

                        resRep = await _repository.NoteKAttributes.AddAsync(newEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            // TODO: Valorar refactorizar los siguiente (este patrón está en varios sitios.
            resService.Entity = resRep.Entity?.GetSimpleDto<NoteKAttributeDto>();
            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }

        public async Task<Result<ResourceDto>> SaveResourceAsync(ResourceDto entity)
        {
            Result<Resource> resRep = null;
            var resService = new Result<ResourceDto>();

            try
            {
                if (entity.ResourceId == Guid.Empty)
                {
                    entity.ResourceId = Guid.NewGuid();
                    var newEntity = new Resource();
                    newEntity.SetSimpleDto(entity);

                    // TODO: update standard control values to newEntity (refactor ...)
                    // ...
                    newEntity.Container = @"NotesResources\" + DateTime.Now.Year.ToString();
                    newEntity.ContentArrayBytes = Convert.FromBase64String(entity.ContentBase64);

                    resRep = await _repository.Resources.AddAsync(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_repository.Resources.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _repository.Resources.ThrowKntException = false;
                    }

                    var entityForUpdate = _repository.Resources.Get(entity.ResourceId).Entity;

                    if (flagThrowKntException == true)
                        _repository.Resources.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entity);
                        entityForUpdate.ContentArrayBytes = Convert.FromBase64String(entity.ContentBase64);
                        resRep = await _repository.Resources.UpdateAsync(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new Resource();
                        newEntity.SetSimpleDto(entity);

                        // TODO: update standard control values to newEntity  (refactor ...)
                        // ...
                        newEntity.Container = @"NotesResources\" + DateTime.Now.Year.ToString();
                        newEntity.ContentArrayBytes = Convert.FromBase64String(entity.ContentBase64);

                        resRep = await _repository.Resources.AddAsync(newEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            // TODO: Valorar refactorizar los siguiente (este patrón está en varios sitios.
            resService.Entity = resRep.Entity?.GetSimpleDto<ResourceDto>();
            if(resService.Entity != null)
                resService.Entity.ContentBase64 = entity.ContentBase64;

            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }

        public Result<List<ResourceDto>> GetNoteResources(Guid idNote)
        {
            var resService = new Result<List<ResourceDto>>();
            try
            {
                var resRep = _repository.Resources.GetAll(r => r.NoteId == idNote);
                resService.Entity = resRep.Entity?.Select(u => u.GetSimpleDto<ResourceDto>()).ToList();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<ResourceDto>> DeleteResourceAsync(Guid id)
        {
            var resService = new Result<ResourceDto>();
            try
            {
                var resRep = await _repository.Resources.GetAsync(id);
                if (resRep.IsValid)
                {
                    resRep = await _repository.Resources.DeleteAsync(resRep.Entity);
                    if (resRep.IsValid)
                        resService.Entity = resRep.Entity?.GetSimpleDto<ResourceDto>();
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

        // TODO: Pendiente de estandarizar
        public async Task<Result<NoteTaskDto>> SaveNoteTaskAsync(NoteTaskDto entityInfo)
        {
            Result<NoteTask> resRep = null;
            var resService = new Result<NoteTaskDto>();

            var userFullName = entityInfo.UserFullName;

            try
            {
                if (entityInfo.NoteTaskId == Guid.Empty)
                {
                    entityInfo.NoteTaskId = Guid.NewGuid();
                    var newEntity = new NoteTask();
                    newEntity.SetSimpleDto(entityInfo);

                    // TODO: update standard control values to newEntity
                    newEntity.CreationDateTime = DateTime.Now;
                    newEntity.ModificationDateTime = DateTime.Now;
                    // ...

                    resRep = await _repository.NoteTasks.AddAsync(newEntity);
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
                        entityForUpdate.SetSimpleDto(entityInfo);
                        entityForUpdate.ModificationDateTime = DateTime.Now;
                        // ...

                        resRep = await _repository.NoteTasks.UpdateAsync(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new NoteTask();
                        newEntity.SetSimpleDto(entityInfo);

                        // TODO: update standard control values to newEntity
                        newEntity.CreationDateTime = DateTime.Now;
                        newEntity.ModificationDateTime = DateTime.Now;
                        // ...

                        resRep = await _repository.NoteTasks.AddAsync(newEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            // TODO: Valorar refactorizar los siguiente (este patrón está en varios sitios.
            resService.Entity = resRep.Entity?.GetSimpleDto<NoteTaskDto>();
            if(resRep.Entity != null)
                resService.Entity.UserFullName = userFullName;
            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }

        public Result<List<NoteTaskDto>> GetNoteTasks(Guid idNote)
        {
            var resService = new Result<List<NoteTaskDto>>();
            try
            {
                // TODO: !!! Borrar ...
                //var resRep = _repository.NoteTasks.GetAll(r => r.NoteId == idNote);
                //resService.Entity = resRep.Entity?.Select(u => u.GetSimpleDto<NoteTaskDto>()).ToList();
                //resService.ErrorList = resRep.ErrorList;

                var listTasks = _repository.NoteTasks.DbSet.Where(n => n.NoteId == idNote)                    
                    .Include(t => t.User)                    
                    .ToList();
                resService.Entity = new List<NoteTaskDto>();
                foreach (var e in listTasks)
                {
                    var nt = e.GetSimpleDto<NoteTaskDto>();
                    nt.UserFullName = e.User.FullName;
                    resService.Entity.Add(nt);
                }                                
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<NoteTaskDto>> DeleteNoteTaskAsync(Guid id)
        {
            var resService = new Result<NoteTaskDto>();
            try
            {
                var resRep = await _repository.NoteTasks.GetAsync(id);
                if (resRep.IsValid)
                {
                    resRep = await _repository.NoteTasks.DeleteAsync(resRep.Entity);
                    if (resRep.IsValid)
                        resService.Entity = resRep.Entity?.GetSimpleDto<NoteTaskDto>();
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


        #region // TODO: !!! Pendiente de estandarizar / Implementar

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

        #endregion

        #endregion

        #region Código provisional, pendiente de eliminación 

        #region Get (int noteNumber)  (Old)
        // TODO: !!! Valorar la eliminación de este método.
        //      la búsqueda por noteNumber se podría desplazar a la búsqueda por filtro. 

        //public Result<NoteDto> Get(int noteNumber)
        //{
        //    var resService = new Result<NoteDto>();
        //    try
        //    {
        //        var resRep = _repository.Notes.Get(n => n.NoteNumber == noteNumber);

        //        resService.Entity = resRep.Entity?.GetSimpleDto<NoteDto>();
        //        // KNote template ... load here aditionals properties for UserDto
        //        // ... 

        //        resService.ErrorList = resRep.ErrorList;
        //    }
        //    catch (Exception ex)
        //    {
        //        AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
        //    }
        //    return ResultDomainAction(resService);
        //}
        #endregion

        #region GetFilter (old)
        //public Result<List<NoteInfoDto>> GetFilter(int _page, int _limit, Guid folderId, string q) 
        //{
        //    var resService = new Result<List<NoteInfoDto>>();
        //    Expression<Func<Note, bool>> predicate;

        //    if (folderId == new Guid("00000000-0000-0000-0000-000000000000"))
        //        if (string.IsNullOrEmpty(q))                    
        //            predicate = null;
        //        else                    
        //            predicate = (n) => (n.Topic.Contains(q) || n.Description.Contains(q));
        //    else
        //        if (string.IsNullOrEmpty(q))
        //        predicate = (n) => n.FolderId == folderId;
        //    else
        //        predicate = (n) => (n.FolderId == folderId) && (n.Topic.Contains(q) || n.Description.Contains(q));

        //    try
        //    {
        //        var totalEntities = _repository.Notes.GetAll(predicate).Entity.Count;
        //        var resRep = _repository.Notes.GetAllWithPagination(_page, _limit, predicate);
        //        if (resRep.IsValid)
        //        {                    
        //            resService.Entity = resRep.Entity.Select(n => n.GetSimpleDto<NoteInfoDto>()).ToList();
        //            resService.Tag = totalEntities.ToString();
        //            return resService;
        //        }
        //        else
        //        {
        //            resService.ErrorList = resRep.ErrorList;
        //            return resService;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        resService.AddErrorMessage("Generic error: " + ex.Message);
        //        return resService;
        //    }
        //}
        #endregion

        #region GetNoteItemList  (old, aplicación de escritorio)
        //// TODO: eliminar, esto se usaba en la aplicación de escritorio
        //public Result<List<NoteItemDto>> GetNoteItemList(Guid? folderId)
        //{            
        //    var result = new Result<List<NoteItemDto>>();

        //    try
        //    {
        //        if (folderId != null)
        //        {
        //            result.Entity = _repository.Notes.DbSet
        //                .Where(n => n.FolderId == folderId).Select(n => new NoteItemDto
        //                {
        //                    NoteId = n.NoteId,
        //                    NoteNumber = n.NoteNumber,
        //                    Topic = n.Topic,
        //                    Priority = n.Priority,
        //                    Tags = n.Tags,
        //                    CreationDateTime = n.CreationDateTime,
        //                    ModificationDateTime = n.ModificationDateTime,
        //                    FolderId = n.FolderId
        //                }).ToList();
        //        }
        //        else
        //        {
        //            result.Entity = _repository.Notes.DbSet
        //                .Select(n => new NoteItemDto
        //                {
        //                    NoteId = n.NoteId,
        //                    NoteNumber = n.NoteNumber,
        //                    Topic = n.Topic,
        //                    Priority = n.Priority,
        //                    Tags = n.Tags,
        //                    CreationDateTime = n.CreationDateTime,
        //                    ModificationDateTime = n.ModificationDateTime,
        //                    FolderId = n.FolderId
        //                }).ToList();
        //        }

        //    }
        //    catch (KntEntityValidationException ex)
        //    {
        //        AddDBEntityErrorsToErrorsList(ex, result.ErrorList);
        //    }
        //    catch (Exception ex)
        //    {
        //        AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
        //    }

        //    return ResultDomainAction<List<NoteItemDto>>(result);
        //}
        #endregion

        #region Delete (no async)
        //public Result<NoteInfoDto> Delete(Guid id)
        //{
        //    var resService = new Result<NoteInfoDto>();
        //    try
        //    {
        //        var resRep = _repository.Notes.Get(id);
        //        if (resRep.IsValid)
        //        {
        //            resRep = _repository.Notes.Delete(resRep.Entity);
        //            if (resRep.IsValid)
        //                resService.Entity = resRep.Entity?.GetSimpleDto<NoteInfoDto>();
        //            else
        //                resService.ErrorList = resRep.ErrorList;
        //        }
        //        else
        //            resService.ErrorList = resRep.ErrorList;
        //    }
        //    catch (Exception ex)
        //    {
        //        AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
        //    }
        //    return ResultDomainAction(resService);
        //}
        #endregion 

        #region Pruebas acceso colecciones 
        //private void PruebaBasura()
        //{
        //    // TODO: borrar, old. 
        //    //var resRep = await _repository.Notes.GetAsync((object)noteId);
        //    //// Load here aditionals properties for NoteDto
        //    //resRep = _repository.Notes.LoadReference(resRep.Entity, n => n.Folder);
        //    //resRep = _repository.Notes.LoadCollection(resRep.Entity, u => u.KAttributes);
        //    //resRep = _repository.Notes.LoadCollection(resRep.Entity, u => u.Resources);

        //    //// Map to dto
        //    //resService.Entity = resRep.Entity?.GetSimpleDto<NoteDto>();
        //    //resService.Entity.FolderDto = resRep.Entity?.Folder.GetSimpleDto<FolderDto>();
        //    //resService.Entity.KAttributesDto = resRep.Entity?.KAttributes.Select(_ => _.KAttribute.GetSimpleDto<NoteKAttributeDto>()).ToList();
        //    //resService.Entity.ResourcesDto = resRep.Entity?.Resources.Select(_ => _.GetSimpleDto<ResourceDto>()).ToList();

        //    //resService.ErrorList = resRep.ErrorList;
        //}
        #endregion


        #endregion

    }

    //public class kv
    //{
    //    public Guid Id { get; set; }
    //    public string KValue { get; set; }
    //}
}
