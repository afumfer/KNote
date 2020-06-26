using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KNote.Model;
using KNote.Model.Dto;
using KNote.Model.Dto.Info;
using KNote.Model.Entities;


namespace KNote.Model.Services
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
