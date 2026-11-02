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

    public async Task<Result> MarkStaleAsync(Guid noteId)
    {
        try
        {
            var ctx = GetOpenConnection();

            await ctx.NoteChunks.Where(c => c.NoteId == noteId).ExecuteUpdateAsync(s => s.SetProperty(c => c.Stale, true));

            await CloseIsTempConnection(ctx);

            return new Result();
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

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

            var notes = ctx.Notes.AsNoTracking();
            if (excludedNoteTypeIds?.Count > 0)
                notes = notes.Where(n => n.NoteTypeId == null || !excludedNoteTypeIds.Contains(n.NoteTypeId.Value));

            var ids = await notes
                .Where(n => !ctx.NoteChunks.Any(c => c.NoteId == n.NoteId && c.Model == model)
                    || ctx.NoteChunks.Any(c => c.NoteId == n.NoteId && c.Model == model
                        && (c.Stale || c.IndexedDateTime < n.ModificationDateTime)))
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

    public async Task<Result<List<NoteChunkVectorDto>>> GetVectorsAsync(string model, DateTime? changedSince = null)
    {
        try
        {
            var ctx = GetOpenConnection();

            var query = ctx.NoteChunks.AsNoTracking().Where(c => c.Model == model);
            if (changedSince != null)
                query = query.Where(c => c.IndexedDateTime >= changedSince.Value);
            var rows = await query
                .Select(c => new { c.NoteChunkId, c.NoteId, c.SourceType, c.Vector })
                .ToListAsync();
            var result = new Result<List<NoteChunkVectorDto>>
            {
                Entity = rows.Select(r => new NoteChunkVectorDto
                {
                    NoteChunkId = r.NoteChunkId,
                    NoteId = r.NoteId,
                    SourceType = (NoteChunkSourceType)r.SourceType,
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

    private static NoteChunkDto ToDto(NoteChunk row, bool withVector) => new()
    {
        NoteChunkId = row.NoteChunkId,
        NoteId = row.NoteId,
        SourceType = (NoteChunkSourceType)row.SourceType,
        SourceId = row.SourceId,
        ChunkIndex = row.ChunkIndex,
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
