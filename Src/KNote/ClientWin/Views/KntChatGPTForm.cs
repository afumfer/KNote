using System.Text;
using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Views;

public partial class KntChatGPTForm : Form, IViewBase
{
    #region Private fields

    private readonly KntChatGPTCtrl _ctrl;
    private bool _viewFinalized = false;
    private int _countNRres;
    private StringBuilder _sbResult = new StringBuilder();
    private const string ViewCaptionText = "KNote ChatAI";

    #endregion

    #region Constructor

    public KntChatGPTForm(KntChatGPTCtrl ctrl)
    {
        AutoScaleMode = AutoScaleMode.Dpi;

        InitializeComponent();

        _ctrl = ctrl;
    }

    #endregion

    #region IViewBase interface

    public void ShowView()
    {
        toolStripStatusServiceRef.Text = $" {_ctrl.ServiceRef.Alias}";
        MarkDownView();
        this.Show();
    }

    public Result<EControllerResult> ShowModalView()
    {
        return _ctrl.DialogResultToControllerResult(this.ShowDialog());
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
        kntEditViewResult.MarkdownContentControl.Text = _ctrl.ChatTextMessasges.ToString();
        kntEditViewResult.MarkdownContentControl.SelectionStart = kntEditViewResult.MarkdownContentControl.Text.Length;
        kntEditViewResult.MarkdownContentControl.ScrollToCaret();
        textPrompt.Text = "";
        toolStripStatusLabelTokens.Text = $"Tokens: {_ctrl.TotalTokens} ";
        toolStripStatusLabelProcessingTime.Text = $" | Processing time: {_ctrl.TotalProcessingTime}";
    }

    #endregion

    #region Form events handlers

    private void KntChatGPTForm_Load(object sender, EventArgs e)
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

    private async void KntChatGPTForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
        {
            if (_ctrl.AutoSaveChatMessagesOnViewExit && !string.IsNullOrEmpty(kntEditViewResult.MarkdownText))
            {
                await SaveChatMessages();
            }
            if (_ctrl.AutoCloseCtrlOnViewExit)
                _ctrl.Finalize();
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
        _ctrl.RootSystemChat = KntConst.DefaultRootSystemChat;
        _ctrl.RestartChatGPT();
        RestartChatGPTView();
        Text = $"{ViewCaptionText}";
    }

    private async void buttonCatalogPrompts_Click(object sender, EventArgs e)
    {
        var assistantInfo = await _ctrl.GetCatalogPrompt();
        if (assistantInfo == null)                   
            return;
        
        RestartChatGPTView();
        Text = $"{ViewCaptionText} - {assistantInfo.Name}";
        textPrompt.Text = assistantInfo.User;
    }

    private void buttonViewSystem_Click(object sender, EventArgs e)
    {
        ShowInfo($"System: {_ctrl.RootSystemChat}", $"{KntConst.AppName} - root system chat ");
    }

    private void buttonMarkDown_Click(object sender, EventArgs e)
    {
        MarkDownView();
    }

    private async void buttonNavigate_Click(object sender, EventArgs e)
    {
        await NavigateView();
    }

    #endregion

    #region Private methods

    private async Task SaveChatMessages()
    {
        try
        {
            var noteEditor = new NoteEditorCtrl(_ctrl.Store);
            await noteEditor.NewModel(_ctrl.Store.GetActiveOrDefaultService());
            noteEditor.Model.Topic = $"{DateTime.Now.ToString()}";
            noteEditor.Model.Description = _ctrl.ChatTextMessasges.ToString();
            noteEditor.Model.Tags = "[ChatGPT]";
            noteEditor.Run();
        }
        catch (Exception ex)
        {
            ShowInfo(ex.Message.ToString());
        }
    }

    private void RestartChatGPTView()
    {
        toolStripStatusLabelTokens.Text = $"Tokens: {_ctrl.TotalTokens} ";
        toolStripStatusLabelProcessingTime.Text = $" | Processing time: --";        
        kntEditViewResult.SetMarkdownContent(_ctrl.ChatTextMessasges.ToString());
        MarkDownView();
        _sbResult.Clear();
        textPrompt.Text = "";
        textPrompt.Focus();
    }

    private void StatusProcessing(bool processing = false)
    {
        if (processing)
        {
            MarkDownView();
            toolStripStatusLabelProcessing.Text = " Processing ...";
            textPrompt.Enabled = false;
            buttonSend.Enabled = false;
            buttonRestart.Enabled = false;
            radioGetCompletion.Enabled = false;
            radioGetStream.Enabled = false;
            buttonCatalogPrompts.Enabled = false;
            buttonViewSystem.Enabled = false;
            buttonMarkDown.Enabled = false;
            buttonNavigate.Enabled = false;
        }
        else
        {           
            toolStripStatusLabelProcessing.Text = " ";
            textPrompt.Enabled = true;
            buttonSend.Enabled = true;
            buttonRestart.Enabled = true;
            radioGetCompletion.Enabled = true;
            radioGetStream.Enabled = true;
            buttonCatalogPrompts.Enabled = true;
            buttonViewSystem.Enabled = true;
            buttonMarkDown.Enabled = true;
            buttonNavigate.Enabled = true;
            kntEditViewResult.MarkdownContentControl.SelectionStart = kntEditViewResult.MarkdownContentControl.Text.Length;
            kntEditViewResult.MarkdownContentControl.ScrollToCaret();
            ActiveControl = textPrompt;
        }
    }

    private async Task GoGetCompletion(string prompt)
    {
        await _ctrl.GetCompletionAsync(prompt);
        RefreshView();
    }

    private async Task GoStreamCompletion(string prompt)
    {
        _countNRres = 0;
        _ctrl.StreamToken += _com_StreamToken;

        await _ctrl.StreamCompletionAsync(prompt);

        RefreshStreamResult();

        textPrompt.Text = "";
        toolStripStatusLabelTokens.Text = $"Tokens: {_ctrl.TotalTokens}";
        toolStripStatusLabelProcessingTime.Text = $" | Processing time: {_ctrl.TotalProcessingTime}";

        _ctrl.StreamToken -= _com_StreamToken;
    }

    private void _com_StreamToken(object sender, ControllerEventArgs<string> e)
    {
        if (kntEditViewResult.MarkdownContentControl.InvokeRequired)
        {
            kntEditViewResult.MarkdownContentControl.Invoke(new MethodInvoker(delegate
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
        if (_countNRres > 10)
        {
            RefreshStreamResult();
            _countNRres = 0;
        }
    }

    private void RefreshStreamResult()
    {
        kntEditViewResult.MarkdownContentControl.Text = _sbResult.ToString();
        kntEditViewResult.MarkdownContentControl.SelectionStart = kntEditViewResult.MarkdownContentControl.Text.Length;        
        kntEditViewResult.MarkdownContentControl.ScrollToCaret();
        kntEditViewResult.MarkdownContentControl.Update();
    }

    private void MarkDownView()
    {
        kntEditViewResult.ShowMarkdownContent();
        buttonMarkDown.Enabled = false;
        buttonNavigate.Enabled = true;
    }

    private async Task NavigateView()
    {
        var service = _ctrl.ServiceRef.Service;
        var content = kntEditViewResult.MarkdownText;
        var htmlContent = service.Notes.UtilMarkdownToHtml(content.Replace(service.RepositoryRef.ResourcesContainerRootUrl, KntConst.VirtualHostNameToFolderMapping));
        await kntEditViewResult.ShowNavigationContent(htmlContent + _ctrl.Store.KNoteWebViewStyle);
        buttonMarkDown.Enabled = true;
        buttonNavigate.Enabled = false;
    }

    #endregion
}
