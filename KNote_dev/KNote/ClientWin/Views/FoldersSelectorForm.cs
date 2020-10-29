using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace KNote.ClientWin.Views
{
    public partial class FoldersSelectorForm : Form, ISelectorView<FolderWithServiceRef>
    {
        private readonly FolderSelectorComponent _com;

        public FoldersSelectorForm(FolderSelectorComponent com)
        {
            InitializeComponent();

            _com = com;
        }

        #region ISelectorView interface

        public void ShowView()
        {
            RefreshView();

            if (_com.ModalMode)
                this.ShowDialog();
            else
                this.Show();
        }

        public async void RefreshView()
        {
            TreeNode rootRepNode = null;

            treeViewFolders.Visible = false;
            treeViewFolders.Nodes.Clear();

            foreach(var serviceRef in _com.Store.GetAllServiceRef())
            {
                //rootRepNode = new TreeNode("[" + service.Alias + "]", 2, 2);
                rootRepNode = new TreeNode("[" + serviceRef.Alias + "]");
                rootRepNode.Tag = serviceRef;
                treeViewFolders.Nodes.Add(rootRepNode);

                var folders = (await serviceRef.Service.Folders.GetTreeAsync()).Entity;

                LoadNodes(rootRepNode, folders);
            }



            treeViewFolders.Visible = true;
        }


        private void LoadNodes(TreeNode node, List<FolderDto> folders)
        {
            if (folders == null)
                return;

            foreach(var f in folders)
            {
                //TreeNode nodeFolder = new TreeNode(f.Name, 1, 0);
                TreeNode nodeFolder = new TreeNode(f.Name);
                nodeFolder.Name = f.FolderId.ToString();
                nodeFolder.Tag = f;
                node.Nodes.Add(nodeFolder);

                LoadNodes(nodeFolder, f.ChildFolders);
            }
        }


        public void AddItem(FolderWithServiceRef item)
        {
            throw new NotImplementedException();
        }

        public void ConfigureEmbededMode()
        {
            throw new NotImplementedException();
        }

        public void ConfigureWindowMode()
        {
            throw new NotImplementedException();
        }

        public void DeleteItem(FolderWithServiceRef item)
        {
            throw new NotImplementedException();
        }

        public void OnClosingView()
        {
            
        }

        public void RefreshItem(FolderWithServiceRef item)
        {
            throw new NotImplementedException();
        }


        public object SelectItem(FolderWithServiceRef item)
        {
            throw new NotImplementedException();
        }

        public void ShowInfo(string info)
        {
            throw new NotImplementedException();
        }


        #endregion

        private void FoldersSelectorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // TODO: !!! Temporal, esto hay que quitarlo de aquí. 
            _com.Finalize();
        }
    }
}
