using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using KNote.Model;

namespace KNote.ClientWin.Core
{
    public class Store
    {
        #region Application state 

        public string AppUserName { get; set; }
        public string ComputerName { get; set; }
        public string LogFile { get; set; }

        public bool LogActivated = false;

        public readonly IFactoryViews FactoryViews;

        private readonly List<ServiceRef> _servicesRefs;

        private readonly List<ComponentBase> _listComponents;

        public AppConfig Config { get; protected set; }

        public ServiceRef PersonalServiceRef 
        {
            get { return _servicesRefs[0]; }
                 
        }

        //public User ActiveUser { get; protected set;}

        public FolderWithServiceRef _activeFolderWithServiceRef;
        public FolderWithServiceRef ActiveFolderWithServiceRef
        {
            set { _activeFolderWithServiceRef = value; }
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
        
        public event EventHandler<ComponentEventArgs<ServiceRef>> AddedServiceRef;
        public void AddServiceRef(ServiceRef serviceRef)
        {
            _servicesRefs.Add(serviceRef);
            if (AddedServiceRef != null)
                AddedServiceRef(this, new ComponentEventArgs<ServiceRef>(serviceRef));
        }
        
        public event EventHandler<ComponentEventArgs<ServiceRef>> RemovedServiceRef;
        public void RemoveServiceRef(ServiceRef serviceRef)
        {
            _servicesRefs.Remove(serviceRef);
            if (RemovedServiceRef != null)
                RemovedServiceRef(this, new ComponentEventArgs<ServiceRef>(serviceRef));
        }

        public List<ServiceRef> GetAllServiceRef()
        {
            return _servicesRefs.ToList();
        }

        public ServiceRef GetServiceRef(Guid id)
        {
            return _servicesRefs.Where(_ => _.IdServiceRef == id).FirstOrDefault();
        }


        public event EventHandler<ComponentEventArgs<EComponentState>> ComponentsStateChanged;
        public void AddComponent(ComponentBase controller)
        {
            _listComponents.Add(controller);
            controller.StateComponentChanged += Components_StateCtrlChanged;
        }

        private void Components_StateCtrlChanged(object sender, ComponentEventArgs<EComponentState> e)
        {
            ComponentsStateChanged?.Invoke(sender, e);
        }

        public event EventHandler<ComponentEventArgs<ComponentBase>> RemovedComponent;
        public void RemoveComponent(ComponentBase component)
        {            
            _listComponents.Remove(component);
            RemovedComponent?.Invoke(this, new ComponentEventArgs<ComponentBase>(component));
        }

        public event EventHandler<ComponentEventArgs<string>> ComponentNotification;
        internal void OnComponentNotification(ComponentBase component, string message)
        {
            ComponentNotification?.Invoke(component, new ComponentEventArgs<string>(message));
        }


        public event EventHandler<ComponentEventArgs<FolderWithServiceRef>> ActiveFolderChanged;
        public void UpdateActiveFolder(FolderWithServiceRef activeFolder)
        {
            _activeFolderWithServiceRef = activeFolder;
            if (ActiveFolderChanged != null)
                ActiveFolderChanged(this, new ComponentEventArgs<FolderWithServiceRef>(activeFolder));
        }

        public void SaveConfig(AppConfig appConfig, string configFile = @"KNoteData.config")
        {
            try
            {                
                TextWriter w = new StreamWriter(configFile);
                XmlSerializer serializer = new XmlSerializer(typeof(AppConfig));
                serializer.Serialize(w, appConfig);
                w.Close();
            }
            catch (Exception )
            {
                throw;
            }
        }

        public AppConfig LoadConfig(string configFile = @"KNoteData.config")
        {
            try
            {                
                if (!File.Exists(configFile))
                    return null;

                AppConfig appConfig;                
                TextReader reader = new StreamReader(configFile);                
                XmlSerializer serializer = new XmlSerializer(typeof(AppConfig));
                appConfig = (AppConfig)serializer.Deserialize(reader);
                appConfig.LastDateTimeStart = DateTime.Now;
                appConfig.RunCounter++;
                reader.Close();
                return appConfig;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion
    }

    #region  Context typos 

    #endregion 
}
