using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Server.Helpers;
using KNote.Service.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace KNote.Server.Hubs;

/// <summary>
/// The chat room (see ChatRoomHubContract for what its clients call and receive). A thin layer over
/// IKntService.ChatRooms, which keeps the threads as notes, plus what only the hub knows: who is subscribed to each
/// thread (one SignalR group per thread, and ChatRoomManager for the online users) and the order of the writes of
/// each thread (ChatRoomManager's locks). Staff, like the chat room commands of the Service.
/// </summary>
[Authorize(Roles = "Staff, ProjectManager, Admin")]
public class ChatHub : Hub
{
    private readonly IKntService _service;
    private readonly ChatRoomManager _manager;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(IKntService service, ChatRoomManager manager, ILogger<ChatHub> logger)
    {
        _service = service;
        _service.Logger = logger;
        _manager = manager;
        _logger = logger;
    }

    // The user of the connection, for the Service. Context isn't available yet in the constructor.
    private IKntService Service
    {
        get
        {
            _service.UserIdentityName = Context.User?.Identity?.Name;
            return _service;
        }
    }

    private string UserName => Context.User?.Identity?.Name;

    public static string ThreadGroup(Guid threadId) => $"chatroom:{threadId}";

    #region Client -> Server

    public async Task<Result<List<ChatRoomThreadInfoDto>>> GetThreads()
    {
        return await RunAsync(nameof(GetThreads), () => Service.ChatRooms.GetThreadsAsync());
    }

    public async Task<Result<ChatRoomThreadInfoDto>> CreateThread(string topic)
    {
        return await RunAsync(nameof(CreateThread), async () =>
        {
            var res = await Service.ChatRooms.CreateAsync(topic);
            if (res.IsValid)
                await AnnounceThreadCreatedAsync(res.Entity);
            return res;
        });
    }

    public async Task<Result<ChatRoomThreadInfoDto>> GetGeneralThread()
    {
        return await RunAsync(nameof(GetGeneralThread), async () =>
        {
            // Under the creation lock, so two users asking for it at the same time don't create it twice.
            Result<List<ChatRoomThreadInfoDto>> resBefore;
            Result<ChatRoomThreadInfoDto> res;
            using (await _manager.LockCreationAsync())
            {
                resBefore = await Service.ChatRooms.GetThreadsAsync();
                res = await Service.ChatRooms.GetGeneralAsync();
            }

            var created = res.IsValid && resBefore.IsValid && !resBefore.Entity.Any(t => t.ThreadId == res.Entity.ThreadId);
            if (created)
                await AnnounceThreadCreatedAsync(res.Entity);
            return res;
        });
    }

    public async Task<Result<ChatRoomThreadDto>> Subscribe(Guid threadId)
    {
        return await RunAsync(nameof(Subscribe), async () =>
        {
            var res = await Service.ChatRooms.GetAsync(threadId);
            if (!res.IsValid)
                return res;

            await Groups.AddToGroupAsync(Context.ConnectionId, ThreadGroup(threadId));
            var online = _manager.Subscribe(Context.ConnectionId, UserName, threadId);
            res.Entity.OnlineUserNames = online;
            await AnnounceOnlineUsersAsync(threadId, online);
            return res;
        });
    }

    public async Task<Result> Unsubscribe(Guid threadId)
    {
        return await RunAsync(nameof(Unsubscribe), async () =>
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, ThreadGroup(threadId));
            var online = _manager.Unsubscribe(Context.ConnectionId, threadId);
            if (online != null)
                await AnnounceOnlineUsersAsync(threadId, online);
            return new Result();
        });
    }

    public async Task<Result<ChatRoomMessageDto>> SendMessage(Guid threadId, string text)
    {
        return await RunAsync(nameof(SendMessage), async () =>
        {
            // Saved and sent inside the thread's lock, so every subscriber receives the messages in the order they
            // are kept in the thread.
            using (await _manager.LockThreadAsync(threadId))
            {
                var res = await Service.ChatRooms.AddMessageAsync(new ChatRoomMessageDto { ThreadId = threadId, Text = text });
                if (!res.IsValid)
                    return res;

                await Clients.Group(ThreadGroup(threadId)).SendAsync(ChatRoomHubContract.MessageReceived, res.Entity);
                await Clients.All.SendAsync(ChatRoomHubContract.ThreadActivity, new ChatRoomThreadActivityDto
                {
                    ThreadId = threadId,
                    ModificationDateTime = res.Entity.SentDateTime,
                    UserName = res.Entity.UserName
                });
                return res;
            }
        });
    }

    #endregion

    #region Connection

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        foreach (var (threadId, online) in _manager.RemoveConnection(Context.ConnectionId))
            await AnnounceOnlineUsersAsync(threadId, online);

        await base.OnDisconnectedAsync(exception);
    }

    #endregion

    #region Private methods

    private Task AnnounceThreadCreatedAsync(ChatRoomThreadInfoDto thread) =>
        Clients.All.SendAsync(ChatRoomHubContract.ThreadCreated, new ChatRoomThreadInfoDto
        {
            ThreadId = thread.ThreadId,
            NoteNumber = thread.NoteNumber,
            Topic = thread.Topic,
            CreationDateTime = thread.CreationDateTime,
            ModificationDateTime = thread.ModificationDateTime
        });

    private Task AnnounceOnlineUsersAsync(Guid threadId, List<string> online) =>
        Clients.Group(ThreadGroup(threadId)).SendAsync(ChatRoomHubContract.OnlineUsersChanged,
            new ChatRoomOnlineUsersDto { ThreadId = threadId, UserNames = online });

    // As the controllers: a failure is an invalid Result with the reason, never an exception to the client.
    private async Task<TResult> RunAsync<TResult>(string method, Func<Task<TResult>> action) where TResult : Result, new()
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ChatHub {method} ({user}).", method, UserName);
            var res = new TResult();
            res.AddErrorMessage(ex.ToApiErrorMessage());
            return res;
        }
    }

    #endregion
}
