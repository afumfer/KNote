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
    public Func<Guid, Task<Result<FolderDto>>>? GetByIdAsyncImpl { get; set; }
    public Func<Task<Result<FolderDto>>>? GetHomeAsyncImpl { get; set; }

    public Task<Result<List<FolderInfoDto>>> GetAllAsync() => throw new NotSupportedException();
    public Task<Result<List<FolderDto>>> GetTreeAsync() => throw new NotSupportedException();
    public Task<Result<FolderDto>> GetHomeAsync() =>
        (GetHomeAsyncImpl ?? throw new NotSupportedException($"{nameof(GetHomeAsync)} not configured for this test"))();
    public Task<Result<List<FolderInfoDto>>> GetHomeChildrenAsync() => throw new NotSupportedException();
    public Task<Result<FolderDto>> GetAsync(Guid folderId) =>
        (GetByIdAsyncImpl ?? throw new NotSupportedException($"{nameof(GetAsync)}(Guid) not configured for this test"))(folderId);
    public Task<Result<FolderDto>> GetAsync(int folderNumber) => throw new NotSupportedException();

    // As KntFolderService: the names from the root down, read with GetAsync (GetByIdAsyncImpl).
    public async Task<Result<string>> GetPathAsync(Guid folderId)
    {
        var names = new List<string>();
        Guid? id = folderId;
        while (id is Guid current)
        {
            var result = await GetAsync(current);
            if (!result.IsValid || result.Entity == null)
            {
                var error = new Result<string>();
                error.AddErrorMessage(result.ErrorMessage);
                return error;
            }
            names.Insert(0, result.Entity.Name);
            id = result.Entity.ParentId;
        }
        return new Result<string>(string.Join("\\", names));
    }
    public Task<Result<FolderDto>> SaveAsync(FolderDto entityInfo) => throw new NotSupportedException();
    public Task<Result> UpdateOrderNotesAsync(Guid folderId, string orderNotes) =>
        (UpdateOrderNotesAsyncImpl ?? throw new NotSupportedException($"{nameof(UpdateOrderNotesAsync)} not configured for this test"))(folderId, orderNotes);
    public Task<Result<FolderDto>> DeleteAsync(Guid id) => throw new NotSupportedException();
}
