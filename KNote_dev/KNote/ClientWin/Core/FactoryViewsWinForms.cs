using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KNote.ClientWin.Components;
using KNote.ClientWin.Views;
using KNote.Model.Dto;

namespace KNote.ClientWin.Core
{
    public class FactoryViewsWinForms : IFactoryViews
    {
        public IViewBase View(MonitorComponent component)
        {
            return new MonitorForm(component);
        }

        public ISelectorView<FolderWithServiceRef> View(FolderSelectorComponent component)
        {
            return new FoldersSelectorForm(component);
        }

        public ISelectorView<NoteInfoDto> View(NotesSelectorComponent component)
        {
            return new NotesSelectorForm(component);
        }

        public IViewBase View(KNoteManagmentComponent component)
        {
            return new KNoteManagmentForm(component);
        }

        public IEditorView<NoteDto> View(NoteEditorComponent component)
        {
            return new NoteEditorForm(component);
        }

        //#region Secondary views

        //public IManagmentView NotifyView(KNoteManagmentCtrl ctrl)
        //{
        //    return new NotifyForm(ctrl);
        //}

        //#endregion 

    }
}
