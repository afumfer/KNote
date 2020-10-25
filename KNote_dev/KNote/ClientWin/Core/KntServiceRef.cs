using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KNote.Model;
//using KNote.DomainModel.Infrastructure;

//using KNote.DomainModel.Services;

namespace KNote.ClientWin.Core
{
    public class KntServiceRef
    {
        public readonly Guid IdServiceRef;

        public string Name { get; }

        public string ConnectionString { get; }

        public string Provider { get;  }

        //private KntService _service;
        //public KntService Service
        //{
        //    get {
        //        if (_service == null)
        //            _service = new KntService(ConnectionString, Provider);
        //        return _service;
        //    }

        //}

        public KntServiceRef (string name, string connectionString, string provider)
        {
            IdServiceRef = Guid.NewGuid();
            Name = name;
            ConnectionString = connectionString;
            Provider = provider;
        }
    }
}
