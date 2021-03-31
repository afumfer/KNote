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
        protected internal readonly RepositoryRef _repositoryRef;
        protected readonly DbConnection SingletonConnection;        

        public KntRepositoryBase(DbConnection singletonConnection, RepositoryRef repositoryRef, bool throwKntException = false)
        {
            SingletonConnection = singletonConnection;
            ThrowKntException = throwKntException;
            _repositoryRef = repositoryRef;            
        }

        public KntRepositoryBase(RepositoryRef repositoryRef, bool throwKntException = false)
        {
            ThrowKntException = throwKntException;
            _repositoryRef = repositoryRef;
        }

        public virtual DbConnection GetOpenConnection()
        {
            if (SingletonConnection != null)
                return SingletonConnection;
            
            if (_repositoryRef.Provider == "Microsoft.Data.SqlClient")
            {
                return new SqlConnection(_repositoryRef.ConnectionString);                
            }
            else if (_repositoryRef.Provider == "Microsoft.Data.Sqlite")
            {
                // SqlMapper is static, this is a problem (to study in the future ) 
                SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());
                SqlMapper.AddTypeHandler(new GuidHandler());
                SqlMapper.AddTypeHandler(new TimeSpanHandler());
                // ---
                return new SqliteConnection(_repositoryRef.ConnectionString);                
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
