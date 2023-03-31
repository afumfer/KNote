using KNote.ClientWin.Components;
using KNote.ClientWin.Views;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Core;

public class FactoryViewsWinForms : IFactoryViews
{
    public IViewBase View(MonitorComponent component)
    {
        return new MonitorForm(component);
    }

    public IViewBase View(KntScriptConsoleComponent component)
    {
        return new KntScriptConsoleForm(component);
    }
    
    public IViewSelector<FolderWithServiceRef> View(FoldersSelectorComponent component)
    {
        return new FoldersSelectorForm(component);
    }

    public IViewSelector<NoteInfoDto> View(NotesSelectorComponent component)
    {
        return new NotesSelectorForm(component);
    }

    public IViewKNoteManagment View(KNoteManagmentComponent component)
    {
        return new KNoteManagmentForm(component);
    }

    public IViewEditorEmbeddable<NoteExtendedDto> View(NoteEditorComponent component)
    {
        return new NoteEditorForm(component);
    }

    public IViewPostIt<NoteDto> View(PostItEditorComponent component)
    {
        return new PostItEditorForm(component);
    }

    public IViewPostIt<WindowDto> View(PostItPropertiesComponent component)
    {
        return new PostItPropertiesForm(component);
    }

    public IViewEditor<FolderDto> View(FolderEditorComponent component)
    {
        return new FolderEditorForm(component);
    }

    public IViewEditor<RepositoryRef> View(RepositoryEditorComponent component)
    {
        return new RepositoryEditorForm(component);
    }

    public IViewBase NotifyView(KNoteManagmentComponent component)
    {
        return new NotifyForm(component);
    }

    public IViewBase AboutView(KNoteManagmentComponent component)
    {
        return new KNoteAboutForm(component);
    }

    public IViewEditor<KMessageDto> View(MessageEditorComponent component)
    {
        return new MessageEditorForm(component);
    }

    public IViewEditor<ResourceDto> View(ResourceEditorComponent component)
    {
        return new ResourceEditorForm(component);
    }

    public IViewEditor<KAttributeDto> View(AttributeEditorComponent component)
    {
        return new AttributeEditorForm(component);
    }

    public IViewEditor<NoteTaskDto> View(TaskEditorComponent component)
    {
        return new TaskEditorForm(component);
    }

    public IViewSelector<NoteTypeDto> View(NoteTypesSelectorComponent component)
    {
        return new NoteTypesSelectorForm(component);
    }

    public IViewEditor<NoteKAttributeDto> View(NoteAttributeEditorComponent component)
    {
        return new NoteAttributeEditorForm(component);
    }

    public IViewSelector<NotesFilterWithServiceRef> View(FiltersSelectorComponent component)
    {
        return new FiltersSelectorForm(component);
    }

    public IViewEditor<AppConfig> View(OptionsEditorComponent component)
    {
        return new OptionsEditorForm(component);
    }

    public IViewBase View(KntChatGPTComponent component)
    {
        return new KntChatGPTForm(component);
    }

    public IViewBase View(KntChatComponent component)
    {
        return new KntChatForm(component);
    }
}
