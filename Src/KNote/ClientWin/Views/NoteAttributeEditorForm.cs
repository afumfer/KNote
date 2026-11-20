using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.ClientWin.Utils;

namespace KNote.ClientWin.Views;

public partial class NoteAttributeEditorForm : KntEditorForm, IViewEditor<NoteKAttributeDto>
{
    #region Private fields

    private readonly NoteAttributeEditorCtrl _ctrl;

    #endregion

    #region Constructor

    public NoteAttributeEditorForm(NoteAttributeEditorCtrl ctrl)
    {
        InitializeComponent();

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

    private void buttonSelDate_Click(object sender, EventArgs e)
    {
        textValue.Text = SelDate(textValue.Text);
    }

    #endregion

    #region Private methods

    protected override void ModelToControls()
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
        textValue.Multiline = false;
        textValue.Text = _ctrl.Model.Value?.ToString();
        textValue.Visible = true;
        SetValueControl(textValue, widthShort ? ValueLayout.Short : ValueLayout.Wide);
    }

    private void ModelToControlTextArea()
    {
        textValue.Multiline = true;
        textValue.Text = _ctrl.Model.Value;
        textValue.Visible = true;
        SetValueControl(textValue, ValueLayout.Tall);
    }

    private void ModelToControlCheck()
    {
        labelAttribute.Text = "Check option:";
        // An attribute that was never given a value has none (or one written elsewhere): unchecked, never an error,
        // which left the editor without opening.
        checkValue.Checked = bool.TryParse(_ctrl.Model.Value, out var value) && value;
        checkValue.Text = _ctrl.Model.Name;
        checkValue.Visible = true;
        SetValueControl(checkValue, ValueLayout.Wide);
    }

    private void ModelToControlDateTime()
    {
        ModelToControlText(true);
        buttonSelDate.Visible = true;
        LayoutValue();
        buttonSelDate.AlignToTextBox(textValue);
    }

    private async void ModelToControlCombo()
    {
        comboValue.Visible = true;
        SetValueControl(comboValue, ValueLayout.Wide);
        var res = await _ctrl.Service.KAttributes.GetKAttributeTabulatedValuesAsync(_ctrl.Model.KAttributeId);
        if (res.IsValid)
        {
            comboValue.Items.Add("");
            foreach (var atr in res.Entity)
                comboValue.Items.Add(atr.Value);
        }
        comboValue.SelectedItem = _ctrl.Model.Value;
    }

    private async void ModelToControlListView()
    {
        PersonalizeListView(listViewValue);
        listViewValue.Columns.Add("Name", LogicalToDeviceUnits(450), HorizontalAlignment.Left);
        listViewValue.Visible = true;
        SetValueControl(listViewValue, ValueLayout.Tall);
        var res = await _ctrl.Service.KAttributes.GetKAttributeTabulatedValuesAsync(_ctrl.Model.KAttributeId);
        if (res.IsValid)
        {
            // The value is the checked ones separated by commas (see ControlsToModel): compared whole, so "Bet"
            // is not checked by "Beta".
            var values = (_ctrl.Model.Value ?? "").Split(',').Select(v => v.Trim()).ToHashSet();
            foreach (var atr in res.Entity)
                listViewValue.Items.Add(new ListViewItem(atr.Value) { Checked = values.Contains(atr.Value) });
        }
        FitListColumn();
    }

    private enum ValueLayout { Wide, Short, Tall }

    private Control _valueControl;
    private ValueLayout _valueLayout;

    private void SetValueControl(Control control, ValueLayout layout)
    {
        _valueControl = control;
        _valueLayout = layout;
        LayoutValue();
    }

    protected override void OnLoad(EventArgs e)
    {
        // Placed again once the form has its final scale; before base.OnLoad, which centers the dialog with its size.
        LayoutValue();
        FitHeightToValue();
        base.OnLoad(e);
    }

    // The value goes under its label with the Designer's margin on both sides, measured on the panel as the form was
    // scaled: by its font, which is not exactly the DPI factor of LogicalToDeviceUnits (about 1.86 against 2 at
    // 200 %), so fixed logical widths made the box pass the right edge and hid the combo's arrow.
    private void LayoutValue()
    {
        if (_valueControl == null)
            return;

        // textValue is never moved from its Designer left (it goes back to it when it is the value).
        var left = textValue.Left;
        labelAttribute.AlignTextTo(left);
        labelDescription.AlignTextTo(left);
        var top = labelAttribute.Bottom + LogicalToDeviceUnits(LabelToValueGap);
        var width = panelForm.ClientSize.Width - 2 * left;
        var control = _valueControl;

        switch (_valueLayout)
        {
            case ValueLayout.Short:
                control.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                control.Bounds = new Rectangle(left, top, Math.Min(width, LogicalToDeviceUnits(200)), control.Height);
                break;
            case ValueLayout.Tall:
                control.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                var bottom = labelDescription.Top - labelDescription.Height;
                control.Bounds = new Rectangle(left, top, width, Math.Max(bottom - top, control.MinimumSize.Height));
                break;
            default:
                if (control is CheckBox check)
                {
                    // Not auto-sized: a long name is cut at the margin instead of passing it.
                    check.AutoSize = false;
                    check.Height = check.PreferredSize.Height;
                }
                control.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                control.Bounds = new Rectangle(left, top, width, control.Height);
                break;
        }

        if (buttonSelDate.Visible)
        {
            buttonSelDate.Width = LogicalToDeviceUnits(ControlLayout.SideButtonWidth);
            buttonSelDate.Left = textValue.Right + LogicalToDeviceUnits(ControlLayout.SideButtonGap);
        }

        FitListColumn();
    }

    // Space between the label and its value, in logical pixels.
    private const int LabelToValueGap = 6;

    // The Designer's size is the one of a multi-line value (a text area or the list of tags): a value of one line
    // leaves the description under it, as far from it as in the text area, and the dialog that much shorter.
    private void FitHeightToValue()
    {
        if (_valueControl == null || _valueLayout == ValueLayout.Tall)
            return;

        var descriptionTop = _valueControl.Bottom + labelDescription.Height;
        var extra = labelDescription.Top - descriptionTop;
        if (extra <= 0)
            return;

        Height -= extra;
        MinimumSize = new Size(0, Height);
    }

    // The only column fills the list (without the scroll bar, when it has one), with no horizontal scroll.
    private void FitListColumn()
    {
        if (listViewValue.Visible && listViewValue.Columns.Count > 0)
            listViewValue.Columns[0].Width = listViewValue.ClientSize.Width;
    }

    private void PersonalizeListView(ListView listView)
    {
        ListViewStyle.ApplyStandard(listView, checkBoxes: true);
        listView.MultiSelect = true;
        listView.HeaderStyle = ColumnHeaderStyle.None;
    }

    protected override void ControlsToModel()
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
                _ctrl.Model.Value = comboValue.SelectedItem?.ToString() ?? "";
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
