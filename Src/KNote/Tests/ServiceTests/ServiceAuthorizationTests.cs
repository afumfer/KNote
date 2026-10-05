using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Tests.Helpers;

namespace KNote.Tests.ServiceTests;

/// <summary>
/// Authorization in the Service layer: with IKntService.EnforceAuthorization on, every command is checked
/// against the [KntAuthorize] role its class declares (KntCommandServiceBase.ValidateAuthorizationAsync,
/// run by KntServiceBase.ExecuteCommand), using the role of UserIdentityName in that repository.
/// Exercised against a real Sqlite repository - the same KntService ClientWin builds through ServiceRef.
/// </summary>
[TestClass]
public class ServiceAuthorizationTests
{
    [TestMethod]
    public async Task EnforcementOff_CommandsRunWithoutAnyRegisteredUser()
    {
        using var db = new RepositoryTestDatabase();
        var service = new KntService(db.CreateRepository("Dapper")) { UserIdentityName = "nobody" };

        var res = await service.NoteTypes.SaveAsync(new NoteTypeDto { Name = $"type-{Guid.NewGuid():N}" });

        Assert.IsTrue(res.IsValid, res.ErrorMessage);
        Assert.IsFalse(res.NotAuthorized);
    }

    [TestMethod]
    public async Task ServiceRef_TurnsEnforcementOn()
    {
        using var db = new RepositoryTestDatabase();
        var repositoryRef = new RepositoryRef
        {
            Alias = "ServiceRefTests",
            Orm = "Dapper",
            Provider = "Microsoft.Data.Sqlite",
            ConnectionString = $"Data Source={db.DatabaseFilePath}"
        };

        var serviceRef = new ServiceRef(repositoryRef, "jdoe");

        Assert.IsTrue(serviceRef.Service.EnforceAuthorization);
        Assert.AreEqual("jdoe", serviceRef.Service.UserIdentityName);

        // ...unless asked not to (ClientWin's read-only Assistant catalog repository).
        var catalogServiceRef = new ServiceRef(repositoryRef, "jdoe", enforceAuthorization: false);
        Assert.IsFalse(catalogServiceRef.Service.EnforceAuthorization);
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task UnregisteredUser_IsRefusedEvenToRead(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = EnforcedService(db, orm, "nobody");

        var res = await service.Folders.GetHomeAsync();

        Assert.IsFalse(res.IsValid);
        Assert.IsTrue(res.NotAuthorized);
        StringAssert.Contains(res.ErrorMessage, "not registered");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Guest_CanRead_ButNotCreateOrSaveNotes(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = EnforcedService(db, orm, "guest");
        await AddUserAsync(service, "guest", "Guest");

        var home = await service.Folders.GetHomeAsync();
        Assert.IsTrue(home.IsValid, home.ErrorMessage);

        var newNote = await service.Notes.NewExtendedAsync();
        Assert.IsTrue(newNote.NotAuthorized, "Starting a new note already needs Staff.");

        var topic = $"Guest note {Guid.NewGuid():N}";
        var save = await service.Notes.SaveAsync(new NoteDto { Topic = topic, FolderId = home.Entity.FolderId });
        Assert.IsTrue(save.NotAuthorized);
        StringAssert.Contains(save.ErrorMessage, "Staff");

        var search = await service.Repository.Notes.GetSearchAsync(new NotesSearchDto { TextSearch = topic });
        Assert.AreEqual(0, search.Entity?.Count ?? 0, "A refused command must not touch the database.");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Staff_CanSaveNotes_ButNotFolders(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = EnforcedService(db, orm, "staff");
        await AddUserAsync(service, "staff", "Staff");

        var note = (await service.Notes.NewAsync()).Entity;
        note.Topic = "Staff note";
        note.FolderId = (await service.Folders.GetHomeAsync()).Entity.FolderId;
        var saveNote = await service.Notes.SaveAsync(note);
        Assert.IsTrue(saveNote.IsValid, saveNote.ErrorMessage);

        var saveFolder = await service.Folders.SaveAsync(new FolderDto { Name = "Staff folder" });
        Assert.IsTrue(saveFolder.NotAuthorized);
        StringAssert.Contains(saveFolder.ErrorMessage, "ProjectManager");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task ProjectManager_CanSaveFolders_ButNotNoteTypes(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = EnforcedService(db, orm, "pm");
        await AddUserAsync(service, "pm", "ProjectManager");

        var saveFolder = await service.Folders.SaveAsync(new FolderDto { Name = "PM folder" });
        Assert.IsTrue(saveFolder.IsValid, saveFolder.ErrorMessage);

        var saveType = await service.NoteTypes.SaveAsync(new NoteTypeDto { Name = $"type-{Guid.NewGuid():N}" });
        Assert.IsTrue(saveType.NotAuthorized);
        StringAssert.Contains(saveType.ErrorMessage, "Admin");
    }

    [TestMethod]
    [DataRow("Dapper")]
    [DataRow("EntityFramework")]
    public async Task Admin_CanDoEverything(string orm)
    {
        using var db = new RepositoryTestDatabase();
        var service = EnforcedService(db, orm, "admin");
        await AddUserAsync(service, "admin", "Staff, Admin");

        Assert.IsTrue((await service.Folders.SaveAsync(new FolderDto { Name = "Admin folder" })).IsValid);
        Assert.IsTrue((await service.NoteTypes.SaveAsync(new NoteTypeDto { Name = $"type-{Guid.NewGuid():N}" })).IsValid);
        Assert.IsTrue((await service.Users.GetAllAsync()).IsValid);
    }

    [TestMethod]
    public async Task DisabledUser_IsRefused()
    {
        using var db = new RepositoryTestDatabase();
        var service = EnforcedService(db, "Dapper", "disabled");
        await AddUserAsync(service, "disabled", "Admin", disabled: true);

        var res = await service.Folders.GetHomeAsync();

        Assert.IsTrue(res.NotAuthorized);
    }

    [TestMethod]
    public async Task GetCurrentUser_TellsADisabledUserApartFromAnUnregisteredOne()
    {
        using var db = new RepositoryTestDatabase();
        var disabled = EnforcedService(db, "Dapper", "disabled");
        await AddUserAsync(disabled, "disabled", "Admin", disabled: true);
        var unregistered = EnforcedService(db, "Dapper", "nobody");

        var disabledUser = await disabled.GetCurrentUserAsync();
        Assert.IsNotNull(disabledUser);
        Assert.IsTrue(disabledUser.Disabled);
        Assert.IsNull(await disabled.GetCurrentUserRoleAsync());

        Assert.IsNull(await unregistered.GetCurrentUserAsync());
        Assert.IsNull(await unregistered.GetCurrentUserRoleAsync());
    }

    [TestMethod]
    public async Task AnonymousCommands_RunForUnregisteredUsers()
    {
        using var db = new RepositoryTestDatabase();
        var service = EnforcedService(db, "Dapper", "newcomer");

        var auth = await service.Users.AuthenticateAsync(new UserCredentialsDto { UserName = "newcomer", Password = "wrong" });
        Assert.IsFalse(auth.IsValid, "Unknown user: not authenticated...");
        Assert.IsFalse(auth.NotAuthorized, "...but not refused by authorization either.");

        var register = await service.Users.RegisterAsync(new UserRegisterDto
        {
            UserName = "newcomer",
            EMail = "newcomer@knote.tests",
            FullName = "Newcomer",
            Password = "Newcomer-Password-1!"
        });
        Assert.IsTrue(register.IsValid, register.ErrorMessage);

        // Registering resets the cached role: the newcomer (first one after adminKNote, so an Admin) can
        // now use the repository with the same service instance.
        var home = await service.Folders.GetHomeAsync();
        Assert.IsTrue(home.IsValid, home.ErrorMessage);
    }

    [TestMethod]
    public async Task ChangingTheCurrentUser_IsPickedUpByTheNextCommand()
    {
        using var db = new RepositoryTestDatabase();
        var service = EnforcedService(db, "Dapper", "admin");
        await AddUserAsync(service, "admin", "Admin");
        Assert.AreEqual(EnumRoles.Admin, await service.GetCurrentUserRoleAsync());

        var me = (await service.Users.GetByUserNameAsync("admin")).Entity;
        me.RoleDefinition = "Staff";
        Assert.IsTrue((await service.Users.SaveAsync(me)).IsValid);

        Assert.AreEqual(EnumRoles.Staff, await service.GetCurrentUserRoleAsync());
        Assert.IsTrue((await service.Folders.SaveAsync(new FolderDto { Name = "After demotion" })).NotAuthorized);
    }

    [TestMethod]
    public async Task GetCurrentUserRole_IsCachedUntilReset()
    {
        using var db = new RepositoryTestDatabase();
        var service = EnforcedService(db, "Dapper", "cached");
        await AddUserAsync(service, "cached", "Guest");
        Assert.AreEqual(EnumRoles.Guest, await service.GetCurrentUserRoleAsync());

        // Changed behind the service's back (straight through the repository, not a Users command).
        var user = (await service.Repository.Users.GetByUserNameAsync("cached")).Entity;
        user.RoleDefinition = "Admin";
        await service.Repository.Users.UpdateAsync(user);

        Assert.AreEqual(EnumRoles.Guest, await service.GetCurrentUserRoleAsync());
        service.ResetCurrentUser();
        Assert.AreEqual(EnumRoles.Admin, await service.GetCurrentUserRoleAsync());
    }

    [TestMethod]
    public async Task RefusedCommand_IsReportedAsNotAuthorizedToCommandExecutedSubscribers()
    {
        using var db = new RepositoryTestDatabase();
        var service = EnforcedService(db, "Dapper", "guest");
        await AddUserAsync(service, "guest", "Guest");

        CommandExecutedEventArgs executed = null!;
        service.CommandExecuted += (s, e) => executed = e;

        await service.Folders.SaveAsync(new FolderDto { Name = "Refused" });

        Assert.AreEqual(CommandOutcome.NotAuthorized, executed.Outcome);
    }

    [TestMethod]
    public async Task CommandWithoutAnAuthorizationAttribute_IsRefused()
    {
        using var db = new RepositoryTestDatabase();
        var service = EnforcedService(db, "Dapper", "admin");
        await AddUserAsync(service, "admin", "Admin");

        var res = await new UndeclaredCommand(service).ValidateAuthorizationAsync();

        Assert.IsFalse(res.IsValid);
        StringAssert.Contains(res.ErrorMessage, nameof(UndeclaredCommand));
    }

    private class UndeclaredCommand(IKntService service) : KntCommandServiceBase<Result>(service)
    {
        public override Task<Result> Execute() => Task.FromResult(new Result());
    }

    private static KntService EnforcedService(RepositoryTestDatabase db, string orm, string userName)
        => new(db.CreateRepository(orm)) { UserIdentityName = userName, EnforceAuthorization = true };

    // Straight through the repository: commands would need an Admin to create users.
    private static async Task AddUserAsync(KntService service, string userName, string roleDefinition, bool disabled = false)
    {
        var res = await service.Repository.Users.AddAsync(new UserDto
        {
            UserId = Guid.NewGuid(),
            UserName = userName,
            EMail = $"{userName}-{Guid.NewGuid():N}@knote.tests",
            FullName = userName,
            RoleDefinition = roleDefinition,
            Disabled = disabled
        });
        Assert.IsTrue(res.IsValid, res.ErrorMessage);
    }
}
