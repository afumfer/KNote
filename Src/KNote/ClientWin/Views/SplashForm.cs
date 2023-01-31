using KNote.ClientWin.Core;
using KNote.Service.Core;

namespace KNote.ClientWin.Views;

public partial class SplashForm : Form
{
    Store _appContext;

    public SplashForm(Store appContext)
    {
        InitializeComponent();

        appContext.AddedServiceRef += AppContext_AddedServiceRef;
        _appContext = appContext;
    }

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
}
