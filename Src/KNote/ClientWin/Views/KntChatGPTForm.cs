using System.Text;
using System.Text.Json;
using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Views;

public partial class KntChatGPTForm : Form, IViewBase
{
    #region Private fields

    private readonly KntChatGPTComponent _com;
    private bool _viewFinalized = false;
    private int _countNRres;
    private StringBuilder _sbResult = new StringBuilder();

    #endregion

    #region Constructor

    public KntChatGPTForm(KntChatGPTComponent com)
    {
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        _com = com;


#if RELEASE

        // Button for debug. Ocult in release mode.
        buttonTest.Visible = false;
        
#endif
    }

    #endregion

    #region IViewBase interface

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

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        return MessageBox.Show(info, caption, buttons, icon);
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

    private async void KntChatGPTForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
        {
            if (_com.AutoSaveChatMessagesOnViewExit && !string.IsNullOrEmpty(textResult.Text))
            {
                await SaveChatMessages();
            }
            if (_com.AutoCloseComponentOnViewExit)
                _com.Finalize();
        }
    }

    #endregion

    #region Private methods

    private async Task SaveChatMessages()
    {
        try
        {
            var noteEditor = new NoteEditorComponent(_com.Store);
            await noteEditor.NewModel(_com.Store.GetActiveOrDefaultServide());
            noteEditor.Model.Topic = $"{DateTime.Now.ToString()}";
            noteEditor.Model.Description = _com.ChatTextMessasges.ToString();
            noteEditor.Model.Tags = "[ChatGPT]";
            noteEditor.Run();
        }
        catch (Exception ex)
        {
            ShowInfo(ex.Message.ToString());
        }
    }

    private void RestartChatGPT()
    {
        _com.RestartChatGPT();

        toolStripStatusLabelTokens.Text = $"Tokens: {_com.TotalTokens} ";
        toolStripStatusLabelProcessingTime.Text = $" | Processing time: --";
        textResult.Text = _com.ChatTextMessasges.ToString();
        _sbResult.Clear();
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
            radioGetCompletion.Enabled = false;
            radioGetStream.Enabled = false;
        }
        else
        {
            toolStripStatusLabelProcessing.Text = " ";
            textPrompt.Enabled = true;
            buttonSend.Enabled = true;
            buttonRestart.Enabled = true;
            radioGetCompletion.Enabled = true;
            radioGetStream.Enabled = true;
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
        _countNRres = 0;
        _com.StreamToken += _com_StreamToken;

        await _com.StreamCompletionAsync(prompt);

        RefreshStreamResult();

        textPrompt.Text = "";
        toolStripStatusLabelTokens.Text = $"Tokens: {_com.TotalTokens}";
        toolStripStatusLabelProcessingTime.Text = $" | Processing time: {_com.TotalProcessingTime}";

        _com.StreamToken -= _com_StreamToken;
    }

    private void _com_StreamToken(object sender, ComponentEventArgs<string> e)
    {
        if (textResult.InvokeRequired)
        {
            textResult.Invoke(new MethodInvoker(delegate
            {
                UpdateTextResult(e.Entity?.ToString());
            }));
        }
        else
        {
            UpdateTextResult(e.Entity?.ToString());
        }
    }

    private void UpdateTextResult(string text)
    {
        _sbResult.Append(text);
        _countNRres++;
        if(_countNRres > 10)
        {
            RefreshStreamResult();
            _countNRres = 0;
        }
    }

    private void RefreshStreamResult()
    {
        textResult.Text = _sbResult.ToString();
        textResult.SelectionStart = textResult.Text.Length;
        textResult.ScrollToCaret();
        textResult.Update();
    }

    #endregion
}
