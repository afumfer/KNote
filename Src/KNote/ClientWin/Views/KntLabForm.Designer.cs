namespace KNote.ClientWin.Views
{
    partial class KntLabForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KntLabForm));
            openFileDialog = new OpenFileDialog();
            tabWebView2 = new TabPage();
            webView2 = new Microsoft.Web.WebView2.WinForms.WebView2();
            btnGoBack = new Button();
            btnNavigate = new Button();
            textStatusWebView2 = new TextBox();
            textUrlWebView2 = new TextBox();
            tabRichEditor = new TabPage();
            htmlDescription = new Pavonis.Html.Editor.HtmlEditorControl();
            tabKntScriptLab = new TabPage();
            groupSamples = new GroupBox();
            buttonSelectScriptDirectory = new Button();
            buttonRunSample = new Button();
            buttonShowSample = new Button();
            listSamples = new ListBox();
            buttonRunBackground = new Button();
            buttonInteract = new Button();
            buttonShowConsole = new Button();
            buttonRunScript = new Button();
            tabAppLab = new TabPage();
            buttonGetPluginFile = new Button();
            textPlugin = new TextBox();
            buttonPlugin = new Button();
            buttonTestReflection = new Button();
            buttonTestProcessStart = new Button();
            buttonTestReadVarItem = new Button();
            label2 = new Label();
            label1 = new Label();
            listMessages = new ListBox();
            buttonImportAnotasXML = new Button();
            buttonRunMonitor = new Button();
            tabControlLab = new TabControl();
            tabWebView2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)webView2).BeginInit();
            tabRichEditor.SuspendLayout();
            tabKntScriptLab.SuspendLayout();
            groupSamples.SuspendLayout();
            tabAppLab.SuspendLayout();
            tabControlLab.SuspendLayout();
            SuspendLayout();
            // 
            // tabWebView2
            // 
            tabWebView2.Controls.Add(webView2);
            tabWebView2.Controls.Add(btnGoBack);
            tabWebView2.Controls.Add(btnNavigate);
            tabWebView2.Controls.Add(textStatusWebView2);
            tabWebView2.Controls.Add(textUrlWebView2);
            tabWebView2.Location = new Point(4, 24);
            tabWebView2.Name = "tabWebView2";
            tabWebView2.Size = new Size(662, 549);
            tabWebView2.TabIndex = 3;
            tabWebView2.Text = "WebView2";
            tabWebView2.UseVisualStyleBackColor = true;
            // 
            // webView2
            // 
            webView2.AllowExternalDrop = true;
            webView2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            webView2.CreationProperties = null;
            webView2.DefaultBackgroundColor = Color.White;
            webView2.Location = new Point(3, 32);
            webView2.Name = "webView2";
            webView2.Size = new Size(656, 485);
            webView2.TabIndex = 4;
            webView2.ZoomFactor = 1D;
            // 
            // btnGoBack
            // 
            btnGoBack.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnGoBack.Location = new Point(585, 3);
            btnGoBack.Name = "btnGoBack";
            btnGoBack.Size = new Size(74, 23);
            btnGoBack.TabIndex = 3;
            btnGoBack.Text = "Go back";
            btnGoBack.UseVisualStyleBackColor = true;
            btnGoBack.Click += btnGoBack_Click;
            // 
            // btnNavigate
            // 
            btnNavigate.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnNavigate.Location = new Point(505, 3);
            btnNavigate.Name = "btnNavigate";
            btnNavigate.Size = new Size(74, 23);
            btnNavigate.TabIndex = 2;
            btnNavigate.Text = "Navigate";
            btnNavigate.UseVisualStyleBackColor = true;
            btnNavigate.Click += btnNavigate_Click;
            // 
            // textStatusWebView2
            // 
            textStatusWebView2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textStatusWebView2.Location = new Point(3, 523);
            textStatusWebView2.Name = "textStatusWebView2";
            textStatusWebView2.Size = new Size(656, 23);
            textStatusWebView2.TabIndex = 1;
            // 
            // textUrlWebView2
            // 
            textUrlWebView2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textUrlWebView2.Location = new Point(3, 3);
            textUrlWebView2.Name = "textUrlWebView2";
            textUrlWebView2.Size = new Size(496, 23);
            textUrlWebView2.TabIndex = 0;
            textUrlWebView2.Text = "https://www.gobiernodecanarias.org/educacion/9/pekweb/ekade";
            // 
            // tabRichEditor
            // 
            tabRichEditor.Controls.Add(htmlDescription);
            tabRichEditor.Location = new Point(4, 24);
            tabRichEditor.Name = "tabRichEditor";
            tabRichEditor.Size = new Size(662, 549);
            tabRichEditor.TabIndex = 2;
            tabRichEditor.Text = "Test rich editor";
            tabRichEditor.UseVisualStyleBackColor = true;
            // 
            // htmlDescription
            // 
            htmlDescription.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            htmlDescription.InnerText = null;
            htmlDescription.Location = new Point(14, 15);
            htmlDescription.Name = "htmlDescription";
            htmlDescription.Size = new Size(634, 520);
            htmlDescription.TabIndex = 10;
            // 
            // tabKntScriptLab
            // 
            tabKntScriptLab.Controls.Add(groupSamples);
            tabKntScriptLab.Controls.Add(buttonRunBackground);
            tabKntScriptLab.Controls.Add(buttonInteract);
            tabKntScriptLab.Controls.Add(buttonShowConsole);
            tabKntScriptLab.Controls.Add(buttonRunScript);
            tabKntScriptLab.Location = new Point(4, 24);
            tabKntScriptLab.Name = "tabKntScriptLab";
            tabKntScriptLab.Padding = new Padding(3);
            tabKntScriptLab.Size = new Size(662, 549);
            tabKntScriptLab.TabIndex = 1;
            tabKntScriptLab.Text = "KntScript lab";
            tabKntScriptLab.UseVisualStyleBackColor = true;
            // 
            // groupSamples
            // 
            groupSamples.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            groupSamples.Controls.Add(buttonSelectScriptDirectory);
            groupSamples.Controls.Add(buttonRunSample);
            groupSamples.Controls.Add(buttonShowSample);
            groupSamples.Controls.Add(listSamples);
            groupSamples.Location = new Point(16, 154);
            groupSamples.Name = "groupSamples";
            groupSamples.Size = new Size(476, 379);
            groupSamples.TabIndex = 8;
            groupSamples.TabStop = false;
            groupSamples.Text = "Samples";
            // 
            // buttonSelectScriptDirectory
            // 
            buttonSelectScriptDirectory.Location = new Point(15, 21);
            buttonSelectScriptDirectory.Name = "buttonSelectScriptDirectory";
            buttonSelectScriptDirectory.Size = new Size(157, 27);
            buttonSelectScriptDirectory.TabIndex = 6;
            buttonSelectScriptDirectory.Text = "Select script directory";
            buttonSelectScriptDirectory.UseVisualStyleBackColor = true;
            buttonSelectScriptDirectory.Click += buttonSelectScriptDirectory_Click;
            // 
            // buttonRunSample
            // 
            buttonRunSample.Location = new Point(297, 101);
            buttonRunSample.Name = "buttonRunSample";
            buttonRunSample.Size = new Size(173, 30);
            buttonRunSample.TabIndex = 5;
            buttonRunSample.Text = "Run sample";
            buttonRunSample.UseVisualStyleBackColor = true;
            buttonRunSample.Click += buttonRunSample_Click;
            // 
            // buttonShowSample
            // 
            buttonShowSample.Location = new Point(297, 64);
            buttonShowSample.Name = "buttonShowSample";
            buttonShowSample.Size = new Size(173, 31);
            buttonShowSample.TabIndex = 4;
            buttonShowSample.Text = "Show sample in KntConsole";
            buttonShowSample.UseVisualStyleBackColor = true;
            buttonShowSample.Click += buttonShowSample_Click;
            // 
            // listSamples
            // 
            listSamples.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            listSamples.FormattingEnabled = true;
            listSamples.ItemHeight = 15;
            listSamples.Location = new Point(13, 64);
            listSamples.Name = "listSamples";
            listSamples.Size = new Size(269, 304);
            listSamples.TabIndex = 2;
            listSamples.SelectedIndexChanged += listSamples_SelectedIndexChanged;
            // 
            // buttonRunBackground
            // 
            buttonRunBackground.Location = new Point(16, 82);
            buttonRunBackground.Name = "buttonRunBackground";
            buttonRunBackground.Size = new Size(282, 31);
            buttonRunBackground.TabIndex = 6;
            buttonRunBackground.Text = "Run simple script in bakground";
            buttonRunBackground.UseVisualStyleBackColor = true;
            buttonRunBackground.Click += buttonRunBackground_Click;
            // 
            // buttonInteract
            // 
            buttonInteract.Location = new Point(16, 48);
            buttonInteract.Name = "buttonInteract";
            buttonInteract.Size = new Size(282, 28);
            buttonInteract.TabIndex = 5;
            buttonInteract.Text = "Interacting with kntscript (embedded code)";
            buttonInteract.UseVisualStyleBackColor = true;
            buttonInteract.Click += buttonInteract_Click;
            // 
            // buttonShowConsole
            // 
            buttonShowConsole.Location = new Point(16, 119);
            buttonShowConsole.Name = "buttonShowConsole";
            buttonShowConsole.Size = new Size(282, 29);
            buttonShowConsole.TabIndex = 7;
            buttonShowConsole.Text = "Show KntConsole";
            buttonShowConsole.UseVisualStyleBackColor = true;
            buttonShowConsole.Click += buttonShowConsole_Click;
            // 
            // buttonRunScript
            // 
            buttonRunScript.Location = new Point(16, 14);
            buttonRunScript.Name = "buttonRunScript";
            buttonRunScript.Size = new Size(282, 28);
            buttonRunScript.TabIndex = 4;
            buttonRunScript.Text = "Run simple script (embedded code)";
            buttonRunScript.UseVisualStyleBackColor = true;
            buttonRunScript.Click += buttonRunScript_Click;
            // 
            // tabAppLab
            // 
            tabAppLab.Controls.Add(buttonGetPluginFile);
            tabAppLab.Controls.Add(textPlugin);
            tabAppLab.Controls.Add(buttonPlugin);
            tabAppLab.Controls.Add(buttonTestReflection);
            tabAppLab.Controls.Add(buttonTestProcessStart);
            tabAppLab.Controls.Add(buttonTestReadVarItem);
            tabAppLab.Controls.Add(label2);
            tabAppLab.Controls.Add(label1);
            tabAppLab.Controls.Add(listMessages);
            tabAppLab.Controls.Add(buttonImportAnotasXML);
            tabAppLab.Controls.Add(buttonRunMonitor);
            tabAppLab.Location = new Point(4, 24);
            tabAppLab.Name = "tabAppLab";
            tabAppLab.Padding = new Padding(3);
            tabAppLab.Size = new Size(662, 549);
            tabAppLab.TabIndex = 0;
            tabAppLab.Text = "Lab app components";
            tabAppLab.UseVisualStyleBackColor = true;
            // 
            // buttonGetPluginFile
            // 
            buttonGetPluginFile.Location = new Point(607, 15);
            buttonGetPluginFile.Name = "buttonGetPluginFile";
            buttonGetPluginFile.Size = new Size(39, 23);
            buttonGetPluginFile.TabIndex = 14;
            buttonGetPluginFile.Text = "<-";
            buttonGetPluginFile.UseVisualStyleBackColor = true;
            buttonGetPluginFile.Click += buttonGetPluginFile_Click;
            // 
            // textPlugin
            // 
            textPlugin.Location = new Point(352, 15);
            textPlugin.Name = "textPlugin";
            textPlugin.Size = new Size(247, 23);
            textPlugin.TabIndex = 13;
            // 
            // buttonPlugin
            // 
            buttonPlugin.Location = new Point(352, 62);
            buttonPlugin.Name = "buttonPlugin";
            buttonPlugin.Size = new Size(297, 28);
            buttonPlugin.TabIndex = 12;
            buttonPlugin.Text = "Test Plugin";
            buttonPlugin.UseVisualStyleBackColor = true;
            buttonPlugin.Click += buttonPlugin_Click;
            // 
            // buttonTestReflection
            // 
            buttonTestReflection.Location = new Point(14, 164);
            buttonTestReflection.Name = "buttonTestReflection";
            buttonTestReflection.Size = new Size(296, 26);
            buttonTestReflection.TabIndex = 11;
            buttonTestReflection.Text = "Test Reflection Model Objects";
            buttonTestReflection.UseVisualStyleBackColor = true;
            buttonTestReflection.Click += buttonReflection_Click;
            // 
            // buttonTestProcessStart
            // 
            buttonTestProcessStart.Location = new Point(14, 133);
            buttonTestProcessStart.Name = "buttonTestProcessStart";
            buttonTestProcessStart.Size = new Size(296, 25);
            buttonTestProcessStart.TabIndex = 10;
            buttonTestProcessStart.Text = "Test ProcessStart";
            buttonTestProcessStart.UseVisualStyleBackColor = true;
            buttonTestProcessStart.Click += buttonProcessStart_Click;
            // 
            // buttonTestReadVarItem
            // 
            buttonTestReadVarItem.Location = new Point(14, 101);
            buttonTestReadVarItem.Name = "buttonTestReadVarItem";
            buttonTestReadVarItem.Size = new Size(296, 26);
            buttonTestReadVarItem.TabIndex = 9;
            buttonTestReadVarItem.Text = "Test ReadVarItem";
            buttonTestReadVarItem.UseVisualStyleBackColor = true;
            buttonTestReadVarItem.Click += buttonReadVar_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(13, 233);
            label2.Name = "label2";
            label2.Size = new Size(38, 15);
            label2.TabIndex = 8;
            label2.Text = "label2";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(13, 218);
            label1.Name = "label1";
            label1.Size = new Size(38, 15);
            label1.TabIndex = 7;
            label1.Text = "label1";
            // 
            // listMessages
            // 
            listMessages.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listMessages.FormattingEnabled = true;
            listMessages.ItemHeight = 15;
            listMessages.Location = new Point(13, 254);
            listMessages.Name = "listMessages";
            listMessages.Size = new Size(636, 274);
            listMessages.TabIndex = 6;
            // 
            // buttonImportAnotasXML
            // 
            buttonImportAnotasXML.Location = new Point(12, 51);
            buttonImportAnotasXML.Name = "buttonImportAnotasXML";
            buttonImportAnotasXML.Size = new Size(298, 28);
            buttonImportAnotasXML.TabIndex = 5;
            buttonImportAnotasXML.Text = "Import anotas xml";
            buttonImportAnotasXML.UseVisualStyleBackColor = true;
            buttonImportAnotasXML.Click += buttonImportAnotasXML_Click;
            // 
            // buttonRunMonitor
            // 
            buttonRunMonitor.Location = new Point(13, 16);
            buttonRunMonitor.Name = "buttonRunMonitor";
            buttonRunMonitor.Size = new Size(297, 28);
            buttonRunMonitor.TabIndex = 2;
            buttonRunMonitor.Text = "Run monitor";
            buttonRunMonitor.UseVisualStyleBackColor = true;
            buttonRunMonitor.Click += buttonRunMonitor_Click;
            // 
            // tabControlLab
            // 
            tabControlLab.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            tabControlLab.Controls.Add(tabAppLab);
            tabControlLab.Controls.Add(tabKntScriptLab);
            tabControlLab.Controls.Add(tabRichEditor);
            tabControlLab.Controls.Add(tabWebView2);
            tabControlLab.Location = new Point(12, 12);
            tabControlLab.Name = "tabControlLab";
            tabControlLab.SelectedIndex = 0;
            tabControlLab.Size = new Size(670, 577);
            tabControlLab.TabIndex = 4;
            // 
            // KntLabForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(690, 595);
            Controls.Add(tabControlLab);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "KntLabForm";
            Text = "KNote Lab";
            FormClosing += KntLabForm_FormClosing;
            Load += LabForm_Load;
            tabWebView2.ResumeLayout(false);
            tabWebView2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)webView2).EndInit();
            tabRichEditor.ResumeLayout(false);
            tabKntScriptLab.ResumeLayout(false);
            groupSamples.ResumeLayout(false);
            tabAppLab.ResumeLayout(false);
            tabAppLab.PerformLayout();
            tabControlLab.ResumeLayout(false);
            ResumeLayout(false);
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