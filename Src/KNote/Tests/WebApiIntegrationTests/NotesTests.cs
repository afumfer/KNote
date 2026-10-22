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
        // A note of its own, in the first folder: the test doesn't depend on a given note being in the database.
        var httpResFolders = await HttpClient.GetAsync($"{UrlBase}api/folders");
        var resFolders = await ProcessResultFromTestHttpResponse<List<FolderInfoDto>>(httpResFolders);

        Assert.IsTrue(resFolders.IsValid);
        var folder = resFolders.Entity?.FirstOrDefault();
        Assert.IsNotNull(folder);

        // Add
        var newNote = new NoteDto
        {
            NoteId = Guid.Empty,
            Topic = "NEW NOTE ###",
            Description = "NEW NOTE ###",
            FolderId = folder.FolderId,
            // MVC validates this nested property too: without it, an empty FolderDto fails its [Required] Name.
            FolderDto = folder,
            CreationDateTime = DateTime.Now,
            ModificationDateTime = DateTime.Now
        };

        var httpResPost = await HttpClient.PostAsJsonAsync($"{UrlBase}api/notes", newNote);
        var resNewNote = await ProcessResultFromTestHttpResponse<NoteDto>(httpResPost);

        Assert.IsTrue(resNewNote.IsValid);
        Assert.IsNotNull(resNewNote.Entity);

        if (!resNewNote.IsValid || resNewNote.Entity is null)
            return;

        var newNoteId = resNewNote.Entity.NoteId;
        newNote = resNewNote.Entity;

        try
        {
            // Get
            var httpResGet = await HttpClient.GetAsync($"{UrlBase}api/notes/{newNoteId}");
            var resGet = await ProcessResultFromTestHttpResponse<NoteDto>(httpResGet);

            Assert.IsTrue(resGet.IsValid);
            Assert.IsTrue(newNote.Topic == resGet.Entity?.Topic);

            // Update
            newNote.Topic = " UPDATED!!";
            newNote.Description = "UPDATED!!";

            var httpResUpd = await HttpClient.PutAsJsonAsync($"{UrlBase}api/notes", newNote);
            var resUpd = await ProcessResultFromTestHttpResponse<NoteDto>(httpResUpd);

            Assert.IsTrue(resUpd.IsValid);
            Assert.IsNotNull(resUpd.Entity);
            Assert.IsTrue(newNote.Topic == resUpd.Entity?.Topic);
            Assert.IsTrue(newNote.Description == resUpd.Entity?.Description);
        }
        finally
        {
            // Delete, also when a step above failed: the note is the test's own.
            var httpResDel = await HttpClient.DeleteAsync($"{UrlBase}api/notes/{newNoteId}");
            var resDel = await ProcessResultFromTestHttpResponse<NoteDto>(httpResDel);

            Assert.IsTrue(resDel.IsValid);
            Assert.IsNotNull(resDel.Entity);
            Assert.IsTrue(newNoteId == resDel.Entity?.NoteId);
        }
    }
}
