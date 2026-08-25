using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Interfaces;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>
/// Minimal IKntNoteTypeService test double: only the members exercised by the tests have a working
/// implementation (via settable delegates); everything else throws, so an unexpectedly-touched
/// member fails loudly instead of silently returning a default value.
/// </summary>
internal class FakeKntNoteTypeService : IKntNoteTypeService
{
    public Func<Task<Result<List<NoteTypeDto>>>>? GetAllAsyncImpl { get; set; }
    public Func<Guid, Task<Result<NoteTypeDto>>>? GetAsyncImpl { get; set; }
    public Func<NoteTypeDto, Task<Result<NoteTypeDto>>>? SaveAsyncImpl { get; set; }
    public Func<Guid, Task<Result<NoteTypeDto>>>? DeleteAsyncImpl { get; set; }

    public Task<Result<List<NoteTypeDto>>> GetAllAsync() =>
        (GetAllAsyncImpl ?? throw new NotSupportedException($"{nameof(GetAllAsync)} not configured for this test"))();

    public Task<Result<NoteTypeDto>> GetAsync(Guid id) =>
        (GetAsyncImpl ?? throw new NotSupportedException($"{nameof(GetAsync)} not configured for this test"))(id);

    public Task<Result<NoteTypeDto>> SaveAsync(NoteTypeDto entity) =>
        (SaveAsyncImpl ?? throw new NotSupportedException($"{nameof(SaveAsync)} not configured for this test"))(entity);

    public Task<Result<NoteTypeDto>> DeleteAsync(Guid id) =>
        (DeleteAsyncImpl ?? throw new NotSupportedException($"{nameof(DeleteAsync)} not configured for this test"))(id);
}
