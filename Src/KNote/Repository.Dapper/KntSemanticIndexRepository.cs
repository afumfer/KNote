using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection;
using Dapper;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Repository.Dapper;

public class KntSemanticIndexRepository : KntRepositoryDapperBase, IKntSemanticIndexRepository
{
    // The ids of an IN list per query (SQL Server allows 2100 parameters).
    private const int IdsPerQuery = 500;

    private const string ChunkColumns = @"NoteChunkId, NoteId, SourceType, SourceId, ChunkIndex, Page, Content, ContentHash,
        SourceHash, Stale, Model, Dimensions, IndexedDateTime";

    // A note (n) the index of @Model has to calculate: without rows, with rows marked as Stale or modified after them.
    private const string Pending = @"(NOT EXISTS (SELECT 1 FROM NoteChunks c WHERE c.NoteId = n.NoteId AND c.Model = @Model)
        OR EXISTS (SELECT 1 FROM NoteChunks c WHERE c.NoteId = n.NoteId AND c.Model = @Model
            AND (c.Stale = 1 OR c.IndexedDateTime < n.ModificationDateTime)))";

    // A note (n) not of the note types in @Excluded.
    private static string NotExcluded(List<Guid> excludedNoteTypeIds) =>
        excludedNoteTypeIds?.Count > 0 ? "(n.NoteTypeId IS NULL OR n.NoteTypeId NOT IN @Excluded)" : "1 = 1";

    public KntSemanticIndexRepository(DbConnection singletonConnection, RepositoryRef repositoryRef)
        : base(singletonConnection, repositoryRef)
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
            var db = GetOpenConnection();

            var sql = $@"SELECT {ChunkColumns}, Vector FROM NoteChunks
                WHERE NoteId = @NoteId AND Model = @Model
                ORDER BY SourceType, SourceId, ChunkIndex";
            var rows = await db.QueryAsync<NoteChunkRow>(sql, new { NoteId = noteId, Model = model });
            var result = new Result<List<NoteChunkDto>> { Entity = rows.Select(r => r.ToDto()).ToList() };

            await CloseIsTempConnection(db);

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
            var db = GetOpenConnection();
            try
            {
                if (db.State != ConnectionState.Open)
                    await db.OpenAsync();

                using var transaction = await db.BeginTransactionAsync();

                await db.ExecuteAsync("DELETE FROM NoteChunks WHERE NoteId = @NoteId AND Model = @Model",
                    new { NoteId = noteId, Model = model }, transaction);

                var sql = @"INSERT INTO NoteChunks (NoteChunkId, NoteId, SourceType, SourceId, ChunkIndex, Page, Content,
                        ContentHash, SourceHash, Stale, Model, Dimensions, Vector, IndexedDateTime)
                    VALUES (@NoteChunkId, @NoteId, @SourceType, @SourceId, @ChunkIndex, @Page, @Content,
                        @ContentHash, @SourceHash, @Stale, @Model, @Dimensions, @Vector, @IndexedDateTime)";
                await db.ExecuteAsync(sql, chunks.Select(c => new
                {
                    NoteChunkId = c.NoteChunkId == Guid.Empty ? Guid.NewGuid() : c.NoteChunkId,
                    NoteId = noteId,
                    SourceType = (int)c.SourceType,
                    c.SourceId,
                    c.ChunkIndex,
                    c.Page,
                    c.Content,
                    c.ContentHash,
                    c.SourceHash,
                    Stale = false,
                    Model = model,
                    Dimensions = c.Vector?.Length ?? 0,
                    Vector = EmbeddingVector.ToBytes(c.Vector ?? []),
                    c.IndexedDateTime
                }).ToList(), transaction);

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

    public Task<Result> MarkStaleAsync(Guid noteId) =>
        ExecuteAsync("UPDATE NoteChunks SET Stale = 1 WHERE NoteId = @NoteId", new { NoteId = noteId });

    public Task<Result> MarkModelStaleAsync(string model) =>
        ExecuteAsync("UPDATE NoteChunks SET Stale = 1 WHERE Model = @Model", new { Model = model });

    public async Task<Result> MarkStaleByFoldersAsync(List<Guid> folderIds)
    {
        foreach (var ids in (folderIds ?? new()).Distinct().Chunk(IdsPerQuery))
        {
            var result = await ExecuteAsync(
                "UPDATE NoteChunks SET Stale = 1 WHERE NoteId IN (SELECT NoteId FROM Notes WHERE FolderId IN @Ids)", new { Ids = ids });
            if (!result.IsValid)
                return result;
        }
        return new Result();
    }

    public Task<Result> MarkStaleByNoteTypeAsync(Guid noteTypeId) =>
        ExecuteAsync("UPDATE NoteChunks SET Stale = 1 WHERE NoteId IN (SELECT NoteId FROM Notes WHERE NoteTypeId = @Id)",
            new { Id = noteTypeId });

    public Task<Result> MarkStaleByKAttributeAsync(Guid kattributeId) =>
        ExecuteAsync("UPDATE NoteChunks SET Stale = 1 WHERE NoteId IN (SELECT NoteId FROM NoteKAttributes WHERE KAttributeId = @Id)",
            new { Id = kattributeId });

    public Task<Result> DeleteModelAsync(string model) =>
        ExecuteAsync("DELETE FROM NoteChunks WHERE Model = @Model", new { Model = model });

    public async Task<Result<List<Guid>>> GetPendingNoteIdsAsync(string model, int max, List<Guid> excludedNoteTypeIds = null)
    {
        try
        {
            var db = GetOpenConnection();

            var isSqlServer = _repositoryRef.Provider == "Microsoft.Data.SqlClient";
            var sql = $@"SELECT {(isSqlServer ? "TOP (@Max)" : "")} n.NoteId FROM Notes n
                WHERE {NotExcluded(excludedNoteTypeIds)} AND {Pending}
                ORDER BY n.ModificationDateTime DESC, n.NoteNumber DESC
                {(isSqlServer ? "" : "LIMIT @Max")}";
            var ids = await db.QueryAsync<Guid>(sql, new { Model = model, Max = max, Excluded = excludedNoteTypeIds });
            var result = new Result<List<Guid>> { Entity = ids.ToList() };

            await CloseIsTempConnection(db);

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
            var db = GetOpenConnection();

            var notExcluded = NotExcluded(excludedNoteTypeIds);
            var sql = $@"SELECT
                (SELECT COUNT(*) FROM Notes n WHERE {notExcluded}) AS TotalNotes,
                (SELECT COUNT(*) FROM Notes n WHERE {notExcluded}
                    AND EXISTS (SELECT 1 FROM NoteChunks c WHERE c.NoteId = n.NoteId AND c.Model = @Model)) AS IndexedNotes,
                (SELECT COUNT(*) FROM Notes n WHERE {notExcluded} AND {Pending}) AS PendingNotes,
                (SELECT COUNT(*) FROM NoteChunks c WHERE c.Model = @Model) AS Chunks";
            var summary = await db.QueryFirstAsync<SemanticIndexSummaryDto>(sql, new { Model = model, Excluded = excludedNoteTypeIds });

            await CloseIsTempConnection(db);

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
            var db = GetOpenConnection();

            var sql = @"SELECT Model AS IndexKey, COUNT(*) AS Chunks, COUNT(DISTINCT NoteId) AS Notes
                FROM NoteChunks GROUP BY Model ORDER BY Model";
            var models = await db.QueryAsync<SemanticIndexModelDto>(sql);

            await CloseIsTempConnection(db);

            return new Result<List<SemanticIndexModelDto>> { Entity = models.ToList() };
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
            var db = GetOpenConnection();

            var sql = @"SELECT NoteChunkId, NoteId, SourceType, SourceId, Vector FROM NoteChunks WHERE Model = @Model"
                + (changedSince != null ? " AND IndexedDateTime >= @ChangedSince" : "");
            var rows = await db.QueryAsync<NoteChunkRow>(sql, new { Model = model, ChangedSince = changedSince });
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

            await CloseIsTempConnection(db);

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
            var db = GetOpenConnection();

            var sql = @"SELECT COUNT(*) AS RowsCount, MAX(IndexedDateTime) AS LastIndexed FROM NoteChunks WHERE Model = @Model";
            var row = await db.QueryFirstAsync(sql, new { Model = model });
            var result = new Result<SemanticIndexWatermarkDto>
            {
                Entity = new SemanticIndexWatermarkDto
                {
                    Count = Convert.ToInt32(row.RowsCount),
                    LastIndexedDateTime = ToDateTime((object)row.LastIndexed)
                }
            };

            await CloseIsTempConnection(db);

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
            var db = GetOpenConnection();

            var chunks = new List<NoteChunkDto>();
            var sql = $"SELECT {ChunkColumns} FROM NoteChunks WHERE NoteChunkId IN @Ids";
            foreach (var ids in (noteChunkIds ?? new()).Distinct().Chunk(IdsPerQuery))
            {
                var rows = await db.QueryAsync<NoteChunkRow>(sql, new { Ids = ids });
                chunks.AddRange(rows.Select(r => r.ToDto()));
            }

            await CloseIsTempConnection(db);

            return new Result<List<NoteChunkDto>> { Entity = chunks };
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    private async Task<Result> ExecuteAsync(string sql, object param)
    {
        try
        {
            var db = GetOpenConnection();

            await db.ExecuteAsync(sql, param);

            await CloseIsTempConnection(db);

            return new Result();
        }
        catch (Exception ex)
        {
            throw new KntRepositoryException($"KNote repository error. ({MethodBase.GetCurrentMethod().DeclaringType})", ex);
        }
    }

    // MAX() of a Sqlite date column comes as its text.
    private static DateTime? ToDateTime(object value) => value switch
    {
        null or DBNull => null,
        DateTime dateTime => dateTime,
        string text => DateTime.Parse(text, CultureInfo.InvariantCulture),
        _ => throw new NotSupportedException($"Unsupported DateTime value type: {value.GetType()}")
    };

    // A row as it is read (the vector in bytes).
    private class NoteChunkRow
    {
        public Guid NoteChunkId { get; set; }
        public Guid NoteId { get; set; }
        public int SourceType { get; set; }
        public Guid SourceId { get; set; }
        public int ChunkIndex { get; set; }
        public int? Page { get; set; }
        public string Content { get; set; }
        public string ContentHash { get; set; }
        public string SourceHash { get; set; }
        public bool Stale { get; set; }
        public string Model { get; set; }
        public int Dimensions { get; set; }
        public byte[] Vector { get; set; }
        public DateTime IndexedDateTime { get; set; }

        public NoteChunkDto ToDto() => new()
        {
            NoteChunkId = NoteChunkId,
            NoteId = NoteId,
            SourceType = (NoteChunkSourceType)SourceType,
            SourceId = SourceId,
            ChunkIndex = ChunkIndex,
            Page = Page,
            Content = Content,
            ContentHash = ContentHash,
            SourceHash = SourceHash,
            Stale = Stale,
            Model = Model,
            Dimensions = Dimensions,
            Vector = EmbeddingVector.FromBytes(Vector),
            IndexedDateTime = IndexedDateTime
        };
    }
}
