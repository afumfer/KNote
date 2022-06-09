using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using KNote.Model;
using KNote.Model.Dto;
using System.Collections.Specialized;

namespace KntRedmineApi;

public class KntRedmineManager
{
    private readonly string _host;
    private readonly string _apiKey;
    private readonly RedmineManager _manager;
    private NameValueCollection _parameters = new NameValueCollection { { "include", "attachments,relations" } };

    public KntRedmineManager(string host, string apiKey)
    {
        _host = host;
        _apiKey = apiKey;
        _manager = new RedmineManager(_host, _apiKey);
    }

    public bool IssueToNoteDto(string id, NoteDto noteDto)
    {
        if (noteDto == null)
            throw new ArgumentException("noteDto cannot be null");

        var issue = _manager.GetObject<Issue>(id, _parameters);

        if (issue == null)
            return false;

        noteDto.Topic = issue.Subject;
        noteDto.Description = issue.Description;
        noteDto.NoteNumber = issue.Id;
        //noteDto.ContentType = 
        //noteDto.CreationDateTime = 
        //noteDto.FolderId = 
        // ...

        return true;
    }


    public FolderDto Test1HelloWorld()
    {
        var f = new FolderDto();
        f.Name = "Hello World";
        return f;
    }

    public Issue Test2HelloWorld()
    {        
        var issue = _manager.GetObject<Issue>("146149", _parameters);
        return issue;
    }

}