﻿using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using System.ComponentModel;

namespace KNote.ClientWin.Views;

public partial class HeavyProcessForm : Form, IViewHeavyProcess
{
    #region Private fields 

    private readonly HeavyProcessCtrl _ctrl;
    private bool _viewFinalized = false;

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

    private void HeavyProcessForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
            _ctrl.Finalize();
    }

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

    public void ShowView()
    {
        TopMost = true;
        Show();
    }

    public Result<EControllerResult> ShowModalView()
    {
        return _ctrl.DialogResultToControllerResult(ShowDialog());
    }

    public void RefreshView()
    {
        Refresh();
    }

    public void OnClosingView()
    {
        _viewFinalized = true;
        this.Close();
    }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Asterisk)
    {
        return MessageBox.Show(info, caption, buttons, icon);
    }

    public void HideView()
    {
        Hide();
    }

    #endregion
}
