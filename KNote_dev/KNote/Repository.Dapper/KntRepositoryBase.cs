using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Dapper;

using KNote.Model;


namespace KNote.Repository.Dapper
{
    public abstract class KntRepositoryBase : DomainActionBase, IDisposable
    {

        protected readonly string ConnectionString;
        protected readonly string Provider;
        protected readonly DbConnection SingletonConnection;

        public KntRepositoryBase(DbConnection singletonConnection, bool throwKntException = false)
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

        public virtual DbConnection GetOpenConnection()
        {
            if (SingletonConnection != null)
                return SingletonConnection;
            
            if (Provider == "Microsoft.Data.SqlClient")
            {
                return new SqlConnection(ConnectionString);                
            }
            else if (Provider == "Microsoft.Data.Sqlite")
            {
                // TODO: Estudiar poner esto en otro sitio, una clase estática. 
                //       SqlMapper es estático.                    
                SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());
                SqlMapper.AddTypeHandler(new GuidHandler());
                SqlMapper.AddTypeHandler(new TimeSpanHandler());
                // ---
                return new SqliteConnection(ConnectionString);                
            }
            else
                throw new Exception("Data provider not suported (KntEx)");            
        }

        public virtual async Task<bool> CloseIsTempConnection(DbConnection db)
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
