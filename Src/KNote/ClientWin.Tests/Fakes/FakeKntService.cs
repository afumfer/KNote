using KNote.MessageBroker;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository;
using KNote.Service.Core;
using KNote.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>
/// Minimal IKntService test double exposing only a FakeKntNoteService via Notes; every other
/// member throws NotSupportedException, so a test that unexpectedly reaches one fails loudly.
/// </summary>
internal class FakeKntService : IKntService
{
    public FakeKntNoteService NotesFake { get; } = new();
    public FakeKntUserService UsersFake { get; } = new();
    public FakeKntNoteTypeService NoteTypesFake { get; } = new();
    public FakeKntTraceNoteTypeService TraceNoteTypesFake { get; } = new();
    public FakeKntKAttributeService KAttributesFake { get; } = new();
    public FakeKntFolderService FoldersFake { get; } = new();

    public ILogger Logger { get; set; }
    public Guid IdServiceRef { get; } = Guid.NewGuid();
    public RepositoryRef RepositoryRef { get; set; } = new RepositoryRef { Alias = "Fake repository" };
    public string UserIdentityName { get; set; }
    public bool EnforceAuthorization { get; set; }

    // What GetCurrentUserAsync returns (null: the user isn't registered in this repository); the role
    // comes from it, as in KntService. An Admin by default, so tests unrelated to authorization run as before.
    public UserDto CurrentUser { get; set; } = new UserDto { UserId = Guid.NewGuid(), UserName = "admin", RoleDefinition = "Admin" };
    public Task<UserDto> GetCurrentUserAsync() => Task.FromResult(CurrentUser);
    public Task<EnumRoles?> GetCurrentUserRoleAsync() =>
        Task.FromResult(CurrentUser == null || CurrentUser.Disabled ? null : KntRoles.Highest(CurrentUser.RoleDefinition));
    public void ResetCurrentUser() { }

    public IKntRepository Repository => throw new NotSupportedException();
    public IKntUserService Users => UsersFake;
    public IKntKAttributeService KAttributes => KAttributesFake;
    public IKntSystemValuesService SystemValues => throw new NotSupportedException();
    public IKntFolderService Folders => FoldersFake;
    public IKntNoteService Notes => NotesFake;
    public IKntNoteTypeService NoteTypes => NoteTypesFake;
    public IKntTraceNoteTypeService TraceNoteTypes => TraceNoteTypesFake;
    public IKntAiSessionService AiSessions => throw new NotSupportedException();
    public IKntMessageBroker MessageBroker => throw new NotSupportedException();

    public Task<bool> TestDbConnection() => throw new NotSupportedException();
    public Task<bool> CreateDataBase() => throw new NotSupportedException();
    public string GetSystemVariable(string scope, string variable) => throw new NotSupportedException();
    public void SaveSystemVariable(string scope, string key, string value) => throw new NotSupportedException();
    public void PublishNoteInMessageBroker(NoteExtendedDto noteInfo) => throw new NotSupportedException();
    public string ReplaceSpecialCharacters(string text) => throw new NotSupportedException();

    // No test exercises these today - trivial no-op implementations rather than throw, so nothing
    // that merely constructs/disposes a FakeKntService starts failing.
    public event EventHandler<CommandExecutingEventArgs> CommandExecuting;
    public event EventHandler<CommandExecutedEventArgs> CommandExecuted;
    public void NotifyCommandExecuting(CommandExecutingEventArgs e) => CommandExecuting?.Invoke(this, e);
    public void NotifyCommandExecuted(CommandExecutedEventArgs e) => CommandExecuted?.Invoke(this, e);

    public void Dispose() { }
}
