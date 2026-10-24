namespace KNote.Client.Auth;

public interface ILoginService
{
    Task Login(string token);
    Task Logout();

    // The server refused the session (BaseService → AppState.OnSessionEnded). True if another window of the app had
    // already signed in again in the same repository (a new token, after changing the password) and this one now uses
    // it; false if the user has been signed out of this window. Null if it was already being handled (several
    // requests refused at the same time).
    Task<bool?> SessionRefusedAsync();

    // The repository of this window's session (its token's knt_repo), or null without one.
    Guid? RepositoryId { get; }

    // The session signed in last in another window of the app (the token in localStorage): its repository, or null.
    Task<Guid?> GetLastSessionRepositoryIdAsync();

    // Takes the session signed in last in another window of the app (or, if there is none, signs out of this one).
    Task AdoptLastSessionAsync();

    // Signs out of this window only (another one signed out, or this one's session was refused).
    Task SignOutHereAsync();
}
