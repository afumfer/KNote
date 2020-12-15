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
    public partial class KNoteManagmentForm : Form, IViewConfigurable
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

        Result<EComponentResult> IViewBase.ShowModalView()
        {
            LinkComponents();
            Application.DoEvents();
            return _com.DialogResultToComponentResult(this.ShowDialog());
        }

        public void RefreshView()
        {
            //
        }

        public void OnClosingView()
        {
            _viewFinalized = true;
            this.Close();
        }

        public DialogResult ShowInfo(string info, string caption = "KeyNote", MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            if(info != null)
                return MessageBox.Show(info);

            if (string.IsNullOrEmpty(_com.SelectedFolderInfo?.Name))
                labelFolerName.Text = "(No folder selected)";
            else
                labelFolerName.Text = _com.SelectedFolderInfo?.Name;
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


        #endregion

        #region Form events handlers

        private void menu_Click(object sender, EventArgs e)
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
            else if (menuSel == menuNewNote)
            {
                _com.AddNote();
            }
            else if (menuSel == menuDeleteNote)
            {
                _com.DeleteNote();
                
            }
            else if (menuSel == menuKntScriptConsole)
            {
                _com.ShowKntScriptConsole();
            }
            else if (menuSel == menuExit)
            {
                if (MessageBox.Show("Are you sure exit KNote?", "KNote", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)                
                    _com.Finalize();                
            }
            else
            {
                MessageBox.Show("Option in development ... ");
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
                _com.Finalize();
        }

        #endregion

        #region Private methods

        private void LinkComponents()
        {
            tabTreeFolders.Controls.Add(_com.FoldersSelectorComponent.View.PanelView());
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
