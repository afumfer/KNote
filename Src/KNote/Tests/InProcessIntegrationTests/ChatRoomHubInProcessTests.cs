using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Tests.Helpers;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;

namespace KNote.Tests.InProcessIntegrationTests;

/// <summary>
/// The chat room hub (ChatHub, ChatRoomHubContract): only for the Server's Staff users (token of the connection),
/// threads kept apart (a message only reaches the subscribers of its thread), messages saved in the thread's note in
/// the order they are sent, the online users of each thread, and the general thread created once.
/// </summary>
[TestClass]
public class ChatRoomHubInProcessTests
{
    private const string Password = "InProcess-Test-Password-1!";
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    private static KNoteWebApplicationFactory _factory = null!;
    private static HttpClient _adminClient = null!;
    private static string _staffAToken = null!;
    private static string _staffBToken = null!;
    private static string _guestToken = null!;
    private static string _staffAName = null!;
    private static string _staffBName = null!;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        (_factory, _adminClient) = await InProcessTestHost.CreateAuthenticatedClientAsync();
        (_staffAName, _staffAToken) = await NewUserAsync("itest-chat-a-", nameof(EnumRoles.Staff));
        (_staffBName, _staffBToken) = await NewUserAsync("itest-chat-b-", nameof(EnumRoles.Staff));
        (_, _guestToken) = await NewUserAsync("itest-chat-g-", nameof(EnumRoles.Guest));
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        _adminClient.Dispose();
        await _factory.DisposeAsync();
    }

    [TestMethod]
    public async Task Connecting_WithoutAToken_OrAsAGuest_IsRefused()
    {
        await using var anonymous = NewConnection(null);
        await using var guest = NewConnection(_guestToken);

        var noToken = await Assert.ThrowsAsync<HttpRequestException>(() => anonymous.StartAsync());
        var asGuest = await Assert.ThrowsAsync<HttpRequestException>(() => guest.StartAsync());

        Assert.AreEqual(HttpStatusCode.Unauthorized, noToken.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, asGuest.StatusCode);
    }

    [TestMethod]
    public async Task TheToken_IsAlsoAcceptedInTheQueryString()
    {
        await using var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(_factory.Server.BaseAddress, $"chathub?access_token={_staffAToken}"), options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.Transports = HttpTransportType.LongPolling;
            })
            .Build();

        await connection.StartAsync();

        var threads = await connection.InvokeAsync<Result<List<ChatRoomThreadInfoDto>>>(ChatRoomHubContract.GetThreads);
        Assert.IsTrue(threads.IsValid, threads.ErrorMessage);
    }

    [TestMethod]
    public async Task AMessage_OnlyReachesTheSubscribersOfItsThread_AndEverybodyHearsOfTheActivity()
    {
        await using var a = await StartAsync(_staffAToken);
        await using var b = await StartAsync(_staffBToken);
        var threadOfA = await CreateThreadAsync(a, "Thread of A");
        var threadOfB = await CreateThreadAsync(b, "Thread of B");
        await SubscribeAsync(a, threadOfA.ThreadId);
        await SubscribeAsync(b, threadOfB.ThreadId);

        var receivedByA = Collect<ChatRoomMessageDto>(a, ChatRoomHubContract.MessageReceived);
        var receivedByB = Collect<ChatRoomMessageDto>(b, ChatRoomHubContract.MessageReceived);
        var activitySeenByB = Collect<ChatRoomThreadActivityDto>(b, ChatRoomHubContract.ThreadActivity);

        var sent = await a.InvokeAsync<Result<ChatRoomMessageDto>>(ChatRoomHubContract.SendMessage, threadOfA.ThreadId, "Only for A's thread");
        var sentToB = await b.InvokeAsync<Result<ChatRoomMessageDto>>(ChatRoomHubContract.SendMessage, threadOfB.ThreadId, "Only for B's thread");

        Assert.IsTrue(sent.IsValid, sent.ErrorMessage);
        Assert.IsTrue(sentToB.IsValid, sentToB.ErrorMessage);
        await WaitUntilAsync(() => receivedByA.Count == 1 && receivedByB.Count == 1 && activitySeenByB.Count >= 1);
        Assert.AreEqual(sent.Entity.MessageId, receivedByA.Single().MessageId);
        Assert.AreEqual(sentToB.Entity.MessageId, receivedByB.Single().MessageId);
        Assert.IsTrue(activitySeenByB.Any(e => e.ThreadId == threadOfA.ThreadId));
    }

    [TestMethod]
    public async Task Messages_AreSavedInTheThread_InTheOrderTheSubscribersReceiveThem_EvenWhenSentAtTheSameTime()
    {
        await using var a = await StartAsync(_staffAToken);
        await using var b = await StartAsync(_staffBToken);
        var thread = await CreateThreadAsync(a, "Busy thread");
        await SubscribeAsync(a, thread.ThreadId);
        var received = Collect<ChatRoomMessageDto>(a, ChatRoomHubContract.MessageReceived);

        var sends = Enumerable.Range(1, 10).SelectMany(i => new[]
        {
            a.InvokeAsync<Result<ChatRoomMessageDto>>(ChatRoomHubContract.SendMessage, thread.ThreadId, $"A {i}"),
            b.InvokeAsync<Result<ChatRoomMessageDto>>(ChatRoomHubContract.SendMessage, thread.ThreadId, $"B {i}")
        });
        var results = await Task.WhenAll(sends);

        Assert.IsTrue(results.All(r => r.IsValid), string.Join(" ", results.Select(r => r.ErrorMessage)));
        await WaitUntilAsync(() => received.Count == 20);

        await using var reader = await StartAsync(_staffBToken);
        var saved = await SubscribeAsync(reader, thread.ThreadId);
        Assert.AreEqual(20, saved.Messages.Count, "None lost.");
        CollectionAssert.AreEqual(received.Select(m => m.MessageId).ToList(), saved.Messages.Select(m => m.MessageId).ToList());
        CollectionAssert.AreEquivalent(new[] { _staffAName, _staffBName }, saved.Participants);
    }

    [TestMethod]
    public async Task TheOnlineUsersOfAThread_FollowItsSubscriptions_AndClosedConnections()
    {
        await using var a = await StartAsync(_staffAToken);
        var b = await StartAsync(_staffBToken);
        var thread = await CreateThreadAsync(a, "Who is here");
        var subscribedA = await SubscribeAsync(a, thread.ThreadId);
        var seenByA = Collect<ChatRoomOnlineUsersDto>(a, ChatRoomHubContract.OnlineUsersChanged);

        var subscribedB = await SubscribeAsync(b, thread.ThreadId);
        await WaitUntilAsync(() => seenByA.Count >= 1);
        await b.DisposeAsync();
        await WaitUntilAsync(() => seenByA.Count >= 2);

        Assert.AreEqual(1, subscribedA.OnlineUserNames.Count);
        Assert.AreEqual(2, subscribedB.OnlineUserNames.Count);
        CollectionAssert.AreEquivalent(subscribedB.OnlineUserNames, seenByA.First().UserNames);
        CollectionAssert.AreEqual(subscribedA.OnlineUserNames, seenByA.Last().UserNames);
    }

    [TestMethod]
    public async Task AfterUnsubscribing_NoMoreMessagesArrive()
    {
        await using var a = await StartAsync(_staffAToken);
        var thread = await CreateThreadAsync(a, "Left");
        await SubscribeAsync(a, thread.ThreadId);
        var received = Collect<ChatRoomMessageDto>(a, ChatRoomHubContract.MessageReceived);

        var unsubscribed = await a.InvokeAsync<Result>(ChatRoomHubContract.Unsubscribe, thread.ThreadId);
        var sent = await a.InvokeAsync<Result<ChatRoomMessageDto>>(ChatRoomHubContract.SendMessage, thread.ThreadId, "Nobody listening");

        Assert.IsTrue(unsubscribed.IsValid, unsubscribed.ErrorMessage);
        Assert.IsTrue(sent.IsValid, sent.ErrorMessage);
        await Task.Delay(300);
        Assert.AreEqual(0, received.Count);
    }

    [TestMethod]
    public async Task CreatingAThread_IsAnnouncedToEverybody_AndAWrongOneIsRefused()
    {
        await using var a = await StartAsync(_staffAToken);
        await using var b = await StartAsync(_staffBToken);
        var createdSeenByB = Collect<ChatRoomThreadInfoDto>(b, ChatRoomHubContract.ThreadCreated);

        var thread = await CreateThreadAsync(a, "Announced");
        var empty = await a.InvokeAsync<Result<ChatRoomThreadInfoDto>>(ChatRoomHubContract.CreateThread, " ");
        var notAThread = await a.InvokeAsync<Result<ChatRoomThreadDto>>(ChatRoomHubContract.Subscribe, Guid.NewGuid());

        await WaitUntilAsync(() => createdSeenByB.Any(t => t.ThreadId == thread.ThreadId));
        Assert.IsFalse(createdSeenByB.Single(t => t.ThreadId == thread.ThreadId).IsParticipant);
        Assert.IsFalse(empty.IsValid);
        Assert.IsFalse(notAThread.IsValid);
    }

    [TestMethod]
    public async Task TheGeneralThread_IsCreatedOnce_WhenAskedForAtTheSameTime()
    {
        await using var a = await StartAsync(_staffAToken);
        await using var b = await StartAsync(_staffBToken);

        var results = await Task.WhenAll(
            a.InvokeAsync<Result<ChatRoomThreadInfoDto>>(ChatRoomHubContract.GetGeneralThread),
            b.InvokeAsync<Result<ChatRoomThreadInfoDto>>(ChatRoomHubContract.GetGeneralThread),
            a.InvokeAsync<Result<ChatRoomThreadInfoDto>>(ChatRoomHubContract.GetGeneralThread));

        Assert.IsTrue(results.All(r => r.IsValid), string.Join(" ", results.Select(r => r.ErrorMessage)));
        Assert.AreEqual(1, results.Select(r => r.Entity.ThreadId).Distinct().Count());
        var threads = await a.InvokeAsync<Result<List<ChatRoomThreadInfoDto>>>(ChatRoomHubContract.GetThreads);
        Assert.AreEqual(1, threads.Entity.Count(t => t.Topic == KntConst.ChatRoomGeneralThreadTopic));
    }

    #region Helpers

    // A user registered and given the role by the class' Admin, signed in again for a token with that role.
    private static async Task<(string UserName, string Token)> NewUserAsync(string prefix, string role)
    {
        var userName = $"{prefix}{Guid.NewGuid():N}"[..24];
        var registerRes = await _adminClient.PostAsJsonAsync("api/users/register", new UserRegisterDto
        {
            UserName = userName,
            EMail = $"{Guid.NewGuid():N}@knote.tests",
            FullName = userName,
            Password = Password
        });
        registerRes.EnsureSuccessStatusCode();
        var registered = await registerRes.Content.ReadFromJsonAsync<UserTokenDto>();

        var user = (await _adminClient.GetFromJsonAsync<Result<UserDto>>($"api/users/{registered!.uid}"))!.Entity;
        user.RoleDefinition = role;
        (await _adminClient.PutAsJsonAsync("api/users", user)).EnsureSuccessStatusCode();

        var loginRes = await _adminClient.PostAsJsonAsync("api/users/login", new UserCredentialsDto { UserName = userName, Password = Password });
        loginRes.EnsureSuccessStatusCode();
        return (userName, (await loginRes.Content.ReadFromJsonAsync<UserTokenDto>())!.token);
    }

    private static HubConnection NewConnection(string? token) =>
        new HubConnectionBuilder()
            .WithUrl(new Uri(_factory.Server.BaseAddress, "chathub"), options =>
            {
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                // The test server has no WebSockets for the SignalR client.
                options.Transports = HttpTransportType.LongPolling;
                if (token != null)
                    options.AccessTokenProvider = () => Task.FromResult<string?>(token);
            })
            .Build();

    private static async Task<HubConnection> StartAsync(string token)
    {
        var connection = NewConnection(token);
        await connection.StartAsync();
        return connection;
    }

    private static async Task<ChatRoomThreadDto> CreateThreadAsync(HubConnection connection, string topic)
    {
        var res = await connection.InvokeAsync<Result<ChatRoomThreadInfoDto>>(ChatRoomHubContract.CreateThread, topic);
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        Assert.IsTrue(res.Entity.IsParticipant);

        // Read through a subscription, left at once: each test subscribes the connections it needs.
        var thread = await connection.InvokeAsync<Result<ChatRoomThreadDto>>(ChatRoomHubContract.Subscribe, res.Entity.ThreadId);
        Assert.IsTrue(thread.IsValid, thread.ErrorMessage);
        await connection.InvokeAsync<Result>(ChatRoomHubContract.Unsubscribe, res.Entity.ThreadId);
        return thread.Entity;
    }

    private static async Task<ChatRoomThreadDto> SubscribeAsync(HubConnection connection, Guid threadId)
    {
        var res = await connection.InvokeAsync<Result<ChatRoomThreadDto>>(ChatRoomHubContract.Subscribe, threadId);
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        return res.Entity;
    }

    private static ConcurrentQueue<T> Collect<T>(HubConnection connection, string method)
    {
        var received = new ConcurrentQueue<T>();
        connection.On<T>(method, received.Enqueue);
        return received;
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow + Timeout;
        while (!condition())
        {
            if (DateTime.UtcNow > deadline)
                Assert.Fail("Timed out waiting for the hub.");
            await Task.Delay(20);
        }
    }

    #endregion
}
