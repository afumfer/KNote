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
public class NoteTypesTests : WebApiTestBase
{
    public NoteTypesTests() : base()
    {
        
    }

    [TestMethod]
    public async Task Get_All()
    {
        var httpRes = await HttpClient.GetAsync($"{UrlBase}api/notetypes");
        var res = await ProcessResultFromTestHttpResponse<List<NoteTypeDto>>(httpRes);

        Assert.IsTrue(res.IsValid);
        Assert.IsTrue(res.Entity.Count > 0);
    }

    [TestMethod]
    public async Task Execute_BasicCRUD()
    {
        // Create 
        string noteTypeName = "__TEST_NOTETYPE_###__";
        string noteTypeDescription = "__TEST_NOTETYPE_DESCRIPTION_###__";
        Guid noteTypeId = Guid.Empty;
        NoteTypeDto noteType = new() { NoteTypeId = noteTypeId, Name = noteTypeName, Description = noteTypeDescription };

        var httpRes = await HttpClient.PostAsJsonAsync($"{UrlBase}api/notetypes", noteType);
        var res = await ProcessResultFromTestHttpResponse<NoteTypeDto>(httpRes);

        Assert.IsTrue(res.IsValid);
        Assert.IsNotNull(res.Entity);
        Assert.IsTrue(noteTypeId != res.Entity?.NoteTypeId);
        Assert.IsTrue(noteTypeName == res.Entity?.Name);
        Assert.IsTrue(noteTypeDescription == res.Entity?.Description);

        if (!res.IsValid || res.Entity is null)
            return;

        // Get the created notetype 
        noteTypeId = res.Entity.NoteTypeId;

        httpRes = await HttpClient.GetAsync($"{UrlBase}api/notetypes/{noteTypeId}");
        res = await ProcessResultFromTestHttpResponse<NoteTypeDto>(httpRes);

        Assert.IsTrue(res.IsValid);
        Assert.IsNotNull(res.Entity);
        Assert.IsTrue(noteTypeId == res.Entity?.NoteTypeId);
        Assert.IsTrue(noteTypeName == res.Entity?.Name);
        Assert.IsTrue(noteTypeDescription == res.Entity?.Description);

        if (!res.IsValid || res.Entity is null)
            return;

        // Update 
        noteType = res.Entity;
        string newDescription = $"{noteType.Description} UPDATED!!";
        noteType.Description = newDescription;
        httpRes = await HttpClient.PutAsJsonAsync($"{UrlBase}api/notetypes", noteType);
        res = await ProcessResultFromTestHttpResponse<NoteTypeDto>(httpRes);

        Assert.IsTrue(res.IsValid);
        Assert.IsNotNull(res.Entity);
        Assert.IsTrue(noteTypeId == res.Entity?.NoteTypeId);
        Assert.IsTrue(newDescription == res.Entity?.Description);

        if (!res.IsValid || res.Entity is null)
            return;

        // Delete
        httpRes = await HttpClient.DeleteAsync($"{UrlBase}api/notetypes/{noteTypeId}");
        var resDel = await ProcessResultFromTestHttpResponse<NoteTypeDto>(httpRes);

        Assert.IsTrue(resDel.IsValid);
        Assert.IsNotNull(resDel.Entity);
        Assert.IsTrue(noteTypeId == resDel.Entity?.NoteTypeId);
    }
}
