﻿using KNote.ClientWin.Core;
using KNote.Model;
using Microsoft.Identity.Client;
using OpenAI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace KNote.ClientWin.Components;

public class KntChatGPTComponent : ComponentBase
{
    #region Private fields

    private OpenAIClient _openAIClient;
    
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

            _openAIClient = new OpenAIClient(new OpenAIAuthentication(apiKey, organization));

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
        
        _result = result.FirstChoice.Message.ToString().Replace("\n", "\r\n");
        _totalTokens += result.Usage.TotalTokens;
        _totalProcessingTime += result.ProcessingTime;
        
        _chatTextMessasges.Append($"\r\n");
        _chatTextMessasges.Append($"**User:** \r\n");
        _chatTextMessasges.Append($"{_prompt}\r\n");
        _chatTextMessasges.Append($"(Tokens: {result.Usage.PromptTokens})\r\n");
        _chatTextMessasges.Append($"\r\n");
        _chatTextMessasges.Append($"**Assistant:** \r\n");
        _chatTextMessasges.Append(result.FirstChoice.Message.ToString().Replace("\n", "\r\n"));
        _chatTextMessasges.Append($"\r\n");
        _chatTextMessasges.Append($"(Tokens: {result.Usage.CompletionTokens} tokens. Processing time: {result.ProcessingTime})\r\n");
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

    public event EventHandler<ComponentEventArgs<string>> StreamToken;
    public async Task StreamCompletionAsync(string prompt)
    {
        StringBuilder tempResult = new();        
        Stopwatch stopwatch = new();

        stopwatch.Start();

        _chatTextMessasges.Append($"**User:** \r\n{prompt}\r\n\r\n");
        StreamToken?.Invoke(this, new ComponentEventArgs<string>($"**User:** \r\n{prompt}\r\n\r\n"));
        _chatTextMessasges.Append($"**Assistant:** \r\n");
        StreamToken?.Invoke(this, new ComponentEventArgs<string>($"**Assistant:** \r\n"));        

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
        StreamToken?.Invoke(this, new ComponentEventArgs<string>($"\r\n\r\n"));
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
