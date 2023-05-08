﻿using System;

using KNote.Model;
using KNote.Repository;
using EF = KNote.Repository.EntityFramework;
using DP = KNote.Repository.Dapper;
using KNote.Service.Services;

namespace KNote.Service.Core
{
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
                if (_repository == null)
                    // TODO: hack, implement here IoC.
                    if (RepositoryRef.Orm == "Dapper")
                        _repository = new DP.KntRepository(RepositoryRef);
                    else if (RepositoryRef.Orm == "EntityFramework")
                        _repository = new EF.KntRepository(RepositoryRef);
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
                    _service = new KntService(Repository, true);
                    _service.UserIdentityName = UserIdentityName;
                }
                return _service;
            }
        }

        public string UserIdentityName { get; init; }


        #endregion

        #region Constructor

        public ServiceRef(RepositoryRef repositoryRef, string userIdentityName)
        {
            RepositoryRef = repositoryRef;
            UserIdentityName = userIdentityName;
        }

        #endregion 
    }
}
