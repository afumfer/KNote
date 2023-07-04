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

}
