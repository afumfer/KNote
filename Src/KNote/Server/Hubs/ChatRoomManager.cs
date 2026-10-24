using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KNote.Server.Hubs;

/// <summary>A chat room thread: each repository of the Server has its own chat room.</summary>
public readonly record struct ChatThreadKey(Guid RepositoryId, Guid ThreadId);

/// <summary>
/// What ChatHub keeps between calls (a Hub instance lives for one call only), as a singleton: which threads each
/// connection is subscribed to (the online users of a thread), and the locks that make the writes of a thread
/// happen one after the other (IKntChatRoomService doesn't serialize them: adding a message reads the thread's
/// note and saves it with one more message, so two at the same time would lose one). Creating the general thread
/// has its own lock in each repository, so two users asking for it at the same time don't create it twice.
/// </summary>
public class ChatRoomManager
{
    private readonly object _sync = new();
    private readonly Dictionary<string, Subscriber> _connections = new();
    private readonly ConcurrentDictionary<ChatThreadKey, SemaphoreSlim> _threadLocks = new();
    private readonly ConcurrentDictionary<Guid, SemaphoreSlim> _creationLocks = new();

    private sealed record Subscriber(string UserName, HashSet<ChatThreadKey> Threads);

    /// <summary>Subscribes the connection to the thread. Returns who is online in the thread now.</summary>
    public List<string> Subscribe(string connectionId, string userName, ChatThreadKey thread)
    {
        lock (_sync)
        {
            if (!_connections.TryGetValue(connectionId, out var subscriber))
            {
                subscriber = new Subscriber(userName, new HashSet<ChatThreadKey>());
                _connections[connectionId] = subscriber;
            }
            subscriber.Threads.Add(thread);
            return OnlineUserNames(thread);
        }
    }

    /// <summary>
    /// Unsubscribes the connection from the thread. Returns who is online in the thread now, or null if the
    /// connection wasn't subscribed to it.
    /// </summary>
    public List<string> Unsubscribe(string connectionId, ChatThreadKey thread)
    {
        lock (_sync)
        {
            if (!_connections.TryGetValue(connectionId, out var subscriber) || !subscriber.Threads.Remove(thread))
                return null;
            if (subscriber.Threads.Count == 0)
                _connections.Remove(connectionId);
            return OnlineUserNames(thread);
        }
    }

    /// <summary>Forgets a closed connection. Returns, for each thread it was subscribed to, who is online now.</summary>
    public Dictionary<ChatThreadKey, List<string>> RemoveConnection(string connectionId)
    {
        lock (_sync)
        {
            if (!_connections.Remove(connectionId, out var subscriber))
                return new Dictionary<ChatThreadKey, List<string>>();
            return subscriber.Threads.ToDictionary(thread => thread, OnlineUserNames);
        }
    }

    public Task<IDisposable> LockThreadAsync(ChatThreadKey thread) =>
        LockAsync(_threadLocks.GetOrAdd(thread, _ => new SemaphoreSlim(1, 1)));

    public Task<IDisposable> LockCreationAsync(Guid repositoryId) =>
        LockAsync(_creationLocks.GetOrAdd(repositoryId, _ => new SemaphoreSlim(1, 1)));

    // A user with several connections (two windows, the Web and ClientWin) is listed once.
    private List<string> OnlineUserNames(ChatThreadKey thread) =>
        _connections.Values
            .Where(s => s.Threads.Contains(thread))
            .Select(s => s.UserName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static async Task<IDisposable> LockAsync(SemaphoreSlim semaphore)
    {
        await semaphore.WaitAsync();
        return new Releaser(semaphore);
    }

    private sealed class Releaser : IDisposable
    {
        private SemaphoreSlim _semaphore;

        public Releaser(SemaphoreSlim semaphore) => _semaphore = semaphore;

        public void Dispose() => Interlocked.Exchange(ref _semaphore, null)?.Release();
    }
}
