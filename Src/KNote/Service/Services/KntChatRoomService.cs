using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Service.Interfaces;
using KNote.Service.ServicesCommands;

namespace KNote.Service.Services;

public class KntChatRoomService : KntServiceBase, IKntChatRoomService
{
    #region Constructor

    public KntChatRoomService(IKntService service) : base(service)
    {

    }

    #endregion

    #region IKntChatRoomService

    public async Task<Result<List<ChatRoomThreadInfoDto>>> GetThreadsAsync()
    {
        var command = new KntChatRoomsGetThreadsAsyncCommand(Service);
        return await ExecuteCommand(command);
    }

    public async Task<Result<ChatRoomThreadDto>> GetAsync(Guid threadId)
    {
        var command = new KntChatRoomsGetAsyncCommand(Service, threadId);
        return await ExecuteCommand(command);
    }

    public async Task<Result<ChatRoomThreadInfoDto>> CreateAsync(string topic)
    {
        var command = new KntChatRoomsCreateAsyncCommand(Service, topic);
        return await ExecuteCommand(command);
    }

    public async Task<Result<ChatRoomThreadInfoDto>> GetGeneralAsync()
    {
        var command = new KntChatRoomsGetGeneralAsyncCommand(Service);
        return await ExecuteCommand(command);
    }

    public async Task<Result<ChatRoomThreadInfoDto>> JoinAsync(Guid threadId)
    {
        var command = new KntChatRoomsJoinAsyncCommand(Service, threadId);
        return await ExecuteCommand(command);
    }

    public async Task<Result<ChatRoomMessageDto>> AddMessageAsync(ChatRoomMessageDto message)
    {
        var command = new KntChatRoomsAddMessageAsyncCommand(Service, message);
        return await ExecuteCommand(command);
    }

    #endregion
}
