﻿using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Views;

public partial class NotifyForm : Form, IViewBase
{
    #region Private fields

    private readonly KNoteManagmentComponent _com;

    #endregion

    #region Constructor

    public NotifyForm(KNoteManagmentComponent com)
    {
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();
        notifyKNote.Text = KntConst.AppName;
        menuShowKNoteManagment.Text = $"Show {KntConst.AppName} managment ...";

        _com = com;
    }

    #endregion 

    #region IViewBase implementation

    public void ShowView()
    {
        this.Show();
    }

    Result<EComponentResult> IViewBase.ShowModalView()
    {
        return _com.DialogResultToComponentResult(this.ShowDialog());
    }

    public DialogResult ShowInfo(string info, string caption = "KeyNotex", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        return MessageBox.Show(info, caption, buttons, icon);
    }

    public void OnClosingView()
    {
    }

    #endregion

    #region Menu events handlers 

    private async void notifyKNote_DoubleClick(object sender, EventArgs e)
    {
        await _com.AddDefaultNotePostIt();
    }

    private async void menuNewNote_Click(object sender, EventArgs e)
    {
        await _com.AddDefaultNotePostIt();
    }

    private void menuShowKNoteManagment_Click(object sender, EventArgs e)
    {
        _com.ShowKNoteManagment();
    }

    private void menuPostItsVisibles_Click(object sender, EventArgs e)
    {
        if (menuPostItsVisibles.Checked)
            _com.Store.ActivatePostIts();
        else
            _com.Store.HidePostIts();
    }

    private void menuKNoteOptions_Click(object sender, EventArgs e)
    {
        _com.Options();
    }

    private void menuHelp_Click(object sender, EventArgs e)
    {
        _com.Help();
    }

    private void menuAbout_Click(object sender, EventArgs e)
    {
        _com.About();
    }

    private void menuExit_Click(object sender, EventArgs e)
    {
        _com?.FinalizeApp();
    }

    public void RefreshView()
    {
        this.Refresh();        
    }

    #endregion
}
