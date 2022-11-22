using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Client.AppStoreService.ClientDataServices;

public interface INoteTypeWebApiService
{
    Task<Result<List<NoteTypeDto>>> GetAllAsync();
    Task<Result<NoteTypeDto>> GetAsync(Guid id);
    Task<Result<NoteTypeDto>> SaveAsync(NoteTypeDto noteType);
    Task<Result<NoteTypeDto>> DeleteAsync(Guid id);
}

