using KNote.ClientWin.Components;
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
    public partial class OptionsEditorForm : Form, IEditorView<AppConfig>
    {
        #region Fields

        private readonly OptionsEditorComponent _com;
        private bool _viewFinalized = false;
        private bool _formIsDisty = false;

        #endregion

        #region Constructor 

        public OptionsEditorForm(OptionsEditorComponent com)
        {
            InitializeComponent();
            _com = com;
        }

        #endregion 

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
            return _com.DialogResultToComponentResult(this.ShowDialog());
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
            // clear controls 
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

        #region Private methods

        private void ModelToControls() { }

        private void ControlsToModel() { }

        #endregion 

    }
}
