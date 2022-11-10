namespace KNote.ClientWin.Views
{
    partial class LabForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LabForm));
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.tabWebView2 = new System.Windows.Forms.TabPage();
            this.webView2 = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.btnGoBack = new System.Windows.Forms.Button();
            this.btnNavigate = new System.Windows.Forms.Button();
            this.textStatusWebView2 = new System.Windows.Forms.TextBox();
            this.textUrlWebView2 = new System.Windows.Forms.TextBox();
            this.tabRichEditor = new System.Windows.Forms.TabPage();
            this.htmlDescription = new Pavonis.Html.Editor.HtmlEditorControl();
            this.tabKntScriptLab = new System.Windows.Forms.TabPage();
            this.groupSamples = new System.Windows.Forms.GroupBox();
            this.buttonSelectScriptDirectory = new System.Windows.Forms.Button();
            this.buttonRunSample = new System.Windows.Forms.Button();
            this.buttonShowSample = new System.Windows.Forms.Button();
            this.listSamples = new System.Windows.Forms.ListBox();
            this.buttonRunBackground = new System.Windows.Forms.Button();
            this.buttonInteract = new System.Windows.Forms.Button();
            this.buttonShowConsole = new System.Windows.Forms.Button();
            this.buttonRunScript = new System.Windows.Forms.Button();
            this.tabAppLab = new System.Windows.Forms.TabPage();
            this.buttonGetPluginFile = new System.Windows.Forms.Button();
            this.textPlugin = new System.Windows.Forms.TextBox();
            this.buttonPlugin = new System.Windows.Forms.Button();
            this.buttonTestReflection = new System.Windows.Forms.Button();
            this.buttonTestProcessStart = new System.Windows.Forms.Button();
            this.buttonTestReadVarItem = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.listMessages = new System.Windows.Forms.ListBox();
            this.buttonImportAnotasXML = new System.Windows.Forms.Button();
            this.buttonRunMonitor = new System.Windows.Forms.Button();
            this.tabControlLab = new System.Windows.Forms.TabControl();
            this.tabWebView2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.webView2)).BeginInit();
            this.tabRichEditor.SuspendLayout();
            this.tabKntScriptLab.SuspendLayout();
            this.groupSamples.SuspendLayout();
            this.tabAppLab.SuspendLayout();
            this.tabControlLab.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabWebView2
            // 
            this.tabWebView2.Controls.Add(this.webView2);
            this.tabWebView2.Controls.Add(this.btnGoBack);
            this.tabWebView2.Controls.Add(this.btnNavigate);
            this.tabWebView2.Controls.Add(this.textStatusWebView2);
            this.tabWebView2.Controls.Add(this.textUrlWebView2);
            this.tabWebView2.Location = new System.Drawing.Point(4, 24);
            this.tabWebView2.Name = "tabWebView2";
            this.tabWebView2.Size = new System.Drawing.Size(662, 549);
            this.tabWebView2.TabIndex = 3;
            this.tabWebView2.Text = "WebView2";
            this.tabWebView2.UseVisualStyleBackColor = true;
            // 
            // webView2
            // 
            this.webView2.AllowExternalDrop = true;
            this.webView2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webView2.CreationProperties = null;
            this.webView2.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webView2.Location = new System.Drawing.Point(3, 32);
            this.webView2.Name = "webView2";
            this.webView2.Size = new System.Drawing.Size(656, 485);
            this.webView2.TabIndex = 4;
            this.webView2.ZoomFactor = 1D;
            // 
            // btnGoBack
            // 
            this.btnGoBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGoBack.Location = new System.Drawing.Point(585, 3);
            this.btnGoBack.Name = "btnGoBack";
            this.btnGoBack.Size = new System.Drawing.Size(74, 23);
            this.btnGoBack.TabIndex = 3;
            this.btnGoBack.Text = "Go back";
            this.btnGoBack.UseVisualStyleBackColor = true;
            this.btnGoBack.Click += new System.EventHandler(this.btnGoBack_Click);
            // 
            // btnNavigate
            // 
            this.btnNavigate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNavigate.Location = new System.Drawing.Point(505, 3);
            this.btnNavigate.Name = "btnNavigate";
            this.btnNavigate.Size = new System.Drawing.Size(74, 23);
            this.btnNavigate.TabIndex = 2;
            this.btnNavigate.Text = "Navigate";
            this.btnNavigate.UseVisualStyleBackColor = true;
            this.btnNavigate.Click += new System.EventHandler(this.btnNavigate_Click);
            // 
            // textStatusWebView2
            // 
            this.textStatusWebView2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textStatusWebView2.Location = new System.Drawing.Point(3, 523);
            this.textStatusWebView2.Name = "textStatusWebView2";
            this.textStatusWebView2.Size = new System.Drawing.Size(656, 23);
            this.textStatusWebView2.TabIndex = 1;
            // 
            // textUrlWebView2
            // 
            this.textUrlWebView2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textUrlWebView2.Location = new System.Drawing.Point(3, 3);
            this.textUrlWebView2.Name = "textUrlWebView2";
            this.textUrlWebView2.Size = new System.Drawing.Size(496, 23);
            this.textUrlWebView2.TabIndex = 0;
            this.textUrlWebView2.Text = "https://www.gobiernodecanarias.org/educacion/9/pekweb/ekade";
            // 
            // tabRichEditor
            // 
            this.tabRichEditor.Controls.Add(this.htmlDescription);
            this.tabRichEditor.Location = new System.Drawing.Point(4, 24);
            this.tabRichEditor.Name = "tabRichEditor";
            this.tabRichEditor.Size = new System.Drawing.Size(662, 549);
            this.tabRichEditor.TabIndex = 2;
            this.tabRichEditor.Text = "Test rich editor";
            this.tabRichEditor.UseVisualStyleBackColor = true;
            // 
            // htmlDescription
            // 
            this.htmlDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.htmlDescription.InnerText = null;
            this.htmlDescription.Location = new System.Drawing.Point(14, 15);
            this.htmlDescription.Name = "htmlDescription";
            this.htmlDescription.Size = new System.Drawing.Size(634, 520);
            this.htmlDescription.TabIndex = 10;
            // 
            // tabKntScriptLab
            // 
            this.tabKntScriptLab.Controls.Add(this.groupSamples);
            this.tabKntScriptLab.Controls.Add(this.buttonRunBackground);
            this.tabKntScriptLab.Controls.Add(this.buttonInteract);
            this.tabKntScriptLab.Controls.Add(this.buttonShowConsole);
            this.tabKntScriptLab.Controls.Add(this.buttonRunScript);
            this.tabKntScriptLab.Location = new System.Drawing.Point(4, 24);
            this.tabKntScriptLab.Name = "tabKntScriptLab";
            this.tabKntScriptLab.Padding = new System.Windows.Forms.Padding(3);
            this.tabKntScriptLab.Size = new System.Drawing.Size(662, 549);
            this.tabKntScriptLab.TabIndex = 1;
            this.tabKntScriptLab.Text = "KntScript lab";
            this.tabKntScriptLab.UseVisualStyleBackColor = true;
            // 
            // groupSamples
            // 
            this.groupSamples.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupSamples.Controls.Add(this.buttonSelectScriptDirectory);
            this.groupSamples.Controls.Add(this.buttonRunSample);
            this.groupSamples.Controls.Add(this.buttonShowSample);
            this.groupSamples.Controls.Add(this.listSamples);
            this.groupSamples.Location = new System.Drawing.Point(16, 154);
            this.groupSamples.Name = "groupSamples";
            this.groupSamples.Size = new System.Drawing.Size(476, 379);
            this.groupSamples.TabIndex = 8;
            this.groupSamples.TabStop = false;
            this.groupSamples.Text = "Samples";
            // 
            // buttonSelectScriptDirectory
            // 
            this.buttonSelectScriptDirectory.Location = new System.Drawing.Point(15, 21);
            this.buttonSelectScriptDirectory.Name = "buttonSelectScriptDirectory";
            this.buttonSelectScriptDirectory.Size = new System.Drawing.Size(157, 27);
            this.buttonSelectScriptDirectory.TabIndex = 6;
            this.buttonSelectScriptDirectory.Text = "Select script directory";
            this.buttonSelectScriptDirectory.UseVisualStyleBackColor = true;
            this.buttonSelectScriptDirectory.Click += new System.EventHandler(this.buttonSelectScriptDirectory_Click);
            // 
            // buttonRunSample
            // 
            this.buttonRunSample.Location = new System.Drawing.Point(297, 101);
            this.buttonRunSample.Name = "buttonRunSample";
            this.buttonRunSample.Size = new System.Drawing.Size(173, 30);
            this.buttonRunSample.TabIndex = 5;
            this.buttonRunSample.Text = "Run sample";
            this.buttonRunSample.UseVisualStyleBackColor = true;
            this.buttonRunSample.Click += new System.EventHandler(this.buttonRunSample_Click);
            // 
            // buttonShowSample
            // 
            this.buttonShowSample.Location = new System.Drawing.Point(297, 64);
            this.buttonShowSample.Name = "buttonShowSample";
            this.buttonShowSample.Size = new System.Drawing.Size(173, 31);
            this.buttonShowSample.TabIndex = 4;
            this.buttonShowSample.Text = "Show sample in KntConsole";
            this.buttonShowSample.UseVisualStyleBackColor = true;
            this.buttonShowSample.Click += new System.EventHandler(this.buttonShowSample_Click);
            // 
            // listSamples
            // 
            this.listSamples.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listSamples.FormattingEnabled = true;
            this.listSamples.ItemHeight = 15;
            this.listSamples.Location = new System.Drawing.Point(13, 64);
            this.listSamples.Name = "listSamples";
            this.listSamples.Size = new System.Drawing.Size(269, 304);
            this.listSamples.TabIndex = 2;
            this.listSamples.SelectedIndexChanged += new System.EventHandler(this.listSamples_SelectedIndexChanged);
            // 
            // buttonRunBackground
            // 
            this.buttonRunBackground.Location = new System.Drawing.Point(16, 82);
            this.buttonRunBackground.Name = "buttonRunBackground";
            this.buttonRunBackground.Size = new System.Drawing.Size(282, 31);
            this.buttonRunBackground.TabIndex = 6;
            this.buttonRunBackground.Text = "Run simple script in bakground";
            this.buttonRunBackground.UseVisualStyleBackColor = true;
            this.buttonRunBackground.Click += new System.EventHandler(this.buttonRunBackground_Click);
            // 
            // buttonInteract
            // 
            this.buttonInteract.Location = new System.Drawing.Point(16, 48);
            this.buttonInteract.Name = "buttonInteract";
            this.buttonInteract.Size = new System.Drawing.Size(282, 28);
            this.buttonInteract.TabIndex = 5;
            this.buttonInteract.Text = "Interacting with kntscript (embedded code)";
            this.buttonInteract.UseVisualStyleBackColor = true;
            this.buttonInteract.Click += new System.EventHandler(this.buttonInteract_Click);
            // 
            // buttonShowConsole
            // 
            this.buttonShowConsole.Location = new System.Drawing.Point(16, 119);
            this.buttonShowConsole.Name = "buttonShowConsole";
            this.buttonShowConsole.Size = new System.Drawing.Size(282, 29);
            this.buttonShowConsole.TabIndex = 7;
            this.buttonShowConsole.Text = "Show KntConsole";
            this.buttonShowConsole.UseVisualStyleBackColor = true;
            this.buttonShowConsole.Click += new System.EventHandler(this.buttonShowConsole_Click);
            // 
            // buttonRunScript
            // 
            this.buttonRunScript.Location = new System.Drawing.Point(16, 14);
            this.buttonRunScript.Name = "buttonRunScript";
            this.buttonRunScript.Size = new System.Drawing.Size(282, 28);
            this.buttonRunScript.TabIndex = 4;
            this.buttonRunScript.Text = "Run simple script (embedded code)";
            this.buttonRunScript.UseVisualStyleBackColor = true;
            this.buttonRunScript.Click += new System.EventHandler(this.buttonRunScript_Click);
            // 
            // tabAppLab
            // 
            this.tabAppLab.Controls.Add(this.buttonGetPluginFile);
            this.tabAppLab.Controls.Add(this.textPlugin);
            this.tabAppLab.Controls.Add(this.buttonPlugin);
            this.tabAppLab.Controls.Add(this.buttonTestReflection);
            this.tabAppLab.Controls.Add(this.buttonTestProcessStart);
            this.tabAppLab.Controls.Add(this.buttonTestReadVarItem);
            this.tabAppLab.Controls.Add(this.label2);
            this.tabAppLab.Controls.Add(this.label1);
            this.tabAppLab.Controls.Add(this.listMessages);
            this.tabAppLab.Controls.Add(this.buttonImportAnotasXML);
            this.tabAppLab.Controls.Add(this.buttonRunMonitor);
            this.tabAppLab.Location = new System.Drawing.Point(4, 24);
            this.tabAppLab.Name = "tabAppLab";
            this.tabAppLab.Padding = new System.Windows.Forms.Padding(3);
            this.tabAppLab.Size = new System.Drawing.Size(662, 549);
            this.tabAppLab.TabIndex = 0;
            this.tabAppLab.Text = "Lab app components";
            this.tabAppLab.UseVisualStyleBackColor = true;
            // 
            // buttonGetPluginFile
            // 
            this.buttonGetPluginFile.Location = new System.Drawing.Point(607, 15);
            this.buttonGetPluginFile.Name = "buttonGetPluginFile";
            this.buttonGetPluginFile.Size = new System.Drawing.Size(39, 23);
            this.buttonGetPluginFile.TabIndex = 14;
            this.buttonGetPluginFile.Text = "<-";
            this.buttonGetPluginFile.UseVisualStyleBackColor = true;
            this.buttonGetPluginFile.Click += new System.EventHandler(this.buttonGetPluginFile_Click);
            // 
            // textPlugin
            // 
            this.textPlugin.Location = new System.Drawing.Point(352, 15);
            this.textPlugin.Name = "textPlugin";
            this.textPlugin.Size = new System.Drawing.Size(247, 23);
            this.textPlugin.TabIndex = 13;
            // 
            // buttonPlugin
            // 
            this.buttonPlugin.Location = new System.Drawing.Point(352, 51);
            this.buttonPlugin.Name = "buttonPlugin";
            this.buttonPlugin.Size = new System.Drawing.Size(297, 28);
            this.buttonPlugin.TabIndex = 12;
            this.buttonPlugin.Text = "Test Plugin";
            this.buttonPlugin.UseVisualStyleBackColor = true;
            this.buttonPlugin.Click += new System.EventHandler(this.buttonPlugin_Click);
            // 
            // buttonTestReflection
            // 
            this.buttonTestReflection.Location = new System.Drawing.Point(14, 164);
            this.buttonTestReflection.Name = "buttonTestReflection";
            this.buttonTestReflection.Size = new System.Drawing.Size(296, 26);
            this.buttonTestReflection.TabIndex = 11;
            this.buttonTestReflection.Text = "Test Reflection Model Objects";
            this.buttonTestReflection.UseVisualStyleBackColor = true;
            this.buttonTestReflection.Click += new System.EventHandler(this.buttonReflection_Click);
            // 
            // buttonTestProcessStart
            // 
            this.buttonTestProcessStart.Location = new System.Drawing.Point(14, 133);
            this.buttonTestProcessStart.Name = "buttonTestProcessStart";
            this.buttonTestProcessStart.Size = new System.Drawing.Size(296, 25);
            this.buttonTestProcessStart.TabIndex = 10;
            this.buttonTestProcessStart.Text = "Test ProcessStart";
            this.buttonTestProcessStart.UseVisualStyleBackColor = true;
            this.buttonTestProcessStart.Click += new System.EventHandler(this.buttonProcessStart_Click);
            // 
            // buttonTestReadVarItem
            // 
            this.buttonTestReadVarItem.Location = new System.Drawing.Point(14, 101);
            this.buttonTestReadVarItem.Name = "buttonTestReadVarItem";
            this.buttonTestReadVarItem.Size = new System.Drawing.Size(296, 26);
            this.buttonTestReadVarItem.TabIndex = 9;
            this.buttonTestReadVarItem.Text = "Test ReadVarItem";
            this.buttonTestReadVarItem.UseVisualStyleBackColor = true;
            this.buttonTestReadVarItem.Click += new System.EventHandler(this.buttonReadVar_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 233);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 15);
            this.label2.TabIndex = 8;
            this.label2.Text = "label2";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 218);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 15);
            this.label1.TabIndex = 7;
            this.label1.Text = "label1";
            // 
            // listMessages
            // 
            this.listMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listMessages.FormattingEnabled = true;
            this.listMessages.ItemHeight = 15;
            this.listMessages.Location = new System.Drawing.Point(13, 254);
            this.listMessages.Name = "listMessages";
            this.listMessages.Size = new System.Drawing.Size(636, 274);
            this.listMessages.TabIndex = 6;
            // 
            // buttonImportAnotasXML
            // 
            this.buttonImportAnotasXML.Location = new System.Drawing.Point(12, 51);
            this.buttonImportAnotasXML.Name = "buttonImportAnotasXML";
            this.buttonImportAnotasXML.Size = new System.Drawing.Size(298, 28);
            this.buttonImportAnotasXML.TabIndex = 5;
            this.buttonImportAnotasXML.Text = "Import anotas xml";
            this.buttonImportAnotasXML.UseVisualStyleBackColor = true;
            this.buttonImportAnotasXML.Click += new System.EventHandler(this.buttonImportAnotasXML_Click);
            // 
            // buttonRunMonitor
            // 
            this.buttonRunMonitor.Location = new System.Drawing.Point(13, 16);
            this.buttonRunMonitor.Name = "buttonRunMonitor";
            this.buttonRunMonitor.Size = new System.Drawing.Size(297, 28);
            this.buttonRunMonitor.TabIndex = 2;
            this.buttonRunMonitor.Text = "Run monitor";
            this.buttonRunMonitor.UseVisualStyleBackColor = true;
            this.buttonRunMonitor.Click += new System.EventHandler(this.buttonRunMonitor_Click);
            // 
            // tabControlLab
            // 
            this.tabControlLab.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlLab.Controls.Add(this.tabAppLab);
            this.tabControlLab.Controls.Add(this.tabKntScriptLab);
            this.tabControlLab.Controls.Add(this.tabRichEditor);
            this.tabControlLab.Controls.Add(this.tabWebView2);
            this.tabControlLab.Location = new System.Drawing.Point(12, 12);
            this.tabControlLab.Name = "tabControlLab";
            this.tabControlLab.SelectedIndex = 0;
            this.tabControlLab.Size = new System.Drawing.Size(670, 577);
            this.tabControlLab.TabIndex = 4;
            // 
            // LabForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(690, 595);
            this.Controls.Add(this.tabControlLab);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LabForm";
            this.Text = "KNote Lab";
            this.Load += new System.EventHandler(this.LabForm_Load);
            this.tabWebView2.ResumeLayout(false);
            this.tabWebView2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.webView2)).EndInit();
            this.tabRichEditor.ResumeLayout(false);
            this.tabKntScriptLab.ResumeLayout(false);
            this.groupSamples.ResumeLayout(false);
            this.tabAppLab.ResumeLayout(false);
            this.tabAppLab.PerformLayout();
            this.tabControlLab.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TabControl tabControlLab;
        private System.Windows.Forms.TabPage tabAppLab;
        private System.Windows.Forms.TabPage tabKntScriptLab;
        private System.Windows.Forms.Button buttonRunBackground;
        private System.Windows.Forms.Button buttonInteract;
        private System.Windows.Forms.Button buttonShowConsole;
        private System.Windows.Forms.Button buttonRunScript;
        private System.Windows.Forms.GroupBox groupSamples;
        private System.Windows.Forms.Button buttonRunSample;
        private System.Windows.Forms.Button buttonShowSample;
        private System.Windows.Forms.ListBox listSamples;
        private System.Windows.Forms.Button buttonRunMonitor;
        private System.Windows.Forms.ListBox listMessages;
        private System.Windows.Forms.Button buttonImportAnotasXML;
        private System.Windows.Forms.TabPage tabRichEditor;
        private Pavonis.Html.Editor.HtmlEditorControl htmlDescription;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonSelectScriptDirectory;
        private System.Windows.Forms.Button buttonTestReadVarItem;
        private System.Windows.Forms.Button buttonTestProcessStart;
        private System.Windows.Forms.Button buttonTestReflection;
        private TabPage tabWebView2;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView2;
        private Button btnGoBack;
        private Button btnNavigate;
        private TextBox textStatusWebView2;
        private TextBox textUrlWebView2;
        private Button buttonPlugin;
        private TextBox textPlugin;
        private Button buttonGetPluginFile;
    }
}