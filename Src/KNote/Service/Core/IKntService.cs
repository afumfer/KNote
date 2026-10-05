using System;
using System.Threading.Tasks;
using KNote.MessageBroker;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Repository;
using KNote.Service.Interfaces;
using Microsoft.Extensions.Logging;

namespace KNote.Service.Core;

public interface IKntService : IDisposable
{
    ILogger Logger { get; set; }
    Guid IdServiceRef { get; }
    RepositoryRef RepositoryRef { get; }
    Task<bool> TestDbConnection();
    Task<bool> CreateDataBase();
    string UserIdentityName { get; set; }

    /// <summary>
    /// When true, every command checks the role of UserIdentityName in this repository against the
    /// role its class declares with [KntAuthorize] (see KntCommandServiceBase.ValidateAuthorizationAsync).
    /// Off by default: ServiceRef (ClientWin) turns it on. Server leaves it off and keeps relying on its
    /// controllers' [Authorize], as do tests that exercise the service without a user.
    /// </summary>
    bool EnforceAuthorization { get; set; }

    /// <summary>
    /// UserIdentityName's row in this repository's Users table (disabled or not), or null when that user
    /// isn't registered there. Read once and cached; ResetCurrentUser forgets it. Reads the repository
    /// directly, so it works before the user is registered or authorized for anything.
    /// </summary>
    Task<UserDto> GetCurrentUserAsync();

    /// <summary>
    /// The highest role of UserIdentityName in this repository, or null when that user isn't registered
    /// there or is disabled (see GetCurrentUserAsync).
    /// </summary>
    Task<EnumRoles?> GetCurrentUserRoleAsync();

    /// <summary>
    /// Forgets the cached current user, so the next GetCurrentUserAsync/GetCurrentUserRoleAsync reads it
    /// again. Called by the commands that change users (their roles or whether they are disabled).
    /// </summary>
    void ResetCurrentUser();

    IKntRepository Repository { get; }

    IKntUserService Users { get; }
    IKntKAttributeService KAttributes { get; }
    IKntSystemValuesService SystemValues { get; }
    IKntFolderService Folders { get; }
    IKntNoteService Notes { get; }
    IKntNoteTypeService NoteTypes { get; }
    IKntTraceNoteTypeService TraceNoteTypes { get; }
    IKntMessageBroker MessageBroker { get; }

    string GetSystemVariable(string scope, string variable);
    void SaveSystemVariable(string scope, string key, string value);
    void PublishNoteInMessageBroker(NoteExtendedDto noteInfo);
    string ReplaceSpecialCharacters(string text);

    /// <summary>
    /// Raised by KntServiceBase.ExecuteCommand around every command run through this IKntService
    /// (~80 concrete commands across Users/KAttributes/SystemValues/Folders/Notes/NoteTypes/
    /// TraceNoteTypes) - CommandExecuting fires once before, CommandExecuted exactly once after
    /// (success, validation failure, authorization failure, or exception), correlated by
    /// CommandEventArgsBase.ExecutionId. A subscriber's exception here is caught and logged by the
    /// implementation - it can never abort the command itself.
    /// </summary>
    event EventHandler<CommandExecutingEventArgs> CommandExecuting;
    event EventHandler<CommandExecutedEventArgs> CommandExecuted;

    /// <summary>
    /// Raises CommandExecuting/CommandExecuted. Called by KntServiceBase.ExecuteCommand - not meant to
    /// be called from anywhere else.
    /// </summary>
    void NotifyCommandExecuting(CommandExecutingEventArgs e);
    void NotifyCommandExecuted(CommandExecutedEventArgs e);
}
