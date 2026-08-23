using KNote.Service.Core;

namespace KNote.ClientWin.Core;

/// <summary>
/// Storage and lookup for the ServiceRef instances (repository/database connections) configured
/// in the running app (Fase 2 of the ClientWin architecture refactor, see ClientWin/CLAUDE.md).
/// Logging, events and AppConfig persistence stay in Store; this class only owns the collection.
/// </summary>
public class ServiceRefRegistry
{
    private readonly List<ServiceRef> _serviceRefs = new();

    public void Add(ServiceRef serviceRef) => _serviceRefs.Add(serviceRef);

    public void Remove(ServiceRef serviceRef) => _serviceRefs.Remove(serviceRef);

    public List<ServiceRef> GetAll() => _serviceRefs.ToList();

    public ServiceRef GetById(Guid id) => _serviceRefs.FirstOrDefault(_ => _.IdServiceRef == id);

    public ServiceRef GetByAlias(string alias) => _serviceRefs.FirstOrDefault(_ => _.Alias == alias);

    public ServiceRef GetFirst() => _serviceRefs.FirstOrDefault();
}
