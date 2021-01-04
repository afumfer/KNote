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
    public partial class PostItEditorForm : Form, IEditorView<NoteDto>
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

        public DialogResult ShowInfo(string info, string caption = "KeyNote", MessageBoxButtons buttons = MessageBoxButtons.OK)
        {
            return MessageBox.Show(info, caption, buttons);
        }

        public void RefreshView()
        {
            ModelToControls();
        }

        public void CleanView()
        {
            // textXxxx = "";
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
            
        }

        private async void PostItEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_viewFinalized)
            {
                var savedOk = await SaveModel();
                if (!savedOk)
                {
                    //if (MessageBox.Show("Do yo want exit?", "KeyNote", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    //    _com.Finalize();
                    //else
                    //    e.Cancel = true;

                    ShowInfo("The note could not be saved");                    
                }
                _com.Finalize();
            }
        }

        private async void postItMenu_Click(object sender, EventArgs e)
        {
            ToolStripItem menuSel;
            menuSel = (ToolStripItem)sender;

            if (menuSel == menuHide)
            {
                await SaveModel();
                _com.Finalize();
            }
            else if (menuSel == menuSaveNow)
            {
                await SaveModel();
            }
            //else if (menuSel == toolXxxxx)
            //{

            //}
        }

        #endregion

        #region private Methods

        private async Task<bool> SaveModel()
        {
            ControlsToModel();            
            return await _com.SaveModel();
        }

        private void ModelToControls()
        {
            labelCaption.Text = _com.Model.Topic;
            textDescription.Text = _com.Model.Description;

            textDescription.SelectionStart = 0;
        }

        private void ControlsToModel()
        {
            _com.Model.Description = textDescription.Text;
        }


        #endregion

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
                    //if ( xxShowBorder )
                        DrawBorder();
                }
            }
            catch { }
        }

        private void DrawBorder()
        {
            // Si queremos border
            Graphics grfx = this.CreateGraphics();
            Pen pn = new Pen(Color.Black);
            grfx.Clear(this.BackColor);
            grfx.DrawRectangle(pn, 0, 0, this.Width - 1, this.Height - 1);

            //Graphics grfx = panelForm.CreateGraphics();
            //Pen pn = new Pen(Color.Black);
            //grfx.Clear(this.BackColor);
            //grfx.DrawRectangle(pn, 0, 0, panelForm.Width - 1, panelForm.Height - 1);
        }

        private void picMenu_MouseUp(object sender, MouseEventArgs e)
        {
            menuPostIt.Show(picMenu, new Point(e.X, e.Y));
        }

        private void PostItEditorForm_Paint(object sender, PaintEventArgs e)
        {
            DrawBorder();
        }
    }
}
