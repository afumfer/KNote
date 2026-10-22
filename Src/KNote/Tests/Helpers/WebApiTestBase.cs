using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Tests.WebApiIntegrationTests;

namespace KNote.Tests.Helpers;

public class WebApiTestBase
{
    // One sign in for the whole run, shared by every test: the Server allows few sign ins a minute per address
    // (RateLimits:SignInPerMinute, KntRateLimits), and signing in for each test went over it (429).
    private static readonly Lazy<Task<TestSession>> session = new(SignInAsync);

    protected HttpClient HttpClient { get; }
    protected string UrlBase { get; }
    protected IConfiguration Configuration { get; }

    public WebApiTestBase()
    {
        var testSession = session.Value.GetAwaiter().GetResult();
        Configuration = testSession.Configuration;
        UrlBase = testSession.UrlBase;

        // A sign in that failed says why (a wrong password in the user secrets, the Server not running, 429...),
        // instead of every request failing later with a result not valid.
        if (testSession.Token == null)
            throw new InvalidOperationException($"The tests user could not sign in at {UrlBase}: {testSession.Error}");

        HttpClient = new HttpClient();
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", testSession.Token);
    }

    private static async Task<TestSession> SignInAsync()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddUserSecrets<FoldersTests>()
            .Build();
        var urlBase = configuration["testsWebApiUrlBase"] ?? "";

        try
        {
            using var httpClient = new HttpClient();
            var credentials = new UserCredentialsDto { UserName = configuration["testsUserName"], Password = configuration["testsUserPwd"] };
            var httpRes = await httpClient.PostAsJsonAsync($"{urlBase}api/users/login", credentials);
            var userToken = await httpRes.Content.ReadFromJsonAsync<UserTokenDto>();
            return userToken?.success == true
                ? new TestSession(configuration, urlBase, userToken.token, null)
                : new TestSession(configuration, urlBase, null, $"{(int)httpRes.StatusCode} {httpRes.StatusCode}: {userToken?.error}");
        }
        catch (Exception ex)
        {
            return new TestSession(configuration, urlBase, null, ex.Message);
        }
    }

    private sealed record TestSession(IConfiguration Configuration, string UrlBase, string? Token, string? Error);

    /// <summary>
    /// Deletes the items of a list with one of the names given. The data a test creates has fixed names: a run that
    /// failed half way left it in the database, and the test could not create it again in the next runs. The tests
    /// that create it call this before (TestInitialize) and after (TestCleanup, also when they fail).
    /// </summary>
    protected async Task DeleteNamedAsync<T>(string listUrl, string deleteUrl, Func<T, string?> nameOf, Func<T, Guid> idOf,
        params string[] names)
    {
        var res = await ProcessResultFromTestHttpResponse<List<T>>(await HttpClient.GetAsync($"{UrlBase}{listUrl}"));
        foreach (var item in res.Entity?.Where(item => names.Contains(nameOf(item))) ?? [])
            await HttpClient.DeleteAsync($"{UrlBase}{deleteUrl}/{idOf(item)}");
    }

    protected async Task<Result<T>> ProcessResultFromTestHttpResponse<T>(HttpResponseMessage httpRes)
    {
        Result<T>? res;

        if (httpRes.IsSuccessStatusCode)
        {
            res = await httpRes.Content.ReadFromJsonAsync<Result<T>>();
            if (res == null)
            {
                res = new Result<T>();
                res.AddErrorMessage($"Error. The web server has responded with the following message: StatusCode - {httpRes.StatusCode}. Reason Phrase - {httpRes.ReasonPhrase}");
            }
        }
        else
        {
            res = new Result<T>();
            res.AddErrorMessage($"Error. The web server has responded with the following message: StatusCode - {httpRes.StatusCode}. Reason Phrase - {httpRes.ReasonPhrase}");
        }

        return res;
    }
}
