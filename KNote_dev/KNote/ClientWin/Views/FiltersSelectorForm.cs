using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service;
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
    public partial class FiltersSelectorForm : Form, ISelectorView<NotesFilterWithServiceRef>
    {
        private FiltersSelectorComponent _com;
        private bool _viewFinalized = false;
        //private bool _formIsDisty = false;

        public FiltersSelectorForm(FiltersSelectorComponent com)
        {
            InitializeComponent();
            _com = com;            
        }

        #region ISelectorView implementation

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
            PersonalizeControls();
            ModelToControls();
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


        public DialogResult ShowInfo(string info, string caption = "KaNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Asterisk)
        {
            return MessageBox.Show(info, caption, buttons, icon);
        }

        public void RefreshItem(NotesFilterWithServiceRef item)
        {
            throw new NotImplementedException();
        }

        public void DeleteItem(NotesFilterWithServiceRef item)
        {
            throw new NotImplementedException();
        }

        public void AddItem(NotesFilterWithServiceRef item)
        {
            throw new NotImplementedException();
        }

        public object SelectItem(NotesFilterWithServiceRef item)
        {
            throw new NotImplementedException();
        }

        public List<NotesFilterWithServiceRef> GetSelectedListItem()
        {
            throw new NotImplementedException();
        }

        #endregion

        private void FilterParamForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_viewFinalized)
                _com.Finalize();
        }

        private void buttonClean_Click(object sender, EventArgs e)
        {
            CleanView();
            buttonSearch_Click(this, e);
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            var filter = new NotesFilterWithServiceRef();
            filter.ServiceRef = (ServiceRef)comboRepositories.SelectedItem;
            if(!checkSearchInDescription.Checked)
                filter.NotesFilter = new NotesFilterDto { TextSearch = textTextSearch.Text };
            else
                filter.NotesFilter = new NotesFilterDto { TextSearch = $"*** {textTextSearch.Text}"  };

            _com.SelectedEntity = filter;            
            _com.NotifySelectedEntity();
        }

        private void buttonAccept_Click(object sender, EventArgs e)
        {
            _com.Accept();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            _com.Cancel();
        }

        private void PersonalizeControls()
        {
            foreach (var serviceRef in _com.Store.GetAllServiceRef())
                comboRepositories.Items.Add(serviceRef);
            comboRepositories.ValueMember = "IdServiceRef";
            comboRepositories.DisplayMember = "Alias";
            comboRepositories.SelectedIndex = 0;
        }

        private void ModelToControls()
        {
            // TODO: ...
            //if(_com.Store.ActiveFolderWithServiceRef != null)
            //    comboRepositories.SelectedItem = _com.Store.ActiveFolderWithServiceRef.ServiceRef.IdServiceRef;
        }

        private void CleanView()
        {
            textTextSearch.Text = "";
            checkSearchInDescription.Checked = true;
        }

        private void textTextSearch_KeyUp(object sender, KeyEventArgs e)
        {                        
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    buttonSearch_Click(this, new EventArgs());
                    break;
                case Keys.Escape:
                    buttonClean_Click(this, new EventArgs());
                    break;
            }            
        }
    }
}
