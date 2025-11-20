using System.ComponentModel;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using MSDN.Html.Editor;

namespace KntWebView
{
    public partial class KntEditView : UserControl
    {
        #region Constructor

        public KntEditView()
        {
            InitializeComponent();
            InitializeEditorsComponent();
        }

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

            set { textUrl.Enabled = value; }
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


        private bool _htmlEditorEditMode;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HtmlEditorEditMode 
        {
            get { return _htmlEditorEditMode; }
            set 
            {
                _htmlEditorEditMode = value;
                if (_htmlEditorEditMode)
                {
                    htmlContent.ToolbarVisible = true;
                    htmlContent.ReadOnly = false;
                }
                else
                {
                    htmlContent.ToolbarVisible = false;
                    htmlContent.ReadOnly = true;
                }
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

        #endregion

        #region Form events managment 

        private async void KntEditView_Load(object sender, EventArgs e)
        {
            if (_isInitialized)
                return;                       
            
            await InitializeAsync();
        }

        private void webView2_NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            statusLabel.Text = webView.Source.ToString();
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

        private async void textUrl_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
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

            if (!_isInitialized)
                await InitializeAsync();
            
            FolderForVirtualHostNameMapping = folder;
            
            if (Directory.Exists(FolderForVirtualHostNameMapping))
            {
                webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "knote.resources", FolderForVirtualHostNameMapping,
                    CoreWebView2HostResourceAccessKind.Allow);
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
                if (!_isInitialized)
                    await InitializeAsync();

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
            webView.CoreWebView2.GoBack();
        }

        public void GoForward()
        {
            webView.CoreWebView2.GoForward();
        }

        #endregion

        #region Private methods

        private async Task InitializeAsync()
        {
            statusLabel.Text = "(Initializing ......)";

            await webView.EnsureCoreWebView2Async(null);

            if ((webView != null) && (webView.CoreWebView2 != null))
            {
                _isInitialized = true;
                webView.CoreWebView2InitializationCompleted += webView2_CoreWebView2InitializationCompleted;
                webView.NavigationStarting += EnsureHttps;
                webView.NavigationCompleted += webView2_NavigationCompleted;
            }
            else
            {
                _isInitialized = false;
            }
            statusLabel.Text = "";
        }

        private void InitializeEditorsComponent()
        {
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

        private void EnableMarkdownView()
        {
            webView.Visible = false;
            ShowNavigationTools = false;
            ShowStatusInfo = false;            
            htmlContent.Visible = false;
            textContent.Visible = true;            
        }

        private void EnableNavigationView()
        {
            textContent.Visible = false;
            htmlContent.Visible = false;            
            webView.Visible = true;            
        }
        
        private void EnableHtmlView()
        {
            textContent.Visible = false;
            ShowNavigationTools = false;
            ShowStatusInfo = false;
            webView.Visible = false;            
            htmlContent.Visible = true;            
        }

        private async Task Navigate()
        {
            try
            {
                if (!_isInitialized)
                    await InitializeAsync();

                if (webView.CoreWebView2 != null)  // This patch is required when using sql server repositories 
                    if(!string.IsNullOrEmpty(textUrl.Text))
                        webView.CoreWebView2.Navigate(textUrl.Text);
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

                if (!_isInitialized)
                    await InitializeAsync();

                if (webView.CoreWebView2 != null)
                    webView.NavigateToString(contentString);
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
