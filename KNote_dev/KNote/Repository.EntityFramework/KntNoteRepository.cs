using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository.Entities;

namespace KNote.Repository.EntityFramework
{
    public class KntNoteRepository: DomainActionBase, IKntNoteRepository
    {
        private IGenericRepositoryEF<KntDbContext, Note> _notes;
        private IGenericRepositoryEF<KntDbContext, NoteKAttribute> _noteKAttributes;
        private IGenericRepositoryEF<KntDbContext, Resource> _resources;
        private IGenericRepositoryEF<KntDbContext, NoteTask> _noteTasks;

        private IKntFolderRepository _folders;
        private IKntKAttributeRepository _kattributes;

        public KntNoteRepository(KntDbContext context, bool throwKntException)
        {
            _notes = new GenericRepositoryEF<KntDbContext, Note>(context, throwKntException);

            _noteKAttributes = new GenericRepositoryEF<KntDbContext, NoteKAttribute>(context, throwKntException);
            _noteTasks = new GenericRepositoryEF<KntDbContext, NoteTask>(context, throwKntException);
            _resources = new GenericRepositoryEF<KntDbContext, Resource>(context, throwKntException);
            _folders = new KntFolderRepository(context, throwKntException);
            _kattributes = new KntKAttributeRepository(context, throwKntException);

            ThrowKntException = throwKntException;
        }

        public async Task<Result<List<NoteInfoDto>>> GetAllAsync()
        {
            var resService = new Result<List<NoteInfoDto>>();
            try
            {
                var resRep = await _notes.GetAllAsync();
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
                var idFolderHome = (await _folders.GetHomeAsync()).Entity?.FolderId;

                var resRep = await _notes.DbSet
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

        public async Task<Result<List<NoteInfoDto>>> GetByFolderAsync(Guid folderId)
        {
            var resService = new Result<List<NoteInfoDto>>();
            try
            {
                var resRep = await _notes.GetAllAsync(n => n.FolderId == folderId);
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
                var query = _notes.Queryable;

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

                foreach (var f in notesFilter.AttributesFilter)
                {
                    //query = query.Where(n => n.KAttributes.Select(a => a.Value).Contains(f.Value));
                    query = query.Where(n => n.KAttributes.Where(_ => _.KAttributeId == f.AtrId).Select(a => a.Value).Contains(f.Value));
                }

                resService.CountColecEntity = await query.CountAsync();

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
            var result = new Result<List<NoteInfoDto>>();
            int searchNumber;

            try
            {
                var query = _notes.Queryable;

                searchNumber = ExtractNoteNumberSearch(notesSearch.TextSearch);

                if (searchNumber > 0)
                    query = query.Where(n => n.NoteNumber == searchNumber);

                else
                {
                    var listTokensAll = ExtractListTokensSearch(notesSearch.TextSearch);
                    var listTokens = listTokensAll.Where(t => t != "***").Select(t => t).ToList();
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

                result.CountColecEntity = await query.CountAsync();

                // Order by and pagination
                query = query
                    .OrderBy(n => n.Priority).ThenBy(n => n.Topic)
                    .Pagination(notesSearch.Pagination);

                // Get content
                result.Entity = await query
                    .Select(u => u.GetSimpleDto<NoteInfoDto>())
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<NoteDto>> GetAsync(Guid noteId)
        {
            var result = new Result<NoteDto>();
            try
            {
                var entity = await _notes.DbSet.Where(n => n.NoteId == noteId)
                    .Include(n => n.KAttributes).ThenInclude(n => n.KAttribute)
                    .Include(n => n.Folder)
                    .Include(n => n.NoteType)
                    .SingleOrDefaultAsync();

                // Map to dto
                result.Entity = entity?.GetSimpleDto<NoteDto>();
                result.Entity.FolderDto = entity?.Folder.GetSimpleDto<FolderDto>();
                result.Entity.NoteTypeDto = entity?.NoteType.GetSimpleDto<NoteTypeDto>();
                result.Entity.KAttributesDto = entity?.KAttributes
                    .Select(_ => _.GetSimpleDto<NoteKAttributeDto>())
                    .Where(_ => _.KAttributeNoteTypeId == null || _.KAttributeNoteTypeId == result.Entity.NoteTypeId)
                    .ToList();

                // Complete Attributes list
                result.Entity.KAttributesDto = await CompleteNoteAttributes(result.Entity.KAttributesDto, entity.NoteId, entity.NoteTypeId);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<NoteDto>> NewAsync(NoteInfoDto entity = null)
        {
            var result = new Result<NoteDto>();
            NoteDto newNote;

            try
            {
                newNote = new NoteDto();
                if (entity != null)
                    newNote.SetSimpleDto(entity);

                //if (newNote.NoteId == Guid.Empty)
                //    newNote.NoteId = Guid.NewGuid();
                newNote.IsNew = true;
                newNote.CreationDateTime = DateTime.Now;
                newNote.KAttributesDto = new List<NoteKAttributeDto>();
                newNote.KAttributesDto = await CompleteNoteAttributes(newNote.KAttributesDto, newNote.NoteId);

                result.Entity = newNote;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }

            return ResultDomainAction(result);
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

                    resRep = await _notes.AddAsync(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_notes.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _notes.ThrowKntException = false;
                    }

                    var entityU = await _notes.DbSet.Where(n => n.NoteId == entity.NoteId)
                        .Include(n => n.KAttributes).ThenInclude(n => n.KAttribute)
                        .Include(n => n.Folder)
                        .Include(n => n.NoteType)
                        .SingleOrDefaultAsync();
                    var entityForUpdate = entityU;

                    if (flagThrowKntException == true)
                        _notes.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        entityForUpdate.SetSimpleDto(entity);
                        entityForUpdate.ModificationDateTime = DateTime.Now;
                        // Delete deprecated atttibutes
                        entityForUpdate.KAttributes.RemoveAll(_ => _.KAttribute.NoteTypeId != entityForUpdate.NoteTypeId && _.KAttribute.NoteTypeId != null);

                        resRep = await _notes.UpdateAsync(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new Note();
                        newEntity.SetSimpleDto(entity);
                        UpdateStandardValuesToNewEntity(newEntity);

                        resRep = await _notes.AddAsync(newEntity);
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
                var resRep = await _notes.GetAsync(id);
                if (resRep.IsValid)
                {
                    resRep = await _notes.DeleteAsync(resRep.Entity);
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

                    resRep = await _noteKAttributes.AddAsync(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_noteKAttributes.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _noteKAttributes.ThrowKntException = false;
                    }

                    var entityForUpdate = (await _noteKAttributes.GetAsync(entity.NoteKAttributeId)).Entity;

                    if (flagThrowKntException == true)
                        _noteKAttributes.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entity);
                        resRep = await _noteKAttributes.UpdateAsync(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new NoteKAttribute();
                        newEntity.SetSimpleDto(entity);

                        // TODO: update standard control values to newEntity
                        // ...

                        resRep = await _noteKAttributes.AddAsync(newEntity);
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

                    resRep = await _resources.AddAsync(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_resources.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _resources.ThrowKntException = false;
                    }

                    var entityForUpdate = _resources.Get(entity.ResourceId).Entity;

                    if (flagThrowKntException == true)
                        _resources.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate
                        // ...
                        entityForUpdate.SetSimpleDto(entity);
                        entityForUpdate.ContentArrayBytes = Convert.FromBase64String(entity.ContentBase64);
                        resRep = await _resources.UpdateAsync(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new Resource();
                        newEntity.SetSimpleDto(entity);

                        // TODO: update standard control values to newEntity  (refactor ...)
                        // ...
                        newEntity.Container = @"NotesResources\" + DateTime.Now.Year.ToString();
                        newEntity.ContentArrayBytes = Convert.FromBase64String(entity.ContentBase64);

                        resRep = await _resources.AddAsync(newEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            // TODO: Valorar refactorizar los siguiente (este patrón está en varios sitios.
            resService.Entity = resRep.Entity?.GetSimpleDto<ResourceDto>();
            if (resService.Entity != null)
                resService.Entity.ContentBase64 = entity.ContentBase64;

            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }

        public async Task<Result<List<ResourceDto>>> GetNoteResourcesAsync(Guid idNote)
        {
            var result = new Result<List<ResourceDto>>();
            try
            {
                var resRep = await _resources.GetAllAsync(r => r.NoteId == idNote);
                result.Entity = resRep.Entity?.Select(u => u.GetSimpleDto<ResourceDto>()).ToList();
                result.ErrorList = resRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<ResourceDto>> DeleteResourceAsync(Guid id)
        {
            var resService = new Result<ResourceDto>();
            try
            {
                var resRep = await _resources.GetAsync(id);
                if (resRep.IsValid)
                {
                    resRep = await _resources.DeleteAsync(resRep.Entity);
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

                    resRep = await _noteTasks.AddAsync(newEntity);
                }
                else
                {
                    bool flagThrowKntException = false;

                    if (_noteTasks.ThrowKntException == true)
                    {
                        flagThrowKntException = true;
                        _noteTasks.ThrowKntException = false;
                    }

                    var entityForUpdate = _noteTasks.Get(entityInfo.NoteTaskId).Entity;

                    if (flagThrowKntException == true)
                        _noteTasks.ThrowKntException = true;

                    if (entityForUpdate != null)
                    {
                        // TODO: update standard control values to entityForUpdate                                                
                        entityForUpdate.SetSimpleDto(entityInfo);
                        entityForUpdate.ModificationDateTime = DateTime.Now;
                        // ...

                        resRep = await _noteTasks.UpdateAsync(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new NoteTask();
                        newEntity.SetSimpleDto(entityInfo);

                        // TODO: update standard control values to newEntity
                        newEntity.CreationDateTime = DateTime.Now;
                        newEntity.ModificationDateTime = DateTime.Now;
                        // ...

                        resRep = await _noteTasks.AddAsync(newEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }

            // TODO: Valorar refactorizar los siguiente (este patrón está en varios sitios.
            resService.Entity = resRep.Entity?.GetSimpleDto<NoteTaskDto>();
            if (resRep.Entity != null)
                resService.Entity.UserFullName = userFullName;
            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }

        public async Task<Result<List<NoteTaskDto>>> GetNoteTasksAsync(Guid idNote)
        {
            var result = new Result<List<NoteTaskDto>>();
            try
            {
                var listTasks = await _noteTasks.DbSet.Where(n => n.NoteId == idNote)
                    .Include(t => t.User)
                    .ToListAsync();
                result.Entity = new List<NoteTaskDto>();
                foreach (var e in listTasks)
                {
                    var nt = e.GetSimpleDto<NoteTaskDto>();
                    nt.UserFullName = e.User.FullName;
                    result.Entity.Add(nt);
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<NoteTaskDto>> DeleteNoteTaskAsync(Guid id)
        {
            var resService = new Result<NoteTaskDto>();
            try
            {
                var resRep = await _noteTasks.GetAsync(id);
                if (resRep.IsValid)
                {
                    resRep = await _noteTasks.DeleteAsync(resRep.Entity);
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

        #region  IDisposable

        public virtual void Dispose()
        {
            _notes.Dispose();

            _noteKAttributes.Dispose();
            _noteTasks.Dispose();
            _resources.Dispose();
        }

        #endregion

        #region Utils methods 

        private int GetNextNoteNumber()
        {
            //var lastNote = _repository.Context.Notes.OrderByDescending(n => n.NoteNumber).FirstOrDefault();
            var lastNote = _notes
                .DbSet.OrderByDescending(n => n.NoteNumber).FirstOrDefault();
            return lastNote != null ? lastNote.NoteNumber + 1 : 1;
        }

        private async Task<List<NoteKAttributeDto>> CompleteNoteAttributes(List<NoteKAttributeDto> attributesNotes, Guid noteId, Guid? noteTypeId = null)
        {                        
            // TODO: pendiente de refactorizar este método 
            var attributes = (await _kattributes.GetAllIncludeNullTypeAsync(noteTypeId)).Entity;
            foreach (KAttributeInfoDto a in attributes)
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
}
