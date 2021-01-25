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
    public partial class PostItPropertiesForm : Form, IEditorView<WindowDto>
    {
        private readonly PostItPropertiesComponent _com;
        private bool _viewFinalized = false;
        private bool _formIsDisty = false;

        public PostItPropertiesForm(PostItPropertiesComponent com)
        {
            InitializeComponent();
            _com = com;
        }

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
        public DialogResult ShowInfo(string info, string caption = "KeyNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
        {
            return MessageBox.Show(info, caption, buttons, icon);
        }

        public void RefreshView()
        {
            ModelToControls();
        }

        public void RefreshModel()
        {
            ControlsToModel();
        }


        public void CleanView()
        {
            // txt = "";            
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

        private async void PostItPropertiesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_viewFinalized)
            {
                var savedOk = await SaveModel();
                if (!savedOk)
                {
                    ShowInfo("The note could not be saved");
                }
                _com.Finalize();
            }
        }

        private async Task<bool> SaveModel()
        {            
            return await _com.SaveModel();
        }

        private void ModelToControls()
        {
            FontStyle style = new FontStyle();
            if (_com.Model.FontBold)
                style = FontStyle.Bold;
            if (_com.Model.FontItalic)
                style = style | FontStyle.Italic;
            if (_com.Model.FontUnderline)
                style = style | FontStyle.Underline;
            if (_com.Model.FontStrikethru)
                style = style | FontStyle.Strikeout;
            Font font = new Font(_com.Model.FontName, _com.Model.FontSize, style);
            labelText.Font = font;
            labelText.Text = font.OriginalFontName;

            labelCaption.BackColor = ColorTranslator.FromOle(_com.Model.TitleColor);
            labelCaption.ForeColor = ColorTranslator.FromOle(_com.Model.TextTitleColor);

            labelNote.BackColor = ColorTranslator.FromOle(_com.Model.NoteColor);            
            labelNote.ForeColor = ColorTranslator.FromOle(_com.Model.TextNoteColor);

            labelText.BackColor = ColorTranslator.FromOle(_com.Model.NoteColor);
            labelText.ForeColor = ColorTranslator.FromOle(_com.Model.TextNoteColor);
        }

        private void ControlsToModel()
        {
            _com.Model.TitleColor = ColorTranslator.ToOle(labelCaption.BackColor);
            _com.Model.TextTitleColor = ColorTranslator.ToOle(labelCaption.ForeColor);

            _com.Model.NoteColor = ColorTranslator.ToOle(labelNote.BackColor);
            _com.Model.TextNoteColor = ColorTranslator.ToOle(labelText.ForeColor);

            _com.Model.FontName = labelText.Font.Name;
            _com.Model.FontSize = (byte)labelText.Font.Size;
            _com.Model.FontBold = labelText.Font.Bold;
            _com.Model.FontItalic = labelText.Font.Italic;
            _com.Model.FontUnderline = labelText.Font.Underline;
            _com.Model.FontStrikethru = labelText.Font.Strikeout;
            _com.Model.ForeColor = ColorTranslator.ToOle(labelText.ForeColor);            
        }



        private async void buttonAccept_Click(object sender, EventArgs e)
        {            
            var res = await _com.SaveModel();
            if (res)
            {
                _formIsDisty = false;
                this.DialogResult = DialogResult.OK;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            OnCancelEdition();
        }

        private bool OnCancelEdition()
        {
            if (_formIsDisty)
            {
                if (MessageBox.Show("You have modified this entity, are you sure you want to exit without recording?", "KeyNote", MessageBoxButtons.YesNo) == DialogResult.No)
                    return false;
            }

            this.DialogResult = DialogResult.Cancel;
            _com.CancelEdition();
            return true;
        }

        private void PostItPropertiesForm_Load(object sender, EventArgs e)
        {
            // TODO: ... for debug ....
            _formIsDisty = true;
        }

        private void buttonStyle_Click(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            if (b == buttonYellow)
                SelctStyle(0);
            else if (b == buttonBlue)
                SelctStyle(1);
            else if (b == buttonGray)
                SelctStyle(2);
            else if (b == buttonCaptionColor)
                ChangeCaptionColor();
            else if (b == buttonCaptionTextColor)
                ChangeCaptionTextColor();
            else if (b == buttonNoteColor)
                ChangeNoteColor();
            else if (b == buttonNoteFont)
                ChangeNoteFont();
        }

        private void ChangeCaptionColor()
        {
            colorDialog.Color = labelCaption.BackColor;
            if (colorDialog.ShowDialog() == DialogResult.OK)            
                labelCaption.BackColor = colorDialog.Color;                            
        }

        private void ChangeCaptionTextColor()
        {
            colorDialog.Color = labelCaption.ForeColor;
            if (colorDialog.ShowDialog() == DialogResult.OK)
                labelCaption.ForeColor = colorDialog.Color;
        }


        private void ChangeNoteColor() 
        {
            colorDialog.Color = labelNote.BackColor;
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                labelNote.BackColor = colorDialog.Color;
                labelText.BackColor = colorDialog.Color;                
            }
        }

        private void ChangeNoteFont()
        {
            fontDialog.Font = labelText.Font;
            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                labelText.Font = fontDialog.Font;
                labelText.Text = fontDialog.Font.Name;
                labelText.ForeColor = fontDialog.Color;
            }
        }

        private void SelctStyle(int estilo)
        {
            FontStyle style;
            Font font;

            switch (estilo)
            {
                default:
                    labelCaption.ForeColor = ColorTranslator.FromOle(0);
                    labelCaption.BackColor = ColorTranslator.FromOle(8454143);                    
                    labelNote.ForeColor = ColorTranslator.FromOle(0);
                    labelNote.BackColor = ColorTranslator.FromOle(12648447);
                    labelText.ForeColor = ColorTranslator.FromOle(0);
                    labelText.BackColor = ColorTranslator.FromOle(12648447);
                    style = new FontStyle();
                    font = new Font("Comic Sans MS", 10, style);
                    break;

                case 1:
                    labelCaption.ForeColor = ColorTranslator.FromOle(16777215);
                    labelCaption.BackColor = ColorTranslator.FromOle(6697728);
                    labelNote.ForeColor = ColorTranslator.FromOle(16777215);
                    labelNote.BackColor = ColorTranslator.FromOle(8404992);
                    labelText.ForeColor = ColorTranslator.FromOle(16777215);
                    labelText.BackColor = ColorTranslator.FromOle(8404992);
                    style = new FontStyle();
                    font = new Font("Times New Roman", 12, style);
                    break;

                case 2:
                    labelCaption.ForeColor = ColorTranslator.FromOle(16777215);
                    labelCaption.BackColor = ColorTranslator.FromOle(0);
                    labelNote.ForeColor = ColorTranslator.FromOle(16777215);
                    labelNote.BackColor = ColorTranslator.FromOle(4605510);
                    labelText.ForeColor = ColorTranslator.FromOle(16777215);
                    labelText.BackColor = ColorTranslator.FromOle(4605510);
                    style = new FontStyle();
                    font = new Font("Courier New", 11, style);
                    break;
            }
            
            labelText.Font = font;
            labelText.Text = font.OriginalFontName;
        }



    }
}
