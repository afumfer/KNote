using KNote.ClientWin.Components;
using KNote.ClientWin.Core;
using KNote.Model;
using Microsoft.Identity.Client;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using System.Diagnostics;
using System.Text;

namespace KNote.ClientWin.Views;

public partial class KntChatGPTForm : Form, IViewBase
{
    #region Private fields

    private readonly KntChatGPTComponent _com;
    private bool _viewFinalized = false;

    private OpenAIClient _openAIClient;

    private string _organization = "";
    private string _apiKey = "";

    private List<ChatMessage> _chatMessages = new List<ChatMessage>();
    private StringBuilder _chatTextMessasges = new StringBuilder();

    private string _prompt = "";
    private int _totalTokens = 0;
    private TimeSpan _totalProcessingTime = TimeSpan.Zero;

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
            _organization = _com.Store.AppConfig.ChatGPTOrganization;
            _apiKey = _com.Store.AppConfig.ChatGPTApiKey;

            _openAIClient = new OpenAIClient(new OpenAIAuthentication(_apiKey, _organization));

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
                await GoGetCompletion(GetChatRequest(textPrompt.Text));
            else
                await GoStreamCompletion(GetChatRequest(textPrompt.Text));

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
        _prompt = "";
        _chatMessages = new List<ChatMessage>();
        _totalTokens = 0;
        _totalProcessingTime = TimeSpan.Zero;
        _chatTextMessasges.Clear();

        toolStripStatusLabelTokens.Text = $"Tokens: {_totalTokens} ";
        toolStripStatusLabelProcessingTime.Text = $" | Processing time: --";
        textResult.Text = _chatTextMessasges.ToString();
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
            textPrompt.Focus();
        }
    }

    private async Task GoGetCompletion(ChatRequest chatRequest)
    {
        var result = await _openAIClient.ChatEndpoint.GetCompletionAsync(chatRequest);

        // Create new messages objects with the response and other details
        // and add it to the messages list
        _chatMessages.Add(new ChatMessage
        {
            Prompt = _prompt,
            Role = "user",
            Tokens = result.Usage.PromptTokens
        });
        _chatMessages.Add(new ChatMessage
        {
            Prompt = result.FirstChoice.Message,
            Role = "assistant",
            Tokens = result.Usage.CompletionTokens
        });
        _totalTokens += result.Usage.TotalTokens;
        _totalProcessingTime += result.ProcessingTime;
        _chatTextMessasges.Append($"\r\n");
        _chatTextMessasges.Append($">> User:\r\n");
        _chatTextMessasges.Append($"{_prompt}\r\n");
        _chatTextMessasges.Append($"(Tokens: {result.Usage.PromptTokens})\r\n");
        _chatTextMessasges.Append($"\r\n");
        _chatTextMessasges.Append($">> Assistant:\r\n");
        _chatTextMessasges.Append(result.FirstChoice.Message.ToString().Replace("\n", "\r\n"));
        _chatTextMessasges.Append($"\r\n");
        _chatTextMessasges.Append($"(Tokens: {result.Usage.CompletionTokens} tokens. Processing time: {result.ProcessingTime})\r\n");
        _chatTextMessasges.Append($"\r\n");
        _chatTextMessasges.Append($"\r\n");

        textResult.Text = _chatTextMessasges.ToString();
        textResult.SelectionStart = textResult.Text.Length;
        textResult.ScrollToCaret();
        textPrompt.Text = "";
        toolStripStatusLabelTokens.Text = $"Tokens: {_totalTokens} ";
        toolStripStatusLabelProcessingTime.Text = $" | Processing time: {_totalProcessingTime}";
    }

    private async Task GoStreamCompletion(ChatRequest chatRequest)
    {
        StringBuilder tempResult = new();
        Stopwatch stopwatch = new();

        textResult.Text += $">> User:\r\n{_prompt}\r\n\r\n";
        textResult.Text += $">> Assistant:\r\n";

        stopwatch.Start();

        // v1
        //await foreach (var result in _openAIClient.ChatEndpoint.StreamCompletionEnumerableAsync(chatRequest))
        //{
        //    var res = result.FirstChoice.ToString()?.Replace("\n", "\r\n");
        //    tempResult.Append(res);
        //    textResult.Text += res;
        //    textResult.SelectionStart = textResult.Text.Length;
        //    textResult.ScrollToCaret();
        //    textResult.Update();
        //}

        // or v2
        await _openAIClient.ChatEndpoint.StreamCompletionAsync(chatRequest, result =>
        {
            var res = result.FirstChoice.ToString()?.Replace("\n", "\r\n");
            tempResult.Append(res);
            textResult.Text += res;
            textResult.SelectionStart = textResult.Text.Length;
            textResult.ScrollToCaret();
            textResult.Update();
        });

        stopwatch.Stop();

        _totalProcessingTime += stopwatch.Elapsed;

        // Create new messages objects with the response and other details
        // and add it to the messages list
        _chatMessages.Add(new ChatMessage
        {
            Prompt = _prompt,
            Role = "user",
            Tokens = _prompt.Length / 4    // TODO: hack, refactor this
        });
        _chatMessages.Add(new ChatMessage
        {
            Prompt = tempResult.ToString(),
            Role = "assistant",
            Tokens = tempResult.Length / 4    // TODO: hack, refactor this
        });

        _totalTokens += (_prompt.Length + tempResult.Length) / 4;    // TODO: hack, refactor this
        _chatTextMessasges.Append(tempResult);
        _chatTextMessasges.Append($"\r\n\r\n");

        textResult.Text += $"\r\n\r\n";
        textPrompt.Text = "";
        toolStripStatusLabelTokens.Text = $"Tokens: {_totalTokens}";
        toolStripStatusLabelProcessingTime.Text = $" | Processing time: {_totalProcessingTime}";
    }

    private ChatRequest GetChatRequest(string prompt)
    {
        _prompt = textPrompt.Text;

        var chatPrompts = new List<ChatPrompt>();

        // Add all existing messages to chatPrompts
        chatPrompts.Add(new ChatPrompt("system", "You are helpful Assistant"));
        foreach (var item in _chatMessages)
        {
            chatPrompts.Add(new ChatPrompt(item.Role, item.Prompt));
        }

        chatPrompts.Add(new ChatPrompt("user", _prompt));

        //return new ChatRequest(chatPrompts, OpenAI.Models.Model.GPT4);

        return new ChatRequest(
            messages: chatPrompts,
            model: OpenAI.Models.Model.GPT4,
            temperature: null,
            topP: null,
            number: null,
            stops: null,
            maxTokens: null,
            presencePenalty: null,
            frequencyPenalty: null,
            logitBias: null,
            user: null);
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
        throw new NotImplementedException();
    }

    #endregion

}
