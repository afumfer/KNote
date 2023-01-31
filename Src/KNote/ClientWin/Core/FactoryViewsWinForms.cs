using KNote.ClientWin.Components;
using KNote.ClientWin.Views;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Core;

public class FactoryViewsWinForms : IFactoryViews
{
    public IViewConfigurable View(MonitorComponent component)
    {
        return new MonitorForm(component);
    }

    public IViewConfigurable View(KntScriptConsoleComponent component)
    {
        return new KntScriptConsoleForm(component);
    }
    
    public ISelectorView<FolderWithServiceRef> View(FoldersSelectorComponent component)
    {
        return new FoldersSelectorForm(component);
    }

    public ISelectorView<NoteInfoDto> View(NotesSelectorComponent component)
    {
        return new NotesSelectorForm(component);
    }

    public IViewConfigurableExt View(KNoteManagmentComponent component)
    {
        return new KNoteManagmentForm(component);
    }

    public IEditorView<NoteExtendedDto> View(NoteEditorComponent component)
    {
        return new NoteEditorForm(component);
    }

    public IEditorViewExt<NoteDto> View(PostItEditorComponent component)
    {
        return new PostItEditorForm(component);
    }

    public IEditorView<WindowDto> View(PostItPropertiesComponent component)
    {
        return new PostItPropertiesForm(component);
    }

    public IEditorView<FolderDto> View(FolderEditorComponent component)
    {
        return new FolderEditorForm(component);
    }

    public IEditorView<RepositoryRef> View(RepositoryEditorComponent component)
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

    public IEditorView<KMessageDto> View(MessageEditorComponent component)
    {
        return new MessageEditorForm(component);
    }

    public IEditorView<ResourceDto> View(ResourceEditorComponent component)
    {
        return new ResourceEditorForm(component);
    }

    public IEditorView<KAttributeDto> View(AttributeEditorComponent component)
    {
        return new AttributeEditorForm(component);
    }

    public IEditorView<NoteTaskDto> View(TaskEditorComponent component)
    {
        return new TaskEditorForm(component);
    }

    public ISelectorView<NoteTypeDto> View(NoteTypesSelectorComponent component)
    {
        return new NoteTypesSelectorForm(component);
    }

    public IEditorView<NoteKAttributeDto> View(NoteAttributeEditorComponent component)
    {
        return new NoteAttributeEditorForm(component);
    }

    public ISelectorView<NotesFilterWithServiceRef> View(FiltersSelectorComponent component)
    {
        return new FiltersSelectorForm(component);
    }

    public IEditorView<AppConfig> View(OptionsEditorComponent component)
    {
        return new OptionsEditorForm(component);
    }
}
