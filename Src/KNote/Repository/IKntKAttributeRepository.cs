using KNote.Model.Dto;
using KNote.Model;

namespace KNote.Repository;

public interface IKntKAttributeRepository : IDisposable
{
    Task<Result<List<KAttributeInfoDto>>> GetAllAsync();
    Task<Result<List<KAttributeInfoDto>>> GetAllAsync(Guid? typeId);        
    Task<Result<List<KAttributeInfoDto>>> GetAllIncludeNullTypeAsync(Guid? typeId);
    Task<Result<KAttributeDto>> GetAsync(Guid id);        
    Task<Result<KAttributeDto>> AddAsync(KAttributeDto entityInfo);
    Task<Result<KAttributeDto>> UpdateAsync(KAttributeDto entityInfo);        
    Task<Result> DeleteAsync(Guid id);
    Task<Result<List<KAttributeTabulatedValueDto>>> GetKAttributeTabulatedValuesAsync(Guid attributeId);
}

