using KNote.ClientWin.Controllers;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Core;

public interface IFactoryViews
{
    IViewBase View(MonitorCtrl component);
    IViewBase View(KntScriptConsoleCtrl component);
    IViewSelector<FolderWithServiceRef> View(FoldersSelectorCtrl component);
    IViewSelector<NoteInfoDto> View(NotesSelectorCtrl component);
    IViewKNoteManagment View(KNoteManagmentCtrl component);
    IViewEditorEmbeddable<NoteExtendedDto> View(NoteEditorCtrl component);
    IViewPostIt<NoteDto> View(PostItEditorCtrl component);
    IViewPostIt<WindowDto> View(PostItPropertiesCtrl component);
    IViewEditor<FolderDto> View(FolderEditorCtrl component);        
    IViewEditor<KMessageDto> View(MessageEditorCtrl component);        
    IViewEditor<ResourceDto> View(ResourceEditorCtrl component);        
    IViewEditor<NoteTaskDto> View(TaskEditorCtrl component);
    IViewSelector<NoteTypeDto> View(NoteTypesSelectorCtrl component);
    IViewEditor<NoteKAttributeDto> View(NoteAttributeEditorCtrl component);
    IViewBase NotifyView(KNoteManagmentCtrl component);
    IViewBase AboutView(KNoteManagmentCtrl component);
    IViewEditor<KAttributeDto> View(AttributeEditorCtrl component);
    IViewSelector<SelectedNotesInServiceRef> View(FiltersSelectorCtrl component);
    IViewEditor<RepositoryRef> View(RepositoryEditorCtrl component);
    IViewEditor<AppConfig> View(OptionsEditorCtrl component);
    IViewBase View(KntChatGPTCtrl component);
    IViewChat View(KntChatCtrl component);
    IViewServerCOM View(KntServerCOMCtrl component);
    IViewBase View(KntLabCtrl component);
    IViewHeavyProcess View(HeavyProcessCtrl component);
}
