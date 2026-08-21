using KNote.Model;
using KNote.Repository;
using EF = KNote.Repository.EntityFramework;
using DP = KNote.Repository.Dapper;

namespace KNote.Service.Core;

public static class KntRepositoryFactory
{
    public static IKntRepository Create(RepositoryRef repositoryRef) => repositoryRef.Orm switch
    {
        "Dapper" => new DP.KntRepository(repositoryRef),
        "EntityFramework" => new EF.KntRepository(repositoryRef),
        _ => throw new KntServiceException($"Unsupported RepositoryRef.Orm value: '{repositoryRef.Orm}'.")
    };
}
