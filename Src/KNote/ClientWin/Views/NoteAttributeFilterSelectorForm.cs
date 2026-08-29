using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

public partial class NoteAttributeFilterSelectorForm : Form
{
    #region Properties

    public AtrFilterDto Result { get; private set; }

    #endregion

    #region Constructor

    public NoteAttributeFilterSelectorForm(List<KAttributeInfoDto> kAttributes, AtrFilterDto existing = null)
    {
        InitializeComponent();

        comboAttribute.DisplayMember = "Name";
        foreach (var kAttribute in kAttributes)
            comboAttribute.Items.Add(kAttribute);

        if (existing != null)
        {
            comboAttribute.SelectedItem = kAttributes.FirstOrDefault(a => a.KAttributeId == existing.AtrId);
            textValue.Text = existing.Value;
        }
        else if (comboAttribute.Items.Count > 0)
        {
            comboAttribute.SelectedIndex = 0;
        }
    }

    #endregion

    #region Form events handlers

    private void buttonAccept_Click(object sender, EventArgs e)
    {
        var selected = comboAttribute.SelectedItem as KAttributeInfoDto;
        if (selected == null || string.IsNullOrWhiteSpace(textValue.Text))
        {
            MessageBox.Show(this, "Select an attribute and enter a value.", "KNote", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Result = new AtrFilterDto
        {
            AtrId = selected.KAttributeId,
            AtrName = selected.Name,
            Value = textValue.Text
        };

        DialogResult = DialogResult.OK;
        Close();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    #endregion
}
