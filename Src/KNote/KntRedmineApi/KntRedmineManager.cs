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
        try
        {
            if (noteDto == null)
                throw new ArgumentException("noteDto cannot be null");

            var issue = _manager.GetObject<Issue>(id, _parameters);

            if (issue == null)
                return false;

            noteDto.Topic = issue.Subject;
            noteDto.Description = issue.Description;
            noteDto.Tags = $"HU#{issue.Id}";
            //noteDto.ContentType = 
            //noteDto.CreationDateTime = 
            //noteDto.FolderId = 
            // ...

            return true;

        }
        catch (Exception)
        {
            throw;
        }
    }


    public bool Test1HelloWorld()
    {
        try
        {

            var issue = _manager.GetObject<Issue>("146149", _parameters);

            if (issue == null)
                return false;

            Console.WriteLine("#{0}: {1}", issue?.Id, issue?.Subject, issue?.Description);
            Console.WriteLine("=======");
            Console.WriteLine(issue?.Description);
            Console.WriteLine("=======");

            var a = issue?.ChangeSets;
            var b = issue?.Children;
            var c = issue?.CustomFields;
            var d = issue?.Notes;
            var e = issue?.PrivateNotes;
            var f = issue?.Relations;
            var g = issue?.Uploads;
            var m = issue?.Watchers;
            var n = issue?.TotalEstimatedHours;
            var o = issue?.TotalSpentHours;

            if (issue?.Attachments != null)
            {
                foreach (var i in issue.Attachments)
                {
                    var cUrl = i.ContentUrl;
                    Console.WriteLine(cUrl);
                }

            }

            // Download file explames
            //var attach1 = manager.GetObject<Attachment>("96613", null);
            // Option 1
            //var webClient = manager.CreateWebClient(null);
            //webClient.DownloadFile(attach1.ContentUrl, @"d:\Tmp\x3.png");
            // option 2
            //var file = manager.DownloadFile(attach1.ContentUrl);


            // ???
            ////var parameters = new NameValueCollection { { "status_id", "*" } };
            //var xx = manager.GetObjects<Issue>(new NameValueCollection { { "issue_id", "146149" } });
            //foreach (var issue in xx)
            //{
            //    Console.WriteLine("#{0}: {1}", issue.Id, issue.Subject);
            //}

            //Create a issue. ???
            //var newIssue = new Issue { Subject = "test", Project = new IdentifiableName { Id = 1 } };
            // manager.CreateObject(newIssue);

            return true;

        }
        catch (Exception)
        {
            throw;
        }
    }


}