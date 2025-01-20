using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model.Dto;
using KNote.Model;
using KNote.Service.Core;

namespace KNote.Service.Interfaces;

public interface IKntNoteService
{
    Task<Result<List<NoteInfoDto>>> GetAllAsync();
    Task<Result<List<NoteInfoDto>>> HomeNotesAsync();
    Task<Result<NoteDto>> GetAsync(Guid noteId);
    Task<Result<NoteDto>> GetAsync(int noteNumber);
    Task<Result<NoteExtendedDto>> GetExtendedAsync(Guid noteId);
    Task<Result<List<NoteInfoDto>>> GetByFolderAsync(Guid folderId);
    Task<Result<List<NoteInfoDto>>> GetFilter(NotesFilterDto notesFilter);
    Task<Result<List<NoteInfoDto>>> GetSearch(NotesSearchDto notesSearch);
    Task<Result<NoteDto>> NewAsync(NoteInfoDto entity = null);
    Task<Result<NoteExtendedDto>> NewExtendedAsync(NoteInfoDto entity = null);
    Task<Result<NoteDto>> SaveAsync(NoteDto entity, bool updateStatus = true);
    Task<Result<NoteExtendedDto>> SaveExtendedAsync(NoteExtendedDto entity);       
    Task<Result<NoteDto>> DeleteAsync(Guid noteId);
    Task<Result<NoteExtendedDto>> DeleteExtendedAsync(Guid noteId);
    Task<Result<List<ResourceDto>>> GetResourcesAsync(Guid noteId);
    Task<Result<List<ResourceInfoDto>>> GetResourcesInfoAsync(Guid noteId);
    Task<Result<ResourceDto>> GetResourceAsync(Guid resourceId);
    Task<Result<ResourceDto>> SaveResourceAsync(ResourceDto entity, bool forceNew = false);
    Task<Result<ResourceInfoDto>> SaveResourceAsync(ResourceInfoDto resourceInfo, bool forceNew = false);
    Task<Result<ResourceDto>> DeleteResourceAsync(Guid resourceId);
    Task<Result<ResourceInfoDto>> DeleteResourceInfoAsync(Guid id);
    Task<Result<List<NoteTaskDto>>> GetNoteTasksAsync(Guid noteId);
    Task<Result<List<NoteTaskDto>>> GetStartedTasksByDateTimeRageAsync(DateTime startDateTime, DateTime endDateTime);
    Task<Result<List<NoteTaskDto>>> GetEstimatedTasksByDateTimeRageAsync(DateTime startDateTime, DateTime endDateTime);
    Task<Result<NoteTaskDto>> GetNoteTaskAsync(Guid noteTaskId);
    Task<Result<NoteTaskDto>> SaveNoteTaskAsync(NoteTaskDto entityInfo, bool forceNew = false);
    Task<Result<NoteTaskDto>> DeleteNoteTaskAsync(Guid noteTaskId);
    Task<Result<List<KMessageDto>>> GetMessagesAsync(Guid noteId);
    Task<Result<KMessageDto>> GetMessageAsync(Guid messageId);
    Task<Result<KMessageDto>> SaveMessageAsync(KMessageDto entity, bool forceNew = false);
    Task<Result<KMessageDto>> DeleteMessageAsync(Guid messageId);
    Task<Result<WindowDto>> GetWindowAsync(Guid noteId, Guid userId);
    Task<Result<WindowDto>> SaveWindowAsync(WindowDto entity, bool forceNew = false);
    Task<Result<List<Guid>>> GetVisibleNotesIdAsync(string userName);
    Task<Result<List<Guid>>> GetAlarmNotesIdAsync(string userName, EnumNotificationType? notificationType = null);

    #region Utils 
    Task<Result<bool>> UtilPatchFolder(Guid noteId, Guid folderId);
    Task<Result<bool>> UtilPatchChangeTags(Guid noteId, string oldTag, string newTag);        
    Task<List<NoteKAttributeDto>> UtilCompleteNoteAttributes(List<NoteKAttributeDto> attributesNotes, Guid noteId, Guid? noteTypeId = null);
    string UtilGetNoteStatus(List<NoteTaskDto> tasks, List<KMessageDto> messages);
    (string, string) UtilGetResourceUrls(ResourceDto resource);
    bool UtilManageResourceContent(ResourceDto resource, bool forceUpdateDto = true);
    string UtilGetResourcePath(ResourceDto resource);
    string UtilUpdateResourceInDescriptionForRead(string description, ReplacementType replacementType, bool considerRootPath = false);
    string UtilUpdateResourceInDescriptionForWrite(string description, ReplacementType replacementType, bool considerRootPath = false);

    #endregion
}
