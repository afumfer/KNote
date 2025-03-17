using KNote.ClientWin.Controllers;
using KNote.ClientWin.Views;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Core;

public class FactoryViewsWinForms : IFactoryViews
{
    public IViewBase View(MonitorCtrl component)
    {
        return new MonitorForm(component);
    }

    public IViewBase View(KntScriptConsoleCtrl component)
    {
        return new KntScriptConsoleForm(component);
    }
    
    public IViewSelector<FolderWithServiceRef> View(FoldersSelectorCtrl component)
    {
        return new FoldersSelectorForm(component);
    }

    public IViewSelector<NoteInfoDto> View(NotesSelectorCtrl component)
    {
        return new NotesSelectorForm(component);
    }

    public IViewKNoteManagment View(KNoteManagmentCtrl component)
    {
        return new KNoteManagmentForm(component);
    }

    public IViewEditorEmbeddable<NoteExtendedDto> View(NoteEditorCtrl component)
    {
        return new NoteEditorForm(component);
    }

    public IViewPostIt<NoteDto> View(PostItEditorCtrl component)
    {
        return new PostItEditorForm(component);
    }

    public IViewPostIt<WindowDto> View(PostItPropertiesCtrl component)
    {
        return new PostItPropertiesForm(component);
    }

    public IViewEditor<FolderDto> View(FolderEditorCtrl component)
    {
        return new FolderEditorForm(component);
    }

    public IViewEditor<RepositoryRef> View(RepositoryEditorCtrl component)
    {
        return new RepositoryEditorForm(component);
    }

    public IViewBase NotifyView(KNoteManagmentCtrl component)
    {
        return new NotifyForm(component);
    }

    public IViewBase AboutView(KNoteManagmentCtrl component)
    {
        return new KNoteAboutForm(component);
    }

    public IViewEditor<KMessageDto> View(MessageEditorCtrl component)
    {
        return new MessageEditorForm(component);
    }

    public IViewEditor<ResourceDto> View(ResourceEditorCtrl component)
    {
        return new ResourceEditorForm(component);
    }

    public IViewEditor<KAttributeDto> View(AttributeEditorCtrl component)
    {
        return new AttributeEditorForm(component);
    }

    public IViewEditor<NoteTaskDto> View(TaskEditorCtrl component)
    {
        return new TaskEditorForm(component);
    }

    public IViewSelector<NoteTypeDto> View(NoteTypesSelectorCtrl component)
    {
        return new NoteTypesSelectorForm(component);
    }

    public IViewEditor<NoteKAttributeDto> View(NoteAttributeEditorCtrl component)
    {
        return new NoteAttributeEditorForm(component);
    }

    public IViewSelector<SelectedNotesInServiceRef> View(FiltersSelectorCtrl component)
    {
        return new FiltersSelectorForm(component);
    }

    public IViewEditor<AppConfig> View(OptionsEditorCtrl component)
    {
        return new OptionsEditorForm(component);
    }

    public IViewBase View(KntChatGPTCtrl component)
    {
        return new KntChatGPTForm(component);
    }

    public IViewChat View(KntChatCtrl component)
    {
        return new KntChatForm(component);
    }

    public IViewServerCOM View(KntServerCOMCtrl component)
    {
        return new KntServerCOMForm(component);
    }

    public IViewBase View(KntLabCtrl component)
    {
        return new KntLabForm(component);
    }

    public IViewHeavyProcess View(HeavyProcessCtrl component)
    {
        return new HeavyProcessForm(component);
    }
}
