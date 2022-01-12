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
            this.panelToolBox = new System.Windows.Forms.Panel();
            this.btnForward = new System.Windows.Forms.Button();
            this.btnBack = new System.Windows.Forms.Button();
            this.btnNavigate = new System.Windows.Forms.Button();
            this.textUrl = new System.Windows.Forms.TextBox();
            this.panelWebView = new System.Windows.Forms.Panel();
            this.webView2 = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.panelToolBox.SuspendLayout();
            this.panelWebView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.webView2)).BeginInit();
            this.statusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelToolBox
            // 
            this.panelToolBox.Controls.Add(this.btnForward);
            this.panelToolBox.Controls.Add(this.btnBack);
            this.panelToolBox.Controls.Add(this.btnNavigate);
            this.panelToolBox.Controls.Add(this.textUrl);
            this.panelToolBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelToolBox.Location = new System.Drawing.Point(0, 0);
            this.panelToolBox.Name = "panelToolBox";
            this.panelToolBox.Size = new System.Drawing.Size(867, 30);
            this.panelToolBox.TabIndex = 13;
            // 
            // btnForward
            // 
            this.btnForward.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnForward.Image = ((System.Drawing.Image)(resources.GetObject("btnForward.Image")));
            this.btnForward.Location = new System.Drawing.Point(842, 2);
            this.btnForward.Name = "btnForward";
            this.btnForward.Size = new System.Drawing.Size(25, 25);
            this.btnForward.TabIndex = 3;
            this.btnForward.UseVisualStyleBackColor = true;
            this.btnForward.Click += new System.EventHandler(this.btnForward_Click);
            // 
            // btnBack
            // 
            this.btnBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBack.Image = ((System.Drawing.Image)(resources.GetObject("btnBack.Image")));
            this.btnBack.Location = new System.Drawing.Point(817, 2);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(25, 25);
            this.btnBack.TabIndex = 2;
            this.btnBack.UseVisualStyleBackColor = true;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // btnNavigate
            // 
            this.btnNavigate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNavigate.Image = ((System.Drawing.Image)(resources.GetObject("btnNavigate.Image")));
            this.btnNavigate.Location = new System.Drawing.Point(792, 2);
            this.btnNavigate.Name = "btnNavigate";
            this.btnNavigate.Size = new System.Drawing.Size(25, 25);
            this.btnNavigate.TabIndex = 1;
            this.btnNavigate.UseVisualStyleBackColor = true;
            this.btnNavigate.Click += new System.EventHandler(this.btnNavigate_Click);
            // 
            // textUrl
            // 
            this.textUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textUrl.Location = new System.Drawing.Point(1, 3);
            this.textUrl.Name = "textUrl";
            this.textUrl.Size = new System.Drawing.Size(786, 23);
            this.textUrl.TabIndex = 0;
            this.textUrl.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textUrl_KeyUp);
            // 
            // panelWebView
            // 
            this.panelWebView.Controls.Add(this.webView2);
            this.panelWebView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelWebView.Location = new System.Drawing.Point(0, 30);
            this.panelWebView.Name = "panelWebView";
            this.panelWebView.Size = new System.Drawing.Size(867, 493);
            this.panelWebView.TabIndex = 14;
            // 
            // webView2
            // 
            this.webView2.CreationProperties = null;
            this.webView2.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webView2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webView2.Location = new System.Drawing.Point(0, 0);
            this.webView2.Name = "webView2";
            this.webView2.Size = new System.Drawing.Size(867, 493);
            this.webView2.TabIndex = 1;
            this.webView2.ZoomFactor = 1D;
            // 
            // statusBar
            // 
            this.statusBar.BackColor = System.Drawing.SystemColors.Control;
            this.statusBar.GripMargin = new System.Windows.Forms.Padding(0);
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusBar.Location = new System.Drawing.Point(0, 523);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(867, 22);
            this.statusBar.SizingGrip = false;
            this.statusBar.TabIndex = 15;
            // 
            // statusLabel
            // 
            this.statusLabel.BackColor = System.Drawing.SystemColors.Control;
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // KWebView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelWebView);
            this.Controls.Add(this.panelToolBox);
            this.Controls.Add(this.statusBar);
            this.Name = "KWebView";
            this.Size = new System.Drawing.Size(867, 545);
            this.Load += new System.EventHandler(this.KNoteWebView_Load);
            this.panelToolBox.ResumeLayout(false);
            this.panelToolBox.PerformLayout();
            this.panelWebView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.webView2)).EndInit();
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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
