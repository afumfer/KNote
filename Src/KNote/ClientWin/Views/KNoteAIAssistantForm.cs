using System.Text;
using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;
using KNote.Model.Dto;
using KNote.ClientWin.Utils;
using KNote.Service.Core;
using KntIcons;

namespace KNote.ClientWin.Views;

public partial class KNoteAIAssistantForm : KntForm, IViewBase
{
    #region Private fields

    private readonly KNoteAIAssistantCtrl _ctrl;
    private readonly AiChatWebRenderer _chatView;
    // The files of the message being written.
    private readonly FlowLayoutPanel _attachmentsBar;

    // "Search the Internet" (KNote Atlas, chapter 23): a toggle next to Attach, and the notice while it is on.
    private readonly Button _buttonWebSearch;
    private readonly Label _webSearchNotice;
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
        buttonAttach.SetKntIcon(KntIcon.Attach);
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
        _chatView = new AiChatWebRenderer(kntEditViewResult, _ctrl.Store.KNoteWebViewStyle, _menuShortcuts.Keys, _ctrl.PreviewOf);
        _chatView.ShortcutPressed += ChatView_ShortcutPressed;
        _chatView.ActionsAnswered += ChatView_ActionsAnswered;
        _ctrl.ActivityChanged += Ctrl_ActivityChanged;
        // The citations of notes (#123) of the answers: notes of the assistant's repository.
        kntEditViewResult.LinkHandler = url => NoteCitations.TryOpen(url, this, _ctrl.Store, _ctrl.ServiceRef?.Service, message => ShowInfo(message));
        kntEditViewResult.NavigationBorder = true;
        _chatView.ShowModelInfo(_ctrl.ShowModelInfo);

        // Between the header and the box: its place among the docked controls of the panel.
        _attachmentsBar = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Visible = false };
        splitChat.Panel2.Controls.Add(_attachmentsBar);
        splitChat.Panel2.Controls.SetChildIndex(_attachmentsBar, 1);
        _webSearchNotice = new Label
        {
            Dock = DockStyle.Top, AutoSize = true, Visible = false,
            Padding = new Padding(3, 4, 3, 4)
        };
        splitChat.Panel2.Controls.Add(_webSearchNotice);
        splitChat.Panel2.Controls.SetChildIndex(_webSearchNotice, 1);
        // A button, not a CheckBox with the look of one: in dark mode WinForms paints that one white, with no image or color.
        _buttonWebSearch = new Button
        {
            Text = "Internet", Font = buttonAttach.Font, Size = buttonAttach.Size, Top = buttonAttach.Top,
            TextImageRelation = TextImageRelation.ImageBeforeText, UseVisualStyleBackColor = true,
            TabIndex = buttonAttach.TabIndex + 1, Visible = false, AccessibleRole = AccessibleRole.CheckButton
        };
        _buttonWebSearch.SetKntIcon(KntIcon.WebSearch);
        _buttonWebSearch.Click += buttonWebSearch_Click;
        new ToolTip().SetToolTip(_buttonWebSearch, AiWebSearchRules.ToggleText);
        panelPromptHeader.Controls.Add(_buttonWebSearch);
        textPrompt.AllowDrop = true;
        textPrompt.KeyDown += textPrompt_KeyDown;
        textPrompt.DragEnter += textPrompt_DragEnter;
        textPrompt.DragDrop += textPrompt_DragDrop;

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

    // The buttons next to the model, without the ones that do not show, so they leave no gap (their Visible says nothing here,
    // as it is false until the window shows). ShowWebSearch aligns it again: Internet depends on the provider.
    private void AlignPromptHeader()
    {
        var controls = new List<Control> { buttonSend, buttonNewSession };
        if (_ctrl.CanAttach)
            controls.Add(buttonAttach);
        if (_ctrl.SupportsWebSearch)
            controls.Add(_buttonWebSearch);
        controls.Add(panelSeparator);
        controls.Add(comboProviders);

        AlignControlsRight(panelPromptHeader, 6, 6, controls.ToArray());
    }

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
        UpdateRepositoryDisplay();
        PopulateProviders();
        ShowWebSearch();
        PopulateRepositories();
        UpdateOptionsMenu();

        // The sessions panel only when the conversation is persisted (the assistant opened from the menu).
        if (_ctrl.PersistSession)
        {
            MinimumSize = new Size(MinimumSize.Width + LogicalToDeviceUnits(SessionsPanelWidth), MinimumSize.Height);
            RestoreWindowBounds();
        }
        else
        {
            buttonAttach.Visible = false;
            splitSessions.Panel2Collapsed = true;
            ClientSize = new Size(ClientSize.Width - LogicalToDeviceUnits(DesignerClientWidth - WidthWithoutSessions), ClientSize.Height);
        }

        this.Show();

        if (_ctrl.PersistSession)
        {
            // In code: the Designer's SplitterDistance is not rescaled with the window (FixedPanel = Panel2).
            var state = _ctrl.Store.State.AiAssistantWindow;
            var sessionsWidth = state.SessionsPanelWidth > 0 ? state.SessionsPanelWidth : LogicalToDeviceUnits(SessionsPanelWidth);
            SetSplitterDistance(splitSessions, splitSessions.Width - splitSessions.SplitterWidth - sessionsWidth);
            if (state.PromptPanelHeight > 0)
                SetSplitterDistance(splitChat, splitChat.Height - splitChat.SplitterWidth - state.PromptPanelHeight);
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
        if (_ctrl.PersistSession)
            SaveWindowState();

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

            if (!await _ctrl.SaveAttachmentsAsync(textPrompt.Text))
                return;

            // A new message without answering the card rejects what it proposed: the page shows it.
            if (_ctrl.RejectPendingActions())
                _chatView.Invalidate();

            await SendAsync(textPrompt.Text, automatic: false);
            ShowAttachments();
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

    private async Task SendAsync(string prompt, bool automatic)
    {
        if (_ctrl.ResponseMode == EAiResponseMode.Completion)
            await GoGetCompletion(prompt, automatic);
        else
            await GoStreamCompletion(prompt, automatic);

        // The answer has just saved the session: a new one, or moved to the top of the list.
        if (_ctrl.PersistSession)
            await LoadSessionsAsync();

        // The Markdown view has no card: the same question, all or nothing.
        if (!ChatView && _ctrl.PendingActions.Count > 0)
        {
            var actions = _ctrl.PendingActions;
            var answer = ShowInfo($"The assistant wants to do this:\n\n{string.Join("\n", actions.Select(a => a.Text))}", "AI assistant", MessageBoxButtons.YesNo);
            await AnswerActionsAsync(answer == DialogResult.Yes ? actions.Select(a => a.ActionId).ToList() : new List<Guid>(), false);
        }
    }

    #region Attachments

    private bool CanAttachNow => _ctrl.CanAttach && !_processing;

    private async void buttonAttach_Click(object sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog { Title = "Attach files to the message", Multiselect = true };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;
        await AddFilesAsync(dialog.FileNames.Select(f => (Path.GetFileName(f), (Func<byte[]>)(() => File.ReadAllBytes(f)))).ToList());
    }

    // Ctrl+V with a picture or files in the clipboard attaches them; with text, pastes it as always.
    private async void textPrompt_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.V || !e.Control || e.Alt || !CanAttachNow)
            return;

        var files = KntClipboardFiles.Read();
        if (files.Count == 0)
            return;

        e.SuppressKeyPress = true;
        await AddFilesAsync(files);
    }

    private void textPrompt_DragEnter(object sender, DragEventArgs e)
    {
        e.Effect = CanAttachNow && e.Data?.GetDataPresent(DataFormats.FileDrop) == true ? DragDropEffects.Copy : DragDropEffects.None;
    }

    private async void textPrompt_DragDrop(object sender, DragEventArgs e)
    {
        if (e.Data?.GetData(DataFormats.FileDrop) is string[] files)
            await AddFilesAsync(files.Select(f => (Path.GetFileName(f), (Func<byte[]>)(() => File.ReadAllBytes(f)))).ToList());
    }

    private async Task AddFilesAsync(IReadOnlyList<(string Name, Func<byte[]> Content)> files)
    {
        foreach (var (name, content) in files)
        {
            try
            {
                var error = _ctrl.AddAttachment(name, content());
                if (error != null)
                    ShowInfo(error);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                ShowInfo($"{name} could not be read: {ex.Message}");
            }
        }
        ShowAttachments();

        if (files.Any(f => AiAttachmentRules.IsImage(AiAttachmentRules.ContentTypeOf(f.Name))) && !await _ctrl.SupportsImagesAsync())
            ShowInfo($"{_ctrl.CurrentProviderRef?.Alias} does not see images: they are only saved with the conversation.");
    }

    private void ShowAttachments()
    {
        _attachmentsBar.SuspendLayout();
        foreach (var control in _attachmentsBar.Controls.Cast<Control>().ToList())
            control.Dispose();

        foreach (var attachment in _ctrl.PendingAttachments)
        {
            var button = new Button { Text = $"{attachment.FileName}  ✕", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, UseVisualStyleBackColor = true };
            button.Click += (s, e) =>
            {
                _ctrl.RemoveAttachment(attachment);
                ShowAttachments();
            };
            _attachmentsBar.Controls.Add(button);
        }

        if (_ctrl.ImagesSentTo is { } provider)
            _attachmentsBar.Controls.Add(new Label { Text = $"Images are sent to {provider}.", AutoSize = true, Margin = new Padding(LogicalToDeviceUnits(3), LogicalToDeviceUnits(8), LogicalToDeviceUnits(3), LogicalToDeviceUnits(3)) });

        _attachmentsBar.Visible = _ctrl.PendingAttachments.Count > 0;
        _attachmentsBar.ResumeLayout();
    }

    #endregion

    private async void ChatView_ActionsAnswered(object sender, AiChatHtml.ActionsAnswer answer)
    {
        if (_processing)
            return;

        try
        {
            StatusProcessing(true);
            await AnswerActionsAsync(answer.Accepted, answer.DontAsk);
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

    private async Task AnswerActionsAsync(IReadOnlyCollection<Guid> accepted, bool dontAsk)
    {
        var goOn = await _ctrl.AnswerActionsAsync(accepted, dontAsk);

        if (ChatView)
            _chatView.ReplaceLastTurn(_ctrl.ChatTurns[^1]);
        else
            kntEditViewResult.ShowMarkdownContent(ChatTranscript());

        if (goOn)
            await SendAsync(AiActionRules.ContinuePrompt, automatic: true);
    }

    // A new conversation (the current one is already saved, see KNoteAIAssistantCtrl.NewSessionAsync).
    private async void buttonNewSession_Click(object sender, EventArgs e)
    {
        try
        {
            if (!await _ctrl.NewSessionAsync())
                return;

            RestartAIAssistantView();
            SetCaption();
            SelectCurrentSession();
        }
        catch (Exception ex)
        {
            KntMessageBox.Show(ex.Message);
        }
    }

    // Another repository: its sessions and its notes, so a new conversation (the current one is saved first).
    private async void comboRepositories_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (comboRepositories.SelectedItem is not ServiceRef repository || repository == _ctrl.ServiceRef)
            return;

        try
        {
            if (_ctrl.ChatTurns.Count > 0)
            {
                var result = ShowInfo($"Changing the repository starts a new conversation, with the sessions of {repository.Name}. Continue?",
                    "KNote", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result != DialogResult.Yes || !await _ctrl.LeaveSessionAsync())
                {
                    SelectRepositoryInCombo(_ctrl.ServiceRef);
                    return;
                }
            }

            _ctrl.SetRepository(repository);
            RestartAIAssistantView();
            UpdateRepositoryDisplay();
            await LoadSessionsAsync();
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
                SetCaption(_ctrl.SessionTopic);
                RefreshView();
                ShowWebSearch();
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
        SetCaption(assistantInfo.Name);
        textPrompt.Text = assistantInfo.User;
        // A new conversation, not in the list yet.
        SelectCurrentSession();
    }

    private async void menuViewSystem_Click(object sender, EventArgs e)
    {
        var composed = await _ctrl.GetSystemPromptAsync();
        new AiPromptViewForm($"{KntConst.AppName} - what the AI receives", composed).ShowDialog(this);
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
            SetCaption();
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

    // Before the first Show(): StartPosition is still CenterScreen then, and switching it to Manual later
    // (e.g. from Load) is unreliable (see AppInfoAlarmsForm). The saved position can be off every current
    // screen (monitor or Windows scale changed since it was saved), and may legitimately be negative.
    private void RestoreWindowBounds()
    {
        var bounds = _ctrl.Store.State.AiAssistantWindow.Bounds;
        if (bounds.Width > 0)
            Size = new Size(bounds.Width, bounds.Height);
        if (bounds.X != 0 || bounds.Y != 0)
        {
            StartPosition = FormStartPosition.Manual;
            Bounds = WindowPlacement.EnsureVisible(new Rectangle(new Point(bounds.X, bounds.Y), Size));
        }
    }

    // On closing: RestoreBounds keeps the normal-state bounds while the window is maximized or minimized.
    // Minimized, the panels may have been laid out to the collapsed window: their sizes aren't kept.
    private void SaveWindowState()
    {
        var state = _ctrl.Store.State.AiAssistantWindow;
        var bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
        state.Bounds.X = bounds.X;
        state.Bounds.Y = bounds.Y;
        state.Bounds.Width = bounds.Width;
        state.Bounds.Height = bounds.Height;
        if (WindowState != FormWindowState.Minimized)
        {
            state.SessionsPanelWidth = splitSessions.Panel2.Width;
            state.PromptPanelHeight = splitChat.Panel2.Height;
        }
        _ctrl.SaveState();
    }

    // Kept within the panels' minimum sizes: the saved size may not fit a smaller window.
    private static void SetSplitterDistance(SplitContainer split, int distance)
    {
        int length = split.Orientation == Orientation.Vertical ? split.Width : split.Height;
        int max = length - split.SplitterWidth - split.Panel2MinSize;
        split.SplitterDistance = Math.Max(split.Panel1MinSize, Math.Min(distance, max));
    }

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

    // The repositories where the sessions can be (only shown with the sessions panel): the one of the assistant
    // selected, even if the user's role there no longer allows the sessions (then it is the only one listed with it).
    private void PopulateRepositories()
    {
        comboRepositories.SelectedIndexChanged -= comboRepositories_SelectedIndexChanged;

        var repositories = _ctrl.Repositories;
        if (!repositories.Contains(_ctrl.ServiceRef))
            repositories.Insert(0, _ctrl.ServiceRef);
        comboRepositories.DataSource = null;
        comboRepositories.DisplayMember = nameof(ServiceRef.Name);
        comboRepositories.DataSource = repositories;
        SelectRepositoryInCombo(_ctrl.ServiceRef);

        comboRepositories.SelectedIndexChanged += comboRepositories_SelectedIndexChanged;
    }

    private void SelectRepositoryInCombo(ServiceRef repository)
    {
        comboRepositories.Enabled = comboRepositories.Items.Count > 1 && !_processing;
        comboRepositories.SelectedItem = repository;
    }

    // Where the assistant works: in the title, the status bar and the sessions panel.
    private void UpdateRepositoryDisplay()
    {
        toolStripStatusServiceRef.Text = $" Repository: {_ctrl.ServiceRef.Name}";
        SetCaption();
    }

    private void SetCaption(string detail = null) =>
        Text = string.IsNullOrEmpty(detail) ? $"{ViewCaptionText} - {_ctrl.ServiceRef.Name}" : $"{ViewCaptionText} - {_ctrl.ServiceRef.Name} - {detail}";

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
        ShowWebSearch();
    }

    // Shown only with a provider that can search; off in a new or resumed conversation (KNoteAIAssistantCtrl.WebSearch).
    private void ShowWebSearch()
    {
        _buttonWebSearch.Visible = _ctrl.SupportsWebSearch;
        _webSearchNotice.Text = _ctrl.WebSearch ? AiWebSearchRules.Notice(_ctrl.CurrentProviderRef?.Provider) : "";
        _webSearchNotice.Visible = _ctrl.WebSearch;
        PaintWebSearchButton(_ctrl.WebSearch);
        AlignPromptHeader();
    }

    // Orange while it is on, as in the Web: the system's pressed button is hard to tell from the others, and the searches send
    // data out. Black over the orange in light and dark mode.
    private void PaintWebSearchButton(bool on)
    {
        if (on)
        {
            _buttonWebSearch.BackColor = Color.Orange;
            _buttonWebSearch.ForeColor = Color.Black;
        }
        else
        {
            _buttonWebSearch.ResetBackColor();
            _buttonWebSearch.ResetForeColor();
        }
        // After the colors: changing the back color turns it off.
        _buttonWebSearch.UseVisualStyleBackColor = !on;
        _buttonWebSearch.SetKntIcon(KntIcon.WebSearch, color: on ? Color.Black : null);
    }

    private void buttonWebSearch_Click(object sender, EventArgs e)
    {
        _ctrl.WebSearch = !_ctrl.WebSearch;
        ShowWebSearch();
    }

    private void UpdateStatusInfo()
    {
        toolStripStatusLabelTokens.Text = $"Tokens: {_ctrl.TotalTokens} ";
        toolStripStatusLabelProcessingTime.Text = $" | Processing time: {_ctrl.TotalProcessingTime}";
    }

    private bool ChatView => !_ctrl.MarkdownResultView;

    // What the Markdown view shows: the conversation exactly as it is saved in its session note (the format shared
    // with the Web assistant), with the hidden marker of each message and the usage of each answer in it.
    private string ChatTranscript() => AiChatSessionTranscript.Write(_ctrl.ChatTurns);

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
            await _chatView.LoadAsync(_ctrl.ChatTurns, _ctrl.CurrentProviderRef?.Alias);
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

    // The ctrl can also run a turn out of the UI thread (the "ln" script engine).
    private void Ctrl_ActivityChanged(object sender, ControllerEventArgs<string> e)
    {
        if (IsDisposed || !IsHandleCreated)
            return;
        if (InvokeRequired)
            BeginInvoke(() => ShowActivity(e.Entity));
        else
            ShowActivity(e.Entity);
    }

    private void ShowActivity(string activity)
    {
        if (!_processing)
            return;
        toolStripStatusLabelProcessing.Text = $" {activity ?? "Processing ..."}";
        if (ChatView)
            _chatView.ShowActivity(activity);
    }

    private void StatusProcessing(bool processing = false)
    {
        if (processing)
        {
            toolStripStatusLabelProcessing.Text = " Processing ...";
            // Without this, disabling the box passes the focus on to the chat. At the end of the turn it would go into the page,
            // to its first control (the reasoning of the first answer), and the browser would scroll up to show it.
            ActiveControl = null;
            textPrompt.Enabled = false;
            comboProviders.Enabled = false;
            comboRepositories.Enabled = false;
            menuOptions.Enabled = false;
            buttonSend.Enabled = false;
            buttonAttach.Enabled = false;
            _buttonWebSearch.Enabled = false;
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
            comboRepositories.Enabled = comboRepositories.Items.Count > 1;
            menuOptions.Enabled = true;
            buttonSend.Enabled = _ctrl.CurrentProviderRef != null;
            buttonAttach.Enabled = _ctrl.CurrentProviderRef != null;
            _buttonWebSearch.Enabled = true;
            buttonNewSession.Enabled = true;
            _processing = false;
            // Sending keeps the current mode: the chat view adds the turn as it arrives.
            UpdateViewButtons();
            ScrollMarkdownToEnd();
            ActiveControl = textPrompt;
        }
    }

    private async Task GoGetCompletion(string prompt, bool automatic)
    {
        // The view can't change while sending (StatusProcessing disables its buttons).
        var chatView = ChatView;
        if (chatView)
            await _chatView.BeginTurnAsync(_ctrl.ChatTurns, automatic ? null : prompt, _ctrl.CurrentProviderRef?.Alias, null, () => _ctrl.StreamingReasoning, _ctrl.TurnAttachments);

        try
        {
            await _ctrl.GetCompletionAsync(prompt, automatic);
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

        if (!automatic)
            textPrompt.Text = "";
        UpdateStatusInfo();
    }

    private async Task GoStreamCompletion(string prompt, bool automatic)
    {
        var chatView = ChatView;
        if (chatView)
        {
            // Repainted from the ctrl's StreamingResult while it streams in: no StreamToken handler needed.
            await _chatView.BeginTurnAsync(_ctrl.ChatTurns, automatic ? null : prompt, _ctrl.CurrentProviderRef?.Alias, () => _ctrl.StreamingResult, () => _ctrl.StreamingReasoning, _ctrl.TurnAttachments);
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
            await _ctrl.StreamCompletionAsync(prompt, automatic);
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

        if (!automatic)
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
