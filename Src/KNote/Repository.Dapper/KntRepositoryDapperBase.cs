using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Dapper;
using KNote.Model;
using KNote.Model.Dto;

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

    /// <summary>
    /// A transaction of the connection (opened if it isn't: SqlClient's are not). Every command of it must be given it, also in
    /// Sqlite. An ambient TransactionScope is no substitute: Sqlite's connections don't enlist in it.
    /// </summary>
    protected static async Task<DbTransaction> BeginTransactionAsync(DbConnection db)
    {
        if (db.State != System.Data.ConnectionState.Open)
            await db.OpenAsync();
        return await db.BeginTransactionAsync();
    }

    /// <summary>
    /// A change of one statement and the rows of the history that note it (H10), in one transaction of the connection: if a row
    /// can't be written, the change is undone (the exception goes on). Without rows, the statement as always. Its rows affected.
    /// </summary>
    protected static async Task<int> ExecuteNotedAsync(DbConnection db, string sql, object param, IEnumerable<KLogWriteDto> history)
    {
        var writes = history?.Where(w => w?.Row != null).ToList();
        if (writes == null || writes.Count == 0)
            return await db.ExecuteAsync(sql, param);

        await using var transaction = await BeginTransactionAsync(db);
        var rows = await db.ExecuteAsync(sql, param, transaction);
        if (rows > 0)
        {
            foreach (var write in writes)
                await KntHistoryWriter.WriteAsync(db, transaction, write);
            await transaction.CommitAsync();
        }
        return rows;
    }

    protected static Task<int> ExecuteNotedAsync(DbConnection db, string sql, object param, KLogWriteDto history) =>
        ExecuteNotedAsync(db, sql, param, history == null ? null : new[] { history });

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