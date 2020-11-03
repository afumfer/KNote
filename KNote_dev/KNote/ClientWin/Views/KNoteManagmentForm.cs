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

namespace KNote.ClientWin.Views
{
    public partial class KNoteManagmentForm : Form, IViewBase
    {
        private readonly KNoteManagmentComponent _com;
        private bool _viewFinalized = false;

        public KNoteManagmentForm(KNoteManagmentComponent com)
        {
            InitializeComponent();

            _com = com;
        }

        #region IViewBase interface 

        public void OnClosingView()
        {
            this.Close();
        }

        public void RefreshView()
        {
            //// TODO: la actualización de estos componentes la vamos a coger en futuro
            ////       mediante la escucha de eventos del contexto cada vez que 
            ////       cambia la capeta seleccionada. 
            //var folderName = _ctrl.Context.ActiveFolderWithServiceRef?.FolderInfo?.Name
            //    + " (#"
            //    + _ctrl.Context.ActiveFolderWithServiceRef?.FolderInfo?.FolderNumber
            //    + ")";
            //var serviceName = _ctrl.Context.ActiveFolderWithServiceRef?.ServiceRef?.Name
            //    + " ("
            //    + _ctrl.Context.ActiveFolderWithServiceRef?.ServiceRef?.Provider
            //    + ")";

            //statusLabel2.Text = serviceName;
            //labelFolerName.Text = folderName;
        }

        public void ShowInfo(string info)
        {
            //MessageBox.Show(info);            
            statusLabel1.Text = info;
        }

        public void ShowView()
        {
            //RefreshView();

            //if (_ctrl.ModalMode)
            //    this.ShowDialog();
            //else
            //{
            //    this.Show();
            //}
            //Application.DoEvents();
            //InitializeCtrComponents();
        }

        #endregion 

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

        #region Private methods

        private void InitializeCtrComponents()
        {
            //tabTreeFolders.Controls.Add((Form)_ctrl.FoldersSelectorCtrl.View);
            //splitContainer2.Panel1.Controls.Add((Form)_ctrl.NotesSelectorCtrl.View);
            //splitContainer2.Panel2.Controls.Add((Form)_ctrl.NoteEditorCtrl.View);
        }

        #endregion

    }
}
