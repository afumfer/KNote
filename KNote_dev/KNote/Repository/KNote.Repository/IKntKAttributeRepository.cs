using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model.Dto;
using KNote.Model;


namespace KNote.Repository
{
    public interface IKntKAttributeRepository : IDisposable
    {
        Task<Result<List<KAttributeDto>>> GetAllAsync();
        Task<Result<List<KAttributeInfoDto>>> GetAllAsync(Guid? typeId);
        Task<Result<KAttributeDto>> GetAsync(Guid id);
        Task<Result<KAttributeDto>> SaveAsync(KAttributeDto entityInfo);
        Task<Result<KAttributeInfoDto>> DeleteAsync(Guid id);
        Task<Result<KAttributeTabulatedValueDto>> SaveTabulateValueAsync(Guid attributeId, KAttributeTabulatedValueDto entityInfo);
        Task<Result<KAttributeTabulatedValueDto>> AddNewKAttributeTabulatedValueAsync(Guid id, KAttributeTabulatedValueDto entityInfo);
        Task<Result<KAttributeTabulatedValueDto>> DeleteKAttributeTabulatedValueAsync(Guid id);
        Task<Result<List<KAttributeTabulatedValueDto>>> GetKAttributeTabulatedValuesAsync(Guid attributeId);
    }
}
