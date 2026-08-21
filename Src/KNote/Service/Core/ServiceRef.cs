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
            }
            return _service;
        }
    }

    public string UserIdentityName { get; init; }

    public bool ActivateMessageBroker { get; init; }

    #endregion

    #region Constructor

    public ServiceRef(RepositoryRef repositoryRef, string userIdentityName, bool activateMessageBroker = false, ILogger logger = null)
    {
        RepositoryRef = repositoryRef;
        UserIdentityName = userIdentityName;
        ActivateMessageBroker = activateMessageBroker;
        Service.Logger = logger;
    }

    #endregion 
}
