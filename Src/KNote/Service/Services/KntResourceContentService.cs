using System;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Service.Interfaces;
using KNote.Service.ServicesCommands;

namespace KNote.Service.Services;

public class KntResourceContentService : KntServiceBase, IKntResourceContentService
{
    public KntResourceContentService(IKntService service) : base(service)
    {

    }

    public async Task<Result<PhotoDescriptionContextDto>> GetPhotoContextAsync(Guid resourceId) =>
        await ExecuteCommand(new KntResourceContentGetPhotoContextAsyncCommand(Service, resourceId));

    public async Task<Result<ResourceInfoDto>> SaveAiDescriptionAsync(AiDescriptionSaveDto description) =>
        await ExecuteCommand(new KntResourceContentSaveAiDescriptionAsyncCommand(Service, description));
}
