using KNote.ClientWin.Core;
using KNote.Service.Core;

namespace KNote.ClientWin.Views;

public partial class SplashForm : Form
{
    #region Private fields 

    Store _appContext;

    #endregion

    #region Constructor

    public SplashForm(Store appContext)
    {
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        appContext.AddedServiceRef += AppContext_AddedServiceRef;
        _appContext = appContext;
    }

    #endregion

    #region Form events handlers

    private void AppContext_AddedServiceRef(object sender, ComponentEventArgs<ServiceRef> e)
    {
        labelMessage.Text = "Loading " + e.Entity.Alias + "...";
        labelMessage.Refresh();
        Application.DoEvents();
    }

    private void SplashForm_Load(object sender, EventArgs e)
    {
        labelVersion.Text = $"Version: {_appContext.AppVersion}";
    }

    #endregion 
}
