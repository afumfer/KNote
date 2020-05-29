﻿using KNote.Shared.Dto;
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

        private string _appDescription = "Other keynotes management for people's group (ver 0.0.4.3)";
        public string AppDescription
        {
            get { return _appDescription; }
            set { _appDescription = value; NotifyStateChanged(); }
        }


        private FolderDto _selectedFolder;
        public FolderDto SelectedFolder
        {
            get 
            {
                if (_selectedFolder == null)
                    _selectedFolder = new FolderDto();
                return _selectedFolder; 
            }
            set 
            { 
                _selectedFolder = value; 
                NotifyStateChanged(); 
            }
        }

        public List<FolderInfoDto> FoldersTree { get; set; }

        public Dictionary<Guid, FolderInfoDto> FoldersIndex { get; set; } = new Dictionary<Guid, FolderInfoDto>();
        
        public event Action OnChange;
        private void NotifyStateChanged() => OnChange?.Invoke();


    }
}
