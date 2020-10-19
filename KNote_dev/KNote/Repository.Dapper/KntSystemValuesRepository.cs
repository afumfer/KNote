using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using System.Data.Common;
using Dapper;

using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Repository.Dapper
{

    // TODO: Pendiente de probar

    public class KntSystemValuesRepository : DomainActionBase, IKntSystemValuesRepository
    {
        protected DbConnection _db;

        public KntSystemValuesRepository(DbConnection db, bool throwKntException)
        {
            _db = db;
            ThrowKntException = throwKntException;
        }

        public async Task<Result<List<SystemValueDto>>> GetAllAsync()
        {
            var result = new Result<List<SystemValueDto>>();
            try
            {
                var sql = @"SELECT SystemValueId, Scope, [Key], [Value] FROM [SystemValues] ORDER BY Scope, [Key];";

                var entity = await _db.QueryAsync<SystemValueDto>(sql.ToString(), new { });
                result.Entity = entity.ToList();
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<SystemValueDto>> GetAsync(string scope, string key)
        {
            var result = new Result<SystemValueDto>();
            try
            {
                var sql = @"SELECT  SystemValueId, Scope, [Key], [Value] FROM [SystemValues] 
                        WHERE Scope = @Scope and [Key] = @Key ";

                var entity = await _db.QueryFirstOrDefaultAsync<SystemValueDto>(sql.ToString(), new { Scope = scope, Key = key });

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

        public async Task<Result<SystemValueDto>> GetAsync(Guid id)
        {
            var result = new Result<SystemValueDto>();
            try
            {
                var sql = @"SELECT  SystemValueId, Scope, [Key], [Value] FROM [SystemValues] 
                        WHERE Id = @Id ";

                var entity = await _db.QueryFirstOrDefaultAsync<SystemValueDto>(sql.ToString(), new { Id = id });

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

        public async Task<Result<SystemValueDto>> AddAsync(SystemValueDto entity)
        {
            var result = new Result<SystemValueDto>();
            try
            {
                var sql = @"INSERT INTO SystemValues (SystemValueId, Scope, [Key], [Value])
                            VALUES (@SystemValueId, @Scope, @Key, @Value)";

                var r = await _db.ExecuteAsync(sql.ToString(),
                    new { entity.SystemValueId, entity.Scope, entity.Key, entity.Value });

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

        public async Task<Result<SystemValueDto>> UpdateAsync(SystemValueDto entity)
        {
            var result = new Result<SystemValueDto>();
            try
            {
                var sql = @"UPDATE SystemValues SET                     
                        Scope = @Scope, 
                        [Key] = @Key, 
                        [Value] = @Value
                    WHERE SystemValueId = @SystemValueId";

                var r = await _db.ExecuteAsync(sql.ToString(),
                    new { entity.SystemValueId, entity.Scope, entity.Key, entity.Value });

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
                var sql = @"DELETE FROM SystemValues WHERE SystemValueId = @Id";

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
