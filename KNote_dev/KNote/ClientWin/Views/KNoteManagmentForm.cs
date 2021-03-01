using KNote.ClientWin.Core;
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

            if(_com.SelectMode == EnumSelectMode.Folders)
            {
                if (string.IsNullOrEmpty(_com.SelectedFolderInfo?.Name))
                    labelFolerName.Text = "(No folder selected)";
                else
                    labelFolerName.Text = _com.SelectedFolderInfo?.Name;
            }
            else
                labelFolerName.Text = "(Filtered notes)";

            labelFolderDetail.Text = $"{_com.FolderPath?.ToString()} ";
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

        private async void menu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuSel;
            menuSel = (ToolStripMenuItem)sender;
            
            if (menuSel == menuKNoteLab)
            {
                // For test ...
                var labForm = new LabForm(_com.Store);
                labForm.Show();
            }
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
            else if (menuSel == menuExecuteKntScript)
            {
                _com.RunScriptSelectedNote();
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
            else if (menuSel == menuMoveSelectedNotes)
            {
                _com.MoveSelectedNotes();
            }
            else if (menuSel == menuExit)
            {
                await _com.FinalizeApp();
            }
            else
            {
                MessageBox.Show("Option under development ... ");
            }
        }

        private void buttonToolBar_Click(object sender, EventArgs e)
        {
            ToolStripItem menuSel;
            menuSel = (ToolStripItem)sender;

            if (menuSel == toolEditNote)
            {
                _com.EditNote();
            }
            else if (menuSel == toolNewNote)
            {
                _com.AddNote();
            }
            else if (menuSel == toolDeleteNote)
            {
                _com.DeleteNote();
            }
        }

        private void KNoteManagmentForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_viewFinalized)
            {
                this.Hide();
                e.Cancel = true;
            }
        }

        private void tabExplorers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabExplorers.SelectedIndex == 0)
                _com.GoActiveFolder();
            else
            {
                _com.GoActiveFilter();
            }
        }

        #endregion

        #region Private methods

        private void LinkComponents()
        {
            tabTreeFolders.Controls.Add(_com.FoldersSelectorComponent.View.PanelView());
            tabSearch.Controls.Add(_com.FilterParamComponent.View.PanelView());
            splitContainer2.Panel1.Controls.Add(_com.NotesSelectorComponent.View.PanelView());
            splitContainer2.Panel2.Controls.Add(_com.NoteEditorComponent.View.PanelView());           
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
