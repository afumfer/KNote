using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using Microsoft.Data.SqlClient;
using Dapper;

using KNote.Model;
using KNote.Model.Dto;


namespace KNote.Repository.Dapper
{
    public class KntNoteTypeRepository : DomainActionBase, IKntNoteTypeRepository
    {
        protected SqlConnection _db;        

        public KntNoteTypeRepository(SqlConnection db, bool throwKntException)
        {
            _db = db;
            ThrowKntException = throwKntException;
        }

        public async Task<Result<List<NoteTypeDto>>> GetAllAsync()
        {
            var res = new Result<List<NoteTypeDto>>();
            try
            {
                var sql = @"SELECT NoteTypeId, Name, Description, ParenNoteTypeId FROM [dbo].[NoteTypes] ORDER BY Name";

                var entity = await _db.QueryAsync<NoteTypeDto>(sql.ToString(), new { });
                res.Entity = entity.ToList();                
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, res.ErrorList);
            }
            //return ResultDomainAction(res);
            return res;
        }

        public Task<Result<NoteTypeDto>> GetAsync(Guid id)
        {
            //var sql = @"SELECT IdCarpeta, NombreCarpeta, IdCarpetaPadre, Orden, OrdenNotas, TipoSalidaNotas 
            //            FROM dbo.Carpetas Where IdCarpeta = @Id";
            //return await db.QueryFirstOrDefaultAsync<Carpeta>(sql.ToString(), new { Id = id });

            throw new NotImplementedException();
        }

        public Task<Result<NoteTypeDto>> SaveAsync(NoteTypeDto entity)
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

        public Task<Result<NoteTypeDto>> DeleteAsync(Guid id)
        {
            //var db = dbConnection();
            //var sql = @"DELETE FROM Carpetas WHERE IdCarpeta = @Id";
            //var result = await db.ExecuteAsync(sql.ToString(), new { Id = id });
            //return result > 0;

            throw new NotImplementedException();
        }


        public void Dispose()
        {
            //
        }

    }
}
