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
    public class NoteEditorComponent : ComponentViewBase<IViewConfigurable>
    {
        public NoteEditorComponent(Store store) : base(store)
        {
        }

        protected override IViewConfigurable CreateView()
        {
            throw new NotImplementedException();
        }
    }
}
