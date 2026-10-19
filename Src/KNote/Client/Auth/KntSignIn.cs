using KNote.Client.AppStoreService;
using KNote.Client.Pages.Auth;
using KNote.Client.Shared;
using KNote.Model;
using KNote.Model.Dto;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace KNote.Client.Auth;

/// <summary>
/// Signing in and registering: the dialog, the request and the session. Used by the header (Pages/Auth/AdminLogin,
/// which then goes to Home) and by a page that needs a signed in user (Shared/KntAccessDenied, which stays on that
/// page: once the session changes, the router shows it). One at a time: a click repeated opens one dialog.
/// </summary>
public class KntSignIn
{
    private readonly IStore store;
    private readonly ILoginService loginService;
    private readonly DialogService dialogService;
    private readonly NotificationService notificationService;
    private readonly NavigationManager navigationManager;
    private readonly KntSingleFlight signing = new();

    public KntSignIn(IStore store, ILoginService loginService, DialogService dialogService,
        NotificationService notificationService, NavigationManager navigationManager)
    {
        this.store = store;
        this.loginService = loginService;
        this.dialogService = dialogService;
        this.notificationService = notificationService;
        this.navigationManager = navigationManager;
    }

    public bool IsBusy => signing.IsBusy;

    /// <summary>Asks for the credentials and signs in; then goes to Home if goHome.</summary>
    public Task SignInAsync(bool goHome) => signing.RunAsync(() => SignInCoreAsync(goHome));

    /// <summary>Asks for the new user's data, registers them and signs in; then goes to Home if goHome.</summary>
    public Task RegisterAsync(bool goHome) => signing.RunAsync(() => RegisterCoreAsync(goHome));

    private async Task SignInCoreAsync(bool goHome)
    {
        UserCredentialsDto? credentials = await dialogService.OpenAsync<Login>($"{KntConst.AppName} login",
            new Dictionary<string, object?>(), KntDialogs.Framed(KntDialogs.Small()));
        if (credentials == null)
            return;

        await StartSessionAsync(await store.Users.LoginAsync(credentials), goHome);
    }

    private async Task RegisterCoreAsync(bool goHome)
    {
        UserRegisterDto? newUser = await dialogService.OpenAsync<Register>("Register",
            new Dictionary<string, object?>(), KntDialogs.Framed(KntDialogs.Medium()));
        if (newUser == null)
            return;

        // The new user's role is decided by the server (see KntUsersRegisterAsyncCommand).
        await StartSessionAsync(await store.Users.RegisterAsync(newUser), goHome);
    }

    private async Task StartSessionAsync(UserTokenDto? result, bool goHome)
    {
        if (result == null || !result.success)
        {
            await dialogService.Alert(result?.error ?? "The user could not be signed in.", "Error", new AlertOptions { OkButtonText = "OK" });
            return;
        }

        notificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Success, Summary = "Login user.", Detail = "Authenticated use.", Duration = 3000 });
        await loginService.Login(result.token);
        if (goHome)
            navigationManager.NavigateTo("");
    }
}
