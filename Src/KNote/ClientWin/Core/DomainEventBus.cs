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
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();

    public void Subscribe<TMessage>(Action<TMessage> handler)
    {
        if (!_handlers.TryGetValue(typeof(TMessage), out var handlersForType))
        {
            handlersForType = new List<Delegate>();
            _handlers[typeof(TMessage)] = handlersForType;
        }

        handlersForType.Add(handler);
    }

    public void Unsubscribe<TMessage>(Action<TMessage> handler)
    {
        if (_handlers.TryGetValue(typeof(TMessage), out var handlersForType))
            handlersForType.Remove(handler);
    }

    public void Publish<TMessage>(TMessage message)
    {
        if (!_handlers.TryGetValue(typeof(TMessage), out var handlersForType))
            return;

        // Snapshot before invoking: a handler may subscribe/unsubscribe as a reaction to this event.
        foreach (var handler in handlersForType.ToArray())
            ((Action<TMessage>)handler).Invoke(message);
    }
}
