using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Dapper;
using KNote.Model;

namespace KNote.Repository.Dapper;

public abstract class KntRepositoryDapperBase : KntRepositoryBase, IDisposable
{
    protected readonly DbConnection _singletonConnection;

    // SqlMapper.AddTypeHandler mutates Dapper's static, process-wide handler registry - registering
    // the same handlers on every GetOpenConnection() call was already dubious under sequential
    // execution (see the comment below) and became a real concurrent-write risk once
    // KntNotesGetExtendedAsyncCommand started opening several Sqlite connections in parallel via
    // Task.WhenAll. Double-checked locking here means the registration itself runs at most once per
    // process, and the lock is never taken again afterwards.
    private static readonly object _sqliteTypeHandlersLock = new();
    private static volatile bool _sqliteTypeHandlersRegistered;

    internal static void EnsureSqliteTypeHandlersRegistered()
    {
        if (_sqliteTypeHandlersRegistered)
            return;

        lock (_sqliteTypeHandlersLock)
        {
            if (_sqliteTypeHandlersRegistered)
                return;

            SqlMapper.AddTypeHandler(new DateTimeOffsetHandler());
            SqlMapper.AddTypeHandler(new GuidHandler());
            SqlMapper.AddTypeHandler(new TimeSpanHandler());

            _sqliteTypeHandlersRegistered = true;
        }
    }

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
            EnsureSqliteTypeHandlersRegistered();

            var connection = new SqliteConnection(_repositoryRef.ConnectionString);
            connection.Open();
            KntSqliteAccentFunctions.Register(connection);
            return connection;
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