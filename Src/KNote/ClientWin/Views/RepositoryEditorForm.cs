using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Views;

public partial class RepositoryEditorForm : Form, IViewEditor<RepositoryRef>
{
    #region Fields

    private readonly RepositoryEditorCtrl _ctrl;
    private bool _viewFinalized = false;
    private bool _formIsDisty = false;

    #endregion

    #region Constructor 

    public RepositoryEditorForm(RepositoryEditorCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;

        panelSqLite.Resize += panelSqLite_Resize;

        tabPageNoteTypes.Controls.Add(_ctrl.NoteTypesManageCtrl.View.PanelView());
        tabPageTraceNoteTypes.Controls.Add(_ctrl.TraceNoteTypesManageCtrl.View.PanelView());
        tabPageAttributes.Controls.Add(_ctrl.KAttributesManageCtrl.View.PanelView());
        tabPageUsers.Controls.Add(_ctrl.UsersManageCtrl.View.PanelView());
    }

    #endregion 

    #region IEditorView implementation 

    public void ShowView()
    {
        this.Show();
    }

    public Result<EControllerResult> ShowModalView()
    {
        return _ctrl.DialogResultToControllerResult(this.ShowDialog());
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
        //textAlias.Text = "";
    }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Asterisk)
    {
        return MessageBox.Show(info, caption, buttons, icon);
    }

    public void OnClosingView()
    {
        _viewFinalized = true;
        this.Close();
    }

    #endregion

    #region Form events handler

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
        OnCancelEdition();
    }

    private void RepositoryEditorForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
        {
            var confirmExit = OnCancelEdition();
            if (!confirmExit)
                e.Cancel = true;
        }
    }

    private void RepositoryEditorForm_KeyPress(object sender, KeyPressEventArgs e)
    {
        _formIsDisty = true;
    }

    private void RepositoryEditorForm_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
            _formIsDisty = true;
    }

    private void RepositoryEditorForm_Load(object sender, EventArgs e)
    {
        panelSqLite.BorderStyle = BorderStyle.None;
        panelMSSqlServer.BorderStyle = BorderStyle.None;
        panelMSSqlServer.Top = panelSqLite.Top;
        panelMSSqlServer.Left = panelSqLite.Left;

        // The form used to be forced to a hardcoded Height, which AutoScaleMode never
        // rescales, so it only fit at the DPI it was tuned for. Derive it instead from
        // the already auto-scaled geometry of the real content and the Accept/Cancel
        // button row, so it fits at any Windows scale factor. tabStripOverhead captures
        // however much vertical space the tab strip itself takes at the current DPI/font
        // (both it and panelForm were scaled by the same factor during InitializeComponent).
        int footerHeight = ClientSize.Height - tabControlMain.Height;
        int tabStripOverhead = tabControlMain.Height - panelForm.Height;
        int contentBottom = Math.Max(panelSqLite.Bottom, panelMSSqlServer.Bottom);
        int targetTabControlHeight = contentBottom + tabStripOverhead;

        // Deliberately NOT "tabControlMain.Height = targetTabControlHeight;" here: tabControlMain is
        // anchored Top+Bottom (not Dock=Top) precisely so it grows/shrinks with the window instead of
        // leaving a gap above the Accept/Cancel buttons when the user resizes taller. But WinForms
        // recomputes an anchored control's "distance from the bottom edge" against the CURRENT
        // ClientSize the instant its own bounds change - so manually setting Height first (while
        // ClientSize is still its old value) locks in the wrong distance, and the ClientSize
        // assignment right after then shrinks tabControlMain again to preserve *that* wrong distance.
        // Setting ClientSize alone, in one shot, lets the anchor resize tabControlMain itself using
        // the original (correct) bottom distance from Designer time.
        ClientSize = new Size(ClientSize.Width, targetTabControlHeight + footerHeight);

        // Floor the window at this just-computed size so shrinking can't push it back below the
        // point where the tab content and the button row would start overlapping instead.
        MinimumSize = new Size(Width, Height);

        // Also run once explicitly: if the runtime DPI happens to match the Designer's
        // baked scale, AutoScale is a no-op and panelSqLite never actually fires Resize.
        panelSqLite_Resize(this, EventArgs.Empty);
    }

    // buttonSelectFile must behave exactly like buttonSelectDirectory: always visible,
    // always at the same computed position. Both textboxes/buttons no longer use Anchor
    // (which proved unreliable here) and are instead positioned explicitly from
    // panelSqLite's own real width plus two fixed margins, driven by its Resize event so
    // it re-runs after every layout pass (initial show, DPI change, form resize).
    private const int SqLitePanelRightMargin = 17;
    private const int SqLiteButtonGap = 8;

    private void panelSqLite_Resize(object sender, EventArgs e)
    {
        int buttonLeft = panelSqLite.Width - SqLitePanelRightMargin - buttonSelectDirectory.Width;

        buttonSelectDirectory.Left = buttonLeft;
        textSqLiteDirectory.Width = buttonLeft - SqLiteButtonGap - textSqLiteDirectory.Left;

        buttonSelectFile.Left = buttonLeft;
        textSqLiteDataBase.Width = buttonLeft - SqLiteButtonGap - textSqLiteDataBase.Left;
    }

    private void radioDataBase_CheckedChanged(object sender, EventArgs e)
    {
        RefreshRadioDatabase();
    }

    private void buttonSelectDirectory_Click(object sender, EventArgs e)
    {
        using (var fbd = new FolderBrowserDialog())
        {
            DialogResult result = fbd.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                textSqLiteDirectory.Text = fbd.SelectedPath;
            }
        }
    }

    private void buttonSelectFile_Click(object sender, EventArgs e)
    {
        using (var ofd = new OpenFileDialog())
        {
            if (Directory.Exists(textSqLiteDirectory.Text))
                ofd.InitialDirectory = textSqLiteDirectory.Text;
            ofd.DefaultExt = "db";
            ofd.Filter = "Sqlite database (*.db)|*.db";
            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(ofd.FileName))
            {
                textSqLiteDataBase.Text = ofd.FileName;
            }
        }
    }

    private void buttonSelectDirectoryResources_Click(object sender, EventArgs e)
    {
        using (var fbd = new FolderBrowserDialog())
        {
            DialogResult result = fbd.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                textResourcesContainerRoot.Text = fbd.SelectedPath;
            }
        }
    }

    #endregion 

    #region Private methods

    private void ModelToControls()
    {
        switch (_ctrl.EditorMode)
        {
            case EnumRepositoryEditorMode.AddLink:
                Text = "Add link to existing repository";
                break;
            case EnumRepositoryEditorMode.Create:
                Text = "Create new repository";
                break;
            case EnumRepositoryEditorMode.Managment:
                Text = "Edit repository properties";
                groupRepositoryType.Enabled = false;
                panelSqLite.Enabled = false;
                panelMSSqlServer.Enabled = false;
                break;
        }

        // Users/Note types/Trace note types/Attributes administration only makes sense for an
        // already-linked repository (Managment mode) and only for a repository user with the Admin role.
        tabPageUsers.Enabled = _ctrl.CurrentUserIsAdmin;
        tabPageNoteTypes.Enabled = _ctrl.CurrentUserIsAdmin;
        tabPageTraceNoteTypes.Enabled = _ctrl.CurrentUserIsAdmin;
        tabPageAttributes.Enabled = _ctrl.CurrentUserIsAdmin;

        var adminTabsHint = _ctrl.EditorMode != EnumRepositoryEditorMode.Managment
            ? "Available once the repository is linked."
            : (_ctrl.CurrentUserIsAdmin ? "" : "Requires the Admin role in this repository.");

        // The tab's content gets disabled along with the tab (see above), so a ToolTip on the
        // TabPage itself is what explains why - it still works when hovering the tab header, since
        // that belongs to the TabControl (which stays enabled), not to the disabled page content.
        toolTipAdminTabs.SetToolTip(tabPageUsers, adminTabsHint);
        toolTipAdminTabs.SetToolTip(tabPageNoteTypes, adminTabsHint);
        toolTipAdminTabs.SetToolTip(tabPageTraceNoteTypes, adminTabsHint);
        toolTipAdminTabs.SetToolTip(tabPageAttributes, adminTabsHint);

        textAliasName.Text = _ctrl.Model.Alias;
        textResourcesContainer.Text = _ctrl.Model.ResourcesContainer;
        checkResourceContentInDB.Checked = _ctrl.Model.ResourceContentInDB;
        textResourcesContainerRoot.Text = _ctrl.Model.ResourcesContainerRootPath;
        textResourcesContainerUrl.Text = _ctrl.Model.ResourcesContainerRootUrl;

        if (!string.IsNullOrEmpty(_ctrl.Model.ConnectionString))
        {                
            var connecionValues = _ctrl.Model.GetConnectionProperties();
            if (_ctrl.Model.Provider == "Microsoft.Data.Sqlite")
            {                    
                textSqLiteDirectory.Text = Path.GetDirectoryName(connecionValues["Data Source"]) ;
                textSqLiteDataBase.Text = Path.GetFileName(connecionValues["Data Source"]);
                radioSqLite.Checked = true;
            }
            else
            {                    
                textSQLServer.Text = connecionValues["Data Source"];
                textSQLDataBase.Text = connecionValues["Initial Catalog"];
                radioMSSqlServer.Checked = true;                
            }            
        }
        else 
            radioSqLite.Checked = true;

    }

    private void ControlsToModel()
    {
        _ctrl.Model.Alias = textAliasName.Text;
        _ctrl.Model.ResourcesContainer = textResourcesContainer.Text;
        _ctrl.Model.ResourceContentInDB = checkResourceContentInDB.Checked;
        _ctrl.Model.ResourcesContainerRootPath = textResourcesContainerRoot.Text;
        _ctrl.Model.ResourcesContainerRootUrl = textResourcesContainerUrl.Text;
        if (radioSqLite.Checked)
        {
            _ctrl.Model.Provider = "Microsoft.Data.Sqlite";
            _ctrl.Model.ConnectionString = $"Data Source={Path.Combine(textSqLiteDirectory.Text, textSqLiteDataBase.Text)}";
        }
        else
        {
            _ctrl.Model.Provider = "Microsoft.Data.SqlClient";
            _ctrl.Model.ConnectionString = $"Data Source={textSQLServer.Text}; Initial Catalog={textSQLDataBase.Text}; Trusted_Connection=True; Connection Timeout=60; MultipleActiveResultSets=true;Encrypt=false";
        }

        // TODO: hack, EntityFramework is default orm when repository is created. (Dapper version no suport create repository). 
        if (_ctrl.EditorMode == EnumRepositoryEditorMode.AddLink || _ctrl.EditorMode == EnumRepositoryEditorMode.Create)
            _ctrl.Model.Orm = "EntityFramework";
    }

    private bool OnCancelEdition()
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

    private void RefreshRadioDatabase()
    {
        if (radioSqLite.Checked == true)
        {
            panelSqLite.Visible = true;
            panelMSSqlServer.Visible = false;
        }
        else
        {
            panelSqLite.Visible = false;
            panelMSSqlServer.Visible = true;
        }
    }

    #endregion
    
}
