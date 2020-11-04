using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Components
{
    public class MonitorComponent : ComponentViewBase<IViewBase>
    {
        public MonitorComponent(Store store) : base(store)
        {

        }
            
        protected override IViewBase CreateView()
        {
            return Store.FactoryViews.View(this);
        }

        protected override Result OnInitialized()
        {
            var result = base.OnInitialized();

            try
            {                                
                View.ShowView();

                Store.ComponentsStateChanged += Store_ComponentsStateChanged;
                Store.AddedServiceRef += Store_AddedServiceRef;
                Store.ActiveFolderChanged += Store_ActiveFolderChanged;
                Store.RemovedServiceRef += Store_RemovedServiceRef;
                Store.ComponentsResultChanged += Store_ComponentsResultChanged;
                      
            }
            catch (Exception ex)
            {                
                result.AddErrorMessage(ex.Message);
            }

            return result;
        }

        protected override Result OnFinalized()
        {
            var result = base.OnFinalized();

            try
            {
                Store.ComponentsStateChanged -= Store_ComponentsStateChanged;
                Store.AddedServiceRef -= Store_AddedServiceRef;
                Store.ActiveFolderChanged -= Store_ActiveFolderChanged;
                Store.RemovedServiceRef -= Store_RemovedServiceRef;
                Store.ComponentsResultChanged -= Store_ComponentsResultChanged;

            }
            catch (Exception ex)
            {
                result.AddErrorMessage(ex.Message);                
            }

            return result;
        }

        #region Store events handlers

        private void Store_ComponentsStateChanged(object sender, ComponentEventArgs<EComponentState> e)
        {
            var info = $"{DateTime.Now} - [ControllersStateChanged] - {sender.ToString()} - {e.Entity.ToString()} - {((ComponentBase)sender).ComponentId}";
            OnShowLog(info);
        }

        private void Store_ComponentsResultChanged(object sender, ComponentEventArgs<EComponentResult> e)
        {
            var info = $"{DateTime.Now} - [ControllersResultChanged] - {sender.ToString()} - {e.Entity.ToString()} - {((ComponentBase)sender).ComponentId}";
            OnShowLog(info);
        }
       
        private void Store_RemovedServiceRef(object sender, ComponentEventArgs<ServiceRef> e)
        {
            var info = $"{DateTime.Now} - [RemovedServiceRef] - {sender.ToString()} - {e.Entity.Alias.ToString()}";
            OnShowLog(info);
        }

        private void Store_ActiveFolderChanged(object sender, ComponentEventArgs<FolderWithServiceRef> e)
        {
            var info = $"{DateTime.Now}  - [ActiveFolderChanged] - {sender.ToString()} - {e.Entity.FolderInfo.Name.ToString()}";
            OnShowLog(info);
        }

        private void Store_AddedServiceRef(object sender, ComponentEventArgs<ServiceRef> e)
        {
            var info = $"{DateTime.Now} - [AddedServiceRef] - {sender.ToString()} - {e.Entity.Alias.ToString()}";
            OnShowLog(info);
        }

        #endregion 

        #region Private methods

        private void OnShowLog(string info)
        {
            View.ShowInfo(info);

            if (!Store.LogActivated)
                return;

            using (StreamWriter outputFile = new StreamWriter(Store.LogFile, true))
            {
                outputFile.WriteLine(info);
            }
        }

        #endregion
    }
}
