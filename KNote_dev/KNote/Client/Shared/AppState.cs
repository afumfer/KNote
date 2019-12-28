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

        private Guid _selectedFolder;
        public Guid SelectedFolder
        {
            get { return _selectedFolder; }
            set { _selectedFolder = value; NotifyStateChanged(); }
        }

        public event Action OnChange;
        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
