using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using KNote.Model;

namespace KNote.Repository.EntityFramework;

public abstract class KntRepositoryBase : DomainActionBase, IDisposable
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

    internal void AddDBEntityErrorsToResult(KntEntityValidationException ex, ResultBase result)
    {
        foreach (var errEntity in ex.ValidationResults)
            foreach (var err in errEntity.ValidationResults)
                result.AddErrorMessage($"{errEntity.ToString()} - {err.ErrorMessage}");
    }

    public void Dispose()
    {
        if (SingletonConnection != null)
            SingletonConnection.Dispose();
    }
}
