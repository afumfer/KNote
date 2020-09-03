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

        public async Task<Result<KAttributeDto>> GetAsync(Guid id)
        {
            var result = new Result<KAttributeDto>();
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
	                NT.ParenNoteTypeId, 

	                TV.KAttributeTabulatedValueId, 
	                TV.KAttributeId, 
	                TV.Value, 
	                TV.Description,
	                TV.[Order]
                FROM
                    KAttributes AS KA LEFT OUTER JOIN
                    KAttributeTabulatedValues AS TV ON KA.KAttributeId = TV.KAttributeId LEFT OUTER JOIN
                    NoteTypes AS NT ON KA.NoteTypeId = NT.NoteTypeId
                 WHERE 
                    KA.KAttributeId = @Id
                ORDER BY 
                    TV.[Order]";

                var atrDictionary = new Dictionary<Guid, KAttributeDto>();

                var entity = await _db.QueryAsync<KAttributeDto, NoteTypeDto, KAttributeTabulatedValueDto, KAttributeDto>(
                    sql.ToString(),
                    (kAttribute, noteType, kAttributeTabulatedValueDto) =>
                    {
                        KAttributeDto attributeEntry;

                        if(!atrDictionary.TryGetValue(kAttribute.KAttributeId, out attributeEntry)){
                            attributeEntry = kAttribute;
                            attributeEntry.NoteTypeDto = noteType;
                            attributeEntry.KAttributeValues = new List<KAttributeTabulatedValueDto>();
                            atrDictionary.Add(kAttribute.KAttributeId, attributeEntry);
                        }

                        attributeEntry.KAttributeValues.Add(kAttributeTabulatedValueDto);
                        return attributeEntry;
                    },
                    new { Id = id }
                    , splitOn: "NoteTypeId, KAttributeTabulatedValueId"
                    );

                result.Entity = entity.ToList().FirstOrDefault<KAttributeDto>();
            }
            catch (Exception ex)
            {
                AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
            }
            return ResultDomainAction(result);
        }

        public async Task<Result<KAttributeDto>> AddAsync(KAttributeDto entity)
        {
            var result = new Result<KAttributeDto>();
            try
            {
                var sql = @"INSERT INTO KAttributes  (KAttributeId, [Name], Description, KAttributeDataType, RequiredValue, [Order], Script, Disabled, NoteTypeId)
                            VALUES (@KAttributeId, @Name, @Description, @KAttributeDataType, @RequiredValue, @Order, @Script, @Disabled, @NoteTypeId)";

                var r = await _db.ExecuteAsync(sql.ToString(),
                    new { entity.KAttributeId, entity.Name, entity.Description, entity.KAttributeDataType, entity.RequiredValue, entity.Order, entity.Script, entity.Disabled, entity.NoteTypeId });

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

        public async Task<Result<KAttributeDto>> UpdateAsync(KAttributeDto entity)
        {
            var result = new Result<KAttributeDto>();
            try
            {
                var sql = @"UPDATE KAttributes SET                     
                        [Name] = @Name, 
                        Description = @Description, 
                        KAttributeDataType = @KAttributeDataType, 
                        RequiredValue = @RequiredValue, 
                        [Order] = @Order, 
                        Script = @Script, 
                        Disabled = @Disabled, 
                        NoteTypeId = @NoteTypeId
                    WHERE KAttributeId = @KAttributeId";

                var r = await _db.ExecuteAsync(sql.ToString(),
                    new { entity.KAttributeId, entity.Name, entity.Description, entity.KAttributeDataType, entity.RequiredValue, entity.Order, entity.Script, entity.Disabled, entity.NoteTypeId });

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
                var sql = @"DELETE FROM KAttributes WHERE KAttributeId = @Id";

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


        public Task<Result<List<KAttributeTabulatedValueDto>>> GetKAttributeTabulatedValuesAsync(Guid attributeId)
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
