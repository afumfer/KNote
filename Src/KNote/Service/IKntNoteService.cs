using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KNote.Model.Dto;
using KNote.Model;

namespace KNote.Service
{
    public interface IKntNoteService
    {
        Task<Result<List<NoteInfoDto>>> GetAllAsync();
        Task<Result<List<NoteInfoDto>>> HomeNotesAsync();
        Task<Result<NoteDto>> GetAsync(Guid noteId);
        Task<Result<NoteDto>> GetAsync(int noteNumber);
        Task<Result<NoteExtendedDto>> GetExtendedAsync(Guid noteId);
        Task <Result<List<NoteInfoDto>>> GetByFolderAsync(Guid folderId);
        Task<Result<List<NoteInfoDto>>> GetFilter(NotesFilterDto notesFilter);
        Task<Result<List<NoteInfoDto>>> GetSearch(NotesSearchDto notesSearch);                        
        Task<Result<NoteDto>> NewAsync(NoteInfoDto entity = null);
        Task<Result<NoteExtendedDto>> NewExtendedAsync(NoteInfoDto entity = null);
        Task<Result<NoteDto>> SaveAsync(NoteDto entity, bool updateStatus = true);
        Task<Result<NoteExtendedDto>> SaveExtendedAsync(NoteExtendedDto entity);
        Task<List<NoteKAttributeDto>> CompleteNoteAttributes(List<NoteKAttributeDto> attributesNotes, Guid noteId, Guid? noteTypeId = null);
        Task<Result<NoteDto>> DeleteAsync(Guid noteId);
        Task<Result<NoteExtendedDto>> DeleteExtendedAsync(Guid noteId);
        Task<Result<List<ResourceDto>>> GetResourcesAsync(Guid noteId);
        Task<Result<ResourceDto>> GetResourceAsync(Guid resourceId); 
        Task<Result<ResourceDto>> SaveResourceAsync(ResourceDto entity, bool forceNew = false);
        bool SaveResourceFileAndRefreshDto(ResourceDto resource, byte[] arrayContent);
        (string, string) GetResourceUrls(ResourceDto resource);
        string GetResourcePath(ResourceDto resource);
        Task<Result<ResourceDto>> DeleteResourceAsync(Guid resourceId);
        Task<Result<List<NoteTaskDto>>> GetNoteTasksAsync(Guid noteId);
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
        Task<Result<bool>> PatchFolder(Guid noteId, Guid folderId);
        Task<Result<bool>> PatchChangeTags(Guid noteId, string oldTag, string newTag);
    }
}
