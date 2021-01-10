using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using Markdig;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KNote.ClientWin.Views
{
    public partial class PostItPropertiesForm : Form, IEditorView<WindowDto>
    {
        private readonly PostItPropertiesComponent _com;
        private bool _viewFinalized = false;
        private bool _formIsDisty = false;

        public PostItPropertiesForm(PostItPropertiesComponent com)
        {
            InitializeComponent();
            _com = com;
        }

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
        public DialogResult ShowInfo(string info, string caption = "KeyNote", MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            return MessageBox.Show(info, caption, buttons);
        }

        public void RefreshView()
        {
            ModelToControls();
        }

        public void CleanView()
        {
            // txt = "";            
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

        private async void PostItPropertiesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_viewFinalized)
            {
                var savedOk = await SaveModel();
                if (!savedOk)
                {
                    ShowInfo("The note could not be saved");
                }
                _com.Finalize();
            }
        }

        private async Task<bool> SaveModel()
        {
            ControlsToModel();
            return await _com.SaveModel();
        }

        private void ModelToControls()
        {

        }

        private void ControlsToModel()
        {

        }

        private async void buttonAccept_Click(object sender, EventArgs e)
        {
            ControlsToModel();
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

        private bool OnCancelEdition()
        {
            if (_formIsDisty)
            {
                if (MessageBox.Show("You have modified this entity, are you sure you want to exit without recording?", "KeyNote", MessageBoxButtons.YesNo) == DialogResult.No)
                    return false;
            }

            this.DialogResult = DialogResult.Cancel;
            _com.CancelEdition();
            return true;
        }

        private void PostItPropertiesForm_Load(object sender, EventArgs e)
        {
            // TODO: ... for debug ....
            _formIsDisty = true;
        }
    }
}
