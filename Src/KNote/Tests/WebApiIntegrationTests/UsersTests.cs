using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using KNote.Model.Dto;
using KNote.Tests.Helpers;

namespace KNote.Tests.WebApiIntegrationTests;

[TestClass]
public class UsersTests : WebApiTestBase
{
    public UsersTests() : base()
    {
        
    }

    [TestMethod]
    public async Task Get_All()
    {
        var httpRes = await HttpClient.GetAsync($"{UrlBase}api/users");
        var res = await ProcessResultFromTestHttpResponse<List<UserDto>>(httpRes);

        Assert.IsTrue(res.IsValid);
        Assert.IsTrue(res.Entity.Count > 0);
    }

    [TestMethod]
    public async Task Get_WithPagination()
    {
        var httpRes = await HttpClient.GetAsync($"{UrlBase}api/users?pageNumber=1&pageSize=3");
        var res = await ProcessResultFromTestHttpResponse<List<UserDto>>(httpRes);

        Assert.IsTrue(res.IsValid);
        Assert.IsTrue(res.Entity.Count == 3);
    }

    [TestMethod]
    public async Task Execute_RegiterAndDelete()
    {
        // Register new user
        string userName = "__TEST_REGISTERUSER_###__";
        string userEmail = "TESTREGISTER@TESTREGISTER.ORG";
        string userFullName = "__TEST_REGISTERUSER_FULLNAME_###__";
        string userRoles = "Public";
        string userPass = "pass12345abcd!!";
        Guid userId = Guid.Empty;        
        UserRegisterDto user = new() { UserId = userId, UserName = userName, EMail = userEmail, FullName = userFullName, RoleDefinition = userRoles, Password = userPass };        
        
        var httpRes = await HttpClient.PostAsJsonAsync($"{UrlBase}api/users/register", user);        
        var res = await httpRes.Content.ReadFromJsonAsync<UserTokenDto>();
        
        Assert.IsNotNull(res);
        Assert.IsTrue(res?.success);
        Assert.IsTrue(!string.IsNullOrEmpty(res?.uid));
        Assert.IsTrue(!string.IsNullOrEmpty(res?.token));
        Assert.IsTrue(string.IsNullOrEmpty(res?.error));

        if (res is null || res.uid is null)
            return;

        userId = Guid.Parse(res.uid);

        // Delete
        httpRes = await HttpClient.DeleteAsync($"{UrlBase}api/users/{userId}");
        var resDel = await ProcessResultFromTestHttpResponse<UserDto>(httpRes);

        Assert.IsTrue(resDel.IsValid);
        Assert.IsNotNull(resDel.Entity);
        Assert.IsTrue(userId == resDel.Entity?.UserId);
    }


    [TestMethod]
    public async Task Execute_BasicCRUD()
    {
        // Create 
        string userName = "__TEST_CREATEUSER_###__";
        string userEmail = "TESTCREATE@TESTCREATE.ORG";
        string userFullName = "__TEST_CREATEUSER_FULLNAME_###__";
        string userRoles = "Public";        
        Guid userId = Guid.Empty;
        UserDto user = new() { UserId = userId, UserName = userName, EMail = userEmail, FullName = userFullName, RoleDefinition = userRoles };
        
        var httpRes = await HttpClient.PostAsJsonAsync($"{UrlBase}api/users", user);
        var res = await ProcessResultFromTestHttpResponse<UserDto>(httpRes);

        Assert.IsTrue(res.IsValid);
        Assert.IsNotNull(res.Entity);
        Assert.IsTrue(userId != res.Entity?.UserId);
        Assert.IsTrue(userName == res.Entity?.UserName);
        Assert.IsTrue(userEmail == res.Entity?.EMail);
        Assert.IsTrue(userFullName == res.Entity?.FullName);
        Assert.IsTrue(userRoles == res.Entity?.RoleDefinition);

        if (!res.IsValid || res.Entity is null)
            return;

        // Get the created user 
        userId = res.Entity.UserId;
        
        httpRes = await HttpClient.GetAsync($"{UrlBase}api/users/{userId}");
        res = await ProcessResultFromTestHttpResponse<UserDto>(httpRes);

        Assert.IsTrue(res.IsValid);
        Assert.IsNotNull(res.Entity);
        Assert.IsTrue(userId == res.Entity?.UserId);
        Assert.IsTrue(userName == res.Entity?.UserName);

        if (!res.IsValid || res.Entity is null)
            return;

        // Update 
        user = res.Entity;
        string newUserFullName = $"{user.FullName} UPDATED!!";
        user.FullName = newUserFullName;
        httpRes = await HttpClient.PutAsJsonAsync($"{UrlBase}api/users", user);
        res = await ProcessResultFromTestHttpResponse<UserDto>(httpRes);

        Assert.IsTrue(res.IsValid);
        Assert.IsNotNull(res.Entity);
        Assert.IsTrue(userId == res.Entity?.UserId);
        Assert.IsTrue(newUserFullName == res.Entity?.FullName);

        if (!res.IsValid || res.Entity is null)
            return;

        // Delete
        httpRes = await HttpClient.DeleteAsync($"{UrlBase}api/users/{userId}");
        var resDel = await ProcessResultFromTestHttpResponse<UserDto>(httpRes);

        Assert.IsTrue(resDel.IsValid);
        Assert.IsNotNull(resDel.Entity);
        Assert.IsTrue(userId == resDel.Entity?.UserId);
    }

}
