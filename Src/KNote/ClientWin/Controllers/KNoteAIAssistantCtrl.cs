using KNote.Ai;
using KNote.ClientWin.Core;
using KNote.ClientWin.Views;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using KntScript;
using Microsoft.Extensions.AI;
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

    // Whether each answer saves the conversation as an AI session of the user (IKntService.AiSessions, in
    // ServiceRef - the repository the tools work on), which can be resumed later here or in the Web assistant.
    // On when the assistant is opened from the menu (ShowAIAssistantView(..., persistSession: true)); a script
    // driving the ctrl doesn't create sessions unless it turns this on.
    public bool PersistSession { get; set; } = false;

    // The session being held: NoteId is Guid.Empty until its first answer is saved.
    private AiChatSessionDto _session = new();
    public Guid SessionNoteId => _session.NoteId;
    public string SessionTopic => _session.Topic;

    // The last save of the session failed: it is saved again before leaving it.
    public bool SessionPendingSave { get; private set; }

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

    public void ShowAIAssistantView(bool autoCloseCtrlOnViewExit, bool persistSession)
    {
        // Refused by Run() (CheckPreconditions): already finalized, nothing to show.
        if (!PreconditionsMet)
            return;

        AutoCloseCtrlOnViewExit = autoCloseCtrlOnViewExit;
        PersistSession = persistSession;
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

        _session = new AiChatSessionDto();
        SessionPendingSave = false;
    }

    public async Task GetCompletionAsync(string prompt)
    {
        // Streamed and put together (RunTurnAsync) rather than one GetResponseAsync call: the answer then comes
        // in as it is written, so a long one can't run into the HTTP timeout of a single response (10 minutes in
        // the Anthropic SDK). It is still shown all at once.
        var turn = await RunTurnAsync(prompt, null);
        turn.Answer = turn.Answer.Replace("\n", "\r\n");
        AddTurn(turn);

        _chatTextMessages.Append($"\r\n");
        _chatTextMessages.Append($"**User:** \r\n");
        _chatTextMessages.Append($"{prompt}\r\n");
        _chatTextMessages.Append($"\r\n");
        _chatTextMessages.Append($"**Assistant:** \r\n");
        _chatTextMessages.Append(_result);
        _chatTextMessages.Append($"\r\n\r\n\r\n");
        _chatTextMessages.Append($"(Tokens: {turn.InputTokens ?? 0} tokens.\r\n");
        _chatTextMessages.Append($"(Tokens: {turn.OutputTokens ?? 0} tokens.\r\n");
        _chatTextMessages.Append($"(Tokens: {turn.TotalTokens} tokens.\r\n");
        _chatTextMessages.Append($"(Processing time: {turn.ProcessingTime})\r\n");
        _chatTextMessages.Append($"\r\n");
        _chatTextMessages.Append($"\r\n");

        await SaveAnsweredTurnAsync();
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

        var intro = $"**User:** \r\n{prompt}\r\n\r\n**Assistant:** \r\n";
        _chatTextMessages.Append(intro);
        StreamToken?.Invoke(this, new ControllerEventArgs<string>(intro));

        AiChatTurnDto turn;
        try
        {
            turn = await RunTurnAsync(prompt, text =>
            {
                var res = text.Replace("\n", "\r\n");
                resAssistant.Append(res);
                StreamToken?.Invoke(this, new ControllerEventArgs<string>(res));
            });
        }
        catch
        {
            // Roll back the transcript's dangling intro, so a retry doesn't pile up orphaned turns. Whatever
            // partial text already reached the view via StreamToken is left as-is; the turn itself (resent to the
            // provider, and persisted) is only added once answered.
            _chatTextMessages.Length -= intro.Length;
            throw;
        }

        turn.Answer = resAssistant.ToString();
        AddTurn(turn);
        _chatTextMessages.Append(resAssistant.ToString());
        _chatTextMessages.Append($"\r\n\r\n");

        StreamToken?.Invoke(this, new ControllerEventArgs<string>($"\r\n\r\n"));

        await SaveAnsweredTurnAsync();
    }

    // One turn, the same as the Web assistant (KNote.Ai's AiChatTurnStreamer): the system prompt and the turns so
    // far are sent with the prompt, and the answer comes back piece by piece (onText) and then whole, with its
    // usage. Nothing is added to the conversation here: an unanswered prompt (the provider failed) leaves no
    // orphaned user message behind to be resent with the next one.
    private async Task<AiChatTurnDto> RunTurnAsync(string prompt, Action<string> onText)
    {
        var history = _chatTurns.Select(t => t.ToDto()).ToList();
        AiChatTurnDto completed = null;

        await foreach (var e in AiChatTurnStreamer.StreamAsync(_chatClient, RootSystemChat, history, prompt))
        {
            if (e.Type == AiChatStreamEventTypes.Delta)
                onText?.Invoke(e.Text);
            else if (e.Type == AiChatStreamEventTypes.Completed)
                completed = e.Turn;
        }

        return completed ?? throw new InvalidOperationException("The AI provider ended the answer unexpectedly.");
    }

    // An answered turn joins the conversation: the messages kept for scripts (ChatMessages), the turns of the
    // views and of the session, and the totals.
    private void AddTurn(AiChatTurnDto turn)
    {
        _chatMessages.Add(new ChatMessage(ChatRole.User, turn.Prompt));
        _chatMessages.Add(new ChatMessage(ChatRole.Assistant, turn.Answer));
        _chatTurns.Add(AiChatTurn.FromDto(turn, _currentProviderRef?.Alias));

        _prompt = turn.Prompt;
        _result = turn.Answer;
        _totalTokens += (int)turn.TotalTokens;
        _totalProcessingTime += turn.ProcessingTime;
    }

    #region Sessions

    // The user's sessions in ServiceRef, most recently modified first (empty when they can't be read).
    public async Task<List<AiChatSessionInfoDto>> GetSessionsAsync()
    {
        try
        {
            var res = await ServiceRef.Service.AiSessions.GetUserSessionsAsync();
            if (res.IsValid)
                return res.Entity ?? new List<AiChatSessionInfoDto>();
            AIAssistantView.ShowInfo($"The AI sessions could not be read: {res.ErrorMessage}");
        }
        catch (Exception ex)
        {
            AIAssistantView.ShowInfo($"The AI sessions could not be read: {ex.Message}");
        }
        return new List<AiChatSessionInfoDto>();
    }

    // Saves the session if it has something unsaved (SessionPendingSave). False if that failed.
    public async Task<bool> SaveSessionAsync()
    {
        if (!SessionPendingSave || _chatTurns.Count == 0)
            return true;

        try
        {
            // The session is held with the provider of its last answer (see AiProviderSelection).
            _session.Provider = _currentProviderRef?.Provider;
            _session.Model = _currentProviderRef?.Model;
            _session.Turns = _chatTurns.Select(t => t.ToDto()).ToList();

            var res = await ServiceRef.Service.AiSessions.SaveAsync(_session);
            if (res.IsValid)
            {
                _session.NoteId = res.Entity.NoteId;
                _session.NoteNumber = res.Entity.NoteNumber;
                _session.Topic = res.Entity.Topic;
                _session.CreationDateTime = res.Entity.CreationDateTime;
                _session.ModificationDateTime = res.Entity.ModificationDateTime;
                SessionPendingSave = false;
                return true;
            }
            AIAssistantView.ShowInfo($"The AI session could not be saved: {res.ErrorMessage}");
        }
        catch (Exception ex)
        {
            AIAssistantView.ShowInfo($"The AI session could not be saved: {ex.Message}");
        }
        return false;
    }

    // Before leaving the conversation (a new one, another session, another provider): saved if it has something
    // unsaved; when that fails, the user decides whether to discard it. False to stay in it.
    public async Task<bool> LeaveSessionAsync()
    {
        if (!PersistSession || await SaveSessionAsync())
            return true;

        return AIAssistantView.ShowInfo("The conversation could not be saved. Do you want to discard its last messages?",
            "KNote", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;
    }

    // Starts a new conversation, with the default system prompt. False if the current one is kept.
    public async Task<bool> NewSessionAsync()
    {
        if (!await LeaveSessionAsync())
            return false;

        RootSystemChat = KntConst.DefaultRootSystemChat;
        RestartAIAssistant();
        return true;
    }

    // Resumes a saved session, with its provider and model or, when they are no longer configured, with the
    // preferred one. False if it couldn't be opened (or the current conversation is kept).
    public async Task<bool> OpenSessionAsync(Guid noteId)
    {
        if (noteId == _session.NoteId || !await LeaveSessionAsync())
            return false;

        Result<AiChatSessionDto> res;
        try
        {
            res = await ServiceRef.Service.AiSessions.GetAsync(noteId);
        }
        catch (Exception ex)
        {
            AIAssistantView.ShowInfo($"The AI session could not be opened: {ex.Message}");
            return false;
        }
        if (!res.IsValid)
        {
            AIAssistantView.ShowInfo($"The AI session could not be opened: {res.ErrorMessage}");
            return false;
        }

        var session = res.Entity;
        var providerRef = AiProviderSelection.ForSession(AiProviderRefs, session.Provider, session.Model);
        if (providerRef == null)
        {
            providerRef = GetPreferredProvider();
            if (providerRef == null)
                return false;
            AIAssistantView.ShowInfo($"{session.Provider} {session.Model}, the model of this session, is no longer configured: " +
                $"it continues with {providerRef.Alias}.");
        }

        RootSystemChat = KntConst.DefaultRootSystemChat;
        ApplyProvider(providerRef);

        foreach (var turn in session.Turns)
        {
            AddTurn(turn);
            _chatTextMessages.Append($"**User:** \r\n{turn.Prompt}\r\n\r\n**Assistant:** \r\n{turn.Answer}\r\n\r\n");
        }
        _session = session;
        _session.Turns = new List<AiChatTurnDto>();
        return true;
    }

    // After each answer, when the conversation is persisted.
    private async Task SaveAnsweredTurnAsync()
    {
        if (!PersistSession)
            return;

        SessionPendingSave = true;
        await SaveSessionAsync();
    }

    #endregion

    public async Task<KntAssistantInfo> GetCatalogPrompt()
    {
        if (!await LeaveSessionAsync())
            return null;

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
