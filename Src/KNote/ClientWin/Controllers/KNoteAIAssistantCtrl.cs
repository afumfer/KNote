using KNote.Ai;
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
// Ollama - see KNote.Ai's AiChatClientFactory). Replaces the retired KntChatGPTCtrl (OpenAI-only, built
// directly on the OpenAI.Chat SDK).
[KntAuthorize(EnumRoles.Staff, AuthorizationScope.Application)]
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

    private StringBuilder _chatTextMessages = new StringBuilder();
    public StringBuilder ChatTextMessages
    {
        get { return _chatTextMessages; }
    }

    // The same conversation turn by turn (kept in step with ChatMessages/ChatTextMessages), for the
    // views that show each question and answer on its own.
    private readonly List<AiChatTurn> _chatTurns = new List<AiChatTurn>();
    public IReadOnlyList<AiChatTurn> ChatTurns
    {
        get { return _chatTurns; }
    }

    // The answer received so far by the running StreamCompletionAsync (or by the last one, until the next
    // starts), so a view can repaint it while it streams in.
    private readonly StringBuilder _streamingResult = new StringBuilder();
    public string StreamingResult
    {
        get { return _streamingResult.ToString(); }
    }

    // Former, misspelled name: scripts saved in users' notes may still use it.
    [Obsolete("Misspelled name kept only so existing scripts keep working; use ChatTextMessages.")]
    public StringBuilder ChatTextMessasges => ChatTextMessages;

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

    // Default response mode the view should preselect (Get Stream / Get Completion radios) when
    // it's shown - Stream matches the pre-existing manual-use default (KNoteManagement menu). A
    // caller that drives the ctrl itself before ever showing the view (e.g. the "ln" script engine,
    // which always calls GetCompletionAsync) sets this to Completion first so the view reflects how
    // the already-obtained result was actually produced, without changing the default for normal use.
    public EAiResponseMode ResponseMode { get; set; } = EAiResponseMode.Stream;

    // Whether the view shows the conversation as its Markdown source instead of as a chat (the default).
    // Remembered in KNoteState.config (State.Session.AiAssistantMarkdownView), like the last provider.
    public bool MarkdownResultView
    {
        get { return Store.State.Session.AiAssistantMarkdownView; }
        set
        {
            if (Store.State.Session.AiAssistantMarkdownView == value)
                return;

            Store.State.Session.AiAssistantMarkdownView = value;
            SaveState();
        }
    }

    // Whether the view shows the usage of each answer (tokens, processing time) below it. Remembered in
    // KNoteState.config (State.Session.AiAssistantShowModelInfo).
    public bool ShowModelInfo
    {
        get { return Store.State.Session.AiAssistantShowModelInfo; }
        set
        {
            if (Store.State.Session.AiAssistantShowModelInfo == value)
                return;

            Store.State.Session.AiAssistantShowModelInfo = value;
            SaveState();
        }
    }

    // KNoteAIAssistant plan (Phase 3): the configured provider/model collection and the one
    // currently active. AiProviderRefs is exposed live from the settings so the view's picker
    // always reflects whatever is currently in KNoteData.config (Phase 4 will add a maintenance
    // UI for it; for now entries are added by hand to the config file).
    public List<AiProviderRef> AiProviderRefs => Store.Settings.Ai.Providers;

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
                var message = "No AI providers are configured yet (Settings.Ai.Providers is empty). " +
                    "Add at least one entry to the <AiProviderRefs> section of KNoteData.config " +
                    "(a maintenance screen for this is planned for a later phase).";
                throw new Exception(message);
            }

            // The provider the user picked last (State.Session.LastAiProviderAlias), else the first one.
            ApplyProvider(GetPreferredProvider());

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
        // Refused by Run() (CheckPreconditions): already finalized, nothing to show.
        if (!PreconditionsMet)
            return;

        AutoCloseCtrlOnViewExit = autoCloseCtrlOnViewExit;
        AutoSaveChatMessagesOnViewExit = autoSaveChatMessagesOnViewExit;
        AIAssistantView.ShowView();
    }

    // For use in KntScript
    public void ShowAIAssistantView()
    {
        if (!PreconditionsMet)
            return;

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
    // The choice is remembered in KNoteState.config (State.Session.LastAiProviderAlias) so the next session
    // - of this assistant or of any other component built on it, like KntServerCOMCtrl - starts with it.
    public void SetProvider(AiProviderRef providerRef)
    {
        ApplyProvider(providerRef);
        SaveLastProviderAlias(providerRef.Alias);
    }

    // The provider to start with: the last one chosen by the user if it still exists, else the first
    // configured one, else null (no providers configured).
    public AiProviderRef GetPreferredProvider()
    {
        var lastAlias = Store.State.Session.LastAiProviderAlias;
        var preferred = string.IsNullOrEmpty(lastAlias) ? null :
            AiProviderRefs.FirstOrDefault(p => string.Equals(p.Alias, lastAlias, StringComparison.OrdinalIgnoreCase));

        return preferred ?? AiProviderRefs.FirstOrDefault();
    }

    private void ApplyProvider(AiProviderRef providerRef)
    {
        if (providerRef is null)
            throw new ArgumentNullException(nameof(providerRef));

        _currentProviderRef = providerRef;
        var tools = new KNoteAiTools(ServiceRef.Service, new KNoteAiToolsHost(Store));
        _chatClient = AiChatClientFactory.Create(providerRef, tools.GetTools());
        RestartAIAssistant();
    }

    private void SaveLastProviderAlias(string alias)
    {
        if (Store.State.Session.LastAiProviderAlias == alias)
            return;

        Store.State.Session.LastAiProviderAlias = alias;
        SaveState();
    }

    private void SaveState()
    {
        try
        {
            Store.SaveConfig();
        }
        catch (Exception)
        {
            // Remembering a choice is a convenience: failing to write the config file must not
            // undo a provider switch (or a view change) that already succeeded.
        }
    }

    // Test seam (ClientWin.Tests): sets the chat client directly, bypassing KNote.Ai's AiChatClientFactory, so
    // unit tests can exercise GetCompletionAsync/StreamCompletionAsync/RestartAIAssistant against a
    // fake IChatClient with no real network call or API key.
    internal void SetChatClientForTesting(IChatClient chatClient, AiProviderRef providerRef = null)
    {
        _chatClient = chatClient;
        _currentProviderRef = providerRef;
    }

    public void RestartAIAssistant()
    {
        _prompt = "";
        _result = "";

        _chatMessages.Clear();
        _chatMessages.Add(new ChatMessage(ChatRole.System, RootSystemChat));

        _chatTextMessages.Clear();
        _chatTurns.Clear();
        _streamingResult.Clear();
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
            // Streamed and put together here rather than one GetResponseAsync call: the answer then comes in
            // as it is written, so a long one can't run into the HTTP timeout of a single response (10 minutes
            // in the Anthropic SDK). It is still shown all at once.
            response = await _chatClient.GetStreamingResponseAsync(_chatMessages).ToChatResponseAsync();
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

        _chatTurns.Add(new AiChatTurn(prompt, _result, _currentProviderRef?.Alias, stopwatch.Elapsed)
        {
            InputTokens = response.Usage?.InputTokenCount,
            OutputTokens = response.Usage?.OutputTokenCount,
            TotalTokens = response.Usage?.TotalTokenCount ?? 0,
            Truncated = response.FinishReason == ChatFinishReason.Length
        });

        _chatTextMessages.Append($"\r\n");
        _chatTextMessages.Append($"**User:** \r\n");
        _chatTextMessages.Append($"{prompt}\r\n");
        _chatTextMessages.Append($"\r\n");
        _chatTextMessages.Append($"**Assistant:** \r\n");
        _chatTextMessages.Append(_result);
        _chatTextMessages.Append($"\r\n\r\n\r\n");
        _chatTextMessages.Append($"(Tokens: {response.Usage?.InputTokenCount ?? 0} tokens.\r\n");
        _chatTextMessages.Append($"(Tokens: {response.Usage?.OutputTokenCount ?? 0} tokens.\r\n");
        _chatTextMessages.Append($"(Tokens: {response.Usage?.TotalTokenCount ?? 0} tokens.\r\n");
        _chatTextMessages.Append($"(Processing time: {stopwatch.Elapsed})\r\n");
        _chatTextMessages.Append($"\r\n");
        _chatTextMessages.Append($"\r\n");
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
        // Cleared before the first await, so a view repainting StreamingResult never shows the previous answer.
        StringBuilder resAssistant = _streamingResult;
        resAssistant.Clear();
        Stopwatch stopwatch = new();
        ChatFinishReason? finishReason = null;

        stopwatch.Start();

        var intro = $"**User:** \r\n{prompt}\r\n\r\n**Assistant:** \r\n";
        _chatTextMessages.Append(intro);
        StreamToken?.Invoke(this, new ControllerEventArgs<string>(intro));

        _chatMessages.Add(new ChatMessage(ChatRole.User, prompt));

        try
        {
            await foreach (ChatResponseUpdate update in _chatClient.GetStreamingResponseAsync(_chatMessages))
            {
                // The last one given: the intermediate steps of a tool call carry their own.
                finishReason = update.FinishReason ?? finishReason;
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
            _chatTextMessages.Length -= intro.Length;
            throw;
        }

        stopwatch.Stop();

        _chatMessages.Add(new ChatMessage(ChatRole.Assistant, resAssistant.ToString()));
        _prompt = prompt;
        _result = resAssistant.ToString();
        var estimatedTokens = (prompt.Length + resAssistant.Length) / 4;    // TODO: hack, refactor this
        _totalTokens += estimatedTokens;
        _totalProcessingTime += stopwatch.Elapsed;
        _chatTurns.Add(new AiChatTurn(prompt, _result, _currentProviderRef?.Alias, stopwatch.Elapsed)
        {
            TotalTokens = estimatedTokens,
            TokensEstimated = true,
            Truncated = finishReason == ChatFinishReason.Length
        });
        _chatTextMessages.Append(resAssistant.ToString());
        _chatTextMessages.Append($"\r\n\r\n");

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
        // A KNote assistant is a KntScript script: running it takes the role any script does.
        if (!Store.CheckCanRunScripts())
            return;

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

public enum EAiResponseMode
{
    Stream,
    Completion
}
