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
    public interface IKntKMessageService
    {
        Result<List<KMessageInfoDto>> GetAll();
        Result<List<KMessageInfoDto>> GetAllForUser(Guid id);
        Result<List<KMessageInfoDto>> GetAllForNote(Guid id);
        Result<KMessageInfoDto> Get(Guid id);
        Result<KMessageDto> New(KMessageInfoDto entity = null);
        Result<KMessageInfoDto> Save(KMessageInfoDto entityInfo);
        Task<Result<KMessageInfoDto>> SaveAsync(KMessageInfoDto entityInfo);
        Result<KMessageInfoDto> Delete(Guid id);
        Task<Result<KMessageInfoDto>> DeleteAsync(Guid id);
    }
}
