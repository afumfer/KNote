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
    public partial class MessageEditorForm : Form, IEditorView<KMessageDto>
    {
        #region Private fields

        private readonly MessageEditorComponent _com;
        private bool _viewFinalized = false;
        private bool _formIsDisty = false;

        #endregion

        #region Constructor 

        public MessageEditorForm(MessageEditorComponent com)
        {
            InitializeComponent();
            PersonalizeControls();
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
            var res = _com.DialogResultToComponentResult(this.ShowDialog());
            return res;
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
            textUserFullName.Text = "";
            textAlarmDateTime.Text = "";
            comboAlarmPeriodicity.SelectedIndex = 0;
            comboNotificationType.SelectedIndex = 0;
            textContent.Text = "";
            checkAlarmActivated.Checked = true;
        }

        public void ConfigureEmbededMode()
        {
            
        }

        public void ConfigureWindowMode()
        {
            
        }

        public void OnClosingView()
        {
            _viewFinalized = true;
            this.Close();
        }

        #endregion

        #region Form event handlers

        private void MessageEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_viewFinalized)
            {
                var confirmExit = OnCandelEdition();
                if (!confirmExit)
                    e.Cancel = true;
            }
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
            OnCandelEdition();
        }

        private void MessageEditorForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
                _formIsDisty = true;
        }

        private void MessageEditorForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            _formIsDisty = true;
        }

        private void MessageEditorForm_Load(object sender, EventArgs e)
        {

        }

        private void buttonSelectDate_Click(object sender, EventArgs e)
        {
            DateTime selDate;
            if (!DateTime.TryParse(textAlarmDateTime.Text, out selDate))
                selDate = DateTime.Now;

            DateSelectorForm dateSelector = new DateSelectorForm();
            dateSelector.Date = selDate;

            if (dateSelector.ShowDialog() == DialogResult.OK)
                textAlarmDateTime.Text = dateSelector.Date.ToString("dd/MM/yyyy HH:mm");
        }

        #endregion

        #region Private methods

        private bool OnCandelEdition()
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

        private void PersonalizeControls()
        {
            foreach (var alarmType in KntConst.AlarmType)
                comboAlarmPeriodicity.Items.Add(alarmType);
            comboAlarmPeriodicity.ValueMember = "Key";
            comboAlarmPeriodicity.DisplayMember = "Value";
            comboAlarmPeriodicity.SelectedIndex = 0;

            foreach (var notType in KntConst.NotificationType)
                comboNotificationType.Items.Add(notType);
            comboNotificationType.ValueMember = "Key";
            comboNotificationType.DisplayMember = "Value";
            comboNotificationType.SelectedIndex = 0;
        }

        private void ModelToControls()
        {
            textUserFullName.Text = _com.Model.UserFullName?.ToString();
            textAlarmDateTime.Text = _com.Model.AlarmDateTime.ToString();
            comboAlarmPeriodicity.SelectedIndex = (int)_com.Model.AlarmType;
            comboNotificationType.SelectedIndex = (int)_com.Model.NotificationType;
            textContent.Text = _com.Model.Content.ToString();
            checkAlarmActivated.Checked = _com.Model.AlarmActivated ?? false;
        }

        private void ControlsToModel()
        {
            _com.Model.AlarmDateTime = _com.TextToDateTime(textAlarmDateTime.Text);
            _com.Model.AlarmType = (EnumAlarmType)comboAlarmPeriodicity.SelectedIndex;
            _com.Model.NotificationType = (EnumNotificationType)comboNotificationType.SelectedIndex;
            _com.Model.Content = textContent.Text;
            _com.Model.AlarmActivated = checkAlarmActivated.Checked;
        }

        #endregion 
    }
}
