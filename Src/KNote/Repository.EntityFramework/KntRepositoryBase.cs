using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.ComponentModel;
using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using KNote.Model;
using KNote.Repository.Entities;

namespace KNote.Repository.EntityFramework
{
    public class KntRepositoryBase : DomainActionBase, IDisposable
    {
        protected internal readonly RepositoryRef _repositoryRef;

        protected readonly KntDbContext SingletonConnection;

        public KntRepositoryBase(KntDbContext singletonConnection, RepositoryRef repositoryRef)            
        {
            SingletonConnection = singletonConnection;            
            _repositoryRef = repositoryRef;
            _repositoryRef.ConnectionString = singletonConnection.Database.GetConnectionString();
        }

        public KntRepositoryBase(RepositoryRef repositoryRef)
        {            
            _repositoryRef = repositoryRef;
        }

        public virtual KntDbContext GetOpenConnection()
        {
            if (SingletonConnection != null)
                return SingletonConnection;

            var optionsBuilder = new DbContextOptionsBuilder<KntDbContext>();

            if (_repositoryRef.Provider == "Microsoft.Data.SqlClient")
                optionsBuilder.UseSqlServer(_repositoryRef.ConnectionString);
            else if (_repositoryRef.Provider == "Microsoft.Data.Sqlite")
                optionsBuilder.UseSqlite(_repositoryRef.ConnectionString);
            else
                throw new Exception("Data provider not suported (KntEx)");

            return new KntDbContext(optionsBuilder.Options);
        }

        public virtual async Task<bool> CloseIsTempConnection(KntDbContext db)
        {
            try
            {
                if (SingletonConnection == null)
                {
                    await db.DisposeAsync();
                    return true;
                }
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Dispose()
        {
            if (SingletonConnection != null)
                SingletonConnection.Dispose();
        }
    }
}
