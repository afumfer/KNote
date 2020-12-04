﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views
{
    public partial class FoldersSelectorForm : Form, ISelectorView<FolderWithServiceRef>
    {
        #region Private fields

        private readonly FoldersSelectorComponent _com;
        private bool _viewFinalized = false;

        #endregion

        #region Constructor

        public FoldersSelectorForm(FoldersSelectorComponent com)
        {
            InitializeComponent();

            _com = com;
        }

        #endregion 

        #region ISelectorView interface

        public Control PanelView()
        {
            return panelForm;
        }

        public void ShowView()
        {                         
            this.Show();
        }

        Result<EComponentResult> IViewBase.ShowModalView()
        {            
            return _com.DialogResultToComponentResult(this.ShowDialog());
        }

        public async void RefreshView()
        {            
            TreeNode rootRepNode = null;

            treeViewFolders.Visible = false;
            treeViewFolders.Nodes.Clear();
            _com.ListEntities.Clear();

            foreach (var serviceRef in _com.ServicesRef)
            {
                rootRepNode = new TreeNode("[" + serviceRef.Alias + "]", 2, 2);         
                rootRepNode.Tag = serviceRef;                
                treeViewFolders.Nodes.Add(rootRepNode);

                LoadNodes(rootRepNode, serviceRef, await _com.LoadEntities(serviceRef));

            }
            treeViewFolders.Visible = true;
        }

        public void OnClosingView()
        {
            _viewFinalized = true;
            this.Close();
        }

        public void ConfigureEmbededMode()
        {
            TopLevel = false;
            Dock = DockStyle.Fill;
            FormBorderStyle = FormBorderStyle.None;
            panelBottom.Visible = false;
        }

        public void ConfigureWindowMode()
        {
            TopLevel = true;
            Dock = DockStyle.None;
            FormBorderStyle = FormBorderStyle.Sizable;
            panelBottom.Visible = true;
            StartPosition = FormStartPosition.CenterScreen;
        }
      
        public object SelectItem(FolderWithServiceRef item)
        {
            var treeNodes = treeViewFolders.Nodes.Find(item.FolderInfo.FolderId.ToString(), true);

            if (treeNodes.Length > 0)
            {
                var node = treeNodes[0];
                treeViewFolders.SelectedNode = node;
                return node;
            }
            else
                return null;
        }

        public DialogResult ShowInfo(string info, string caption = "KeyNote", MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            return MessageBox.Show(info, caption, buttons);
        }

        #region Extensions managment ... 

        // TODO: extensions managment ... 

        public void AddItem(FolderWithServiceRef item)
        {
            TreeNode newNode = new TreeNode(item.FolderInfo.Name, 1, 0);
            newNode.Tag = item;
            newNode.Name = item.FolderInfo.FolderId.ToString();

            TreeNode n0 = treeViewFolders.SelectedNode;            
            if (n0.Name == item.FolderInfo.ParentId.ToString())
            {
                n0.Nodes.Add(newNode);                                
                n0.Expand();
                treeViewFolders.SelectedNode = newNode;
            }
            else
                _com.ShowMessage("KMSG: The parent node of the new node is not correct.", "KeyNote");
        }

        public void DeleteItem(FolderWithServiceRef item)
        {
            if (SelectItem(item) != null)
            {
                TreeNode parentNode = treeViewFolders.SelectedNode.Parent;
                treeViewFolders.SelectedNode.Remove();
                if(parentNode != null)
                    treeViewFolders.SelectedNode = parentNode;
            }
        }

        public void RefreshItem(FolderWithServiceRef item)
        {
            if (SelectItem(item) != null)
            {
                treeViewFolders.SelectedNode.Tag = item;
                treeViewFolders.SelectedNode.Text = item.FolderInfo.Name;
            }
        }

        #endregion 

        #endregion

        #region Form events handlers 

        private void treeViewFolders_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (sender != null)
                this.Cursor = Cursors.WaitCursor;
            try
            {
                FolderWithServiceRef v = null;

                if (e.Node.Tag is FolderWithServiceRef)
                    v = (FolderWithServiceRef)e.Node.Tag;
                else if (e.Node.Tag is ServiceRef)
                {
                    v = new FolderWithServiceRef() { ServiceRef = (ServiceRef)e.Node.Tag, FolderInfo = null };
                }

                _com.SelectedEntity = v;
                _com.Path = treeViewFolders.SelectedNode.FullPath;
                _com.NotifySelectedEntity();                
            }
            catch (Exception)
            {
                // TODO: ... investigar excepciones en este punto
                throw;
            }
            finally
            {
                if (sender != null)
                    this.Cursor = Cursors.Default;
            }
        }

        private void buttonAccept_Click(object sender, EventArgs e)
        {            
            _com.Accept();            
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _com.Cancel();
        }

        private void FoldersSelectorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_viewFinalized)
                _com.Finalize();
        }

        #endregion

        #region Private methods

        private void LoadNodes(TreeNode node, ServiceRef service, List<FolderDto> folders)
        {
            if (folders == null)
                return;

            foreach (var f in folders)
            {
                TreeNode nodeFolder = new TreeNode(f.Name, 1, 0);                
                nodeFolder.Name = f.FolderId.ToString();
                var folderWithServiceRef = new FolderWithServiceRef() { ServiceRef = service, FolderInfo = f };
                nodeFolder.Tag = folderWithServiceRef;
                _com.ListEntities.Add(folderWithServiceRef);
                node.Nodes.Add(nodeFolder);

                LoadNodes(nodeFolder, service, f.ChildFolders);
            }
        }

        #endregion
    }
}
