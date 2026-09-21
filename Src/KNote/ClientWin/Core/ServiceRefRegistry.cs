using KNote.Service.Core;

namespace KNote.ClientWin.Core;

/// <summary>
/// Storage and lookup for the ServiceRef instances (repository/database connections) configured
/// in the running app (Fase 2 of the ClientWin architecture refactor, see ClientWin/CLAUDE.md).
/// Logging, events and config persistence stay in Store; this class only owns the collection.
/// </summary>
public class ServiceRefRegistry
{
    // Same shape of bug as ControllerRegistry/DomainEventBus: Add/Remove normally run on the UI
    // thread, but nothing stops a KntScript running on its own background Thread
    // (Store.RunKntSCodeInNewThread) from reaching a code path that adds/removes a ServiceRef while
    // MessagesManagmentCtrl's autosave/alarm timers (UI thread) call GetAll()/GetById()/etc. GetAll
    // already returned a copy, which hid the most common symptom (a live-list foreach throwing),
    // but the copy itself - and every other read here - was still an unsynchronized read racing an
    // unsynchronized write.
    private readonly object _lock = new();
    private readonly List<ServiceRef> _serviceRefs = new();

    public void Add(ServiceRef serviceRef)
    {
        lock (_lock)
            _serviceRefs.Add(serviceRef);
    }

    public void Remove(ServiceRef serviceRef)
    {
        lock (_lock)
            _serviceRefs.Remove(serviceRef);
    }

    public List<ServiceRef> GetAll()
    {
        lock (_lock)
            return _serviceRefs.ToList();
    }

    public ServiceRef GetById(Guid id)
    {
        lock (_lock)
            return _serviceRefs.FirstOrDefault(_ => _.IdServiceRef == id);
    }

    public ServiceRef GetByAlias(string alias)
    {
        lock (_lock)
            return _serviceRefs.FirstOrDefault(_ => _.Alias == alias);
    }

    public ServiceRef GetFirst()
    {
        lock (_lock)
            return _serviceRefs.FirstOrDefault();
    }
}
