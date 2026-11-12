using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model.Dto;
using KNote.Service.Core;
using KntIcons;

namespace KNote.ClientWin.Views;

public partial class NotesSearchParamForm : KntForm, IViewEmbeddable
{
    #region Private fields

    private readonly NotesSearchParamCtrl _ctrl;

    #endregion

    #region Constructor

    public NotesSearchParamForm(NotesSearchParamCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;

        buttonSearch.SetKntIcon(KntIcon.Search);
        comboRepositories.SelectedIndexChanged += async (s, e) => await RefreshSearchModeAsync();
    }

    #endregion

    #region IViewEmbeddable implementation

    public Control PanelView()
    {
        return panelForm;
    }

    public override void RefreshView()
    {
        PersonalizeControls();
    }

    public override void ConfigureEmbededMode()
    {
        TopLevel = false;
        Dock = DockStyle.Fill;
        FormBorderStyle = FormBorderStyle.None;
        panelBottom.Visible = false;
    }

    public override void ConfigureWindowMode()
    {
        TopLevel = true;
        Dock = DockStyle.None;
        FormBorderStyle = FormBorderStyle.Sizable;
        panelBottom.Visible = true;
        StartPosition = FormStartPosition.CenterScreen;
    }

    #endregion

    #region Form events handlers

    protected override void OnUserClosing(FormClosingEventArgs e)
        => _ctrl.Finalize();

    private void buttonClean_Click(object sender, EventArgs e)
    {
        CleanView();
        buttonSearch_Click(this, e);
    }

    private void buttonSearch_Click(object sender, EventArgs e)
    {
        var search = new SelectedNotesInServiceRef
        {
            ServiceRef = (ServiceRef)comboRepositories.SelectedItem,
            NotesSearch = new NotesSearchDto
            {
                TextSearch = textTextSearch.Text,
                SearchInDescription = checkSearchInDescription.Checked,
                SearchInNoteTasks = checkSearchInNoteTasks.Checked,
                SearchInResources = checkSearchInResources.Checked,
                Mode = radioSearchMeaning.Visible && radioSearchMeaning.Checked ? EnumNotesSearchMode.Meaning : EnumNotesSearchMode.Words,
                // KNote Atlas 4.6b: by meaning, the notes, or the photos or the documents.
                ResourceKind = radioSearchMeaning.Visible && radioSearchMeaning.Checked && comboResourceKind.SelectedItem is EnumResourceKind kind
                    ? kind : EnumResourceKind.All
            }
        };

        _ctrl.NotifySearchApplied(search);
    }

    // The options of the search by words do not apply to the search by meaning (it reads the whole note).
    private void radioSearchMode_CheckedChanged(object sender, EventArgs e)
    {
        checkSearchInDescription.Enabled = radioSearchWords.Checked;
        checkSearchInNoteTasks.Enabled = radioSearchWords.Checked;
        checkSearchInResources.Enabled = radioSearchWords.Checked;
        comboResourceKind.Enabled = radioSearchMeaning.Checked;
    }

    private void buttonAccept_Click(object sender, EventArgs e)
    {
        _ctrl.Finalize();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
        _ctrl.Finalize();
    }

    #endregion

    #region Private methods

    private void PersonalizeControls()
    {
        comboRepositories.Items.Clear();
        foreach (var serviceRef in _ctrl.Store.GetAllServiceRef())
            comboRepositories.Items.Add(serviceRef);
        comboRepositories.ValueMember = "IdServiceRef";
        comboRepositories.DisplayMember = "Name";
        comboRepositories.SelectedIndex = comboRepositories.Items.Count > 0 ? 0 : -1;

        comboResourceKind.Items.Clear();
        foreach (var kind in Enum.GetValues<EnumResourceKind>())
            comboResourceKind.Items.Add(kind);
        comboResourceKind.SelectedIndex = 0;
        comboResourceKind.Enabled = radioSearchMeaning.Checked;
    }

    // «Search by» (words or meaning) only where the repository chosen can search by meaning.
    private async Task RefreshSearchModeAsync()
    {
        var serviceRef = comboRepositories.SelectedItem as ServiceRef;
        var canSearchByMeaning = await _ctrl.Store.CanSearchByMeaningAsync(serviceRef);
        if (IsDisposed || comboRepositories.SelectedItem != serviceRef)
            return; // another repository chosen meanwhile

        labelSearchMode.Visible = canSearchByMeaning;
        radioSearchWords.Visible = canSearchByMeaning;
        radioSearchMeaning.Visible = canSearchByMeaning;
        labelResourceKind.Visible = comboResourceKind.Visible = canSearchByMeaning;
        if (!canSearchByMeaning)
            radioSearchWords.Checked = true;
    }

    private void CleanView()
    {
        textTextSearch.Text = "";
        checkSearchInDescription.Checked = true;
        checkSearchInNoteTasks.Checked = false;
        checkSearchInResources.Checked = false;
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

    #endregion
}
