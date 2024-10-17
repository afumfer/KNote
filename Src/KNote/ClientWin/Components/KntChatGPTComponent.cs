﻿using KNote.ClientWin.Core;
using KNote.Model;
using OpenAI.Chat;
using System.ClientModel;
using System.Diagnostics;
using System.Text;

namespace KNote.ClientWin.Components;

public class KntChatGPTComponent : ComponentBase
{
    #region Private fields

    private ChatClient _openAIClient;
    
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

    public bool AutoCloseComponentOnViewExit { get; set; } = false;

    public bool AutoSaveChatMessagesOnViewExit { get; set; } = false;

    public string Tag { get; set; } = "KntChatGPTComponent v 0.1";

    #endregion

    #region Constructor

    public KntChatGPTComponent(Store store) : base(store)
    {
        ComponentName = "KntChatGPT Component";        
    }

    #endregion

    #region Events

    public event EventHandler<ComponentEventArgs<string>> StreamToken;

    #endregion 

    #region Protected methods 

    protected override Result<EComponentResult> OnInitialized()
    {
        try
        {
            var organization = Store.AppConfig.ChatGPTOrganization;
            var apiKey = Store.AppConfig.ChatGPTApiKey;

            if (string.IsNullOrEmpty(organization) || string.IsNullOrEmpty(apiKey))
            {
                var message = "It does not have an ApiKey or an IdOrganización of the OpenAI API defined. You must configure these values (ChatGPTApiKey and ChatGPTOrganization) in the program settings.";
                throw new Exception(message);
            }

            _openAIClient = new(model: "gpt-4o-mini", apiKey ?? "--");            
            
            RestartChatGPT();

            return new Result<EComponentResult>(EComponentResult.Executed);
        }
        catch (Exception ex)
        {
            var res = new Result<EComponentResult>(EComponentResult.Error);
            var resMessage = $"OnInitialized KntChatGPTComponent error: {ex.Message}";
            res.AddErrorMessage(resMessage);
            ChatGPTView.ShowInfo(resMessage);
            return res;
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

    public void ShowChatGPTView(bool autoCloseComponentOnViewExit, bool autoSaveChatMessagesOnViewExit)
    {
        AutoCloseComponentOnViewExit = autoCloseComponentOnViewExit;
        AutoSaveChatMessagesOnViewExit = autoSaveChatMessagesOnViewExit;
        ChatGPTView.ShowView();
    }

    // For use in KntScript
    public void ShowChatGPTView()
    {
        if(ComponentState == EComponentState.Started)
        {
            ChatGPTView.ShowView();
        }
        else
        {
            ChatGPTView.ShowInfo("KntChatGPT component is no started.");
        }
    }

    #endregion

    #region Public Methods

    public void RestartChatGPT()
    {
        _prompt = "";
        _result = "";
                
        _chatMessages.Clear();
        _chatMessages.Add(new SystemChatMessage("Eres una asistente útil."));

        _chatTextMessasges.Clear();
        _totalTokens = 0;
        _totalProcessingTime = TimeSpan.Zero;
    }

    public async Task GetCompletionAsync(string prompt)
    {       
        Stopwatch stopwatch = new();

        stopwatch.Start();

        _chatMessages.Add(new UserChatMessage(prompt));

        ChatCompletion completion = await _openAIClient.CompleteChatAsync(_chatMessages);
     
        _chatMessages.Add(new AssistantChatMessage(completion.Content[0].Text));

        _prompt = prompt;
        _result = completion.Content[0].Text.Replace("\n", "\r\n");        
        _totalTokens += completion.Usage.TotalTokenCount;        
        _totalProcessingTime += stopwatch.Elapsed;
        
        _chatTextMessasges.Append($"\r\n");
        _chatTextMessasges.Append($"**User:** \r\n");
        _chatTextMessasges.Append($"{prompt}\r\n");        
        _chatTextMessasges.Append($"\r\n");
        _chatTextMessasges.Append($"**Assistant:** \r\n");
        _chatTextMessasges.Append(_result);
        _chatTextMessasges.Append($"\r\n\r\n\r\n");
        _chatTextMessasges.Append($"(Tokens: {completion.Usage.InputTokenCount} tokens.\r\n");
        _chatTextMessasges.Append($"(Tokens: {completion.Usage.OutputTokenCount} tokens.\r\n");
        _chatTextMessasges.Append($"(Tokens: {completion.Usage.TotalTokenCount} tokens.\r\n");
        _chatTextMessasges.Append($"(Processing time: {stopwatch.Elapsed})\r\n");
        _chatTextMessasges.Append($"\r\n");
        _chatTextMessasges.Append($"\r\n");
    }

    // --------------------------------------------------------------------------
    // Warning: this method can cause a deadlock in single-threaded environments
    // (for example, Windows Forms or WPF applications) or ASP.NET applications.
    // It is recommended to use the asynchronous version of this method.
    // Use only in KntScript
    public void GetCompletion(string prompt)
    {
        Task.Run(() => GetCompletionAsync(prompt)).Wait();
    }
    // --------------------------------------------------------------------------
   
    public async Task StreamCompletionAsync(string prompt)
    {
        StringBuilder resAssistant = new();        
        Stopwatch stopwatch = new();

        stopwatch.Start();

        var intro = $"**User:** \r\n{prompt}\r\n\r\n**Assistant:** \r\n";
        _chatTextMessasges.Append(intro);
        StreamToken?.Invoke(this, new ComponentEventArgs<string>(intro));

        _chatMessages.Add(new UserChatMessage(prompt));

        AsyncCollectionResult<StreamingChatCompletionUpdate> updates
                    = _openAIClient.CompleteChatStreamingAsync(_chatMessages);
        await foreach (StreamingChatCompletionUpdate update in updates)
        {
            foreach (ChatMessageContentPart updatePart in update.ContentUpdate)
            {
                var res = updatePart.Text?.Replace("\n", "\r\n");                
                resAssistant.Append(res);
                StreamToken?.Invoke(this, new ComponentEventArgs<string>(res));
            }
        }

        stopwatch.Stop();

        _chatMessages.Add(new AssistantChatMessage(resAssistant.ToString()));        
        _prompt = prompt;
        _result = resAssistant.ToString();
        _totalTokens += (prompt.Length + resAssistant.Length) / 4;    // TODO: hack, refactor this
        _totalProcessingTime += stopwatch.Elapsed;         
        _chatTextMessasges.Append(resAssistant.ToString());
        _chatTextMessasges.Append($"\r\n\r\n");

        StreamToken?.Invoke(this, new ComponentEventArgs<string>($"\r\n\r\n"));
    }

    #endregion
}
