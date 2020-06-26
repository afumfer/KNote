using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KNote.Shared.Dto.Info;
using KNote.Shared.Dto;
using KNote.Shared;
using KNote.Shared.Entities;

namespace KNote.Shared.Services
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
