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
                result.Entity.NoteTypeDto = entity?.NoteType?.GetSimpleDto<NoteTypeDto>();
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
                newNote.ModificationDateTime = DateTime.Now;
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

        public async Task<Result<NoteDto>> AddAsync(NoteDto entity)
        {
            //return await SaveAsync(entity);
            Result<Note> resRep = null;
            var result = new Result<NoteDto>();

            try
            {
                var newEntity = new Note();
                UpdateStandardValuesToNewEntity(entity);
                newEntity.SetSimpleDto(entity);
                
                resRep = await _notes.AddAsync(newEntity);
                // TODO: !!! Importante !!! pendiente de capturar y volcar errores de res en resService

                // Complete Attributes list
                //result.Entity.KAttributesDto = await CompleteNoteAttributes(result.Entity.KAttributesDto, entity.NoteId, entity.NoteTypeId);

                // TODO: Limpiar lo siguiente, está sucio ...

                foreach (NoteKAttributeDto atr in entity.KAttributesDto)
                {
                    atr.NoteId = entity.NoteId;
                    var res = (await SaveAttrtibuteAsync(atr)).Entity;
                    // TODO: !!! Importante !!! pendiente de capturar y volcar errores de res en resService
                    atr.KAttributeId = res.KAttributeId;
                    atr.NoteKAttributeId = res.NoteKAttributeId;
                }

                result.Entity = entity;                
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }

            result.ErrorList = resRep.ErrorList;
            return ResultDomainAction(result);
        }

        public async Task<Result<NoteDto>> UpdateAsync(NoteDto entity)
        {
            Result<Note> resRep = null;
            var result = new Result<NoteDto>();

            try
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
                    resRep.Entity = null;
                    resRep.AddErrorMessage("Can't find entity for update.");
                }
                                

                // TODO: Limiar lo siguiente está sucio ...

                result.Entity = resRep.Entity?.GetSimpleDto<NoteDto>();

                foreach (NoteKAttributeDto atr in entity.KAttributesDto)
                {
                    var res = await SaveAttrtibuteAsync(atr);
                    // TODO: !!! Importante !!! pendiente de capturar y volcar errores de res en resService
                    result.Entity.KAttributesDto.Add(res.Entity);
                }

            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resRep.ErrorList);
            }
            
            result.ErrorList = resRep.ErrorList;
            return ResultDomainAction(result);
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            var response = new Result();
            try
            {
                var resGenRep = await _notes.DeleteAsync(id);
                if (!resGenRep.IsValid)
                    response.ErrorList = resGenRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);

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

        public async Task<Result<ResourceDto>> AddResourceAsync(ResourceDto entity)
        {
            var response = new Result<ResourceDto>();
            try
            {
                var newEntity = new Resource();
                newEntity.SetSimpleDto(entity);
                // TODO: refactorizar la sigueinte línea (generalizar)
                newEntity.Container = @"NotesResources\" + DateTime.Now.Year.ToString();
                newEntity.ContentArrayBytes = Convert.FromBase64String(entity.ContentBase64);

                var resGenRep = await _resources.AddAsync(newEntity);

                response.Entity = resGenRep.Entity?.GetSimpleDto<ResourceDto>();
                if (response.Entity != null)
                    response.Entity.ContentBase64 = entity.ContentBase64;

                response.ErrorList = resGenRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);
        }

        public async Task<Result<ResourceDto>> UpdateResourceAsync(ResourceDto entity)
        {
            // TODO: Pendiente de probar 
            var resGenRep = new Result<Resource>();
            var response = new Result<ResourceDto>();

            try
            {
                bool flagThrowKntException = false;
                if (_resources.ThrowKntException == true)
                {
                    flagThrowKntException = true;
                    _resources.ThrowKntException = false;
                }

                var resGenRepGet = await _resources.GetAsync(entity.ResourceId);
                Resource entityForUpdate;

                if (flagThrowKntException == true)
                    _resources.ThrowKntException = true;

                if (resGenRepGet.IsValid)
                {
                    entityForUpdate = resGenRepGet.Entity;
                    entityForUpdate.SetSimpleDto(entity);
                    entityForUpdate.ContentArrayBytes = Convert.FromBase64String(entity.ContentBase64);
                    resGenRep = await _resources.UpdateAsync(entityForUpdate);
                }
                else
                {
                    resGenRep.Entity = null;
                    resGenRep.AddErrorMessage("Can't find entity for update.");
                }

                response.Entity = resGenRep.Entity?.GetSimpleDto<ResourceDto>();
                if (response.Entity != null)
                    response.Entity.ContentBase64 = entity.ContentBase64;

                response.ErrorList = resGenRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }

            return ResultDomainAction(response);
        }

        public async Task<Result> DeleteResourceAsync(Guid id)
        {
            var response = new Result();
            try
            {
                var resGenRep = await _resources.DeleteAsync(id);
                if (!resGenRep.IsValid)
                    response.ErrorList = resGenRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);
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

        public async Task<Result<NoteTaskDto>> AddNoteTaskAsync(NoteTaskDto entity)
        {
            var response = new Result<NoteTaskDto>();
            try
            {
                var newEntity = new NoteTask();
                newEntity.SetSimpleDto(entity);                
                newEntity.CreationDateTime = DateTime.Now;
                newEntity.ModificationDateTime = DateTime.Now;
                
                var resGenRep = await _noteTasks.AddAsync(newEntity);

                response.Entity = resGenRep.Entity?.GetSimpleDto<NoteTaskDto>();
                if (response.Entity != null)
                    response.Entity.UserFullName = entity.UserFullName;

                response.ErrorList = resGenRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);
        }

        public async Task<Result<NoteTaskDto>> UpdateNoteTaskAsync(NoteTaskDto entity)
        {
            var resGenRep = new Result<NoteTask>();
            var response = new Result<NoteTaskDto>();

            try
            {
                bool flagThrowKntException = false;
                if (_noteTasks.ThrowKntException == true)
                {
                    flagThrowKntException = true;
                    _noteTasks.ThrowKntException = false;
                }

                var resGenRepGet = await _noteTasks.GetAsync(entity.NoteTaskId);
                NoteTask entityForUpdate;

                if (flagThrowKntException == true)
                    _noteTasks.ThrowKntException = true;

                if (resGenRepGet.IsValid)
                {
                    entityForUpdate = resGenRepGet.Entity;
                    entityForUpdate.SetSimpleDto(entity);
                    entityForUpdate.ModificationDateTime = DateTime.Now;
                    resGenRep = await _noteTasks.UpdateAsync(entityForUpdate);
                }
                else
                {
                    resGenRep.Entity = null;
                    resGenRep.AddErrorMessage("Can't find entity for update.");
                }

                response.Entity = resGenRep.Entity?.GetSimpleDto<NoteTaskDto>();
                if (response.Entity != null)
                    response.Entity.UserFullName = entity.UserFullName;
                response.ErrorList = resGenRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }

            return ResultDomainAction(response);
        }

        public async Task<Result> DeleteNoteTaskAsync(Guid id)
        {
            var response = new Result();
            try
            {
                var resGenRep = await _noteTasks.DeleteAsync(id);
                if (!resGenRep.IsValid)
                    response.ErrorList = resGenRep.ErrorList;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);
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

        private void UpdateStandardValuesToNewEntity(NoteDto newEntity)
        {
            newEntity.NoteNumber = GetNextNoteNumber();
            if (newEntity.CreationDateTime == DateTime.MinValue)
                newEntity.CreationDateTime = DateTime.Now;

            if (newEntity.ModificationDateTime == DateTime.MinValue)
                newEntity.ModificationDateTime = DateTime.Now;
        }

        private async Task<Result<NoteKAttributeDto>> SaveAttrtibuteAsync(NoteKAttributeDto entity)
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

        #endregion
    }
}
