using KNote.Model.Dto;
using KNote.Model;

namespace KNote.Repository;

public interface IKntKAttributeRepository : IDisposable
{
    Task<Result<List<KAttributeInfoDto>>> GetAllAsync();
    Task<Result<List<KAttributeInfoDto>>> GetAllAsync(Guid? typeId);        
    Task<Result<List<KAttributeInfoDto>>> GetAllIncludeNullTypeAsync(Guid? typeId);
    Task<Result<KAttributeDto>> GetAsync(Guid id);        
    // The attribute and its tabulated values in one transaction, with the row of the history (KntHistory, H10) if given: if it
    // can't be noted, nothing changes.
    Task<Result<KAttributeDto>> AddAsync(KAttributeDto entityInfo, KLogWriteDto history = null);
    Task<Result<KAttributeDto>> UpdateAsync(KAttributeDto entityInfo, KLogWriteDto history = null);
    Task<Result> DeleteAsync(Guid id, KLogWriteDto history = null);
    Task<Result<List<KAttributeTabulatedValueDto>>> GetKAttributeTabulatedValuesAsync(Guid attributeId);
    Task<Result<int>> CountNoteUsagesAsync(Guid kattributeId);
}

