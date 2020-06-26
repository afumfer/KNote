using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.Model.Dto.Info;
using KNote.Model.Dto;
using KNote.Model;
using KNote.Model.Entities;

namespace KNote.Model.Services
{
    public interface IKntKEventService
    {
        Result<List<KEventInfoDto>> GetAll();
        Result<KEventInfoDto> Get(Guid eventId);
        Result<KEventDto> New(KEventInfoDto entityInfo = null);
        Result<KEventInfoDto> Save(KEventInfoDto entityInfo);
        Result<KEventInfoDto> Delete(Guid id);
    }
}
