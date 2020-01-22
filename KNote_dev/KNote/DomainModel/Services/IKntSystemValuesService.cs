using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KNote.Shared;
// TODO: Pendiente de eliminar
//using KNote.DomainModel.Dto;
using KNote.Shared.Dto;
using KNote.DomainModel.Entities;


namespace KNote.DomainModel.Services
{
    public interface IKntSystemValuesService
    {
        Result<List<SystemValueInfoDto>> GetAll();
        Result<SystemValueInfoDto> Get(string key);
        Result<SystemValueInfoDto> Get(Guid id);
        Result<SystemValueInfoDto> Save(SystemValueInfoDto entityInfo);
        Result<SystemValueInfoDto> Delete(Guid id);
    }
}
