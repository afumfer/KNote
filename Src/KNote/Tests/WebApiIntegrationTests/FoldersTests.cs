using KNote.Model.Dto;
using KNote.Tests.Helpers;
using System.Net.Http.Json;

namespace KNote.Tests.WebApiIntegrationTests;

[TestClass]
public class FoldersTests : WebApiTestBase
{
    public FoldersTests() : base()
    {

    }

    [TestMethod]
    public async Task Get_All()
    {
        var httpRes = await HttpClient.GetAsync($"{UrlBase}api/folders");
        var res = await ProcessResultFromTestHttpResponse<List<FolderInfoDto>>(httpRes);

        Assert.IsTrue(res.IsValid);
        Assert.IsTrue(res.Entity.Count > 0);
    }

    [TestMethod]
    public async Task Get_Tree()
    {
        var httpRes = await HttpClient.GetAsync($"{UrlBase}api/folders/tree");
        var res = await ProcessResultFromTestHttpResponse<List<FolderDto>>(httpRes);

        Assert.IsTrue(res.IsValid);
        Assert.IsTrue(res.Entity.Count > 0);
    }

    [TestMethod]
    public async Task Execute_BasicCRUD()
    {
        // Create 
        string folderName = "__TEST FOLDER ###__";
        Guid folderId = Guid.Empty;
        FolderDto folder = new() { FolderId = folderId, FolderNumber = 0 , Name = folderName, ParentId = null};

        var httpRes = await HttpClient.PostAsJsonAsync($"{UrlBase}api/folders", folder);
        var res = await ProcessResultFromTestHttpResponse<FolderDto>(httpRes);

        Assert.IsTrue(res.IsValid);
        Assert.IsNotNull(res.Entity);
        Assert.IsTrue(folderId != res.Entity?.FolderId);
        Assert.IsTrue(folderName == res.Entity?.Name);

        if (!res.IsValid || res.Entity is null)
            return;

        // Get the created folderId 
        folderId = res.Entity.FolderId;

        httpRes = await HttpClient.GetAsync($"{UrlBase}api/folders/{folderId}");
        res = await ProcessResultFromTestHttpResponse<FolderDto>(httpRes);

        Assert.IsTrue(res.IsValid);
        Assert.IsNotNull(res.Entity);
        Assert.IsTrue(folderId == res.Entity?.FolderId);
        Assert.IsTrue(folderName == res.Entity?.Name);

        if (!res.IsValid || res.Entity is null)
            return;

        // Update 
        folder = res.Entity;
        string newFolderName = $"{folder.Name} UPDATED!!" ;
        folder.Name = newFolderName;
        httpRes = await HttpClient.PutAsJsonAsync($"{UrlBase}api/folders", folder);
        res = await ProcessResultFromTestHttpResponse<FolderDto>(httpRes);

        Assert.IsTrue(res.IsValid);
        Assert.IsNotNull(res.Entity);
        Assert.IsTrue(folderId == res.Entity?.FolderId);
        Assert.IsTrue(newFolderName == res.Entity?.Name);

        if (!res.IsValid || res.Entity is null)
            return;

        // Delete
        httpRes = await HttpClient.DeleteAsync($"{UrlBase}api/folders/{folderId}");
        res = await ProcessResultFromTestHttpResponse<FolderDto>(httpRes);

        Assert.IsTrue(res.IsValid);
        Assert.IsNotNull(res.Entity);
        Assert.IsTrue(folderId == res.Entity?.FolderId);
    }   

}