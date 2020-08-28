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

        public KntNoteRepository(SqlConnection db, bool throwKntException)
        {
            _db = db;
            ThrowKntException = throwKntException;
        }

        public async Task<Result<List<NoteInfoDto>>> HomeNotesAsync()
        {
            var res = new Result<List<NoteInfoDto>>();

            var sql = @"SELECT IdCarpeta, NombreCarpeta, IdCarpetaPadre, Orden, OrdenNotas, TipoSalidaNotas 
                        FROM dbo.Carpetas Where IdCarpeta > 2300 order by IdCarpeta";

            var entity = await _db.QueryAsync<NoteInfoDto>(sql.ToString(), new { });

            res.Entity = entity.ToList();

            return res;

            throw new NotImplementedException();
        }

        public Task<Result<List<NoteInfoDto>>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Result<NoteDto>> GetAsync(Guid noteId)
        {
            //var sql = @"SELECT IdCarpeta, NombreCarpeta, IdCarpetaPadre, Orden, OrdenNotas, TipoSalidaNotas 
            //            FROM dbo.Carpetas Where IdCarpeta = @Id";
            //return await db.QueryFirstOrDefaultAsync<Carpeta>(sql.ToString(), new { Id = id });

            throw new NotImplementedException();
        }

        public Task<Result<List<NoteInfoDto>>> GetByFolderAsync(Guid folderId)
        {
            throw new NotImplementedException();
        }

        public Task<Result<List<NoteInfoDto>>> GetFilter(NotesFilterDto notesFilter)
        {
            throw new NotImplementedException();
        }

        public int GetNextNoteNumber()
        {
            throw new NotImplementedException();
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


    }
}
