using System.Data.Common;
using Dapper;
using KNote.Model;
using KNote.Model.Dto;
using System.Transactions;

namespace KNote.Repository.Dapper;

public class KntKAttributeRepository : KntRepositoryBase, IKntKAttributeRepository
{        
    public KntKAttributeRepository(DbConnection singletonConnection, RepositoryRef repositoryRef)
        : base(singletonConnection, repositoryRef)
    {
    }

    public KntKAttributeRepository(RepositoryRef repositoryRef)
        : base(repositoryRef)
    {
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

    public async Task<Result<KAttributeDto>> GetAsync(Guid id)
    {
        var result = new Result<KAttributeDto>();
        try
        {
            var db = GetOpenConnection();

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

            var entity = await db.QueryAsync<KAttributeDto, NoteTypeDto, KAttributeTabulatedValueDto, KAttributeDto>(
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

                    if(kAttributeTabulatedValueDto != null)
                        attributeEntry.KAttributeValues.Add(kAttributeTabulatedValueDto);
                    return attributeEntry;
                },
                new { Id = id }
                , splitOn: "NoteTypeId, KAttributeTabulatedValueId"
                );

            result.Entity = entity.ToList().FirstOrDefault<KAttributeDto>();
                
            await CloseIsTempConnection(db);
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
            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var db = GetOpenConnection();

                var sql = @"INSERT INTO KAttributes  (KAttributeId, [Name], Description, KAttributeDataType, RequiredValue, [Order], Script, Disabled, NoteTypeId)
                            VALUES (@KAttributeId, @Name, @Description, @KAttributeDataType, @RequiredValue, @Order, @Script, @Disabled, @NoteTypeId)";

                var r = await db.ExecuteAsync(sql.ToString(),
                    new { entity.KAttributeId, entity.Name, entity.Description, entity.KAttributeDataType, entity.RequiredValue, entity.Order, entity.Script, entity.Disabled, entity.NoteTypeId });
                if (r == 0)
                    result.ErrorList.Add("Entity not inserted");
                
                var resTabValues = await SaveTabulateValueAsync(db, entity.KAttributeId, entity.KAttributeValues);
                if (!resTabValues.IsValid)
                    CopyErrorList(resTabValues.ErrorList, result.ErrorList);
                
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

    public async Task<Result<KAttributeDto>> UpdateAsync(KAttributeDto entity)
    {
        var result = new Result<KAttributeDto>();
        try
        {                
            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var db = GetOpenConnection();

                // Check notetype in notes.
                if (entity.NoteTypeId != null)
                {
                    var sqlType = @"SELECT NoteTypeId FROM KAttributes WHERE KAttributeId = @KAttributeId";
                    var noteTypeOld = await db.QueryFirstOrDefaultAsync<Guid?>(sqlType, new { KAttributeId = entity.KAttributeId });

                    if(noteTypeOld != entity.NoteTypeId)
                    {
                        var sqlType2 = "SELECT count(*) FROM NoteKAttributes WHERE KAttributeId = @KAttributeId";
                        var n = await db.ExecuteScalarAsync<long>(sqlType2, new { KAttributeId = entity.KAttributeId });
                        if (n > 0)
                        {
                            result.AddErrorMessage("You can not change the note type for this attribute. This attribute is already being used by several notes. ");
                            result.Entity = entity;
                            return result;
                        }
                    }
                }

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

                var r = await db.ExecuteAsync(sql.ToString(),
                    new { entity.KAttributeId, entity.Name, entity.Description, entity.KAttributeDataType, entity.RequiredValue, entity.Order, entity.Script, entity.Disabled, entity.NoteTypeId });
                if (r == 0)
                    result.ErrorList.Add("Entity not updated");

                var resTabValues = await SaveTabulateValueAsync(db, entity.KAttributeId, entity.KAttributeValues);
                if (!resTabValues.IsValid)
                    CopyErrorList(resTabValues.ErrorList, result.ErrorList);

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

            var sql = @"DELETE FROM KAttributes WHERE KAttributeId = @Id";
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

    public async Task<Result<List<KAttributeTabulatedValueDto>>> GetKAttributeTabulatedValuesAsync(Guid attributeId)
    {            
        var result = new Result<List<KAttributeTabulatedValueDto>>();
        try
        {
            var db = GetOpenConnection();

            var sql = @"SELECT KAttributeTabulatedValueId, KAttributeId, [Value], [Description], [Order] 
                        FROM [KAttributeTabulatedValues]
                        WHERE KAttributeId = @AttributeId ORDER BY [Order], [Description]";
            var entity = await db.QueryAsync<KAttributeTabulatedValueDto>(sql.ToString(), new { AttributeId = attributeId });
            result.Entity = entity.ToList();

            await CloseIsTempConnection(db);
        }
        catch (Exception ex)
        {
            AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
        }
        return ResultDomainAction(result);
    }

    #region Private methods

    private async Task<Result<List<KAttributeInfoDto>>> GetAllAsync(bool applyFilter, Guid? typeId, bool includeNullType)
    {
        var result = new Result<List<KAttributeInfoDto>>();
        try
        {
            var db = GetOpenConnection();

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
                    sql += " OR KA.NoteTypeId is null ";
            }

            sql += " ORDER BY NT.Name, [Order], KA.Name";

            var entity = await db.QueryAsync<KAttributeInfoDto, NoteTypeDto, KAttributeInfoDto>(
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

            await CloseIsTempConnection(db);
        }
        catch (Exception ex)
        {
            AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
        }
        return ResultDomainAction(result);
    }

    private async Task<Result<List<KAttributeTabulatedValueDto>>> SaveTabulateValueAsync(DbConnection db, Guid kattributeId, List<KAttributeTabulatedValueDto> tabulatedValues)
    {            
        var result = new Result<List<KAttributeTabulatedValueDto>>();
        var idsTabValues = "";
        string sql;
        string sqlInsert = @"INSERT INTO [KAttributeTabulatedValues] (KAttributeTabulatedValueId, KAttributeId, [Value], [Description], [Order]) 
                                VALUES (@KAttributeTabulatedValueId, @KAttributeId, @Value, @Description, @Order);";
        string sqlUpdate = @"UPDATE [KAttributeTabulatedValues] SET                                     
                                    KAttributeId = @KAttributeId, 
                                    [Value] = @Value, 
                                    [Description] = @Description, 
                                    [Order] = @Order  
                                WHERE KAttributeTabulatedValueId = @KAttributeTabulatedValueId ;";
        int r = 0;
        try
        {                
            foreach (var tv in tabulatedValues)
            {
                if (tv != null)
                {
                    tv.KAttributeId = kattributeId;

                    if (tv.KAttributeTabulatedValueId == Guid.Empty)
                    {
                        tv.KAttributeTabulatedValueId = Guid.NewGuid();
                        sql = sqlInsert;
                    }
                    else
                    {
                        var sqlCount = "SELECT COUNT(*) FROM KAttributeTabulatedValues WHERE KAttributeTabulatedValueId = @KAttributeTabulatedValueId";
                        var countTabValue = await db.ExecuteScalarAsync<long>(sqlCount, new { KAttributeTabulatedValueId = tv.KAttributeTabulatedValueId });
                        sql = (countTabValue > 0) ? sqlUpdate : sqlInsert;
                    }

                    r = await db.ExecuteAsync(sql.ToString(),
                        new { tv.KAttributeTabulatedValueId, tv.KAttributeId, tv.Value, tv.Description, tv.Order });

                    if (r == 0)
                        result.ErrorList.Add($"Entity tabulated value - {tv.KAttributeTabulatedValueId} - not updated.");

                    if (!string.IsNullOrEmpty(idsTabValues))
                        idsTabValues += ", ";
                    idsTabValues += "'" + tv.KAttributeTabulatedValueId + "'";
                }
            }

            sql = $"DELETE FROM [KAttributeTabulatedValues]  WHERE KAttributeId = '{kattributeId.ToString().ToUpper()}' AND KAttributeTabulatedValueId NOT IN ( {idsTabValues.ToString().ToUpper()} )";
            r = await db.ExecuteAsync(sql.ToString(),
                        new { });

            result.Entity = tabulatedValues;                
        }
        catch (Exception ex)
        {
            AddExecptionsMessagesToErrorsList(ex, result.ErrorList);
        }
        return ResultDomainAction(result);
    }

    #endregion
}

