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
        Result<KAttributeDto> Get(Guid id);
        Result<KAttributeDto> GetFull(Guid id);
        Task<Result<KAttributeDto>> GetAsync(Guid id);
        Result<KAttributeDto> New(KAttributeInfoDto entity = null);
        Result<KAttributeDto> Save(KAttributeDto entity);
        Task<Result<KAttributeDto>> SaveAsync(KAttributeDto entityInfo);
        Result<KAttributeInfoDto> Delete(Guid id);
        Task<Result<KAttributeInfoDto>> DeleteAsync(Guid id);

        // TODO: Pendiente de refactorizar los tres siguientes métodos 
        Result<KAttributeTabulatedValueInfoDto> SaveTabulateValue(Guid attributeId, KAttributeTabulatedValueInfoDto entityInfo);        
        Result<KAttributeTabulatedValueInfoDto> AddNewKAttributeTabulatedValue(Guid id, KAttributeTabulatedValueInfoDto entityInfo);
        Result<KAttributeTabulatedValueInfoDto> DeleteKAttributeTabulatedValue(Guid id);       
        
    }
}
