﻿using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Views;

public partial class KNoteAboutForm : Form, IViewBase
{
    #region Private fields 

    private readonly KNoteManagmentComponent _com;

    #endregion

    #region Constructor

    public KNoteAboutForm(KNoteManagmentComponent com)
    {
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();
        Text = KntConst.AppName;

        _com = com;
    }

    #endregion 

    #region IViewBase implementation

    public void ShowView()
    {
        this.Show();
    }

    public Result<EComponentResult> ShowModalView()
    {
        return _com.DialogResultToComponentResult(this.ShowDialog());
    }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Asterisk)
    {
        return MessageBox.Show(info, caption, buttons, icon);
    }

    public void OnClosingView()
    {

    }

    #endregion 

    #region Form events handlers 

    private void KNoteAboutForm_Load(object sender, EventArgs e)
    {
        labelRepository.Text = KntConst.GithubProject;
        labelVersion.Text = $"Version: {_com.Store.AppVersion}";
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
