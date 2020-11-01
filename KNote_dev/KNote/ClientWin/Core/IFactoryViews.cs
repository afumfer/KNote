using KNote.ClientWin.Components;
using KNote.Model.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace KNote.ClientWin.Core
{
    public interface IFactoryViews
    {
        IViewBase View(MonitorComponent component);
        ISelectorView<FolderWithServiceRef> View(FolderSelectorComponent component);
        ISelectorView<NoteInfoDto> View(NotesSelectorComponent component);
        IViewBase View(KNoteManagmentComponent component);
        IEditorView<NoteDto> View(NoteEditorComponent component);
    }
}
