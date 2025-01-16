using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Interfaces;
using KNote.Service.Core;
using KNote.Service.ServicesCommands;

namespace KNote.Service.Services;

public class KntNoteService : KntServiceBase, IKntNoteService
{
    #region Constructor

    public KntNoteService(IKntService service) : base(service)
    {

    }

    #endregion

    #region IKntNoteService

    public async Task<Result<List<NoteInfoDto>>> GetAllAsync()
    {
        var command = new KntNotesGetAllAsyncCommand(Service);
        return await ExecuteCommand(command);
    }
    
    public async Task<Result<List<NoteInfoDto>>> HomeNotesAsync()
    {     
        var command = new KntNotesHomeAllAsyncCommand(Service);
        return await ExecuteCommand(command);
    }

    public async Task <Result<NoteDto>> GetAsync(Guid noteId)
    {        
        var command = new KntNotesGetAsyncCommand(Service, noteId);
        return await ExecuteCommand(command);
    }

    public async Task<Result<NoteDto>> GetAsync(int noteNumber)
    {        
        var command = new KntNotesGetByNumberAsyncCommand(Service, noteNumber);
        return await ExecuteCommand(command);
    }

    public async Task<Result<NoteExtendedDto>> GetExtendedAsync(Guid noteId)
    {
        var command = new KntNotesGetExtendedAsyncCommand(Service, noteId);
        return await ExecuteCommand(command);
    }

    public async Task<Result<List<NoteInfoDto>>> GetByFolderAsync(Guid folderId)
    {
        var command = new KntNotesGetByFolderAsyncCommand(Service, folderId);
        return await ExecuteCommand(command);
    }

    public async Task<Result<List<NoteInfoDto>>> GetFilter(NotesFilterDto notesFilter)
    {     
        var command = new KntNotesGetFilterAsyncCommand(Service, notesFilter);
        return await ExecuteCommand(command);
    }

    public async Task<Result<List<NoteInfoDto>>> GetSearch(NotesSearchDto notesSearch)
    {        
        var command = new KntNotesGetSearchAsyncCommand(Service, notesSearch);
        return await ExecuteCommand(command);
    }

    public async Task<Result<NoteDto>> NewAsync(NoteInfoDto entityInfo = null)
    {
        var command = new KntNotesNewAsyncCommand(Service, entityInfo);
        return await ExecuteCommand(command);
    }

    public async Task<Result<NoteExtendedDto>> NewExtendedAsync(NoteInfoDto entityInfo = null)
    {
        var command = new KntNotesNewExtendedAsyncCommand(Service, entityInfo);
        return await ExecuteCommand(command);
    }

    public async Task<Result<NoteDto>> SaveAsync(NoteDto entity, bool updateStatus = true)
    {
        var command = new KntNotesSaveAsyncCommand(Service, entity, updateStatus);
        return await ExecuteCommand(command);
    }
    
    public async Task<Result<NoteExtendedDto>> SaveExtendedAsync(NoteExtendedDto entity)
    {
        var command = new KntNotesSaveExtendedAsyncCommand(Service, entity);
        return await ExecuteCommand(command);
    }

    public async Task<Result<NoteDto>> DeleteAsync(Guid id)
    {
        var command = new KntNotesDeleteAsyncCommand(Service, id);
        return await ExecuteCommand(command);
    }

    public async Task<Result<NoteExtendedDto>> DeleteExtendedAsync(Guid id)
    {
        var command = new KntNotesDeleteExtendedAsyncCommand(Service, id);
        return await ExecuteCommand(command);
    }

    public async Task<Result<List<ResourceDto>>> GetResourcesAsync(Guid noteId)
    {
        var command = new KntNotesGetResourcesAsyncCommand(Service, noteId);
        return await ExecuteCommand(command);
    }

    public async Task<Result<List<ResourceInfoDto>>> GetResourcesInfoAsync(Guid noteId)
    {
        var command = new KntNotesGetResourcesInfoAsyncCommand(Service, noteId);
        return await ExecuteCommand(command);
    }

    public async Task<Result<ResourceDto>> GetResourceAsync(Guid resourceId)
    {
        var command = new KntNotesGetResourceAsyncCommand(Service, resourceId);
        return await ExecuteCommand(command);
    }

    public async Task<Result<ResourceDto>> SaveResourceAsync(ResourceDto resource, bool forceNew = false)
    {
        var command = new KntNotesSaveResourceAsyncCommand(Service, resource, forceNew);
        return await ExecuteCommand(command);
    }

    public async Task<Result<ResourceInfoDto>> SaveResourceAsync(ResourceInfoDto resourceInfo, bool forceNew = false)
    {
        var command = new KntNotesSaveResourceInfoAsyncCommand(Service, resourceInfo, forceNew);
        return await ExecuteCommand(command);
    }

    public async Task<Result<ResourceDto>> DeleteResourceAsync(Guid id)
    {
        var command = new KntNotesDeleteResourceAsyncCommand(Service, id);
        return await ExecuteCommand(command);
    }

    public async Task<Result<ResourceInfoDto>> DeleteResourceInfoAsync(Guid id)
    {
        var command = new KntNotesDeleteResourceInfoAsyncCommand(Service, id);
        return await ExecuteCommand(command);
    }

    public async Task<Result<List<NoteTaskDto>>> GetNoteTasksAsync(Guid idNote)
    {
        var command = new KntNotesGetNoteTasksAsyncCommand(Service, idNote);
        return await ExecuteCommand(command);
    }

    public async Task<Result<List<NoteTaskDto>>> GetStartedTasksByDateTimeRageAsync(DateTime startDateTime, DateTime endDateTime)
    {     
        var command = new KntNotesGetStartedTasksByDateTimeRageAsyncCommand(Service, startDateTime, endDateTime);
        return await ExecuteCommand(command);
    }

    public async Task<Result<List<NoteTaskDto>>> GetEstimatedTasksByDateTimeRageAsync(DateTime startDateTime, DateTime endDateTime)
    {        
        var command = new KntNotesGetEstimatedTasksByDateTimeRageAsyncCommand(Service, startDateTime, endDateTime);
        return await ExecuteCommand(command);
    }


    public async Task<Result<NoteTaskDto>> GetNoteTaskAsync(Guid noteTaskId)
    {
        var command = new KntNotesGetNoteTaskAsyncCommand(Service, noteTaskId);
        return await ExecuteCommand(command);
    }

    public async Task<Result<NoteTaskDto>> SaveNoteTaskAsync(NoteTaskDto entity, bool forceNew = false)
    {
        var command = new KntNotesSaveNoteTaskAsyncCommand(Service, entity, forceNew);
        return await ExecuteCommand(command);
    }

    public async Task<Result<NoteTaskDto>> DeleteNoteTaskAsync(Guid id)
    {
        var command = new KntNotesDeleteNoteTaskAsyncCommand(Service, id);
        return await ExecuteCommand(command);
    }

    public async Task<Result<List<KMessageDto>>> GetMessagesAsync(Guid noteId)
    {
        var command = new KntNotesGetMessagesAsyncCommand(Service, noteId);
        return await ExecuteCommand(command);
    }

    public async Task<Result<KMessageDto>> GetMessageAsync(Guid messageId)
    {        
        var command = new KntNotesGetMessageAsyncCommand(Service, messageId);
        return await ExecuteCommand(command);
    }

    public async Task<Result<KMessageDto>> SaveMessageAsync(KMessageDto entity, bool forceNew = false)
    {
        var command = new KntNotesSaveMessageAsyncCommand(Service, entity, forceNew);
        return await ExecuteCommand(command);
    }

    public async Task<Result<KMessageDto>> DeleteMessageAsync(Guid messageId)
    {
        var command = new KntNotesDeleteMessageAsyncCommand(Service, messageId);
        return await ExecuteCommand(command);
    }

    public async Task<Result<WindowDto>> GetWindowAsync(Guid noteId, Guid userId)
    {     
        var command = new KntNotesGetWindowAsyncCommand(Service, noteId, userId);
        return await ExecuteCommand(command);
    }

    public async Task<Result<WindowDto>> SaveWindowAsync(WindowDto entity, bool forceNew = false)
    {
        var command = new KntNotesSaveWindowAsyncCommand(Service, entity, forceNew);
        return await ExecuteCommand(command);
    }

    public async Task<Result<List<Guid>>> GetVisibleNotesIdAsync(string userName)
    {
        var command = new KntNotesGetVisibleNotesIdAsyncCommand(Service, userName);
        return await ExecuteCommand(command);
    }

    public async Task<Result<List<Guid>>> GetAlarmNotesIdAsync(string userName, EnumNotificationType? notificationType = null)
    {
        var command = new KntNotesGetAlarmNotesIdAsyncCommand(Service, userName, notificationType);
        return await ExecuteCommand(command);
    }

    #endregion

    #region Utils

    public async Task<Result<bool>> UtilPatchFolder(Guid noteId, Guid folderId)
    {
        var command = new KntNotesPatchFolderAsyncCommand(Service, noteId, folderId);
        return await ExecuteCommand(command);
    }

    public async Task<Result<bool>> UtilPatchChangeTags(Guid noteId, string oldTag, string newTag)
    {        
        var command = new KntNotesPatchChangeTagsAsyncCommand(Service, noteId, oldTag, newTag);
        return await ExecuteCommand(command);
    }

    public string UtilGetNoteStatus(List<NoteTaskDto> tasks, List<KMessageDto> messages)
    {
        string status = "";

        var tasksValid = tasks.Where(t => t.IsDeleted() == false).Select(t => t).ToList();
        var messagesValid = messages.Where(m => m.IsDeleted() == false).Select(m => m).ToList();

        bool allTaskResolved = true;
        if (tasksValid?.Count > 0)
        {
            foreach (var item in tasks)
            {
                if (item.Resolved == false)
                {
                    allTaskResolved = false;
                    break;
                }
            }
        }
        else
        {
            allTaskResolved = false;
        }

        bool alarmsPending = false;
        foreach (var item in messagesValid)
        {
            if (item.AlarmActivated == true)
            {
                alarmsPending = true;
                break;
            }

        }

        if (allTaskResolved == true)
            status = KntConst.Status[EnumStatus.Resolved];

        if (alarmsPending == true)
        {
            if (!string.IsNullOrEmpty(status))
                status += "; ";
            status += KntConst.Status[EnumStatus.AlarmsPending];
        }

        return status;
    }

    public (string, string) UtilGetResourceUrls(ResourceDto resource)
    {
        string rootUrl = Repository.RespositoryRef.ResourcesContainerRootUrl;
        string relativeUrl;
        string fullUrl;

        if (string.IsNullOrEmpty(resource.Container))
        {
            resource.Container = Repository.RespositoryRef.ResourcesContainer + @"\" + DateTime.Now.Year.ToString();
            resource.ContentInDB = Repository.RespositoryRef.ResourceContentInDB;
        }

        if (string.IsNullOrEmpty(rootUrl) || string.IsNullOrEmpty(resource.Container) || string.IsNullOrEmpty(resource.Name))
            return (null, null);

        relativeUrl = (Path.Combine(resource.Container, resource.Name)).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        fullUrl = (Path.Combine(rootUrl, relativeUrl)).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        return (relativeUrl, fullUrl);
    }

    public async Task<List<NoteKAttributeDto>> UtilCompleteNoteAttributes(List<NoteKAttributeDto> attributesNotes, Guid noteId, Guid? noteTypeId = null)
    {     
        return await Repository.Notes.CompleteNoteAttributes(attributesNotes, noteId, noteTypeId);
    }

    public bool UtilManageResourceContent(ResourceDto resource, bool forceUpdateDto = true)
    {        
        if (resource == null)
            return false;

        string rootCacheResource = Repository.RespositoryRef.ResourcesContainerRootPath;
        if (string.IsNullOrEmpty(resource.Container))
        {
            if (forceUpdateDto)
            {
                resource.Container = Repository.RespositoryRef.ResourcesContainer + @"\" + DateTime.Now.Year.ToString();
                resource.ContentInDB = Repository.RespositoryRef.ResourceContentInDB;
            }
        }

        // TODO: anotate this meesage in log, and throw exception !!! ???
        if (rootCacheResource == null || resource.Container == null || resource.Name == null)
            return false;

        try
        {
            string dirPath = Path.Combine(new string[] { rootCacheResource, resource.Container });
            string file = UtilGetResourcePath(resource);

            if (resource.ContentArrayBytes != null)
            {
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);
                if (!File.Exists(file))
                    File.WriteAllBytes(file, resource.ContentArrayBytes);
            }

            if (forceUpdateDto)
            {
                if (!resource.ContentInDB)
                    resource.ContentArrayBytes = File.ReadAllBytes(file);

                (resource.RelativeUrl, resource.FullUrl) = UtilGetResourceUrls(resource);
            }
        }
        catch (Exception ex)
        {
            // TODO: anotate this meesage in log, and throw exception !!! ???
            var errMsg = ex.ToString();
            return false;
        }

        return true;
    }

    public string UtilGetResourcePath(ResourceDto resource)
    {        
        string rootPath = Repository.RespositoryRef.ResourcesContainerRootPath;
        string relativePath;
        string fullPath;

        if (string.IsNullOrEmpty(rootPath) || string.IsNullOrEmpty(resource.Container) || string.IsNullOrEmpty(resource.Name))
            return null;

        relativePath = Path.Combine(resource.Container, resource.Name);
        fullPath = Path.Combine(rootPath, relativePath);

        return fullPath;
    }

    public string UtilUpdateResourceInDescriptionForRead(string description, bool considerRootPath = false)
    {
        var replaceString = UtilGetReplaceResourceString(considerRootPath);

        if (replaceString == null)
            return description;
        return description?
            .Replace(Repository.RespositoryRef.ResourcesContainer, replaceString).Replace(Path.DirectorySeparatorChar, '/');
    }

    public string UtilUpdateResourceInDescriptionForWrite(string description, bool considerRootPath = false)
    {
        var replaceString = UtilGetReplaceResourceString(considerRootPath);

        if (replaceString == null)
            return description;
        else
            return description?
                .Replace(replaceString, Repository.RespositoryRef.ResourcesContainer);
    }

    private string UtilGetReplaceResourceString(bool considerRootPath = false)
    {
        if (Repository.RespositoryRef == null || string.IsNullOrEmpty(Repository.RespositoryRef?.ResourcesContainer))
            return null;

        string replaceString = null;

        if (!string.IsNullOrEmpty(Repository.RespositoryRef?.ResourcesContainerRootUrl))
        {
            replaceString = Path.Combine(Repository.RespositoryRef?.ResourcesContainerRootUrl, Repository.RespositoryRef?.ResourcesContainer);
            // Replace @"\" with @"/"
            replaceString = replaceString.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        else
        {
            if (considerRootPath)
            {
                if (!string.IsNullOrEmpty(Repository.RespositoryRef?.ResourcesContainerRootPath))
                    replaceString = Path.Combine(Repository.RespositoryRef?.ResourcesContainerRootPath, Repository.RespositoryRef?.ResourcesContainer);
            }
        }
        
        return replaceString;                
    }

    #endregion
}
