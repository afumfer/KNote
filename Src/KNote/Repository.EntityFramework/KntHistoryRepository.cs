using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace KNote.Repository.EntityFramework;

public class KntHistoryRepository : KntRepositoryEFBase, IKntHistoryRepository
{
    public KntHistoryRepository(KntDbContext singletonContext, RepositoryRef repositoryRef)
        : base(singletonContext, repositoryRef)
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
            var ctx = GetOpenConnection();
            var row = await ctx.KLogs.AsNoTracking()
                .Where(l => l.EntityName == entityName && l.EntityId == entityId)
                .OrderByDescending(l => l.LastDateTime).ThenByDescending(l => l.RegistryDateTime)
                .FirstOrDefaultAsync();
            await CloseIsTempConnection(ctx);
            return new Result<KLogDto> { Entity = row == null ? null : ToDto(row) };
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
            var ctx = GetOpenConnection();
            var rows = await ctx.KLogs.AsNoTracking()
                .Where(l => l.EntityName == entityName && l.EntityId == entityId)
                .OrderByDescending(l => l.LastDateTime).ThenByDescending(l => l.RegistryDateTime)
                .ToListAsync();
            await CloseIsTempConnection(ctx);
            return new Result<List<KLogDto>> { Entity = rows.Select(ToDto).ToList() };
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
            var ctx = GetOpenConnection();
            var content = await ctx.Database
                .SqlQuery<string>($"SELECT PreviousContent AS Value FROM KLogs WHERE KLogId = {klogId}")
                .FirstOrDefaultAsync();
            await CloseIsTempConnection(ctx);
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
            var ctx = GetOpenConnection();
            foreach (var klogId in (klogIds ?? new()).Distinct())
                await ctx.Database.ExecuteSqlAsync(
                    $"UPDATE KLogs SET PreviousContent = NULL, PreviousContentSize = NULL WHERE KLogId = {klogId}");
            await CloseIsTempConnection(ctx);
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
            var ctx = GetOpenConnection();
            var deleted = nameof(EnumHistoryAction.Deleted);
            var ids = await ctx.KLogs.AsNoTracking()
                .Where(d => d.EntityName == entityName && d.ActionType == deleted && d.RegistryDateTime < deletedBefore
                    && !ctx.KLogs.Any(l => l.EntityName == entityName && l.EntityId == d.EntityId && l.RegistryDateTime > d.RegistryDateTime)
                    && ctx.KLogs.Any(c => c.EntityName == entityName && c.EntityId == d.EntityId && c.PreviousContentSize != null))
                .Select(d => d.EntityId)
                .Distinct()
                .ToListAsync();
            await CloseIsTempConnection(ctx);
            return new Result<List<Guid>> { Entity = ids };
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result> ClearEntityContentAsync(string entityName, Guid entityId)
    {
        try
        {
            var ctx = GetOpenConnection();
            await ctx.Database.ExecuteSqlAsync(
                $"UPDATE KLogs SET PreviousContent = NULL, PreviousContentSize = NULL WHERE EntityName = {entityName} AND EntityId = {entityId}");
            await CloseIsTempConnection(ctx);
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
            var ctx = GetOpenConnection();
            try
            {
                await using var transaction = await ctx.Database.BeginTransactionAsync();
                await KntHistoryWriter.WriteAsync(ctx, write);
                await transaction.CommitAsync();
            }
            finally
            {
                await CloseIsTempConnection(ctx);
            }
            return new Result();
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    internal static KLogDto ToDto(KLog row) => new()
    {
        KLogId = row.KLogId,
        EntityId = row.EntityId,
        EntityName = row.EntityName,
        RegistryDateTime = row.RegistryDateTime,
        RegistryMessage = row.RegistryMessage,
        ActionType = row.ActionType,
        LastDateTime = row.LastDateTime,
        UserName = row.UserName,
        Origin = row.Origin,
        Fields = (EnumHistoryFields)row.Fields,
        ChangeDetails = row.ChangeDetails,
        EntityTitle = row.EntityTitle,
        PreviousContentSize = row.PreviousContentSize
    };
}

/// <summary>
/// Writes a row of the history (KLogWriteDto) with the context of the change it notes, so it goes in the transaction the
/// caller has open on it (H10). SQL of its own: PreviousContent is not in the EF model.
/// </summary>
internal static class KntHistoryWriter
{
    public static async Task WriteAsync(KntDbContext ctx, KLogWriteDto write)
    {
        if (write?.Row == null)
            return;

        if (write.MergeIntoKLogId is Guid klogId)
        {
            var row = write.Row;
            var size = write.PreviousContent == null ? (int?)null : KNote.Model.Core.HistoryRules.ContentSize(write.PreviousContent);
            var merged = await ctx.Database.ExecuteSqlAsync($@"UPDATE KLogs SET
                    LastDateTime = {row.LastDateTime}, Fields = {(int)row.Fields}, RegistryMessage = {row.RegistryMessage},
                    ChangeDetails = {row.ChangeDetails}, EntityTitle = {row.EntityTitle},
                    PreviousContentSize = CASE WHEN PreviousContent IS NULL THEN {size} ELSE PreviousContentSize END,
                    PreviousContent = CASE WHEN PreviousContent IS NULL THEN {write.PreviousContent} ELSE PreviousContent END
                WHERE KLogId = {klogId} AND LastDateTime = {write.ExpectedLastDateTime}");
            if (merged == 1)
                return;

            // Another write came first: this change alone, as a new row.
            await InsertAsync(ctx, write.Standalone ?? row, write.StandalonePreviousContent);
            return;
        }

        await InsertAsync(ctx, write.Row, write.PreviousContent);
    }

    private static async Task InsertAsync(KntDbContext ctx, KLogDto row, string previousContent)
    {
        if (row.KLogId == Guid.Empty)
            row.KLogId = Guid.NewGuid();
        var size = previousContent == null ? (int?)null : KNote.Model.Core.HistoryRules.ContentSize(previousContent);

        await ctx.Database.ExecuteSqlAsync($@"INSERT INTO KLogs
                (KLogId, EntityId, EntityName, RegistryDateTime, RegistryMessage, ActionType, LastDateTime, UserName, Origin, Fields,
                ChangeDetails, EntityTitle, PreviousContentSize, PreviousContent)
            VALUES
                ({row.KLogId}, {row.EntityId}, {row.EntityName}, {row.RegistryDateTime}, {row.RegistryMessage ?? ""}, {row.ActionType},
                {row.LastDateTime}, {row.UserName}, {row.Origin}, {(int)row.Fields},
                {row.ChangeDetails}, {row.EntityTitle}, {size}, {previousContent})");
    }

}
