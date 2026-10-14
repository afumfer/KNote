using System;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Repository;
using Microsoft.Extensions.Logging;

namespace KNote.Service.Core;

public class ServiceRef
{
    #region Properties 

    public Guid IdServiceRef
    {
        get
        {
            return Service.IdServiceRef;
        }
    }

    // Not to be confused with IdServiceRef, which identifies this ServiceRef while the application runs:
    // RepositoryId identifies the database itself and never changes (see LoadRepositoryInfo).
    public Guid RepositoryId
    {
        get
        {
            return RepositoryRef?.RepositoryId ?? Guid.Empty;
        }
    }

    public string Name
    {
        get
        {
            return RepositoryRef?.Name;
        }
    }

    public RepositoryRef RepositoryRef { get; protected set; }

    private IKntRepository _repository;
    protected IKntRepository Repository
    {
        get
        {
            _repository ??= KntRepositoryFactory.Create(RepositoryRef);
            return _repository;
        }

    }

    public IKntService _service;
    public IKntService Service
    {
        get
        {
            if (_service == null)
            {
                _service = new KntService(Repository, ActivateMessageBroker);
                _service.UserIdentityName = UserIdentityName;
                _service.EnforceAuthorization = EnforceAuthorization;
            }
            return _service;
        }
    }

    public string UserIdentityName { get; init; }

    public bool ActivateMessageBroker { get; init; }

    // ServiceRef is ClientWin's way into the Service layer (Server builds KntService through DI instead),
    // so by default every command is checked against the role of UserIdentityName in this repository.
    public bool EnforceAuthorization { get; }

    #endregion

    #region Constructor

    public ServiceRef(RepositoryRef repositoryRef, string userIdentityName, bool activateMessageBroker = false, ILogger logger = null,
        bool enforceAuthorization = true)
    {
        RepositoryRef = repositoryRef;
        UserIdentityName = userIdentityName;
        ActivateMessageBroker = activateMessageBroker;
        EnforceAuthorization = enforceAuthorization;
        Service.Logger = logger;
        LoadRepositoryInfo();
    }

    #endregion

    #region Public methods

    // The repository's id and name come from its database (opened, and given them if it had none, by
    // KntRepositoryFactory when Service was built above), not from the configuration. A repository that
    // can't tell them can't be told apart from the others, so it is not opened.
    public void LoadRepositoryInfo()
    {
        var result = Task.Run(() => Service.SystemValues.GetRepositoryInfoAsync()).GetAwaiter().GetResult();
        if (!result.IsValid)
            throw new KntServiceException($"The identity of the repository '{RepositoryRef?.Name}' could not be read: {result.ErrorMessage}");

        RepositoryRef.RepositoryId = result.Entity.RepositoryId;
        RepositoryRef.Name = result.Entity.Name;
    }

    #endregion 
}
