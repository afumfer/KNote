using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service;

namespace KNote.ClientWin.Components
{
    public class NoteTypesSelectorComponent : ComponentSelectorBase<ISelectorView<NoteTypeDto>, NoteTypeDto>
    {

        public NoteTypesSelectorComponent(Store store): base(store)
        {

        }

        protected override ISelectorView<NoteTypeDto> CreateView()
        {
            return Store.FactoryViews.View(this);
        }

        public async override Task<bool> LoadEntities(IKntService service, bool refreshView = true)
        {
            try
            {
                Service = service;
                
                var response = await Service.NoteTypes.GetAllAsync();

                if (response.IsValid)
                {
                    ListEntities = response.Entity;

                    if(refreshView)
                        View.RefreshView();

                    if (ListEntities?.Count > 0)
                        SelectedEntity = ListEntities[0];
                    else
                        SelectedEntity = null;

                    NotifySelectedEntity();
                }
                else
                {
                    View.ShowInfo(response.Message);
                    return false;
                }
            }
            catch (Exception ex)
            {
                View.ShowInfo(ex.Message);
                return false;
            }

            return true;
        }


        public override void AddItem(NoteTypeDto item)
        {
            throw new NotImplementedException();
        }

        public override void DeleteItem(NoteTypeDto item)
        {
            throw new NotImplementedException();
        }

        public override void RefreshItem(NoteTypeDto item)
        {
            throw new NotImplementedException();
        }

        public override void SelectItem(NoteTypeDto item)
        {
            throw new NotImplementedException();
        }


    }
}
