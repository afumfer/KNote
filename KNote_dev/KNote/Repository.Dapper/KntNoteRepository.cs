using System;
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

                result.CountColecEntity = GetCountFilter(sqlWhere);

                result.Entity = entity.ToList();
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public Task<Result<NoteDto>> GetAsync(Guid noteId)
        {
            throw new NotImplementedException();

            //var result = new Result<List<NoteInfoDto>>();
            //try
            //{
            //    var sql = @"SELECT        
	           //                 Notes.NoteId, Notes.NoteNumber, Notes.Topic, Notes.CreationDateTime, 
	           //                 Notes.ModificationDateTime, Notes.Description, Notes.ContentType, Notes.Script, 
	           //                 Notes.InternalTags, Notes.Tags, Notes.Priority, Notes.FolderId, 
	           //                 Folders.FolderId, Folders.FolderNumber, Folders.CreationDateTime, 
	           //                 Folders.ModificationDateTime, Folders.Name, Folders.Tags, Folders.PathFolder, 
	           //                 Folders.[Order], Folders.OrderNotes, 
	           //                 Folders.Script, Folders.ParentId, Notes.NoteTypeId, NoteTypes.NoteTypeId, 
	           //                 NoteTypes.Name, NoteTypes.Description, NoteTypes.ParenNoteTypeId
            //                FROM  Notes 
            //                INNER JOIN  Folders ON Notes.FolderId = Folders.FolderId 
            //                LEFT OUTER JOIN  NoteTypes ON Notes.NoteTypeId = NoteTypes.NoteTypeId
            //                WHERE FolderId = @folderId ORDER BY [Priority], Topic ;";


            //    var entity = await _db.QueryAsync<NoteInfoDto, FolderDto, NoteTypeDto, NoteInfoDto>(
            //        sql.ToString(),
            //        (note, folder, noteType) =>
            //        {
            //            note.FolderDto = folder;
            //            note.NoteTypeId = noteType;
            //            return note;
            //        },
            //        new { folderId }
            //        , splitOn: "FolderId, NoteTypeId"
            //        );

            //    result.Entity = entity.ToList();
            //}
            //catch (Exception ex)
            //{
            //    AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            //}
            //return ResultDomainAction(result);

        }

        public Task<Result<List<ResourceDto>>> GetNoteResourcesAsync(Guid idNote)
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<NoteTaskDto>>> GetNoteTasksAsync(Guid idNote)
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<NoteInfoDto>>> GetSearch(NotesSearchDto notesSearch)
        {
            throw new NotImplementedException();
        }

        public Task<Result<NoteDto>> NewAsync(NoteInfoDto entity = null)
        {
            throw new NotImplementedException();
        }

        public Task<Result<NoteDto>> SaveAsync(NoteDto entityInfo)
        {
            //var db = dbConnection();
            //var sql = @"INSERT INTO Carpetas (NombreCarpeta, IdCarpetaPadre, Orden, OrdenNotas, TipoSalidaNotas)
            //            VALUES (@NombreCarpeta, @IdCarpetaPadre, @Orden, @OrdenNotas, @TipoSalidaNotas)";
            //var result = await db.ExecuteAsync(sql.ToString(), new { carpeta.NombreCarpeta, carpeta.IdCarpetaPadre, carpeta.Orden, carpeta.OrdenNotas, carpeta.TipoSalidaNotas });
            //return result > 0;

            //var db = dbConnection();
            //var sql = @"UPDATE Carpetas SET
            //            NombreCarpeta = @NombreCarpeta
            //            , IdCarpetaPadre = @IdCarpetaPadre
            //            , Orden = @Orden
            //            , OrdenNotas = @OrdenNotas
            //            , TipoSalidaNotas = @TipoSalidaNotas 
            //            WHERE IdCarpeta = @IdCarpeta;";
            //var result = await db.ExecuteAsync(sql.ToString(),
            //                new { carpeta.IdCarpeta, carpeta.NombreCarpeta, carpeta.IdCarpetaPadre, carpeta.Orden, carpeta.OrdenNotas, carpeta.TipoSalidaNotas });
            //return result > 0;


            throw new NotImplementedException();
        }

        public Task<Result<NoteKAttributeDto>> SaveAttrtibuteAsync(NoteKAttributeDto entity)
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

        public Task<Result<NoteInfoDto>> DeleteAsync(Guid id)
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
            //return @"SELECT NoteId, NoteNumber, Topic, CreationDateTime, ModificationDateTime,            
            //                [Description], ContentType, Script, InternalTags, Tags, [Priority], FolderId, NoteTypeId
            //        FROM Notes ";

            return @"SELECT DISTINCT
                 Notes.NoteId, Notes.NoteNumber, Notes.Topic, Notes.CreationDateTime, 
                 Notes.ModificationDateTime, Notes.Description, Notes.ContentType, Notes.Script, 
                 Notes.InternalTags, Notes.Tags, Notes.Priority, Notes.FolderId, 
                 Notes.NoteTypeId
            FROM 
                Notes LEFT OUTER JOIN
                NoteKAttributes ON Notes.NoteId = NoteKAttributes.NoteId";
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
                // query = query.Where(n => n.KAttributes.Where(_ => _.KAttributeId == f.AtrId).Select(a => a.Value).Contains(f.Value));
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

        #endregion


    }
}
