using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using KNote.Model.Dto;

namespace KNote.Tests.Helpers;

/// <summary>
/// Shared bootstrap for the in-process (WebApplicationFactory-based) test suite: builds a
/// KNoteWebApplicationFactory, warms it up, registers a throwaway Admin user and returns an
/// HttpClient already carrying its bearer token. Each test class calls this once from its own
/// [ClassInitialize] to get its own isolated factory/database - see KNoteWebApplicationFactory's
/// remarks for why only one of these should be built at a time (hence [assembly: DoNotParallelize]
/// in Usings.cs).
/// </summary>
public static class InProcessTestHost
{
    public static async Task<(KNoteWebApplicationFactory Factory, HttpClient Client)> CreateAuthenticatedClientAsync()
    {
        var factory = new KNoteWebApplicationFactory();
        var client = factory.CreateClient();

        // Warm-up request: the first request against a freshly built WebApplicationFactory host can
        // race the (lazily built) endpoint routing table and spuriously 404/405. Discard the result.
        await client.GetAsync("api/users/register");

        var registerDto = new UserRegisterDto
        {
            UserName = $"itest-{Guid.NewGuid():N}"[..24],
            EMail = $"{Guid.NewGuid():N}@knote.tests",
            FullName = "In-Process Test User",
            // Admin covers every role check across the controllers this suite exercises
            // (Folders/KAttributes/NoteTypes/Users require "Admin"; Notes accepts
            // "Admin, Staff, ProjecManager").
            RoleDefinition = "Admin",
            Password = "InProcess-Test-Password-1!"
        };

        var registerResponse = await client.PostAsJsonAsync("api/users/register", registerDto);
        registerResponse.EnsureSuccessStatusCode();

        var token = await registerResponse.Content.ReadFromJsonAsync<UserTokenDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token!.token);

        return (factory, client);
    }
}
