using KNote.ClientWin.Components;
using KNote.Model;
using KNote.Model.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace KNote.ClientWin.Core
{
    public interface IFactoryViews
    {
        IViewConfigurable View(MonitorComponent component);
        IViewConfigurable View(KntScriptConsoleComponent component);
        ISelectorView<FolderWithServiceRef> View(FoldersSelectorComponent component);
        ISelectorView<NoteInfoDto> View(NotesSelectorComponent component);
        IViewConfigurableExt View(KNoteManagmentComponent component);
        IEditorView<NoteExtendedDto> View(NoteEditorComponent component);
        IEditorViewExt<NoteDto> View(PostItEditorComponent component);
        IEditorView<WindowDto> View(PostItPropertiesComponent component);
        IEditorView<FolderDto> View(FolderEditorComponent component);        
        IEditorView<KMessageDto> View(MessageEditorComponent component);        
        IEditorView<ResourceDto> View(ResourceEditorComponent component);        
        IEditorView<NoteTaskDto> View(TaskEditorComponent component);
        ISelectorView<NoteTypeDto> View(NoteTypesSelectorComponent component);
        IEditorView<NoteKAttributeDto> View(NoteAttributeEditorComponent component);
        IViewBase NotifyView(KNoteManagmentComponent component);
        IViewBase AboutView(KNoteManagmentComponent component);
        IEditorView<KAttributeDto> View(AttributeEditorComponent component);
        ISelectorView<NotesFilterWithServiceRef> View(FiltersSelectorComponent component);
        IEditorView<RepositoryRef> View(RepositoryEditorComponent component);
        IEditorView<AppConfig> View(OptionsEditorComponent component);
    }
}
