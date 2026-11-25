using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using KNote.Model;
using KNote.Model.Dto;

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
            optionsBuilder.AddInterceptors(new KntSqliteAccentInterceptor());
        }
        else
            throw new Exception("Data provider not suported (KntEx)");

        return new KntDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// A change and the rows of the history that note it (H10), in one transaction of the context: if the change is not valid
    /// or a row can't be written (the exception goes on), nothing is kept. Without rows, the change as always.
    /// </summary>
    protected static async Task<TResult> NotedAsync<TResult>(KntDbContext ctx, IEnumerable<KLogWriteDto> history, Func<Task<TResult>> change)
        where TResult : Result
    {
        var writes = history?.Where(w => w?.Row != null).ToList();
        if (writes == null || writes.Count == 0)
            return await change();

        await using var transaction = await ctx.Database.BeginTransactionAsync();
        var result = await change();
        if (result.IsValid)
        {
            foreach (var write in writes)
                await KntHistoryWriter.WriteAsync(ctx, write);
            await transaction.CommitAsync();
        }
        return result;
    }

    protected static Task<TResult> NotedAsync<TResult>(KntDbContext ctx, KLogWriteDto history, Func<Task<TResult>> change)
        where TResult : Result =>
        NotedAsync(ctx, history == null ? null : new[] { history }, change);

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
