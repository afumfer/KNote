using System.Net.Http.Headers;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using KNote.Client.Helpers;
using KNote.Client.AppStoreService;
using System.IdentityModel.Tokens.Jwt;

namespace KNote.Client.Auth;

public class AuthenticationProviderJWT : AuthenticationStateProvider, ILoginService
{
    public static readonly string TOKENKEY = "TOKENKEY";
    private readonly IJSRuntime js;
    private readonly HttpClient httpClient;
    private readonly IStore store;

    private AuthenticationState Anonymous =>
        new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

    public AuthenticationProviderJWT(IJSRuntime js, HttpClient httpClient, IStore store)
    {
        this.js = js;
        this.httpClient = httpClient;
        this.store = store;
    }

    public async override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await js.LocalStorageGetItem(TOKENKEY);

        if (string.IsNullOrEmpty(token))            
            return Anonymous;

        return BuildAuthenticationState(token);
    }

    public AuthenticationState BuildAuthenticationState(string token)
    {        
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);       
        var authState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt")));        
        store.AppState.UserName = authState?.User?.Identity?.Name;
        return authState!;
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string token)
    {
        var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        var tokenDeserialized = jwtSecurityTokenHandler.ReadJwtToken(token);
        return tokenDeserialized.Claims;
    }

    public async Task Login(string token)
    {
        await js.LocalStorageSetItem(TOKENKEY, token);
        var authState = BuildAuthenticationState(token);
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
    }

    public async Task Logout()
    {
        await js.LocalStorageRemoveItem(TOKENKEY);
        httpClient.DefaultRequestHeaders.Authorization = null;
        store.AppState.UserName = null;
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }
}

