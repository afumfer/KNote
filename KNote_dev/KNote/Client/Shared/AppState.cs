using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KNote.Client.Shared
{
    public class AppState
    {
        private string _appMode = "Task";
        public string AppMode
        {
            get { return _appMode; }
            set { _appMode = value; NotifyStateChanged(); }
        }

        private string _appName = "KNote";
        public string AppName
        {
            get { return _appName; }
            set { _appName = value; NotifyStateChanged(); }
        }

        private string _appDescription = "Other keynotes management for people's group (ver 0.0.4.2)";
        public string AppDescription
        {
            get { return _appDescription; }
            set { _appDescription = value; NotifyStateChanged(); }
        }


        private Guid _selectedFolder;
        public Guid SelectedFolder
        {
            get { return _selectedFolder; }
            set { _selectedFolder = value; NotifyStateChanged(); }
        }

        // TODO: !!! pendiente de eliminar 
        private string _textSearch;
        public string TextSearch
        {
            get { return _textSearch; }
            set { _textSearch = value; NotifyStateChanged(); }
        }

        public event Action OnChange;
        private void NotifyStateChanged() => OnChange?.Invoke();

        

    }
}
