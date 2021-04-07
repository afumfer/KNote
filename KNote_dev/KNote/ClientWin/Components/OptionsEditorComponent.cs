using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;

using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service;
using System.ComponentModel.DataAnnotations;

namespace KNote.ClientWin.Components
{
    public class OptionsEditorComponent : ComponentEditor<IEditorView<AppConfig>, AppConfig>
    {
        #region Constructor 

        public OptionsEditorComponent(Store store) : base(store)
        {
            ComponentName = "Options editor";
        }

        #endregion 

        #region ComponentEditor implementation 

        protected override IEditorView<AppConfig> CreateView()
        {
            return Store.FactoryViews.View(this);
        }

        public async override Task<bool> LoadModelById(IKntService service, Guid id, bool refreshView = true)
        {
            return await Task.FromResult<bool>(true);
        }

        public async override Task<bool> NewModel(IKntService service)
        {
            return await Task.FromResult<bool>(true);
        }

        public async override Task<bool> SaveModel()
        {
            return await Task.FromResult<bool>(true);
        }

        public async override Task<bool> DeleteModel(IKntService service, Guid id)
        {
            return await Task.FromResult<bool>(true);
        }

        public async override Task<bool> DeleteModel()
        {
            return await Task.FromResult<bool>(true);
        }

        #endregion 
    }
}
