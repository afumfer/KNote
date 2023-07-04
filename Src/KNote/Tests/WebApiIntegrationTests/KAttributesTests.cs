using KNote.Model.Dto;
using KNote.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Tests.WebApiIntegrationTests;

[TestClass]
public class KAttributesTests : WebApiTestBase
{
    public KAttributesTests() : base()
    {
        
    }

    [TestMethod]
    public async Task Get_All()
    {
        var httpRes = await HttpClient.GetAsync($"{UrlBase}api/kattributes");
        var res = await ProcessResultFromTestHttpResponse<List<KAttributeInfoDto>>(httpRes);

        Assert.IsTrue(res.IsValid);
        Assert.IsTrue(res.Entity.Count > 0);
    }

    [TestMethod]
    public async Task Execute_BasicCRUD()
    {
        // Create 
        string kattributeName = "__TEST_KATTRIBUTE_###__";
        string kattributeDescription = "__TEST_KATTRIBUTE_DESCRIPTION_###__";
        Guid kattributeId = Guid.Empty;
        KAttributeInfoDto kAttribute = new() { KAttributeId = kattributeId, Name = kattributeName, Description = kattributeDescription };

        var httpRes = await HttpClient.PostAsJsonAsync($"{UrlBase}api/kattributes", kAttribute);
        var res = await ProcessResultFromTestHttpResponse<KAttributeInfoDto>(httpRes);

        Assert.IsTrue(res.IsValid);
        Assert.IsNotNull(res.Entity);
        Assert.IsTrue(kattributeId != res.Entity?.KAttributeId);
        Assert.IsTrue(kattributeName == res.Entity?.Name);
        Assert.IsTrue(kattributeDescription == res.Entity?.Description);

        if (!res.IsValid || res.Entity is null)
            return;

        // Get the created KAttribute 
        kattributeId = res.Entity.KAttributeId;

        httpRes = await HttpClient.GetAsync($"{UrlBase}api/kattributes/{kattributeId}");
        res = await ProcessResultFromTestHttpResponse<KAttributeInfoDto>(httpRes);

        Assert.IsTrue(res.IsValid);
        Assert.IsNotNull(res.Entity);
        Assert.IsTrue(kattributeId == res.Entity?.KAttributeId);
        Assert.IsTrue(kattributeName == res.Entity?.Name);
        Assert.IsTrue(kattributeDescription == res.Entity?.Description);

        if (!res.IsValid || res.Entity is null)
            return;

        // Update 
        kAttribute = res.Entity;
        string newDescription = $"{kAttribute.Description} UPDATED!!";
        kAttribute.Description = newDescription;
        httpRes = await HttpClient.PutAsJsonAsync($"{UrlBase}api/kattributes", kAttribute);
        res = await ProcessResultFromTestHttpResponse<KAttributeInfoDto>(httpRes);

        Assert.IsTrue(res.IsValid);
        Assert.IsNotNull(res.Entity);
        Assert.IsTrue(kattributeId == res.Entity?.KAttributeId);
        Assert.IsTrue(newDescription == res.Entity?.Description);

        if (!res.IsValid || res.Entity is null)
            return;

        // Delete
        httpRes = await HttpClient.DeleteAsync($"{UrlBase}api/kattributes/{kattributeId}");
        var resDel = await ProcessResultFromTestHttpResponse<KAttributeInfoDto>(httpRes);

        Assert.IsTrue(resDel.IsValid);
        Assert.IsNotNull(resDel.Entity);
        Assert.IsTrue(kattributeId == resDel.Entity?.KAttributeId);
    }

}
