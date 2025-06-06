﻿using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

public partial class NoteAttributeEditorForm : Form, IViewEditor<NoteKAttributeDto>
{
    #region Private fields

    private readonly NoteAttributeEditorCtrl _ctrl;
    private bool _viewFinalized = false;
    private bool _formIsDisty = false;

    #endregion

    #region Constructor

    public NoteAttributeEditorForm(NoteAttributeEditorCtrl ctrl)
    {
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        _ctrl = ctrl;
    }

    #endregion 

    #region IEditorView implementation 

    public void ShowView()
    {
        this.Show();
    }

    public Result<EControllerResult> ShowModalView()
    {            
        var res = _ctrl.DialogResultToControllerResult(this.ShowDialog());
        return res;
    }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        return MessageBox.Show(info, caption, buttons, icon);
    }

    public void RefreshModel()
    {
        ControlsToModel();
    }

    public void CleanView()
    {
        textValue.Text = "";
        labelDescription.Text = "";
        labelAttribute.Text = "";
    }

    public void RefreshView()
    {
        ModelToControls();
    }

    public void ConfigureEmbededMode()
    {
        
    }

    public void ConfigureWindowMode()
    {
        
    }

    public Control PanelView()
    {
        return panelForm;
    }

    public void OnClosingView()
    {
        _viewFinalized = true;
        this.Close();
    }

    #endregion

    #region Form events handlers

    private async void buttonAccept_Click(object sender, EventArgs e)
    {            
        var res = await _ctrl.SaveModel();
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

    private void buttonSelDate_Click(object sender, EventArgs e)
    {
        textValue.Text = SelDate(textValue.Text);
    }

    private void NoteAttributeEditorForm_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            _formIsDisty = true;
    }

    private void NoteAttributeEditorForm_KeyPress(object sender, KeyPressEventArgs e)
    {
        _formIsDisty = true;
    }

    private void NoteAttributeEditorForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
        {
            var confirmExit = OnCandelEdition();
            if (!confirmExit)
                e.Cancel = true;
        }
    }

    #endregion

    #region Private methods

    private void ModelToControls()
    {
        labelDescription.Text = "* " + _ctrl.Model.Description;
        labelAttribute.Text = _ctrl.Model.Name;

        switch (_ctrl.Model.KAttributeDataType)
        {
            case EnumKAttributeDataType.String:
                ModelToControlText();
                break;
            case EnumKAttributeDataType.Int:                    
            case EnumKAttributeDataType.Double:
                ModelToControlText(true);
                break;
            case EnumKAttributeDataType.TextArea:
                ModelToControlTextArea();
                break;
            case EnumKAttributeDataType.DateTime:
                ModelToControlDateTime();
                break;
            case EnumKAttributeDataType.Bool:
                ModelToControlCheck();
                break;
            case EnumKAttributeDataType.TabulatedValue:
                ModelToControlCombo();
                break;
            case EnumKAttributeDataType.TagsValue:
                ModelToControlListView();
                break;
            default:
                break;
        }            
    }

    private void ModelToControlText(bool widthShort = false)
    {
        textValue.Location = new Point(10, 32);
        textValue.Multiline = false;
        if(widthShort)
            textValue.Size = new Size(200, 23);
        else
            textValue.Size = new Size(478, 23);
        textValue.Text = _ctrl.Model.Value?.ToString();
        textValue.Visible = true;
        textValue.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
    }

    private void ModelToControlTextArea()
    {
        textValue.Location = new Point(10, 32);
        textValue.Multiline = true;
        textValue.Size = new Size(478, 140);
        textValue.Text = _ctrl.Model.Value;
        textValue.Visible = true;
        textValue.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
    }

    private void ModelToControlCheck()
    {
        labelAttribute.Text = "Check option:";
        checkValue.Location = new Point(10, 32);
        checkValue.Checked = bool.Parse(_ctrl.Model.Value);
        checkValue.Text = _ctrl.Model.Name;
        checkValue.Visible = true;
        checkValue.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
    }

    private void ModelToControlDateTime()
    {
        ModelToControlText(true);
        buttonSelDate.Location = new Point(216, 32);
        buttonSelDate.Visible = true;
    }

    private async void ModelToControlCombo()
    {
        var res = await _ctrl.Service.KAttributes.GetKAttributeTabulatedValuesAsync(_ctrl.Model.KAttributeId);
        if (res.IsValid)
        {
            comboValue.Items.Add("");
            foreach (var atr in res.Entity)
                comboValue.Items.Add(atr.Value);
        }
        comboValue.SelectedItem = _ctrl.Model.Value;
        comboValue.Location = new Point(10, 32);
        comboValue.Size = new Size(478, 23);
        comboValue.Visible = true;
        comboValue.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
    }

    private async void ModelToControlListView()
    {
        PersonalizeListView(listViewValue);
        listViewValue.Columns.Add("Name", 450, HorizontalAlignment.Left);
        var res = await _ctrl.Service.KAttributes.GetKAttributeTabulatedValuesAsync(_ctrl.Model.KAttributeId);
        if (res.IsValid)
        {                
            foreach (var atr in res.Entity)
            {
                var itemList = new ListViewItem(atr.Value);
                if(_ctrl.Model.Value.IndexOf(atr.Value) >= 0 )
                    itemList.Checked = true;
                else
                    itemList.Checked = false;
                listViewValue.Items.Add(itemList);
            }
                
        }            
        listViewValue.Location = new Point(10, 32);
        listViewValue.Size = new Size(478, 140);
        listViewValue.Visible = true;
        listViewValue.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));            
    }

    private void PersonalizeListView(ListView listView)
    {
        listView.View = View.Details;
        listView.LabelEdit = false;
        listView.AllowColumnReorder = false;
        listView.CheckBoxes = true;
        listView.FullRowSelect = true;
        listView.GridLines = true;
        listView.MultiSelect = true;
        listView.HeaderStyle = ColumnHeaderStyle.None;
        listView.Sorting = SortOrder.None;            
    }

    private void ControlsToModel()
    {            
        switch (_ctrl.Model.KAttributeDataType)
        {
            case EnumKAttributeDataType.String:                    
            case EnumKAttributeDataType.Int:
            case EnumKAttributeDataType.Double:
            case EnumKAttributeDataType.TextArea:                    
            case EnumKAttributeDataType.DateTime:
                _ctrl.Model.Value = textValue.Text;
                break;
            case EnumKAttributeDataType.Bool:
                _ctrl.Model.Value = checkValue.Checked.ToString();
                break;
            case EnumKAttributeDataType.TabulatedValue:
                _ctrl.Model.Value = comboValue.SelectedItem.ToString();
                break;
            case EnumKAttributeDataType.TagsValue:
                var newValue = "";
                foreach(var it in listViewValue.Items)
                {
                    var item = (ListViewItem)it;
                    if (item.Checked)
                    {
                        if (!string.IsNullOrEmpty(newValue))
                            newValue += ", ";
                        newValue += item.Text;
                    }
                }
                _ctrl.Model.Value = newValue;
                break;
            default:
                break;
        }
    }

    private bool OnCandelEdition()
    {
        if (_formIsDisty)
        {
            if (MessageBox.Show("You have modified this entity, are you sure you want to exit without recording?", KntConst.AppName, MessageBoxButtons.YesNo) == DialogResult.No)
                return false;
        }

        this.DialogResult = DialogResult.Cancel;
        _ctrl.CancelEdition();
        return true;
    }

    private string SelDate(string date)
    {
        DateTime selDateIn;
        string selDate = "";

        if (!DateTime.TryParse(date, out selDateIn))
            selDateIn = DateTime.Now;

        DateSelectorForm dateSelector = new DateSelectorForm();
        dateSelector.Date = selDateIn;

        if (dateSelector.ShowDialog() == DialogResult.OK)
            selDate = dateSelector.Date.ToString("dd/MM/yyyy HH:mm");

        return selDate;
    }

    #endregion
}
