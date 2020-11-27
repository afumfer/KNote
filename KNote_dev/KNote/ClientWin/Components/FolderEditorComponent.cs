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
    public class FolderEditorComponent : ComponentEditorBase<IEditorView<FolderDto>, FolderDto>
    {
        private FolderDto _folderEdit;
        public FolderDto FolderDto
        {
            set
            {
                _folderEdit = value;
            }
            get
            {
                if (_folderEdit == null)
                    _folderEdit = new FolderDto();
                return _folderEdit;
            }
        }

        public FolderEditorComponent(Store store): base(store)
        {
            
        }

        protected override IEditorView<FolderDto> CreateView()
        {
            return Store.FactoryViews.View(this);
        }

        public override void LoadModelById(IKntService service, Guid noteId)
        {
            throw new NotImplementedException();
        }

        public override void NewModel(IKntService service)
        {
            throw new NotImplementedException();
        }

        public override void SaveModel()
        {
            throw new NotImplementedException();
        }

        public override Task<bool> DeleteModel(IKntService service, Guid noteId)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> DeleteModel()
        {
            throw new NotImplementedException();
        }



    }
}
