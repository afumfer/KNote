﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;
using Dapper;

using KNote.Model;
using KNote.Model.Dto;
using System.Linq;

namespace KNote.Repository.Dapper
{
    public class KntNoteRepository : DomainActionBase, IKntNoteRepository
    {
        protected SqlConnection _db;
        private IKntFolderRepository _folders;
        private IKntKAttributeRepository _kattributes;

        public KntNoteRepository(SqlConnection db, bool throwKntException)
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
	                Notes.InternalTags, Notes.Tags, Notes.Priority, Notes.FolderId, 
	                
                    Folders.FolderId, Folders.FolderNumber, Folders.CreationDateTime, 
	                Folders.ModificationDateTime, Folders.Name, Folders.Tags, Folders.PathFolder, 
	                Folders.[Order], Folders.OrderNotes, Folders.Script, Folders.ParentId, 

                    Notes.NoteTypeId, NoteTypes.NoteTypeId, 
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
                        //note.NoteTypeDto = noteType;
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


        //public Task<Result<NoteDto>> SaveAsync(NoteDto entityInfo)
        //{
        //    //var db = dbConnection();
        //    //var sql = @"INSERT INTO Carpetas (NombreCarpeta, IdCarpetaPadre, Orden, OrdenNotas, TipoSalidaNotas)
        //    //            VALUES (@NombreCarpeta, @IdCarpetaPadre, @Orden, @OrdenNotas, @TipoSalidaNotas)";
        //    //var result = await db.ExecuteAsync(sql.ToString(), new { carpeta.NombreCarpeta, carpeta.IdCarpetaPadre, carpeta.Orden, carpeta.OrdenNotas, carpeta.TipoSalidaNotas });
        //    //return result > 0;

        //    //var db = dbConnection();
        //    //var sql = @"UPDATE Carpetas SET
        //    //            NombreCarpeta = @NombreCarpeta
        //    //            , IdCarpetaPadre = @IdCarpetaPadre
        //    //            , Orden = @Orden
        //    //            , OrdenNotas = @OrdenNotas
        //    //            , TipoSalidaNotas = @TipoSalidaNotas 
        //    //            WHERE IdCarpeta = @IdCarpeta;";
        //    //var result = await db.ExecuteAsync(sql.ToString(),
        //    //                new { carpeta.IdCarpeta, carpeta.NombreCarpeta, carpeta.IdCarpetaPadre, carpeta.Orden, carpeta.OrdenNotas, carpeta.TipoSalidaNotas });
        //    //return result > 0;


        //    throw new NotImplementedException();
        //}

        public Task<Result<NoteDto>> AddAsync(NoteDto entity)
        {
            throw new NotImplementedException();
        }

        public Task<Result<NoteDto>> UpdateAsync(NoteDto entity)
        {
            throw new NotImplementedException();
        }


        public Task<Result<NoteTaskDto>> SaveNoteTaskAsync(NoteTaskDto entityInfo)
        {
            throw new NotImplementedException();
        }

        public Task<Result<ResourceDto>> SaveResourceAsync(ResourceDto entity)
        {
            throw new NotImplementedException();
        }

        public Task<Result> DeleteAsync(Guid id)
        {
            //var db = dbConnection();
            //var sql = @"DELETE FROM Carpetas WHERE IdCarpeta = @Id";
            //var result = await db.ExecuteAsync(sql.ToString(), new { Id = id });
            //return result > 0;

            throw new NotImplementedException();
        }

        public Task<Result<NoteTaskDto>> DeleteNoteTaskAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<ResourceDto>> DeleteResourceAsync(Guid id)
        {
            throw new NotImplementedException();
        }


        public void Dispose()
        {
            // For clear private referencies
        }

        #region Private methods

        private int GetNextNoteNumber()
        {            
            var sql = "SELECT MAX(NoteNumber) FROM Notess";
            var result = _db.ExecuteScalar(sql);

            return (result == null) ? 1 : ((int)result) + 1;
        }

        private int GetCountFilter(string filter)
        {
            //@"SELECT COUNT(*) FROM Notes " 
            var sql =
                @"SELECT count(*) 
                FROM 
                    Notes LEFT OUTER JOIN
                    NoteKAttributes ON Notes.NoteId = NoteKAttributes.NoteId"
                + filter;
            var result = _db.ExecuteScalar(sql);

            return (result == null) ? 0 : ((int)result);
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
                strWhere += "FolderId = '" + notesFilter.FolderId.ToString() + "' ";
            }
            
            if (notesFilter.NoteTypeId != null)
            {
                strWhere = AddAndToStringSQL(strWhere);
                strWhere += "NoteTypeId = '" + notesFilter.NoteTypeId.ToString() + "' ";
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
