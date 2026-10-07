using System.Text;
using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.ClientWin.Utils;
using KntIcons;

namespace KNote.ClientWin.Views;

public partial class KNoteAIAssistantForm : KntForm, IViewBase
{
    #region Private fields

    private readonly KNoteAIAssistantCtrl _ctrl;
    private readonly AiChatWebRenderer _chatView;
    // The menu items with a keyboard shortcut, by its text (AiChatHtml.ShortcutText).
    private readonly Dictionary<string, ToolStripMenuItem> _menuShortcuts;
    private int _countNRres;
    private StringBuilder _sbResult = new StringBuilder();
    private const string ViewCaptionText = "KNote AI Assistant";

    #endregion

    #region Constructor

    public KNoteAIAssistantForm(KNoteAIAssistantCtrl ctrl)
    {
        InitializeComponent();

        buttonSend.SetKntIcon(KntIcon.Send);
        buttonRestart.SetKntIcon(KntIcon.Restart);
        buttonMarkDown.SetKntIcon(KntIcon.Markdown);
        buttonNavigate.SetKntIcon(KntIcon.Navigate);

        _ctrl = ctrl;
        _menuShortcuts = MenuItems(menuAssistant.Items)
            .Where(item => item.ShortcutKeys != Keys.None)
            .ToDictionary(item => AiChatHtml.ShortcutText(item.ShortcutKeys));
        _chatView = new AiChatWebRenderer(kntEditViewResult, _ctrl.Store.KNoteWebViewStyle, _menuShortcuts.Keys);
        _chatView.ShortcutPressed += ChatView_ShortcutPressed;
        kntEditViewResult.NavigationBorder = true;
        _chatView.ShowModelInfo(_ctrl.ShowModelInfo);

        // The Actions menu repeats the buttons and the model list, running the same handlers: its items
        // follow their enabled state, which also keeps the shortcuts off while they are off.
        LinkMenuToControl(menuSend, buttonSend);
        LinkMenuToControl(menuRestart, buttonRestart);
        LinkMenuToControl(menuModel, comboProviders);
        LinkMenuToControl(menuNavigateView, buttonNavigate);
        LinkMenuToControl(menuMarkdownView, buttonMarkDown);

        // Anchor=Right is not reliable for controls nested inside a SplitContainer panel
        // when AutoScaleMode rescales the form at a different DPI than the Designer was
        // saved at. Reposition explicitly instead, driven by the header panel's own
        // Resize (fires on load, DPI change and splitter drag alike).
        panelResultHeader.Resize += (s, e) => AlignResultHeader();
        panelPromptHeader.Resize += (s, e) => AlignPromptHeader();

        // InitializeComponent has already docked both headers to their panel's width (the Designer's one less
        // the panel's Padding), so their Resize has fired before these handlers existed: without aligning them
        // now, the rightmost buttons keep the Designer's position and are cut until the window is resized.
        AlignResultHeader();
        AlignPromptHeader();
    }

    private void AlignResultHeader() => AlignControlsRight(panelResultHeader, 8, 8,
        buttonMarkDown, buttonNavigate);

    private void AlignPromptHeader() => AlignControlsRight(panelPromptHeader, 6, 6,
        buttonSend, buttonRestart, panelSeparator, comboProviders);

    private static IEnumerable<ToolStripMenuItem> MenuItems(ToolStripItemCollection items)
        => items.OfType<ToolStripMenuItem>().SelectMany(item => MenuItems(item.DropDownItems).Prepend(item));

    private static void LinkMenuToControl(ToolStripMenuItem item, Control control)
    {
        item.Enabled = control.Enabled;
        control.EnabledChanged += (s, e) => item.Enabled = control.Enabled;
    }

    private static void AlignControlsRight(Control header, int rightMargin, int spacing, params Control[] controlsLeftToRight)
    {
        int right = header.Width - rightMargin;
        for (int i = controlsLeftToRight.Length - 1; i >= 0; i--)
        {
            Control c = controlsLeftToRight[i];
            c.Left = right - c.Width;
            right = c.Left - spacing;
        }
    }

    #endregion

    #region IViewBase interface

    public override void ShowView()
    {
        toolStripStatusServiceRef.Text = $" {_ctrl.ServiceRef.Alias}";
        PopulateProviders();
        UpdateOptionsMenu();
        this.Show();
        // The ctrl may already carry a completed conversation by the time the view is shown
        // (e.g. the "ln" script engine calls GetCompletionAsync before ever showing this view) -
        // sync the display to it instead of assuming a fresh, empty ctrl. After Show(): the chat
        // view needs the WebView2 of a created window.
        RefreshView();
    }

    public override void RefreshView()
    {
        textPrompt.Text = "";
        UpdateStatusInfo();
        ShowResult();
    }

    #endregion

    #region Form events handlers

    private void KNoteAIAssistantForm_Load(object sender, EventArgs e)
    {
        try
        {
            StatusProcessing(false);
        }
        catch (Exception ex)
        {
            KntMessageBox.Show(ex.Message);
        }
    }

    private async void KNoteAIAssistantForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!ViewFinalized)
        {
            // From the ctrl, not the view: in the chat view the Markdown text box is not kept up to date.
            if (_ctrl.AutoSaveChatMessagesOnViewExit && _ctrl.ChatTurns.Count > 0)
            {
                await SaveChatMessages();
            }
            if (_ctrl.AutoCloseCtrlOnViewExit)
                _ctrl.Finalize();
        }
    }

    private async void buttonSend_Click(object sender, EventArgs e)
    {
        try
        {
            StatusProcessing(true);

            if (_ctrl.ResponseMode == EAiResponseMode.Completion)
                await GoGetCompletion(textPrompt.Text);
            else
                await GoStreamCompletion(textPrompt.Text);

        }
        catch (Exception ex)
        {
            KntMessageBox.Show(ex.Message);
        }
        finally
        {
            StatusProcessing(false);
        }
    }

    private void buttonRestart_Click(object sender, EventArgs e)
    {
        _ctrl.RootSystemChat = KntConst.DefaultRootSystemChat;
        _ctrl.RestartAIAssistant();
        RestartAIAssistantView();
        Text = $"{ViewCaptionText}";
    }

    // A menu shortcut pressed while the chat view had the focus: run it as the menu would have, only if the
    // item and its menus are enabled.
    private void ChatView_ShortcutPressed(object sender, string shortcut)
    {
        if (!_menuShortcuts.TryGetValue(shortcut, out var item))
            return;

        for (ToolStripItem i = item; i != null; i = i.OwnerItem)
            if (!i.Enabled)
                return;

        item.PerformClick();
    }

    private void menuModel_DropDownOpening(object sender, EventArgs e)
    {
        foreach (ToolStripMenuItem item in menuModel.DropDownItems)
            item.Checked = item.Tag == _ctrl.CurrentProviderRef;
    }

    private void menuGetStream_Click(object sender, EventArgs e)
    {
        _ctrl.ResponseMode = EAiResponseMode.Stream;
        UpdateOptionsMenu();
    }

    private void menuGetCompletion_Click(object sender, EventArgs e)
    {
        _ctrl.ResponseMode = EAiResponseMode.Completion;
        UpdateOptionsMenu();
    }

    private async void menuCatalogPrompts_Click(object sender, EventArgs e)
    {
        var assistantInfo = await _ctrl.GetCatalogPrompt();
        if (assistantInfo == null)
            return;

        RestartAIAssistantView();
        Text = $"{ViewCaptionText} - {assistantInfo.Name}";
        textPrompt.Text = assistantInfo.User;
    }

    private void menuViewSystem_Click(object sender, EventArgs e)
    {
        ShowInfo($"System: {_ctrl.RootSystemChat}", $"{KntConst.AppName} - root system chat ");
    }

    private void menuShowModelInfo_Click(object sender, EventArgs e)
    {
        _ctrl.ShowModelInfo = !_ctrl.ShowModelInfo;
        UpdateOptionsMenu();
        // The chat view hides/shows it in place; the Markdown view is rewritten with or without it.
        _chatView.ShowModelInfo(_ctrl.ShowModelInfo);
        if (!ChatView)
            ShowResult();
    }

    private async void menuManageModels_Click(object sender, EventArgs e)
    {
        var manageCtrl = new AiProvidersManageCtrl(_ctrl.Store);
        await manageCtrl.LoadEntitiesAsync(null, false);
        manageCtrl.RunModal();

        // Providers may have been added/edited/removed: refresh the picker in place instead of
        // requiring the user to close and reopen the assistant.
        PopulateProviders();
    }

    private void comboProviders_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (comboProviders.SelectedItem is not AiProviderRef providerRef || providerRef == _ctrl.CurrentProviderRef)
            return;

        if (_ctrl.ChatTurns.Count > 0)
        {
            var result = ShowInfo("Switching the AI provider resets the current conversation. Continue?",
                "KNote", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes)
            {
                SelectProviderInCombo(_ctrl.CurrentProviderRef);
                return;
            }
        }

        _ctrl.SetProvider(providerRef);
        RestartAIAssistantView();
        Text = ViewCaptionText;
        // Covers the case where the assistant opened with zero providers configured (Send stays
        // disabled until one is picked): SetProvider just succeeded, so it's safe to re-enable now.
        buttonSend.Enabled = true;
    }

    private void buttonMarkDown_Click(object sender, EventArgs e)
    {
        _ctrl.MarkdownResultView = true;
        ShowResult();
    }

    private void buttonNavigate_Click(object sender, EventArgs e)
    {
        _ctrl.MarkdownResultView = false;
        ShowResult();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _chatView.Dispose();
        base.OnFormClosed(e);
    }

    #endregion

    #region Private methods

    private void PopulateProviders()
    {
        // Detach first: setting DataSource auto-selects an item and would otherwise fire
        // comboProviders_SelectedIndexChanged (which resets the session) during startup.
        comboProviders.SelectedIndexChanged -= comboProviders_SelectedIndexChanged;

        comboProviders.DataSource = null;
        comboProviders.DisplayMember = nameof(AiProviderRef.Alias);
        comboProviders.DataSource = _ctrl.AiProviderRefs;
        SelectProviderInCombo(_ctrl.CurrentProviderRef);

        comboProviders.SelectedIndexChanged += comboProviders_SelectedIndexChanged;

        PopulateModelMenu();
    }

    // Actions > Model: one item per provider, choosing it in the list as if picked there (same confirmation
    // and conversation reset). Its check mark is updated when the submenu opens.
    private void PopulateModelMenu()
    {
        foreach (ToolStripItem item in menuModel.DropDownItems)
            item.Dispose();
        menuModel.DropDownItems.Clear();

        foreach (var providerRef in _ctrl.AiProviderRefs)
        {
            var item = new ToolStripMenuItem(providerRef.Alias?.Replace("&", "&&")) { Tag = providerRef };
            item.Click += (s, e) => comboProviders.SelectedItem = providerRef;
            menuModel.DropDownItems.Add(item);
        }
    }

    private void SelectProviderInCombo(AiProviderRef providerRef)
    {
        comboProviders.Enabled = _ctrl.AiProviderRefs.Count > 0;
        comboProviders.SelectedItem = providerRef;
    }

    private async Task SaveChatMessages()
    {
        try
        {
            var noteEditor = new NoteEditorCtrl(_ctrl.Store);
            if (!await noteEditor.NewModel(_ctrl.Store.GetActiveOrDefaultService()))
            {
                noteEditor.Finalize();
                return;
            }
            noteEditor.Model.Topic = $"{DateTime.Now.ToString()}";
            noteEditor.Model.Description = ChatTranscript();
            noteEditor.Model.Tags = "[AIAssistant]";
            noteEditor.Run();
        }
        catch (Exception ex)
        {
            ShowInfo(ex.Message.ToString());
        }
    }

    private void RestartAIAssistantView()
    {
        toolStripStatusLabelTokens.Text = $"Tokens: {_ctrl.TotalTokens} ";
        toolStripStatusLabelProcessingTime.Text = $" | Processing time: --";
        _sbResult.Clear();
        textPrompt.Text = "";
        textPrompt.Focus();
        ShowResult();
    }

    private void UpdateStatusInfo()
    {
        toolStripStatusLabelTokens.Text = $"Tokens: {_ctrl.TotalTokens} ";
        toolStripStatusLabelProcessingTime.Text = $" | Processing time: {_ctrl.TotalProcessingTime}";
    }

    private bool ChatView => !_ctrl.MarkdownResultView;

    // What the Markdown view shows (and what is saved as a note), with the model info if it is shown.
    private string ChatTranscript() => AiChatTranscript.Markdown(_ctrl.ChatTurns, _ctrl.ShowModelInfo);

    private void UpdateOptionsMenu()
    {
        menuGetStream.Checked = _ctrl.ResponseMode == EAiResponseMode.Stream;
        menuGetCompletion.Checked = _ctrl.ResponseMode == EAiResponseMode.Completion;
        menuShowModelInfo.Checked = _ctrl.ShowModelInfo;
    }

    // For the synchronous event handlers.
    private async void ShowResult()
    {
        try
        {
            await ShowResultAsync();
        }
        catch (Exception ex)
        {
            KntMessageBox.Show(ex.Message);
        }
    }

    // The whole conversation in the current mode: as a chat (Navigation, the default) or as its Markdown source.
    private async Task ShowResultAsync()
    {
        if (ChatView)
        {
            await _chatView.LoadAsync(_ctrl.ChatTurns);
            // Loading the page moves the focus into WebView2: back to the prompt, to type in it (and use the
            // menu shortcuts) right away.
            if (textPrompt.Enabled)
                ActiveControl = textPrompt;
        }
        else
            kntEditViewResult.ShowMarkdownContent(ChatTranscript());

        ScrollMarkdownToEnd();
        UpdateViewButtons();
    }

    // The button of the mode not shown is the enabled one.
    private void UpdateViewButtons()
    {
        buttonMarkDown.Enabled = ChatView;
        buttonNavigate.Enabled = !ChatView;
    }

    private void ScrollMarkdownToEnd()
    {
        if (ChatView)
            return;

        kntEditViewResult.MarkdownContentControl.SelectionStart = kntEditViewResult.MarkdownContentControl.Text.Length;
        kntEditViewResult.MarkdownContentControl.ScrollToCaret();
    }

    private void StatusProcessing(bool processing = false)
    {
        if (processing)
        {
            toolStripStatusLabelProcessing.Text = " Processing ...";
            textPrompt.Enabled = false;
            comboProviders.Enabled = false;
            menuOptions.Enabled = false;
            buttonSend.Enabled = false;
            buttonRestart.Enabled = false;
            buttonMarkDown.Enabled = false;
            buttonNavigate.Enabled = false;
        }
        else
        {
            toolStripStatusLabelProcessing.Text = " ";
            textPrompt.Enabled = true;
            comboProviders.Enabled = _ctrl.AiProviderRefs.Count > 0;
            menuOptions.Enabled = true;
            buttonSend.Enabled = _ctrl.CurrentProviderRef != null;
            buttonRestart.Enabled = true;
            // Sending keeps the current mode: the chat view adds the turn as it arrives.
            UpdateViewButtons();
            ScrollMarkdownToEnd();
            ActiveControl = textPrompt;
        }
    }

    private async Task GoGetCompletion(string prompt)
    {
        // The view can't change while sending (StatusProcessing disables its buttons).
        var chatView = ChatView;
        if (chatView)
            await _chatView.BeginTurnAsync(_ctrl.ChatTurns, prompt, _ctrl.CurrentProviderRef?.Alias, null);

        try
        {
            await _ctrl.GetCompletionAsync(prompt);
        }
        catch
        {
            if (chatView)
                _chatView.FailTurn();
            throw;
        }

        if (chatView)
            _chatView.EndTurn(_ctrl.ChatTurns[^1]);
        else
            kntEditViewResult.ShowMarkdownContent(ChatTranscript());

        textPrompt.Text = "";
        UpdateStatusInfo();
    }

    private async Task GoStreamCompletion(string prompt)
    {
        var chatView = ChatView;
        if (chatView)
        {
            // Repainted from the ctrl's StreamingResult while it streams in: no StreamToken handler needed.
            await _chatView.BeginTurnAsync(_ctrl.ChatTurns, prompt, _ctrl.CurrentProviderRef?.Alias, () => _ctrl.StreamingResult);
        }
        else
        {
            // Start from the whole transcript so far, not an empty buffer: the streamed turn (the ctrl's
            // StreamToken, same layout) is appended to the conversation already shown. Rebuilt from the
            // ctrl's turns, it also drops the partial text of a failed stream.
            _sbResult.Clear();
            _sbResult.Append(ChatTranscript());
            _countNRres = 0;
            _ctrl.StreamToken += _com_StreamToken;
        }

        try
        {
            await _ctrl.StreamCompletionAsync(prompt);
        }
        catch
        {
            if (chatView)
                _chatView.FailTurn();
            throw;
        }
        finally
        {
            // Must run even if the stream throws mid-way (e.g. a transient SDK error on the
            // trailing chunk): otherwise this handler stays subscribed and the next attempt
            // fires two handlers at once, interleaving garbled text into the result view.
            _ctrl.StreamToken -= _com_StreamToken;
        }

        if (chatView)
            _chatView.EndTurn(_ctrl.ChatTurns[^1]);
        else
            // The streamed text, plus the usage line of the new answer if the model info is shown.
            kntEditViewResult.ShowMarkdownContent(ChatTranscript());

        textPrompt.Text = "";
        UpdateStatusInfo();
    }

    private void _com_StreamToken(object sender, ControllerEventArgs<string> e)
    {
        if (kntEditViewResult.MarkdownContentControl.InvokeRequired)
        {
            kntEditViewResult.MarkdownContentControl.Invoke(new MethodInvoker(delegate
            {
                UpdateTextResult(e.Entity?.ToString());
            }));
        }
        else
        {
            UpdateTextResult(e.Entity?.ToString());
        }
    }

    private void UpdateTextResult(string text)
    {
        _sbResult.Append(text);
        _countNRres++;
        if (_countNRres > 10)
        {
            RefreshStreamResult();
            _countNRres = 0;
        }
    }

    private void RefreshStreamResult()
    {
        kntEditViewResult.MarkdownContentControl.Text = _sbResult.ToString();
        kntEditViewResult.MarkdownContentControl.SelectionStart = kntEditViewResult.MarkdownContentControl.Text.Length;
        kntEditViewResult.MarkdownContentControl.ScrollToCaret();
        kntEditViewResult.MarkdownContentControl.Update();
    }

    #endregion
}
