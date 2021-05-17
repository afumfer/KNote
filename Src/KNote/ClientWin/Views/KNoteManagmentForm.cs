﻿using KNote.ClientWin.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using KNote.ClientWin.Components;
using KNote.Model.Dto;
using KNote.Model;

namespace KNote.ClientWin.Views
{
    public partial class KNoteManagmentForm : Form, IViewConfigurableExt
    {
        private readonly KNoteManagmentComponent _com;
        private bool _viewFinalized = false;

        public KNoteManagmentForm(KNoteManagmentComponent com)
        {
            InitializeComponent();

            _com = com;

            _com.Store.ComponentNotification += Store_ComponentNotification;

            
            // TODO: options ... for next version
            menuReports.Visible = false;
            menuToolsS1.Visible = false;
            toolPrintReports.Visible = false;
            toolStripSeparator2.Visible = false;
#if DEBUG
            menuKNoteLab.Visible = true;
#endif
        }

        private void Store_ComponentNotification(object sender, ComponentEventArgs<string> e)
        {
            string comName;
            if (!string.IsNullOrEmpty(e?.Entity.ToString()))
                comName = ((ComponentBase)sender)?.ComponentName + ": ";
            else
                comName = "";
            statusLabel2.Text = $" {comName} {e?.Entity.ToString()}";
            statusBarManagment.Refresh();            
        }

        #region IViewBase interface 

        public Control PanelView()
        {
            throw new NotImplementedException();
        }

        public void ShowView()
        {
            LinkComponents();
            Application.DoEvents();
            this.Show();
        }

        public void HideView()
        {
            this.Hide();
        }

        public void ActivateView()
        {
            this.Show();            
            this.Select();
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;
        }

        Result<EComponentResult> IViewBase.ShowModalView()
        {
            LinkComponents();
            Application.DoEvents();
            return _com.DialogResultToComponentResult(this.ShowDialog());
        }

        public void OnClosingView()
        {
            _viewFinalized = true;
            this.Close();
        }

        public DialogResult ShowInfo(string info, string caption = "KeyNoteX", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            if(info != null)
                return MessageBox.Show(info, caption, buttons, icon);

            string msg1;
            string msg2;

            if (_com.SelectMode == EnumSelectMode.Folders)
            {
                if (string.IsNullOrEmpty(_com.SelectedFolderInfo?.Name))
                    msg1 = "(No folder selected)";
                else
                    msg1 = _com.SelectedFolderInfo?.Name;
            }
            else
                msg1 = "(Filtered notes)";

            msg2 = $"{_com.FolderPath?.ToString()} ";

            if (menuHeaderPanelVisible.Checked)
            {
                labelFolder.Text = msg1;
                labelFolderDetail.Text = msg2;                
            }
            else
                Text = $"KaNote Managment - {msg2} ";

            statusLabel1.Text = $"Notes: {_com.CountNotes.ToString()}";
            
            return DialogResult.OK;
        }

        public void ConfigureEmbededMode()
        {
            
        }

        public void ConfigureWindowMode()
        {
            
        }

        public void RefreshView()
        {
            
        }

        #endregion

        #region Form events handlers

        private void KNoteManagmentForm_Load(object sender, EventArgs e)
        {
            SetViewPositionAndSize();
        }

        private async void KNoteManagmentForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_viewFinalized)
            {
                this.Hide();
                if (e.CloseReason == CloseReason.WindowsShutDown)
                {
                    SaveViewSizeAndPosition();
                    await _com.FinalizeAppForce();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private async void menu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuSel;
            menuSel = (ToolStripMenuItem)sender;

            if (menuSel == menuKNoteLab) {
                // For test ...
                var labForm = new LabForm(_com.Store);
                labForm.Show(); }
            else if (menuSel == menuNewFolder)
            {
                _com.NewFolder();            
            }            
            else if (menuSel == menuEditFolder)
            {
                _com.EditFolder();            
            }
            else if (menuSel == menuDeleteFolder)
            {
                _com.DeleteFolder();           
            }            
            else if (menuSel == menuRemoveRepositoryLink)
            {
                _com.RemoveRepositoryLink();            
            }
            else if (menuSel == menuAddRepositoryLink)
            {
                _com.AddRepositoryLink();            
            }
            else if (menuSel == menuCreateRepository)
            {
                _com.CreateRepository();            
            }
            else if (menuSel == menuManagmentRepository)
            {
                _com.ManagmentRepository();            
            }
            else if (menuSel == menuRefreshTreeFolders) {
                Text = "KaNote Managment";
                _com.RefreshRepositoryAndFolderTree(); }
            else if (menuSel == menuEditNote)
            {
                _com.EditNote();            
            }
            else if (menuSel == menuEditNoteAsPostIt)
            {
                _com.EditNotePostIt();            
            }
            else if (menuSel == menuNewNote)
            {
                _com.AddNote();            
            }
            else if (menuSel == menuNewNoteAsPostIt)
            {
                _com.AddNotePostIt();                        
            }
            else if (menuSel == menuDeleteNote)
            {
                _com.DeleteNote();                            
            }
            else if (menuSel == menuKntScriptConsole)
            {
                _com.ShowKntScriptConsole();            
            }
            else if (menuSel == menuHide)
            {
                _com.HideKNoteManagment();
            }
            else if (menuSel == menuAbout)
            {
                _com.About();
            }
            else if (menuSel == menuHelpDoc)
            {
                _com.Help();
            }
            else if (menuSel == menuMoveSelectedNotes)
            {
                _com.MoveSelectedNotes();            
            }
            else if (menuSel == menuAddTags)
            {
                _com.AddTagsSelectedNotes();            
            }
            else if (menuSel == menuRemoveTags)
            {
                _com.RemoveTagsSelectedNotes();
            }
            else if (menuSel == menuExecuteKntScript)
            {
                _com.RunScriptSelectedNotes();
            }
            else if (menuSel == menuOptions)
            {
                _com.Options();
            }
            else if (menuSel == menuFoldersExplorer) {
                if (tabExplorers.SelectedIndex == 0)
                    return;
                SelectTab(0); }
            else if (menuSel == menuSearchPanel) {
                if (tabExplorers.SelectedIndex == 1)
                    return;
                SelectTab(1); }            
            else if (menuSel == menuHeaderPanelVisible) {
                if(!panelSupManagment.Visible)
                    Text = "KaNote Managment";
                panelSupManagment.Visible = !panelSupManagment.Visible; }
            else if (menuSel == menuMainVisible) {
                menuMangment.Visible = !menuMangment.Visible;
                menuMainVisible.Checked = !menuMainVisible.Checked; }
            else if (menuSel == menuVerticalPanelForNotes) {
                if(splitContainer2.Orientation == Orientation.Horizontal) {
                    splitContainer2.Orientation = Orientation.Vertical;
                    menuVerticalPanelForNotes.Checked = true; }
                else {
                    splitContainer2.Orientation = Orientation.Horizontal;
                    menuVerticalPanelForNotes.Checked = false; } }            
            else if (menuSel == menuExit)
            {
                SaveViewSizeAndPosition();
                await _com.FinalizeApp();            
            }
            else
                MessageBox.Show("In construction ... ");            
        }

        private void buttonToolBar_Click(object sender, EventArgs e)
        {
            ToolStripItem menuSel;
            menuSel = (ToolStripItem)sender;

            if (menuSel == toolEditNote)
                _com.EditNote();
            else if (menuSel == toolNewNote)
                _com.AddNote();
            else if (menuSel == toolDeleteNote)
                _com.DeleteNote();
            else if (menuSel == toolConfiguration)
                _com.ManagmentRepository();

        }

        private void tabExplorers_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectTab(tabExplorers.SelectedIndex);
        }

        #endregion

        #region Private methods

        private void SelectTab(int tabIndex)
        {
            if(tabIndex == 0)
            {
                tabExplorers.SelectedTab = tabExplorers.TabPages[0];
                menuFoldersExplorer.Checked = true;
                menuSearchPanel.Checked = false;
                _com.GoActiveFolder();
            }
            else if (tabIndex == 1)
            {
                tabExplorers.SelectedTab = tabExplorers.TabPages[1];
                menuFoldersExplorer.Checked = false;
                menuSearchPanel.Checked = true;
                _com.GoActiveFilter();
            }
        }

        private void LinkComponents()
        {
            tabTreeFolders.Controls.Add(_com.FoldersSelectorComponent.View.PanelView());
            tabSearch.Controls.Add(_com.FilterParamComponent.View.PanelView());
            splitContainer2.Panel1.Controls.Add(_com.NotesSelectorComponent.View.PanelView());
            splitContainer2.Panel2.Controls.Add(_com.NoteEditorComponent.View.PanelView());           
        }

        private void SaveViewSizeAndPosition()
        {
            if (WindowState == FormWindowState.Minimized)
                return;

            _com.Store.AppConfig.ManagmentLocX = Location.X;
            _com.Store.AppConfig.ManagmentLocY = Location.Y;
            _com.Store.AppConfig.ManagmentWidth = Width;
            _com.Store.AppConfig.ManagmentHeight = Height;
        }

        private void SetViewPositionAndSize()
        {
            if (_com.Store.AppConfig.ManagmentLocX > SystemInformation.VirtualScreen.Width - 100)
                _com.Store.AppConfig.ManagmentLocX = 100;
            if (_com.Store.AppConfig.ManagmentLocY > SystemInformation.VirtualScreen.Height - 100)
                _com.Store.AppConfig.ManagmentLocY = 100;

            if (_com.Store.AppConfig.ManagmentLocY > 0)
                Top = _com.Store.AppConfig.ManagmentLocY;
            if (_com.Store.AppConfig.ManagmentLocX > 0)
                Left = _com.Store.AppConfig.ManagmentLocX;
            if (_com.Store.AppConfig.ManagmentWidth > 0)
                Width = _com.Store.AppConfig.ManagmentWidth;
            if (_com.Store.AppConfig.ManagmentHeight > 0)
                Height = _com.Store.AppConfig.ManagmentHeight;
        }


        #endregion

    }

    public class WaitCursor : IDisposable
    {
        public WaitCursor()
        {
            Cursor.Current = Cursors.WaitCursor;            
        }

        public void Dispose()
        {
            Cursor.Current = Cursors.Default;            
        }
    }

}