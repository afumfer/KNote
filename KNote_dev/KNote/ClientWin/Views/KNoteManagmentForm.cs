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

        public void ShowInfo(string info)
        {
            if(info != null)
                MessageBox.Show(info);            
            labelFolerName.Text = _com.SelectedFolderInfo?.Name;
            labelFolderDetail.Text = $"{_com.SelectedServiceRef?.Alias} - [{_com.SelectedServiceRef?.Provider}]";
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
            //ToolStripMenuItem menuSel;
            //menuSel = (ToolStripMenuItem)sender;

            //// For TEST ...
            //if (menuSel == menuKNoteLab)
            //{
            //    var testForm = new TestForm();
            //    testForm.Show();
            //}
            //// Editar tarea
            //else if (menuSel == menuExit)
            //{
            //    if (MessageBox.Show("Are you sure get out KNote?", "KNote",
            //                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            //    {
            //        _ctrl.FinalizeCtrl();
            //    }
            //}
            //else if (menuSel == menuAntScriptConsole)
            //{
            //    try
            //    {
            //        _ctrl.ShowAnTScriptConsole();
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show(ex.ToString());
            //    }
            //}
            //else
            //{
            //    MessageBox.Show("Option in development ... ");
            //}
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
}
