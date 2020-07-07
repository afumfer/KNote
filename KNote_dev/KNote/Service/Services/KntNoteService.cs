using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KNote.Repository.EntityFramework;
using KNote.Model;
using KNote.Repository.Entities;
using KNote.Model.Dto;

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using KNote.Service;
using KNote.Repository;

namespace KNote.Service.Services
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

        public async Task<Result<List<NoteInfoDto>>> GetAllAsync()
        {
            var resService = new Result<List<NoteInfoDto>>();
            try
            {
                var resRep = await _repository.Notes.GetAllAsync();
                resService.Entity = resRep.Entity?
                    .Select(_ => _.GetSimpleDto<NoteInfoDto>())
                    .OrderBy(_ => _.Priority).ThenBy(_ => _.Topic)
                    .ToList();
                resService.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }
        
        public async Task<Result<List<NoteInfoDto>>> HomeNotesAsync()
        {
            var resService = new Result<List<NoteInfoDto>>();
            try
            {
                // TODO: Repensar esto 

                var idFolderHome = await _repository.Folders.DbSet
                    .Where(f => f.FolderNumber == 1)
                    .Select(f => f.FolderId)
                    .SingleOrDefaultAsync();

                var resRep = await _repository.Notes.DbSet
                    .Include(n => n.Folder)                    
                    .Where(n => n.FolderId == idFolderHome)
                    .OrderBy(n => n.Priority).ThenBy(n => n.Topic)
                    .Take(25).ToListAsync();

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
                resService.Entity.KAttributesDto = entity?.KAttributes
                    .Select(_ => _.GetSimpleDto<NoteKAttributeDto>())
                    .Where(_ => _.KAttributeNoteTypeId == null || _.KAttributeNoteTypeId == resService.Entity.NoteTypeId)
                    .ToList();                

                // Complete Attributes list
                resService.Entity.KAttributesDto = await CompleteNoteAttributes(resService.Entity.KAttributesDto, entity.NoteId, entity.NoteTypeId);
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

        public async Task<Result<List<NoteInfoDto>>> GetByFolderAsync(Guid folderId)
        {
            var resService = new Result<List<NoteInfoDto>>();
            try
            {
                var resRep = await _repository.Notes.GetAllAsync(n => n.FolderId == folderId);
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
                    .OrderBy(n => n.Priority).ThenBy(n => n.Topic)
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
            int searchNumber;
            
            try
            {
                var query = _repository.Notes.Queryable;

                searchNumber = ExtractNoteNumberSearch(notesSearch.TextSearch);
                
                if (searchNumber > 0)
                    query = query.Where(n => n.NoteNumber == searchNumber);

                else
                {
                    var listTokensAll = ExtractListTokensSearch(notesSearch.TextSearch);
                    var listTokens = listTokensAll.Where(t => t != "***").Select( t => t).ToList();
                    var flagSearchDescription = listTokensAll.Where(t => t == "***").Select(t => t).FirstOrDefault();

                    if (flagSearchDescription != "***")
                    {
                        foreach (var token in listTokens)
                        {
                            if (!string.IsNullOrEmpty(token))
                            {
                                if (token[0] != '!')
                                    query = query.Where(n => n.Topic.ToLower().Contains(token.ToLower()));
                                else
                                {
                                    var tokenNot = token.Substring(1, token.Length - 1);
                                    query = query.Where(n => !n.Topic.ToLower().Contains(tokenNot.ToLower()));
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var token in listTokens)
                        {
                            if (!string.IsNullOrEmpty(token))
                            {
                                if (token[0] != '!')
                                    query = query.Where(n => n.Topic.ToLower().Contains(token.ToLower()) || n.Description.ToLower().Contains(token.ToLower()));
                                else
                                {
                                    var tokenNot = token.Substring(1, token.Length - 1);
                                    query = query.Where(n => !n.Topic.ToLower().Contains(tokenNot.ToLower()) && !n.Description.ToLower().Contains(tokenNot.ToLower()));
                                }
                            }
                        }
                    }
                }

                resService.CountEntity = await query.CountAsync();

                // Order by and pagination
                query = query
                    .OrderBy(n => n.Priority).ThenBy(n => n.Topic)
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

        public async Task<Result<NoteDto>> NewAsync(NoteInfoDto entityInfo = null)
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

                newNote.IsNew = true;

                newNote.KAttributesDto = new List<NoteKAttributeDto>();
                newNote.KAttributesDto = await CompleteNoteAttributes(newNote.KAttributesDto, newNote.NoteId);

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
                    UpdateStandardValuesToNewEntity(newEntity);

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

                    var entityU = await _repository.Notes.DbSet.Where(n => n.NoteId == entity.NoteId)
                        .Include(n => n.KAttributes).ThenInclude(n => n.KAttribute)
                        .Include(n => n.Folder)
                        .Include(n => n.NoteType)
                        .SingleOrDefaultAsync();
                    var entityForUpdate = entityU;

                    if (flagThrowKntException == true)
                        _repository.Notes.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        entityForUpdate.SetSimpleDto(entity);
                        entityForUpdate.ModificationDateTime = DateTime.Now;
                        // Delete deprecated atttibutes
                        entityForUpdate.KAttributes.RemoveAll(_ => _.KAttribute.NoteTypeId != entityForUpdate.NoteTypeId && _.KAttribute.NoteTypeId != null);                        

                        resRep = await _repository.Notes.UpdateAsync(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new Note();
                        newEntity.SetSimpleDto(entity);
                        UpdateStandardValuesToNewEntity(newEntity);

                        resRep = await _repository.Notes.AddAsync(newEntity);
                    }
                }

                resService.Entity = resRep.Entity?.GetSimpleDto<NoteDto>();
               
                foreach (NoteKAttributeDto atr in entity.KAttributesDto)
                {                                        
                    var res = await SaveAttrtibuteAsync(atr);                    
                    // TODO: !!! Importante !!! pendiente de capturar y volcar errores de res en resService
                    resService.Entity.KAttributesDto.Add(res.Entity);                    
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

        public async Task<Result<List<ResourceDto>>> GetNoteResourcesAsync(Guid idNote)
        {
            var resService = new Result<List<ResourceDto>>();
            try
            {
                var resRep = await _repository.Resources.GetAllAsync(r => r.NoteId == idNote);
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

        public async Task<Result<List<NoteTaskDto>>> GetNoteTasksAsync(Guid idNote)
        {
            var resService = new Result<List<NoteTaskDto>>();
            try
            {
                // TODO: !!! Borrar ...
                //var resRep = _repository.NoteTasks.GetAll(r => r.NoteId == idNote);
                //resService.Entity = resRep.Entity?.Select(u => u.GetSimpleDto<NoteTaskDto>()).ToList();
                //resService.ErrorList = resRep.ErrorList;

                var listTasks = await _repository.NoteTasks.DbSet.Where(n => n.NoteId == idNote)                    
                    .Include(t => t.User)                    
                    .ToListAsync();
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

        //public async Task<Result<WindowDto>> SaveWindowAsync(WindowDto entity)
        //{
        //    Result<Window> resRep = null;
        //    var resService = new Result<WindowDto>();

        //    try
        //    {
        //        if (entity.WindowId == Guid.Empty)
        //        {
        //            entity.WindowId = Guid.NewGuid();
        //            var newEntity = new Window();
        //            newEntity.SetSimpleDto(entity);

        //            // TODO: update standard control values to newEntity
        //            // ...

        //            resRep = await _repository.Windows.AddAsync(newEntity);
        //        }
        //        else
        //        {
        //            bool flagThrowKntException = false;

        //            if (_repository.Windows.ThrowKntException == true)
        //            {
        //                flagThrowKntException = true;
        //                _repository.Windows.ThrowKntException = false;
        //            }

        //            var entityForUpdate = (await _repository.Windows.GetAsync(entity.WindowId)).Entity;

        //            if (flagThrowKntException == true)
        //                _repository.Windows.ThrowKntException = true;

        //            if (entityForUpdate != null)
        //            {
        //                // TODO: update standard control values to entityForUpdate
        //                // ...
        //                entityForUpdate.SetSimpleDto(entity);
        //                resRep = await _repository.Windows.UpdateAsync(entityForUpdate);
        //            }
        //            else
        //            {
        //                var newEntity = new Window();
        //                newEntity.SetSimpleDto(entity);

        //                // TODO: update standard control values to newEntity
        //                // ...

        //                resRep = await _repository.Windows.AddAsync(newEntity);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
        //    }

        //    // TODO: Valorar refactorizar los siguiente (este patrón está en varios sitios.
        //    resService.Entity = resRep.Entity?.GetSimpleDto<WindowDto>();
        //    resService.ErrorList = resRep.ErrorList;

        //    return ResultDomainAction(resService);
        //}
        
        //public async Task<Result<TraceNoteDto>> SaveTraceNoteAsync(TraceNoteDto entity)
        //{
        //    Result<TraceNote> resRep = null;
        //    var resService = new Result<TraceNoteDto>();

        //    try
        //    {
        //        if (entity.TraceNoteId == Guid.Empty)
        //        {
        //            entity.TraceNoteId = Guid.NewGuid();
        //            var newEntity = new TraceNote();
        //            newEntity.SetSimpleDto(entity);

        //            // TODO: update standard control values to newEntity
        //            // ...

        //            resRep = await _repository.TraceNotes.AddAsync(newEntity);
        //        }
        //        else
        //        {
        //            bool flagThrowKntException = false;

        //            if (_repository.TraceNotes.ThrowKntException == true)
        //            {
        //                flagThrowKntException = true;
        //                _repository.TraceNotes.ThrowKntException = false;
        //            }

        //            var entityForUpdate = (await _repository.TraceNotes.GetAsync(entity.TraceNoteId)).Entity;

        //            if (flagThrowKntException == true)
        //                _repository.TraceNotes.ThrowKntException = true;

        //            if (entityForUpdate != null)
        //            {
        //                // TODO: update standard control values to entityForUpdate
        //                // ...
        //                entityForUpdate.SetSimpleDto(entity);
        //                resRep = await _repository.TraceNotes.UpdateAsync(entityForUpdate);
        //            }
        //            else
        //            {
        //                var newEntity = new TraceNote();
        //                newEntity.SetSimpleDto(entity);

        //                // TODO: update standard control values to newEntity
        //                // ...

        //                resRep = await _repository.TraceNotes.AddAsync(newEntity);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
        //    }

        //    // TODO: Valorar refactorizar los siguiente (este patrón está en varios sitios.
        //    resService.Entity = resRep.Entity?.GetSimpleDto<TraceNoteDto>();
        //    resService.ErrorList = resRep.ErrorList;

        //    return ResultDomainAction(resService);
        //}

        #endregion

        #endregion

        #region Utils methods 

        private int ExtractNoteNumberSearch(string textSearch)
        {
            if (textSearch == null)
                return 0;

            var n = 0;
            string strStartNumber = "";

            if (textSearch[0] == '#')
            {
                var i = textSearch.IndexOf(' ', 0);
                if (i > 0)
                    strStartNumber = textSearch.Substring(1, i - 1);
                else
                    strStartNumber = textSearch.Substring(1, textSearch.Length - 1);
                int.TryParse(strStartNumber, out n);
            }

            return n;
        }

        private List<string> ExtractListTokensSearch(string textIn)
        {
            List<string> tokens = new List<string>();
            int i = 0, lenString = 0;
            int state = 0;
            char c;
            string word = "";
            char action = 'a';
            string especialToken = "";

            if (textIn == null)
                return tokens;

            lenString = textIn.Length;

            while (i < lenString)
            {
                c = textIn[i];

                switch (c)
                {
                    case '\"':
                        if (state == 0)
                        {
                            state = 1;
                            action = 'p';
                        }
                        else
                        {
                            state = 0;
                            action = 'p';
                        }
                        break;
                    case ' ':
                        if (state == 0 || state == 2)
                            action = 'p';
                        break;
                    default:
                        action = 'a';
                        break;
                }

                if (action == 'p')
                {
                    if (word != "")
                    {
                        // Si es un Token especial => va con la siguiente palabra
                        if (word == "!")
                            especialToken = word;
                        else
                        {
                            word = especialToken + word;
                            especialToken = "";
                            tokens.Add(word);
                        }
                    }
                    word = "";
                }
                if (action == 'a')
                    word += c;

                i++;
            }

            if (word != "")
                tokens.Add(word);

            return tokens;
        }

        private async Task<List<NoteKAttributeDto>> CompleteNoteAttributes(List<NoteKAttributeDto> attributesNotes, Guid noteId, Guid? noteTypeId = null)
        {
            var attributes = (await _repository.KAttributes.GetAllAsync(_ => _.NoteTypeId == null || _.NoteTypeId == noteTypeId)).Entity;
            foreach (KAttribute a in attributes)
            {
                var atrTmp = attributesNotes
                    .Where(na => na.KAttributeId == a.KAttributeId)
                    .Select(at => at).SingleOrDefault();
                if (atrTmp == null)
                {
                    attributesNotes.Add(new NoteKAttributeDto
                    {
                        KAttributeId = a.KAttributeId,
                        NoteId = noteId,
                        Value = "",
                        Name = a.Name,
                        Description = a.Description,
                        KAttributeDataType = a.KAttributeDataType,
                        KAttributeNoteTypeId = a.NoteTypeId,
                        RequiredValue = a.RequiredValue,
                        Order = a.Order,
                        Script = a.Script,
                        Disabled = a.Disabled
                    });
                }
                else
                {
                    atrTmp.Name = a.Name;
                    atrTmp.Description = a.Description;
                    atrTmp.KAttributeDataType = a.KAttributeDataType;
                    atrTmp.KAttributeNoteTypeId = a.NoteTypeId;
                    atrTmp.RequiredValue = a.RequiredValue;
                    atrTmp.Order = a.Order;
                    atrTmp.Script = a.Script;
                    atrTmp.Disabled = a.Disabled;
                }
            }
            return attributesNotes.OrderBy(_ => _.Order).ThenBy(_ => _.Name).ToList();
        }

        private void UpdateStandardValuesToNewEntity(Note newEntity)
        {
            newEntity.NoteNumber = GetNextNoteNumber();
            if (newEntity.CreationDateTime == DateTime.MinValue)
                newEntity.CreationDateTime = DateTime.Now;

            if (newEntity.ModificationDateTime == DateTime.MinValue)
                newEntity.ModificationDateTime = DateTime.Now;
        }

        #endregion


    }

    public class SearchTokens
    {
        public int NoteNumber { get; set; }

        public List<string> TextTokens { get; set; } = new List<string>();

        public bool SearchInDescription { get; set; }
    }

}
