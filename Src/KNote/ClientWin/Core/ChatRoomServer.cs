using System.Net;
using System.Net.Http.Json;
using KNote.Model;
using KNote.Model.Dto;
using Microsoft.AspNetCore.SignalR.Client;

namespace KNote.ClientWin.Core;

/// <summary>
/// The KNote Server of the chat room, as ClientWin reaches it: the hub's url (ChatHubConfig.Url, ending in
/// ChatRoomHubContract.Path, e.g. https://server/KNote/chathub), the sign-in that gives the token the hub asks for
/// (api/users/login of the same Server), and a connection test. Used by Options, where the user signs in once.
/// </summary>
public static class ChatRoomServer
{
    // Bounds how long a connection attempt can block the caller before giving up: an unreachable server must never
    // hang the app.
    public static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(8);

    /// <summary>The Server's sign-in url, next to its hub: null if hubUrl is not an absolute url ending in /chathub.</summary>
    public static Uri LoginUrl(string hubUrl)
    {
        if (!Uri.TryCreate(hubUrl?.Trim(), UriKind.Absolute, out var hub)
            || (hub.Scheme != Uri.UriSchemeHttp && hub.Scheme != Uri.UriSchemeHttps))
            return null;

        var path = hub.AbsolutePath.TrimEnd('/');
        if (!path.EndsWith(ChatRoomHubContract.Path, StringComparison.OrdinalIgnoreCase))
            return null;

        var basePath = path[..^ChatRoomHubContract.Path.Length];
        return new UriBuilder(hub) { Path = $"{basePath}/api/users/login", Query = "" }.Uri;
    }

    /// <summary>Signs in to the Server of the hub. The token, or the reason it was refused.</summary>
    public static async Task<Result<string>> SignInAsync(string hubUrl, string userName, string password)
    {
        var result = new Result<string>();

        var loginUrl = LoginUrl(hubUrl);
        if (loginUrl == null)
        {
            result.AddErrorMessage($"The chat hub url must be the address of a KNote Server ending in {ChatRoomHubContract.Path} (e.g. https://server/KNote/chathub).");
            return result;
        }
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrEmpty(password))
        {
            result.AddErrorMessage("Enter the user name and the password of your user in the KNote Server.");
            return result;
        }

        try
        {
            using var httpClient = new HttpClient { Timeout = ConnectTimeout };
            var response = await httpClient.PostAsJsonAsync(loginUrl, new UserCredentialsDto { UserName = userName.Trim(), Password = password });
            var token = await ReadTokenAsync(response);
            if (token?.success == true && !string.IsNullOrEmpty(token.token))
                result.Entity = token.token;
            else
                result.AddErrorMessage(string.IsNullOrEmpty(token?.error) ? $"The server refused the sign in ({(int)response.StatusCode} {response.ReasonPhrase})." : token.error);
        }
        catch (Exception ex)
        {
            result.AddErrorMessage($"The server could not be reached: {ex.Message}");
        }

        return result;
    }

    /// <summary>Connects to the hub with the token, and leaves. Invalid with the reason when it fails.</summary>
    public static async Task<Result> TestConnectionAsync(string hubUrl, string token)
    {
        var result = new Result();
        HubConnection connection = null;
        try
        {
            connection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options => options.AccessTokenProvider = () => Task.FromResult(token))
                .Build();
            using var cts = new CancellationTokenSource(ConnectTimeout);
            await connection.StartAsync(cts.Token);
        }
        catch (Exception ex)
        {
            result.AddErrorMessage(ConnectionErrorMessage(ex));
        }
        finally
        {
            if (connection != null)
                await connection.DisposeAsync();
        }
        return result;
    }

    /// <summary>The reason a connection failed, telling apart the sign-in refused by the Server.</summary>
    public static string ConnectionErrorMessage(Exception ex) => IsNotAuthorized(ex)
        ? "The chat room server refused your sign in (it expired, or your user is not Staff there). Sign in again from Options."
        : ex.Message;

    public static bool IsNotAuthorized(Exception ex) =>
        ex is HttpRequestException { StatusCode: HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden };

    // The login answers with a UserTokenDto, also when it refuses the credentials.
    private static async Task<UserTokenDto> ReadTokenAsync(HttpResponseMessage response)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<UserTokenDto>();
        }
        catch (Exception)
        {
            return null;
        }
    }
}
