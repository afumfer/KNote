using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KNote.Service.Interfaces;

namespace KNote.ClientWin.Tests.Fakes;

/// <summary>
/// Minimal IKntNoteService test double: only the members exercised by the tests have a working
/// implementation (via settable delegates); everything else throws, so an unexpectedly-touched
/// member fails loudly instead of silently returning a default value.
/// </summary>
internal class FakeKntNoteService : IKntNoteService
{
    public Func<NoteExtendedDto, Task<Result<NoteExtendedDto>>>? SaveExtendedAsyncImpl { get; set; }
    public Func<Guid, Task<Result<NoteExtendedDto>>>? DeleteExtendedAsyncImpl { get; set; }
    public Func<Guid, Task<Result<NoteExtendedDto>>>? GetExtendedAsyncImpl { get; set; }
    public Func<NoteTaskDto, bool, Task<Result<NoteTaskDto>>>? SaveNoteTaskAsyncImpl { get; set; }
    public Func<NotesSearchDto, Task<Result<List<NoteMinimalDto>>>>? GetSearchMinimalAsyncImpl { get; set; }
    public Func<Guid, Task<Result<NoteDto>>>? GetByIdAsyncImpl { get; set; }
    public Func<int, Task<Result<NoteDto>>>? GetByNumberAsyncImpl { get; set; }

    public Task<Result<NoteExtendedDto>> SaveExtendedAsync(NoteExtendedDto entity) =>
        (SaveExtendedAsyncImpl ?? throw new NotSupportedException($"{nameof(SaveExtendedAsync)} not configured for this test"))(entity);

    public Task<Result<NoteExtendedDto>> DeleteExtendedAsync(Guid noteId) =>
        (DeleteExtendedAsyncImpl ?? throw new NotSupportedException($"{nameof(DeleteExtendedAsync)} not configured for this test"))(noteId);

    public Task<Result<NoteTaskDto>> SaveNoteTaskAsync(NoteTaskDto entityInfo, bool forceNew = false) =>
        (SaveNoteTaskAsyncImpl ?? throw new NotSupportedException($"{nameof(SaveNoteTaskAsync)} not configured for this test"))(entityInfo, forceNew);

    public Task<Result<List<NoteInfoDto>>> GetAllAsync() => throw new NotSupportedException();
    public Task<Result<List<NoteMinimalDto>>> GetAllMinimalAsync() => throw new NotSupportedException();
    public Task<Result<List<NoteInfoDto>>> HomeNotesAsync() => throw new NotSupportedException();
    public Task<Result<NoteDto>> GetAsync(Guid noteId) =>
        (GetByIdAsyncImpl ?? throw new NotSupportedException($"{nameof(GetAsync)}(Guid) not configured for this test"))(noteId);
    public Task<Result<NoteDto>> GetAsync(int noteNumber) =>
        (GetByNumberAsyncImpl ?? throw new NotSupportedException($"{nameof(GetAsync)}(int) not configured for this test"))(noteNumber);
    public Task<Result<NoteExtendedDto>> GetExtendedAsync(Guid noteId) =>
        (GetExtendedAsyncImpl ?? throw new NotSupportedException($"{nameof(GetExtendedAsync)} not configured for this test"))(noteId);
    public Task<Result<List<NoteInfoDto>>> GetByFolderAsync(Guid folderId) => throw new NotSupportedException();
    public Task<Result<List<NoteMinimalDto>>> GetByFolderMinimalAsync(Guid folderId) => throw new NotSupportedException();
    public Task<Result<List<NoteInfoDto>>> GetFilterAsync(NotesFilterDto notesFilter) => throw new NotSupportedException();
    public Task<Result<List<NoteMinimalDto>>> GetFilterMinimalAsync(NotesFilterDto notesFilter) => throw new NotSupportedException();
    public Task<Result<List<NoteInfoDto>>> GetSearchAsync(NotesSearchDto notesSearch) => throw new NotSupportedException();
    public Task<Result<List<NoteMinimalDto>>> GetSearchMinimalAsync(NotesSearchDto notesSearch) =>
        (GetSearchMinimalAsyncImpl ?? throw new NotSupportedException($"{nameof(GetSearchMinimalAsync)} not configured for this test"))(notesSearch);
    public Task<Result<NoteDto>> NewAsync(NoteInfoDto entity = null) => throw new NotSupportedException();
    public Task<Result<NoteExtendedDto>> NewExtendedAsync(NoteInfoDto entity = null) => throw new NotSupportedException();
    public Task<Result<NoteDto>> SaveAsync(NoteDto entity, bool updateStatus = true) => throw new NotSupportedException();
    public Task<Result<NoteDto>> DeleteAsync(Guid noteId) => throw new NotSupportedException();
    public Task<Result<List<ResourceDto>>> GetResourcesAsync(Guid noteId) => throw new NotSupportedException();
    public Task<Result<List<ResourceInfoDto>>> GetResourcesInfoAsync(Guid noteId) => throw new NotSupportedException();
    public Task<Result<ResourceDto>> GetResourceAsync(Guid resourceId) => throw new NotSupportedException();
    public Task<Result<ResourceDto>> SaveResourceAsync(ResourceDto entity, bool forceNew = false) => throw new NotSupportedException();
    public Task<Result<ResourceInfoDto>> SaveResourceAsync(ResourceInfoDto resourceInfo, bool forceNew = false) => throw new NotSupportedException();
    public Task<Result<ResourceDto>> DeleteResourceAsync(Guid resourceId) => throw new NotSupportedException();
    public Task<Result<ResourceInfoDto>> DeleteResourceInfoAsync(Guid id) => throw new NotSupportedException();
    public Task<Result<List<NoteTaskDto>>> GetNoteTasksAsync(Guid noteId) => throw new NotSupportedException();
    public Task<Result<List<NoteTaskDto>>> GetStartedTasksByDateTimeRageAsync(DateTime startDateTime, DateTime endDateTime) => throw new NotSupportedException();
    public Task<Result<List<NoteTaskDto>>> GetEstimatedTasksByDateTimeRageAsync(DateTime startDateTime, DateTime endDateTime) => throw new NotSupportedException();
    public Task<Result<NoteTaskDto>> GetNoteTaskAsync(Guid noteTaskId) => throw new NotSupportedException();
    public Task<Result<NoteTaskDto>> DeleteNoteTaskAsync(Guid noteTaskId) => throw new NotSupportedException();
    public Task<Result<List<KMessageDto>>> GetMessagesAsync(Guid noteId) => throw new NotSupportedException();
    public Task<Result<KMessageDto>> GetMessageAsync(Guid messageId) => throw new NotSupportedException();
    public Task<Result<KMessageDto>> SaveMessageAsync(KMessageDto entity, bool forceNew = false) => throw new NotSupportedException();
    public Task<Result<KMessageDto>> DeleteMessageAsync(Guid messageId) => throw new NotSupportedException();
    public Task<Result<WindowDto>> GetWindowAsync(Guid noteId, Guid userId) => throw new NotSupportedException();
    public Task<Result<WindowDto>> SaveWindowAsync(WindowDto entity, bool forceNew = false) => throw new NotSupportedException();
    public Task<Result<List<Guid>>> GetVisibleNotesIdAsync(string userName) => throw new NotSupportedException();
    public Task<Result<List<Guid>>> GetAlarmNotesIdAsync(string userName, EnumNotificationType? notificationType = null) => throw new NotSupportedException();
    public Task<Result<bool>> UtilPatchFolderAsync(Guid noteId, Guid folderId) => throw new NotSupportedException();
    public Task<Result<bool>> UtilPatchChangeTagsAsync(Guid noteId, string oldTag, string newTag) => throw new NotSupportedException();
    public Task<List<NoteKAttributeDto>> UtilCompleteNoteAttributesAsync(List<NoteKAttributeDto> attributesNotes, Guid noteId, Guid? noteTypeId = null) => throw new NotSupportedException();
    public string UtilGetNoteStatus(List<NoteTaskDto> tasks, List<KMessageDto> messages) => throw new NotSupportedException();
    public (string, string) UtilGetResourceUrls(ResourceDto resource) => throw new NotSupportedException();
    public bool UtilManageResourceContent(ResourceDto resource, bool forceUpdateDto = true) => throw new NotSupportedException();
    public string UtilGetResourceFilePath(ResourceDto resource) => throw new NotSupportedException();
    public string UtilGetResourceFileUrl(string container, string fileName) => throw new NotSupportedException();
    public string UtilGetDefaultNewResourceContainer() => throw new NotSupportedException();
    public string UtilUpdateResourceInDescriptionForRead(string description, bool considerRootPath = false) => throw new NotSupportedException();
    public string UtilUpdateResourceInDescriptionForWrite(string description, bool considerRootPath = false) => throw new NotSupportedException();
    public string UtilHtmlToMarkdown(string html) => throw new NotSupportedException();
    public string UtilMarkdownToHtml(string markdown) => throw new NotSupportedException();
}
