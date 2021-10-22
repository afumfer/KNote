using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model.Dto;
using KNote.Model;

namespace KNote.Service
{
    public interface IKntKAttributeService
    {        
        Task<Result<List<KAttributeInfoDto>>> GetAllAsync();
        Task<Result<List<KAttributeInfoDto>>> GetAllAsync(Guid? typeId);
        Task<Result<KAttributeDto>> GetAsync(Guid id);        
        Task<Result<KAttributeDto>> SaveAsync(KAttributeDto kattribute);
        Task<Result<KAttributeInfoDto>> DeleteAsync(Guid id);                
        Task<Result<List<KAttributeTabulatedValueDto>>> GetKAttributeTabulatedValuesAsync(Guid id);       
    }
}
