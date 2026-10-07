using System.ComponentModel;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using MSDN.Html.Editor;
using KntIcons;

namespace KntWebView
{
    public partial class KntEditView : UserControl
    {
        #region Constructor

        public KntEditView()
        {
            InitializeComponent();
            InitializeEditorsComponent();

            btnBack.SetKntIcon(KntIcon.Back);
            btnForward.SetKntIcon(KntIcon.Forward);
            btnNavigate.SetKntIcon(KntIcon.Refresh);
        }

        #endregion

        #region Static configuration

        /// <summary>
        /// Folder WebView2 uses to store its browser profile/cache (CoreWebView2Environment's
        /// userDataFolder). Host applications should set this once at startup, before the first
        /// KntEditView is shown, to keep it out of the app's binaries folder. Falls back to the
        /// historical default (next to the executable) if left unset.
        /// </summary>
        public static string? WebView2UserDataFolder { get; set; }

        /// <summary>
        /// Raised when a WebView2 process (browser, renderer, GPU, ...) of any KntEditView fails.
        /// Static for the same reason as WebView2UserDataFolder: the host application subscribes
        /// once at startup (e.g. to log it) instead of having to reach every KntEditView instance.
        /// </summary>
        public static event EventHandler<CoreWebView2ProcessFailedEventArgs>? WebView2ProcessFailed;

        /// <summary>
        /// Raised when a WebView2 operation of any KntEditView (initialization, virtual host
        /// mapping, ...) fails. The failure is contained here - the content area is just left empty -
        /// instead of escaping to the caller, so the host only needs to log it.
        /// </summary>
        public static event EventHandler<Exception>? WebView2ErrorOccurred;

        #endregion

        #region Public properties

        private bool _isInitialized = false;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsInitialized
        {
            get { return _isInitialized; }

            set { _isInitialized = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string TextUrl
        {
            get { return textUrl.Text; }

            set { textUrl.Text = value; }  
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowNavigationTools
        {
            get { return panelToolBox.Visible; }

            set { panelToolBox.Visible = value; statusBar.Visible = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool EnableUrlBox
        {
            get { return textUrl.Enabled; }

            set
            {
                textUrl.Enabled = value;
                // The box around the text (panelUrl) follows the text box's own enabled/disabled background.
                panelUrl.BackColor = value ? SystemColors.Window : SystemColors.Control;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowStatusInfo
        {
            get { return statusBar.Visible; }
            set { statusBar.Visible = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string StatusInfoBackcolor
        {
            get { return ColorTranslator.ToHtml(statusBar.BackColor); }
            set { statusBar.BackColor = ColorTranslator.FromHtml(value); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ForceHttps { get; set; } = false;

        private bool _navigationBorder = false;
        /// <summary>
        /// Draws a thin border around the web content of the navigation view, which has none of its
        /// own (unlike the text box of the markdown view).
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool NavigationBorder
        {
            get { return _navigationBorder; }
            set { _navigationBorder = value; ApplyNavigationBorder(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string? FolderForVirtualHostNameMapping { get; private set; }

        private string _contentType = "navigation";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ContentType {
            get { return _contentType; }
            set 
            { 
                _contentType = value; 

                if(_contentType == "markdown")
                {
                    EnableMarkdownView();
                }
                else if (_contentType == "navigation")
                {
                    EnableNavigationView();                    
                }
                else if (_contentType == "html")
                {
                    EnableHtmlView();
                }
            } 
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string BodyHtml
        {
            get { return htmlContent.BodyHtml; }            
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string? MarkdownText
        {
            get { return textContent.Text; }
        }


        // Defaults to editable: HtmlEditorEditMode is historically only ever set to false (to force
        // a read-only embedded note view); nothing sets it to true, relying on this default instead.
        private bool _htmlEditorEditMode = true;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HtmlEditorEditMode
        {
            get { return _htmlEditorEditMode; }
            set
            {
                _htmlEditorEditMode = value;
                ApplyHtmlEditorState();
            }
        }

        private bool _contentLocked;
        /// <summary>
        /// When true, the markdown/html sub-controls become read-only regardless of
        /// HtmlEditorEditMode, independently of which content mode (markdown/html/navigation) is shown.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ContentLocked
        {
            get { return _contentLocked; }
            set
            {
                _contentLocked = value;
                textContent.ReadOnly = _contentLocked;
                ApplyHtmlEditorState();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public HtmlEditorControl HtmlContentControl
        {
            get { return htmlContent; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TextBox MarkdownContentControl 
        {
            get { return textContent; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public WebView2 WebViewControl
        {
            get { return webView; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Panel NavigationToolBox
        {
            get { return panelToolBox; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TextBox UrlTextBox
        {
            get { return textUrl; }
        }

        // The URL box as it is seen: the bordered box (as tall as the navigation buttons) that holds the URL
        // text box. What lays something out over the URL bar measures this one, not UrlTextBox.
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control UrlBox
        {
            get { return panelUrl; }
        }

        #endregion
        
        public event EventHandler NavigationStart;

        public event EventHandler NavigationEnd;        

        #region Form events management 

        private async void KntEditView_Load(object sender, EventArgs e)
        {
            try
            {
                await EnsureInitializedAsync();
            }
            catch (Exception ex)
            {
                WebView2ErrorOccurred?.Invoke(this, ex);
            }
        }

        private void webView2_NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            statusLabel.Text = webView.Source?.ToString() ?? "";
            NavigationEnd?.Invoke(this, new EventArgs());
        }

        private void CoreWebView2_ProcessFailed(object? sender, CoreWebView2ProcessFailedEventArgs e)
        {
            WebView2ProcessFailed?.Invoke(this, e);
        }

        private void webView2_CoreWebView2InitializationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            statusLabel.Text = "";
        }

        private void EnsureHttps(object? sender, CoreWebView2NavigationStartingEventArgs args)
        {
            if (!ForceHttps)
                return;

            String uri = args.Uri;
            if (!uri.StartsWith("https://"))
            {
                webView.CoreWebView2.ExecuteScriptAsync($"alert('{uri} is not safe, try an https link')");
                args.Cancel = true;
            }
        }

        // A single-line TextBox can't be made taller with its text centered: the URL box is panelUrl, as tall
        // as the navigation buttons beside it, with a borderless textUrl centered vertically inside it.
        private void panelUrl_Layout(object sender, LayoutEventArgs e)
        {
            int margin = panelUrl.LogicalToDeviceUnits(3);
            textUrl.SetBounds(margin, (panelUrl.ClientSize.Height - textUrl.Height) / 2,
                Math.Max(0, panelUrl.ClientSize.Width - 2 * margin), textUrl.Height);
        }

        // Same 1px border as a text box, in a system color, so it fits the light and dark modes alike.
        private void panelUrl_Paint(object sender, PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, panelUrl.ClientRectangle, SystemColors.ControlDark, ButtonBorderStyle.Solid);
        }

        // A click on the box around the text works like a click on the text box itself.
        private void panelUrl_Click(object sender, EventArgs e)
        {
            textUrl.Focus();
        }

        private async void textUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;
            // A single-line text box beeps on Enter.
            e.SuppressKeyPress = true;
            await Navigate();
        }

        private async void btnNavigate_Click(object sender, EventArgs e)
        {
            await Navigate();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            GoBack();
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            GoForward();
        }

        private void btnNavigate_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                statusBar.Visible = !statusBar.Visible;
            }
        }

        #endregion

        #region Public methods
        
        public void SetMarkdownContent(string content)
        {            
            textContent.Text = content;            
        }

        public void ShowMarkdownContent(string? content = null)
        {            
            if(content != null)
                textContent.Text = content;
            ContentType = "markdown";            
        }

        public async Task ShowNavigationContent(string content)
        {
            ShowNavigationTools = false;
            ShowStatusInfo = false;
            await NavigateToString(content);            
            ContentType = "navigation";
        }

        public async Task ShowNavigationUrlContent(string content)
        {
            ShowStatusInfo = false;
            await Navigate(content);            
            ContentType = "navigation";            
        }

        public void ShowHtmlContent(string content)
        {            
            htmlContent.BodyHtml = "";
            htmlContent.BodyHtml = content;            
            ContentType = "html";
            htmlContent.Refresh();
        }

        public async Task SetVirtualHostNameToFolderMapping(string folder)
        {
            if (webView.IsDisposed == true)
                return;

            await EnsureInitializedAsync();

            FolderForVirtualHostNameMapping = folder;

            // Null when WebView2 could not be initialized (see InitializeAsync).
            if (webView.CoreWebView2 == null)
                return;

            if (Directory.Exists(FolderForVirtualHostNameMapping))
            {
                try
                {
                    webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                        "knote.resources", FolderForVirtualHostNameMapping,
                        CoreWebView2HostResourceAccessKind.Allow);
                }
                catch (Exception ex)
                {
                    // E.g. its browser process has just failed (see WebView2ProcessFailed).
                    WebView2ErrorOccurred?.Invoke(this, ex);
                }
            }
        }
        
        public async Task ClearWebView()
        {
            await NavigateToString(" ");
            textContent.Text = "";
            htmlContent.BodyHtml = "";
        }

        public async Task ExecuteScriptAsync(string script)
        {
            try
            {
                await EnsureInitializedAsync();

                if (webView.CoreWebView2 != null)
                    await webView.CoreWebView2.ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"You can not execute to the indicated script. ({ex.Message})");
            }
        }

        public void GoBack()
        {
            webView.CoreWebView2?.GoBack();
        }

        public void GoForward()
        {
            webView.CoreWebView2?.GoForward();
        }

        #endregion

        #region Private methods

        // Caches the in-flight/completed initialization so concurrent callers (e.g. the control's
        // Load event racing with an explicit SetVirtualHostNameToFolderMapping/Navigate call before
        // _isInitialized flips to true) await the same task instead of each creating their own
        // CoreWebView2Environment, which WebView2 rejects with "already initialized with a
        // different CoreWebView2Environment".
        private Task? _initializationTask;

        // A completed initialization that didn't succeed is started again on a later call. Checked
        // here rather than by clearing _initializationTask from InitializeAsync itself, which would
        // be undone by the "??=" assignment if InitializeAsync ever completed synchronously.
        private Task EnsureInitializedAsync()
        {
            if (_initializationTask == null || (_initializationTask.IsCompleted && !_isInitialized))
                _initializationTask = InitializeAsync();
            return _initializationTask;
        }

        // Never throws: a failure (WebView2 runtime being updated, browser process failing to
        // start under heavy load, control disposed meanwhile, ...) is reported through
        // WebView2ErrorOccurred and leaves CoreWebView2 null, which every caller already checks.
        private async Task InitializeAsync()
        {
            statusLabel.Text = "(Initializing ......)";

            try
            {
                // WebView2's default UserDataFolder is derived from the hosting process's exe path.
                // When launched via "dotnet exec" (e.g. VS Code's coreclr debugger), the host is
                // dotnet.exe under Program Files, which is not writable, causing E_ACCESSDENIED.
                // Pinning an explicit folder avoids that regardless of how it was launched. Prefer the
                // host-configured WebView2UserDataFolder (set by ClientWin's Program.cs to a per-user
                // AppData folder) so the cache doesn't pile up next to the application binaries; fall
                // back to the historical location if the host never set it.
                var userDataFolder = WebView2UserDataFolder ?? Path.Combine(Application.StartupPath, "WebView2Cache");
                var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: userDataFolder);
                await webView.EnsureCoreWebView2Async(environment);
            }
            catch (Exception ex)
            {
                WebView2ErrorOccurred?.Invoke(this, ex);
            }

            if ((webView != null) && (webView.CoreWebView2 != null))
            {
                _isInitialized = true;
                webView.CoreWebView2InitializationCompleted += webView2_CoreWebView2InitializationCompleted;
                webView.NavigationStarting += EnsureHttps;
                webView.NavigationCompleted += webView2_NavigationCompleted;
                webView.CoreWebView2.ProcessFailed += CoreWebView2_ProcessFailed;

                // Left at its default (true): callers (NoteEditorForm/PostItEditorForm) no longer
                // wire WinForms DragDrop on WebViewControl, so the browser handles OS drops itself
                // as it normally would.

                // The pages' "prefers-color-scheme" follows the app's color mode, not Windows' (its
                // default): the host's style sheet (KNoteWebViewStyle.css in ClientWin) has a dark variant.
                webView.CoreWebView2.Profile.PreferredColorScheme = Application.IsDarkModeEnabled
                    ? CoreWebView2PreferredColorScheme.Dark
                    : CoreWebView2PreferredColorScheme.Light;
            }
            else
            {
                _isInitialized = false; // allows a retry on a later call (see EnsureInitializedAsync)
            }
            statusLabel.Text = "";
        }

        private void InitializeEditorsComponent()
        {
            // Shown until the first page is painted: dark in dark mode, so there is no white flash. Same
            // color as the dark variant of the host's style sheet.
            if (Application.IsDarkModeEnabled)
                webView.DefaultBackgroundColor = Color.FromArgb(0x1F, 0x1F, 0x1F);

            webView.Dock = DockStyle.Fill;
            textContent.Dock = DockStyle.Fill;
            htmlContent.Dock = DockStyle.Fill;

            if (_contentType.Contains("navigation"))
                EnableNavigationView();
            else if (_contentType.Contains("markdown"))
                EnableMarkdownView();
            else if (_contentType.Contains("html"))
                EnableHtmlView();
        }

        private void ApplyHtmlEditorState()
        {
            bool editable = _htmlEditorEditMode && !_contentLocked;
            htmlContent.ToolbarVisible = editable;
            htmlContent.ReadOnly = !editable;
        }

        private void EnableMarkdownView()
        {
            NavigationEnd?.Invoke(this, new EventArgs());
            webView.Visible = false;
            ShowNavigationTools = false;
            ShowStatusInfo = false;            
            htmlContent.Visible = false;
            textContent.Visible = true;
            ApplyNavigationBorder();
        }

        private void EnableNavigationView()
        {
            textContent.Visible = false;
            htmlContent.Visible = false;
            webView.Visible = true;
            ApplyNavigationBorder();
        }

        private void EnableHtmlView()
        {
            NavigationEnd?.Invoke(this, new EventArgs());
            textContent.Visible = false;
            ShowNavigationTools = false;
            ShowStatusInfo = false;
            webView.Visible = false;
            htmlContent.Visible = true;
            ApplyNavigationBorder();
        }

        // The border is the panel's background showing through a 1 px padding around the web view: the
        // same discreet color as the URL box's border, in light and dark mode. Only in the navigation view,
        // so the other views' own borders aren't doubled.
        private void ApplyNavigationBorder()
        {
            var border = _navigationBorder && _contentType == "navigation";
            panelWebView.Padding = border ? new Padding(1) : Padding.Empty;
            panelWebView.BackColor = border ? SystemColors.ControlDark : SystemColors.Control;
        }

        private async Task Navigate()
        {
            try
            {
                await EnsureInitializedAsync();

                if (webView.CoreWebView2 != null)  // This patch is required when using sql server repositories
                    if (!string.IsNullOrEmpty(textUrl.Text))
                    {
                        NavigationStart?.Invoke(this, new EventArgs());
                        webView.CoreWebView2.Navigate(textUrl.Text);
                    }
            }
            catch
            {
                // TODO: hack, for test in slow local network 
                //throw;
            }
        }

        private async Task NavigateToString(string contentString)
        {
            try
            {
                TextUrl = "";

                await EnsureInitializedAsync();

                if (webView.CoreWebView2 != null)
                {
                    NavigationStart?.Invoke(this, new EventArgs());
                    webView.NavigateToString(contentString);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"You can not navigate to the indicated string. ({ex.Message})");
            }
        }

        private async Task Navigate(string url)
        {
            textUrl.Text = url;
            await Navigate();
        }

        #endregion
    }
}
