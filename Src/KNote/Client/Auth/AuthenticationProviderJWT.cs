using System.Net.Http.Headers;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using KNote.Client.Helpers;
using KNote.Client.AppStoreService;
using KNote.Model;
using System.IdentityModel.Tokens.Jwt;

namespace KNote.Client.Auth;

/// <summary>
/// The session of this window of the app (every window is another instance of the app): a token, of one repository of
/// the server (its knt_repo claim). The token is kept in sessionStorage, so the window keeps its session, and its
/// repository, when it is reloaded; localStorage has the session signed in last, which a new window (or tab) starts with.
/// So a window of a note stays in the note's repository when the session changes to another one in the main window
/// (KntWindowLayout tells the user), and never takes a token of another repository.
/// </summary>
public class AuthenticationProviderJWT : AuthenticationStateProvider, ILoginService
{
    public static readonly string TOKENKEY = "TOKENKEY";
    public static readonly string SESSIONTOKENKEY = "KNoteSessionToken";
    private readonly IJSRuntime js;
    private readonly HttpClient httpClient;
    private readonly IStore store;
    private readonly KntWindows kntWindows;

    private AuthenticationState Anonymous =>
        new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

    public AuthenticationProviderJWT(IJSRuntime js, HttpClient httpClient, IStore store, KntWindows kntWindows)
    {
        this.js = js;
        this.httpClient = httpClient;
        this.store = store;
        this.kntWindows = kntWindows;
    }

    public Guid? RepositoryId => RepositoryIdOf(httpClient.DefaultRequestHeaders.Authorization?.Parameter);

    public async override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await js.SessionStorageGetItem(SESSIONTOKENKEY);
        if (string.IsNullOrEmpty(token))
        {
            token = await js.LocalStorageGetItem(TOKENKEY);
            if (string.IsNullOrEmpty(token))
                return Anonymous;
            await js.SessionStorageSetItem(SESSIONTOKENKEY, token);
        }

        return BuildAuthenticationState(token);
    }

    public AuthenticationState BuildAuthenticationState(string token)
    {
        // A session in another repository: what was read with the previous one doesn't belong to it.
        if (RepositoryId is Guid previous && previous != RepositoryIdOf(token))
            store.AppState.ClearUserData();

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
        var authState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt")));
        store.AppState.UserName = authState?.User?.Identity?.Name;
        return authState!;
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string token)
    {
        var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        var tokenDeserialized = jwtSecurityTokenHandler.ReadJwtToken(token);
        return tokenDeserialized.Claims;
    }

    public static Guid? RepositoryIdOf(string? token)
    {
        if (string.IsNullOrEmpty(token))
            return null;
        try
        {
            var claim = ParseClaimsFromJwt(token).FirstOrDefault(c => c.Type == KntConst.RepositoryClaimType)?.Value;
            return Guid.TryParse(claim, out var repositoryId) ? repositoryId : null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    // Signs in this window, and tells the other windows (they follow it, or keep their repository, see the class).
    public async Task Login(string token)
    {
        await js.LocalStorageSetItem(TOKENKEY, token);
        await js.SessionStorageSetItem(SESSIONTOKENKEY, token);
        NotifyAuthenticationStateChanged(Task.FromResult(BuildAuthenticationState(token)));
        await kntWindows.NotifySessionChangedAsync(RepositoryIdOf(token));
    }

    public async Task Logout()
    {
        await js.LocalStorageRemoveItem(TOKENKEY);
        await SignOutHereAsync();
        await kntWindows.NotifySessionChangedAsync(null);
    }

    public async Task SignOutHereAsync()
    {
        await js.SessionStorageRemoveItem(SESSIONTOKENKEY);
        httpClient.DefaultRequestHeaders.Authorization = null;
        store.AppState.UserName = null;
        store.AppState.ClearUserData();
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }

    public async Task<Guid?> GetLastSessionRepositoryIdAsync() => RepositoryIdOf(await js.LocalStorageGetItem(TOKENKEY));

    public async Task AdoptLastSessionAsync()
    {
        var stored = await js.LocalStorageGetItem(TOKENKEY);
        if (string.IsNullOrEmpty(stored))
        {
            await SignOutHereAsync();
            return;
        }
        if (stored == httpClient.DefaultRequestHeaders.Authorization?.Parameter)
            return;

        await js.SessionStorageSetItem(SESSIONTOKENKEY, stored);
        NotifyAuthenticationStateChanged(Task.FromResult(BuildAuthenticationState(stored)));
    }

    private bool handlingRefusedSession;

    // Every window of the app is another instance with its own HttpClient: one that changed the password stored the
    // new token, and the others still send the old one. Taking it, instead of signing out, keeps the session that the
    // other window has just renewed; only one of the same repository (a window of another repository's note must never
    // write with it).
    public async Task<bool?> SessionRefusedAsync()
    {
        if (handlingRefusedSession)
            return null;

        handlingRefusedSession = true;
        try
        {
            var current = httpClient.DefaultRequestHeaders.Authorization?.Parameter;
            var stored = await js.LocalStorageGetItem(TOKENKEY);
            if (!string.IsNullOrEmpty(stored) && stored != current && RepositoryIdOf(stored) == RepositoryIdOf(current))
            {
                await js.SessionStorageSetItem(SESSIONTOKENKEY, stored);
                NotifyAuthenticationStateChanged(Task.FromResult(BuildAuthenticationState(stored)));
                return true;
            }

            // The session signed in last is this one, refused too: no window can start with it.
            if (stored == current)
                await js.LocalStorageRemoveItem(TOKENKEY);
            await SignOutHereAsync();
            return false;
        }
        finally
        {
            handlingRefusedSession = false;
        }
    }
}

