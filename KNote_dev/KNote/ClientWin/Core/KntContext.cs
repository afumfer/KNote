using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KNote.Model;

namespace KNote.ClientWin.Core
{
    public class KntContext
    {
        #region Application state 

        public string LogFile { get; set; }

        public bool LogActivatd = false;

        public readonly ViewsFactory FactoryViews;

        private readonly List<KntServiceRef> _servicesRefs;

        private readonly List<BaseCtrl> _listCtrl;

        public AppConfig Config { get; }

        public KntServiceRef PersonalServiceRef { get; }
       
        //public User ActiveUser { get; }


        //public FolderWithServiceRef _activeFolderWithServiceRef;
        //public FolderWithServiceRef ActiveFolderWithServiceRef
        //{
        //    get { return _activeFolderWithServiceRef; }
        //}

        #endregion

        #region Constructor 

        public KntContext()
        {
            if (Config == null)
                Config = new AppConfig();

            _listCtrl = new List<BaseCtrl>();
            _servicesRefs = new List<KntServiceRef>();            
            FactoryViews = new ViewsFactory();
        }

        public KntContext(AppConfig config) : this ()
        {
            Config = config;
        }

        #endregion

        #region Actions        
        public event EventHandler<EntityEventArgs<KntServiceRef>> AddedServiceRef;
        public void AddServiceRef(KntServiceRef serviceRef)
        {
            _servicesRefs.Add(serviceRef);
            if (AddedServiceRef != null)
                AddedServiceRef(this, new EntityEventArgs<KntServiceRef>(serviceRef));
        }
        
        public event EventHandler<EntityEventArgs<KntServiceRef>> RemovedServiceRef;
        public void RemoveServiceRef(KntServiceRef serviceRef)
        {
            _servicesRefs.Remove(serviceRef);
            if (RemovedServiceRef != null)
                RemovedServiceRef(this, new EntityEventArgs<KntServiceRef>(serviceRef));
        }

        public List<KntServiceRef> GetAllServiceRef()
        {
            return _servicesRefs.ToList();
        }

        public event EventHandler<StateCtrlEventArgs> ControllersStateChanged;
        public void AddController(BaseCtrl controller)
        {
            _listCtrl.Add(controller);
            controller.StateCtrlChanged += Controllers_StateCtrlChanged;
        }

        private void Controllers_StateCtrlChanged(object sender, StateCtrlEventArgs e)
        {
            if(ControllersStateChanged != null)
                ControllersStateChanged(sender, e);
        }

        public void RemoveController(BaseCtrl controller)
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
