using System.Text;
using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.Model;

namespace KNote.ClientWin.Views;

public partial class KNoteAIAssistantForm : Form, IViewBase
{
    #region Private fields

    private readonly KNoteAIAssistantCtrl _ctrl;
    private bool _viewFinalized = false;
    private int _countNRres;
    private StringBuilder _sbResult = new StringBuilder();
    private const string ViewCaptionText = "KNote AI Assistant";

    #endregion

    #region Constructor

    public KNoteAIAssistantForm(KNoteAIAssistantCtrl ctrl)
    {
        InitializeComponent();

        _ctrl = ctrl;

        // Anchor=Right is not reliable for controls nested inside a SplitContainer panel
        // when AutoScaleMode rescales the form at a different DPI than the Designer was
        // saved at. Reposition explicitly instead, driven by the header panel's own
        // Resize (fires on load, DPI change and splitter drag alike).
        panelResultHeader.Resize += (s, e) => AlignControlsRight(panelResultHeader, 8, 8,
            radioGetStream, radioGetCompletion, buttonMarkDown, buttonNavigate);
        panelPromptHeader.Resize += (s, e) => AlignControlsRight(panelPromptHeader, 6, 6,
            comboProviders, buttonManageProviders, buttonSend, buttonRestart, panelSeparator, buttonCatalogPrompts, buttonViewSystem);
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

    public void ShowView()
    {
        toolStripStatusServiceRef.Text = $" {_ctrl.ServiceRef.Alias}";
        PopulateProviders();
        MarkDownView();
        radioGetCompletion.Checked = _ctrl.ResponseMode == EAiResponseMode.Completion;
        radioGetStream.Checked = !radioGetCompletion.Checked;
        // The ctrl may already carry a completed conversation by the time the view is shown
        // (e.g. the "ln" script engine calls GetCompletionAsync before ever showing this view) -
        // sync the display to it instead of assuming a fresh, empty ctrl.
        RefreshView();
        this.Show();
    }

    public Result<EControllerResult> ShowModalView()
    {
        return _ctrl.DialogResultToControllerResult(this.ShowDialog());
    }

    public void OnClosingView()
    {
        _viewFinalized = true;
        this.Close();
    }

    public DialogResult ShowInfo(string info, string caption = "KNote", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        return MessageBox.Show(info, caption, buttons, icon);
    }

    public void RefreshView()
    {
        kntEditViewResult.MarkdownContentControl.Text = _ctrl.ChatTextMessasges.ToString();
        kntEditViewResult.MarkdownContentControl.SelectionStart = kntEditViewResult.MarkdownContentControl.Text.Length;
        kntEditViewResult.MarkdownContentControl.ScrollToCaret();
        textPrompt.Text = "";
        toolStripStatusLabelTokens.Text = $"Tokens: {_ctrl.TotalTokens} ";
        toolStripStatusLabelProcessingTime.Text = $" | Processing time: {_ctrl.TotalProcessingTime}";
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
            MessageBox.Show(ex.Message);
        }
    }

    private async void KNoteAIAssistantForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (!_viewFinalized)
        {
            if (_ctrl.AutoSaveChatMessagesOnViewExit && !string.IsNullOrEmpty(kntEditViewResult.MarkdownText))
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

            if (radioGetCompletion.Checked)
                await GoGetCompletion(textPrompt.Text);
            else
                await GoStreamCompletion(textPrompt.Text);

        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
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

    private async void buttonCatalogPrompts_Click(object sender, EventArgs e)
    {
        var assistantInfo = await _ctrl.GetCatalogPrompt();
        if (assistantInfo == null)
            return;

        RestartAIAssistantView();
        Text = $"{ViewCaptionText} - {assistantInfo.Name}";
        textPrompt.Text = assistantInfo.User;
    }

    private void buttonViewSystem_Click(object sender, EventArgs e)
    {
        ShowInfo($"System: {_ctrl.RootSystemChat}", $"{KntConst.AppName} - root system chat ");
    }

    private async void buttonManageProviders_Click(object sender, EventArgs e)
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

        if (!string.IsNullOrEmpty(_ctrl.ChatTextMessasges.ToString()))
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
        MarkDownView();
    }

    private async void buttonNavigate_Click(object sender, EventArgs e)
    {
        await NavigateView();
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
            await noteEditor.NewModel(_ctrl.Store.GetActiveOrDefaultService());
            noteEditor.Model.Topic = $"{DateTime.Now.ToString()}";
            noteEditor.Model.Description = _ctrl.ChatTextMessasges.ToString();
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
        kntEditViewResult.SetMarkdownContent(_ctrl.ChatTextMessasges.ToString());
        MarkDownView();
        _sbResult.Clear();
        textPrompt.Text = "";
        textPrompt.Focus();
    }

    private void StatusProcessing(bool processing = false)
    {
        if (processing)
        {
            MarkDownView();
            toolStripStatusLabelProcessing.Text = " Processing ...";
            textPrompt.Enabled = false;
            comboProviders.Enabled = false;
            buttonManageProviders.Enabled = false;
            buttonSend.Enabled = false;
            buttonRestart.Enabled = false;
            radioGetCompletion.Enabled = false;
            radioGetStream.Enabled = false;
            buttonCatalogPrompts.Enabled = false;
            buttonViewSystem.Enabled = false;
            buttonMarkDown.Enabled = false;
            buttonNavigate.Enabled = false;
        }
        else
        {
            toolStripStatusLabelProcessing.Text = " ";
            textPrompt.Enabled = true;
            comboProviders.Enabled = _ctrl.AiProviderRefs.Count > 0;
            buttonManageProviders.Enabled = true;
            buttonSend.Enabled = _ctrl.CurrentProviderRef != null;
            buttonRestart.Enabled = true;
            radioGetCompletion.Enabled = true;
            radioGetStream.Enabled = true;
            buttonCatalogPrompts.Enabled = true;
            buttonViewSystem.Enabled = true;
            // Every send leaves the result view in markdown mode (GoGetCompletion/GoStreamCompletion
            // never navigate to the rendered HTML view), so Markdown must stay disabled here - matching
            // what MarkDownView() already set - not force-reenabled like the rest of the controls.
            buttonMarkDown.Enabled = false;
            buttonNavigate.Enabled = true;
            kntEditViewResult.MarkdownContentControl.SelectionStart = kntEditViewResult.MarkdownContentControl.Text.Length;
            kntEditViewResult.MarkdownContentControl.ScrollToCaret();
            ActiveControl = textPrompt;
        }
    }

    private async Task GoGetCompletion(string prompt)
    {
        await _ctrl.GetCompletionAsync(prompt);
        RefreshView();
    }

    private async Task GoStreamCompletion(string prompt)
    {
        _sbResult.Clear();
        _countNRres = 0;
        _ctrl.StreamToken += _com_StreamToken;

        try
        {
            await _ctrl.StreamCompletionAsync(prompt);
        }
        finally
        {
            // Must run even if the stream throws mid-way (e.g. a transient SDK error on the
            // trailing chunk): otherwise this handler stays subscribed and the next attempt
            // fires two handlers at once, interleaving garbled text into the result view.
            _ctrl.StreamToken -= _com_StreamToken;
        }

        RefreshStreamResult();

        textPrompt.Text = "";
        toolStripStatusLabelTokens.Text = $"Tokens: {_ctrl.TotalTokens}";
        toolStripStatusLabelProcessingTime.Text = $" | Processing time: {_ctrl.TotalProcessingTime}";
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

    private void MarkDownView()
    {
        kntEditViewResult.ShowMarkdownContent();
        buttonMarkDown.Enabled = false;
        buttonNavigate.Enabled = true;
    }

    private async Task NavigateView()
    {
        var service = _ctrl.ServiceRef.Service;
        var content = kntEditViewResult.MarkdownText;
        var htmlContent = service.Notes.UtilMarkdownToHtml(content.Replace(service.RepositoryRef.ResourcesContainerRootUrl, KntConst.VirtualHostNameToFolderMapping));
        await kntEditViewResult.ShowNavigationContent(htmlContent + _ctrl.Store.KNoteWebViewStyle);
        buttonMarkDown.Enabled = true;
        buttonNavigate.Enabled = false;
    }

    #endregion
}
