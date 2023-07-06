using KNote.Model;
using KNote.Model.Dto;
using KNote.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace KNote.Tests.WebApiIntegrationTests;

[TestClass]
public class NotesTests : WebApiTestBase
{
    public NotesTests() : base()
    {
        
    }

    [TestMethod]
    public async Task Get_HomeNotes()
    {
        var httpRes = await HttpClient.GetAsync($"{UrlBase}api/notes/homenotes");
        var res = await ProcessResultFromTestHttpResponse<List<NoteInfoDto>>(httpRes);

        Assert.IsTrue(res.IsValid);
        Assert.IsTrue(res.Entity.Count > 0);
    }

    [TestMethod]
    public async Task Get_Search()
    {        
        var queryString = "textSearch=Hopper&pageNumber=1&pageSize=25";

        var httpRes = await HttpClient.GetAsync($"{UrlBase}api/notes/search?{queryString}");
        var res = await ProcessResultFromTestHttpResponse<List<NoteInfoDto>>(httpRes);

        Assert.IsTrue(res.IsValid);
        Assert.IsTrue(res.Entity.Count > 0);
    }

    [TestMethod]
    public async Task Get_Filter()
    {
        PageIdentifier page = new() { PageNumber = 1, PageSize = 10 };
        NotesFilterDto notesFilter = new() { Topic = "Hopper", PageIdentifier = page };

        var httpRes = await HttpClient.PostAsJsonAsync($"{UrlBase}api/notes/filter", notesFilter);
        var res = await ProcessResultFromTestHttpResponse<List<NoteInfoDto>>(httpRes);

        Assert.IsTrue(res.IsValid);
        Assert.IsTrue(res.Entity.Count > 0);
    }

    [TestMethod]
    public async Task Execute_BasicCRUD()
    {
        // Get note 
        // TODO: hack this guid
        Guid id = Guid.Parse("1BF07EDF-039C-4CE9-98A6-0001A9C1E0C2");

        var httpResGet = await HttpClient.GetAsync($"{UrlBase}api/notes/{id}");
        var res = await ProcessResultFromTestHttpResponse<NoteDto>(httpResGet);

        Assert.IsTrue(res.IsValid);

        if (!res.IsValid || res.Entity is null)
            return;

        // Add
        var newNote = res.Entity.GetSimpleDto<NoteDto>();
        newNote.Topic = "NEW NOTE ###";
        newNote.Description = "NEW NOTE ###";
        newNote.NoteId = Guid.Empty;
        newNote.NoteNumber = 0;

        var httpResPost = await HttpClient.PostAsJsonAsync($"{UrlBase}api/notes", newNote);
        var resNewNote = await ProcessResultFromTestHttpResponse<NoteDto>(httpResPost);

        Assert.IsTrue(resNewNote.IsValid);
        Assert.IsNotNull(resNewNote.Entity);

        if (!resNewNote.IsValid || resNewNote.Entity is null)
            return;

        var newNoteId = resNewNote.Entity.NoteId;
        newNote = resNewNote.Entity;

        // Update 
        newNote.Topic = " UPDATED!!";
        newNote.Description = "UPDATED!!";

        var httpResUpd = await HttpClient.PutAsJsonAsync($"{UrlBase}api/notes", newNote);
        var resUpd = await ProcessResultFromTestHttpResponse<NoteDto>(httpResUpd);

        Assert.IsTrue(resUpd.IsValid);
        Assert.IsNotNull(resUpd.Entity);
        Assert.IsTrue(newNote.Topic == resUpd.Entity?.Topic);
        Assert.IsTrue(newNote.Description == resUpd.Entity?.Description);

        if (!res.IsValid || res.Entity is null)
            return;

        // Delete
        var httpResDel = await HttpClient.DeleteAsync($"{UrlBase}api/notes/{newNoteId}");
        var resDel = await ProcessResultFromTestHttpResponse<NoteDto>(httpResDel);

        Assert.IsTrue(resDel.IsValid);
        Assert.IsNotNull(resDel.Entity);
        Assert.IsTrue(newNoteId == resDel.Entity?.NoteId);
    }
}
