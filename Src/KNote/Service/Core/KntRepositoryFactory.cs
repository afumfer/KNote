using KNote.Model;
using KNote.Repository;
using EF = KNote.Repository.EntityFramework;
using DP = KNote.Repository.Dapper;

namespace KNote.Service.Core;

public static class KntRepositoryFactory
{
    // EF is the single point of database/schema creation and update, regardless of which
    // ORM is configured to serve queries afterwards (Dapper is a query-only, auxiliary engine
    // with no schema-creation logic of its own).
    public static IKntRepository Create(RepositoryRef repositoryRef)
    {
        EF.KntSchemaUpdater.EnsureUpToDate(repositoryRef);

        return repositoryRef.Orm switch
        {
            "Dapper" => new DP.KntRepository(repositoryRef),
            "EntityFramework" => new EF.KntRepository(repositoryRef),
            _ => throw new KntServiceException($"Unsupported RepositoryRef.Orm value: '{repositoryRef.Orm}'.")
        };
    }
}
