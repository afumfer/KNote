namespace KNote.ClientWin.Core;

/// <summary>
/// Simple in-process publish/subscribe bus (Fase 3 of the ClientWin architecture refactor, see
/// ClientWin/CLAUDE.md). Lets any controller publish or consume domain events by message type,
/// without Store needing to know about concrete controller types - unlike the
/// NoteEditorCtrl/PostItEditorCtrl special-casing in Store.AddController/RemoveController, which
/// is left untouched for now and is migrated to this bus in a later phase of the plan.
/// </summary>
public class DomainEventBus
{
    // Subscribe/Unsubscribe normally happen on the UI thread (a controller subscribing/
    // unsubscribing in OnInitialized/OnFinalized, e.g. opening or closing the Monitor window), but
    // Publish can be called from a non-UI thread too - most concretely, KntServiceBase raises
    // CommandExecuting/CommandExecuted synchronously on whatever thread runs the command, including
    // a KntScript's own background Thread (Store.RunKntSCodeInNewThread), and Store republishes
    // those onto this same bus. Without synchronization, a Dictionary/List mutated by
    // Subscribe/Unsubscribe while Publish concurrently reads/iterates it is undefined behavior, not
    // just a rare exception - opening/closing a window while a background script publishes events
    // is an entirely ordinary sequence of user actions, not an edge case.
    private readonly object _lock = new();
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();

    public void Subscribe<TMessage>(Action<TMessage> handler)
    {
        lock (_lock)
        {
            if (!_handlers.TryGetValue(typeof(TMessage), out var handlersForType))
            {
                handlersForType = new List<Delegate>();
                _handlers[typeof(TMessage)] = handlersForType;
            }

            handlersForType.Add(handler);
        }
    }

    public void Unsubscribe<TMessage>(Action<TMessage> handler)
    {
        lock (_lock)
        {
            if (_handlers.TryGetValue(typeof(TMessage), out var handlersForType))
                handlersForType.Remove(handler);
        }
    }

    public void Publish<TMessage>(TMessage message)
    {
        Delegate[] handlersSnapshot;

        lock (_lock)
        {
            if (!_handlers.TryGetValue(typeof(TMessage), out var handlersForType))
                return;

            // Snapshot before invoking: a handler may subscribe/unsubscribe as a reaction to this event.
            handlersSnapshot = handlersForType.ToArray();
        }

        // Invoke outside the lock: handlers can run arbitrary UI code (including opening a modal
        // dialog) or themselves call Subscribe/Unsubscribe/Publish, and holding the lock across
        // that would block every other thread's Subscribe/Unsubscribe/Publish for as long as the
        // slowest handler takes - or, on the same thread, unnecessarily reacquire a lock that's
        // already reentrant-safe once released here.
        foreach (var handler in handlersSnapshot)
            ((Action<TMessage>)handler).Invoke(message);
    }
}
