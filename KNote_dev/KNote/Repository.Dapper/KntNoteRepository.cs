﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using System.Data.Common;
using Dapper;

using KNote.Model;
using KNote.Model.Dto;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace KNote.Repository.Dapper
{
    public class KntNoteRepository : DomainActionBase, IKntNoteRepository
    {
        protected DbConnection _db;
        private IKntFolderRepository _folders;
        private IKntKAttributeRepository _kattributes;

        public KntNoteRepository(DbConnection db, bool throwKntException)
        {
            _db = db;
            ThrowKntException = throwKntException;

            _folders = new KntFolderRepository(db, throwKntException);
            _kattributes = new KntKAttributeRepository(db, throwKntException);
        }

        public async Task<Result<List<NoteInfoDto>>> HomeNotesAsync()
        {
            var idHomeFolder = (await _folders.GetHomeAsync()).Entity?.FolderId;

            if (idHomeFolder != null)
                return await GetByFolderAsync((Guid)idHomeFolder);
            else
                return null;
        }

        public async Task<Result<List<NoteInfoDto>>> GetByFolderAsync(Guid folderId)
        {
            var result = new Result<List<NoteInfoDto>>();
            try
            {
                var sql = GetSelectFilter();
                sql += @" WHERE FolderId = @folderId ORDER BY [Priority], Topic ;";

                var entity = await _db.QueryAsync<NoteInfoDto>(sql.ToString(), new { folderId });
                result.Entity = entity.ToList();
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<List<NoteInfoDto>>> GetAllAsync()
        {
            var result = new Result<List<NoteInfoDto>>();
            try
            {
                var sql = GetSelectFilter();
                sql += @" ORDER BY [Priority], Topic ;";

                var entity = await _db.QueryAsync<NoteInfoDto>(sql.ToString(), new { });
                result.Entity = entity.ToList();
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<List<NoteInfoDto>>> GetFilter(NotesFilterDto notesFilter)
        {
            var result = new Result<List<NoteInfoDto>>();
            try
            {
                IEnumerable<NoteInfoDto> entity;

                var sql = GetSelectFilter();
                var sqlWhere = GetWhereFilterNotesInfoDto(notesFilter);

                result.CountColecEntity = GetCountFilter(sqlWhere);

                sql = sql + sqlWhere + @" ORDER BY [Priority], Topic ";
                
                var pagination = notesFilter.Pagination;

                if (pagination != null)
                {                    
                    // Pagination SqlServer != SQlite
                    if (_db.GetType().Name == "SqliteConnection")
                        sql += " LIMIT @NumRecords OFFSET @NumRecords * (@Page - 1) ;";
                    else
                        sql += " OFFSET @NumRecords * (@Page - 1) ROWS FETCH NEXT @NumRecords ROWS ONLY;";

                    entity = await _db.QueryAsync<NoteInfoDto>(sql.ToString(), new { Page = pagination.Page, NumRecords = pagination.NumRecords });                    
                }
                else
                {
                    entity = await _db.QueryAsync<NoteInfoDto>(sql.ToString(), new { });
                }
                
                result.Entity = entity.ToList();
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async  Task<Result<List<NoteInfoDto>>> GetSearch(NotesSearchDto notesSearch)
        {
            var result = new Result<List<NoteInfoDto>>();
            var searchNumber = 0;            
            var sql = "";
            var sqlWhere = "";
            var sqlOrder = "";
            IEnumerable<NoteInfoDto> entity;

            try
            {                
                searchNumber = ExtractNoteNumberSearch(notesSearch.TextSearch);
                sql = GetSelectSearch();
                sqlWhere = "";                
                sqlOrder = @" ORDER BY Topic ";

                if (searchNumber > 0)
                {
                    sqlWhere = " WHERE NoteNumber = " + searchNumber.ToString() + " " ;
                }                
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
                                {
                                    sqlWhere = AddAndToStringSQL(sqlWhere);
                                    sqlWhere += " (Topic LIKE '%" + token + "%') ";
                                }
                                else
                                {
                                    var tokenNot = token.Substring(1, token.Length - 1);
                                    sqlWhere = AddAndToStringSQL(sqlWhere);
                                    sqlWhere += " (Topic NOT LIKE '%" + tokenNot + "%')";
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
                                {                                    
                                    sqlWhere = AddAndToStringSQL(sqlWhere);
                                    sqlWhere += " (Topic LIKE '%" + token + "%' OR Description LIKE '%" + token + "%') ";
                                }
                                else
                                {
                                    var tokenNot = token.Substring(1, token.Length - 1);                                 
                                    sqlWhere = AddAndToStringSQL(sqlWhere);
                                    sqlWhere += " (Topic NOT LIKE '%" + tokenNot + "%' AND Description NOT LIKE '%" + tokenNot + "%') ";
                                }
                            }
                        }
                    }
                    if (sqlWhere != "")
                        sqlWhere = " WHERE " + sqlWhere;
                }

                sql = sql + sqlWhere + sqlOrder;
                                
                result.CountColecEntity = GetCountFilter(sqlWhere);

                sql += " OFFSET @NumRecords * (@Page - 1) ROWS FETCH NEXT @NumRecords ROWS ONLY;";
                var pagination = notesSearch.Pagination;
                entity = await _db.QueryAsync<NoteInfoDto>(sql.ToString(), new { Page = pagination.Page, NumRecords = pagination.NumRecords });

                result.Entity = entity.ToList();
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
                var sql = @"SELECT        
                    Notes.NoteId, Notes.NoteNumber, Notes.Topic, Notes.CreationDateTime, 
	                Notes.ModificationDateTime, Notes.Description, Notes.ContentType, Notes.Script, 
	                Notes.InternalTags, Notes.Tags, Notes.Priority, Notes.FolderId , Notes.NoteTypeId, 
	                
                    Folders.FolderId, Folders.FolderNumber, Folders.CreationDateTime, 
	                Folders.ModificationDateTime, Folders.Name, Folders.Tags, Folders.PathFolder, 
	                Folders.[Order], Folders.OrderNotes, Folders.Script, Folders.ParentId, 

                    NoteTypes.NoteTypeId, 
	                NoteTypes.Name, NoteTypes.Description, NoteTypes.ParenNoteTypeId
                            
                    FROM  Notes 
                          INNER JOIN  Folders ON Notes.FolderId = Folders.FolderId 
                          LEFT OUTER JOIN  NoteTypes ON Notes.NoteTypeId = NoteTypes.NoteTypeId
                    
                    WHERE NoteId = @noteId ;";
                
                var entity = await _db.QueryAsync<NoteDto, FolderDto, NoteTypeDto, NoteDto>(
                    sql.ToString(),
                    (note, folder, noteType) =>
                    {
                        note.FolderDto = folder;
                        note.NoteTypeDto = noteType;
                        return note;
                    },
                    new { noteId }, 
                    splitOn: "FolderId, NoteTypeId"
                    );

                result.Entity = entity.ToList().FirstOrDefault();

                var sqlAtr = @"
                        SELECT 
                              NoteKAttributes.NoteKAttributeId, NoteKAttributes.NoteId, NoteKAttributes.Value, 
                              NoteKAttributes.KAttributeId, 

                              KAttributes.KAttributeId, KAttributes.Name, KAttributes.Description, KAttributes.KAttributeDataType, 
                              KAttributes.RequiredValue, KAttributes.[Order], KAttributes.Script, KAttributes.Disabled

                         FROM  NoteKAttributes INNER JOIN
                            KAttributes ON NoteKAttributes.KAttributeId = KAttributes.KAttributeId

                         WHERE NoteId = @noteId";

                var entityList = await _db.QueryAsync<NoteKAttributeDto>(sqlAtr.ToString(), new { noteId } );

                var atrList = entityList.ToList();

                // Complete Attributes list                                                
                atrList = await CompleteNoteAttributes(atrList, result.Entity.NoteId, result.Entity.NoteTypeId);

                result.Entity.KAttributesDto = atrList;
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
            var result = new Result<NoteDto>();
            try
            {
                entity.CreationDateTime = DateTime.Now;
                entity.ModificationDateTime = DateTime.Now;
                entity.NoteNumber = GetNextNoteNumber();

                var sql = @"INSERT INTO [Notes] 
                                (NoteId, NoteNumber, Topic, CreationDateTime, ModificationDateTime, 
                                [Description], ContentType, Script, InternalTags, Tags, 
                                [Priority], FolderId, NoteTypeId)
                          VALUES
                                (@NoteId, @NoteNumber, @Topic, @CreationDateTime, @ModificationDateTime, 
                                @Description, @ContentType, @Script, @InternalTags, @Tags, 
                                @Priority, @FolderId, @NoteTypeId)";
                var r = await _db.ExecuteAsync(sql.ToString(),
                    new
                    {
                        entity.NoteId,
                        entity.NoteNumber,
                        entity.Topic,
                        entity.CreationDateTime,
                        entity.ModificationDateTime,
                        entity.Description,
                        entity.ContentType,
                        entity.Script,
                        entity.InternalTags,
                        entity.Tags,
                        entity.Priority,
                        entity.FolderId,
                        entity.NoteTypeId
                    });

                if (r == 0)
                {
                    result.ErrorList.Add("Entity not inserted");
                    return ResultDomainAction(result);
                }

                foreach (var atr in entity.KAttributesDto)
                {
                    atr.NoteKAttributeId = Guid.NewGuid();
                    atr.NoteId = entity.NoteId;

                    sql = @"INSERT INTO [NoteKAttributes] 
                                (NoteKAttributeId, NoteId, KAttributeId, [Value])
                          VALUES
                                ( @NoteKAttributeId, @NoteId, @KAttributeId, @Value )";
                    var rA = await _db.ExecuteAsync(sql.ToString(),
                        new
                        {
                            atr.NoteKAttributeId,
                            atr.NoteId,
                            atr.KAttributeId,
                            atr.Value
                        });

                    if (rA == 0)
                    {
                        result.ErrorList.Add("Entity not inserted");
                        return ResultDomainAction(result);
                    }
                }

                result.Entity = entity;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<NoteDto>> UpdateAsync(NoteDto entity)
        {            
            var result = new Result<NoteDto>();
            try
            {                
                entity.ModificationDateTime = DateTime.Now;
                
                var sql = @"UPDATE [Notes] SET 
                                NoteId = @NoteId, 
                                NoteNumber = @NoteNumber, 
                                Topic = @Topic, 
                                CreationDateTime = @CreationDateTime, 
                                ModificationDateTime = @ModificationDateTime, 
                                [Description] = @Description, 
                                ContentType = @ContentType, 
                                Script = @Script, 
                                InternalTags = @InternalTags, 
                                Tags = @Tags, 
                                [Priority] = @Priority, 
                                FolderId = @FolderId, 
                                NoteTypeId = @NoteTypeId
                          WHERE NoteId = @NoteId";
                var r = await _db.ExecuteAsync(sql.ToString(),
                    new
                    {
                        entity.NoteId, entity.NoteNumber, entity.Topic, entity.CreationDateTime, entity.ModificationDateTime,
                        entity.Description, entity.ContentType, entity.Script, entity.InternalTags, entity.Tags,
                        entity.Priority, entity.FolderId, entity.NoteTypeId
                    });

                if (r == 0)
                {
                    result.ErrorList.Add("Entity not inserted");
                    return ResultDomainAction(result);
                }

                foreach (var atr in entity.KAttributesDto)
                {
                    if(atr.NoteKAttributeId == Guid.Empty)
                    {
                        atr.NoteKAttributeId = Guid.NewGuid();
                        atr.NoteId = entity.NoteId;
                        sql = @"INSERT INTO [NoteKAttributes] 
                                    (NoteKAttributeId, NoteId, KAttributeId, [Value])
                              VALUES
                                    ( @NoteKAttributeId, @NoteId, @KAttributeId, @Value )";
                    }
                    else
                    {
                        sql = @"UPDATE [NoteKAttributes] SET                                    
                                    NoteId = @NoteId, 
                                    KAttributeId = @KAttributeId, 
                                    [Value] = @Value
                              WHERE NoteKAttributeId = @NoteKAttributeId";
                    }

                    var rA = await _db.ExecuteAsync(sql.ToString(),
                        new
                        {
                            atr.NoteKAttributeId,
                            atr.NoteId,
                            atr.KAttributeId,
                            atr.Value
                        });

                    if (rA == 0)
                    {
                        result.ErrorList.Add("Entity not inserted");
                        return ResultDomainAction(result);
                    }
                }

                // Delete old attributes
                sql = "SELECT NoteKAttributeId, NoteId, KAttributeId, [Value] FROM NoteKAttributes WHERE NoteId = @NoteId";
                var allAtr = (await _db.QueryAsync<NoteKAttributeDto>(sql.ToString(), new { entity.NoteId })).ToList();

                foreach (var a in allAtr)
                {
                    var delAtr = entity.KAttributesDto.Where(_ => _.NoteKAttributeId == a.NoteKAttributeId).FirstOrDefault();
                    if (delAtr == null)
                    {
                        sql = @"DELETE [NoteKAttributes] WHERE NoteKAttributeId = @NoteKAttributeId";
                        var rDel = await _db.ExecuteAsync(sql.ToString(), new { a.NoteKAttributeId });
                        if (rDel == 0)
                        {
                            result.ErrorList.Add("Entity not deleted");
                            return ResultDomainAction(result);
                        }
                    }
                }

                result.Entity = entity;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result> DeleteAsync(Guid id)
        {
            var result = new Result();
            try
            {
                var sql = @"DELETE FROM Notes WHERE NoteId = @Id";

                var r = await _db.ExecuteAsync(sql.ToString(), new { Id = id });

                if (r == 0)
                    result.AddErrorMessage("Entity not deleted");

            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<List<ResourceDto>>> GetNoteResourcesAsync(Guid idNote)
        {
            var result = new Result<List<ResourceDto>>();
            try
            {
                var sql = @"SELECT 
                        ResourceId, [Name], Container, [Description], [Order], FileType, ContentInDB, ContentArrayBytes, NoteId 
                    FROM Resources
                    WHERE NoteId = @idNote 
                    ORDER BY [Order];";

                var entity = await _db.QueryAsync<ResourceDto>(sql.ToString(), new { idNote });
                result.Entity = entity.ToList();
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<ResourceDto>> GetNoteResourceAsync(Guid idNoteResource)
        {            
            var result = new Result<ResourceDto>();
            try
            {
                var sql = @"SELECT  ResourceId, [Name], Container, [Description], [Order], FileType, 
                                ContentInDB, ContentArrayBytes, NoteId 
                        FROM Resources 
                        WHERE ResourceId = @Id";

                var entity = await _db.QueryFirstOrDefaultAsync<ResourceDto>(sql.ToString(), new { Id = idNoteResource });

                if (entity == null)
                    result.AddErrorMessage("Entity not found.");

                result.Entity = entity;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<ResourceDto>> AddResourceAsync(ResourceDto entity)
        {            
            var result = new Result<ResourceDto>();
            try
            {
                // TODO: pendiente, parametrizar esto. 
                entity.Container = @"NotesResources\" + DateTime.Now.Year.ToString();
                entity.ContentArrayBytes = Convert.FromBase64String(entity.ContentBase64);

                var sql = @"INSERT INTO Resources 
                            (ResourceId, [Name], Container, [Description], [Order], 
                                FileType, ContentInDB, ContentArrayBytes, NoteId)
                            VALUES (@ResourceId, @Name, @Container, @Description, @Order, 
                                @FileType, @ContentInDB, @ContentArrayBytes, @NoteId)";

                var r = await _db.ExecuteAsync(sql.ToString(),
                    new { entity.ResourceId, entity.Name, entity.Container, 
                        entity.Description, entity.Order, entity.FileType, 
                        entity.ContentInDB, entity.ContentArrayBytes, entity.NoteId });
                
                if (r == 0)
                    result.ErrorList.Add("Entity not inserted");

                result.Entity = entity;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<ResourceDto>> UpdateResourceAsync(ResourceDto entity)
        {            
            // TODO: Pendiente de probar
            var result = new Result<ResourceDto>();
            try
            {
                // TODO: pendiente, parametrizar esto. 
                entity.Container = @"NotesResources\" + DateTime.Now.Year.ToString();
                entity.ContentArrayBytes = Convert.FromBase64String(entity.ContentBase64);

                var sql = @"UPDATE Resources SET                                                
                            [Name] = @Name, 
                            Container = @Container, 
                            [Description] = @Description, 
                            [Order] = @Order, 
                            FileType = @FileType, 
                            ContentInDB = @ContentInDB, 
                            ContentArrayBytes = @ContentArrayBytes, 
                            NoteId = @NoteId
                    WHERE ResourceId = @ResourceId";

                var r = await _db.ExecuteAsync(sql.ToString(),
                    new {
                        entity.ResourceId,
                        entity.Name,
                        entity.Container,
                        entity.Description,
                        entity.Order,
                        entity.FileType,
                        entity.ContentInDB,
                        entity.ContentArrayBytes,
                        entity.NoteId
                    });

                if (r == 0)
                    result.ErrorList.Add("Entity not updated");

                result.Entity = entity;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result> DeleteResourceAsync(Guid id)
        {
            var result = new Result();
            try
            {
                var sql = @"DELETE FROM Resources WHERE ResourceId = @Id";

                var r = await _db.ExecuteAsync(sql.ToString(), new { Id = id });

                if (r == 0)
                    result.AddErrorMessage("Entity not deleted");
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<List<NoteTaskDto>>> GetNoteTasksAsync(Guid idNote)
        {
            var result = new Result<List<NoteTaskDto>>();
            try
            {
                var sql = @"SELECT
                         NoteTasks.NoteTaskId, NoteTasks.NoteId, NoteTasks.UserId, NoteTasks.CreationDateTime, 
                         NoteTasks.ModificationDateTime, NoteTasks.Description, NoteTasks.Tags, NoteTasks.Priority, NoteTasks.Resolved, 
                         NoteTasks.EstimatedTime, NoteTasks.SpentTime, NoteTasks.DifficultyLevel, NoteTasks.ExpectedStartDate, 
                         NoteTasks.ExpectedEndDate, NoteTasks.StartDate, NoteTasks.EndDate, Users.FullName as UserFullName
                    FROM  NoteTasks LEFT OUTER JOIN
                         Users ON NoteTasks.UserId = Users.UserId
                    WHERE (NoteTasks.NoteId = @idNote)

                    ORDER BY [CreationDateTime];";

                var entity = await _db.QueryAsync<NoteTaskDto>(sql.ToString(), new { idNote });
                result.Entity = entity.ToList();
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
                var sql = @"SELECT 
                        NoteTasks.NoteTaskId, NoteTasks.NoteId, NoteTasks.UserId, NoteTasks.CreationDateTime, 
                        NoteTasks.ModificationDateTime, NoteTasks.Description, NoteTasks.Tags, NoteTasks.Priority, NoteTasks.Resolved, 
                        NoteTasks.EstimatedTime, NoteTasks.SpentTime, NoteTasks.DifficultyLevel, NoteTasks.ExpectedStartDate, 
                        NoteTasks.ExpectedEndDate, NoteTasks.StartDate, NoteTasks.EndDate, Users.FullName as UserFullName
                    FROM  NoteTasks LEFT OUTER JOIN
                        Users ON NoteTasks.UserId = Users.UserId 
                    WHERE NoteTaskId = @Id";

                var entity = await _db.QueryFirstOrDefaultAsync<NoteTaskDto>(sql.ToString(), new { Id = idNoteTask });

                if (entity == null)
                    result.AddErrorMessage("Entity not found.");

                result.Entity = entity;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);

        }

        public async Task<Result<NoteTaskDto>> AddNoteTaskAsync(NoteTaskDto entity)
        {            
            var result = new Result<NoteTaskDto>();
            try
            {
                entity.CreationDateTime = DateTime.Now;
                entity.ModificationDateTime = DateTime.Now;
                var sql = @"INSERT INTO [NoteTasks] 
                                (NoteTaskId, NoteId, UserId, CreationDateTime, ModificationDateTime, 
                                [Description], Tags, [Priority], Resolved, EstimatedTime, SpentTime, 
                                DifficultyLevel, ExpectedStartDate, ExpectedEndDate, StartDate, EndDate)
                            VALUES 
                                (@NoteTaskId, @NoteId, @UserId, @CreationDateTime, @ModificationDateTime, 
                                @Description, @Tags, @Priority, @Resolved, @EstimatedTime, @SpentTime, 
                                @DifficultyLevel, @ExpectedStartDate, @ExpectedEndDate, @StartDate, @EndDate
                                )";

                var r = await _db.ExecuteAsync(sql.ToString(),
                    new {
                        entity.NoteTaskId,
                        entity.NoteId,
                        entity.UserId,
                        entity.CreationDateTime,
                        entity.ModificationDateTime,
                        entity.Description,
                        entity.Tags,
                        entity.Priority,
                        entity.Resolved,
                        entity.EstimatedTime,
                        entity.SpentTime,
                        entity.DifficultyLevel,
                        entity.ExpectedStartDate,
                        entity.ExpectedEndDate,
                        entity.StartDate,
                        entity.EndDate
                    });

                if (r == 0)
                    result.ErrorList.Add("Entity not inserted");

                result.Entity = entity;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<NoteTaskDto>> UpdateNoteTaskAsync(NoteTaskDto entity)
        {            
            var result = new Result<NoteTaskDto>();
            try
            {
                entity.ModificationDateTime = DateTime.Now;

                var sql = @"UPDATE [NoteTasks] SET                     
                        NoteId = @NoteId, 
                        UserId = @UserId, 
                        CreationDateTime = @CreationDateTime, 
                        ModificationDateTime = @ModificationDateTime, 
                        [Description] = @Description, 
                        Tags = @Tags, 
                        [Priority] = @Priority, 
                        Resolved = @Resolved, 
                        EstimatedTime = @EstimatedTime, 
                        SpentTime = @SpentTime, 
                        DifficultyLevel = @DifficultyLevel, 
                        ExpectedStartDate = @ExpectedStartDate, 
                        ExpectedEndDate = @ExpectedEndDate, 
                        StartDate = @StartDate, 
                        EndDate = @EndDate
                    WHERE NoteTaskId = @NoteTaskId";

                var r = await _db.ExecuteAsync(sql.ToString(),
                    new {
                        entity.NoteTaskId,
                        entity.NoteId,
                        entity.UserId,
                        entity.CreationDateTime,
                        entity.ModificationDateTime,
                        entity.Description,
                        entity.Tags,
                        entity.Priority,
                        entity.Resolved,
                        entity.EstimatedTime,
                        entity.SpentTime,
                        entity.DifficultyLevel,
                        entity.ExpectedStartDate,
                        entity.ExpectedEndDate,
                        entity.StartDate,
                        entity.EndDate
                    });

                if (r == 0)
                    result.ErrorList.Add("Entity not updated");

                result.Entity = entity;
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result> DeleteNoteTaskAsync(Guid id)
        {            
            var result = new Result();
            try
            {
                var sql = @"DELETE FROM NoteTasks WHERE NoteTaskId = @Id";

                var r = await _db.ExecuteAsync(sql.ToString(), new { Id = id });

                if (r == 0)
                    result.AddErrorMessage("Entity not deleted");
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public void Dispose()
        {
            // For clear private referencies
        }

        #region Private methods

        private int GetNextNoteNumber()
        {            
            var sql = "SELECT MAX(NoteNumber) FROM Notes";
            var result = _db.ExecuteScalar<int>(sql);

            //return (result == null) ? 1 : ((int)result) + 1;
            return result + 1;
        }

        private long GetCountFilter(string filter)
        {            
            var sql =
                @"SELECT count(*) 
                FROM 
                    Notes LEFT OUTER JOIN
                    NoteKAttributes ON Notes.NoteId = NoteKAttributes.NoteId"
                + filter;
            var result = _db.ExecuteScalar(sql);
            
            return (result == null) ? 0 : Convert.ToInt64(result);
        }

        private string GetSelectFilter()
        {
            return @"SELECT DISTINCT
                 Notes.NoteId, Notes.NoteNumber, Notes.Topic, Notes.CreationDateTime, 
                 Notes.ModificationDateTime, Notes.Description, Notes.ContentType, Notes.Script, 
                 Notes.InternalTags, Notes.Tags, Notes.Priority, Notes.FolderId, 
                 Notes.NoteTypeId
            FROM 
                Notes LEFT OUTER JOIN
                NoteKAttributes ON Notes.NoteId = NoteKAttributes.NoteId";
        }

        private string GetSelectSearch()
        {
            return @"SELECT NoteId, NoteNumber, Topic, CreationDateTime, ModificationDateTime,            
                            [Description], ContentType, Script, InternalTags, Tags, [Priority], FolderId, NoteTypeId
                    FROM Notes ";
        }

        private string GetWhereFilterNotesInfoDto(NotesFilterDto notesFilter)
        {
            string strWhere = "" ;

            if (notesFilter.FolderId != null)
            {
                strWhere = AddAndToStringSQL(strWhere);
                strWhere += "FolderId = '" + notesFilter.FolderId.ToString().ToUpper() + "' ";
            }
            
            if (notesFilter.NoteTypeId != null)
            {
                strWhere = AddAndToStringSQL(strWhere);
                strWhere += "NoteTypeId = '" + notesFilter.NoteTypeId.ToString().ToUpper() + "' ";
            }
           
            if (!string.IsNullOrEmpty(notesFilter.Topic))
            {
                strWhere = AddAndToStringSQL(strWhere);
                strWhere += "Topic LIKE '%" + notesFilter.Topic.ToString() + "%' ";                
            }

            if (!string.IsNullOrEmpty(notesFilter.Tags))
            {
                strWhere = AddAndToStringSQL(strWhere);
                strWhere += "Tags LIKE '%" + notesFilter.Tags.ToString() + "%' ";                
            }

            if (!string.IsNullOrEmpty(notesFilter.Description))
            {
                strWhere = AddAndToStringSQL(strWhere);
                strWhere += "Description LIKE '%" + notesFilter.Description.ToString() + "%' ";                
            }

            foreach (var f in notesFilter.AttributesFilter)
            {
                strWhere = AddAndToStringSQL(strWhere);
                strWhere += " NoteKAttributes.KAttributeId = '" + f.AtrId + "' AND NoteKAttributes.Value LIKE '%" + f.Value + "%'" ;                
            }

            if (!string.IsNullOrEmpty(strWhere))
                strWhere = " WHERE " + strWhere;

            return strWhere;
        }

        private string AddAndToStringSQL (string str)
        {
            if (str != "")
                str += " AND ";
            return str;
        }

        private async Task<List<NoteKAttributeDto>> CompleteNoteAttributes(List<NoteKAttributeDto> attributesNotes, Guid noteId, Guid? noteTypeId = null)
        {
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

        private Task<Result<NoteKAttributeDto>> SaveAttrtibuteAsync(NoteKAttributeDto entity)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
