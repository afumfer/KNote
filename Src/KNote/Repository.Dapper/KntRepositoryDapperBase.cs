using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Dapper;
using KNote.Model;

namespace KNote.Repository.Dapper;

public abstract class KntRepositoryDapperBase : KntRepositoryBase, IDisposable
{    
    protected readonly DbConnection _singletonConnection;

    public KntRepositoryDapperBase(DbConnection singletonConnection, RepositoryRef repositoryRef) : base(repositoryRef)
    {
        _singletonConnection = singletonConnection;
        
    }

    public KntRepositoryDapperBase(RepositoryRef repositoryRef) : base(repositoryRef)
    {
        
    }

    public virtual DbConnection GetOpenConnection()
    {
        if (_singletonConnection != null)
            return _singletonConnection;

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
            if (_singletonConnection == null)
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
        if (_singletonConnection != null)
            _singletonConnection.Dispose();
    }

}