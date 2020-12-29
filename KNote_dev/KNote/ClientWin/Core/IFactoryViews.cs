using KNote.ClientWin.Components;
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
        IViewConfigurable View(KNoteManagmentComponent component);
        IEditorView<NoteExtendedDto> View(NoteEditorComponent component);
        IEditorView<FolderDto> View(FolderEditorComponent component);        
        IEditorView<KMessageDto> View(MessageEditorComponent component);        
        IEditorView<ResourceDto> View(ResourceEditorComponent component);
        IEditorView<KAttributeDto> View(AttributeEditorComponent component);
        IEditorView<NoteTaskDto> View(TaskEditorComponent component);
        ISelectorView<NoteTypeDto> View(NoteTypesSelectorComponent component);

        IViewBase NotifyView(KNoteManagmentComponent component);
    }
}
