using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using KNote.Shared;
// TODO: Pendiente de eliminar
//using KNote.DomainModel.Dto;
using KNote.Shared.Dto;
using KNote.DomainModel.Entities;



namespace KNote.DomainModel.Services
{
    public interface IKntKAttributeService
    {
        Result<List<KAttributeInfoDto>> GetAll();        
        Task<Result<KAttributeDto>> GetAsync(Guid id);
        Result<KAttributeDto> New(KAttributeInfoDto entity = null);
        Task<Result<KAttributeDto>> SaveAsync(KAttributeDto entityInfo);
        Task<Result<KAttributeInfoDto>> DeleteAsync(Guid id);        
        Task<Result<KAttributeTabulatedValueDto>> SaveTabulateValueAsync(Guid attributeId, KAttributeTabulatedValueDto entityInfo);        
        Result<KAttributeTabulatedValueInfoDto> AddNewKAttributeTabulatedValue(Guid id, KAttributeTabulatedValueInfoDto entityInfo);
        Task<Result<KAttributeTabulatedValueInfoDto>> DeleteKAttributeTabulatedValueAsync(Guid id);
        Result<List<KAttributeTabulatedValueDto>> GetKAttributeTabulatedValues(Guid attributeId);
    }

}
