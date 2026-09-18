using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Utils;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

public partial class AttributeEditorForm : KntEditorForm, IViewEditor<KAttributeDto>
{
    #region Private fields

    private readonly AttributeEditorCtrl _ctrl;

    // Sentinel item for "no note type" (KAttributeInfoDto.NoteTypeId is nullable): a real
    // NoteTypeDto so comboNoteType.DisplayMember="Name" works uniformly for every item, with an
    // explicit Name so NoteTypeDto's own "(Enter new type note name)" placeholder default (for a
    // null Name) never shows up here.
    private static readonly NoteTypeDto NoNoteTypeItem = new() { NoteTypeId = Guid.Empty, Name = "(none)" };

    // Primary/growing column for ListViewColumnResizer - "Description" is the tabulated value's
    // descriptive, most variable-length column; "Value"/"Order" stay at their designer width.
    private const int PrimaryColumnIndex = 1;

    private ListViewColumnSorter _sorter;

    #endregion

    #region Constructor

    public AttributeEditorForm(AttributeEditorCtrl ctrl)
    {
        InitializeComponent();
        PersonalizeControls();

        _ctrl = ctrl;
    }

    #endregion

    #region Form events handlers

    private async void buttonAccept_Click(object sender, EventArgs e)
    {
        await AcceptEditionAsync();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
        TryCancelEdition();
    }

    protected override Task<bool> SaveModelAsync()
        => _ctrl.SaveModel();

    protected override void CancelEdition()
        => _ctrl.CancelEdition();

    private void comboDataType_SelectedIndexChanged(object sender, EventArgs e)
    {
        RefreshTabulatedValuesVisibility();
    }

    private async void buttonAddTabValue_Click(object sender, EventArgs e)
    {
        var value = await _ctrl.NewTabulatedValue();
        if (value != null)
        {
            listViewTabulatedValues.Items.Add(TabulatedValueToListViewItem(value));
            ListViewSelectionHelper.SelectByKey(listViewTabulatedValues, value.KAttributeTabulatedValueId.ToString(), _sorter);
        }
    }

    private void buttonDeleteTabValue_Click(object sender, EventArgs e)
    {
        var selected = SelectedTabulatedValue();
        if (selected == Guid.Empty)
        {
            KntMessageBox.Show("There is no tabulated value selected.", KntConst.AppName);
            return;
        }
        if (_ctrl.DeleteTabulatedValue(selected))
        {
            listViewTabulatedValues.Items[selected.ToString()]?.Remove();
            ListViewSelectionHelper.SelectFirst(listViewTabulatedValues, _sorter);
        }
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
        ListViewColumnResizer.Resize(listViewTabulatedValues, PrimaryColumnIndex);
    }

    #endregion

    #region Private methods

    private void EditSelectedTabulatedValue()
    {
        var selected = SelectedTabulatedValue();
        if (selected == Guid.Empty)
        {
            KntMessageBox.Show("There is no tabulated value selected.", KntConst.AppName);
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

    private void PersonalizeControls()
    {
        comboDataType.DisplayMember = "Value";
        comboDataType.ValueMember = "Key";
        foreach (var dataType in KntConst.KAttributes)
            comboDataType.Items.Add(dataType);

        comboNoteType.DisplayMember = "Name";
        ListViewStyle.ApplyStandard(listViewTabulatedValues);
        _sorter = ListViewSortHelper.Attach(listViewTabulatedValues);
    }

    protected override void ModelToControls()
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
        // Only the primary column (see PrimaryColumnIndex) is ever resized dynamically - every other
        // column needs a real, fixed pixel width here: a "-2" (native auto-size) width on a column
        // nothing else manages is what used to leave "Order" collapsed.
        listViewTabulatedValues.Columns.Add("Value", 150, HorizontalAlignment.Left);
        listViewTabulatedValues.Columns.Add("Description", 150, HorizontalAlignment.Left);
        listViewTabulatedValues.Columns.Add("Order", 60, HorizontalAlignment.Left);
        foreach (var value in _ctrl.Model.KAttributeValues)
            listViewTabulatedValues.Items.Add(TabulatedValueToListViewItem(value));
        ListViewSortHelper.ApplyInitialOrder(listViewTabulatedValues, _sorter);
        ListViewColumnResizer.Resize(listViewTabulatedValues, PrimaryColumnIndex);

        RefreshTabulatedValuesVisibility();
    }

    protected override void ControlsToModel()
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

        ListViewSelectionHelper.SelectByKey(listViewTabulatedValues, value.KAttributeTabulatedValueId.ToString(), _sorter);
    }

    #endregion
}
