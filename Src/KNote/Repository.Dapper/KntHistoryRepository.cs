using System.Data.Common;
using System.Reflection;
using Dapper;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;

namespace KNote.Repository.Dapper;

public class KntHistoryRepository : KntRepositoryDapperBase, IKntHistoryRepository
{
    // The ids of an IN list per query (SQL Server allows 2100 parameters).
    private const int IdsPerQuery = 500;

    // Every column but PreviousContent, which only GetPreviousContentAsync reads.
    internal const string LightColumns = @"KLogId, EntityId, EntityName, RegistryDateTime, RegistryMessage, ActionType, LastDateTime,
        UserName, Origin, Fields, ChangeDetails, EntityTitle, PreviousContentSize";

    public KntHistoryRepository(DbConnection singletonConnection, RepositoryRef repositoryRef)
        : base(singletonConnection, repositoryRef)
    {
    }

    public KntHistoryRepository(RepositoryRef repositoryRef)
        : base(repositoryRef)
    {
    }

    public async Task<Result<KLogDto>> GetLastAsync(string entityName, Guid entityId)
    {
        try
        {
            var db = GetOpenConnection();
            var rows = await db.QueryAsync<KLogDto>($@"SELECT {LightColumns} FROM KLogs
                WHERE EntityName = @entityName AND EntityId = @entityId
                ORDER BY LastDateTime DESC, RegistryDateTime DESC", new { entityName, entityId });
            await CloseIsTempConnection(db);
            return new Result<KLogDto> { Entity = rows.FirstOrDefault() };
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<List<KLogDto>>> GetEntityHistoryAsync(string entityName, Guid entityId)
    {
        try
        {
            var db = GetOpenConnection();
            var rows = await db.QueryAsync<KLogDto>($@"SELECT {LightColumns} FROM KLogs
                WHERE EntityName = @entityName AND EntityId = @entityId
                ORDER BY LastDateTime DESC, RegistryDateTime DESC", new { entityName, entityId });
            await CloseIsTempConnection(db);
            return new Result<List<KLogDto>> { Entity = rows.ToList() };
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<string>> GetPreviousContentAsync(Guid klogId)
    {
        try
        {
            var db = GetOpenConnection();
            var content = await db.ExecuteScalarAsync<string>("SELECT PreviousContent FROM KLogs WHERE KLogId = @klogId", new { klogId });
            await CloseIsTempConnection(db);
            return new Result<string> { Entity = content };
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result> ClearPreviousContentAsync(List<Guid> klogIds)
    {
        try
        {
            var db = GetOpenConnection();
            foreach (var ids in (klogIds ?? new()).Distinct().Chunk(IdsPerQuery))
                await db.ExecuteAsync("UPDATE KLogs SET PreviousContent = NULL, PreviousContentSize = NULL WHERE KLogId IN @ids", new { ids });
            await CloseIsTempConnection(db);
            return new Result();
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<List<Guid>>> GetDeletedWithContentAsync(string entityName, DateTime deletedBefore)
    {
        try
        {
            var db = GetOpenConnection();
            var ids = await db.QueryAsync<Guid>(@"SELECT DISTINCT d.EntityId FROM KLogs d
                WHERE d.EntityName = @entityName AND d.ActionType = @deleted AND d.RegistryDateTime < @deletedBefore
                    AND NOT EXISTS (SELECT 1 FROM KLogs l WHERE l.EntityName = d.EntityName AND l.EntityId = d.EntityId
                        AND l.RegistryDateTime > d.RegistryDateTime)
                    AND EXISTS (SELECT 1 FROM KLogs c WHERE c.EntityName = d.EntityName AND c.EntityId = d.EntityId
                        AND c.PreviousContentSize IS NOT NULL)",
                new { entityName, deleted = nameof(EnumHistoryAction.Deleted), deletedBefore });
            await CloseIsTempConnection(db);
            return new Result<List<Guid>> { Entity = ids.ToList() };
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<List<KLogDto>>> GetDeletedAsync(string entityName)
    {
        try
        {
            var db = GetOpenConnection();
            var rows = await db.QueryAsync<KLogDto>($@"SELECT {LightColumns} FROM KLogs d
                WHERE d.EntityName = @entityName AND d.ActionType = @deleted AND d.PreviousContentSize IS NOT NULL
                    AND NOT EXISTS (SELECT 1 FROM KLogs l WHERE l.EntityName = d.EntityName AND l.EntityId = d.EntityId
                        AND l.RegistryDateTime > d.RegistryDateTime)
                ORDER BY d.LastDateTime DESC", new { entityName, deleted = nameof(EnumHistoryAction.Deleted) });
            await CloseIsTempConnection(db);
            return new Result<List<KLogDto>> { Entity = rows.ToList() };
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<KLogPageDto>> GetChangesAsync(DateTime from, DateTime toExclusive, IReadOnlyCollection<string> entityNames,
        string actionType, string userName, string origin, int skip, int take)
    {
        try
        {
            var db = GetOpenConnection();
            var (where, parameters) = ChangesWhere(from, toExclusive, entityNames, actionType, userName, origin);
            var total = await db.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM KLogs WHERE {where}", parameters);

            var sql = $"SELECT {LightColumns} FROM KLogs WHERE {where} ORDER BY LastDateTime DESC, RegistryDateTime DESC, KLogId";
            sql += db.GetType().Name == "SqliteConnection" ? " LIMIT @take OFFSET @skip" : " OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY";
            parameters.Add("skip", Math.Max(skip, 0));
            parameters.Add("take", Math.Max(take, 1));
            var rows = await db.QueryAsync<KLogDto>(sql, parameters);
            await CloseIsTempConnection(db);
            return new Result<KLogPageDto> { Entity = new KLogPageDto { Rows = rows.ToList(), Total = total } };
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<List<string>>> GetChangeUserNamesAsync(DateTime from, DateTime toExclusive, IReadOnlyCollection<string> entityNames)
    {
        try
        {
            var db = GetOpenConnection();
            var (where, parameters) = ChangesWhere(from, toExclusive, entityNames, null, null, null);
            var names = await db.QueryAsync<string>(
                $"SELECT DISTINCT UserName FROM KLogs WHERE {where} AND UserName IS NOT NULL AND UserName <> '' ORDER BY UserName", parameters);
            await CloseIsTempConnection(db);
            return new Result<List<string>> { Entity = names.ToList() };
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    // The conditions of «Repository changes»: the period by LastDateTime (its index) and the optional filters.
    private static (string Where, DynamicParameters Parameters) ChangesWhere(DateTime from, DateTime toExclusive,
        IReadOnlyCollection<string> entityNames, string actionType, string userName, string origin)
    {
        var parameters = new DynamicParameters();
        var where = "LastDateTime >= @from AND LastDateTime < @toExclusive";
        parameters.Add("from", from, System.Data.DbType.DateTime2);
        parameters.Add("toExclusive", toExclusive, System.Data.DbType.DateTime2);
        if (entityNames?.Count > 0)
        {
            where += " AND EntityName IN @entityNames";
            parameters.Add("entityNames", entityNames.ToList());
        }
        if (!string.IsNullOrEmpty(actionType))
        {
            where += " AND ActionType = @actionType";
            parameters.Add("actionType", actionType);
        }
        if (!string.IsNullOrEmpty(userName))
        {
            where += " AND UserName = @userName";
            parameters.Add("userName", userName);
        }
        if (!string.IsNullOrEmpty(origin))
        {
            where += " AND Origin = @origin";
            parameters.Add("origin", origin);
        }
        return (where, parameters);
    }

    public async Task<Result> ClearEntityContentAsync(string entityName, Guid entityId)
    {
        try
        {
            var db = GetOpenConnection();
            await db.ExecuteAsync(@"UPDATE KLogs SET PreviousContent = NULL, PreviousContentSize = NULL
                WHERE EntityName = @entityName AND EntityId = @entityId", new { entityName, entityId });
            await CloseIsTempConnection(db);
            return new Result();
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result> WriteAsync(KLogWriteDto write)
    {
        try
        {
            if (write?.Row == null)
                return new Result();
            var db = GetOpenConnection();
            try
            {
                await using var transaction = await BeginTransactionAsync(db);
                await KntHistoryWriter.WriteAsync(db, transaction, write);
                await transaction.CommitAsync();
            }
            finally
            {
                await CloseIsTempConnection(db);
            }
            return new Result();
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }
}

/// <summary>
/// Writes a row of the history (KLogWriteDto) in the transaction of the change it notes (H10), on its connection.
/// </summary>
internal static class KntHistoryWriter
{
    public static async Task WriteAsync(DbConnection db, DbTransaction transaction, KLogWriteDto write)
    {
        if (write?.Row == null)
            return;

        if (write.MergeIntoKLogId is Guid klogId)
        {
            var row = write.Row;
            var parameters = new DynamicParameters(new
            {
                Fields = (int)row.Fields,
                row.RegistryMessage,
                row.ChangeDetails,
                row.EntityTitle,
                Size = write.PreviousContent == null ? (int?)null : HistoryRules.ContentSize(write.PreviousContent),
                Content = write.PreviousContent,
                KLogId = klogId
            });
            AddDate(parameters, "LastDateTime", row.LastDateTime);
            AddDate(parameters, "ExpectedLastDateTime", write.ExpectedLastDateTime);
            var merged = await db.ExecuteAsync(@"UPDATE KLogs SET
                    LastDateTime = @LastDateTime, Fields = @Fields, RegistryMessage = @RegistryMessage,
                    ChangeDetails = @ChangeDetails, EntityTitle = @EntityTitle,
                    PreviousContentSize = CASE WHEN PreviousContent IS NULL THEN @Size ELSE PreviousContentSize END,
                    PreviousContent = CASE WHEN PreviousContent IS NULL THEN @Content ELSE PreviousContent END
                WHERE KLogId = @KLogId AND LastDateTime = @ExpectedLastDateTime",
                parameters, transaction);
            if (merged == 1)
                return;

            // Another write came first: this change alone, as a new row.
            await InsertAsync(db, transaction, write.Standalone ?? row, write.StandalonePreviousContent);
            return;
        }

        await InsertAsync(db, transaction, write.Row, write.PreviousContent);
    }

    private static async Task InsertAsync(DbConnection db, DbTransaction transaction, KLogDto row, string previousContent)
    {
        if (row.KLogId == Guid.Empty)
            row.KLogId = Guid.NewGuid();

        var parameters = new DynamicParameters(new
        {
            row.KLogId,
            row.EntityId,
            row.EntityName,
            RegistryMessage = row.RegistryMessage ?? "",
            row.ActionType,
            row.UserName,
            row.Origin,
            Fields = (int)row.Fields,
            row.ChangeDetails,
            row.EntityTitle,
            Size = previousContent == null ? (int?)null : HistoryRules.ContentSize(previousContent),
            Content = previousContent
        });
        AddDate(parameters, "RegistryDateTime", row.RegistryDateTime);
        AddDate(parameters, "LastDateTime", row.LastDateTime);

        await db.ExecuteAsync(@"INSERT INTO KLogs
                (KLogId, EntityId, EntityName, RegistryDateTime, RegistryMessage, ActionType, LastDateTime, UserName, Origin, Fields,
                ChangeDetails, EntityTitle, PreviousContentSize, PreviousContent)
            VALUES
                (@KLogId, @EntityId, @EntityName, @RegistryDateTime, @RegistryMessage, @ActionType, @LastDateTime, @UserName, @Origin, @Fields,
                @ChangeDetails, @EntityTitle, @Size, @Content)",
            parameters, transaction);
    }

    // The dates of the history, whole (datetime2 in SQL Server: Dapper's default, datetime, would round the milliseconds and
    // the row of a session wouldn't be found again by its LastDateTime). Sqlite writes them as text, as EF does.
    private static void AddDate(DynamicParameters parameters, string name, DateTime? value) =>
        parameters.Add(name, value, System.Data.DbType.DateTime2);
}
