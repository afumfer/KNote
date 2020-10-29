using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KNote.ClientWin.Components;
using KNote.ClientWin.Views;

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


        // #region Primary views

        //public ISelectorView<NoteItemDto> View(NotesSelectorCtrl ctrl)
        //{                        
        //    return new NotesSelectorForm(ctrl);            
        //}


        //public IManagmentView View(KNoteManagmentCtrl ctrl)
        //{
        //    return new KNoteManagmentForm(ctrl);
        //}


        //public IEditorView<NoteDto> View(NoteEditorCtrl ctrl)
        //{
        //    return new NoteEditorForm(ctrl);
        //}


        //#endregion

        //#region Secondary views

        //public IManagmentView NotifyView(KNoteManagmentCtrl ctrl)
        //{
        //    return new NotifyForm(ctrl);
        //}

        //#endregion 

    }
}
