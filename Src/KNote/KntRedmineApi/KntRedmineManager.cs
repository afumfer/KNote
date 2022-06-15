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

    public bool IssueToNoteDto(string id, NoteExtendedDto? noteDto, ref string? folder)
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
            var customFields = issue?.CustomFields;

            // TODO: hack for extract folder name.
            if (customFields != null)
            {
                folder = customFields[0].Values[0].Info.ToString();
                noteDto.KAttributesDto[2].Value = customFields[0].Values[0].Info.ToString();
                noteDto.KAttributesDto[5].Value = customFields[1].Values[0].Info.ToString();
                noteDto.KAttributesDto[14].Value = customFields[2].Values[0].Info.ToString();
                noteDto.KAttributesDto[15].Value = customFields[3].Values[0].Info.ToString();
            }
            
            noteDto.KAttributesDto[0].Value = issue?.Author.Name;
            noteDto.KAttributesDto[1].Value = issue?.Project.Name;
            noteDto.KAttributesDto[3].Value = issue?.Priority.Name;
            noteDto.KAttributesDto[4].Value = issue?.Status.Name;
            noteDto.KAttributesDto[6].Value = issue?.TotalEstimatedHours.ToString();
            noteDto.KAttributesDto[7].Value = issue?.TotalSpentHours.ToString();
            noteDto.KAttributesDto[8].Value = issue?.CreatedOn.ToString();
            noteDto.KAttributesDto[9].Value = issue?.UpdatedOn.ToString();
            noteDto.KAttributesDto[10].Value = issue?.DueDate.ToString();
            noteDto.KAttributesDto[11].Value = issue?.StartDate.ToString();
            noteDto.KAttributesDto[12].Value = issue?.ClosedOn.ToString();
            noteDto.KAttributesDto[13].Value = issue?.DoneRatio.ToString();
            noteDto.KAttributesDto[16].Value = issue?.FixedVersion?.Name;

            if (issue?.Attachments != null)
            {
                foreach (var atch in issue.Attachments)
                {
                    var resource = new ResourceDto();

                    var findRes = noteDto.Resources.FirstOrDefault(r => r.Name.IndexOf(atch.FileName)>-1);

                    if(findRes == null)
                    {
                        resource.ResourceId = Guid.NewGuid();
                        resource.ContentInDB = false;
                        resource.Name = $"{resource.ResourceId}_{atch.FileName}";
                        resource.Description = atch.Description;
                        resource.Order = 0;
                        resource.ContentArrayBytes = _manager.DownloadFile(atch.ContentUrl);
                    
                        noteDto.Resources.Add(resource);
                    }
                }
            }

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