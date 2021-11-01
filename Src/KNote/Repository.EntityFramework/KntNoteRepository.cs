using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository.Entities;
using System.Transactions;

namespace KNote.Repository.EntityFramework
{
    public class KntNoteRepository: KntRepositoryBase, IKntNoteRepository
    {
        #region Constructor

        public KntNoteRepository(KntDbContext singletonContext, RepositoryRef repositoryRef)
            : base(singletonContext, repositoryRef)
        {
        }

        public KntNoteRepository(RepositoryRef repositoryRef)
            : base(repositoryRef)
        {
        }

        #endregion

        #region IKntNoteRepository implementation

        public async Task<Result<List<NoteInfoDto>>> GetAllAsync()
        {
            var resService = new Result<List<NoteInfoDto>>();
            try
            {
                var ctx = GetOpenConnection();
                var notes = new GenericRepositoryEF<KntDbContext, Note>(ctx);

                var resRep = await notes.GetAllAsync();
                resService.Entity = resRep.Entity?
                    .Select(_ => _.GetSimpleDto<NoteInfoDto>())
                    .OrderBy(_ => _.Priority).ThenBy(_ => _.Topic)
                    .ToList();
                resService.ErrorList = resRep.ErrorList;

                await CloseIsTempConnection(ctx);
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
                var ctx = GetOpenConnection();
                var notes = new GenericRepositoryEF<KntDbContext, Note>(ctx);
                var folders = new KntFolderRepository(ctx, _repositoryRef);

                var idFolderHome = (await folders.GetHomeAsync()).Entity?.FolderId;

                var resRep = await notes.DbSet
                    .Include(n => n.Folder)
                    .Where(n => n.FolderId == idFolderHome)
                    .OrderBy(n => n.Priority).ThenBy(n => n.Topic)
                    .Take(25).ToListAsync();

                resService.Entity = resRep.Select(u => u.GetSimpleDto<NoteInfoDto>()).ToList();

                await CloseIsTempConnection(ctx);
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
                var ctx = GetOpenConnection();
                var notes = new GenericRepositoryEF<KntDbContext, Note>(ctx);

                var resRep = await notes.GetAllAsync(n => n.FolderId == folderId);
                resService.Entity = resRep.Entity?.Select(n => n.GetSimpleDto<NoteInfoDto>()).ToList();
                resService.ErrorList = resRep.ErrorList; ;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<List<NoteInfoDto>>> GetFilter(NotesFilterDto notesFilter)
        {            
            var resService = new Result<List<NoteInfoDto>>();
            try
            {
                var ctx = GetOpenConnection();
                var notes = new GenericRepositoryEF<KntDbContext, Note>(ctx);

                var query = notes.Queryable;

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
                    query = query.Where(n => n.KAttributes.Where(_ => _.KAttributeId == f.AtrId).Select(a => a.Value).Contains(f.Value));
                }

                resService.TotalCount = await query.CountAsync();

                // Order by and pagination
                query = query
                    .OrderBy(n => n.Priority).ThenBy(n => n.Topic)
                    .Pagination(notesFilter.PageIdentifier);

                // Get content
                resService.Entity = await query
                    .Select(u => u.GetSimpleDto<NoteInfoDto>())
                    .ToListAsync();

                await CloseIsTempConnection(ctx);
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
                var ctx = GetOpenConnection();
                var notes = new GenericRepositoryEF<KntDbContext, Note>(ctx);

                var query = notes.Queryable;

                searchNumber = ExtractNoteNumberSearch(notesSearch.TextSearch);

                if (searchNumber > 0)
                    query = query.Where(n => n.NoteNumber == searchNumber);

                else
                {
                    var listTokensAll = ExtractListTokensSearch(notesSearch.TextSearch);

                    // TODO: refactor this -----------------------------
                    var listTokens = listTokensAll.Where(t => t != "***").Select(t => t).ToList();
                    var flagTextSearchDescription = (listTokensAll.Where(t => t == "***").Select(t => t).FirstOrDefault());                    
                    bool flagSearchDescription = (flagTextSearchDescription == "***") || notesSearch.SearchInDescription;
                    // --------------------------------------------------
                    
                    if (!flagSearchDescription)
                    {
                        foreach (var token in listTokens)
                        {
                            if (!string.IsNullOrEmpty(token))
                            {
                                if (token[0] != '!')
                                    query = query.Where(n => n.Topic.ToLower().Contains(token.ToLower()) || n.Tags.ToLower().Contains(token.ToLower()));
                                else
                                {
                                    var tokenNot = token.Substring(1, token.Length - 1);
                                    query = query.Where(n => !n.Topic.ToLower().Contains(tokenNot.ToLower()) && !n.Tags.ToLower().Contains(tokenNot.ToLower()));
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
                                    query = query.Where(n => n.Topic.ToLower().Contains(token.ToLower()) || n.Tags.ToLower().Contains(token.ToLower()) || n.Description.ToLower().Contains(token.ToLower()));
                                else
                                {
                                    var tokenNot = token.Substring(1, token.Length - 1);
                                    query = query.Where(n => !n.Topic.ToLower().Contains(tokenNot.ToLower()) && !n.Tags.ToLower().Contains(tokenNot.ToLower()) && !n.Description.ToLower().Contains(tokenNot.ToLower()));
                                }
                            }
                        }
                    }
                }

                result.TotalCount = await query.CountAsync();

                // Order by and pagination
                query = query
                    .OrderBy(n => n.Priority).ThenBy(n => n.Topic)
                    .Pagination(notesSearch.PageIdentifier);

                // Get content
                result.Entity = await query
                    .Select(u => u.GetSimpleDto<NoteInfoDto>())
                    .ToListAsync();

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<NoteDto>> GetAsync(Guid noteId)
        {
            return await GetAsync(noteId, null);
        }

        public async Task<Result<NoteDto>> GetAsync(int noteNumber)
        {
            return await GetAsync(null, noteNumber);
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
                newNote.SetIsNew(true);
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
            Result<Note> resRep = null;
            var result = new Result<NoteDto>();

            try
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var ctx = GetOpenConnection();
                    var notes = new GenericRepositoryEF<KntDbContext, Note>(ctx);

                    var newEntity = new Note();
                    UpdateStandardValuesToNewEntity(notes, entity);
                    newEntity.SetSimpleDto(entity);
                
                    resRep = await notes.AddAsync(newEntity);
                    if (!resRep.IsValid)
                        ExceptionHasHappened = true;
                
                    foreach (NoteKAttributeDto atr in entity.KAttributesDto)
                    {
                        atr.NoteId = entity.NoteId;
                        var res = (await SaveAttrtibuteAsync(ctx, atr)).Entity;
                        // TODO: !!! Importante, pendiente de capturar y volcar errores de res en resService
                        atr.KAttributeId = res.KAttributeId;
                        atr.NoteKAttributeId = res.NoteKAttributeId;
                    }

                    result.Entity = entity;
                    result.ErrorList = resRep.ErrorList;

                    scope.Complete();

                    await CloseIsTempConnection(ctx);
                }
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            
            return ResultDomainAction(result);
        }

        public async Task<Result<NoteDto>> UpdateAsync(NoteDto entity)
        {
            Result<Note> resRep = null;
            var result = new Result<NoteDto>();

            try
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var ctx = GetOpenConnection();
                    var notes = new GenericRepositoryEF<KntDbContext, Note>(ctx);                

                    var entityU = await notes.DbSet.Where(n => n.NoteId == entity.NoteId)
                        .Include(n => n.KAttributes).ThenInclude(n => n.KAttribute)
                        .Include(n => n.Folder)
                        .Include(n => n.NoteType)
                        .SingleOrDefaultAsync();
                    var entityForUpdate = entityU;

                    if (entityForUpdate != null)
                    {
                        entity.ModificationDateTime = DateTime.Now;
                        entityForUpdate.SetSimpleDto(entity);                    
                        // Delete deprecated atttibutes
                        entityForUpdate.KAttributes.RemoveAll(_ => _.KAttribute.NoteTypeId != entityForUpdate.NoteTypeId && _.KAttribute.NoteTypeId != null);

                        resRep = await notes.UpdateAsync(entityForUpdate);
                    }
                    else
                    {
                        resRep.Entity = null;
                        resRep.AddErrorMessage("Can't find entity for update.");
                    }
                                
                    // TODO: Limiar lo siguiente está sucio ...

                    result.Entity = resRep.Entity?.GetSimpleDto<NoteDto>();
                    result.Entity.FolderDto = entity.FolderDto;

                    foreach (NoteKAttributeDto atr in entity.KAttributesDto)
                    {
                        var res = await SaveAttrtibuteAsync(ctx, atr);
                        // TODO: !!! Importante !!! pendiente de capturar y volcar errores de res en resService
                        result.Entity.KAttributesDto.Add(res.Entity);
                    }

                    scope.Complete();

                    await CloseIsTempConnection(ctx);
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
                var ctx = GetOpenConnection();
                var notes = new GenericRepositoryEF<KntDbContext, Note>(ctx);

                var resGenRep = await notes.DeleteAsync(id);
                if (!resGenRep.IsValid)
                    response.ErrorList = resGenRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);
        }

        public async Task<Result<List<ResourceDto>>> GetResourcesAsync(Guid idNote)
        {
            var result = new Result<List<ResourceDto>>();
            try
            {
                var ctx = GetOpenConnection();                
                var resources = new GenericRepositoryEF<KntDbContext, Resource>(ctx);

                var resRep = await resources.GetAllAsync(r => r.NoteId == idNote);
                result.Entity = resRep.Entity?.Select(u => u.GetSimpleDto<ResourceDto>()).OrderBy( r => r.Order).ThenBy( r => r.Name).ToList();
                result.ErrorList = resRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async  Task<Result<ResourceDto>> GetResourceAsync(Guid idNoteResource)
        {            
            var result = new Result<ResourceDto>();
            try
            {
                var ctx = GetOpenConnection();
                var resources = new GenericRepositoryEF<KntDbContext, Resource>(ctx);

                var resRep = await resources.GetAsync(r => r.ResourceId == idNoteResource);
                result.Entity = resRep.Entity.GetSimpleDto<ResourceDto>(); 
                result.ErrorList = resRep.ErrorList;

                await CloseIsTempConnection(ctx);
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
                var ctx = GetOpenConnection();
                var resources = new GenericRepositoryEF<KntDbContext, Resource>(ctx);

                var newEntity = new Resource();
                newEntity.SetSimpleDto(entity);
                // TODO: refactor this line (generalize)
                if (string.IsNullOrEmpty(entity.Container))
                    newEntity.Container = _repositoryRef.ResourcesContainer + @"\" + DateTime.Now.Year.ToString();
                if (!string.IsNullOrEmpty(entity.ContentBase64))
                    newEntity.ContentArrayBytes = Convert.FromBase64String(entity.ContentBase64);
                else
                    newEntity.ContentArrayBytes = null;

                var resGenRep = await resources.AddAsync(newEntity);

                response.Entity = resGenRep.Entity?.GetSimpleDto<ResourceDto>();
                if (response.Entity != null)
                    response.Entity.ContentBase64 = entity.ContentBase64;

                response.ErrorList = resGenRep.ErrorList;
                
                await CloseIsTempConnection(ctx);
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
                var ctx = GetOpenConnection();
                var resources = new GenericRepositoryEF<KntDbContext, Resource>(ctx);

                var resGenRepGet = await resources.GetAsync(entity.ResourceId);
                Resource entityForUpdate;

                if (resGenRepGet.IsValid)
                {
                    entityForUpdate = resGenRepGet.Entity;
                    entityForUpdate.SetSimpleDto(entity);

                    if (!string.IsNullOrEmpty(entity.ContentBase64))
                        entityForUpdate.ContentArrayBytes = Convert.FromBase64String(entity.ContentBase64);
                    else
                        entityForUpdate.ContentArrayBytes = null;

                    resGenRep = await resources.UpdateAsync(entityForUpdate);
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

                await CloseIsTempConnection(ctx);
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
                var ctx = GetOpenConnection();
                var resources = new GenericRepositoryEF<KntDbContext, Resource>(ctx);

                var resGenRep = await resources.DeleteAsync(id);
                if (!resGenRep.IsValid)
                    response.ErrorList = resGenRep.ErrorList;

                await CloseIsTempConnection(ctx);
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
                var ctx = GetOpenConnection();                
                var noteTasks = new GenericRepositoryEF<KntDbContext, NoteTask>(ctx);

                var listTasks = await noteTasks.DbSet.Where(n => n.NoteId == idNote)
                    .Include(t => t.User)
                    .OrderBy( t => t.Priority)
                    .ThenBy(t => t.CreationDateTime)
                    .ToListAsync();
                result.Entity = new List<NoteTaskDto>();
                foreach (var e in listTasks)
                {
                    var nt = e.GetSimpleDto<NoteTaskDto>();
                    nt.UserFullName = e.User.FullName;
                    result.Entity.Add(nt);
                }

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);            
        }

        public async Task<Result<NoteTaskDto>> GetNoteTaskAsync(Guid idNoteTask)
        {
            var result = new Result<NoteTaskDto>();
            try
            {
                var ctx = GetOpenConnection();
                var noteTasks = new GenericRepositoryEF<KntDbContext, NoteTask>(ctx);

                var entiy = await noteTasks.DbSet.Where(n => n.NoteTaskId == idNoteTask)
                    .Include(t => t.User)
                    .SingleOrDefaultAsync();

                result.Entity = entiy.GetSimpleDto<NoteTaskDto>();

                if (entiy != null)
                    result.Entity.UserFullName = entiy.User?.FullName;
                else                
                    result.AddErrorMessage("Task not found.");
                
                await CloseIsTempConnection(ctx);
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
                var ctx = GetOpenConnection();
                var noteTasks = new GenericRepositoryEF<KntDbContext, NoteTask>(ctx);

                var newEntity = new NoteTask();
                newEntity.SetSimpleDto(entity);                
                newEntity.CreationDateTime = DateTime.Now;
                newEntity.ModificationDateTime = DateTime.Now;
                
                var resGenRep = await noteTasks.AddAsync(newEntity);

                response.Entity = resGenRep.Entity?.GetSimpleDto<NoteTaskDto>();
                if (response.Entity != null)
                    response.Entity.UserFullName = entity.UserFullName;

                response.ErrorList = resGenRep.ErrorList;

                await CloseIsTempConnection(ctx);
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
                var ctx = GetOpenConnection();
                var noteTasks = new GenericRepositoryEF<KntDbContext, NoteTask>(ctx);

                var resGenRepGet = await noteTasks.GetAsync(entity.NoteTaskId);
                NoteTask entityForUpdate;
                if (resGenRepGet.IsValid)
                {
                    entityForUpdate = resGenRepGet.Entity;
                    entityForUpdate.SetSimpleDto(entity);
                    entityForUpdate.ModificationDateTime = DateTime.Now;
                    resGenRep = await noteTasks.UpdateAsync(entityForUpdate);
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

                await CloseIsTempConnection(ctx);
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
                var ctx = GetOpenConnection();
                var noteTasks = new GenericRepositoryEF<KntDbContext, NoteTask>(ctx);

                var resGenRep = await noteTasks.DeleteAsync(id);
                if (!resGenRep.IsValid)
                    response.ErrorList = resGenRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);
        }

        public async Task<Result<List<KMessageDto>>> GetMessagesAsync(Guid noteId)
        {
            var result = new Result<List<KMessageDto>>();
            try
            {
                var ctx = GetOpenConnection();                
                var kmessages = new GenericRepositoryEF<KntDbContext, KMessage>(ctx);

                var listMessages = await kmessages.DbSet.Where(m => m.NoteId == noteId)
                    .Include(m => m.User)
                    .OrderBy(m => m.AlarmDateTime)
                    .ToListAsync();
                result.Entity = new List<KMessageDto>();
                foreach (var m in listMessages)
                {
                    var msg = m.GetSimpleDto<KMessageDto>();
                    msg.UserFullName = m.User.FullName;
                    result.Entity.Add(msg);
                }

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<KMessageDto>> GetMessageAsync(Guid messageId)
        {
            var response = new Result<KMessageDto>();
            try
            {
                var ctx = GetOpenConnection();
                var kmessages = new GenericRepositoryEF<KntDbContext, KMessage>(ctx);

                var resGenRep = await kmessages.GetAsync((object)messageId);

                response.Entity = resGenRep.Entity?.GetSimpleDto<KMessageDto>();
                response.ErrorList = resGenRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);
        }

        public async Task<Result<KMessageDto>> AddMessageAsync(KMessageDto entity)
        {
            var response = new Result<KMessageDto>();
            try
            {
                var ctx = GetOpenConnection();
                var kmessages = new GenericRepositoryEF<KntDbContext, KMessage>(ctx);

                var newEntity = new KMessage();
                newEntity.SetSimpleDto(entity);

                var resGenRep = await kmessages.AddAsync(newEntity);

                response.Entity = resGenRep.Entity?.GetSimpleDto<KMessageDto>();
                response.ErrorList = resGenRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);
        }

        public async Task<Result<KMessageDto>> UpdateMessageAsync(KMessageDto entity)
        {
            var resGenRep = new Result<KMessage>();
            var response = new Result<KMessageDto>();

            try
            {
                var ctx = GetOpenConnection();
                var kmessages = new GenericRepositoryEF<KntDbContext, KMessage>(ctx);

                var resGenRepGet = await kmessages.GetAsync(entity.KMessageId);
                KMessage entityForUpdate;

                if (resGenRepGet.IsValid)
                {
                    entityForUpdate = resGenRepGet.Entity;
                    entityForUpdate.SetSimpleDto(entity);
                    resGenRep = await kmessages.UpdateAsync(entityForUpdate);
                }
                else
                {
                    resGenRep.Entity = null;
                    resGenRep.AddErrorMessage("Can't find entity for update.");
                }

                response.Entity = resGenRep.Entity?.GetSimpleDto<KMessageDto>();
                response.ErrorList = resGenRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }

            return ResultDomainAction(response);
        }

        public async Task<Result> DeleteMessageAsync(Guid messageId)
        {
            var response = new Result();
            try
            {
                var ctx = GetOpenConnection();
                var kmessages = new GenericRepositoryEF<KntDbContext, KMessage>(ctx);

                var resGenRep = await kmessages.DeleteAsync(messageId);
                if (!resGenRep.IsValid)
                    response.ErrorList = resGenRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);
        }

        public async Task<Result<WindowDto>> GetWindowAsync(Guid noteId, Guid userId)
        {            
            var response = new Result<WindowDto>();
            try
            {
                var ctx = GetOpenConnection();
                var windows = new GenericRepositoryEF<KntDbContext, Window>(ctx);

                var resGenRep = await windows.GetAsync(_ => _.NoteId == noteId && _.UserId == userId);

                response.Entity = resGenRep.Entity?.GetSimpleDto<WindowDto>();
                response.ErrorList = resGenRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);

        }

        public async Task<Result<WindowDto>> AddWindowAsync(WindowDto entity)
        {            
            var response = new Result<WindowDto>();
            try
            {
                var ctx = GetOpenConnection();
                var windows = new GenericRepositoryEF<KntDbContext, Window>(ctx);

                var newEntity = new Window();
                newEntity.SetSimpleDto(entity);

                var resGenRep = await windows.AddAsync(newEntity);

                response.Entity = resGenRep.Entity?.GetSimpleDto<WindowDto>();
                response.ErrorList = resGenRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);

        }

        public async Task<Result<WindowDto>> UpdateWindowAsync(WindowDto entity)
        {
            var resGenRep = new Result<Window>();
            var response = new Result<WindowDto>();

            try
            {
                var ctx = GetOpenConnection();
                var window = new GenericRepositoryEF<KntDbContext, Window>(ctx);

                var resGenRepGet = await window.GetAsync(entity.WindowId);
                Window entityForUpdate;

                if (resGenRepGet.IsValid)
                {
                    entityForUpdate = resGenRepGet.Entity;
                    entityForUpdate.SetSimpleDto(entity);
                    resGenRep = await window.UpdateAsync(entityForUpdate);
                }
                else
                {
                    resGenRep.Entity = null;
                    resGenRep.AddErrorMessage("Can't find entity for update.");
                }

                response.Entity = resGenRep.Entity?.GetSimpleDto<WindowDto>();
                response.ErrorList = resGenRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }

            return ResultDomainAction(response);
        }

        public async Task<Result<int>> CountNotesInFolder(Guid folderId)
        {
            Result<int> response = new Result<int>();

            try
            {
                var ctx = GetOpenConnection();
                var notes = new GenericRepositoryEF<KntDbContext, Note>(ctx);

                var count = await notes.DbSet.CountAsync(_ => _.FolderId == folderId);
                response.Entity = count;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, response.ErrorList);
            }
            return ResultDomainAction(response);
        }

        public async Task<Result<List<Guid>>> GetVisibleNotesIdAsync(Guid userId)
        {
            var resService = new Result<List<Guid>>();
            try
            {
                var ctx = GetOpenConnection();
                var notes = new GenericRepositoryEF<KntDbContext, Window>(ctx);

                var resRep = await notes.GetAllAsync(w => w.UserId == userId && w.Visible == true);
                resService.Entity = resRep.Entity?.Select(w => w.NoteId).ToList();
                resService.ErrorList = resRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<List<Guid>>> GetAlarmNotesIdAsync(Guid userId, EnumNotificationType? notificationType = null)
        {
            var resService = new Result<List<Guid>>();
            try
            {
                var ctx = GetOpenConnection();
                var notes = new GenericRepositoryEF<KntDbContext, KMessage>(ctx);

                Result<List<KMessage>> resRep;
                
                if(notificationType == null)
                    resRep = await notes.GetAllAsync(m => m.UserId == userId && m.AlarmActivated == true && m.AlarmDateTime <= DateTime.Now && m.NoteId != null);
                else
                    resRep = await notes.GetAllAsync(m => m.UserId == userId && m.AlarmActivated == true 
                        && m.AlarmDateTime <= DateTime.Now && m.NoteId != null && m.NotificationType == (EnumNotificationType)notificationType);

                foreach (var m in resRep.Entity)
                {
                    ApplyAlarmControl(m);
                    await UpdateMessageAsync(m.GetSimpleDto<KMessageDto>());                    
                }

                resService.Entity = resRep.Entity?.Select(w => (Guid)w.NoteId).ToList();
                resService.ErrorList = resRep.ErrorList;

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
            return ResultDomainAction(resService);
        }

        public async Task<Result<bool>> PatchFolder(Guid noteId, Guid folderId)
        {
            Result<Note> resRep = null;
            var result = new Result<bool>();

            try
            {
                var ctx = GetOpenConnection();
                var notes = new GenericRepositoryEF<KntDbContext, Note>(ctx);

                var entityForUpdate = await notes.DbSet.Where(n => n.NoteId == noteId).SingleOrDefaultAsync();                

                if (entityForUpdate != null)
                {                    
                    entityForUpdate.FolderId = folderId;
                    resRep = await notes.UpdateAsync(entityForUpdate);                    
                    result.Entity = resRep.IsValid;
                }
                else
                {
                    result.Entity = false;
                    result.AddErrorMessage("Can't find entity for update.");
                }

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resRep.ErrorList);
            }

            result.ErrorList = resRep.ErrorList;
            return ResultDomainAction(result); ;
        }

        public async Task<Result<bool>> PatchChangeTags(Guid noteId, string oldTag, string newTag)
        {
            var result = new Result<bool>();

            if (string.IsNullOrEmpty(oldTag) && string.IsNullOrEmpty(newTag))
            {
                result.AddErrorMessage("Invalid tags parameters.");
                return result;
            }

            Result<Note> resRep = null;
            
            try
            {
                var ctx = GetOpenConnection();
                var notes = new GenericRepositoryEF<KntDbContext, Note>(ctx);

                var entityForUpdate = await notes.DbSet.Where(n => n.NoteId == noteId).SingleOrDefaultAsync();

                if (entityForUpdate != null)
                {
                    if (string.IsNullOrEmpty(oldTag))
                    {
                        if (!(entityForUpdate.Tags.IndexOf(newTag) >= 0))
                            entityForUpdate.Tags += " " + newTag;
                    }
                    else
                        entityForUpdate.Tags = entityForUpdate.Tags.Replace(oldTag, newTag);

                    entityForUpdate.Tags = entityForUpdate.Tags.Trim();
                    resRep = await notes.UpdateAsync(entityForUpdate);
                    result.Entity = resRep.IsValid;
                }
                else
                {
                    result.Entity = false;
                    result.AddErrorMessage("Can't find entity for update.");
                }

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resRep.ErrorList);
            }

            result.ErrorList = resRep.ErrorList;
            return ResultDomainAction(result); ;
        }

        public async Task<List<NoteKAttributeDto>> CompleteNoteAttributes(List<NoteKAttributeDto> attributesNotes, Guid noteId, Guid? noteTypeId = null)
        {
            var ctx = GetOpenConnection();
            var kattributes = new KntKAttributeRepository(ctx, _repositoryRef);

            var attributes = (await kattributes.GetAllIncludeNullTypeAsync(noteTypeId)).Entity;
            foreach (KAttributeInfoDto a in attributes)
            {
                var atrTmp = attributesNotes
                    .Where(na => na.KAttributeId == a.KAttributeId)
                    .Select(at => at).SingleOrDefault();
                if (atrTmp == null)
                {
                    attributesNotes.Add(new NoteKAttributeDto
                    {
                        NoteKAttributeId = Guid.NewGuid(),
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

            await CloseIsTempConnection(ctx);

            return attributesNotes.OrderBy(_ => _.Order).ThenBy(_ => _.Name).ToList();
        }

        #endregion 

        #region Utils methods 

        private async Task<Result<NoteDto>> GetAsync(Guid? noteId, int? noteNumber)
        {
            var result = new Result<NoteDto>();
            try
            {
                var ctx = GetOpenConnection();
                var notes = new GenericRepositoryEF<KntDbContext, Note>(ctx);

                Note entity;
                if (noteId != null)
                {
                    entity = await notes.DbSet.Where(n => n.NoteId == noteId)
                        .Include(n => n.KAttributes).ThenInclude(n => n.KAttribute)
                        .Include(n => n.Folder)
                        .Include(n => n.NoteType)
                        .SingleOrDefaultAsync();
                }
                else
                {
                    entity = await notes.DbSet.Where(n => n.NoteNumber == noteNumber)
                        .Include(n => n.KAttributes).ThenInclude(n => n.KAttribute)
                        .Include(n => n.Folder)
                        .Include(n => n.NoteType)
                        .SingleOrDefaultAsync();
                }

                // Map to dto
                if (entity != null)
                {
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
                else
                {
                    result.AddErrorMessage("Entity not found.");
                }

                await CloseIsTempConnection(ctx);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        // TODO refactor (duplicated code in dapper repository )
        private void ApplyAlarmControl(KMessage message)
        {
            switch (message.AlarmType)
            {
                case EnumAlarmType.Standard:
                    message.AlarmActivated = false;                                        
                    break;
                case EnumAlarmType.Annual:
                    while (message.AlarmDateTime < DateTime.Now)
                        message.AlarmDateTime = ((DateTime)message.AlarmDateTime).AddYears(1);                    
                    break;
                case EnumAlarmType.Monthly:
                    while (message.AlarmDateTime < DateTime.Now)
                        message.AlarmDateTime = ((DateTime)message.AlarmDateTime).AddMonths(1);                    
                    break;
                case EnumAlarmType.Weekly:
                    while (message.AlarmDateTime < DateTime.Now)
                        message.AlarmDateTime = ((DateTime)message.AlarmDateTime).AddDays(7);                    
                    break;
                case EnumAlarmType.Daily:
                    while (message.AlarmDateTime < DateTime.Now)
                        message.AlarmDateTime = ((DateTime)message.AlarmDateTime).AddDays(1);                    
                    break;
                case EnumAlarmType.InMinutes:
                    while (message.AlarmDateTime < DateTime.Now)
                        message.AlarmDateTime = ((DateTime)message.AlarmDateTime).AddMinutes((int)message.AlarmMinutes);                    
                    break;
                default:                    
                    break;
            }
        }

        private int GetNextNoteNumber(GenericRepositoryEF<KntDbContext, Note> notes)
        {
            //var lastNote = _repository.Context.Notes.OrderByDescending(n => n.NoteNumber).FirstOrDefault();
            var lastNote = notes
                .DbSet.OrderByDescending(n => n.NoteNumber).FirstOrDefault();
            return lastNote != null ? lastNote.NoteNumber + 1 : 1;
        }


        private void UpdateStandardValuesToNewEntity(GenericRepositoryEF<KntDbContext, Note> notes, NoteDto newEntity)
        {            
            if (newEntity.NoteNumber == 0)
                newEntity.NoteNumber = GetNextNoteNumber(notes);
            if (newEntity.CreationDateTime == DateTime.MinValue)
                newEntity.CreationDateTime = DateTime.Now;

            if (newEntity.ModificationDateTime == DateTime.MinValue)
                newEntity.ModificationDateTime = DateTime.Now;
        }

        private async Task<Result<NoteKAttributeDto>> SaveAttrtibuteAsync(KntDbContext ctx, NoteKAttributeDto entity)
        {
            Result<NoteKAttribute> resRep = null;
            var resService = new Result<NoteKAttributeDto>();

            try
            {                
                var noteKAttributes = new GenericRepositoryEF<KntDbContext, NoteKAttribute>(ctx);
                
                var findNoteAttribute = (noteKAttributes.Get(_ => _.NoteKAttributeId == entity.NoteKAttributeId)).Entity;

                if(findNoteAttribute == null)                
                {
                    entity.NoteKAttributeId = Guid.NewGuid();
                    var newEntity = new NoteKAttribute();
                    newEntity.SetSimpleDto(entity);
                    resRep = await noteKAttributes.AddAsync(newEntity);
                }
                else
                {
                    var entityForUpdate = (await noteKAttributes.GetAsync(entity.NoteKAttributeId)).Entity;

                    if (entityForUpdate != null)
                    {
                        entityForUpdate.SetSimpleDto(entity);
                        resRep = await noteKAttributes.UpdateAsync(entityForUpdate);
                    }
                    else
                    {
                        var newEntity = new NoteKAttribute();
                        newEntity.SetSimpleDto(entity);
                        resRep = await noteKAttributes.AddAsync(newEntity);
                    }
                }                
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, resService.ErrorList);
            }
                        
            resService.Entity = entity;
            Model.ModelExtensions.UtilCopyProperties(resService.Entity, resRep.Entity);

            resService.ErrorList = resRep.ErrorList;

            return ResultDomainAction(resService);
        }

        #endregion
    }
}
