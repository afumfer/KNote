namespace KNote.Client.Auth;

public interface ILoginService
{
    Task Login(string token);
    Task Logout();

    // The server refused the session (BaseService → AppState.OnSessionEnded). True if another window of the app had
    // already signed in again (a new token, after changing the password) and this one now uses it; false if the
    // user has been signed out. Null if it was already being handled (several requests refused at the same time).
    Task<bool?> SessionRefusedAsync();
}

