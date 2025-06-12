using KNote.ClientWin.Core;
using KNote.ClientWin.Views;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KntScript;
using Microsoft.IdentityModel.Tokens;
using OpenAI.Chat;
using System.ClientModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace KNote.ClientWin.Controllers;

public class KntChatGPTCtrl : CtrlBase
{
    #region Private fields

    private ChatClient _chatClient;
    
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

    public bool AutoCloseCtrlOnViewExit { get; set; } = false;

    public bool AutoSaveChatMessagesOnViewExit { get; set; } = false;

    public string Tag { get; set; } = "KntChatGPTCtrl v0.2";

    public ServiceRef ServiceRef { get; private set; }

    public string RootSystemChat { get; set; }

    #endregion

    #region Constructor

    public KntChatGPTCtrl(Store store) : base(store)
    {
        ControllerName = "KntChatGPT Controller";
        ServiceRef = store.GetActiveOrDefaultServiceRef();        
        RootSystemChat = KntConst.DefaultRootSystemChat;
    }

    #endregion

    #region Events

    public event EventHandler<ControllerEventArgs<string>> StreamToken;

    #endregion 

    #region Protected methods 

    protected override Result<EControllerResult> OnInitialized()
    {
        try
        {            
            var apiKey = Store.AppConfig.ChatGPTApiKey;

            if (string.IsNullOrEmpty(apiKey))
                apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

            if (string.IsNullOrEmpty(apiKey))
            {                
                var message = "It does not have an ApiKey of the OpenAI API defined. You must configure these values (ChatGPTApiKey) in the program settings.";
                throw new Exception(message);
            }
           
            //_chatClient = new(model: "gpt-4o-mini", apiKey ?? "--");
            _chatClient = new(model: Store.AppConfig.ChatGPTDefaultModel, apiKey ?? "--");

            RestartChatGPT();

            return new Result<EControllerResult>(EControllerResult.Executed);
        }
        catch (Exception ex)
        {
            var res = new Result<EControllerResult>(EControllerResult.Error);
            var resMessage = $"OnInitialized KntChatGPTController error: {ex.Message}";
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

    public void ShowChatGPTView(bool autoCloseCtrlOnViewExit, bool autoSaveChatMessagesOnViewExit)
    {
        AutoCloseCtrlOnViewExit = autoCloseCtrlOnViewExit;
        AutoSaveChatMessagesOnViewExit = autoSaveChatMessagesOnViewExit;
        ChatGPTView.ShowView();
    }

    // For use in KntScript
    public void ShowChatGPTView()
    {
        if(ControllerState == EControllerState.Started)
        {
            ChatGPTView.ShowView();
        }
        else
        {
            ChatGPTView.ShowInfo("KntChatGPT controller is no started.");
        }
    }

    #endregion

    #region Public Methods

    public void RestartChatGPT()
    {
        _prompt = "";
        _result = "";
                
        _chatMessages.Clear();
        _chatMessages.Add(new SystemChatMessage(RootSystemChat));

        _chatTextMessasges.Clear();
        _totalTokens = 0;
        _totalProcessingTime = TimeSpan.Zero;
    }

    public async Task GetCompletionAsync(string prompt)
    {       
        Stopwatch stopwatch = new();

        stopwatch.Start();

        _chatMessages.Add(new UserChatMessage(prompt));

        ChatCompletion completion = await _chatClient.CompleteChatAsync(_chatMessages);
     
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
        StreamToken?.Invoke(this, new ControllerEventArgs<string>(intro));

        _chatMessages.Add(new UserChatMessage(prompt));

        AsyncCollectionResult<StreamingChatCompletionUpdate> updates
                    = _chatClient.CompleteChatStreamingAsync(_chatMessages);
        await foreach (StreamingChatCompletionUpdate update in updates)
        {
            foreach (ChatMessageContentPart updatePart in update.ContentUpdate)
            {
                var res = updatePart.Text?.Replace("\n", "\r\n");                
                resAssistant.Append(res);
                StreamToken?.Invoke(this, new ControllerEventArgs<string>(res));
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

        StreamToken?.Invoke(this, new ControllerEventArgs<string>($"\r\n\r\n"));
    }

    public async Task<KntAssistantInfo> GetCatalogPrompt()
    {
        var assistantServiceRef = Store.GetAssistantServiceRef() ?? ServiceRef;
        var catalogItem = await Store.GetCatalogItem(assistantServiceRef, KntConst.PromptTag, "Select prompt");

        if (string.IsNullOrEmpty(catalogItem?.Description))
            return null;

        var chatTemplate = new KntAssistantInfo();

        try 
        { 
            chatTemplate = JsonSerializer.Deserialize<KntAssistantInfo>(catalogItem.Description); 
        }
        catch 
        { 
            chatTemplate.User = catalogItem.Description; 
        }        
        chatTemplate.Name = catalogItem.Topic;
        if (!string.IsNullOrEmpty(chatTemplate.System))
            RootSystemChat = chatTemplate.System;
        else
            RootSystemChat = KntConst.DefaultRootSystemChat;

        RestartChatGPT();

        return chatTemplate;
    }

    public async Task ExecChatAssistant()
    {
        var assistantServiceRef = Store.GetAssistantServiceRef() ?? ServiceRef;
        var catalogItem = await Store.GetCatalogItem(assistantServiceRef, KntConst.AssistantTag, "Select KNote assistant");
        if (catalogItem == null)
            return;  // Action cancelled.

        var kntScript = new KntSEngine(new InOutDeviceForm(), new KNoteScriptLibrary(Store));
        var assistantInfo = new KntAssistantInfo();
        var assistantScript = "";

        try
        {
            NoteDto codeInfo;
            string err = "";

            assistantInfo = JsonSerializer.Deserialize<KntAssistantInfo>(catalogItem.Description);

            if (assistantInfo.AssistantScriptNumber != 0)
            {
                codeInfo = (await assistantServiceRef.Service.Notes.GetAsync(assistantInfo.AssistantScriptNumber)).Entity;
                if (codeInfo == null)
                    err = "The assistant cannot be run, the assistant script cannot be found (by identification number).";
            }
            else
            {
                codeInfo = (await assistantServiceRef.Service.Notes.GetAsync(catalogItem.NoteId)).Entity;
                if (codeInfo == null)
                    err = "The assistant cannot be run, the assistant script cannot be found (by identification guid).";
            }

            if (string.IsNullOrEmpty(err))
                assistantScript = codeInfo.Script;
            else
            {
                _chatGPTView.ShowInfo(err);
                return;
            }
        }
        catch
        {
            assistantInfo.User = catalogItem.Description;
        }



        // Inject variables for KntScript
        if (!string.IsNullOrEmpty(assistantInfo.System))
            kntScript.AddVar("_rootSystemChat", assistantInfo.System);
        else
            kntScript.AddVar("_rootSystemChat", KntConst.DefaultRootSystemChat);
        if (string.IsNullOrEmpty(assistantInfo.User))
            assistantInfo.User = "";
        kntScript.AddVar("_promptChat", assistantInfo.User);
        ////////////////////kntScript.AddVar("_knote", Model);

        try
        {
            kntScript.Run(assistantScript);
        }
        catch (Exception ex)
        {
            _chatGPTView.ShowInfo($"The assistant cannot be run, {ex.Message}");
        }
    }

    #endregion
}
