using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;

namespace KntWebView
{
    public partial class KntEditView : UserControl
    {
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

            set { textUrl.Text = value; }  // !!! private o sólo lectura
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
        public string FolderForVirtualHostNameMapping { get; private set; }

        private string _contentType = "navigation";
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ContentType {
            get { return _contentType; }
            set { _contentType = value; } 
        }

        #endregion

        #region Constructor

        public KntEditView()
        {
            InitializeComponent();
            InitializeEditorsComponent();
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

        // Public params:
        //Content
        //ResourcesContainer
        //ResourcesContainerRootPath
        //ResourcesContainerRootUrl

        // Private fields 
        //"knote.resources"
        //KntConst.VirtualHostNameToFolderMapping




        public async Task SetVirtualHostNameToFolderMapping(string folder)
        {
            if (webView.IsDisposed == true)
                return;

            if (!_isInitialized)
                await InitializeAsync();
            
            FolderForVirtualHostNameMapping = folder;
            
            webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "knote.resources", FolderForVirtualHostNameMapping,
                CoreWebView2HostResourceAccessKind.Allow);
        }
        
        public async Task Navigate(string url)
        {
            textUrl.Text = url;
            await Navigate();
        }

        public async Task NavigateToString(string contentString)
        {
            try
            {
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

        public async Task ClearWebView()
        {
            await NavigateToString(" ");
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
                    webView.CoreWebView2.Navigate(textUrl.Text);
            }
            catch
            {
                throw;
            }
        }


        #endregion
    }
}
