using KNote.ClientWin.Core;
using KNote.Model;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using System.Text;

namespace KNote.ClientWin.Views;

public partial class ChatGPTForm : Form
{
    private Store _store;
    private OpenAIClient _openAIClient;

    private string Organization = "";
    private string ApiKey = "";
    private List<ChatMessage> chatMessages = new List<ChatMessage>();
    private string prompt = "";
    private StringBuilder chatTextMessasges = new StringBuilder();
    private int TotalTokens = 0;
    //private TimeSpan TotalProcessingTime = TimeSpan.Zero;

    public ChatGPTForm()
    {
        InitializeComponent();
    }

    public ChatGPTForm(Store store) : this()
    {
        _store = store;

        Organization = _store.AppConfig.ChatGPTOrganization;
        ApiKey = _store.AppConfig.ChatGPTApiKey;
    }

    private void ChatGPTForm_Load(object sender, EventArgs e)
    {
        try
        {
            _openAIClient = new OpenAIClient(new OpenAIAuthentication(ApiKey, Organization));
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

            prompt = textPrompt.Text;

            var chatPrompts = new List<ChatPrompt>();

            // Add all existing messages to chatPrompts
            chatPrompts.Add(new ChatPrompt("system", "You are helpful Assistant"));
            foreach (var item in chatMessages)
            {
                chatPrompts.Add(new ChatPrompt(item.Role, item.Prompt));
            }

            chatPrompts.Add(new ChatPrompt("user", prompt));
            var chatRequest = new ChatRequest(chatPrompts, OpenAI.Models.Model.GPT4);


            await GoGetCompletion(chatRequest);

            // await GoStreamCompletion(chatRequest);

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
        prompt = "";
        chatMessages = new List<ChatMessage>();
        TotalTokens = 0;
        toolStripStatusLabelTokens.Text = $"Tokens: {TotalTokens}";
        //TotalProcessingTime = TimeSpan.Zero;
        //toolStripStatusLabelProcessingTime.Text = $". Processing time: {TotalProcessingTime}";
        chatTextMessasges.Clear();
        textResult.Text = chatTextMessasges.ToString();
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
        chatMessages.Add(new ChatMessage
        {
            Prompt = prompt,
            Role = "user",
            Tokens = result.Usage.PromptTokens
        });
        chatMessages.Add(new ChatMessage
        {
            Prompt = result.FirstChoice.Message,
            Role = "assistant",
            Tokens = result.Usage.CompletionTokens
        });

        TotalTokens += result.Usage.TotalTokens;
        //TotalProcessingTime += result.ProcessingTime;

        chatTextMessasges.Append($"\r\n");
        chatTextMessasges.Append($">> User:\r\n");
        chatTextMessasges.Append($"{prompt}\r\n");
        chatTextMessasges.Append($"(Tokens: {result.Usage.PromptTokens})\r\n");
        chatTextMessasges.Append($"\r\n");
        chatTextMessasges.Append($">> Assistant:\r\n");
        chatTextMessasges.Append(result.FirstChoice.Message.ToString().Replace("\n", "\r\n"));
        chatTextMessasges.Append($"\r\n");
        chatTextMessasges.Append($"(Tokens: {result.Usage.CompletionTokens} tokens. Processing time: {result.ProcessingTime})\r\n");
        chatTextMessasges.Append($"\r\n");
        chatTextMessasges.Append($"\r\n");
        textResult.Text = chatTextMessasges.ToString();
        textResult.SelectionStart = textResult.Text.Length;
        textResult.ScrollToCaret();
        textPrompt.Text = "";
        toolStripStatusLabelTokens.Text = $"Tokens: {TotalTokens}";
        //toolStripStatusLabelProcessingTime.Text = $". Processing time: {TotalProcessingTime}";    
    }

    private async Task GoStreamCompletion(ChatRequest chatRequest)
    {        
        textResult.Text += $">> User:\r\n{prompt}\r\n\r\n";
        textResult.Text += $">> Assistant:\r\n";

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
            textResult.Text += result.FirstChoice.ToString()?.Replace("\n", "\r\n");
            textResult.SelectionStart = textResult.Text.Length;
            textResult.ScrollToCaret();
            textResult.Update();
        });

        textResult.Text += $"\r\n\r\n";
        textPrompt.Text = "";        
    }

}
