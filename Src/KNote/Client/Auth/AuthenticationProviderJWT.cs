﻿using System.Net.Http.Headers;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using KNote.Client.Helpers;
using KNote.Client.AppStoreService;

namespace KNote.Client.Auth;

public class AuthenticationProviderJWT : AuthenticationStateProvider, ILoginService
{
    public static readonly string TOKENKEY = "TOKENKEY";
    private readonly IJSRuntime _js;
    private readonly HttpClient _httpClient;
    private readonly IStore _store;

    private AuthenticationState Anonymous =>
        new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

    public AuthenticationProviderJWT(IJSRuntime js, HttpClient httpClient, IStore store)
    {
        _js = js;
        _httpClient = httpClient;
        _store = store;
    }

    public async override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _js.LocalStorageGetItem(TOKENKEY);

        if (string.IsNullOrEmpty(token))            
            return Anonymous;

        return BuildAuthenticationState(token);
    }

    public AuthenticationState BuildAuthenticationState(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);       
        var authState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt")));        
        _store.AppState.UserName = authState.User.Identity.Name;
        return authState;
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        keyValuePairs.TryGetValue(ClaimTypes.Role, out object roles);

        if (roles != null)
        {
            if (roles.ToString().Trim().StartsWith("["))
            {
                var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString());

                foreach (var parsedRole in parsedRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, parsedRole));
                }
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, roles.ToString()));
            }

            keyValuePairs.Remove(ClaimTypes.Role);
        }

        claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString())));
        return claims;
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }

    public async Task Login(string token)
    {
        await _js.LocalStorageSetItem(TOKENKEY, token);
        var authState = BuildAuthenticationState(token);
        NotifyAuthenticationStateChanged(Task.FromResult(authState));
    }

    public async Task Logout()
    {
        await _js.LocalStorageRemoveItem(TOKENKEY);
        _httpClient.DefaultRequestHeaders.Authorization = null;
        _store.AppState.UserName = null;
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }
}

