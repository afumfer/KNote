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
        Result<KAttributeInfoDto> Get(string key);
        Result<KAttributeInfoDto> Get(Guid id);
        Result<KAttributeDto> GetFull(Guid id);
        Result<KAttributeDto> New(KAttributeInfoDto entity = null);
        Result<KAttributeInfoDto> Save(KAttributeInfoDto entity);
        Result<KAttributeTabulatedValueInfoDto> SaveTabulateValue(Guid attributeId, KAttributeTabulatedValueInfoDto entityInfo);
        Result<KAttributeDto> Delete(Guid id);
        Result<KAttributeTabulatedValueInfoDto> AddNewKAttributeTabulatedValue(Guid id, KAttributeTabulatedValueInfoDto entityInfo);
        Result<KAttributeTabulatedValueInfoDto> DeleteKAttributeTabulatedValue(Guid id);
    }
}
