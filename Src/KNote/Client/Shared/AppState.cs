﻿using KNote.Model.Dto;

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

        private string _appName = "KaNote";
        public string AppName
        {
            get { return _appName; }
            set { _appName = value; NotifyStateChanged(); }
        }

        private string _appDescription = "Another keynotes management for people's group (ver 0.0.5.14)";
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

        // Only used in KntFoldersTreeView  (hack)
        public FolderDto folderOldSelected { get; set; }

        private List<FolderDto> _foldersTree;
        public List<FolderDto> FoldersTree 
        { 
            get
            {
                return _foldersTree;
            }
            set
            {
                _foldersTree = value;
                if(_foldersIndex != null)
                    _foldersIndex = null;
            } 
        }

        private Dictionary<Guid, FolderDto> _foldersIndex;
        public Dictionary<Guid, FolderDto> FoldersIndex 
        {
            get
            {
                if(_foldersIndex == null)
                    _foldersIndex = new Dictionary<Guid, FolderDto>();
                return _foldersIndex;
            }
        } 
        
        public event Action OnChange;
        private void NotifyStateChanged() => OnChange?.Invoke();


    }
}