using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using KNote.Model;
using KNote.ClientWin.Components;
using KntScript;
using KNote.ClientWin.Views;
using KNote.Service;
using System.Threading;
using System.Windows.Forms;

namespace KNote.ClientWin.Core
{
    public class Store
    {
        #region Private fields

        private readonly List<ServiceRef> _servicesRefs;

        private readonly List<ComponentBase> _listComponents;

        #endregion 

        #region Public properties, application state 

        public AppConfig AppConfig { get; protected set; }

        public string AppUserName { get; set; }

        public string ComputerName { get; set; }

        public readonly IFactoryViews FactoryViews;

        public FolderWithServiceRef _dafaultFolderWithServiceRef;
        public FolderWithServiceRef DefaultFolderWithServiceRef
        {
            set { _dafaultFolderWithServiceRef = value; }
            get { return _dafaultFolderWithServiceRef; }
        }

        public FolderWithServiceRef _activeFolderWithServiceRef;
        public FolderWithServiceRef ActiveFolderWithServiceRef
        {
            set { _activeFolderWithServiceRef = value; }
            get { return _activeFolderWithServiceRef; }            
        }

        public NotesFilterWithServiceRef _activeFilterWithServiceRef;
        public NotesFilterWithServiceRef ActiveFilterWithServiceRef
        {
            set { _activeFilterWithServiceRef = value; }
            get { return _activeFilterWithServiceRef; }
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

        public void AddServiceRefInAppConfig(ServiceRef serviceRef)
        {
            AppConfig.RespositoryRefs.Add(serviceRef.RepositoryRef);
        }

        public event EventHandler<ComponentEventArgs<ServiceRef>> RemovedServiceRef;
        public void RemoveServiceRef(ServiceRef serviceRef)
        {            
            _servicesRefs.Remove(serviceRef);
            AppConfig.RespositoryRefs.Remove(serviceRef.RepositoryRef);
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

        public ServiceRef GetServiceRef(string alias)
        {
            return _servicesRefs.Where(_ => _.Alias == alias).FirstOrDefault();
        }

        public ServiceRef GetFirstServiceRef()
        {
            return _servicesRefs.FirstOrDefault();
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

        public void SaveConfig(string configFile = null)
        {
            if(string.IsNullOrEmpty(configFile))
                configFile = Path.Combine(Application.StartupPath, "KNoteData.config"); ;
            try
            {
                TextWriter w = new StreamWriter(configFile);
                XmlSerializer serializer = new XmlSerializer(typeof(AppConfig));
                serializer.Serialize(w, AppConfig);
                w.Close();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void LoadConfig(string configFile = @"KNoteData.config")
        {
            try
            {
                if (!File.Exists(configFile))
                    return;
                
                TextReader reader = new StreamReader(configFile);
                XmlSerializer serializer = new XmlSerializer(typeof(AppConfig));
                AppConfig = (AppConfig)serializer.Deserialize(reader);
                AppConfig.LastDateTimeStart = DateTime.Now;
                AppConfig.RunCounter++;
                reader.Close();                
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

        public async Task<bool> SaveAndCloseActiveNotes(Guid serviceId)
        {
            var stackPostIts = new Stack<PostItEditorComponent>();
            var stackNotes = new Stack<NoteEditorComponent>();

            try
            {
                foreach (var com in _listComponents)
                {
                    if (com is PostItEditorComponent)
                    {                        
                        var comNote = (PostItEditorComponent)com;
                        if (comNote.ServiceRef.IdServiceRef == serviceId)
                        {
                            await comNote.SaveModel();                            
                            stackPostIts.Push(comNote);
                        }
                    }
                    if (com is NoteEditorComponent)
                    {
                        var comNote = (NoteEditorComponent)com;
                        if (comNote.ServiceRef.IdServiceRef == serviceId)
                        {
                            if (comNote.EditMode)
                            {
                                await comNote.SaveModel();                                
                                stackNotes.Push(comNote);
                            }
                        }                        
                    }
                }
                while(stackPostIts.Count > 0)
                {
                    var postIt = stackPostIts.Pop();
                    postIt.Finalize();
                }
                while (stackNotes.Count > 0)
                {
                    var note = stackNotes.Pop();
                    note.Finalize();
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
                {

                    ((PostItEditorComponent)com).HidePostIt();
                }
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

        public void RunScript(string code, bool newThread = true)
        {
            if (string.IsNullOrEmpty(code))
                return;

            var kntScript = new KntSEngine(new InOutDeviceForm(), new KNoteScriptLibrary(this));
                                  
            if (newThread)
            {
                var t = new Thread(() => kntScript.Run(code));
                t.IsBackground = false;
                t.Start();
            }
            else
                kntScript.Run(code);
        }

        public string ExtensionFileToFileType(string extension)
        {
            // TODO: study this method ...
            if (extension == ".jpg")
                return @"image/jpeg";
            if (extension == ".jpeg")
                return @"image/jpeg";
            else if (extension == ".png")
                return "image/png";
            else if (extension == ".pdf")
                return "application/pdf";
            else if (extension == ".zip")
                return "application/zip";
            else
                return "";
        }

        #endregion
    }

    #region  Context typos 

    #endregion 
}
