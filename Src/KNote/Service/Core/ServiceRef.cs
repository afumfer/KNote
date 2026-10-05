using System;
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

    public string Alias
    {
        get
        {
            return RepositoryRef?.Alias;
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
    }

    #endregion 
}
