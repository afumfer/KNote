using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace KNote.Client.Auth;

public class AuthenticationProviderTest : AuthenticationStateProvider
{
    public async override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        //await Task.Delay(5000);

        //var anonimous = new ClaimsIdentity();

        var anonimous = new ClaimsIdentity(new List<Claim>() {
            new Claim("llave1", "valor1")
            ,new Claim(ClaimTypes.Name, "Armando")
            ,new Claim(ClaimTypes.Role, "Admin")
            ,new Claim(ClaimTypes.Role, "ProjecManager")
            ,new Claim(ClaimTypes.Role, "Staff")
        }, "test");

        return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(anonimous)));
    }
}

