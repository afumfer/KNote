namespace KntWebView
{
    partial class KWebView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KWebView));
            panelToolBox = new Panel();
            btnForward = new Button();
            btnBack = new Button();
            btnNavigate = new Button();
            textUrl = new TextBox();
            panelWebView = new Panel();
            webView2 = new Microsoft.Web.WebView2.WinForms.WebView2();
            statusBar = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            panelToolBox.SuspendLayout();
            panelWebView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)webView2).BeginInit();
            statusBar.SuspendLayout();
            SuspendLayout();
            // 
            // panelToolBox
            // 
            panelToolBox.Controls.Add(btnForward);
            panelToolBox.Controls.Add(btnBack);
            panelToolBox.Controls.Add(btnNavigate);
            panelToolBox.Controls.Add(textUrl);
            panelToolBox.Dock = DockStyle.Top;
            panelToolBox.Location = new Point(0, 0);
            panelToolBox.Name = "panelToolBox";
            panelToolBox.Size = new Size(867, 30);
            panelToolBox.TabIndex = 13;
            // 
            // btnForward
            // 
            btnForward.Image = (Image)resources.GetObject("btnForward.Image");
            btnForward.Location = new Point(26, 2);
            btnForward.Name = "btnForward";
            btnForward.Size = new Size(25, 25);
            btnForward.TabIndex = 1;
            btnForward.UseVisualStyleBackColor = true;
            btnForward.Click += btnForward_Click;
            // 
            // btnBack
            // 
            btnBack.Image = (Image)resources.GetObject("btnBack.Image");
            btnBack.Location = new Point(1, 2);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(25, 25);
            btnBack.TabIndex = 0;
            btnBack.UseVisualStyleBackColor = true;
            btnBack.Click += btnBack_Click;
            // 
            // btnNavigate
            // 
            btnNavigate.Image = (Image)resources.GetObject("btnNavigate.Image");
            btnNavigate.Location = new Point(51, 2);
            btnNavigate.Name = "btnNavigate";
            btnNavigate.Size = new Size(25, 25);
            btnNavigate.TabIndex = 2;
            btnNavigate.UseVisualStyleBackColor = true;
            btnNavigate.Click += btnNavigate_Click;
            btnNavigate.MouseDown += btnNavigate_MouseDown;
            // 
            // textUrl
            // 
            textUrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textUrl.Location = new Point(77, 3);
            textUrl.Name = "textUrl";
            textUrl.Size = new Size(788, 23);
            textUrl.TabIndex = 3;
            textUrl.KeyUp += textUrl_KeyUp;
            // 
            // panelWebView
            // 
            panelWebView.BackColor = SystemColors.Control;
            panelWebView.Controls.Add(webView2);
            panelWebView.Dock = DockStyle.Fill;
            panelWebView.Location = new Point(0, 30);
            panelWebView.Name = "panelWebView";
            panelWebView.Size = new Size(867, 493);
            panelWebView.TabIndex = 14;
            // 
            // webView2
            // 
            webView2.AllowExternalDrop = true;
            webView2.CreationProperties = null;
            webView2.DefaultBackgroundColor = Color.White;
            webView2.Dock = DockStyle.Fill;
            webView2.Location = new Point(0, 0);
            webView2.Name = "webView2";
            webView2.Size = new Size(867, 493);
            webView2.TabIndex = 5;
            webView2.ZoomFactor = 1D;
            // 
            // statusBar
            // 
            statusBar.BackColor = SystemColors.Control;
            statusBar.GripMargin = new Padding(0);
            statusBar.Items.AddRange(new ToolStripItem[] { statusLabel });
            statusBar.Location = new Point(0, 523);
            statusBar.Name = "statusBar";
            statusBar.Size = new Size(867, 22);
            statusBar.SizingGrip = false;
            statusBar.TabIndex = 15;
            // 
            // statusLabel
            // 
            statusLabel.BackColor = SystemColors.Control;
            statusLabel.ForeColor = SystemColors.ControlDarkDark;
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(0, 17);
            // 
            // KWebView
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(panelWebView);
            Controls.Add(panelToolBox);
            Controls.Add(statusBar);
            Name = "KWebView";
            Size = new Size(867, 545);
            Load += KNoteWebView_Load;
            panelToolBox.ResumeLayout(false);
            panelToolBox.PerformLayout();
            panelWebView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)webView2).EndInit();
            statusBar.ResumeLayout(false);
            statusBar.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Panel panelToolBox;
        private Button btnForward;
        private Button btnBack;
        private Button btnNavigate;
        private TextBox textUrl;
        private Panel panelWebView;
        private Panel panelStatus;
        private Label statusInfo;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView2;
        private StatusStrip statusBar;
        private ToolStripStatusLabel statusLabel;
    }
}
