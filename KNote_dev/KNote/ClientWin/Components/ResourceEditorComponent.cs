using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using System.Windows.Forms;

using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service;
using System.ComponentModel.DataAnnotations;

namespace KNote.ClientWin.Components
{
    public class ResourceEditorComponent : ComponentEditorBase<IEditorView<ResourceDto>, ResourceDto>
    {
        public ResourceEditorComponent(Store store): base(store)
        {
            ComponentName = "Resource editor";
        }

        #region Abstract members implementations

        protected override IEditorView<ResourceDto> CreateView()
        {
            return Store.FactoryViews.View(this);
        }

        public override Task<bool> DeleteModel(IKntService service, Guid id)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> DeleteModel()
        {
            throw new NotImplementedException();
        }

        public override Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
        {
            throw new NotImplementedException();
        }

        public override void NewModel(IKntService service)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> SaveModel()
        {
            throw new NotImplementedException();
        }

        #endregion 
    }
}
