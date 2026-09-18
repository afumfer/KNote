using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Views;

public partial class KNoteAboutForm : KntForm, IViewBase
{
    #region Private fields 

    private readonly KNoteManagmentCtrl _ctrl;

    #endregion

    #region Constructor

    public KNoteAboutForm(KNoteManagmentCtrl ctrl)
    {
        InitializeComponent();
        Text = KntConst.AppName;
        labelAppName.Text = KntConst.AppName;

        _ctrl = ctrl;
    }

    #endregion 

    #region IViewBase implementation

    public override void OnClosingView()
    {

    }

    #endregion 

    #region Form events handlers 

    private void KNoteAboutForm_Load(object sender, EventArgs e)
    {
        labelRepository.Text = KntConst.GithubProject;
        labelVersion.Text = $"Version: {_ctrl.Store.AppVersion}";
        labelInfo.Text = KntConst.License;
    }

    private void buttonAccept_Click(object sender, EventArgs e)
    {
        this.DialogResult = DialogResult.OK;
    }

    public void RefreshView()
    {
        throw new NotImplementedException();
    }

    #endregion
}
