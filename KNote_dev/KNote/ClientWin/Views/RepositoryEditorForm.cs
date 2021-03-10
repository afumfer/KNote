using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KNote.ClientWin.Views
{
    public partial class RepositoryEditorForm : Form, IEditorView<RepositoryRef>
    {
        private readonly RepositoryEditorComponent _com;
        private bool _viewFinalized = false;
        private bool _formIsDisty = false;

        public RepositoryEditorForm(RepositoryEditorComponent com)
        {
            InitializeComponent();
            _com = com;
        }

        #region IEditorView implementation 

        public Control PanelView()
        {
            return panelForm;
        }

        public void ShowView()
        {
            this.Show();
        }

        public Result<EComponentResult> ShowModalView()
        {
            var res = _com.DialogResultToComponentResult(this.ShowDialog());
            return res;
        }

        public void RefreshView()
        {
            ModelToControls();
        }

        public void RefreshModel()
        {
            ControlsToModel();
        }

        public void CleanView()
        {
            //textAlias.Text = "";
        }

        public DialogResult ShowInfo(string info, string caption = "KaNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Asterisk)
        {
            return MessageBox.Show(info, caption, buttons, icon);
        }

        public void OnClosingView()
        {
            _viewFinalized = true;
            this.Close();
        }

        public void ConfigureEmbededMode()
        {
            
        }

        public void ConfigureWindowMode()
        {
            
        }

        #endregion

        #region Form events handler

        private async void buttonAccept_Click(object sender, EventArgs e)
        {
            var res = await _com.SaveModel();
            if (res)
            {
                _formIsDisty = false;
                this.DialogResult = DialogResult.OK;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            OnCancelEdition();
        }

        private void RepositoryEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_viewFinalized)
            {
                var confirmExit = OnCancelEdition();
                if (!confirmExit)
                    e.Cancel = true;
            }
        }

        private void RepositoryEditorForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            _formIsDisty = true;
        }

        private void RepositoryEditorForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
                _formIsDisty = true;
        }

        #endregion 

        #region Private methods

        private void ModelToControls()
        {
            textAliasName.Text = _com.Model.Alias;
            if (_com.Model.Provider == "Microsoft.Data.Sqlite")
            {
                // ...
                radioSqLite.Checked = true;
            }
            else
            {
                // ...
                radioSqLite.Checked = false;
            }
        }

        private void ControlsToModel()
        {

        }

        private bool OnCancelEdition()
        {
            if (_formIsDisty)
            {
                if (MessageBox.Show("You have modified this entity, are you sure you want to exit without recording?", "KaNote", MessageBoxButtons.YesNo) == DialogResult.No)
                    return false;
            }

            this.DialogResult = DialogResult.Cancel;
            _com.CancelEdition();
            return true;
        }

        #endregion

        private void RepositoryEditorForm_Load(object sender, EventArgs e)
        {
            this.Height = 350;
            // 
            panelSqLite.BorderStyle = BorderStyle.None;
            panelMSSqlServer.BorderStyle = BorderStyle.None;
            panelMSSqlServer.Top = panelSqLite.Top;
            panelMSSqlServer.Left = panelSqLite.Left;

            radioSqLite.Checked = true;
            panelSqLite.Visible = true;
            panelMSSqlServer.Visible = false;
        }

        private void radioDataBase_CheckedChanged(object sender, EventArgs e)
        {
            if(radioSqLite.Checked == true)
            {
                panelSqLite.Visible = true;
                panelMSSqlServer.Visible = false;
            }
            else
            {
                panelSqLite.Visible = false;
                panelMSSqlServer.Visible = true;
            }
        }

    }
}
