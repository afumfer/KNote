using KNote.Model.Dto;
using KNote.Tests.Helpers;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace KNote.Tests.WebApiIntegrationTests;


[TestClass]
public class FoldersTests : WebApiTestBase
{
    private readonly HttpClient _httpClient;
    private readonly string _urlBase;
    private IConfiguration Configuration { get; }

    public FoldersTests() : base()
    {
        var configurationBuilder = new ConfigurationBuilder()
                               .SetBasePath(Directory.GetCurrentDirectory())
                               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                               .AddUserSecrets<FoldersTests>();

        Configuration = configurationBuilder.Build();

        _httpClient = new HttpClient();

        string? testsUserName = Configuration["testsUserName"];
        string? testsUserPwd = Configuration["testsUserPwd"];
        _urlBase = Configuration["testsWebApiUrlBase"] ?? "";

        var jwt = Task.Run(() => GetAuthenticationJwtAsync(testsUserName, testsUserPwd)).Result;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", jwt);
    }

    #region Tests 

    [TestMethod]
    public async Task Get_ReturnAllFolders()
    {
        var httpRes = await _httpClient.GetAsync($"{_urlBase}api/folders");

        var result = httpRes.Content;

        Assert.IsNotNull(result);
    }

    #endregion 

    #region Private methods 

    private async Task<string?> GetAuthenticationJwtAsync(string? userName, string? password)
    {
        UserCredentialsDto user = new UserCredentialsDto { UserName = userName, Password = password };
        UserTokenDto? userTokenDto;

        var httpRes = await _httpClient.PostAsJsonAsync($"{_urlBase}api/users/login", user);
        userTokenDto = await httpRes.Content.ReadFromJsonAsync<UserTokenDto>();
        return userTokenDto?.token ?? null;
    }

    #endregion 

}