﻿using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.ClientWin.Views;

public partial class FiltersSelectorForm : Form, IViewSelector<SelectedNotesInServiceRef>
{
    #region Private fields

    private FiltersSelectorCtrl _ctrl;
    private bool _viewFinalized = false;
    //private bool _formIsDisty = false;

    #endregion

    #region Constructor

    public FiltersSelectorForm(FiltersSelectorCtrl ctrl)
    {
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        _ctrl = ctrl;            
    }

    #endregion 

    #region ISelectorView implementation

    public Control PanelView()
    {
        return panelForm;
    }

    public void ShowView()
    {
        this.Show();
    }

    public Result<EControllerResult> ShowModalView()
    {
        var res = _ctrl.DialogResultToControllerResult(this.ShowDialog());
        return res;
    }

    public void RefreshView()
    {
        PersonalizeControls();
        ModelToControls();
    }

    public void OnClosingView()
    {
        _viewFinalized = true;
        this.Close();
    }

    public void ConfigureEmbededMode()
    {
        TopLevel = false;
        Dock = DockStyle.Fill;
        FormBorderStyle = FormBorderStyle.None;
        panelBottom.Visible = false;
    }

    public void ConfigureWindowMode()
    {
        TopLevel = true;
        Dock = DockStyle.None;
        FormBorderStyle = FormBorderStyle.Sizable;
        panelBottom.Visible = true;
        StartPosition = FormStartPosition.CenterScreen;
    }


    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Asterisk)
    {
        return MessageBox.Show(info, caption, buttons, icon);
    }

    public void RefreshItem(SelectedNotesInServiceRef item)
    {
        throw new NotImplementedException();
    }

    public void DeleteItem(SelectedNotesInServiceRef item)
    {
        throw new NotImplementedException();
    }

    public void AddItem(SelectedNotesInServiceRef item)
    {
        throw new NotImplementedException();
    }

    public object SelectItem(SelectedNotesInServiceRef item)
    {
        throw new NotImplementedException();
    }

    public List<SelectedNotesInServiceRef> GetSelectedListItem()
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Form events handlers

    private void FilterParamForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
            _ctrl.Finalize();
    }

    private void buttonClean_Click(object sender, EventArgs e)
    {
        CleanView();
        buttonSearch_Click(this, e);
    }

    private void buttonSearch_Click(object sender, EventArgs e)
    {
        var filter = new SelectedNotesInServiceRef();
        filter.ServiceRef = (ServiceRef)comboRepositories.SelectedItem;

        //if(!checkSearchInDescription.Checked)
        //    filter.NotesFilter = new NotesFilterDto { TextSearch = textTextSearch.Text };
        //else
        //    filter.NotesFilter = new NotesFilterDto { TextSearch = $"*** {textTextSearch.Text}"  };

        filter.NotesSearch = new NotesSearchDto { TextSearch = textTextSearch.Text, SearchInDescription = checkSearchInDescription.Checked };

        _ctrl.SelectedEntity = filter;            
        _ctrl.NotifySelectedEntity();
    }

    private void buttonAccept_Click(object sender, EventArgs e)
    {
        _ctrl.Accept();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
        _ctrl.Cancel();
    }

    #endregion

    #region Private methods

    private void PersonalizeControls()
    {
        foreach (var serviceRef in _ctrl.Store.GetAllServiceRef())
            comboRepositories.Items.Add(serviceRef);
        comboRepositories.ValueMember = "IdServiceRef";
        comboRepositories.DisplayMember = "Alias";
        comboRepositories.SelectedIndex = 0;
    }

    private void ModelToControls()
    {
        // TODO: ...
        //if(_ctrl.Store.ActiveFolderWithServiceRef != null)
        //    comboRepositories.SelectedItem = _ctrl.Store.ActiveFolderWithServiceRef.ServiceRef.IdServiceRef;
    }

    private void CleanView()
    {
        textTextSearch.Text = "";
        checkSearchInDescription.Checked = true;
    }

    private void textTextSearch_KeyUp(object sender, KeyEventArgs e)
    {                        
        switch (e.KeyCode)
        {
            case Keys.Enter:
                buttonSearch_Click(this, new EventArgs());
                break;
            case Keys.Escape:
                buttonClean_Click(this, new EventArgs());
                break;
        }            
    }

    #endregion 
}
