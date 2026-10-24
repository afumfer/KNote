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
/// each thread (ChatRoomManager's locks). Each repository of the Server has its own chat room: a connection is in the
/// repository of its token's session (one group per repository for what goes to all its clients, and the threads'
/// groups and ChatRoomManager's keys include it). Staff, like the chat room commands of the Service. The images of the messages
/// (resources of the thread's note, see ChatRoomController) are kept in the note with the path note descriptions use,
/// relative to the resources, and reach the clients with the absolute url they are served from, as NotesController does
/// with descriptions.
/// </summary>
[Authorize(Roles = "Staff, ProjectManager, Admin")]
public class ChatHub : Hub
{
    private readonly KntRepositories _repositories;
    private readonly ChatRoomManager _manager;
    private readonly ILogger<ChatHub> _logger;
    private IKntService _service;

    public ChatHub(KntRepositories repositories, ChatRoomManager manager, ILogger<ChatHub> logger)
    {
        _repositories = repositories;
        _manager = manager;
        _logger = logger;
    }

    // The repository of the connection's session (its token's) and its user, for the Service, with the resources
    // served from this host unless the configuration says otherwise (KntRepositories; the url, of the connection's
    // request). Created from Context, which isn't available yet in the constructor.
    private IKntService Service
    {
        get
        {
            if (_service == null)
            {
                _service = _repositories.CreateService(_repositories.ForUser(Context.User), UserName, Context.GetHttpContext()?.Request);
                _service.Logger = _logger;
            }
            return _service;
        }
    }

    private string UserName => Context.User?.Identity?.Name;

    private Guid RepositoryId => _repositories.ForUser(Context.User).RepositoryId;

    private ChatThreadKey Thread(Guid threadId) => new(RepositoryId, threadId);

    // Each repository has its own chat room: its clients, and one group per thread.
    public static string RepositoryGroup(Guid repositoryId) => $"repository:{repositoryId}";

    public static string ThreadGroup(ChatThreadKey thread) => $"chatroom:{thread.RepositoryId}:{thread.ThreadId}";

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
            using (await _manager.LockCreationAsync(RepositoryId))
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
            foreach (var message in res.Entity.Messages)
                message.Text = ForRead(message.Text);

            await Groups.AddToGroupAsync(Context.ConnectionId, ThreadGroup(Thread(threadId)));
            var online = _manager.Subscribe(Context.ConnectionId, UserName, Thread(threadId));
            res.Entity.OnlineUserNames = online;
            await AnnounceOnlineUsersAsync(threadId, online);
            return res;
        });
    }

    public async Task<Result> Unsubscribe(Guid threadId)
    {
        return await RunAsync(nameof(Unsubscribe), async () =>
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, ThreadGroup(Thread(threadId)));
            var online = _manager.Unsubscribe(Context.ConnectionId, Thread(threadId));
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
            using (await _manager.LockThreadAsync(Thread(threadId)))
            {
                var res = await Service.ChatRooms.AddMessageAsync(new ChatRoomMessageDto { ThreadId = threadId, Text = ForWrite(text) });
                if (!res.IsValid)
                    return res;
                res.Entity.Text = ForRead(res.Entity.Text);

                await Clients.Group(ThreadGroup(Thread(threadId))).SendAsync(ChatRoomHubContract.MessageReceived, res.Entity);
                await Clients.Group(RepositoryGroup(RepositoryId)).SendAsync(ChatRoomHubContract.ThreadActivity, new ChatRoomThreadActivityDto
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

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, RepositoryGroup(RepositoryId));
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        foreach (var (thread, online) in _manager.RemoveConnection(Context.ConnectionId))
            await AnnounceOnlineUsersAsync(thread.ThreadId, online);

        await base.OnDisconnectedAsync(exception);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _service?.Dispose();
        base.Dispose(disposing);
    }

    #endregion

    #region Private methods

    // The urls of the images: absolute for the clients, relative to the resources in the note.
    private string ForRead(string text) => Service.Notes.UtilUpdateResourceInDescriptionForRead(text);

    private string ForWrite(string text) => Service.Notes.UtilUpdateResourceInDescriptionForWrite(text);

    private Task AnnounceThreadCreatedAsync(ChatRoomThreadInfoDto thread) =>
        Clients.Group(RepositoryGroup(RepositoryId)).SendAsync(ChatRoomHubContract.ThreadCreated, new ChatRoomThreadInfoDto
        {
            ThreadId = thread.ThreadId,
            NoteNumber = thread.NoteNumber,
            Topic = thread.Topic,
            CreationDateTime = thread.CreationDateTime,
            ModificationDateTime = thread.ModificationDateTime
        });

    private Task AnnounceOnlineUsersAsync(Guid threadId, List<string> online) =>
        Clients.Group(ThreadGroup(Thread(threadId))).SendAsync(ChatRoomHubContract.OnlineUsersChanged,
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
