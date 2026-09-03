using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Interfaces;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>
/// Minimal IKntTraceNoteTypeService test double: only the members exercised by the tests have a
/// working implementation (via settable delegates); everything else throws, so an unexpectedly-
/// touched member fails loudly instead of silently returning a default value.
/// </summary>
internal class FakeKntTraceNoteTypeService : IKntTraceNoteTypeService
{
    public Func<Task<Result<List<TraceNoteTypeDto>>>>? GetAllAsyncImpl { get; set; }
    public Func<Guid, Task<Result<TraceNoteTypeDto>>>? GetAsyncImpl { get; set; }
    public Func<TraceNoteTypeDto, Task<Result<TraceNoteTypeDto>>>? SaveAsyncImpl { get; set; }
    public Func<Guid, Task<Result<TraceNoteTypeDto>>>? DeleteAsyncImpl { get; set; }

    public Task<Result<List<TraceNoteTypeDto>>> GetAllAsync() =>
        (GetAllAsyncImpl ?? throw new NotSupportedException($"{nameof(GetAllAsync)} not configured for this test"))();

    public Task<Result<TraceNoteTypeDto>> GetAsync(Guid id) =>
        (GetAsyncImpl ?? throw new NotSupportedException($"{nameof(GetAsync)} not configured for this test"))(id);

    public Task<Result<TraceNoteTypeDto>> SaveAsync(TraceNoteTypeDto entity) =>
        (SaveAsyncImpl ?? throw new NotSupportedException($"{nameof(SaveAsync)} not configured for this test"))(entity);

    public Task<Result<TraceNoteTypeDto>> DeleteAsync(Guid id) =>
        (DeleteAsyncImpl ?? throw new NotSupportedException($"{nameof(DeleteAsync)} not configured for this test"))(id);
}
