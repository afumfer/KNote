﻿using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

public partial class PostItEditorForm : Form, IViewPostIt<NoteDto>
{
    #region Private fields

    private readonly PostItEditorCtrl _ctrl;
    private bool _viewFinalized = false;

    private int _leftPosition;
    private int _topPosition;
    private int _heightRedim;
    private int _widthRedim;
        
    private Guid _selectedFolderId;

    #endregion

    #region Constructor

    public PostItEditorForm(PostItEditorCtrl com)
    {
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        _ctrl = com;
    }

    #endregion 

    #region IView interface

    public void ShowView()
    {
        this.Show();
    }

    public Result<EComponentResult> ShowModalView()
    {
        return _ctrl.DialogResultToComponentResult(this.ShowDialog());
    }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        return MessageBox.Show(info, caption, buttons, icon);
    }

    public void RefreshView()
    {
        ModelToControls();
    }

    public void HideView()
    {
        this.Hide();
    }

    public void ActivateView()
    {
        this.Show();
    }

    public void RefreshModel()
    {
        ControlsToModel();
    }

    public void CleanView()
    {
            
    }

    public void OnClosingView()
    {
        _viewFinalized = true;
        this.Close();
    }

    #endregion

    #region Form events handlers

    private void PostItEditorForm_Load(object sender, EventArgs e)
    {
        InitializeComponentEditor();
        ModelToControlsPostIt(true, _ctrl.ForceAlwaysTop);
    }

    private void InitializeComponentEditor()
    {
        if (_ctrl.Model is null)
            return;

        if (_ctrl.Model.ContentType.Contains("html"))
        {
            htmlDescription.Location = new System.Drawing.Point(3, 28);
            htmlDescription.Size = new System.Drawing.Size(472, 292);
            htmlDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));                
        }
        else if (_ctrl.Model.ContentType.Contains("navigation"))
        {
            webView2.Location = new System.Drawing.Point(3, 28);
            webView2.Size = new System.Drawing.Size(472, 292);
            webView2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
        }
        else
        {
            textDescription.Location = new System.Drawing.Point(3, 28);
            textDescription.Size = new System.Drawing.Size(472, 292);
            textDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));                
        }
    }

    private async void PostItEditorForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
        {
            var savedOk = await _ctrl.SaveModel();
            if (!savedOk)                
                ShowInfo("The note could not be saved");                    
                
            _ctrl.Finalize();
        }
    }

    private async void postItMenu_Click(object sender, EventArgs e)
    {
        ToolStripItem menuSel;
        menuSel = (ToolStripItem)sender;

        if (menuSel == menuHide)
        {
            await _ctrl.SaveAndHide();
        }
        else if (menuSel == menuSaveNow)
        {
            await _ctrl.SaveModel();
        }
        else if (menuSel == menuDelete)
        {
            await _ctrl.DeleteAndFinalize();
        }
        else if (menuSel == menuAlwaysFront)
        {
            AlwaysFront();
        }
        else if (menuSel == menuExtendedEdition)
        {
            await _ctrl.ExtendedNoteEdit();
        }
        else if (menuSel == menuPostItProperties)
        {
            PostItPropertiesEdit();
        }
        else if (menuSel == menuAddResolvedTask)
        {
            await _ctrl.FastTaskAndHide();
        }
        else if (menuSel == menuFastAlarm10m)
        {
            await _ctrl.FastAlarmAndHide("m", 10);
        }
        else if (menuSel == menuFastAlarm30m)
        {
            await _ctrl.FastAlarmAndHide("m", 30);
        }
        else if (menuSel == menuFastAlarm1h)
        {
            await _ctrl.FastAlarmAndHide("h", 1);
        }
        else if (menuSel == menuFastAlarm2h)
        {
            await _ctrl.FastAlarmAndHide("h", 2);
        }
        else if (menuSel == menuFastAlarm4h)
        {
            await _ctrl.FastAlarmAndHide("h", 4);
        }
        else if (menuSel == menuFastAlarm8h)
        {
            await _ctrl.FastAlarmAndHide("h", 8);
        }
        else if (menuSel == menuFastAlarm10h)
        {
            await _ctrl.FastAlarmAndHide("h", 10);
        }
        else if (menuSel == menuFastAlarm12h)
        {
            await _ctrl.FastAlarmAndHide("h", 12);
        }
        else if (menuSel == menuFastAlarm24h)
        {
            await _ctrl.FastAlarmAndHide("h", 24);
        }
        else if (menuSel == menuFastAlarm1week)
        {
            await _ctrl.FastAlarmAndHide("week", 1);
        }
        else if (menuSel == menuFastAlarm1month)
        {
            await _ctrl.FastAlarmAndHide("month", 1);
        }
        else if (menuSel == menuFastAlarm1year)
        {
            await _ctrl.FastAlarmAndHide("year", 1);
        }
    }

    private async void PostItEditorForm_KeyUp(object sender, KeyEventArgs e)
    {
        if (e.Control)
        {
            switch (e.KeyCode)
            {
                case Keys.S:
                    await _ctrl.SaveModel();
                    break;
                case Keys.Q:
                    await _ctrl.SaveAndHide();
                    break;
                case Keys.D:
                    await _ctrl.DeleteAndFinalize();
                    break;
                case Keys.F:
                    AlwaysFront();
                    break;
                case Keys.E:
                    await _ctrl.ExtendedNoteEdit();
                    break;
                case Keys.P:
                    PostItPropertiesEdit();
                    break;
            }
        }
    }

    private void labelCaption_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _leftPosition = e.X;
            _topPosition = e.Y;
        }
    }

    private void labelCaption_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            this.Left = this.Left - (_leftPosition - e.X);
            this.Top = this.Top - (_topPosition - e.Y);
        }
    }

    private void picResize_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _heightRedim = e.Y;
            _widthRedim = e.X;
        }
    }

    private void picResize_MouseMove(object sender, MouseEventArgs e)
    {
        try
        {
            int widthTmp = this.Width;
            int heightTmp = this.Height;
            if (e.Button == MouseButtons.Left)
            {
                widthTmp = widthTmp - (_widthRedim - e.X);
                heightTmp = heightTmp - (_heightRedim - e.Y);
                this.Width = widthTmp;
                this.Height = heightTmp;
                DrawFormBorder();
            }
        }
        catch { }
    }

    private void picMenu_MouseUp(object sender, MouseEventArgs e)
    {
        menuPostIt.Show(picMenu, new Point(e.X, e.Y));
    }

    private void PostItEditorForm_Paint(object sender, PaintEventArgs e)
    {
        DrawFormBorder();
    }

    private async void labelCaption_DoubleClick(object sender, EventArgs e)
    {
        await _ctrl.ExtendedNoteEdit();
    }

    private void labelStatus_DoubleClick(object sender, EventArgs e)
    {
        var tmpTopMost = TopMost;
        TopMost = false;
        Refresh();

        var folder = _ctrl.GetFolder();
        if (folder != null)
        {
            _selectedFolderId = folder.FolderId;
            _ctrl.Model.FolderDto = folder.GetSimpleDto<FolderDto>();
            RefreshStatus();                
        }

        TopMost = tmpTopMost;
    }

    #endregion

    #region Private Methods

    private async void ModelToControls()
    {
        if (_ctrl.Model is null)
            return;

        labelCaption.Text = _ctrl.Model.Topic;
        RefreshStatus();
        _selectedFolderId = _ctrl.Model.FolderId;

        if (_ctrl.Model.ContentType.Contains("html"))
            htmlDescription.BodyHtml = _ctrl.Service?.Notes.UtilUpdateResourceInDescriptionForRead(_ctrl.Model?.Description, true);
        else if (_ctrl.Model.ContentType.Contains("navigation"))
        {
            webView2.TextUrl = _ctrl.Model.Description;            
            if (!string.IsNullOrEmpty(_ctrl.Model.Description))            
                await webView2.Navigate();
            else
                await webView2.NavigateToString(" ");
        }
        else
        {            
            textDescription.Text = _ctrl.Service?.Notes.UtilUpdateResourceInDescriptionForRead(_ctrl.Model?.Description, true);
            textDescription.SelectionStart = 0;
        }
    }

    private void ControlsToModel()
    {
        if (_ctrl.Model is null)
            return;

        if (_ctrl.Model.ContentType.Contains("html"))
            _ctrl.Model.Description = _ctrl.Service?.Notes.UtilUpdateResourceInDescriptionForWrite(htmlDescription.BodyHtml, true);
        else if (_ctrl.Model.ContentType.Contains("navigation"))
            _ctrl.Model.Description = webView2.TextUrl;
        else            
            _ctrl.Model.Description = _ctrl.Service?.Notes.UtilUpdateResourceInDescriptionForWrite(textDescription.Text, true);

        _ctrl.Model.FolderId = _selectedFolderId;
        _ctrl.Model.Topic = labelCaption.Text;

        ControlsToModelPostIt();
    }

    private void ModelToControlsPostIt(bool updateSizeAndLocation = true, bool forceAlwaysTop = false)
    {
        if (_ctrl.Model is null)
            return;

        if (_ctrl.Model.ContentType.Contains("html"))
        {
            htmlDescription.Visible = true;
        }
        if (_ctrl.Model.ContentType.Contains("navigation"))
        {
            webView2.Visible = true;
        }
        else
        {
            FontStyle style = new FontStyle();
            if (_ctrl.WindowPostIt.FontBold)
                style = FontStyle.Bold;
            if (_ctrl.WindowPostIt.FontItalic)
                style = style | FontStyle.Italic;
            if (_ctrl.WindowPostIt.FontUnderline)
                style = style | FontStyle.Underline;
            if (_ctrl.WindowPostIt.FontStrikethru)
                style = style | FontStyle.Strikeout;
            Font font = new Font(_ctrl.WindowPostIt.FontName, _ctrl.WindowPostIt.FontSize, style);
            textDescription.Font = font;
            textDescription.BackColor = ColorTranslator.FromHtml(_ctrl.WindowPostIt.NoteColor);
            textDescription.ForeColor = ColorTranslator.FromHtml(_ctrl.WindowPostIt.TextNoteColor);
            textDescription.Visible = true;
        }

        if (updateSizeAndLocation)
        {
            // Avoid positions outside the view zone
            if (_ctrl.WindowPostIt.PosX > SystemInformation.VirtualScreen.Width - 50)
                _ctrl.WindowPostIt.PosX = SystemInformation.VirtualScreen.Width - _ctrl.WindowPostIt.Width;
            if (_ctrl.WindowPostIt.PosY > SystemInformation.VirtualScreen.Height - 50)
                _ctrl.WindowPostIt.PosY = SystemInformation.VirtualScreen.Height - _ctrl.WindowPostIt.Height;

            this.Location = new System.Drawing.Point(_ctrl.WindowPostIt.PosX, _ctrl.WindowPostIt.PosY);
            this.Size = new System.Drawing.Size(_ctrl.WindowPostIt.Width, _ctrl.WindowPostIt.Height);

            if (forceAlwaysTop)
                _ctrl.WindowPostIt.AlwaysOnTop = true;
            this.TopMost = menuAlwaysFront.Checked = _ctrl.WindowPostIt.AlwaysOnTop;
        }

        labelCaption.BackColor = ColorTranslator.FromHtml(_ctrl.WindowPostIt.TitleColor);
        labelCaption.ForeColor = ColorTranslator.FromHtml(_ctrl.WindowPostIt.TextTitleColor);
        BackColor = ColorTranslator.FromHtml(_ctrl.WindowPostIt.NoteColor);
        labelStatus.BackColor = ColorTranslator.FromHtml(_ctrl.WindowPostIt.NoteColor);
    }

    private void ControlsToModelPostIt()
    {
        _ctrl.WindowPostIt.PosY = this.Top;
        _ctrl.WindowPostIt.PosX = this.Left;
        _ctrl.WindowPostIt.Width = this.Width;
        _ctrl.WindowPostIt.Height = this.Height;

        _ctrl.WindowPostIt.AlwaysOnTop = menuAlwaysFront.Checked;
    }

    private void PostItPropertiesEdit()
    {
        var copyTopMost = TopMost;
        TopMost = false;
        var window = _ctrl.GetWindow();
        if (window != null)
        {
            _ctrl.WindowPostIt = window;
            ModelToControlsPostIt(false);
        }
        TopMost = copyTopMost;
    }

    public void AlwaysFront()
    {
        menuAlwaysFront.Checked = !menuAlwaysFront.Checked;
        TopMost = menuAlwaysFront.Checked;
        // It is necessary set focus or select this form
        textDescription.Focus();
    }

    private void DrawFormBorder()
    {
        Graphics grfx = this.CreateGraphics();
        Pen pn = new Pen(Color.Black);
        grfx.Clear(this.BackColor);
        grfx.DrawRectangle(pn, 0, 0, this.Width - 1, this.Height - 1);
    }

    private void RefreshStatus()
    {
        var status = string.IsNullOrEmpty(_ctrl.Model.InternalTags) ? "" : $" - ({_ctrl.Model.InternalTags})";
        labelStatus.Text = $"{_ctrl.ServiceRef?.Alias} >> [{_ctrl.Model.FolderDto.Name}] {status}";
    }

    #endregion 
}