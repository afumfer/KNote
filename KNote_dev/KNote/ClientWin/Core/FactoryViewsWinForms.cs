using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KNote.ClientWin.Components;
using KNote.ClientWin.Views;
using KNote.Model.Dto;
using KntScript;

namespace KNote.ClientWin.Core
{
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

        public IViewConfigurable View(KNoteManagmentComponent component)
        {
            return new KNoteManagmentForm(component);
        }

        public IEditorView<NoteExtendedDto> View(NoteEditorComponent component)
        {
            return new NoteEditorForm(component);
        }
        public IEditorView<FolderDto> View(FolderEditorComponent component)
        {
            return new FolderEditorForm(component);
        }

        #region Secondary views

        public IViewBase NotifyView(KNoteManagmentComponent component)
        {
            return new NotifyForm(component);
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


        #endregion

    }
}
