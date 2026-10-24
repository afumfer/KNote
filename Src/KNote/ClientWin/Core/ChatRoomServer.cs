using System.Net;
using System.Net.Http.Json;
using KNote.Model;
using KNote.Model.Dto;
using Microsoft.AspNetCore.SignalR.Client;

namespace KNote.ClientWin.Core;

/// <summary>
/// Signing in to the chat room's Server (ChatRoomServer.SignInAsync): the token and its repository's name or, when the
/// credentials are valid in several repositories of the Server, those repositories (no token yet).
/// </summary>
public sealed class ChatRoomSignIn
{
    public string Token { get; init; }

    public string RepositoryName { get; init; }

    public List<RepositoryInfoDto> Repositories { get; init; }
}

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

    /// <summary>
    /// The base of the Server's application, where its hub is (e.g. https://server/KNote/, ending in '/'): null if
    /// hubUrl is not an absolute url ending in /chathub. Its api and the urls of its resources are relative to it.
    /// </summary>
    public static Uri BaseUrl(string hubUrl)
    {
        if (!Uri.TryCreate(hubUrl?.Trim(), UriKind.Absolute, out var hub)
            || (hub.Scheme != Uri.UriSchemeHttp && hub.Scheme != Uri.UriSchemeHttps))
            return null;

        var path = hub.AbsolutePath.TrimEnd('/');
        if (!path.EndsWith(ChatRoomHubContract.Path, StringComparison.OrdinalIgnoreCase))
            return null;

        var basePath = path[..^ChatRoomHubContract.Path.Length];
        return new UriBuilder(hub) { Path = $"{basePath}/", Query = "", Fragment = "" }.Uri;
    }

    /// <summary>The Server's sign-in url, next to its hub: null if hubUrl is not an absolute url ending in /chathub.</summary>
    public static Uri LoginUrl(string hubUrl)
    {
        var baseUrl = BaseUrl(hubUrl);
        return baseUrl == null ? null : new Uri(baseUrl, "api/users/login");
    }

    /// <summary>
    /// Sends an image to a thread (ChatRoomHubContract.ImagesPath), kept by the Server as a resource of the thread's
    /// note. The image as kept (its Url and the Markdown to send it in a message), or the reason it was refused.
    /// </summary>
    public static async Task<Result<ChatRoomImageDto>> UploadImageAsync(string hubUrl, string token, ChatRoomImageDto image)
    {
        var result = new Result<ChatRoomImageDto>();
        var baseUrl = BaseUrl(hubUrl);
        if (baseUrl == null)
        {
            result.AddErrorMessage($"The chat hub url must be the address of a KNote Server ending in {ChatRoomHubContract.Path}.");
            return result;
        }

        try
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(2) };
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token);
            var response = await httpClient.PostAsJsonAsync(new Uri(baseUrl, ChatRoomHubContract.ImagesPath(image.ThreadId)), image);

            // The Server answers with a Result, also when it refuses the image; not when it refuses the request (401/403).
            Result<ChatRoomImageDto> answer = null;
            try
            {
                answer = await response.Content.ReadFromJsonAsync<Result<ChatRoomImageDto>>();
            }
            catch (Exception)
            {
            }

            if (answer != null)
                return answer;
            result.AddErrorMessage(response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden
                ? "The chat room server refused your sign in (it expired, or your user is not Staff there). Sign in again from Options."
                : $"The server refused the image ({(int)response.StatusCode} {response.ReasonPhrase}).");
        }
        catch (Exception ex)
        {
            result.AddErrorMessage($"The image could not be sent: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Signs in to the Server of the hub, in the repository given (repositoryId) or, without it, in the only one where
    /// the credentials are valid. A Server can have several repositories, each one with its chat room: when the
    /// credentials are valid in several, the answer is them (ChatRoomSignIn.Repositories, without a token), to sign in
    /// again with the one chosen. The token and its repository's name, or the reason it was refused.
    /// </summary>
    public static Task<Result<ChatRoomSignIn>> SignInAsync(string hubUrl, string userName, string password, Guid? repositoryId = null) =>
        SignInAsync(new HttpClientHandler(), hubUrl, userName, password, repositoryId);

    // handler: how the requests are sent (the tests use a fake one).
    internal static async Task<Result<ChatRoomSignIn>> SignInAsync(HttpMessageHandler handler, string hubUrl, string userName, string password, Guid? repositoryId = null)
    {
        var result = new Result<ChatRoomSignIn>();

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
            using var httpClient = new HttpClient(handler) { Timeout = ConnectTimeout };
            var response = await httpClient.PostAsJsonAsync(loginUrl, new UserCredentialsDto { UserName = userName.Trim(), Password = password, RepositoryId = repositoryId });
            var token = await ReadTokenAsync(response);
            if (token?.success == true && !string.IsNullOrEmpty(token.token))
                result.Entity = new ChatRoomSignIn { Token = token.token, RepositoryName = await ReadRepositoryNameAsync(httpClient, BaseUrl(hubUrl), token.token) };
            else if (token?.Repositories is { Count: > 1 } repositories)
                result.Entity = new ChatRoomSignIn { Repositories = repositories };
            else
                result.AddErrorMessage(string.IsNullOrEmpty(token?.error) ? $"The server refused the sign in ({(int)response.StatusCode} {response.ReasonPhrase})." : token.error);
        }
        catch (Exception ex)
        {
            result.AddErrorMessage($"The server could not be reached: {ex.Message}");
        }

        return result;
    }

    // The name of the token's repository (null if the Server doesn't say it).
    private static async Task<string> ReadRepositoryNameAsync(HttpClient httpClient, Uri baseUrl, string token)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(baseUrl, "api/systemvalues/repositoryname"));
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token);
            var response = await httpClient.SendAsync(request);
            var name = await response.Content.ReadFromJsonAsync<Result<string>>();
            return name?.IsValid == true ? name.Entity : null;
        }
        catch (Exception)
        {
            return null;
        }
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
