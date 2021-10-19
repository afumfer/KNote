using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using System.Data.Common;
using Dapper;

using KNote.Model;
using KNote.Model.Dto;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using System.Data;
using System.Transactions;

namespace KNote.Repository.Dapper
{
    public class KntNoteRepository : KntRepositoryBase, IKntNoteRepository
    {
        #region Constructor

        public KntNoteRepository(DbConnection singletonConnection, RepositoryRef repositoryRef) 
            : base(singletonConnection, repositoryRef)
        {

        }

        public KntNoteRepository(RepositoryRef repositoryRef)
            : base(repositoryRef)
        {
        }

        #endregion

        #region IKntNoteRepository

        public async Task<Result<List<NoteInfoDto>>> HomeNotesAsync()
        {
            using var db = GetOpenConnection();
            var folders = new KntFolderRepository(db, _repositoryRef);
           
            var idHomeFolder = (await folders.GetHomeAsync()).Entity?.FolderId;
          
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
                var db = GetOpenConnection();

                var sql = GetSelectNotes();
                sql += @" WHERE FolderId = @folderId ORDER BY [Priority], Topic ;";
                var entity = await db.QueryAsync<NoteInfoDto>(sql.ToString(), new { folderId });
                result.Entity = entity.ToList();

                await CloseIsTempConnection(db);
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
                var db = GetOpenConnection();
                var sql = GetSelectNotes();
                sql += @" ORDER BY [Priority], Topic ;";

                var entity = await db.QueryAsync<NoteInfoDto>(sql.ToString(), new { });
                result.Entity = entity.ToList();

                await CloseIsTempConnection(db);
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
                var db = GetOpenConnection();

                IEnumerable<NoteInfoDto> entity;

                var sql = GetSelectNotes();
                var sqlWhere = GetWhereFilterNotesInfoDto(notesFilter);

                result.TotalCount = GetCountNotes(db, sqlWhere);

                sql = sql + sqlWhere + @" ORDER BY [Priority], Topic ";
                
                var pagination = notesFilter.PageIdentifier;

                if (pagination != null)
                {                    
                    // Pagination SqlServer != SQlite
                    if (db.GetType().Name == "SqliteConnection")
                        sql += " LIMIT @NumRecords OFFSET @Offset ;";
                    else
                        sql += " OFFSET @Offset ROWS FETCH NEXT @NumRecords ROWS ONLY;";

                    entity = await db.QueryAsync<NoteInfoDto>(sql.ToString(), new { Offset = pagination.Offset, NumRecords = pagination.PageSize });
                }
                else
                {
                    entity = await db.QueryAsync<NoteInfoDto>(sql.ToString(), new { });
                }
                
                result.Entity = entity.ToList();

                await CloseIsTempConnection(db);
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
                var db = GetOpenConnection();

                searchNumber = ExtractNoteNumberSearch(notesSearch.TextSearch);
                sql = GetSelectNotes();
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
                                    sqlWhere += $" (Topic LIKE '%{token}%' OR Tags LIKE '%{token}%' ) ";
                                }
                                else
                                {
                                    var tokenNot = token.Substring(1, token.Length - 1);
                                    sqlWhere = AddAndToStringSQL(sqlWhere);                                    
                                    sqlWhere += $" (Topic NOT LIKE '%{tokenNot}%' AND Tags NOT LIKE '%{tokenNot}%')";
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
                                    sqlWhere = AddOrToStringSQL(sqlWhere);                                    
                                    sqlWhere += $" (Topic LIKE '%{token}%' OR Tags LIKE '%{token}%' OR Description LIKE '%{token}%') ";
                                }
                                else
                                {
                                    var tokenNot = token.Substring(1, token.Length - 1);                                 
                                    sqlWhere = AddAndToStringSQL(sqlWhere);                                    
                                    sqlWhere += $" (Topic NOT LIKE '%{tokenNot}%' AND Tags NOT LIKE '%{tokenNot}%' AND Description NOT LIKE '%{tokenNot}%') ";
                                }
                            }
                        }
                    }
                    if (sqlWhere != "")
                        sqlWhere = " WHERE " + sqlWhere;
                }

                sql = sql + sqlWhere + sqlOrder;
                                
                result.TotalCount = GetCountNotes(db, sqlWhere);

                if (db.GetType().Name == "SqliteConnection")
                    sql += " LIMIT @NumRecords OFFSET @Offset ;";
                else
                    sql += " OFFSET @Offset ROWS FETCH NEXT @NumRecords ROWS ONLY;";

                var pagination = notesSearch.PageIdentifier;
                entity = await db.QueryAsync<NoteInfoDto>(sql.ToString(), new { Offset = pagination.Offset, NumRecords = pagination.PageSize });

                result.Entity = entity.ToList();

                await CloseIsTempConnection(db);
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

        private async Task<Result<NoteDto>> GetAsync(Guid? noteId, int? noteNumber)
        {            
            var result = new Result<NoteDto>();
            try
            {
                var db = GetOpenConnection();

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
                          LEFT OUTER JOIN  NoteTypes ON Notes.NoteTypeId = NoteTypes.NoteTypeId ";
                    
                    if(noteId != null)
                        sql += "WHERE NoteId = @noteId ;";
                    else
                        sql += "WHERE NoteNumber = @noteNumber ;";

                var entity = await db.QueryAsync<NoteDto, FolderDto, NoteTypeDto, NoteDto>(
                    sql.ToString(),
                    (note, folder, noteType) =>
                    {
                        note.FolderDto = folder;
                        note.NoteTypeDto = noteType;
                        return note;
                    },
                    new { noteId, noteNumber }, 
                    splitOn: "FolderId, NoteTypeId"
                    );

                result.Entity = entity.ToList().FirstOrDefault();

                if (result.Entity != null)
                {
                    var sqlAtr = @"
                            SELECT 
                                  NoteKAttributes.NoteKAttributeId, NoteKAttributes.NoteId, NoteKAttributes.Value, 
                                  NoteKAttributes.KAttributeId, 

                                  KAttributes.KAttributeId, KAttributes.Name, KAttributes.Description, KAttributes.KAttributeDataType, 
                                  KAttributes.RequiredValue, KAttributes.[Order], KAttributes.Script, KAttributes.Disabled

                             FROM  NoteKAttributes INNER JOIN
                                KAttributes ON NoteKAttributes.KAttributeId = KAttributes.KAttributeId

                             WHERE NoteId = @noteId";

                    var entityList = await db.QueryAsync<NoteKAttributeDto>(sqlAtr.ToString(), new { noteId });

                    var atrList = entityList.ToList();

                    // Complete Attributes list                                                
                    atrList = await CompleteNoteAttributes(db, atrList, result.Entity.NoteId, result.Entity.NoteTypeId);

                    result.Entity.KAttributesDto = atrList;
                }
                else
                    result.AddErrorMessage("Entity not found.");

                await CloseIsTempConnection(db);
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
                var db = GetOpenConnection();

                newNote = new NoteDto();
                if (entity != null)
                    newNote.SetSimpleDto(entity);

                newNote.SetIsNew(true);
                newNote.CreationDateTime = DateTime.Now;
                newNote.ModificationDateTime = DateTime.Now;
                newNote.KAttributesDto = new List<NoteKAttributeDto>();                
                newNote.KAttributesDto = await CompleteNoteAttributes(db, newNote.KAttributesDto, newNote.NoteId);

                result.Entity = newNote;

                await CloseIsTempConnection(db);
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
                using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var db = GetOpenConnection();

                    entity.CreationDateTime = DateTime.Now;
                    entity.ModificationDateTime = DateTime.Now;
                    if(entity.NoteNumber == 0)
                        entity.NoteNumber = GetNextNoteNumber(db);

                    var sql = @"INSERT INTO [Notes] 
                                    (NoteId, NoteNumber, Topic, CreationDateTime, ModificationDateTime, 
                                    [Description], ContentType, Script, InternalTags, Tags, 
                                    [Priority], FolderId, NoteTypeId)
                              VALUES
                                    (@NoteId, @NoteNumber, @Topic, @CreationDateTime, @ModificationDateTime, 
                                    @Description, @ContentType, @Script, @InternalTags, @Tags, 
                                    @Priority, @FolderId, @NoteTypeId)";
                    var r = await db.ExecuteAsync(sql.ToString(),
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
                        result.ErrorList.Add("Note entity not inserted");
                        ExceptionHasHappened = true;
                        return ResultDomainAction(result);
                    }

                    foreach (var atr in entity.KAttributesDto)
                    {
                        if (!string.IsNullOrEmpty(atr.Value))
                        {
                            atr.NoteKAttributeId = Guid.NewGuid();
                            atr.NoteId = entity.NoteId;

                            sql = @"INSERT INTO [NoteKAttributes] 
                                        (NoteKAttributeId, NoteId, KAttributeId, [Value])
                                  VALUES
                                        ( @NoteKAttributeId, @NoteId, @KAttributeId, @Value )";
                            var rA = await db.ExecuteAsync(sql.ToString(),
                                new
                                {
                                    atr.NoteKAttributeId,
                                    atr.NoteId,
                                    atr.KAttributeId,
                                    atr.Value
                                });

                            if (rA == 0)
                            {
                                result.ErrorList.Add("Atribute-value note entity not inserted");                        
                            }
                        }
                    }

                    result.Entity = entity;

                    scope.Complete();

                    await CloseIsTempConnection(db);
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
            var result = new Result<NoteDto>();
            try
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    var db = GetOpenConnection();

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
                    var r = await db.ExecuteAsync(sql.ToString(),
                        new
                        {
                            entity.NoteId, entity.NoteNumber, entity.Topic, entity.CreationDateTime, entity.ModificationDateTime,
                            entity.Description, entity.ContentType, entity.Script, entity.InternalTags, entity.Tags,
                            entity.Priority, entity.FolderId, entity.NoteTypeId
                        });

                    if (r == 0)
                    {
                        result.ErrorList.Add("Note entity not updated.");
                        ExceptionHasHappened = true;
                        return ResultDomainAction(result);
                    }

                    // Delete old attributes
                    int rDel = 0;
                    if (entity.NoteTypeId == null)
                    {
                        sql = @"DELETE FROM NoteKAttributes WHERE NoteId = @NoteId AND 
                                    KAttributeId NOT IN (SELECT KAttributeId FROM KAttributes WHERE NoteTypeId IS NULL)";
                        rDel = await db.ExecuteAsync(sql.ToString(), new { NoteId = entity.NoteId, NoteTypeId = entity.NoteTypeId });

                    }
                    else
                    {
                        sql = @"DELETE FROM NoteKAttributes WHERE NoteId = @NoteId AND 
                                    KAttributeId NOT IN (SELECT KAttributeId FROM KAttributes WHERE NoteTypeId IS NULL OR NoteTypeId = @NoteTypeId)";
                        rDel = await db.ExecuteAsync(sql.ToString(), new { NoteId = entity.NoteId,  NoteTypeId = entity.NoteTypeId });
                    }

                    // Add new attributes or update 
                    var sqlFindNoteKAttribute = "SELECT * from NoteKAttributes WHERE NoteKAttributeId = @NoteKAttributeId;";
                    foreach (var atr in entity.KAttributesDto)
                    {                    
                        var entityNoteKAttribute = await db.QueryFirstOrDefaultAsync<NoteKAttributeDto>(sqlFindNoteKAttribute.ToString(), new { NoteKAttributeId = atr.NoteKAttributeId });
                        if (entityNoteKAttribute == null)
                        {
                            if (!string.IsNullOrEmpty(atr.Value))
                            {
                                atr.NoteKAttributeId = Guid.NewGuid();
                                atr.NoteId = entity.NoteId;
                                sql = @"INSERT INTO [NoteKAttributes] 
                                            (NoteKAttributeId, NoteId, KAttributeId, [Value])
                                      VALUES
                                            ( @NoteKAttributeId, @NoteId, @KAttributeId, @Value )";
                            }
                            else
                                sql = "";
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(atr.Value))                        
                                sql = @"UPDATE [NoteKAttributes] SET                                    
                                        NoteId = @NoteId, 
                                        KAttributeId = @KAttributeId, 
                                        [Value] = @Value
                                    WHERE NoteKAttributeId = @NoteKAttributeId";
                            else
                                sql = @"DELETE FROM [NoteKAttributes] 
                                    WHERE NoteKAttributeId = @NoteKAttributeId";

                        }

                        if (!string.IsNullOrEmpty(sql))
                        {
                            var rA = await db.ExecuteAsync(sql.ToString(),
                                new
                                {
                                    atr.NoteKAttributeId,
                                    atr.NoteId,
                                    atr.KAttributeId,
                                    atr.Value
                                });

                            if (rA == 0)
                            {
                                result.ErrorList.Add("Atribute-value note entity not saved");                        
                            }
                        }
                    }

                    result.Entity = entity;

                    scope.Complete();

                    await CloseIsTempConnection(db);
                }
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
                var db = GetOpenConnection();

                var sql = @"DELETE FROM Notes WHERE NoteId = @Id";

                var r = await db.ExecuteAsync(sql.ToString(), new { Id = id });

                if (r == 0)
                    result.AddErrorMessage("Entity not deleted");
                
                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<List<ResourceDto>>> GetResourcesAsync(Guid idNote)
        {
            var result = new Result<List<ResourceDto>>();
            try
            {
                var db = GetOpenConnection();

                var sql = @"SELECT 
                        ResourceId, [Name], Container, [Description], [Order], FileType, ContentInDB, ContentArrayBytes, NoteId 
                    FROM Resources
                    WHERE NoteId = @idNote 
                    ORDER BY [Order];";

                var entity = await db.QueryAsync<ResourceDto>(sql.ToString(), new { idNote });
                result.Entity = entity.ToList();

                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<ResourceDto>> GetResourceAsync(Guid idNoteResource)
        {            
            var result = new Result<ResourceDto>();
            try
            {
                var db = GetOpenConnection();

                var sql = @"SELECT  ResourceId, [Name], Container, [Description], [Order], FileType, 
                                ContentInDB, ContentArrayBytes, NoteId 
                        FROM Resources 
                        WHERE ResourceId = @Id";

                var entity = await db.QueryFirstOrDefaultAsync<ResourceDto>(sql.ToString(), new { Id = idNoteResource });

                if (entity == null)
                    result.AddErrorMessage("Entity not found.");

                result.Entity = entity;

                await CloseIsTempConnection(db);
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
                var db = GetOpenConnection();
                                
                if(string.IsNullOrEmpty(entity.Container))
                    entity.Container = _repositoryRef.ResourcesContainer + @"\" + DateTime.Now.Year.ToString();
                if (!string.IsNullOrEmpty(entity.ContentBase64))
                    entity.ContentArrayBytes = Convert.FromBase64String(entity.ContentBase64);
                else
                    entity.ContentArrayBytes = null;

                var sql = @"INSERT INTO Resources 
                            (ResourceId, [Name], Container, [Description], [Order], 
                                FileType, ContentInDB, ContentArrayBytes, NoteId)
                            VALUES (@ResourceId, @Name, @Container, @Description, @Order, 
                                @FileType, @ContentInDB, @ContentArrayBytes, @NoteId)";

                var r = await db.ExecuteAsync(sql.ToString(),
                    new { entity.ResourceId, entity.Name, entity.Container, 
                        entity.Description, entity.Order, entity.FileType, 
                        entity.ContentInDB, entity.ContentArrayBytes, entity.NoteId });
                
                if (r == 0)
                    result.ErrorList.Add("Entity not inserted");

                result.Entity = entity;

                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<ResourceDto>> UpdateResourceAsync(ResourceDto entity)
        {                        
            var result = new Result<ResourceDto>();
            try
            {
                var db = GetOpenConnection();

                if (!string.IsNullOrEmpty(entity.ContentBase64))
                    entity.ContentArrayBytes = Convert.FromBase64String(entity.ContentBase64);
                else
                    entity.ContentArrayBytes = null;

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
                var r = await db.ExecuteAsync(sql.ToString(),
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

                await CloseIsTempConnection(db);
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
                var db = GetOpenConnection();

                var sql = @"DELETE FROM Resources WHERE ResourceId = @Id";

                var r = await db.ExecuteAsync(sql.ToString(), new { Id = id });

                if (r == 0)
                    result.AddErrorMessage("Entity not deleted");

                await CloseIsTempConnection(db);
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
                var db = GetOpenConnection();

                var sql = @"SELECT
                         NoteTasks.NoteTaskId, NoteTasks.NoteId, NoteTasks.UserId, NoteTasks.CreationDateTime, 
                         NoteTasks.ModificationDateTime, NoteTasks.Description, NoteTasks.Tags, NoteTasks.Priority, NoteTasks.Resolved, 
                         NoteTasks.EstimatedTime, NoteTasks.SpentTime, NoteTasks.DifficultyLevel, NoteTasks.ExpectedStartDate, 
                         NoteTasks.ExpectedEndDate, NoteTasks.StartDate, NoteTasks.EndDate, Users.FullName as UserFullName
                    FROM  NoteTasks LEFT OUTER JOIN
                         Users ON NoteTasks.UserId = Users.UserId
                    WHERE (NoteTasks.NoteId = @idNote)
                    ORDER BY [CreationDateTime];";

                var entity = await db.QueryAsync<NoteTaskDto>(sql.ToString(), new { idNote });
                result.Entity = entity.ToList();

                await CloseIsTempConnection(db);
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
                var db = GetOpenConnection();

                var sql = @"SELECT 
                        NoteTasks.NoteTaskId, NoteTasks.NoteId, NoteTasks.UserId, NoteTasks.CreationDateTime, 
                        NoteTasks.ModificationDateTime, NoteTasks.Description, NoteTasks.Tags, NoteTasks.Priority, NoteTasks.Resolved, 
                        NoteTasks.EstimatedTime, NoteTasks.SpentTime, NoteTasks.DifficultyLevel, NoteTasks.ExpectedStartDate, 
                        NoteTasks.ExpectedEndDate, NoteTasks.StartDate, NoteTasks.EndDate, Users.FullName as UserFullName
                    FROM  NoteTasks LEFT OUTER JOIN
                        Users ON NoteTasks.UserId = Users.UserId 
                    WHERE NoteTaskId = @Id";

                var entity = await db.QueryFirstOrDefaultAsync<NoteTaskDto>(sql.ToString(), new { Id = idNoteTask });

                if (entity == null)
                    result.AddErrorMessage("Entity not found.");

                result.Entity = entity;

                await CloseIsTempConnection(db);
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
                var db = GetOpenConnection();

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

                var r = await db.ExecuteAsync(sql.ToString(),
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

                await CloseIsTempConnection(db);
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
                var db = GetOpenConnection();

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

                var r = await db.ExecuteAsync(sql.ToString(),
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

                await CloseIsTempConnection(db);
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
                var db = GetOpenConnection();

                var sql = @"DELETE FROM NoteTasks WHERE NoteTaskId = @Id";

                var r = await db.ExecuteAsync(sql.ToString(), new { Id = id });

                if (r == 0)             
                    result.AddErrorMessage("Entity not deleted");

                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<List<KMessageDto>>> GetMessagesAsync(Guid noteId)
        {
            var result = new Result<List<KMessageDto>>();
            try
            {
                var db = GetOpenConnection();

                var sql = @"
                    SELECT
                        KMessages.KMessageId, KMessages.NoteId, KMessages.ActionType, KMessages.NotificationType, 
                        KMessages.AlarmType, KMessages.[Comment], KMessages.AlarmActivated, KMessages.AlarmDateTime, 
                        KMessages.AlarmMinutes, KMessages.UserId, Users.FullName AS UserFullName
                    FROM  KMessages INNER JOIN
                        Users ON KMessages.UserId = Users.UserId
                    WHERE (KMessages.NoteId = @noteId)
                    ORDER BY KMessages.AlarmDateTime;";

                var entity = await db.QueryAsync<KMessageDto>(sql.ToString(), new { noteId });
                result.Entity = entity.ToList();

                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<KMessageDto>> GetMessageAsync(Guid messageId)
        {
            var result = new Result<KMessageDto>();
            try
            {
                var db = GetOpenConnection();

                var sql = @"
                    SELECT                        
                        KMessages.KMessageId, KMessages.NoteId, KMessages.ActionType, KMessages.NotificationType, 
                        KMessages.AlarmType, KMessages.[Comment], KMessages.AlarmActivated, KMessages.AlarmDateTime, 
                        KMessages.AlarmMinutes, KMessages.UserId, Users.FullName AS UserFullName
                    FROM  KMessages INNER JOIN
                         Users ON KMessages.UserId = Users.UserId  
                    WHERE KMessageId = @Id";

                var entity = await db.QueryFirstOrDefaultAsync<KMessageDto>(sql.ToString(), new { Id = messageId });

                if (entity == null)
                    result.AddErrorMessage("Entity not found.");

                result.Entity = entity;

                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<KMessageDto>> AddMessageAsync(KMessageDto entity)
        {
            var result = new Result<KMessageDto>();
            try
            {
                var db = GetOpenConnection();

                var sql = @"INSERT INTO KMessages (KMessageId, UserId, NoteId, ActionType, 
                                NotificationType, AlarmType, Comment,
                                AlarmActivated, AlarmDateTime, AlarmMinutes )
                            VALUES (@KMessageId, @UserId, @NoteId, @ActionType, 
                                @NotificationType, @AlarmType, @Comment, 
                                @AlarmActivated, @AlarmDateTime, @AlarmMinutes)";

                var r = await db.ExecuteAsync(sql.ToString(),
                    new
                    {
                        entity.KMessageId,
                        entity.UserId,
                        entity.NoteId,
                        entity.ActionType,
                        entity.NotificationType,
                        entity.AlarmType,                        
                        entity.Comment,
                        entity.AlarmActivated,
                        entity.AlarmDateTime,
                        entity.AlarmMinutes
                    });

                if (r == 0)
                    result.ErrorList.Add("Entity not inserted");

                result.Entity = entity;

                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async  Task<Result<KMessageDto>> UpdateMessageAsync(KMessageDto entity)
        {
            var result = new Result<KMessageDto>();
            try
            {
                var db = GetOpenConnection();

                var sql = @"UPDATE KMessages SET                         
                        UserId = @UserId, 
                        NoteId = @NoteId, 
                        ActionType = @ActionType, 
                        NotificationType = @NotificationType, 
                        AlarmType = @AlarmType,                         
                        Comment = @Comment,                                                 
                        AlarmActivated = @AlarmActivated, 
                        AlarmDateTime = @AlarmDateTime, 
                        AlarmMinutes = @AlarmMinutes
                    WHERE KMessageId = @KMessageId";

                var r = await db.ExecuteAsync(sql.ToString(),
                    new { entity.KMessageId, entity.UserId, entity.NoteId, entity.ActionType, entity.NotificationType,
                        entity.AlarmType, entity.Comment,  entity.AlarmActivated, entity.AlarmDateTime, entity.AlarmMinutes
                    });

                if (r == 0)
                    result.ErrorList.Add("Entity not updated");

                result.Entity = entity;

                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result> DeleteMessageAsync(Guid messageId)
        {
            var result = new Result();
            try
            {
                var db = GetOpenConnection();

                var sql = @"DELETE FROM KMessages WHERE KMessageId = @Id";

                var r = await db.ExecuteAsync(sql.ToString(), new { Id = messageId });

                if (r == 0)
                    result.AddErrorMessage("Entity not deleted");

                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<WindowDto>> GetWindowAsync(Guid noteId, Guid userId)
        {
            var result = new Result<WindowDto>();
            try
            {
                var db = GetOpenConnection();

                var sql = @"
                    SELECT                        
                        WindowId, NoteId, UserId, Host, Visible, AlwaysOnTop, PosX, PosY, Width, Height, FontName, 
                        FontSize, FontBold, FontItalic, FontUnderline, FontStrikethru, ForeColor, 
                        TitleColor, TextTitleColor, NoteColor, TextNoteColor
                    FROM Windows  
                    WHERE NoteId = @NoteId and UserId = @UserId";

                var entity = await db.QueryFirstOrDefaultAsync<WindowDto>(sql.ToString(), new { NoteId = noteId, UserId = userId });

                if (entity == null)
                    result.AddErrorMessage("Entity not found.");

                result.Entity = entity;

                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<WindowDto>> AddWindowAsync(WindowDto entity)
        {
            var result = new Result<WindowDto>();
            try
            {
                var db = GetOpenConnection();

                var sql = @"INSERT INTO Windows 
                                (WindowId, NoteId, UserId, Host, Visible, AlwaysOnTop, PosX, PosY, Width, Height, FontName, 
                                FontSize, FontBold, FontItalic, FontUnderline, FontStrikethru, ForeColor, 
                                TitleColor, TextTitleColor, NoteColor, TextNoteColor )
                            VALUES (@WindowId, @NoteId, @UserId, @Host, @Visible, @AlwaysOnTop, @PosX, @PosY, @Width, @Height, @FontName, 
                                @FontSize, @FontBold, @FontItalic, @FontUnderline, @FontStrikethru, @ForeColor, 
                                @TitleColor, @TextTitleColor, @NoteColor, @TextNoteColor)";

                var r = await db.ExecuteAsync(sql.ToString(),
                    new
                    {
                        entity.WindowId,
                        entity.NoteId,
                        entity.UserId,
                        entity.Host,
                        entity.Visible,
                        entity.AlwaysOnTop,
                        entity.PosX,
                        entity.PosY,
                        entity.Width,
                        entity.Height,
                        entity.FontName,
                        entity.FontSize,
                        entity.FontBold,
                        entity.FontItalic,
                        entity.FontUnderline,
                        entity.FontStrikethru,
                        entity.ForeColor,
                        entity.TitleColor,
                        entity.TextTitleColor,
                        entity.NoteColor,
                        entity.TextNoteColor
                    });

                if (r == 0)
                    result.ErrorList.Add("Entity not inserted");

                result.Entity = entity;

                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<WindowDto>> UpdateWindowAsync(WindowDto entity)
        {
            var result = new Result<WindowDto>();
            try
            {
                var db = GetOpenConnection();

                var sql = @"UPDATE Windows SET                                                 
                        NoteId = @NoteId,
                        UserId = @UserId,
                        Host = @Host,
                        Visible = @Visible,
                        AlwaysOnTop = @AlwaysOnTop,
                        PosX = @PosX,
                        PosY = @PosY,
                        Width = @Width,
                        Height = @Height,
                        FontName = @FontName,
                        FontSize = @FontSize,
                        FontBold = @FontBold,
                        FontItalic = @FontItalic,
                        FontUnderline = @FontUnderline,
                        FontStrikethru = @FontStrikethru,
                        ForeColor = @ForeColor,
                        TitleColor = @TitleColor,
                        TextTitleColor = @TextTitleColor,
                        NoteColor = @NoteColor,
                        TextNoteColor = @TextNoteColor
                    WHERE WindowId = @WindowId";

                var r = await db.ExecuteAsync(sql.ToString(),
                    new
                    {
                        entity.WindowId,
                        entity.NoteId,
                        entity.UserId,
                        entity.Host,
                        entity.Visible,
                        entity.AlwaysOnTop,
                        entity.PosX,
                        entity.PosY,
                        entity.Width,
                        entity.Height,
                        entity.FontName,
                        entity.FontSize,
                        entity.FontBold,
                        entity.FontItalic,
                        entity.FontUnderline,
                        entity.FontStrikethru,
                        entity.ForeColor,
                        entity.TitleColor,
                        entity.TextTitleColor,
                        entity.NoteColor,
                        entity.TextNoteColor
                    });

                if (r == 0)
                    result.ErrorList.Add("Entity not updated");

                result.Entity = entity;

                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<int>> CountNotesInFolder(Guid folderId)
        {
            var result = new Result<int>();
            try
            {
                var db = GetOpenConnection();

                var sql = @"SELECT COUNT(*) FROM Notes WHERE FolderId = @folderId";
                var countNotes = await db.ExecuteScalarAsync<int>(sql.ToString(), new { folderId });                    
                result.Entity = countNotes;

                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<List<Guid>>> GetVisibleNotesIdAsync(Guid userId)
        {
            var result = new Result<List<Guid>>();
            try
            {
                var db = GetOpenConnection();

                var sql = "SELECT [NoteId] from [Windows] where UserId = @userId and Visible = 1;";
                
                var entity = await db.QueryAsync<Guid>(sql.ToString(), new { userId });
                result.Entity = entity.ToList();

                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<List<Guid>>> GetAlarmNotesIdAsync(Guid userId, EnumNotificationType? notificationType = null)
        {
            var result = new Result<List<Guid>>();            
            try
            {
                var db = GetOpenConnection();
                var alarm = DateTime.Now;
                EnumNotificationType notType = EnumNotificationType.PostIt;

                var sql = "SELECT * from [KMessages] where UserId = @userId and [AlarmDateTime] <= @alarm and [AlarmActivated] = 1 and NoteId is not null";

                if(notificationType != null)
                {
                    notType = (EnumNotificationType)notificationType;
                    sql += " and NotificationType = @notType";
                }                                   
                
                var messageList = await db.QueryAsync<KMessageDto>(sql.ToString(), new { userId, alarm, notType });

                foreach (var m in messageList)
                {
                    ApplyAlarmControl(m);
                    await UpdateMessageAsync(m);
                }

                result.Entity = messageList.Select(m => (Guid)m.NoteId).ToList();

                
                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<bool>> PatchFolder(Guid noteId, Guid folderId)
        {
            var result = new Result<bool>();
            try
            {
                var db = GetOpenConnection();

                var sql = @"UPDATE Notes SET                         
                        FolderId = @FolderId  
                    WHERE NoteId = @NoteId";

                var r = await db.ExecuteAsync(sql.ToString(),
                    new
                    {                        
                        NoteId = noteId,
                        FolderId = folderId,
                    });

                if (r == 0)
                    result.ErrorList.Add("Entity not updated");

                result.Entity = true;

                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<bool>> PatchChangeTags(Guid noteId, string oldTag, string newTag)
        {
            var result = new Result<bool>();
            var sql = "";
            try
            {
                var db = GetOpenConnection();

                sql = @"Select Tags FROM Notes WHERE NoteId = @NoteId";                
                var actualTag = await db.ExecuteScalarAsync<string>(sql.ToString(), new { NoteId = noteId });         
                
                if (string.IsNullOrEmpty(oldTag))
                {
                    if (!(actualTag.IndexOf(newTag) >= 0))
                        actualTag += " " + newTag;
                }
                else                    
                    actualTag = (actualTag.Replace(oldTag, newTag));
                actualTag = actualTag.Trim();

                sql = @"UPDATE Notes SET Tags = @NewTag  WHERE NoteId = @NoteId";
                var r = await db.ExecuteAsync(sql.ToString(), new { NoteId = noteId, NewTag = actualTag });
                if (r == 0)
                    result.ErrorList.Add("Entity not updated");

                result.Entity = true;

                await CloseIsTempConnection(db);
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        #endregion

        #region Private methods

        // TODO refactor (duplicated code in EntityFramework repository)
        private void ApplyAlarmControl(KMessageDto message)
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
                    message.AlarmActivated = false;
                    break;
            }
        }

        private int GetNextNoteNumber(DbConnection db)
        {
            var sql = "SELECT MAX(NoteNumber) FROM Notes";
            var result = db.ExecuteScalar<int>(sql);
            
            return result + 1;
        }

        private long GetCountNotes(DbConnection db, string filter)
        {
            var sql =
                @"SELECT count(*) 
                FROM Notes "
                + filter;

            var result = db.ExecuteScalar(sql);

            return (result == null) ? 0 : Convert.ToInt64(result);
        }

        private string GetSelectNotes()
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
                strWhere += $@" Notes.NoteId in (SELECT [NoteKAttributes].NoteId FROM NoteKAttributes 
                                    WHERE [NoteKAttributes].NoteId = [Notes].NoteId  
                                        AND NoteKAttributes.KAttributeId = '{f.AtrId.ToString().ToUpper()}' 
                                        AND NoteKAttributes.Value like '%{f.Value}%' ) ";
            }

            if (!string.IsNullOrEmpty(strWhere))
                strWhere = " WHERE " + strWhere;

            return strWhere;
        }

        private string AddAndToStringSQL (string str)
        {
            if (!string.IsNullOrEmpty(str))
                str += " AND ";
            return str;
        }

        private string AddOrToStringSQL(string str)
        {
            if (!string.IsNullOrEmpty(str))
                str += " OR ";
            return str;
        }


        public async Task<List<NoteKAttributeDto>> CompleteNoteAttributes(List<NoteKAttributeDto> attributesNotes, Guid noteId, Guid? noteTypeId = null)
        {
            var db = GetOpenConnection();
            return await CompleteNoteAttributes(db, attributesNotes, noteId, noteTypeId);
        }

        private async Task<List<NoteKAttributeDto>> CompleteNoteAttributes(DbConnection conn, List<NoteKAttributeDto> attributesNotes, Guid noteId, Guid? noteTypeId = null)
        {
            //var kattributes = new KntKAttributeRepository(ConnectionString, Provider, ThrowKntException);
            var kattributes = new KntKAttributeRepository(conn, _repositoryRef);
            var attributes = (await kattributes.GetAllIncludeNullTypeAsync(noteTypeId)).Entity;
            
            if (attributes == null)
                return null;

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
            return attributesNotes.OrderBy(_ => _.Order).ThenBy(_ => _.Name).ToList();
        }

        #endregion
    }
}
