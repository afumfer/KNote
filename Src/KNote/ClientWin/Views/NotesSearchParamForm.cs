using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Views;

public partial class NotesSearchParamForm : Form, IViewEmbeddable
{
    #region Private fields

    private readonly NotesSearchParamCtrl _ctrl;
    private bool _viewFinalized = false;

    #endregion

    #region Constructor

    public NotesSearchParamForm(NotesSearchParamCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;
    }

    #endregion

    #region IViewEmbeddable implementation

    public Control PanelView()
    {
        return panelForm;
    }

    public void ShowView()
    {
        this.Show();
    }

    public Result<EControllerResult> ShowModalView()
    {
        var res = _ctrl.DialogResultToControllerResult(this.ShowDialog());
        return res;
    }

    public void RefreshView()
    {
        PersonalizeControls();
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

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Asterisk)
    {
        return MessageBox.Show(info, caption, buttons, icon);
    }

    #endregion

    #region Form events handlers

    private void NotesSearchParamForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
            _ctrl.Finalize();
    }

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
            NotesSearch = new NotesSearchDto { TextSearch = textTextSearch.Text, SearchInDescription = checkSearchInDescription.Checked }
        };

        _ctrl.NotifySearchApplied(search);
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
        comboRepositories.DisplayMember = "Alias";
        comboRepositories.SelectedIndex = comboRepositories.Items.Count > 0 ? 0 : -1;
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

    #endregion
}
