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
    public class KntKAttributeRepository : DomainActionBase, IKntKAttributeRepository
    {
        protected SqlConnection _db;

        public KntKAttributeRepository(SqlConnection db, bool throwKntException)
        {
            _db = db;
            ThrowKntException = throwKntException;
        }

        public async Task<Result<List<KAttributeInfoDto>>> GetAllAsync()
        {
            return await GetAllAsync(false, null, false);
        }

        public async Task<Result<List<KAttributeInfoDto>>> GetAllAsync(Guid? typeId)
        {            
            return await GetAllAsync(true, typeId, false);
        }

        public async Task<Result<List<KAttributeInfoDto>>> GetAllIncludeNullTypeAsync(Guid? typeId)
        {            
            return await GetAllAsync(true, typeId, true);
        }

        private async Task<Result<List<KAttributeInfoDto>>> GetAllAsync(bool applyFilter, Guid? typeId, bool includeNullType)
        {
            var result = new Result<List<KAttributeInfoDto>>();
            try
            {                
                var sql = @"SELECT        
	                        KA.KAttributeId, 
	                        KA.Name, 
	                        KA.Description, 
	                        KA.KAttributeDataType, 
	                        KA.RequiredValue, 
	                        KA.[Order], 
	                        KA.Script, 
	                        KA.Disabled, 	
                            KA.NoteTypeId, 
	                        NT.NoteTypeId, 
	                        NT.Name, 
	                        NT.Description, 
	                        NT.ParenNoteTypeId 
                        FROM
	                        KAttributes KA LEFT OUTER JOIN  NoteTypes NT
	                        ON KA.NoteTypeId = NT.NoteTypeId";

                if (applyFilter)
                {
                    sql += " WHERE KA.NoteTypeId = @NoteTypeId ";
                    if (includeNullType)
                        sql += " AND NoteTypeId is null ";                    
                }
                    
                sql += " ORDER BY NT.Name, [Order], KA.Name";

                var entity = await _db.QueryAsync<KAttributeInfoDto, NoteTypeDto, KAttributeInfoDto>(
                    sql.ToString(),
                    (kAttribute, noteType) =>
                    {
                        kAttribute.NoteTypeDto = noteType;
                        return kAttribute;
                    },
                    new { NoteTypeId = typeId }
                    , splitOn: "NoteTypeId"
                    );

                result.Entity = entity.ToList();
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public Task<Result<KAttributeDto>> GetAsync(Guid id)
        {
            // NOTA: Para obtener objetos complejos se puede usar query y luego
            //       el FirstOrDefault de dicho query. 

            throw new NotImplementedException();
        }

        public Task<Result<List<KAttributeTabulatedValueDto>>> GetKAttributeTabulatedValuesAsync(Guid attributeId)
        {
            throw new NotImplementedException();
        }

        public Task<Result<KAttributeDto>> SaveAsync(KAttributeDto entityInfo)
        {
            throw new NotImplementedException();
        }

        public Task<Result<KAttributeTabulatedValueDto>> SaveTabulateValueAsync(Guid attributeId, KAttributeTabulatedValueDto entityInfo)
        {
            throw new NotImplementedException();
        }

        public Task<Result<KAttributeTabulatedValueDto>> AddNewKAttributeTabulatedValueAsync(Guid id, KAttributeTabulatedValueDto entityInfo)
        {
            throw new NotImplementedException();
        }

        public Task<Result<KAttributeInfoDto>> DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Result<KAttributeTabulatedValueDto>> DeleteKAttributeTabulatedValueAsync(Guid id)
        {
            throw new NotImplementedException();
        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }

    }
}
