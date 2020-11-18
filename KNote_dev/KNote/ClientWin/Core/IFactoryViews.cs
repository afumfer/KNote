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
        IEditorView<NoteDto> View(NoteEditorComponent component);
        IViewBase NotifyView(KNoteManagmentComponent component);
    }
}
