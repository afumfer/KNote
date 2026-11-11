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

public class KntSemanticIndexRepository : KntRepositoryEFBase, IKntSemanticIndexRepository
{
    // The ids of a Contains per query, as the Dapper repository.
    private const int IdsPerQuery = 500;

    public KntSemanticIndexRepository(KntDbContext singletonContext, RepositoryRef repositoryRef)
        : base(singletonContext, repositoryRef)
    {
    }

    public KntSemanticIndexRepository(RepositoryRef repositoryRef)
        : base(repositoryRef)
    {
    }

    public async Task<Result<List<NoteChunkDto>>> GetNoteChunksAsync(Guid noteId, string model)
    {
        try
        {
            var ctx = GetOpenConnection();

            var rows = await ctx.NoteChunks.AsNoTracking()
                .Where(c => c.NoteId == noteId && c.Model == model)
                .OrderBy(c => c.SourceType).ThenBy(c => c.SourceId).ThenBy(c => c.ChunkIndex)
                .ToListAsync();
            var result = new Result<List<NoteChunkDto>> { Entity = rows.Select(r => ToDto(r, withVector: true)).ToList() };

            await CloseIsTempConnection(ctx);

            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result> ReplaceNoteChunksAsync(Guid noteId, string model, List<NoteChunkDto> chunks)
    {
        try
        {
            var ctx = GetOpenConnection();
            try
            {
                await using var transaction = await ctx.Database.BeginTransactionAsync();

                await ctx.NoteChunks.Where(c => c.NoteId == noteId && c.Model == model).ExecuteDeleteAsync();

                ctx.NoteChunks.AddRange(chunks.Select(c => new NoteChunk
                {
                    NoteChunkId = c.NoteChunkId == Guid.Empty ? Guid.NewGuid() : c.NoteChunkId,
                    NoteId = noteId,
                    SourceType = (int)c.SourceType,
                    SourceId = c.SourceId,
                    ChunkIndex = c.ChunkIndex,
                    Page = c.Page,
                    Content = c.Content,
                    ContentHash = c.ContentHash,
                    SourceHash = c.SourceHash,
                    Stale = false,
                    Model = model,
                    Dimensions = c.Vector?.Length ?? 0,
                    Vector = EmbeddingVector.ToBytes(c.Vector ?? []),
                    IndexedDateTime = c.IndexedDateTime
                }));
                await ctx.SaveChangesAsync();

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

    public Task<Result> MarkStaleAsync(Guid noteId) =>
        MarkStaleAsync(ctx => ctx.NoteChunks.Where(c => c.NoteId == noteId));

    public Task<Result> MarkModelStaleAsync(string model) =>
        MarkStaleAsync(ctx => ctx.NoteChunks.Where(c => c.Model == model));

    public async Task<Result> MarkStaleByFoldersAsync(List<Guid> folderIds)
    {
        foreach (var ids in (folderIds ?? new()).Distinct().Chunk(IdsPerQuery))
        {
            var result = await MarkStaleAsync(ctx => ctx.NoteChunks
                .Where(c => ctx.Notes.Any(n => n.NoteId == c.NoteId && ids.Contains(n.FolderId))));
            if (!result.IsValid)
                return result;
        }
        return new Result();
    }

    public Task<Result> MarkStaleByNoteTypeAsync(Guid noteTypeId) =>
        MarkStaleAsync(ctx => ctx.NoteChunks.Where(c => ctx.Notes.Any(n => n.NoteId == c.NoteId && n.NoteTypeId == noteTypeId)));

    public Task<Result> MarkStaleByKAttributeAsync(Guid kattributeId) =>
        MarkStaleAsync(ctx => ctx.NoteChunks
            .Where(c => ctx.NoteKAttributes.Any(a => a.NoteId == c.NoteId && a.KAttributeId == kattributeId)));

    public async Task<Result> DeleteModelAsync(string model)
    {
        try
        {
            var ctx = GetOpenConnection();

            await ctx.NoteChunks.Where(c => c.Model == model).ExecuteDeleteAsync();

            await CloseIsTempConnection(ctx);

            return new Result();
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<List<Guid>>> GetPendingNoteIdsAsync(string model, int max, List<Guid> excludedNoteTypeIds = null)
    {
        try
        {
            var ctx = GetOpenConnection();

            var ids = await Pending(ctx, NotExcluded(ctx, excludedNoteTypeIds), model)
                .OrderByDescending(n => n.ModificationDateTime).ThenByDescending(n => n.NoteNumber)
                .Select(n => n.NoteId)
                .Take(max)
                .ToListAsync();
            var result = new Result<List<Guid>> { Entity = ids };

            await CloseIsTempConnection(ctx);

            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<SemanticIndexSummaryDto>> GetIndexSummaryAsync(string model, List<Guid> excludedNoteTypeIds = null)
    {
        try
        {
            var ctx = GetOpenConnection();

            var notes = NotExcluded(ctx, excludedNoteTypeIds);
            var summary = new SemanticIndexSummaryDto
            {
                TotalNotes = await notes.CountAsync(),
                IndexedNotes = await notes.CountAsync(n => ctx.NoteChunks.Any(c => c.NoteId == n.NoteId && c.Model == model)),
                PendingNotes = await Pending(ctx, notes, model).CountAsync(),
                Chunks = await ctx.NoteChunks.CountAsync(c => c.Model == model)
            };

            await CloseIsTempConnection(ctx);

            return new Result<SemanticIndexSummaryDto> { Entity = summary };
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<List<SemanticIndexModelDto>>> GetIndexModelsAsync()
    {
        try
        {
            var ctx = GetOpenConnection();

            var models = await ctx.NoteChunks.AsNoTracking()
                .GroupBy(c => c.Model)
                .Select(g => new SemanticIndexModelDto
                {
                    IndexKey = g.Key,
                    Chunks = g.Count(),
                    Notes = g.Select(c => c.NoteId).Distinct().Count()
                })
                .OrderBy(m => m.IndexKey)
                .ToListAsync();

            await CloseIsTempConnection(ctx);

            return new Result<List<SemanticIndexModelDto>> { Entity = models };
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<List<NoteChunkVectorDto>>> GetVectorsAsync(string model, DateTime? changedSince = null)
    {
        try
        {
            var ctx = GetOpenConnection();

            var query = ctx.NoteChunks.AsNoTracking().Where(c => c.Model == model);
            if (changedSince != null)
                query = query.Where(c => c.IndexedDateTime >= changedSince.Value);
            var rows = await query
                .Select(c => new { c.NoteChunkId, c.NoteId, c.SourceType, c.SourceId, c.Vector })
                .ToListAsync();
            var result = new Result<List<NoteChunkVectorDto>>
            {
                Entity = rows.Select(r => new NoteChunkVectorDto
                {
                    NoteChunkId = r.NoteChunkId,
                    NoteId = r.NoteId,
                    SourceType = (NoteChunkSourceType)r.SourceType,
                    SourceId = r.SourceId,
                    Vector = EmbeddingVector.FromBytes(r.Vector)
                }).ToList()
            };

            await CloseIsTempConnection(ctx);

            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<SemanticIndexWatermarkDto>> GetIndexWatermarkAsync(string model)
    {
        try
        {
            var ctx = GetOpenConnection();

            var rows = ctx.NoteChunks.AsNoTracking().Where(c => c.Model == model);
            var result = new Result<SemanticIndexWatermarkDto>
            {
                Entity = new SemanticIndexWatermarkDto
                {
                    Count = await rows.CountAsync(),
                    LastIndexedDateTime = await rows.MaxAsync(c => (DateTime?)c.IndexedDateTime)
                }
            };

            await CloseIsTempConnection(ctx);

            return result;
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    public async Task<Result<List<NoteChunkDto>>> GetChunkContentsAsync(List<Guid> noteChunkIds)
    {
        try
        {
            var ctx = GetOpenConnection();

            var chunks = new List<NoteChunkDto>();
            foreach (var ids in (noteChunkIds ?? new()).Distinct().Chunk(IdsPerQuery))
            {
                var rows = await ctx.NoteChunks.AsNoTracking()
                    .Where(c => ids.Contains(c.NoteChunkId))
                    .Select(c => new NoteChunk
                    {
                        NoteChunkId = c.NoteChunkId,
                        NoteId = c.NoteId,
                        SourceType = c.SourceType,
                        SourceId = c.SourceId,
                        ChunkIndex = c.ChunkIndex,
                        Page = c.Page,
                        Content = c.Content,
                        ContentHash = c.ContentHash,
                        SourceHash = c.SourceHash,
                        Stale = c.Stale,
                        Model = c.Model,
                        Dimensions = c.Dimensions,
                        IndexedDateTime = c.IndexedDateTime
                    })
                    .ToListAsync();
                chunks.AddRange(rows.Select(r => ToDto(r, withVector: false)));
            }

            await CloseIsTempConnection(ctx);

            return new Result<List<NoteChunkDto>> { Entity = chunks };
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    private async Task<Result> MarkStaleAsync(Func<KntDbContext, IQueryable<NoteChunk>> rows)
    {
        try
        {
            var ctx = GetOpenConnection();

            await rows(ctx).ExecuteUpdateAsync(s => s.SetProperty(c => c.Stale, true));

            await CloseIsTempConnection(ctx);

            return new Result();
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    // The notes not of the excluded note types (the Dapper repository's NotExcluded).
    private static IQueryable<Note> NotExcluded(KntDbContext ctx, List<Guid> excludedNoteTypeIds)
    {
        var notes = ctx.Notes.AsNoTracking();
        if (excludedNoteTypeIds?.Count > 0)
            notes = notes.Where(n => n.NoteTypeId == null || !excludedNoteTypeIds.Contains(n.NoteTypeId.Value));
        return notes;
    }

    // The notes the index of a model has to calculate (the Dapper repository's Pending).
    private static IQueryable<Note> Pending(KntDbContext ctx, IQueryable<Note> notes, string model) =>
        notes.Where(n => !ctx.NoteChunks.Any(c => c.NoteId == n.NoteId && c.Model == model)
            || ctx.NoteChunks.Any(c => c.NoteId == n.NoteId && c.Model == model
                && (c.Stale || c.IndexedDateTime < n.ModificationDateTime)));

    private static NoteChunkDto ToDto(NoteChunk row, bool withVector) => new()
    {
        NoteChunkId = row.NoteChunkId,
        NoteId = row.NoteId,
        SourceType = (NoteChunkSourceType)row.SourceType,
        SourceId = row.SourceId,
        ChunkIndex = row.ChunkIndex,
        Page = row.Page,
        Content = row.Content,
        ContentHash = row.ContentHash,
        SourceHash = row.SourceHash,
        Stale = row.Stale,
        Model = row.Model,
        Dimensions = row.Dimensions,
        Vector = withVector ? EmbeddingVector.FromBytes(row.Vector) : null,
        IndexedDateTime = row.IndexedDateTime
    };
}
