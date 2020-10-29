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

        public readonly IFactoryViews FactoryViews;

        private readonly List<ServiceRef> _servicesRefs;

        private readonly List<ComponentBase> _listComponents;

        public AppConfig Config { get; }

        public ServiceRef PersonalServiceRef 
        {
            get { return _servicesRefs[0]; }
                 
        }

        //public User ActiveUser { get; }


        public FolderWithServiceRef _activeFolderWithServiceRef;
        public FolderWithServiceRef ActiveFolderWithServiceRef
        {
            get { return _activeFolderWithServiceRef; }
        }

        #endregion

        #region Constructor 

        public Store(IFactoryViews factoryViews)
        {
            if (Config == null)
                Config = new AppConfig();

            _listComponents = new List<ComponentBase>();
            _servicesRefs = new List<ServiceRef>();
            FactoryViews = factoryViews; //
        }

        public Store(AppConfig config, IFactoryViews factoryViews) : this (factoryViews)
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

        public event EventHandler<StateComponentEventArgs> ComponentsStateChanged;
        public void AddComponent(ComponentBase controller)
        {
            _listComponents.Add(controller);
            controller.StateCtrlChanged += Components_StateCtrlChanged;
        }

        private void Components_StateCtrlChanged(object sender, StateComponentEventArgs e)
        {
            if(ComponentsStateChanged != null)
                ComponentsStateChanged(sender, e);
        }

        public void RemoveComponent(ComponentBase component)
        {            
            _listComponents.Remove(component);            
        }

        public event EventHandler<EntityEventArgs<FolderWithServiceRef>> ActiveFolderChanged;
        public void UpdateActiveFolder(FolderWithServiceRef activeFolder)
        {
            _activeFolderWithServiceRef = activeFolder;
            if (ActiveFolderChanged != null)
                ActiveFolderChanged(this, new EntityEventArgs<FolderWithServiceRef>(activeFolder));
        }

        #endregion
    }

    #region  Context typos 

    #endregion 
}
