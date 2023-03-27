using KNote.ClientWin.Core;
using KNote.Model;
using Microsoft.Identity.Client;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using System.Diagnostics;
using System.Text;

namespace KNote.ClientWin.Views;

public partial class ChatGPTForm : Form
{
    private Store _store;
    private OpenAIClient _openAIClient;

    private string _organization = "";
    private string _apiKey = "";

    private List<ChatMessage> _chatMessages = new List<ChatMessage>();
    private StringBuilder _chatTextMessasges = new StringBuilder();

    private string _prompt = "";
    private int _totalTokens = 0;
    private TimeSpan _totalProcessingTime = TimeSpan.Zero;

    public ChatGPTForm()
    {
        InitializeComponent();
    }

    public ChatGPTForm(Store store) : this()
    {
        _store = store;

    }

    private void ChatGPTForm_Load(object sender, EventArgs e)
    {
        try
        {
            _organization = _store.AppConfig.ChatGPTOrganization;
            _apiKey = _store.AppConfig.ChatGPTApiKey;

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
        Stopwatch stopwatch = new ();

        textResult.Text += $">> User:\r\n{_prompt}\r\n\r\n";
        textResult.Text += $">> Assistant:\r\n";

        stopwatch.Start();

        // v1
        //await foreach (var result in _openAIClient.ChatEndpoint.StreamCompletionEnumerableAsync(chatRequest))
        //{
        //    textResult.Text += result.FirstChoice.ToString()?.Replace("\n", "\r\n");
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

}
