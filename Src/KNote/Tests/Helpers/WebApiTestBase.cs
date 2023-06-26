using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Tests.WebApiIntegrationTests;

namespace KNote.Tests.Helpers;

public class WebApiTestBase
{
    protected HttpClient HttpClient { get; }
    protected string UrlBase { get; }
    protected IConfiguration Configuration { get; }

    public WebApiTestBase()
    {
        var configurationBuilder = new ConfigurationBuilder()
                               .SetBasePath(Directory.GetCurrentDirectory())
                               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                               .AddUserSecrets<FoldersTests>();

        Configuration = configurationBuilder.Build();

        HttpClient = new HttpClient();

        string? testsUserName = Configuration["testsUserName"];
        string? testsUserPwd = Configuration["testsUserPwd"];
        UrlBase = Configuration["testsWebApiUrlBase"] ?? "";

        var jwt = Task.Run(() => GetAuthenticationJwtAsync(testsUserName, testsUserPwd)).Result;
        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
    }

    
    protected async Task<string?> GetAuthenticationJwtAsync(string? userName, string? password)
    {
        UserCredentialsDto user = new UserCredentialsDto { UserName = userName, Password = password };
        UserTokenDto? userTokenDto;

        var httpRes = await HttpClient.PostAsJsonAsync($"{UrlBase}api/users/login", user);
        userTokenDto = await httpRes.Content.ReadFromJsonAsync<UserTokenDto>();
        return userTokenDto?.token ?? null;
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
