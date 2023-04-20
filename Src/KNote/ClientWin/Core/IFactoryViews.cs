using KNote.ClientWin.Components;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Core;

public interface IFactoryViews
{
    IViewBase View(MonitorComponent component);
    IViewBase View(KntScriptConsoleComponent component);
    IViewSelector<FolderWithServiceRef> View(FoldersSelectorComponent component);
    IViewSelector<NoteInfoDto> View(NotesSelectorComponent component);
    IViewKNoteManagment View(KNoteManagmentComponent component);
    IViewEditorEmbeddable<NoteExtendedDto> View(NoteEditorComponent component);
    IViewPostIt<NoteDto> View(PostItEditorComponent component);
    IViewPostIt<WindowDto> View(PostItPropertiesComponent component);
    IViewEditor<FolderDto> View(FolderEditorComponent component);        
    IViewEditor<KMessageDto> View(MessageEditorComponent component);        
    IViewEditor<ResourceDto> View(ResourceEditorComponent component);        
    IViewEditor<NoteTaskDto> View(TaskEditorComponent component);
    IViewSelector<NoteTypeDto> View(NoteTypesSelectorComponent component);
    IViewEditor<NoteKAttributeDto> View(NoteAttributeEditorComponent component);
    IViewBase NotifyView(KNoteManagmentComponent component);
    IViewBase AboutView(KNoteManagmentComponent component);
    IViewEditor<KAttributeDto> View(AttributeEditorComponent component);
    IViewSelector<NotesFilterWithServiceRef> View(FiltersSelectorComponent component);
    IViewEditor<RepositoryRef> View(RepositoryEditorComponent component);
    IViewEditor<AppConfig> View(OptionsEditorComponent component);
    IViewBase View(KntChatGPTComponent component);
    IViewChat View(KntChatComponent component);
    IViewChat View(KntServerCOMComponent component);
    IViewBase View(KntLabComponent component);
}
