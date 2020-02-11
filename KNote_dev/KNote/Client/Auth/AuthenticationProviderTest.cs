using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KNote.Client.Auth
{
    public class AuthenticationProviderTest : AuthenticationStateProvider
    {
        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            //await Task.Delay(5000);

            //var anonimo = new ClaimsIdentity();

            var anonimo = new ClaimsIdentity(new List<Claim>() {
                new Claim("llave1", "valor1")
                ,new Claim(ClaimTypes.Name, "Armando")
                ,new Claim(ClaimTypes.Role, "Admin")
                ,new Claim(ClaimTypes.Role, "ProjecManager")
                ,new Claim(ClaimTypes.Role, "Staff")
            }, "test");

            return await Task.FromResult(new AuthenticationState(new ClaimsPrincipal(anonimo)));


        }
    }
}
