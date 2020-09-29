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
            var result = new Result<List<NoteTypeDto>>();
            try
            {
                var sql = @"SELECT NoteTypeId, Name, Description, ParenNoteTypeId FROM [dbo].[NoteTypes] ORDER BY Name;";

                var entity = await _db.QueryAsync<NoteTypeDto>(sql.ToString(), new { });
                result.Entity = entity.ToList();
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<NoteTypeDto>> GetAsync(Guid id)
        {
            var result = new Result<NoteTypeDto>();
            try
            {
                var sql = @"SELECT NoteTypeId, Name, Description, ParenNoteTypeId FROM NoteTypes 
                        WHERE NoteTypeId = @Id";

                var entity =  await _db.QueryFirstOrDefaultAsync<NoteTypeDto>(sql.ToString(), new { Id = id });

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

        public async Task<Result<NoteTypeDto>> AddAsync(NoteTypeDto entity)
        {
            var result = new Result<NoteTypeDto>();
            try
            {                
                var sql = @"INSERT INTO NoteTypes (NoteTypeId, Name, Description, ParenNoteTypeId)
                            VALUES (@NoteTypeId, @Name, @Description, @ParenNoteTypeId)";

                var r = await _db.ExecuteAsync(sql.ToString(), 
                    new { entity.NoteTypeId, entity.Name, entity.Description, entity.ParenNoteTypeId });

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

        public async Task<Result<NoteTypeDto>> UpdateAsync(NoteTypeDto entity)
        {
            var result = new Result<NoteTypeDto>();
            try
            {
                var sql = @"UPDATE NoteTypes SET                     
                       Name = @Name
                      , Description = @Description
                      , ParenNoteTypeId = @ParenNoteTypeId
                    WHERE NoteTypeId = @NoteTypeId";
                
                var r = await _db.ExecuteAsync(sql.ToString(),
                    new { entity.NoteTypeId, entity.Name, entity.Description, entity.ParenNoteTypeId });

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

        public async Task<Result> DeleteAsync(Guid id)
        {
            var result = new Result();
            try
            {
                var sql = @"DELETE FROM NoteTypes WHERE NoteTypeId = @Id";

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
            //
        }

    }
}
