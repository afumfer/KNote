using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KNote.Model;
using KNote.Repository;
using EF = KNote.Repository.EntityFramework;
using DP = KNote.Repository.Dapper;

using KNote.Service;
using KNote.Service.Services;

namespace KNote.ClientWin.Core
{
    public class ServiceRef
    {
        public Guid IdServiceRef 
        {
            get
            {
                return Service.IdServiceRef;
            }
        }

        public string Alias { 
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
                {
                    //_service = new KntService(ConnectionString, Provider);
                    if (RepositoryRef.Orm == "Dapper")
                        _repository = new DP.KntRepository(RepositoryRef.ConnectionString, RepositoryRef.Provider);
                    else if (RepositoryRef.Orm == "EntityFramework")
                        _repository = new EF.KntRepository(RepositoryRef.ConnectionString, RepositoryRef.Provider);
                }
                return _repository;
            }

        }

        public IKntService _service;
        public IKntService Service
        {
            get
            {
                if (_service == null)
                    _service = new KntService(Repository);
                return _service;
            }
        }

        public ServiceRef (string name, string connectionString, string provider, string orm)
        {            
            RepositoryRef = new RepositoryRef { Alias = name, ConnectionString = connectionString, Provider = provider, Orm = orm };
        }

        public ServiceRef(RepositoryRef repositoryRef)
        {         
            RepositoryRef = repositoryRef;
        }

    }
}
