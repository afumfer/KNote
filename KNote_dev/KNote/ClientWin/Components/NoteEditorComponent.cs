using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Components
{
    public class NoteEditorComponent : ComponentViewBase<IViewBase>
    {
        public NoteEditorComponent(Store store) : base(store)
        {
        }

        protected override IViewBase CreateView()
        {
            throw new NotImplementedException();
        }
    }
}
