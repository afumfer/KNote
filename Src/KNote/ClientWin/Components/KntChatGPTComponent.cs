using KNote.ClientWin.Core;
using KNote.Model;
using Microsoft.Identity.Client;
using OpenAI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNote.ClientWin.Components;

public class KntChatGPTComponent : ComponentBase
{
    #region Private fields

    private OpenAIClient _openAIClient;

    private string _organization = "";
    private string _apiKey = "";

    #endregion

    #region Properties

    private List<ChatMessage> _chatMessages = new List<ChatMessage>();
    public List<ChatMessage> ChatMessages
    {
        get { return _chatMessages; }
    }

    private StringBuilder _chatTextMessasges = new StringBuilder();
    public StringBuilder ChatTextMessasges
    {
        get { return _chatTextMessasges; }
    }

    private string _prompt = "";
    public string Prompt
    {
        get { return _prompt; }
    }

    private string _result = "";
    public string Result
    {
        get { return _result; }
    }

    private int _totalTokens = 0;
    public int TotalTokens
    {
        get { return _totalTokens; }
    }

    private TimeSpan _totalProcessingTime = TimeSpan.Zero;
    public TimeSpan TotalProcessingTime
    {
        get { return _totalProcessingTime; }
    }

    #endregion

    #region Constructor

    public KntChatGPTComponent(Store store) : base(store)
    {
        ComponentName = "KntChatGPT Component";
    }

    #endregion

    #region Protected methods 

    protected override Result<EComponentResult> OnInitialized()
    {
        try
        {
            _organization = Store.AppConfig.ChatGPTOrganization;
            _apiKey = Store.AppConfig.ChatGPTApiKey;

            _openAIClient = new OpenAIClient(new OpenAIAuthentication(_apiKey, _organization));

            return new Result<EComponentResult>(EComponentResult.Executed);
        }
        catch (Exception ex)
        {
            ChatGPTView.ShowInfo($"OnInitialized KntChatGPTComponent error: {ex.Message}");
            return new Result<EComponentResult>(EComponentResult.Error);
        }
    }

    #endregion 

    #region Views

    IViewBase _chatGPTView;

    protected IViewBase ChatGPTView
    {
        get
        {
            if (_chatGPTView == null)
                _chatGPTView = Store.FactoryViews.View(this);
            return _chatGPTView;
        }
    }

    public void ShowChatGPTView()
    {
        ChatGPTView.ShowView();
    }

    #endregion

    #region Public Methods

    public void RestartChatGPT()
    {
        _prompt = "";
        _result = "";
        _chatMessages = new List<ChatMessage>();
        _chatTextMessasges.Clear();
        _totalTokens = 0;
        _totalProcessingTime = TimeSpan.Zero;
    }

    public async Task GetCompletionAsync(string prompt)
    {
        var result = await _openAIClient.ChatEndpoint.GetCompletionAsync(GetChatRequest(prompt));

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
        
        _result = result.FirstChoice.Message;
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
    }

    public event EventHandler<ComponentEventArgs<string>> StreamToken;
    public async Task StreamCompletionAsync(string prompt)
    {
        StringBuilder tempResult = new();
        Stopwatch stopwatch = new();

        stopwatch.Start();

        await _openAIClient.ChatEndpoint.StreamCompletionAsync(GetChatRequest(prompt), result =>
        {
            var res = result.FirstChoice.ToString()?.Replace("\n", "\r\n");
            tempResult.Append(res);
            StreamToken?.Invoke(this, new ComponentEventArgs<string>(res));
        });

        stopwatch.Stop();

        _totalProcessingTime += stopwatch.Elapsed;

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

        _result = tempResult.ToString();
        _totalTokens += (_prompt.Length + tempResult.Length) / 4;    // TODO: hack, refactor this
        
        _chatTextMessasges.Append(tempResult);
        _chatTextMessasges.Append($"\r\n\r\n");
    }

    #endregion

    #region Private Methods

    private ChatRequest GetChatRequest(string prompt)
    {
        _prompt = prompt;

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

}
