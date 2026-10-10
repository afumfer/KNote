using System.Text;
using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
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
    // Designer widths of the window and of the sessions panel (logical units): without sessions the window
    // keeps the width it had before the panel existed.
    private const int DesignerClientWidth = 1100;
    private const int WidthWithoutSessions = 858;
    private const int SessionsPanelWidth = 254;
    // The sessions list is being filled or its selection set from code: not the user choosing a session.
    private bool _syncingSessions;
    // An answer is being written (StatusProcessing).
    private bool _processing;

    #endregion

    #region Constructor

    public KNoteAIAssistantForm(KNoteAIAssistantCtrl ctrl)
    {
        InitializeComponent();

        buttonSend.SetKntIcon(KntIcon.Send);
        buttonNewSession.SetKntIcon(KntIcon.Restart);
        buttonMarkDown.SetKntIcon(KntIcon.Markdown);
        buttonNavigate.SetKntIcon(KntIcon.Navigate);

        _ctrl = ctrl;

        ListViewStyle.ApplyStandard(listViewSessions);
        listViewSessions.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        listViewSessions.ShowItemToolTips = true;
        listViewSessions.Columns.Add("Session", LogicalToDeviceUnits(140), HorizontalAlignment.Left);
        listViewSessions.Columns.Add("Modified", LogicalToDeviceUnits(100), HorizontalAlignment.Left);
        ListViewColumnResizer.Attach(listViewSessions, 0);

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
        LinkMenuToControl(menuNewSession, buttonNewSession);
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
        buttonNavigate, buttonMarkDown);

    private void AlignPromptHeader() => AlignControlsRight(panelPromptHeader, 6, 6,
        buttonSend, buttonNewSession, panelSeparator, comboProviders);

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

        // The sessions panel only when the conversation is persisted (the assistant opened from the menu).
        if (_ctrl.PersistSession)
            MinimumSize = new Size(MinimumSize.Width + LogicalToDeviceUnits(SessionsPanelWidth), MinimumSize.Height);
        else
        {
            splitSessions.Panel2Collapsed = true;
            ClientSize = new Size(ClientSize.Width - LogicalToDeviceUnits(DesignerClientWidth - WidthWithoutSessions), ClientSize.Height);
        }

        this.Show();

        if (_ctrl.PersistSession)
        {
            // In code: the Designer's SplitterDistance is not rescaled with the window (FixedPanel = Panel2).
            splitSessions.SplitterDistance = Math.Max(splitSessions.Panel1MinSize,
                splitSessions.Width - splitSessions.SplitterWidth - LogicalToDeviceUnits(SessionsPanelWidth));
            LoadSessions();
        }

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
            // Saved after each answer: this only retries a save that failed.
            if (_ctrl.PersistSession)
                await _ctrl.SaveSessionAsync();
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

            // The answer has just saved the session: a new one, or moved to the top of the list.
            if (_ctrl.PersistSession)
                await LoadSessionsAsync();
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

    // A new conversation (the current one is already saved, see KNoteAIAssistantCtrl.NewSessionAsync).
    private async void buttonNewSession_Click(object sender, EventArgs e)
    {
        try
        {
            if (!await _ctrl.NewSessionAsync())
                return;

            RestartAIAssistantView();
            Text = $"{ViewCaptionText}";
            SelectCurrentSession();
        }
        catch (Exception ex)
        {
            KntMessageBox.Show(ex.Message);
        }
    }

    private async void listViewSessions_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
    {
        if (_syncingSessions || !e.IsSelected || e.Item.Tag is not Guid noteId || noteId == _ctrl.SessionNoteId)
            return;

        // While an answer is being written: back to the session in the chat, once this selection change is over.
        if (_processing)
        {
            BeginInvoke(SelectCurrentSession);
            return;
        }

        try
        {
            if (await _ctrl.OpenSessionAsync(noteId))
            {
                // Resumed with its own provider, which may not be the one in the list.
                SelectProviderInCombo(_ctrl.CurrentProviderRef);
                buttonSend.Enabled = true;
                Text = $"{ViewCaptionText} - {_ctrl.SessionTopic}";
                RefreshView();
            }
            else
                SelectCurrentSession();
        }
        catch (Exception ex)
        {
            KntMessageBox.Show(ex.Message);
        }
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
        // A new conversation, not in the list yet.
        SelectCurrentSession();
    }

    private void menuViewSystem_Click(object sender, EventArgs e)
    {
        ShowInfo($"System: {_ctrl.RootSystemChat}", $"{KntConst.AppName} - root system chat ");
    }

    private void menuShowModelInfo_Click(object sender, EventArgs e)
    {
        _ctrl.ShowModelInfo = !_ctrl.ShowModelInfo;
        UpdateOptionsMenu();
        // The chat view hides/shows it in place. The Markdown view always has it, in the marker of each answer.
        _chatView.ShowModelInfo(_ctrl.ShowModelInfo);
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

    // A different provider can't continue the conversation: it starts a new one (the current one is saved first).
    private async void comboProviders_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (comboProviders.SelectedItem is not AiProviderRef providerRef || providerRef == _ctrl.CurrentProviderRef)
            return;

        try
        {
            if (_ctrl.ChatTurns.Count > 0)
            {
                var result = ShowInfo("Switching the AI provider starts a new conversation. Continue?",
                    "KNote", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result != DialogResult.Yes || !await _ctrl.LeaveSessionAsync())
                {
                    SelectProviderInCombo(_ctrl.CurrentProviderRef);
                    return;
                }
            }

            _ctrl.SetProvider(providerRef);
            RestartAIAssistantView();
            Text = ViewCaptionText;
            SelectCurrentSession();
            // Covers the case where the assistant opened with zero providers configured (Send stays
            // disabled until one is picked): SetProvider just succeeded, so it's safe to re-enable now.
            buttonSend.Enabled = true;
        }
        catch (Exception ex)
        {
            KntMessageBox.Show(ex.Message);
        }
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
        // Disposing an item also removes it from its menu: enumerate a copy, not the collection that changes.
        foreach (var item in menuModel.DropDownItems.Cast<ToolStripItem>().ToArray())
            item.Dispose();

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

    // For the synchronous ShowView.
    private async void LoadSessions()
    {
        try
        {
            await LoadSessionsAsync();
        }
        catch (Exception ex)
        {
            KntMessageBox.Show(ex.Message);
        }
    }

    // The user's sessions, most recent first, with the one in the chat selected.
    private async Task LoadSessionsAsync()
    {
        var sessions = await _ctrl.GetSessionsAsync();

        _syncingSessions = true;
        listViewSessions.BeginUpdate();
        try
        {
            listViewSessions.Items.Clear();
            foreach (var session in sessions)
            {
                var item = new ListViewItem(session.Topic) { Tag = session.NoteId, ToolTipText = session.Topic };
                item.SubItems.Add(session.ModificationDateTime.ToString("g"));
                listViewSessions.Items.Add(item);
            }
        }
        finally
        {
            listViewSessions.EndUpdate();
            _syncingSessions = false;
        }

        SelectCurrentSession();
    }

    // Selects the session in the chat in the list (none for a new conversation, not saved yet).
    private void SelectCurrentSession()
    {
        _syncingSessions = true;
        try
        {
            foreach (ListViewItem item in listViewSessions.Items)
            {
                item.Selected = item.Tag is Guid noteId && noteId == _ctrl.SessionNoteId;
                if (item.Selected)
                    item.EnsureVisible();
            }
        }
        finally
        {
            _syncingSessions = false;
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

    // What the Markdown view shows: the conversation exactly as it is saved in its session note (the format shared
    // with the Web assistant), with the hidden marker of each message and the usage of each answer in it.
    private string ChatTranscript() => AiChatSessionTranscript.Write(_ctrl.ChatTurns.Select(t => t.ToDto()));

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
            buttonNewSession.Enabled = false;
            buttonMarkDown.Enabled = false;
            buttonNavigate.Enabled = false;
            // The sessions list stays enabled (disabled, the native list paints a white background in dark mode):
            // choosing a session is ignored while processing (see listViewSessions_ItemSelectionChanged).
            _processing = true;
        }
        else
        {
            toolStripStatusLabelProcessing.Text = " ";
            textPrompt.Enabled = true;
            comboProviders.Enabled = _ctrl.AiProviderRefs.Count > 0;
            menuOptions.Enabled = true;
            buttonSend.Enabled = _ctrl.CurrentProviderRef != null;
            buttonNewSession.Enabled = true;
            _processing = false;
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
            // StreamToken, still without its markers) is appended to the conversation already shown, and the view
            // is rewritten in the saved format once it is answered. Rebuilt from the ctrl's turns, it also drops
            // the partial text of a failed stream.
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
            // The whole conversation in the saved format: the new turn now with its markers and usage.
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
