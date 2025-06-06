﻿using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Views;

public partial class FoldersSelectorForm : Form, IViewSelector<FolderWithServiceRef>
{
    #region Private fields

    private readonly FoldersSelectorCtrl _ctrl;
    private bool _viewFinalized = false;

    #endregion

    #region Constructor

    public FoldersSelectorForm(FoldersSelectorCtrl ctrl)
    {
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        _ctrl = ctrl;
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

    Result<EControllerResult> IViewBase.ShowModalView()
    {            
        return _ctrl.DialogResultToControllerResult(this.ShowDialog());
    }

    public async void RefreshView()
    {            
        using (new WaitCursor())
        {               
            TreeNode rootRepNode;
                                   
            treeViewFolders.Nodes.Clear();
            _ctrl.ListEntities.Clear();                

            foreach (var serviceRef in _ctrl.ServicesRef)
            {
                _ctrl.NotifyMessage($"Loading folderes in {serviceRef.Alias} ...");

                rootRepNode = new TreeNode("[" + serviceRef.Alias + "]", 2, 2);         
                rootRepNode.Tag = serviceRef;                
                treeViewFolders.Nodes.Add(rootRepNode);
               
                LoadNodes(rootRepNode, serviceRef, await _ctrl.LoadEntities(serviceRef));
                rootRepNode.Expand();
                treeViewFolders.Refresh();
                
            }
            _ctrl.NotifyMessage("");            
        }
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

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        return MessageBox.Show(info, caption, buttons, icon);
    }

    #region Extensions managment ... 

    // TODO: extensions managment here ... 
    // ....

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
            _ctrl.ShowMessage("KMSG: The parent node of the new node is not correct.", KntConst.AppName);
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

    public void RefreshItem(FolderWithServiceRef newItem)
    {
        var selected = SelectItem(newItem);

        if (selected == null)
            return;

        var selectedNode = (TreeNode)selected;
        var selectedItem = (FolderWithServiceRef)selectedNode.Tag;

        if (selectedItem.FolderInfo == null)
            return;
        
        treeViewFolders.SelectedNode.Tag = newItem;
        treeViewFolders.SelectedNode.Text = newItem.FolderInfo.Name;
        if (_ctrl.OldParent == newItem.FolderInfo.ParentId)            
            return;
                   
        TreeNode[] treeNodes;
        if (newItem.FolderInfo.ParentId != null)
            treeNodes = treeViewFolders.Nodes.Find(newItem.FolderInfo.ParentId.ToString(), true);
        else
        {
            treeNodes = new TreeNode[1];
            treeNodes[0] = GetRootRepositoryNode(selectedNode);
        }

        if (treeNodes?.Length > 0)
        {                               
            var node = treeNodes[0];
            selectedNode.Parent.Nodes.Remove(selectedNode);
            node.Nodes.Add(selectedNode);
            treeViewFolders.SelectedNode = selectedNode;
        }            
    }

    private TreeNode GetRootRepositoryNode(TreeNode node)
    {
        if (node == null)
            return null;

        TreeNode rootRepositoryNode = node.Parent;
        while(rootRepositoryNode.Parent != null)
        {
            rootRepositoryNode = rootRepositoryNode.Parent;
        }
        return rootRepositoryNode;
    }

    public List<FolderWithServiceRef> GetSelectedListItem()
    {
        throw new NotImplementedException();
    }

    #endregion 

    #endregion

    #region Form events handlers 

    private void treeViewFolders_AfterSelect(object sender, TreeViewEventArgs e)
    {
        try
        {
            if (!buttonAccept.Enabled)
                buttonAccept.Enabled = true;

            FolderWithServiceRef v = null;

            if (e.Node.Tag is FolderWithServiceRef)
                v = (FolderWithServiceRef)e.Node.Tag;
            else if (e.Node.Tag is ServiceRef)
            {
                v = new FolderWithServiceRef() { ServiceRef = (ServiceRef)e.Node.Tag, FolderInfo = null };
            }

            _ctrl.SelectedEntity = v;
            _ctrl.Path = treeViewFolders.SelectedNode.FullPath;
            _ctrl.NotifySelectedEntity();                
        }
        catch (Exception)
        {            
            throw;
        }
    }

    private void buttonAccept_Click(object sender, EventArgs e)
    {
        if (_ctrl.SelectedEntity != null)
        {
            _ctrl.Accept();
            this.DialogResult = DialogResult.OK;
        }
        else
        {
            _ctrl.ShowMessage("You have not selected a folder. Select a folder and click 'Accept' button or press 'Cancel' button to abort the folder selection..", KntConst.AppName);
            this.DialogResult = DialogResult.None;
        }
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
        _ctrl.Cancel();
    }

    private void FoldersSelectorForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
            _ctrl.Finalize();
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
            _ctrl.ListEntities.Add(folderWithServiceRef);
            node.Nodes.Add(nodeFolder);

            LoadNodes(nodeFolder, service, f.ChildFolders);
        }
    }

    #endregion

    #region Extensions

    // TODO: Esto es más código repetido, hay que pasar a una clase base 
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        this.SuspendLayout();

        if (_ctrl.Extensions.Keys.Count > 0)
            foreach (string s in _ctrl.Extensions.Keys)
                if (s.StartsWith("--"))
                    contextMenu.Items.Add("-", null, extension_Click);
                else
                    contextMenu.Items.Add(s, null, extension_Click);

        this.ResumeLayout();
    }

    private void extension_Click(object sender, EventArgs e)
    {
        ToolStripMenuItem menuSel;
        menuSel = (ToolStripMenuItem)sender;

        _ctrl.Extensions[menuSel.Text](this, new ControllerEventArgs<FolderWithServiceRef>(_ctrl.SelectedEntity));
    }

    #endregion
}
