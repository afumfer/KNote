using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;
using OpenAI;
using OpenAI.Chat;
using System.Diagnostics;
using System.Text;

namespace KNote.ClientWin.Views;

public partial class KntChatGPTForm : Form, IViewBase
{
    #region Private fields

    private readonly KntChatGPTComponent _com;
    private bool _viewFinalized = false;

    #endregion

    #region Constructor

    public KntChatGPTForm(KntChatGPTComponent com)
    {
        InitializeComponent();

        _com = com;
    }

    #endregion

    #region Form events handlers

    private void ChatGPTForm_Load(object sender, EventArgs e)
    {
        try
        {
            StatusProcessing(false);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private async void buttonSend_Click(object sender, EventArgs e)
    {
        try
        {
            StatusProcessing(true);

            if (radioGetCompletion.Checked)
                await GoGetCompletion(textPrompt.Text);
            else
                await GoStreamCompletion(textPrompt.Text);

        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        finally
        {
            StatusProcessing(false);
        }

    }

    private void buttonRestart_Click(object sender, EventArgs e)
    {
        RestartChatGPT();
    }

    private void KntChatGPTForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
            _com.Finalize();
    }

    #endregion 

    #region Private methods

    private void RestartChatGPT()
    {
        _com.RestartChatGPT();

        toolStripStatusLabelTokens.Text = $"Tokens: {_com.TotalTokens} ";
        toolStripStatusLabelProcessingTime.Text = $" | Processing time: --";
        textResult.Text = _com.ChatTextMessasges.ToString();
        textPrompt.Text = "";
    }

    private void StatusProcessing(bool processing = false)
    {
        if (processing)
        {
            toolStripStatusLabelProcessing.Text = " Processing ...";
            textPrompt.Enabled = false;
            buttonSend.Enabled = false;
            buttonRestart.Enabled = false;
        }
        else
        {
            toolStripStatusLabelProcessing.Text = " ";
            textPrompt.Enabled = true;
            buttonSend.Enabled = true;
            buttonRestart.Enabled = true;
            textResult.SelectionStart = textResult.Text.Length;
            textResult.ScrollToCaret();
            ActiveControl = textPrompt;
        }
    }

    private async Task GoGetCompletion(string prompt)
    {
        await _com.GetCompletionAsync(prompt);      
        RefreshView();
    }

    private async Task GoStreamCompletion(string prompt)
    {
        _com.StreamToken += _com_StreamToken;

        textResult.Text += $">> User:\r\n{prompt}\r\n\r\n";
        textResult.Text += $">> Assistant:\r\n";

        await _com.StreamCompletionAsync(prompt);

        textResult.Text += $"\r\n\r\n";
        textPrompt.Text = "";
        toolStripStatusLabelTokens.Text = $"Tokens: {_com.TotalTokens}";
        toolStripStatusLabelProcessingTime.Text = $" | Processing time: {_com.TotalProcessingTime}";

        _com.StreamToken -= _com_StreamToken;
    }

    private void _com_StreamToken(object sender, ComponentEventArgs<string> e)
    {
        textResult.Text += e.Entity?.ToString();
        textResult.SelectionStart = textResult.Text.Length;
        textResult.ScrollToCaret();
        textResult.Update();
    }


    #endregion

    #region IView interface

    public void ShowView()
    {
        this.Show();
    }

    public Result<EComponentResult> ShowModalView()
    {
        return _com.DialogResultToComponentResult(this.ShowDialog());
    }

    public void OnClosingView()
    {
        _viewFinalized = true;
        this.Close();
    }

    public DialogResult ShowInfo(string info, string caption = "KaNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        return MessageBox.Show("KaNote", caption, buttons, icon);
    }

    public void RefreshView()
    {
        textResult.Text = _com.ChatTextMessasges.ToString();
        textResult.SelectionStart = textResult.Text.Length;
        textResult.ScrollToCaret();
        textPrompt.Text = "";
        toolStripStatusLabelTokens.Text = $"Tokens: {_com.TotalTokens} ";
        toolStripStatusLabelProcessingTime.Text = $" | Processing time: {_com.TotalProcessingTime}";
    }

    #endregion

}
