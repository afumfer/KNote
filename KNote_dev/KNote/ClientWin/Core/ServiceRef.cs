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
        public readonly Guid IdRepositoryRef;

        public string Alias { get; }

        public string ConnectionString { get; }

        public string Provider { get;  }        
        public string Orm { get; }

        private IKntRepository _repository;
        protected IKntRepository Repository
        {
            get
            {
                if (_repository == null)
                {
                    //_service = new KntService(ConnectionString, Provider);
                    if (Orm == "Dapper")
                        _repository = new DP.KntRepository(ConnectionString, Provider);
                    else if (Orm == "EntityFramework")
                        _repository = new EF.KntRepository(ConnectionString, Provider);
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
            IdRepositoryRef = Guid.NewGuid();
            Alias = name;
            ConnectionString = connectionString;
            Provider = provider;
            Orm = orm;
        }
    }
}
