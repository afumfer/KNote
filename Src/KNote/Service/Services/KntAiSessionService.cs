using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Service.Interfaces;
using KNote.Service.ServicesCommands;

namespace KNote.Service.Services;

public class KntAiSessionService : KntServiceBase, IKntAiSessionService
{
    #region Constructor

    public KntAiSessionService(IKntService service) : base(service)
    {

    }

    #endregion

    #region IKntAiSessionService

    public async Task<Result<List<AiChatSessionInfoDto>>> GetUserSessionsAsync()
    {
        var command = new KntAiSessionsGetUserSessionsAsyncCommand(Service);
        return await ExecuteCommand(command);
    }

    public async Task<Result<AiChatSessionDto>> GetAsync(Guid noteId)
    {
        var command = new KntAiSessionsGetAsyncCommand(Service, noteId);
        return await ExecuteCommand(command);
    }

    public async Task<Result<AiChatSessionDto>> SaveAsync(AiChatSessionDto session)
    {
        var command = new KntAiSessionsSaveAsyncCommand(Service, session);
        return await ExecuteCommand(command);
    }

    #endregion
}
