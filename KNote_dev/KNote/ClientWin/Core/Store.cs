using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KNote.Model;

namespace KNote.ClientWin.Core
{
    public class Store
    {
        #region Application state 

        public string LogFile { get; set; }

        public bool LogActivated = false;

        public readonly FactoryViews FactoryViews;

        private readonly List<ServiceRef> _servicesRefs;

        private readonly List<ComponentBase> _listCtrl;

        public AppConfig Config { get; }

        public ServiceRef PersonalServiceRef { get; }
       
        //public User ActiveUser { get; }


        //public FolderWithServiceRef _activeFolderWithServiceRef;
        //public FolderWithServiceRef ActiveFolderWithServiceRef
        //{
        //    get { return _activeFolderWithServiceRef; }
        //}

        #endregion

        #region Constructor 

        public Store()
        {
            if (Config == null)
                Config = new AppConfig();

            _listCtrl = new List<ComponentBase>();
            _servicesRefs = new List<ServiceRef>();            
            FactoryViews = new FactoryViews();
        }

        public Store(AppConfig config) : this ()
        {
            Config = config;
        }

        #endregion

        #region Actions        
        public event EventHandler<EntityEventArgs<ServiceRef>> AddedServiceRef;
        public void AddServiceRef(ServiceRef serviceRef)
        {
            _servicesRefs.Add(serviceRef);
            if (AddedServiceRef != null)
                AddedServiceRef(this, new EntityEventArgs<ServiceRef>(serviceRef));
        }
        
        public event EventHandler<EntityEventArgs<ServiceRef>> RemovedServiceRef;
        public void RemoveServiceRef(ServiceRef serviceRef)
        {
            _servicesRefs.Remove(serviceRef);
            if (RemovedServiceRef != null)
                RemovedServiceRef(this, new EntityEventArgs<ServiceRef>(serviceRef));
        }

        public List<ServiceRef> GetAllServiceRef()
        {
            return _servicesRefs.ToList();
        }

        public event EventHandler<StateComponentEventArgs> ControllersStateChanged;
        public void AddController(ComponentBase controller)
        {
            _listCtrl.Add(controller);
            controller.StateCtrlChanged += Controllers_StateCtrlChanged;
        }

        private void Controllers_StateCtrlChanged(object sender, StateComponentEventArgs e)
        {
            if(ControllersStateChanged != null)
                ControllersStateChanged(sender, e);
        }

        public void RemoveController(ComponentBase controller)
        {            
            _listCtrl.Remove(controller);            
        }

        //public event EventHandler<EntityEventArgs<FolderWithServiceRef>> ActiveFolderChanged;
        //public void UpdateActiveFolder(FolderWithServiceRef activeFolder )
        //{
        //    _activeFolderWithServiceRef = activeFolder;
        //    if (ActiveFolderChanged != null)
        //        ActiveFolderChanged(this, new EntityEventArgs<FolderWithServiceRef>(activeFolder));
        //}
       
        #endregion
    }

    #region  Context typos 

    #endregion 
}
