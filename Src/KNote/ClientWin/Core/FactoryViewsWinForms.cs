using KNote.ClientWin.Controllers;
using KNote.ClientWin.Views;
using KNote.Model;
using KNote.Model.Dto;
using System.ComponentModel;

namespace KNote.ClientWin.Core;

public class FactoryViewsWinForms : IFactoryViews
{
    public IViewBase View(MonitorCtrl controller)
    {
        return new MonitorForm(controller);
    }

    public IViewBase View(KntScriptConsoleCtrl controller)
    {
        return new KntScriptConsoleForm(controller);
    }
    
    public IViewSelector<FolderWithServiceRef> View(FoldersSelectorCtrl controller)
    {
        return new FoldersSelectorForm(controller);
    }

    public IViewSelector<NoteMinimalDto> View(NotesSelectorCtrl controller)
    {
        return new NotesSelectorForm(controller);
    }

    public IViewKNoteManagment View(KNoteManagmentCtrl controller)
    {
        return new KNoteManagmentForm(controller);
    }

    public IViewEditorEmbeddable<NoteExtendedDto> View(NoteEditorCtrl controller)
    {
        return new NoteEditorForm(controller);
    }

    public IViewPostIt<NoteDto> View(PostItEditorCtrl controller)
    {
        return new PostItEditorForm(controller);
    }

    public IViewPostIt<WindowDto> View(PostItPropertiesCtrl controller)
    {
        return new PostItPropertiesForm(controller);
    }

    public IViewEditor<FolderDto> View(FolderEditorCtrl controller)
    {
        return new FolderEditorForm(controller);
    }

    public IViewEditor<RepositoryRef> View(RepositoryEditorCtrl controller)
    {
        return new RepositoryEditorForm(controller);
    }

    public IViewBase NotifyView(KNoteManagmentCtrl controller)
    {
        return new NotifyForm(controller);
    }

    public IViewBase AboutView(KNoteManagmentCtrl controller)
    {
        return new KNoteAboutForm(controller);
    }

    public IViewEditor<KMessageDto> View(MessageEditorCtrl controller)
    {
        return new MessageEditorForm(controller);
    }

    public IViewEditor<ResourceDto> View(ResourceEditorCtrl controller)
    {
        return new ResourceEditorForm(controller);
    }

    public IViewEditor<KAttributeDto> View(AttributeEditorCtrl controller)
    {
        return new AttributeEditorForm(controller);
    }

    public IViewEditor<NoteTaskDto> View(TaskEditorCtrl controller)
    {
        return new TaskEditorForm(controller);
    }

    public IViewSelector<NoteTypeDto> View(NoteTypesSelectorCtrl controller)
    {
        return new NoteTypesSelectorForm(controller);
    }

    public IViewEditor<NoteKAttributeDto> View(NoteAttributeEditorCtrl controller)
    {
        return new NoteAttributeEditorForm(controller);
    }

    public IViewSelector<SelectedNotesInServiceRef> View(FiltersSelectorCtrl controller)
    {
        return new FiltersSelectorForm(controller);
    }

    public IViewEditor<AppConfig> View(OptionsEditorCtrl controller)
    {
        return new OptionsEditorForm(controller);
    }

    public IViewBase View(KntChatGPTCtrl controller)
    {
        return new KntChatGPTForm(controller);
    }

    public IViewChat View(KntChatCtrl controller)
    {
        return new KntChatForm(controller);
    }

    public IViewServerCOM View(KntServerCOMCtrl controller)
    {
        return new KntServerCOMForm(controller);
    }

    public IViewBase View(KntLabCtrl controller)
    {
        return new KntLabForm(controller);
    }
    
    public IViewHeavyProcess View(HeavyProcessCtrl controller)
    {
        return new HeavyProcessForm(controller);
    }
}
