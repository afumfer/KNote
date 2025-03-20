using KNote.ClientWin.Controllers;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Core;

public interface IFactoryViews
{
    IViewBase View(MonitorCtrl controller);
    IViewBase View(KntScriptConsoleCtrl controller);
    IViewSelector<FolderWithServiceRef> View(FoldersSelectorCtrl controller);
    IViewSelector<NoteInfoDto> View(NotesSelectorCtrl controller);
    IViewKNoteManagment View(KNoteManagmentCtrl controller);
    IViewEditorEmbeddable<NoteExtendedDto> View(NoteEditorCtrl controller);
    IViewPostIt<NoteDto> View(PostItEditorCtrl controller);
    IViewPostIt<WindowDto> View(PostItPropertiesCtrl controller);
    IViewEditor<FolderDto> View(FolderEditorCtrl controller);        
    IViewEditor<KMessageDto> View(MessageEditorCtrl controller);        
    IViewEditor<ResourceDto> View(ResourceEditorCtrl controller);        
    IViewEditor<NoteTaskDto> View(TaskEditorCtrl controller);
    IViewSelector<NoteTypeDto> View(NoteTypesSelectorCtrl controller);
    IViewEditor<NoteKAttributeDto> View(NoteAttributeEditorCtrl controller);
    IViewBase NotifyView(KNoteManagmentCtrl controller);
    IViewBase AboutView(KNoteManagmentCtrl controller);
    IViewEditor<KAttributeDto> View(AttributeEditorCtrl controller);
    IViewSelector<SelectedNotesInServiceRef> View(FiltersSelectorCtrl controller);
    IViewEditor<RepositoryRef> View(RepositoryEditorCtrl controller);
    IViewEditor<AppConfig> View(OptionsEditorCtrl controller);
    IViewBase View(KntChatGPTCtrl controller);
    IViewChat View(KntChatCtrl controller);
    IViewServerCOM View(KntServerCOMCtrl controller);
    IViewBase View(KntLabCtrl controller);
    IViewHeavyProcess View(HeavyProcessCtrl controller);
}
