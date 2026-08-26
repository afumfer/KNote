using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

public partial class AttributeEditorForm : Form, IViewEditor<KAttributeDto>
{
    #region Private fields

    private readonly AttributeEditorCtrl _ctrl;
    private bool _viewFinalized = false;
    private bool _formIsDisty = false;

    // Sentinel item for "no note type" (KAttributeInfoDto.NoteTypeId is nullable): a real
    // NoteTypeDto so comboNoteType.DisplayMember="Name" works uniformly for every item, with an
    // explicit Name so NoteTypeDto's own "(Enter new type note name)" placeholder default (for a
    // null Name) never shows up here.
    private static readonly NoteTypeDto NoNoteTypeItem = new() { NoteTypeId = Guid.Empty, Name = "(none)" };

    #endregion

    #region Constructor

    public AttributeEditorForm(AttributeEditorCtrl ctrl)
    {
        InitializeComponent();
        PersonalizeControls();

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
        textName.Text = "";
        textDescription.Text = "";
        checkRequiredValue.Checked = false;
        numericOrder.Value = 0;
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

    private void AttributeEditorForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
        {
            var confirmExit = OnCandelEdition();
            if (!confirmExit)
                e.Cancel = true;
        }
    }

    private void AttributeEditorForm_KeyPress(object sender, KeyPressEventArgs e)
    {
        _formIsDisty = true;
    }

    private void AttributeEditorForm_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            _formIsDisty = true;
    }

    private void comboDataType_SelectedIndexChanged(object sender, EventArgs e)
    {
        RefreshTabulatedValuesVisibility();
    }

    private async void buttonAddTabValue_Click(object sender, EventArgs e)
    {
        var value = await _ctrl.NewTabulatedValue();
        if (value != null)
            listViewTabulatedValues.Items.Add(TabulatedValueToListViewItem(value));
    }

    private void buttonDeleteTabValue_Click(object sender, EventArgs e)
    {
        var selected = SelectedTabulatedValue();
        if (selected == Guid.Empty)
        {
            MessageBox.Show("There is no tabulated value selected.", KntConst.AppName);
            return;
        }
        if (_ctrl.DeleteTabulatedValue(selected))
            listViewTabulatedValues.Items[selected.ToString()]?.Remove();
    }

    private void buttonEditTabValue_Click(object sender, EventArgs e)
    {
        EditSelectedTabulatedValue();
    }

    private void listViewTabulatedValues_DoubleClick(object sender, EventArgs e)
    {
        EditSelectedTabulatedValue();
    }

    private void listViewTabulatedValues_Resize(object sender, EventArgs e)
    {
        SizeLastColumn(listViewTabulatedValues);
    }

    #endregion

    #region Private methods

    private void EditSelectedTabulatedValue()
    {
        var selected = SelectedTabulatedValue();
        if (selected == Guid.Empty)
        {
            MessageBox.Show("There is no tabulated value selected.", KntConst.AppName);
            return;
        }
        var value = _ctrl.EditTabulatedValue(selected);
        if (value != null)
            UpdateTabulatedValueItem(value);
    }

    private Guid SelectedTabulatedValue()
    {
        if (listViewTabulatedValues.SelectedItems.Count == 0)
            return Guid.Empty;
        return Guid.Parse(listViewTabulatedValues.SelectedItems[0].Name);
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

    private void PersonalizeControls()
    {
        comboDataType.DisplayMember = "Value";
        comboDataType.ValueMember = "Key";
        foreach (var dataType in KntConst.KAttributes)
            comboDataType.Items.Add(dataType);

        comboNoteType.DisplayMember = "Name";
        listViewTabulatedValues.View = View.Details;
        listViewTabulatedValues.LabelEdit = false;
        listViewTabulatedValues.AllowColumnReorder = false;
        listViewTabulatedValues.CheckBoxes = false;
        listViewTabulatedValues.FullRowSelect = true;
        listViewTabulatedValues.GridLines = true;
        listViewTabulatedValues.Sorting = SortOrder.None;
    }

    private void ModelToControls()
    {
        textName.Text = _ctrl.Model.Name;
        textDescription.Text = _ctrl.Model.Description;
        checkRequiredValue.Checked = _ctrl.Model.RequiredValue;
        numericOrder.Value = _ctrl.Model.Order;

        comboNoteType.Items.Clear();
        comboNoteType.Items.Add(NoNoteTypeItem);
        foreach (var noteType in _ctrl.NoteTypes)
            comboNoteType.Items.Add(noteType);
        comboNoteType.SelectedItem = _ctrl.Model.NoteTypeId == null
            ? NoNoteTypeItem
            : _ctrl.NoteTypes.FirstOrDefault(_ => _.NoteTypeId == _ctrl.Model.NoteTypeId) ?? NoNoteTypeItem;

        var dataTypeIndex = comboDataType.Items.Cast<KeyValuePair<EnumKAttributeDataType, string>>()
            .ToList()
            .FindIndex(_ => _.Key == _ctrl.Model.KAttributeDataType);
        comboDataType.SelectedIndex = dataTypeIndex >= 0 ? dataTypeIndex : 0;

        listViewTabulatedValues.Clear();
        listViewTabulatedValues.Columns.Add("Value", 150, HorizontalAlignment.Left);
        listViewTabulatedValues.Columns.Add("Description", 150, HorizontalAlignment.Left);
        listViewTabulatedValues.Columns.Add("Order", -2, HorizontalAlignment.Left);
        foreach (var value in _ctrl.Model.KAttributeValues)
            listViewTabulatedValues.Items.Add(TabulatedValueToListViewItem(value));

        RefreshTabulatedValuesVisibility();
    }

    private void ControlsToModel()
    {
        _ctrl.Model.Name = textName.Text;
        _ctrl.Model.Description = textDescription.Text;
        _ctrl.Model.RequiredValue = checkRequiredValue.Checked;
        _ctrl.Model.Order = (int)numericOrder.Value;

        var selectedNoteType = comboNoteType.SelectedItem as NoteTypeDto;
        if (selectedNoteType == null || selectedNoteType.NoteTypeId == Guid.Empty)
        {
            _ctrl.Model.NoteTypeId = null;
            _ctrl.Model.NoteTypeDto = null;
        }
        else
        {
            _ctrl.Model.NoteTypeId = selectedNoteType.NoteTypeId;
            _ctrl.Model.NoteTypeDto = selectedNoteType;
        }

        if (comboDataType.SelectedItem is KeyValuePair<EnumKAttributeDataType, string> selectedDataType)
            _ctrl.Model.KAttributeDataType = selectedDataType.Key;
    }

    private void RefreshTabulatedValuesVisibility()
    {
        if (comboDataType.SelectedItem is not KeyValuePair<EnumKAttributeDataType, string> selected)
            return;

        panelTabulatedValues.Visible = selected.Key == EnumKAttributeDataType.TabulatedValue
            || selected.Key == EnumKAttributeDataType.TagsValue;
    }

    private ListViewItem TabulatedValueToListViewItem(KAttributeTabulatedValueDto value)
    {
        var item = new ListViewItem(value.Value) { Name = value.KAttributeTabulatedValueId.ToString() };
        item.SubItems.Add(value.Description);
        item.SubItems.Add(value.Order.ToString());
        return item;
    }

    private void UpdateTabulatedValueItem(KAttributeTabulatedValueDto value)
    {
        var listItem = listViewTabulatedValues.Items[value.KAttributeTabulatedValueId.ToString()];
        if (listItem == null)
            return;

        listItem.Text = value.Value;
        listItem.SubItems[1].Text = value.Description;
        listItem.SubItems[2].Text = value.Order.ToString();
    }

    private void SizeLastColumn(ListView lv)
    {
        try
        {
            lv.Columns[lv.Columns.Count - 1].Width = -2;
        }
        catch (Exception) { }
    }

    #endregion
}
