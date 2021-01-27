using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using KNote.Model;
using KNote.ClientWin.Components;

namespace KNote.ClientWin.Core
{
    public class Store
    {
        #region Application state 

        public AppConfig AppConfig { get; protected set; }

        public string AppUserName { get; set; }
        public string ComputerName { get; set; }
        public string LogFile { get; set; }

        public bool LogActivated = false;

        public readonly IFactoryViews FactoryViews;

        private readonly List<ServiceRef> _servicesRefs;

        private readonly List<ComponentBase> _listComponents;
        
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
            if (AppConfig == null)
                AppConfig = new AppConfig();

            _listComponents = new List<ComponentBase>();
            _servicesRefs = new List<ServiceRef>();
            FactoryViews = factoryViews; //
        }

        public Store(AppConfig config, IFactoryViews factoryViews) : this (factoryViews)
        {
            AppConfig = config;
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
        
        public async Task<bool> CheckNoteIsActive(Guid noteId)
        {
            foreach(var com in _listComponents)
            {
                if (com is NoteEditorComponent)
                {
                    var comNote = (NoteEditorComponent)com;
                    if (comNote.Model.NoteId == noteId && comNote.EditMode == true )
                        return await Task.FromResult<bool>(true);

                }
            }
            return await Task.FromResult<bool>(false);
        }

        public async Task<bool> CheckPostItIsActive(Guid noteId)
        {
            foreach (var com in _listComponents)
            {
                if (com is PostItEditorComponent)
                    if (((PostItEditorComponent)com).Model.NoteId == noteId)
                        return await Task.FromResult<bool>(true);
            }
            return await Task.FromResult<bool>(false);
        }

        public async Task<bool> SaveActiveNotes()
        {
            try
            {
                foreach (var com in _listComponents)
                {
                    if (com is PostItEditorComponent)
                        await ((PostItEditorComponent)com).SaveModel();

                    if (com is NoteEditorComponent)
                    {
                        var comNote = (NoteEditorComponent)com;
                        if (comNote.EditMode)
                            await comNote.SaveModel();
                    }
                }

                return await Task.FromResult<bool>(true);
            }
            catch (Exception)
            {
                return await Task.FromResult<bool>(false);
            }
        }

        public void HidePostIts()
        {
            foreach (var com in _listComponents)
            {
                if (com is PostItEditorComponent)
                    ((PostItEditorComponent)com).HidePostIt();                        
            }            
        }

        public void ActivatePostIts()
        {
            foreach (var com in _listComponents)
            {
                if (com is PostItEditorComponent)
                    ((PostItEditorComponent)com).ActivatePostIt();
            }
        }


        #endregion
    }

    #region  Context typos 

    #endregion 
}
