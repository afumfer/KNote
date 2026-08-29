using KNote.ClientWin.Core;
using KNote.ClientWin.Views;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KntScript;
using Microsoft.Extensions.AI;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace KNote.ClientWin.Controllers;

// Built on Microsoft.Extensions.AI's IChatClient abstraction (provider-agnostic: OpenAI, Anthropic,
// Ollama - see AiChatClientFactory). Replaces the retired KntChatGPTCtrl (OpenAI-only, built
// directly on the OpenAI.Chat SDK).
public class KNoteAIAssistantCtrl : CtrlBase
{
    #region Private fields

    private IChatClient _chatClient;

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

    public string Tag { get; set; } = "KNoteAIAssistantCtrl v0.1";

    public ServiceRef ServiceRef { get; private set; }

    public string RootSystemChat { get; set; }

    // KNoteAIAssistant plan (Phase 3): the configured provider/model collection and the one
    // currently active. AiProviderRefs is exposed live from AppConfig so the view's picker
    // always reflects whatever is currently in KNoteData.config (Phase 4 will add a maintenance
    // UI for it; for now entries are added by hand to the config file).
    public List<AiProviderRef> AiProviderRefs => Store.AppConfig.AiProviderRefs;

    private AiProviderRef _currentProviderRef;
    public AiProviderRef CurrentProviderRef => _currentProviderRef;

    #endregion

    #region Constructor

    public KNoteAIAssistantCtrl(Store store) : base(store)
    {
        ControllerName = "KNoteAIAssistant Controller";
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
            if (AiProviderRefs.Count == 0)
            {
                var message = "No AI providers are configured yet (AppConfig.AiProviderRefs is empty). " +
                    "Add at least one entry to the <AiProviderRefs> section of KNoteData.config " +
                    "(a maintenance screen for this is planned for a later phase).";
                throw new Exception(message);
            }

            SetProvider(AiProviderRefs[0]);

            return new Result<EControllerResult>(EControllerResult.Executed);
        }
        catch (Exception ex)
        {
            var res = new Result<EControllerResult>(EControllerResult.Error);
            var resMessage = $"OnInitialized KNoteAIAssistantController error: {ex.Message}";
            res.AddErrorMessage(resMessage);
            AIAssistantView.ShowInfo(resMessage);
            return res;
        }
    }

    #endregion

    #region Views

    IViewBase _aiAssistantView;

    protected IViewBase AIAssistantView
    {
        get
        {
            if (_aiAssistantView == null)
                _aiAssistantView = Store.FactoryViews.Registry.Resolve<KNoteAIAssistantCtrl, IViewBase>(this);
            return _aiAssistantView;
        }
    }

    public void ShowAIAssistantView(bool autoCloseCtrlOnViewExit, bool autoSaveChatMessagesOnViewExit)
    {
        AutoCloseCtrlOnViewExit = autoCloseCtrlOnViewExit;
        AutoSaveChatMessagesOnViewExit = autoSaveChatMessagesOnViewExit;
        AIAssistantView.ShowView();
    }

    // For use in KntScript
    public void ShowAIAssistantView()
    {
        if(ControllerState == EControllerState.Started)
        {
            AIAssistantView.ShowView();
        }
        else
        {
            AIAssistantView.ShowInfo("KNoteAIAssistant controller is no started.");
        }
    }

    #endregion

    #region Public Methods

    // Switching provider mid-session invalidates the in-flight conversation (a different
    // provider/model can't continue the same message history), so this always resets it.
    // The view is responsible for confirming with the user first if there is one in progress.
    public void SetProvider(AiProviderRef providerRef)
    {
        if (providerRef is null)
            throw new ArgumentNullException(nameof(providerRef));

        _currentProviderRef = providerRef;
        _chatClient = AiChatClientFactory.Create(providerRef, ServiceRef);
        RestartAIAssistant();
    }

    public void RestartAIAssistant()
    {
        _prompt = "";
        _result = "";

        _chatMessages.Clear();
        _chatMessages.Add(new ChatMessage(ChatRole.System, RootSystemChat));

        _chatTextMessasges.Clear();
        _totalTokens = 0;
        _totalProcessingTime = TimeSpan.Zero;
    }

    public async Task GetCompletionAsync(string prompt)
    {
        Stopwatch stopwatch = new();

        stopwatch.Start();

        _chatMessages.Add(new ChatMessage(ChatRole.User, prompt));

        ChatResponse response;
        try
        {
            response = await _chatClient.GetResponseAsync(_chatMessages);
        }
        catch
        {
            // Roll back the unanswered turn so a retry (or a provider/model switch) doesn't send
            // an orphaned user message with no matching assistant reply.
            _chatMessages.RemoveAt(_chatMessages.Count - 1);
            throw;
        }

        _chatMessages.Add(new ChatMessage(ChatRole.Assistant, response.Text));

        _prompt = prompt;
        _result = response.Text.Replace("\n", "\r\n");
        _totalTokens += (int)(response.Usage?.TotalTokenCount ?? 0);
        _totalProcessingTime += stopwatch.Elapsed;

        _chatTextMessasges.Append($"\r\n");
        _chatTextMessasges.Append($"**User:** \r\n");
        _chatTextMessasges.Append($"{prompt}\r\n");
        _chatTextMessasges.Append($"\r\n");
        _chatTextMessasges.Append($"**Assistant:** \r\n");
        _chatTextMessasges.Append(_result);
        _chatTextMessasges.Append($"\r\n\r\n\r\n");
        _chatTextMessasges.Append($"(Tokens: {response.Usage?.InputTokenCount ?? 0} tokens.\r\n");
        _chatTextMessasges.Append($"(Tokens: {response.Usage?.OutputTokenCount ?? 0} tokens.\r\n");
        _chatTextMessasges.Append($"(Tokens: {response.Usage?.TotalTokenCount ?? 0} tokens.\r\n");
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

        _chatMessages.Add(new ChatMessage(ChatRole.User, prompt));

        try
        {
            await foreach (ChatResponseUpdate update in _chatClient.GetStreamingResponseAsync(_chatMessages))
            {
                var res = update.Text?.Replace("\n", "\r\n");
                if (string.IsNullOrEmpty(res))
                    continue;
                resAssistant.Append(res);
                StreamToken?.Invoke(this, new ControllerEventArgs<string>(res));
            }
        }
        catch
        {
            // Roll back the unanswered turn - both the message sent to the provider and the
            // transcript's dangling intro - so a retry doesn't pile up orphaned turns. Whatever
            // partial text already reached the view via StreamToken is left as-is; only the
            // canonical history (resent to the provider, and persisted on save) is rolled back.
            _chatMessages.RemoveAt(_chatMessages.Count - 1);
            _chatTextMessasges.Length -= intro.Length;
            throw;
        }

        stopwatch.Stop();

        _chatMessages.Add(new ChatMessage(ChatRole.Assistant, resAssistant.ToString()));
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

        RestartAIAssistant();

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
                _aiAssistantView.ShowInfo(err);
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
        // kntScript.AddVar("_knote", Model);

        try
        {
            kntScript.Run(assistantScript);
        }
        catch (Exception ex)
        {
            _aiAssistantView.ShowInfo($"The assistant cannot be run, {ex.Message}");
        }
    }

    #endregion
}
