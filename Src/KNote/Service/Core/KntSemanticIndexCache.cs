using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KNote.Model.Dto;
using KNote.Repository;

namespace KNote.Service.Core;

/// <summary>
/// The vectors of the semantic index in memory, for the search by meaning (KNote Atlas): one cache per database and
/// index key in the process, shared by every service of that database (the Server's requests, ClientWin's windows).
/// <list type="bullet">
/// <item>The first search loads every vector of the index. The next ones compare the watermark of the repository (rows
/// and last IndexedDateTime) and read only the rows written since the last one, with a margin: IndexedDateTime is when
/// the note was read, before its vectors were calculated and written, so a note read before another one may be written
/// after it. The rows read replace the ones of their notes (a note is always written whole).</item>
/// <item>If the rows then are not as many as the repository's (notes or an index removed), everything is read again.</item>
/// </list>
/// A search uses a snapshot that does not change: a later refresh builds a new one.
/// </summary>
internal sealed class KntSemanticIndexCache
{
    public static readonly TimeSpan ChangesMargin = TimeSpan.FromMinutes(5);

    private static readonly ConcurrentDictionary<string, KntSemanticIndexCache> Caches = new();

    private readonly SemaphoreSlim _lock = new(1, 1);
    private Snapshot _snapshot;

    /// <summary>The vectors of an index, as they are now in the repository.</summary>
    public sealed class Snapshot
    {
        public Snapshot(Dictionary<Guid, NoteChunkVectorDto[]> notes, SemanticIndexWatermarkDto watermark)
        {
            Notes = notes;
            Watermark = watermark;
            Chunks = notes.Values.SelectMany(c => c).ToArray();
        }

        public IReadOnlyDictionary<Guid, NoteChunkVectorDto[]> Notes { get; }
        public IReadOnlyList<NoteChunkVectorDto> Chunks { get; }
        public SemanticIndexWatermarkDto Watermark { get; }
    }

    public static KntSemanticIndexCache For(string connectionString, string indexKey) =>
        Caches.GetOrAdd($"{connectionString}|{indexKey}", _ => new KntSemanticIndexCache());

    public async Task<Snapshot> GetAsync(IKntSemanticIndexRepository repository, string indexKey, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var watermark = Check(await repository.GetIndexWatermarkAsync(indexKey));
            var current = _snapshot;

            // The same watermark: nothing written since. (Two processes indexing at once could write a note read before
            // the last one without changing it: its previous vectors serve until its next change.)
            if (current != null && SameWatermark(current.Watermark, watermark))
                return current;

            if (current != null && watermark.Count > 0 && current.Watermark.LastIndexedDateTime is DateTime last
                && watermark.LastIndexedDateTime >= last)
            {
                var changed = Check(await repository.GetVectorsAsync(indexKey, last - ChangesMargin));
                var notes = new Dictionary<Guid, NoteChunkVectorDto[]>(current.Notes);
                foreach (var note in changed.GroupBy(c => c.NoteId))
                    notes[note.Key] = note.ToArray();

                if (notes.Values.Sum(c => c.Length) == watermark.Count)
                    return _snapshot = new Snapshot(notes, watermark);
            }

            var all = Check(await repository.GetVectorsAsync(indexKey));
            return _snapshot = new Snapshot(all.GroupBy(c => c.NoteId).ToDictionary(g => g.Key, g => g.ToArray()), watermark);
        }
        finally
        {
            _lock.Release();
        }
    }

    private static bool SameWatermark(SemanticIndexWatermarkDto a, SemanticIndexWatermarkDto b) =>
        a.Count == b.Count && a.LastIndexedDateTime == b.LastIndexedDateTime;

    private static T Check<T>(KNote.Model.Result<T> result)
    {
        if (!result.IsValid)
            throw new KntServiceException(result.ErrorMessage);
        return result.Entity;
    }
}
