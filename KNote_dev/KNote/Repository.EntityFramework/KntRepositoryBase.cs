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
        protected readonly string ConnectionString;
        protected readonly string Provider;
        protected readonly KntDbContext SingletonConnection;

        public KntRepositoryBase(KntDbContext singletonConnection, bool throwKntException = false)
        {
            SingletonConnection = singletonConnection;
            ThrowKntException = throwKntException;
        }

        public KntRepositoryBase(string connectionString, string provider, bool throwKntException = false)
        {
            ConnectionString = connectionString;
            Provider = provider;
            ThrowKntException = throwKntException;
        }

        public virtual KntDbContext GetOpenConnection()
        {
            if (SingletonConnection != null)
                return SingletonConnection;

            var optionsBuilder = new DbContextOptionsBuilder<KntDbContext>();

            if (Provider == "Microsoft.Data.SqlClient")
                optionsBuilder.UseSqlServer(ConnectionString);
            else if (Provider == "Microsoft.Data.Sqlite")
                optionsBuilder.UseSqlite(ConnectionString);
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
