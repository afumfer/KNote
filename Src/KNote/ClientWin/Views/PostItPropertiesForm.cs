using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Views;

public partial class PostItPropertiesForm : Form, IViewPostIt<WindowDto>
{
    #region Private fields

    private readonly PostItPropertiesCtrl _ctrl;
    private bool _viewFinalized = false;
    private bool _formIsDisty = false;

    #endregion

    #region Constructor

    public PostItPropertiesForm(PostItPropertiesCtrl ctrl)
    {
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        _ctrl = ctrl;
    }

    #endregion

    #region IView

    public void ShowView()
    {
        this.Show();
    }

    public Result<EControllerResult> ShowModalView()
    {
        return _ctrl.DialogResultToControllerResult(this.ShowDialog());
    }
    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
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

    public void OnClosingView()
    {
        _viewFinalized = true;
        this.Close();
    }

    public void HideView()
    {
        throw new NotImplementedException();
    }

    public void ActivateView()
    {
        throw new NotImplementedException();
    }

    public void CleanView()
    {
        throw new NotImplementedException();
    }

    #endregion 

    #region Form events handlers

    private async void PostItPropertiesForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
        {
            var savedOk = await SaveModel();
            if (!savedOk)
            {
                ShowInfo("The note could not be saved");
            }
            _ctrl.Finalize();
        }
    }

    private async void buttonAccept_Click(object sender, EventArgs e)
    {            
        var res = await _ctrl.SaveModel();
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
    
    private void PostItPropertiesForm_Load(object sender, EventArgs e)
    {            
        _formIsDisty = false;
    }

    #endregion

    #region Private methods

    private async Task<bool> SaveModel()
    {
        return await _ctrl.SaveModel();
    }

    private void ModelToControls()
    {
        FontStyle style = new FontStyle();
        if (_ctrl.Model.FontBold)
            style = FontStyle.Bold;
        if (_ctrl.Model.FontItalic)
            style = style | FontStyle.Italic;
        if (_ctrl.Model.FontUnderline)
            style = style | FontStyle.Underline;
        if (_ctrl.Model.FontStrikethru)
            style = style | FontStyle.Strikeout;
        Font font = new Font(_ctrl.Model.FontName, _ctrl.Model.FontSize, style);
        labelText.Font = font;
        labelText.Text = font.OriginalFontName;

        labelCaption.BackColor = ColorTranslator.FromHtml(_ctrl.Model.TitleColor);
        labelCaption.ForeColor = ColorTranslator.FromHtml(_ctrl.Model.TextTitleColor);
        labelNote.BackColor = ColorTranslator.FromHtml(_ctrl.Model.NoteColor);
        labelNote.ForeColor = ColorTranslator.FromHtml(_ctrl.Model.TextNoteColor);
        labelText.BackColor = ColorTranslator.FromHtml(_ctrl.Model.NoteColor);
        labelText.ForeColor = ColorTranslator.FromHtml(_ctrl.Model.TextNoteColor);
    }

    private void ControlsToModel()
    {
        _ctrl.Model.TitleColor = ColorTranslator.ToHtml(labelCaption.BackColor);
        _ctrl.Model.TextTitleColor = ColorTranslator.ToHtml(labelCaption.ForeColor);

        _ctrl.Model.NoteColor = ColorTranslator.ToHtml(labelNote.BackColor);
        _ctrl.Model.TextNoteColor = ColorTranslator.ToHtml(labelText.ForeColor);

        _ctrl.Model.FontName = labelText.Font.Name;
        _ctrl.Model.FontSize = (byte)labelText.Font.Size;
        _ctrl.Model.FontBold = labelText.Font.Bold;
        _ctrl.Model.FontItalic = labelText.Font.Italic;
        _ctrl.Model.FontUnderline = labelText.Font.Underline;
        _ctrl.Model.FontStrikethru = labelText.Font.Strikeout;
        _ctrl.Model.ForeColor = ColorTranslator.ToHtml(labelText.ForeColor);
    }

    private bool OnCancelEdition()
    {
        if (_formIsDisty)
        {
            if (MessageBox.Show("You have modified this entity, are you sure you want to exit without recording?", KntConst.AppName, MessageBoxButtons.YesNo) == DialogResult.No)
                return false;
        }

        this.DialogResult = DialogResult.Cancel;
        _ctrl.CancelEdition();
        return true;
    }

    private void buttonStyle_Click(object sender, EventArgs e)
    {            
        Button b = (Button)sender;
        if (b == buttonYellow)
            SelctStyle(0);
        else if (b == buttonLightGray)
            SelctStyle(1);
        else if (b == buttonDark)
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
        {
            labelCaption.BackColor = colorDialog.Color;                            
            _formIsDisty = true;
        }
    }

    private void ChangeCaptionTextColor()
    {
        colorDialog.Color = labelCaption.ForeColor;
        if (colorDialog.ShowDialog() == DialogResult.OK)
        {
            labelCaption.ForeColor = colorDialog.Color;
            _formIsDisty = true;
        }
    }

    private void ChangeNoteColor() 
    {
        colorDialog.Color = labelNote.BackColor;
        if (colorDialog.ShowDialog() == DialogResult.OK)
        {
            labelNote.BackColor = colorDialog.Color;
            labelText.BackColor = colorDialog.Color;
            _formIsDisty = true;
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
            _formIsDisty = true;
        }
    }

    private void SelctStyle(int estilo)
    {
        FontStyle style;
        Font font;
                        
        switch (estilo)
        {
            default:
                labelCaption.ForeColor = ColorTranslator.FromHtml("Black");
                labelCaption.BackColor = ColorTranslator.FromHtml("#FFFF80");
                labelNote.ForeColor = ColorTranslator.FromHtml("Black");
                labelNote.BackColor = ColorTranslator.FromHtml("#FFFFC0");
                labelText.ForeColor = ColorTranslator.FromHtml("Black");
                labelText.BackColor = ColorTranslator.FromHtml("#FFFFC0");

                style = new FontStyle();
                font = new Font("Comic Sans MS", 10, style);
                break;

            case 1:                   
                labelCaption.ForeColor = ColorTranslator.FromHtml("#484848");
                labelCaption.BackColor = ColorTranslator.FromHtml("Silver");
                labelNote.ForeColor = ColorTranslator.FromHtml("Black");
                labelNote.BackColor = ColorTranslator.FromHtml("#EBEBEB");
                labelText.ForeColor = ColorTranslator.FromHtml("Black");
                labelText.BackColor = ColorTranslator.FromHtml("#EBEBEB");

                style = new FontStyle();
                font = new Font("Courier New", 11, style);
                break;

            case 2:
                labelCaption.ForeColor = ColorTranslator.FromHtml("White");
                labelCaption.BackColor = ColorTranslator.FromHtml("Black");
                labelNote.ForeColor = ColorTranslator.FromHtml("White");
                labelNote.BackColor = ColorTranslator.FromHtml("#464646");
                labelText.ForeColor = ColorTranslator.FromHtml("White");
                labelText.BackColor = ColorTranslator.FromHtml("#464646");

                style = new FontStyle();
                font = new Font("Courier New", 11, style);
                break;
        }
            
        labelText.Font = font;
        labelText.Text = font.OriginalFontName;
        _formIsDisty = true;
    }

    #endregion
}
