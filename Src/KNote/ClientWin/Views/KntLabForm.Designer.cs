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
            textFolderForVirtualHostNameMapping = new TextBox();
            label3 = new Label();
            textScript = new TextBox();
            btnExecuteScript = new Button();
            textHtml = new TextBox();
            btnNavToString = new Button();
            webView2 = new Microsoft.Web.WebView2.WinForms.WebView2();
            btnGoBack = new Button();
            btnNavigate = new Button();
            textStatusWebView2 = new TextBox();
            textUrlWebView2 = new TextBox();
            tabRichEditor = new TabPage();
            htmlDescription = new MSDN.Html.Editor.HtmlEditorControl();
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
            buttonNLog = new Button();
            buttonServerCOMForm = new Button();
            buttonMessageBrokerSendMessage = new Button();
            buttonConfigureMessageBroker = new Button();
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
            tabKntEditView = new TabPage();
            textForKntEditView = new TextBox();
            btnKntEditViewHtml = new Button();
            btnKntEditViewNavigation = new Button();
            btnKntEditViewMarkdown = new Button();
            panel1 = new Panel();
            kntEditView = new KntWebView.KntEditView();
            tabWebView2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)webView2).BeginInit();
            tabRichEditor.SuspendLayout();
            tabKntScriptLab.SuspendLayout();
            groupSamples.SuspendLayout();
            tabAppLab.SuspendLayout();
            tabControlLab.SuspendLayout();
            tabKntEditView.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // tabWebView2
            // 
            tabWebView2.Controls.Add(textFolderForVirtualHostNameMapping);
            tabWebView2.Controls.Add(label3);
            tabWebView2.Controls.Add(textScript);
            tabWebView2.Controls.Add(btnExecuteScript);
            tabWebView2.Controls.Add(textHtml);
            tabWebView2.Controls.Add(btnNavToString);
            tabWebView2.Controls.Add(webView2);
            tabWebView2.Controls.Add(btnGoBack);
            tabWebView2.Controls.Add(btnNavigate);
            tabWebView2.Controls.Add(textStatusWebView2);
            tabWebView2.Controls.Add(textUrlWebView2);
            tabWebView2.Location = new Point(4, 24);
            tabWebView2.Name = "tabWebView2";
            tabWebView2.Size = new Size(726, 597);
            tabWebView2.TabIndex = 3;
            tabWebView2.Text = "WebView2";
            tabWebView2.UseVisualStyleBackColor = true;
            // 
            // textFolderForVirtualHostNameMapping
            // 
            textFolderForVirtualHostNameMapping.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            textFolderForVirtualHostNameMapping.Location = new Point(101, 567);
            textFolderForVirtualHostNameMapping.Name = "textFolderForVirtualHostNameMapping";
            textFolderForVirtualHostNameMapping.Size = new Size(256, 23);
            textFolderForVirtualHostNameMapping.TabIndex = 10;
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label3.AutoSize = true;
            label3.Location = new Point(6, 570);
            label3.Name = "label3";
            label3.Size = new Size(91, 15);
            label3.TabIndex = 9;
            label3.Text = "Vir. Host Folder:";
            // 
            // textScript
            // 
            textScript.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            textScript.Location = new Point(372, 390);
            textScript.Multiline = true;
            textScript.Name = "textScript";
            textScript.Size = new Size(351, 204);
            textScript.TabIndex = 8;
            // 
            // btnExecuteScript
            // 
            btnExecuteScript.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnExecuteScript.Location = new Point(374, 361);
            btnExecuteScript.Name = "btnExecuteScript";
            btnExecuteScript.Size = new Size(132, 23);
            btnExecuteScript.TabIndex = 7;
            btnExecuteScript.Text = "Execute Script";
            btnExecuteScript.UseVisualStyleBackColor = true;
            btnExecuteScript.Click += btnExecuteScript_Click;
            // 
            // textHtml
            // 
            textHtml.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            textHtml.Location = new Point(6, 390);
            textHtml.Multiline = true;
            textHtml.Name = "textHtml";
            textHtml.Size = new Size(351, 171);
            textHtml.TabIndex = 6;
            // 
            // btnNavToString
            // 
            btnNavToString.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnNavToString.Location = new Point(6, 361);
            btnNavToString.Name = "btnNavToString";
            btnNavToString.Size = new Size(132, 23);
            btnNavToString.TabIndex = 5;
            btnNavToString.Text = "Navigate to string";
            btnNavToString.UseVisualStyleBackColor = true;
            btnNavToString.Click += btnNavToString_Click;
            // 
            // webView2
            // 
            webView2.AllowExternalDrop = true;
            webView2.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            webView2.CreationProperties = null;
            webView2.DefaultBackgroundColor = Color.White;
            webView2.Location = new Point(3, 32);
            webView2.Name = "webView2";
            webView2.Size = new Size(720, 284);
            webView2.TabIndex = 4;
            webView2.ZoomFactor = 1D;
            // 
            // btnGoBack
            // 
            btnGoBack.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnGoBack.Location = new Point(649, 3);
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
            btnNavigate.Location = new Point(569, 3);
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
            textStatusWebView2.Location = new Point(3, 322);
            textStatusWebView2.Name = "textStatusWebView2";
            textStatusWebView2.Size = new Size(720, 23);
            textStatusWebView2.TabIndex = 1;
            // 
            // textUrlWebView2
            // 
            textUrlWebView2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textUrlWebView2.Location = new Point(3, 3);
            textUrlWebView2.Name = "textUrlWebView2";
            textUrlWebView2.Size = new Size(560, 23);
            textUrlWebView2.TabIndex = 0;
            textUrlWebView2.Text = "https://www.gobiernodecanarias.org/educacion/9/pekweb/ekade";
            // 
            // tabRichEditor
            // 
            tabRichEditor.Controls.Add(htmlDescription);
            tabRichEditor.Location = new Point(4, 24);
            tabRichEditor.Name = "tabRichEditor";
            tabRichEditor.Size = new Size(726, 597);
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
            tabKntScriptLab.Size = new Size(726, 597);
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
            tabAppLab.Controls.Add(buttonNLog);
            tabAppLab.Controls.Add(buttonServerCOMForm);
            tabAppLab.Controls.Add(buttonMessageBrokerSendMessage);
            tabAppLab.Controls.Add(buttonConfigureMessageBroker);
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
            tabAppLab.Size = new Size(726, 597);
            tabAppLab.TabIndex = 0;
            tabAppLab.Text = "Lab app components";
            tabAppLab.UseVisualStyleBackColor = true;
            // 
            // buttonNLog
            // 
            buttonNLog.Location = new Point(349, 164);
            buttonNLog.Name = "buttonNLog";
            buttonNLog.Size = new Size(127, 28);
            buttonNLog.TabIndex = 18;
            buttonNLog.Text = "Test NLog";
            buttonNLog.UseVisualStyleBackColor = true;
            buttonNLog.Click += buttonNLog_Click;
            // 
            // buttonServerCOMForm
            // 
            buttonServerCOMForm.Location = new Point(349, 85);
            buttonServerCOMForm.Name = "buttonServerCOMForm";
            buttonServerCOMForm.Size = new Size(297, 28);
            buttonServerCOMForm.TabIndex = 17;
            buttonServerCOMForm.Text = "ServerCOM";
            buttonServerCOMForm.UseVisualStyleBackColor = true;
            buttonServerCOMForm.Click += buttonServerCOMForm_Click;
            // 
            // buttonMessageBrokerSendMessage
            // 
            buttonMessageBrokerSendMessage.Location = new Point(524, 128);
            buttonMessageBrokerSendMessage.Name = "buttonMessageBrokerSendMessage";
            buttonMessageBrokerSendMessage.Size = new Size(122, 28);
            buttonMessageBrokerSendMessage.TabIndex = 16;
            buttonMessageBrokerSendMessage.Text = "Send Message";
            buttonMessageBrokerSendMessage.UseVisualStyleBackColor = true;
            buttonMessageBrokerSendMessage.Click += buttonMessageBrokerSendMessage_Click;
            // 
            // buttonConfigureMessageBroker
            // 
            buttonConfigureMessageBroker.Location = new Point(349, 128);
            buttonConfigureMessageBroker.Name = "buttonConfigureMessageBroker";
            buttonConfigureMessageBroker.Size = new Size(169, 28);
            buttonConfigureMessageBroker.TabIndex = 15;
            buttonConfigureMessageBroker.Text = "Configure Message Broker";
            buttonConfigureMessageBroker.UseVisualStyleBackColor = true;
            buttonConfigureMessageBroker.Click += buttonConfigureMessageBroker_Click;
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
            buttonPlugin.Location = new Point(352, 51);
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
            tabControlLab.Controls.Add(tabKntEditView);
            tabControlLab.Controls.Add(tabWebView2);
            tabControlLab.Controls.Add(tabRichEditor);
            tabControlLab.Controls.Add(tabKntScriptLab);
            tabControlLab.Controls.Add(tabAppLab);
            tabControlLab.Location = new Point(12, 12);
            tabControlLab.Name = "tabControlLab";
            tabControlLab.SelectedIndex = 0;
            tabControlLab.Size = new Size(734, 625);
            tabControlLab.TabIndex = 4;
            // 
            // tabKntEditView
            // 
            tabKntEditView.Controls.Add(panel1);
            tabKntEditView.Controls.Add(textForKntEditView);
            tabKntEditView.Controls.Add(btnKntEditViewHtml);
            tabKntEditView.Controls.Add(btnKntEditViewNavigation);
            tabKntEditView.Controls.Add(btnKntEditViewMarkdown);
            tabKntEditView.Location = new Point(4, 24);
            tabKntEditView.Name = "tabKntEditView";
            tabKntEditView.Size = new Size(726, 597);
            tabKntEditView.TabIndex = 4;
            tabKntEditView.Text = "KntEditView";
            tabKntEditView.UseVisualStyleBackColor = true;
            // 
            // textForKntEditView
            // 
            textForKntEditView.Location = new Point(218, 12);
            textForKntEditView.Multiline = true;
            textForKntEditView.Name = "textForKntEditView";
            textForKntEditView.Size = new Size(313, 103);
            textForKntEditView.TabIndex = 4;
            textForKntEditView.Text = "Test";
            // 
            // btnKntEditViewHtml
            // 
            btnKntEditViewHtml.Location = new Point(17, 78);
            btnKntEditViewHtml.Name = "btnKntEditViewHtml";
            btnKntEditViewHtml.Size = new Size(181, 26);
            btnKntEditViewHtml.TabIndex = 3;
            btnKntEditViewHtml.Text = "KntEditView html";
            btnKntEditViewHtml.UseVisualStyleBackColor = true;
            btnKntEditViewHtml.Click += btnKntEditViewHtml_Click;
            // 
            // btnKntEditViewNavigation
            // 
            btnKntEditViewNavigation.Location = new Point(17, 46);
            btnKntEditViewNavigation.Name = "btnKntEditViewNavigation";
            btnKntEditViewNavigation.Size = new Size(181, 26);
            btnKntEditViewNavigation.TabIndex = 2;
            btnKntEditViewNavigation.Text = "KntEditView navigation";
            btnKntEditViewNavigation.UseVisualStyleBackColor = true;
            btnKntEditViewNavigation.Click += btnKntEditViewNavigation_Click;
            // 
            // btnKntEditViewMarkdown
            // 
            btnKntEditViewMarkdown.Location = new Point(17, 14);
            btnKntEditViewMarkdown.Name = "btnKntEditViewMarkdown";
            btnKntEditViewMarkdown.Size = new Size(181, 26);
            btnKntEditViewMarkdown.TabIndex = 1;
            btnKntEditViewMarkdown.Text = "KntEditView markdown";
            btnKntEditViewMarkdown.UseVisualStyleBackColor = true;
            btnKntEditViewMarkdown.Click += btnKntEditViewMarkdown_Click;
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel1.Controls.Add(kntEditView);
            panel1.Location = new Point(17, 139);
            panel1.Name = "panel1";
            panel1.Size = new Size(693, 439);
            panel1.TabIndex = 5;
            // 
            // kntEditView
            // 
            kntEditView.BorderStyle = BorderStyle.FixedSingle;
            kntEditView.Location = new Point(40, 44);
            kntEditView.Name = "kntEditView";
            kntEditView.Size = new Size(281, 148);
            kntEditView.TabIndex = 1;
            // 
            // KntLabForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(754, 643);
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
            tabKntEditView.ResumeLayout(false);
            tabKntEditView.PerformLayout();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private TabControl tabControlLab;
        private TabPage tabAppLab;
        private TabPage tabKntScriptLab;
        private Button buttonRunBackground;
        private Button buttonInteract;
        private Button buttonShowConsole;
        private Button buttonRunScript;
        private GroupBox groupSamples;
        private Button buttonRunSample;
        private Button buttonShowSample;
        private ListBox listSamples;
        private Button buttonRunMonitor;
        private ListBox listMessages;
        private Button buttonImportAnotasXML;
        private TabPage tabRichEditor;
        private MSDN.Html.Editor.HtmlEditorControl htmlDescription;
        private OpenFileDialog openFileDialog;
        private Label label1;
        private Label label2;
        private Button buttonSelectScriptDirectory;
        private Button buttonTestReadVarItem;
        private Button buttonTestProcessStart;
        private Button buttonTestReflection;
        private TabPage tabWebView2;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView2;
        private Button btnGoBack;
        private Button btnNavigate;
        private TextBox textStatusWebView2;
        private TextBox textUrlWebView2;
        private Button buttonPlugin;
        private TextBox textPlugin;
        private Button buttonGetPluginFile;
        private Button buttonMessageBrokerSendMessage;
        private Button buttonConfigureMessageBroker;
        private Button buttonServerCOMForm;
        private Button buttonNLog;
        private TextBox textHtml;
        private Button btnNavToString;
        private TextBox textScript;
        private Button btnExecuteScript;
        private Label label3;
        private TextBox textFolderForVirtualHostNameMapping;
        private TabPage tabKntEditView;
        private Button btnKntEditViewNavigation;
        private Button btnKntEditViewMarkdown;
        private Button btnKntEditViewHtml;
        private TextBox textForKntEditView;
        private Panel panel1;
        private KntWebView.KntEditView kntEditView;
    }
}