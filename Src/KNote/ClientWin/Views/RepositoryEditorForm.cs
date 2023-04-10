using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Views;

public partial class RepositoryEditorForm : Form, IViewEditor<RepositoryRef>
{
    #region Fields

    private readonly RepositoryEditorComponent _com;
    private bool _viewFinalized = false;
    private bool _formIsDisty = false;

    #endregion

    #region Constructor 

    public RepositoryEditorForm(RepositoryEditorComponent com)
    {
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        _com = com;
    }

    #endregion 

    #region IEditorView implementation 

    public void ShowView()
    {
        this.Show();
    }

    public Result<EComponentResult> ShowModalView()
    {
        return _com.DialogResultToComponentResult(this.ShowDialog());
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

    public DialogResult ShowInfo(string info, string caption = "KaNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Asterisk)
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
        var res = await _com.SaveModel();
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
        this.Height = 500;            
        panelSqLite.BorderStyle = BorderStyle.None;
        panelMSSqlServer.BorderStyle = BorderStyle.None;
        panelMSSqlServer.Top = panelSqLite.Top;
        panelMSSqlServer.Left = panelSqLite.Left;
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
        switch (_com.EditorMode)
        {
            case EnumRepositoryEditorMode.AddLink:
                Text = "Add link to existing repository";
                buttonSelectFile.Visible = true;
                textSqLiteDataBase.Width = 464;
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

        textAliasName.Text = _com.Model.Alias;
        textResourcesContainer.Text = _com.Model.ResourcesContainer;
        checkResourceContentInDB.Checked = _com.Model.ResourceContentInDB;
        textResourcesContainerRoot.Text = _com.Model.ResourcesContainerCacheRootPath;
        textResourcesContainerUrl.Text = _com.Model.ResourcesContainerCacheRootUrl;

        if (!string.IsNullOrEmpty(_com.Model.ConnectionString))
        {                
            var connecionValues = _com.Model.GetConnectionProperties();
            if (_com.Model.Provider == "Microsoft.Data.Sqlite")
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
        _com.Model.Alias = textAliasName.Text;
        _com.Model.ResourcesContainer = textResourcesContainer.Text;
        _com.Model.ResourceContentInDB = checkResourceContentInDB.Checked;
        _com.Model.ResourcesContainerCacheRootPath = textResourcesContainerRoot.Text;
        _com.Model.ResourcesContainerCacheRootUrl = textResourcesContainerUrl.Text;
        if (radioSqLite.Checked)
        {
            _com.Model.Provider = "Microsoft.Data.Sqlite";
            _com.Model.ConnectionString = $"Data Source={Path.Combine(textSqLiteDirectory.Text, textSqLiteDataBase.Text)}";
        }
        else
        {
            _com.Model.Provider = "Microsoft.Data.SqlClient";
            _com.Model.ConnectionString = $"Data Source={textSQLServer.Text}; Initial Catalog={textSQLDataBase.Text}; Trusted_Connection=True; Connection Timeout=60; MultipleActiveResultSets=true;Encrypt=false";
        }

        // TODO: hack, EntityFramework is default orm when repository is created. (Dapper version no suport create repository). 
        if (_com.EditorMode == EnumRepositoryEditorMode.AddLink || _com.EditorMode == EnumRepositoryEditorMode.Create)
            _com.Model.Orm = "EntityFramework";
    }

    private bool OnCancelEdition()
    {
        if (_formIsDisty)
        {
            if (MessageBox.Show("You have modified this entity, are you sure you want to exit without recording?", "KaNote", MessageBoxButtons.YesNo) == DialogResult.No)
                return false;
        }

        this.DialogResult = DialogResult.Cancel;
        _com.CancelEdition();
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
