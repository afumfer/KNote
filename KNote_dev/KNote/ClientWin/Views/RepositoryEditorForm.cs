﻿using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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

        private void RepositoryEditorForm_Load(object sender, EventArgs e)
        {
            this.Height = 350;            
            panelSqLite.BorderStyle = BorderStyle.None;
            panelMSSqlServer.BorderStyle = BorderStyle.None;
            panelMSSqlServer.Top = panelSqLite.Top;
            panelMSSqlServer.Left = panelSqLite.Left;
        }

        private void radioDataBase_CheckedChanged(object sender, EventArgs e)
        {
            RefreshRadioDatabase();
        }

        #endregion 

        #region Private methods

        private void ModelToControls()
        {
            switch (_com.EditorMode)
            {
                case EnumRepositoryEditorMode.AddLink:
                    Text = "Add link to existing repository";
                    break;
                case EnumRepositoryEditorMode.Create:
                    Text = "Create new repository";
                    break;
                case EnumRepositoryEditorMode.Managment:
                    //Text = "Managment repository";
                    Text = "Edit repository properties";
                    groupRepositoryType.Enabled = false;
                    panelSqLite.Enabled = false;
                    panelMSSqlServer.Enabled = false;
                    break;
            }

            textAliasName.Text = _com.Model.Alias;

            var connecionValues = GetConnectionValues(_com.Model.ConnectionString);
            if (_com.Model.Provider == "Microsoft.Data.Sqlite")
            {
                // @"Data Source=D:\DBs\KNote05DB_Sqlite.db",                
                textSqLiteDirectory.Text = Path.GetDirectoryName(connecionValues["Data Source"]) ;
                textSqLiteDataBase.Text = Path.GetFileName(connecionValues["Data Source"]);
                radioSqLite.Checked = true;                
            }
            else
            {
                // @"Data Source=.\sqlexpress;Initial Catalog=KNote05DB;Trusted_Connection=True;Connection Timeout=60;MultipleActiveResultSets=true;                
                textSQLServer.Text = connecionValues["Data Source"];
                textSQLDataBase.Text = connecionValues["Initial Catalog"];
                radioMSSqlServer.Checked = true;                
            }
        }

        private void ControlsToModel()
        {
            _com.Model.Alias = textAliasName.Text;
            if (radioSqLite.Checked)
            {
                _com.Model.Provider = "Microsoft.Data.Sqlite";
                _com.Model.ConnectionString = Path.Combine(textSqLiteDirectory.Text, textSqLiteDataBase.Text);
            }
            else
            {
                _com.Model.Provider = "Microsoft.Data.SqlClient";
                _com.Model.ConnectionString = $"Data Source={textSQLServer.Text}; Initial Catalog={textSQLDataBase.Text}; Trusted_Connection=True; Connection Timeout=60; MultipleActiveResultSets=true;";
            }

            if (_com.EditorMode == EnumRepositoryEditorMode.AddLink || _com.EditorMode == EnumRepositoryEditorMode.Create)
                _com.Model.Orm = "EntityFramework";
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

        private Dictionary<string, string> GetConnectionValues(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException("Invalid connection string");

            try
            {
                var connectValues = new Dictionary<string, string>();
                var arrayValues = connectionString.Split(';');

                foreach(var strCon in arrayValues)
                {
                    if (!string.IsNullOrEmpty(strCon))
                    {
                        var keyValue = strCon.Trim().Split("=");
                        connectValues.Add(keyValue[0], keyValue[1]);
                    }
                }

                return connectValues;
            }
            catch (Exception)
            {
                throw;
            }            
        }

        private void RefreshRadioDatabase()
        {
            if (radioSqLite.Checked == true)
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

        #endregion

    }
}
