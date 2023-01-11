using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using KNote.Model;

namespace KNote.Repository.EntityFramework;

public abstract class KntRepositoryEFBase : KntRepositoryBase, IDisposable
{   
    protected readonly KntDbContext _singletonConnection;

    public KntRepositoryEFBase(KntDbContext singletonConnection, RepositoryRef repositoryRef) : base (repositoryRef)
    {
        _singletonConnection = singletonConnection;        
        _repositoryRef.ConnectionString = singletonConnection.Database.GetConnectionString();
    }

    public KntRepositoryEFBase(RepositoryRef repositoryRef) : base(repositoryRef)
    {
        
    }

    public virtual KntDbContext GetOpenConnection()
    {
        if (_singletonConnection != null)
            return _singletonConnection;

        var optionsBuilder = new DbContextOptionsBuilder<KntDbContext>();

        if (_repositoryRef.Provider == "Microsoft.Data.SqlClient")
            optionsBuilder.UseSqlServer(_repositoryRef.ConnectionString);
        else if (_repositoryRef.Provider == "Microsoft.Data.Sqlite")
        {
            optionsBuilder.UseSqlite(_repositoryRef.ConnectionString);
            // Entity framework core for Sqlite no support AmbientTransaction
            optionsBuilder.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
        }
        else
            throw new Exception("Data provider not suported (KntEx)");

        return new KntDbContext(optionsBuilder.Options);
    }

    public virtual async Task<bool> CloseIsTempConnection(KntDbContext db)
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
