using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using System.ComponentModel;

namespace KNote.ClientWin.Views;

public partial class HeavyProcessForm : KntForm, IViewHeavyProcess
{
    #region Private fields 

    private readonly HeavyProcessCtrl _ctrl;

    #endregion

    #region Constructor

    public HeavyProcessForm(HeavyProcessCtrl ctrl)  // Func<Task> process
    {
        InitializeComponent();

        _ctrl = ctrl;
    }

    #endregion

    #region Events handlers

    private void buttonCancel_Click(object sender, EventArgs e)
    {
        CancellationToken.Cancel();
    }

    protected override void OnUserClosing(FormClosingEventArgs e)
        => _ctrl.Finalize();

    #endregion 

    #region IView interface

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public CancellationTokenSource CancellationToken { get; set; }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IProgress<KeyValuePair<int, string>> ReportProgress { get; set; }

    public void UpdateProgress(int progress)
    {
        progressProcess.Value = progress;
    }

    public void UpdateProcessName(string process)
    {
        labelProcess.Text = process;
        labelProcess.Refresh();
    }

    public void UpdateProcessInfo(string info)
    {
        labelInfo.Text = info;        
    }

    public override void ShowView()
    {
        TopMost = true;
        Show();
    }

    public void RefreshView()
    {
        Refresh();
    }

    public void HideView()
    {
        Hide();
    }

    #endregion
}
