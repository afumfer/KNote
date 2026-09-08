using KNote.ClientWin.Core;
using KNote.Model;
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
        InitializeComponent();
        this.labelANotas.Text = KntConst.AppName;

        appContext.Events.Subscribe<ServiceRefAdded>(AppContext_AddedServiceRef);
        _appContext = appContext;

        // Program.cs's own Shown handler (subscribed after this constructor runs, so it fires after
        // this one) immediately starts LoadAppStore()'s work - real, synchronous-at-first I/O - on
        // this same UI thread. Without forcing a paint here first, the window frame appears (its
        // background) but WM_PAINT for the icon/labels is still only queued, not yet processed, and
        // that synchronous work blocks the message loop from getting to it until LoadAppStore's first
        // real await - showing an emptied-out splash for a moment. Refresh() forces that paint now.
        Shown += (s, e) => Refresh();
    }

    #endregion

    #region Form events handlers

    private void AppContext_AddedServiceRef(ServiceRefAdded e)
    {
        labelMessage.Text = "Loading " + e.ServiceRef.Alias + "...";
        labelMessage.Refresh();
        Application.DoEvents();
    }

    private void SplashForm_Load(object sender, EventArgs e)
    {
        labelVersion.Text = $"Version: {_appContext.AppVersion}";
    }

    #endregion 
}
