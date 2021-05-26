using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using Markdig;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KNote.ClientWin.Views
{
    public partial class PostItEditorForm : Form, IEditorViewExt<NoteDto>
    {
        #region Private fields

        private readonly PostItEditorComponent _com;
        private bool _viewFinalized = false;

        private int _leftPosition;
        private int _topPosition;
        private int _heightRedim;
        private int _widthRedim;
        
        private Guid _selectedFolderId;

        #endregion

        public PostItEditorForm(PostItEditorComponent com)
        {
            InitializeComponent();
            _com = com;
        }

        #region IEditorView interface

        public Control PanelView()
        {
            return panelForm;
        }

        public void ShowView()
        {
            this.Show();
        }

        public Result<EComponentResult> ShowModalView()
        {
            return _com.DialogResultToComponentResult(this.ShowDialog());
        }

        public DialogResult ShowInfo(string info, string caption = "KaNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
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

        public void ConfigureEmbededMode()
        {
            
        }

        public void ConfigureWindowMode()
        {
            
        }

        #endregion

        #region Form events handlers

        private void PostItEditorForm_Load(object sender, EventArgs e)
        {
            InitializeComponentEditor();
            ModelToControlsPostIt(true, _com.ForceAlwaysTop);
        }

        private void InitializeComponentEditor()
        {
            if (_com.Model?.ContentType == "html")
            {
                htmlDescription.Location = new System.Drawing.Point(3, 28);
                htmlDescription.Size = new System.Drawing.Size(472, 292);
                htmlDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
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
                var savedOk = await _com.SaveModel();
                if (!savedOk)                
                    ShowInfo("The note could not be saved");                    
                
                _com.Finalize();
            }
        }

        private async void postItMenu_Click(object sender, EventArgs e)
        {
            ToolStripItem menuSel;
            menuSel = (ToolStripItem)sender;

            if (menuSel == menuHide)
            {
                await _com.SaveAndHide();
            }
            else if (menuSel == menuSaveNow)
            {
                await _com.SaveModel();
            }
            else if (menuSel == menuDelete)
            {
                await _com.DeleteAndFinalize();
            }
            else if (menuSel == menuAlwaysFront)
            {
                AlwaysFront();
            }
            else if (menuSel == menuExtendedEdition)
            {
                await _com.ExtendedNoteEdit();
            }
            else if (menuSel == menuPostItProperties)
            {
                PostItPropertiesEdit();
            }
            else if (menuSel == menuAddResolvedTask)
            {
                await _com.FastTaskAndHide();
            }
            else if (menuSel == menuFastAlarm10m)
            {
                await _com.FastAlarmAndHide("m", 10);
            }
            else if (menuSel == menuFastAlarm30m)
            {
                await _com.FastAlarmAndHide("m", 30);
            }
            else if (menuSel == menuFastAlarm1h)
            {
                await _com.FastAlarmAndHide("h", 1);
            }
            else if (menuSel == menuFastAlarm2h)
            {
                await _com.FastAlarmAndHide("h", 2);
            }
            else if (menuSel == menuFastAlarm4h)
            {
                await _com.FastAlarmAndHide("h", 4);
            }
            else if (menuSel == menuFastAlarm8h)
            {
                await _com.FastAlarmAndHide("h", 8);
            }
            else if (menuSel == menuFastAlarm10h)
            {
                await _com.FastAlarmAndHide("h", 10);
            }
            else if (menuSel == menuFastAlarm12h)
            {
                await _com.FastAlarmAndHide("h", 12);
            }
            else if (menuSel == menuFastAlarm24h)
            {
                await _com.FastAlarmAndHide("h", 24);
            }
            else if (menuSel == menuFastAlarm1week)
            {
                await _com.FastAlarmAndHide("week", 1);
            }
            else if (menuSel == menuFastAlarm1month)
            {
                await _com.FastAlarmAndHide("month", 1);
            }
            else if (menuSel == menuFastAlarm1year)
            {
                await _com.FastAlarmAndHide("year", 1);
            }
        }

        private async void PostItEditorForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.S:
                        await _com.SaveModel();
                        break;
                    case Keys.Q:
                        await _com.SaveAndHide();
                        break;
                    case Keys.D:
                        await _com.DeleteAndFinalize();
                        break;
                    case Keys.F:
                        AlwaysFront();
                        break;
                    case Keys.E:
                        await _com.ExtendedNoteEdit();
                        break;
                    case Keys.P:
                        PostItPropertiesEdit();
                        break;
                }
            }
            //else if (e.KeyCode == Keys.F5)
            //    menuExecKntScript_Click(this, null);
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
            await _com.ExtendedNoteEdit();
        }

        private void labelStatus_DoubleClick(object sender, EventArgs e)
        {
            var folder = _com.GetFolder();
            if (folder != null)
            {
                _selectedFolderId = folder.FolderId;
                RefreshStatus();                
            }
        }

        #endregion

        #region private Methods

        private void ModelToControls()
        {
            labelCaption.Text = _com.Model.Topic;
            RefreshStatus();
            _selectedFolderId = _com.Model.FolderId;            
            if (_com.Model?.ContentType == "html")                            
                htmlDescription.BodyHtml = _com.Model.ModelToViewDescription(_com.Service?.RepositoryRef);            
            else
            {             
                textDescription.Text = _com.Model.ModelToViewDescription(_com.Service?.RepositoryRef);
                textDescription.SelectionStart = 0;
            }
        }

        private void ControlsToModel()
        {
            if (_com.Model.ContentType == "html")
                _com.Model.Description = _com.Model.ViewToModelDescription(_com.Service?.RepositoryRef, htmlDescription.BodyHtml);
            else
                _com.Model.Description = _com.Model.ViewToModelDescription(_com.Service?.RepositoryRef, textDescription.Text);

            _com.Model.FolderId = _selectedFolderId;
            _com.Model.Topic = labelCaption.Text;

            ControlsToModelPostIt();
        }

        private void ModelToControlsPostIt(bool updateSizeAndLocation = true, bool forceAlwaysTop = false)
        {
            if (_com.Model?.ContentType == "html")
            {
                htmlDescription.Visible = true;
            }
            else
            {
                FontStyle style = new FontStyle();
                if (_com.WindowPostIt.FontBold)
                    style = FontStyle.Bold;
                if (_com.WindowPostIt.FontItalic)
                    style = style | FontStyle.Italic;
                if (_com.WindowPostIt.FontUnderline)
                    style = style | FontStyle.Underline;
                if (_com.WindowPostIt.FontStrikethru)
                    style = style | FontStyle.Strikeout;
                Font font = new Font(_com.WindowPostIt.FontName, _com.WindowPostIt.FontSize, style);
                textDescription.Font = font;
                textDescription.BackColor = ColorTranslator.FromHtml(_com.WindowPostIt.NoteColor);
                textDescription.ForeColor = ColorTranslator.FromHtml(_com.WindowPostIt.TextNoteColor);
                textDescription.Visible = true;
            }

            if (updateSizeAndLocation)
            {
                // Avoid positions outside the view zone
                if (_com.WindowPostIt.PosX > SystemInformation.VirtualScreen.Width - 50)
                    _com.WindowPostIt.PosX = SystemInformation.VirtualScreen.Width - _com.WindowPostIt.Width;
                if (_com.WindowPostIt.PosY > SystemInformation.VirtualScreen.Height - 50)
                    _com.WindowPostIt.PosY = SystemInformation.VirtualScreen.Height - _com.WindowPostIt.Height;

                this.Location = new System.Drawing.Point(_com.WindowPostIt.PosX, _com.WindowPostIt.PosY);
                this.Size = new System.Drawing.Size(_com.WindowPostIt.Width, _com.WindowPostIt.Height);

                if (forceAlwaysTop)
                    _com.WindowPostIt.AlwaysOnTop = true;
                this.TopMost = menuAlwaysFront.Checked = _com.WindowPostIt.AlwaysOnTop;
            }

            labelCaption.BackColor = ColorTranslator.FromHtml(_com.WindowPostIt.TitleColor);
            labelCaption.ForeColor = ColorTranslator.FromHtml(_com.WindowPostIt.TextTitleColor);
            BackColor = ColorTranslator.FromHtml(_com.WindowPostIt.NoteColor);
            labelStatus.BackColor = ColorTranslator.FromHtml(_com.WindowPostIt.NoteColor);
        }

        private void ControlsToModelPostIt()
        {
            _com.WindowPostIt.PosY = this.Top;
            _com.WindowPostIt.PosX = this.Left;
            _com.WindowPostIt.Width = this.Width;
            _com.WindowPostIt.Height = this.Height;

            _com.WindowPostIt.AlwaysOnTop = menuAlwaysFront.Checked;
        }

        private void PostItPropertiesEdit()
        {
            var copyTopMost = TopMost;
            TopMost = false;
            var window = _com.GetWindow();
            if (window != null)
            {
                _com.WindowPostIt = window;
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
            var status = string.IsNullOrEmpty(_com.Model.InternalTags) ? "" : $" - ({_com.Model.InternalTags})";
            labelStatus.Text = $"{_com.ServiceRef?.Alias} >> [{_com.Model.FolderDto.Name}] {status}";
        }
       
        #endregion
    }
}
