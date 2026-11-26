using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.ClientWin.Utils;

namespace KNote.ClientWin.Views;

public partial class RepositoryEditorForm : KntEditorForm, IViewEditor<RepositoryRef>
{
    #region Fields

    private readonly RepositoryEditorCtrl _ctrl;

    #endregion

    #region Constructor 

    public RepositoryEditorForm(RepositoryEditorCtrl ctrl)
    {
        InitializeComponent();

        buttonSelectDirectory.AlignToTextBox(textSqLiteDirectory);
        buttonSelectFile.AlignToTextBox(textSqLiteDataBase);
        buttonSelectDirectoryResources.AlignToTextBox(textResourcesContainerRoot);

        _ctrl = ctrl;

        panelSqLite.Resize += panelSqLite_Resize;

        tabPageNoteTypes.Controls.Add(_ctrl.NoteTypesManageCtrl.View.PanelView());
        tabPageTraceNoteTypes.Controls.Add(_ctrl.TraceNoteTypesManageCtrl.View.PanelView());
        tabPageAttributes.Controls.Add(_ctrl.KAttributesManageCtrl.View.PanelView());
        tabPageUsers.Controls.Add(_ctrl.UsersManageCtrl.View.PanelView());

        CreateHistoryTab();
    }

    #endregion

    #region History tab (H25)

    // The retention of the history of the notes: built here and not in the Designer, the last tab. Saved with «Accept».
    private TabPage tabPageHistory;
    private Label labelHistorySince;
    private NumericUpDown numericKeepAllMonths;
    private NumericUpDown numericDailyUntilMonths;
    private NumericUpDown numericMaxCopiesPerNote;
    private NumericUpDown numericMaxContentSizeMB;
    private NumericUpDown numericDeletedNotesDays;

    private void CreateHistoryTab()
    {
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoSize = true, Padding = new Padding(10) };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        labelHistorySince = new Label { AutoSize = true, ForeColor = SystemColors.GrayText, Margin = new Padding(3, 3, 3, 10), MaximumSize = new Size(600, 0) };
        layout.Controls.Add(labelHistorySince, 0, 0);
        layout.SetColumnSpan(labelHistorySince, 2);

        NumericUpDown AddValue(string label, int max, int min, string help)
        {
            var row = layout.RowCount++;
            layout.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 7, 12, 3) }, 0, row);
            var numeric = new NumericUpDown { Minimum = min, Maximum = max, Width = 90, Anchor = AnchorStyles.Left };
            var cell = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, Margin = new Padding(0), WrapContents = false };
            cell.Controls.Add(numeric);
            if (help != null)
                cell.Controls.Add(new Label { Text = help, AutoSize = true, ForeColor = SystemColors.GrayText, Margin = new Padding(8, 7, 3, 3) });
            layout.Controls.Add(cell, 1, row);
            return numeric;
        }

        layout.RowCount = 1;
        numericKeepAllMonths = AddValue("Months with all the versions:", HistoryRetentionDto.MaxMonths, 0, null);
        numericDailyUntilMonths = AddValue("Months with one version a day:", HistoryRetentionDto.MaxMonths, 0, "Then, one a month.");
        numericMaxCopiesPerNote = AddValue("Versions per note:", HistoryRetentionDto.MaxCopiesLimit, 1, "The oldest ones go first.");
        numericMaxContentSizeMB = AddValue("Largest note copied (MB):", HistoryRetentionDto.MaxContentSizeLimitMB, 0, "A bigger note is noted without its copy.");
        numericDeletedNotesDays = AddValue("Days in the deleted notes:", HistoryRetentionDto.MaxDeletedNotesDays, 0, "Then it can no longer be recovered.");

        tabPageHistory = new TabPage("History") { Padding = tabPageAttributes.Padding, UseVisualStyleBackColor = true };
        tabPageHistory.Controls.Add(layout);
        tabControlMain.TabPages.Add(tabPageHistory);
    }

    private void RetentionToControls()
    {
        var retention = _ctrl.Retention;
        tabPageHistory.Enabled = _ctrl.AdministrationAvailable && retention != null;
        toolTipAdminTabs.SetToolTip(tabPageHistory, _ctrl.AdministrationAvailable ? "" : "Available once the repository is linked.");
        labelHistorySince.Text = (_ctrl.HistorySince is DateTime since ? $"KNote keeps the history of the notes since {since:d}. " : "")
            + "How long the versions of the notes are kept. Reducing a period empties versions: it is said before saving.";
        if (retention == null)
            return;
        numericKeepAllMonths.Value = retention.KeepAllMonths;
        numericDailyUntilMonths.Value = retention.DailyUntilMonths;
        numericMaxCopiesPerNote.Value = Math.Max(1, retention.MaxCopiesPerNote);
        numericMaxContentSizeMB.Value = retention.MaxContentSizeMB;
        numericDeletedNotesDays.Value = retention.DeletedNotesDays;
    }

    private void ControlsToRetention()
    {
        var retention = _ctrl.Retention;
        if (retention == null)
            return;
        retention.KeepAllMonths = (int)numericKeepAllMonths.Value;
        retention.DailyUntilMonths = (int)numericDailyUntilMonths.Value;
        retention.MaxCopiesPerNote = (int)numericMaxCopiesPerNote.Value;
        retention.MaxContentSizeMB = (int)numericMaxContentSizeMB.Value;
        retention.DeletedNotesDays = (int)numericDeletedNotesDays.Value;
    }

    #endregion 

    #region Form events handler

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
        // The last row is Name, which is not shown when linking a repository (see ModelToControls; asked
        // through the mode because Visible is still false for every control until the form is shown). The
        // space left below it is the same as between the other rows.
        var lastRow = _ctrl.EditorMode == EnumRepositoryEditorMode.AddLink ? textResourcesContainerUrl : textName;
        int contentBottom = lastRow.Bottom + (panelSqLite.Top - groupRepositoryType.Bottom);
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

    protected override void ModelToControls()
    {
        switch (_ctrl.EditorMode)
        {
            case EnumRepositoryEditorMode.AddLink:
                Text = "Add link to existing repository";
                break;
            case EnumRepositoryEditorMode.Create:
                Text = "Create new repository";
                break;
            case EnumRepositoryEditorMode.Management:
                Text = "Edit repository properties";
                groupRepositoryType.Enabled = false;
                panelSqLite.Enabled = false;
                panelMSSqlServer.Enabled = false;
                break;
        }

        // Users/Note types/Trace note types/Attributes administration (KNote Atlas has its own window, Tools › KNote Atlas) only makes sense for an
        // already-linked repository (Management mode).
        tabPageUsers.Enabled = _ctrl.AdministrationAvailable;
        tabPageNoteTypes.Enabled = _ctrl.AdministrationAvailable;
        tabPageTraceNoteTypes.Enabled = _ctrl.AdministrationAvailable;
        tabPageAttributes.Enabled = _ctrl.AdministrationAvailable;

        var adminTabsHint = _ctrl.AdministrationAvailable ? "" : "Available once the repository is linked.";

        // The tab's content gets disabled along with the tab (see above), so a ToolTip on the
        // TabPage itself is what explains why - it still works when hovering the tab header, since
        // that belongs to the TabControl (which stays enabled), not to the disabled page content.
        toolTipAdminTabs.SetToolTip(tabPageUsers, adminTabsHint);
        toolTipAdminTabs.SetToolTip(tabPageNoteTypes, adminTabsHint);
        toolTipAdminTabs.SetToolTip(tabPageTraceNoteTypes, adminTabsHint);
        toolTipAdminTabs.SetToolTip(tabPageAttributes, adminTabsHint);

        // A linked repository is named in its own database.
        labelName.Visible = _ctrl.EditorMode != EnumRepositoryEditorMode.AddLink;
        textName.Visible = _ctrl.EditorMode != EnumRepositoryEditorMode.AddLink;
        textName.Text = _ctrl.Model.Name;
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

        RetentionToControls();
    }

    protected override void ControlsToModel()
    {
        if (_ctrl.EditorMode != EnumRepositoryEditorMode.AddLink)
            _ctrl.Model.Name = textName.Text?.Trim();
        _ctrl.Model.ResourcesContainer = textResourcesContainer.Text;
        _ctrl.Model.ResourceContentInDB = checkResourceContentInDB.Checked;
        _ctrl.Model.ResourcesContainerRootPath = textResourcesContainerRoot.Text;
        _ctrl.Model.ResourcesContainerRootUrl = textResourcesContainerUrl.Text;
        ControlsToRetention();
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
