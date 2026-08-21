using KNote.Model.Dto;
using KNote.Repository;
using KNote.Tests.Helpers;

namespace KNote.Tests.RepositoryParityTests;

/// <summary>
/// Documents (and guards) a divergence found while building this suite: Repository.Dapper and
/// Repository.EntityFramework both wrap failures in KntRepositoryException with the same outer
/// message shape ("KNote repository error. ({DeclaringType})"), but their InnerException differs -
/// EF wraps the provider error as DbUpdateException -> SqliteException/SqlException, Dapper
/// propagates the provider exception directly with no intermediate wrapper. No caller currently
/// inspects InnerException to distinguish "duplicate key" from other failures, so this is not a
/// live bug today - but it is exactly the kind of asymmetry this suite exists to keep visible
/// (see the improvement proposal's point 1, "Suite de tests de paridad Dapper/EF").
/// </summary>
[TestClass]
public class ConstraintViolationParityTests
{
    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task AddUser_WithDuplicateEmail_ThrowsKntRepositoryException(string orm)
    {
        using var db = new RepositoryTestDatabase();
        using var repo = db.CreateRepository(orm);

        string duplicateEmail = $"{Guid.NewGuid():N}@parity.tests";

        var firstUser = new UserDto
        {
            UserId = Guid.NewGuid(),
            UserName = $"parity-1-{Guid.NewGuid():N}"[..24],
            EMail = duplicateEmail,
            FullName = "Parity User 1",
            RoleDefinition = "Public"
        };
        var firstRes = await repo.Users.AddAsync(firstUser);
        Assert.IsTrue(firstRes.IsValid, firstRes.ErrorMessage);

        var secondUser = new UserDto
        {
            UserId = Guid.NewGuid(),
            UserName = $"parity-2-{Guid.NewGuid():N}"[..24],
            EMail = duplicateEmail,
            FullName = "Parity User 2",
            RoleDefinition = "Public"
        };

        var ex = await Assert.ThrowsExactlyAsync<KntRepositoryException>(() => repo.Users.AddAsync(secondUser));
        Assert.IsNotNull(ex.InnerException);
    }
}
