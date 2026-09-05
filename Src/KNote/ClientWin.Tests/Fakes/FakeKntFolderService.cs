using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Interfaces;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>
/// Minimal IKntFolderService test double: only the members exercised by the tests have a working
/// implementation (via settable delegates); everything else throws, so an unexpectedly-touched
/// member fails loudly instead of silently returning a default value.
/// </summary>
internal class FakeKntFolderService : IKntFolderService
{
    public Func<Guid, string, Task<Result>>? UpdateOrderNotesAsyncImpl { get; set; }

    public Task<Result<List<FolderInfoDto>>> GetAllAsync() => throw new NotSupportedException();
    public Task<Result<List<FolderDto>>> GetTreeAsync() => throw new NotSupportedException();
    public Task<Result<FolderDto>> GetHomeAsync() => throw new NotSupportedException();
    public Task<Result<FolderDto>> GetAsync(Guid folderId) => throw new NotSupportedException();
    public Task<Result<FolderDto>> GetAsync(int folderNumber) => throw new NotSupportedException();
    public Task<Result<FolderDto>> SaveAsync(FolderDto entityInfo) => throw new NotSupportedException();
    public Task<Result> UpdateOrderNotesAsync(Guid folderId, string orderNotes) =>
        (UpdateOrderNotesAsyncImpl ?? throw new NotSupportedException($"{nameof(UpdateOrderNotesAsync)} not configured for this test"))(folderId, orderNotes);
    public Task<Result<FolderDto>> DeleteAsync(Guid id) => throw new NotSupportedException();
}
