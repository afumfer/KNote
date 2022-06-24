using Redmine.Net.Api;
using Redmine.Net.Api.Types;
using KNote.Model;
using KNote.Model.Dto;
using System.Collections.Specialized;
using System.Diagnostics;

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

    public bool IssueToNoteDto(string id, NoteExtendedDto? noteDto, bool loadAttachments = true)
    {
        try
        {
            if (noteDto == null)
                throw new ArgumentException("noteDto cannot be null");

            var issue = _manager.GetObject<Issue>(id, _parameters);

            if (issue == null)
                return false;

            noteDto.Topic = issue.Subject;                        
            noteDto.Tags = $"HU#{issue.Id}";
            noteDto.ContentType = "markdown";
            noteDto.Description = issue.Description;
            var customFields = issue?.CustomFields;

            if(noteDto.KAttributesDto.Count > 0)
            {
                // TODO: hack for extract folder name.
                if (customFields != null)
                {
                    if (customFields[0]?.Values != null)
                        noteDto.KAttributesDto[2].Value = customFields[0]?.Values[0]?.Info?.ToString();
                    if (customFields[1]?.Values != null)
                        noteDto.KAttributesDto[5].Value = customFields[1]?.Values[0]?.Info?.ToString();
                    if(customFields[2]?.Values != null)
                        noteDto.KAttributesDto[14].Value = customFields[2]?.Values[0]?.Info?.ToString();
                    if (customFields[3]?.Values != null)
                        noteDto.KAttributesDto[15].Value = customFields[3]?.Values[0]?.Info?.ToString();
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
            }

            if (issue?.Attachments != null && loadAttachments)
            {
                foreach (var atch in issue.Attachments)
                {
                    // TODO: Go to to service layer for get new resourceDto (with container completed).
                    //var resource = new ResourceDto();

                    var findRes = noteDto.Resources.FirstOrDefault(r => r.Name.IndexOf(atch.FileName)>-1);

                    if (findRes == null)
                    {
                        findRes = new ResourceDto();
                        findRes.ResourceId = Guid.NewGuid();                        
                        findRes.ContentInDB = false;
                        findRes.Name = $"{findRes.ResourceId}_{atch.FileName}";
                        findRes.Description = atch.Description;
                        findRes.Order = 0;
                        findRes.ContentArrayBytes = _manager.DownloadFile(atch.ContentUrl);                    
                        noteDto.Resources.Add(findRes);                        
                    }
                }
            }
                        
            return true;
        }
        catch (Exception ex)
        {
            var a = ex.Message;
            throw;
        }
    }

    public string PredictPH(string gestion, string tema, string descripcion)
    {        
        RedMinePunHis.ModelInput dataInput = new RedMinePunHis.ModelInput()
        {
            Gestion = gestion,
            Tema = tema,
            Descripcion = descripcion
        };

        var predictionResult = RedMinePunHis.Predict(dataInput);

        return predictionResult.PredictedLabel;
    }

    public string PredictGestion(string tema, string descripcion)
    {
        RedMineGestion.ModelInput dataInput = new RedMineGestion.ModelInput()
        {            
            Tema = tema,
            Descripcion = descripcion
        };

        var predictionResult = RedMineGestion.Predict(dataInput);

        return predictionResult.PredictedLabel;
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