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
    public partial class KWebView : UserControl
    {        
        #region Constructor

        public KWebView()
        {
            InitializeComponent();
        }

        #endregion 

        #region Form events managment 

        private async void KNoteWebView_Load(object sender, EventArgs e)
        {
            if (_isInitialized)
                return;

            await InitializeAsync();           
        }

        private void webView2_NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            statusLabel.Text = webView2.Source.ToString();
        }

        private void webView2_CoreWebView2InitializationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs e)
        {
            statusLabel.Text = "";
        }

        void EnsureHttps(object? sender, CoreWebView2NavigationStartingEventArgs args)
        {
            if (!ForceHttps)
                return;

            String uri = args.Uri;
            if (!uri.StartsWith("https://"))
            {
                webView2.CoreWebView2.ExecuteScriptAsync($"alert('{uri} is not safe, try an https link')");
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

        #endregion

        #region Private methods

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

            set { panelToolBox.Visible = value; }
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
        public bool ForceHttps { get; set; } = false;

        #endregion

        #region Public methods

        public async Task InitializeAsync()
        {
            statusLabel.Text = "(Initializing ......)";

            await webView2.EnsureCoreWebView2Async(null);

            if ((webView2 != null) && (webView2.CoreWebView2 != null))
            {
                _isInitialized = true;
                webView2.CoreWebView2InitializationCompleted += webView2_CoreWebView2InitializationCompleted;
                webView2.NavigationStarting += EnsureHttps;
                webView2.NavigationCompleted += webView2_NavigationCompleted;
            }
            else
            {
                _isInitialized = false;                
            }
            statusLabel.Text = "";
        }

        public async Task Navigate()
        {
            try
            {
                if (!_isInitialized)
                    await InitializeAsync();

                if (webView2.CoreWebView2 != null)  // This patch is required when using sql server repositories 
                    webView2.CoreWebView2.Navigate(textUrl.Text);
            }
            catch
            {                
                throw;
            }
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

                if (webView2.CoreWebView2 != null)  // This patch is required when using sql server repositories
                    webView2.NavigateToString(contentString);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"You can not navigate to the indicated string. ({ex.Message})");
            }
        }

        public void GoBack()
        {
            webView2.CoreWebView2.GoBack();
        }

        public void GoForward()
        {
            webView2.CoreWebView2.GoForward();
        }

        #endregion
    }
}
