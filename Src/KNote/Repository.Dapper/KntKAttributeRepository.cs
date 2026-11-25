using System.Data.Common;
using Dapper;
using KNote.Model;
using KNote.Model.Dto;
using System.Transactions;
using static Dapper.SqlMapper;
using System.Reflection;

namespace KNote.Repository.Dapper;

public class KntKAttributeRepository : KntRepositoryDapperBase, IKntKAttributeRepository
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
        try
        {
            var result = new Result<KAttributeDto>();

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
    
            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    // The attribute, its tabulated values and its row of the history, in one transaction of the connection: an ambient
    // TransactionScope does nothing in Sqlite (its connections don't enlist), so they weren't saved together there.
    public async Task<Result<KAttributeDto>> AddAsync(KAttributeDto entity, KLogWriteDto history = null)
    {
        try
        {
            var result = new Result<KAttributeDto>();
            var db = GetOpenConnection();
            DbTransaction transaction = null;
            try
            {
                transaction = await BeginTransactionAsync(db);

                var sql = @"INSERT INTO KAttributes  (KAttributeId, [Name], Description, KAttributeDataType, RequiredValue, [Order], Script, Disabled, NoteTypeId)
                            VALUES (@KAttributeId, @Name, @Description, @KAttributeDataType, @RequiredValue, @Order, @Script, @Disabled, @NoteTypeId)";

                var r = await db.ExecuteAsync(sql.ToString(),
                    new { entity.KAttributeId, entity.Name, entity.Description, entity.KAttributeDataType, entity.RequiredValue, entity.Order, entity.Script, entity.Disabled, entity.NoteTypeId },
                    transaction);
                if (r == 0)
                    result.AddErrorMessage("Entity not inserted");

                var resTabValues = await SaveTabulateValueAsync(db, transaction, entity.KAttributeId, entity.KAttributeValues);
                if (!resTabValues.IsValid)
                    result.AddListErrorMessage(resTabValues.ListErrorMessage);

                result.Entity = entity;

                if (result.IsValid)
                {
                    await KntHistoryWriter.WriteAsync(db, transaction, history);
                    await transaction.CommitAsync();
                }
            }
            finally
            {
                if (transaction != null)
                    await transaction.DisposeAsync();
                await CloseIsTempConnection(db);
            }

            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<KAttributeDto>> UpdateAsync(KAttributeDto entity, KLogWriteDto history = null)
    {
        try
        {
            var result = new Result<KAttributeDto>();
            var db = GetOpenConnection();
            DbTransaction transaction = null;
            try
            {
                transaction = await BeginTransactionAsync(db);

                // Check notetype in notes.
                if (entity.NoteTypeId != null)
                {
                    var sqlType = @"SELECT NoteTypeId FROM KAttributes WHERE KAttributeId = @KAttributeId";
                    var noteTypeOld = await db.QueryFirstOrDefaultAsync<Guid?>(sqlType, new { KAttributeId = entity.KAttributeId }, transaction);

                    if (noteTypeOld != entity.NoteTypeId && await CountNoteUsagesAsync(db, entity.KAttributeId, transaction) > 0)
                    {
                        result.AddErrorMessage("You can not change the note type for this attribute. This attribute is already being used by several notes. ");
                        result.Entity = entity;
                        return result;
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
                    new { entity.KAttributeId, entity.Name, entity.Description, entity.KAttributeDataType, entity.RequiredValue, entity.Order, entity.Script, entity.Disabled, entity.NoteTypeId },
                    transaction);
                if (r == 0)
                    result.AddErrorMessage("Entity not updated");

                var resTabValues = await SaveTabulateValueAsync(db, transaction, entity.KAttributeId, entity.KAttributeValues);
                if (!resTabValues.IsValid)
                    result.AddListErrorMessage(resTabValues.ListErrorMessage);

                result.Entity = entity;

                if (result.IsValid)
                {
                    await KntHistoryWriter.WriteAsync(db, transaction, history);
                    await transaction.CommitAsync();
                }
            }
            finally
            {
                if (transaction != null)
                    await transaction.DisposeAsync();
                await CloseIsTempConnection(db);
            }

            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result> DeleteAsync(Guid id, KLogWriteDto history = null)
    {
        try
        {
            var result = new Result();

            var db = GetOpenConnection();

            var sql = @"DELETE FROM KAttributes WHERE KAttributeId = @Id";
            var r = await ExecuteNotedAsync(db, sql.ToString(), new { Id = id }, history);
            if (r == 0)
                result.AddErrorMessage("Entity not deleted");

            await CloseIsTempConnection(db);
            
            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<int>> CountNoteUsagesAsync(Guid kattributeId)
    {
        try
        {
            var result = new Result<int>();

            var db = GetOpenConnection();

            result.Entity = await CountNoteUsagesAsync(db, kattributeId);

            await CloseIsTempConnection(db);

            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<List<KAttributeTabulatedValueDto>>> GetKAttributeTabulatedValuesAsync(Guid attributeId)
    {
        try
        {
            var result = new Result<List<KAttributeTabulatedValueDto>>();

            var db = GetOpenConnection();

            var sql = @"SELECT KAttributeTabulatedValueId, KAttributeId, [Value], [Description], [Order] 
                        FROM [KAttributeTabulatedValues]
                        WHERE KAttributeId = @AttributeId ORDER BY [Order], [Description]";
            var entity = await db.QueryAsync<KAttributeTabulatedValueDto>(sql.ToString(), new { AttributeId = attributeId });
            result.Entity = entity.ToList();

            await CloseIsTempConnection(db);
            
            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    #region Private methods

    private async Task<Result<List<KAttributeInfoDto>>> GetAllAsync(bool applyFilter, Guid? typeId, bool includeNullType)
    {
        try
        {
            var result = new Result<List<KAttributeInfoDto>>();

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
            
            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    /// <summary>
    /// Number of NoteKAttributes rows referencing this attribute (i.e. notes that already have a
    /// value stored for it). Shared by the "can't change note type while in use" check in
    /// UpdateAsync and by KntKAttributesDeleteAsyncCommand's "can't delete while in use" business
    /// rule - takes an already-open connection so it can run inside UpdateAsync's TransactionScope.
    /// </summary>
    private async Task<int> CountNoteUsagesAsync(DbConnection db, Guid kattributeId, DbTransaction transaction = null)
    {
        var sql = "SELECT count(*) FROM NoteKAttributes WHERE KAttributeId = @KAttributeId";
        return await db.ExecuteScalarAsync<int>(sql, new { KAttributeId = kattributeId }, transaction);
    }

    // The values given are the attribute's: the others are deleted, also all of them when none is given (a value list emptied).
    private async Task<Result<List<KAttributeTabulatedValueDto>>> SaveTabulateValueAsync(DbConnection db, DbTransaction transaction, Guid kattributeId, List<KAttributeTabulatedValueDto> tabulatedValues)
    {
        try
        {                
            var result = new Result<List<KAttributeTabulatedValueDto>>();
            var idsTabValues = new List<Guid>();
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

            foreach (var tv in tabulatedValues ?? new List<KAttributeTabulatedValueDto>())
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
                        var countTabValue = await db.ExecuteScalarAsync<long>(sqlCount, new { KAttributeTabulatedValueId = tv.KAttributeTabulatedValueId }, transaction);
                        sql = (countTabValue > 0) ? sqlUpdate : sqlInsert;
                    }

                    r = await db.ExecuteAsync(sql.ToString(),
                        new { tv.KAttributeTabulatedValueId, tv.KAttributeId, tv.Value, tv.Description, tv.Order }, transaction);

                    if (r == 0)
                        result.AddErrorMessage($"Entity tabulated value - {tv.KAttributeTabulatedValueId} - not updated.");

                    idsTabValues.Add(tv.KAttributeTabulatedValueId);
                }
            }

            // Without values, all of them go (before, emptying the list left them in the database).
            if (idsTabValues.Count > 0)
                await db.ExecuteAsync("DELETE FROM [KAttributeTabulatedValues] WHERE KAttributeId = @KAttributeId AND KAttributeTabulatedValueId NOT IN @IdsTabValues",
                    new { KAttributeId = kattributeId, IdsTabValues = idsTabValues }, transaction);
            else
                await db.ExecuteAsync("DELETE FROM [KAttributeTabulatedValues] WHERE KAttributeId = @KAttributeId",
                    new { KAttributeId = kattributeId }, transaction);

            result.Entity = tabulatedValues;                
         
            return result;
        }        
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    #endregion
}

