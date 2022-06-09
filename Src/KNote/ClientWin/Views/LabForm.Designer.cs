﻿namespace KNote.ClientWin.Views
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
            this.tabControlLab = new System.Windows.Forms.TabControl();
            this.tabAppLab = new System.Windows.Forms.TabPage();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.listMessages = new System.Windows.Forms.ListBox();
            this.buttonTest4 = new System.Windows.Forms.Button();
            this.buttonTest1 = new System.Windows.Forms.Button();
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
            this.tabRichEditor = new System.Windows.Forms.TabPage();
            this.htmlDescription = new Pavonis.Html.Editor.HtmlEditorControl();
            this.tabWebView2 = new System.Windows.Forms.TabPage();
            this.webView2 = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.btnGoBack = new System.Windows.Forms.Button();
            this.btnNavigate = new System.Windows.Forms.Button();
            this.textStatusWebView2 = new System.Windows.Forms.TextBox();
            this.textUrlWebView2 = new System.Windows.Forms.TextBox();
            this.tabRedMineLab = new System.Windows.Forms.TabPage();
            this.label6 = new System.Windows.Forms.Label();
            this.listInfoRedmine = new System.Windows.Forms.ListBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textHuIdsRedmine = new System.Windows.Forms.TextBox();
            this.textApiKey = new System.Windows.Forms.TextBox();
            this.textHost = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonTestKntRedmineApi = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.tabControlLab.SuspendLayout();
            this.tabAppLab.SuspendLayout();
            this.tabKntScriptLab.SuspendLayout();
            this.groupSamples.SuspendLayout();
            this.tabRichEditor.SuspendLayout();
            this.tabWebView2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.webView2)).BeginInit();
            this.tabRedMineLab.SuspendLayout();
            this.SuspendLayout();
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
            this.tabControlLab.Controls.Add(this.tabRedMineLab);
            this.tabControlLab.Location = new System.Drawing.Point(12, 12);
            this.tabControlLab.Name = "tabControlLab";
            this.tabControlLab.SelectedIndex = 0;
            this.tabControlLab.Size = new System.Drawing.Size(670, 577);
            this.tabControlLab.TabIndex = 4;
            // 
            // tabAppLab
            // 
            this.tabAppLab.Controls.Add(this.button3);
            this.tabAppLab.Controls.Add(this.button2);
            this.tabAppLab.Controls.Add(this.button1);
            this.tabAppLab.Controls.Add(this.label2);
            this.tabAppLab.Controls.Add(this.label1);
            this.tabAppLab.Controls.Add(this.listMessages);
            this.tabAppLab.Controls.Add(this.buttonTest4);
            this.tabAppLab.Controls.Add(this.buttonTest1);
            this.tabAppLab.Location = new System.Drawing.Point(4, 24);
            this.tabAppLab.Name = "tabAppLab";
            this.tabAppLab.Padding = new System.Windows.Forms.Padding(3);
            this.tabAppLab.Size = new System.Drawing.Size(662, 549);
            this.tabAppLab.TabIndex = 0;
            this.tabAppLab.Text = "Lab app components";
            this.tabAppLab.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(14, 164);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(296, 26);
            this.button3.TabIndex = 11;
            this.button3.Text = "Test Reflection Model Objects";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(14, 133);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(296, 25);
            this.button2.TabIndex = 10;
            this.button2.Text = "Test ProcessStart";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(14, 101);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(296, 26);
            this.button1.TabIndex = 9;
            this.button1.Text = "Test ReadVarItem";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
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
            // buttonTest4
            // 
            this.buttonTest4.Location = new System.Drawing.Point(12, 51);
            this.buttonTest4.Name = "buttonTest4";
            this.buttonTest4.Size = new System.Drawing.Size(298, 28);
            this.buttonTest4.TabIndex = 5;
            this.buttonTest4.Text = "Import anotas xml";
            this.buttonTest4.UseVisualStyleBackColor = true;
            this.buttonTest4.Click += new System.EventHandler(this.buttonTest4_Click);
            // 
            // buttonTest1
            // 
            this.buttonTest1.Location = new System.Drawing.Point(13, 16);
            this.buttonTest1.Name = "buttonTest1";
            this.buttonTest1.Size = new System.Drawing.Size(297, 28);
            this.buttonTest1.TabIndex = 2;
            this.buttonTest1.Text = "Run monitor";
            this.buttonTest1.UseVisualStyleBackColor = true;
            this.buttonTest1.Click += new System.EventHandler(this.buttonTest1_Click);
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
            // tabRedMineLab
            // 
            this.tabRedMineLab.Controls.Add(this.label6);
            this.tabRedMineLab.Controls.Add(this.listInfoRedmine);
            this.tabRedMineLab.Controls.Add(this.label5);
            this.tabRedMineLab.Controls.Add(this.textHuIdsRedmine);
            this.tabRedMineLab.Controls.Add(this.textApiKey);
            this.tabRedMineLab.Controls.Add(this.textHost);
            this.tabRedMineLab.Controls.Add(this.label4);
            this.tabRedMineLab.Controls.Add(this.label3);
            this.tabRedMineLab.Controls.Add(this.buttonTestKntRedmineApi);
            this.tabRedMineLab.Location = new System.Drawing.Point(4, 24);
            this.tabRedMineLab.Name = "tabRedMineLab";
            this.tabRedMineLab.Size = new System.Drawing.Size(662, 549);
            this.tabRedMineLab.TabIndex = 4;
            this.tabRedMineLab.Text = "RedMine lab";
            this.tabRedMineLab.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(291, 118);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(28, 15);
            this.label6.TabIndex = 8;
            this.label6.Text = "Info";
            // 
            // listInfoRedmine
            // 
            this.listInfoRedmine.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listInfoRedmine.FormattingEnabled = true;
            this.listInfoRedmine.ItemHeight = 15;
            this.listInfoRedmine.Location = new System.Drawing.Point(291, 136);
            this.listInfoRedmine.Name = "listInfoRedmine";
            this.listInfoRedmine.Size = new System.Drawing.Size(358, 394);
            this.listInfoRedmine.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 118);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(43, 15);
            this.label5.TabIndex = 6;
            this.label5.Text = "HU IDs";
            // 
            // textHuIdsRedmine
            // 
            this.textHuIdsRedmine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.textHuIdsRedmine.Location = new System.Drawing.Point(12, 136);
            this.textHuIdsRedmine.Multiline = true;
            this.textHuIdsRedmine.Name = "textHuIdsRedmine";
            this.textHuIdsRedmine.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textHuIdsRedmine.Size = new System.Drawing.Size(273, 394);
            this.textHuIdsRedmine.TabIndex = 5;
            this.textHuIdsRedmine.Text = "173755";
            // 
            // textApiKey
            // 
            this.textApiKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textApiKey.Location = new System.Drawing.Point(12, 75);
            this.textApiKey.Name = "textApiKey";
            this.textApiKey.Size = new System.Drawing.Size(457, 23);
            this.textApiKey.TabIndex = 4;
            // 
            // textHost
            // 
            this.textHost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textHost.Location = new System.Drawing.Point(12, 25);
            this.textHost.Name = "textHost";
            this.textHost.Size = new System.Drawing.Size(457, 23);
            this.textHost.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 57);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(44, 15);
            this.label4.TabIndex = 2;
            this.label4.Text = "ApiKey";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 15);
            this.label3.TabIndex = 1;
            this.label3.Text = "Host";
            // 
            // buttonTestKntRedmineApi
            // 
            this.buttonTestKntRedmineApi.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTestKntRedmineApi.Location = new System.Drawing.Point(503, 25);
            this.buttonTestKntRedmineApi.Name = "buttonTestKntRedmineApi";
            this.buttonTestKntRedmineApi.Size = new System.Drawing.Size(144, 27);
            this.buttonTestKntRedmineApi.TabIndex = 0;
            this.buttonTestKntRedmineApi.Text = "Test KntRedmineApi";
            this.buttonTestKntRedmineApi.UseVisualStyleBackColor = true;
            this.buttonTestKntRedmineApi.Click += new System.EventHandler(this.buttonTestKntRedmineApi_Click);
            // 
            // LabForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(690, 595);
            this.Controls.Add(this.tabControlLab);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LabForm";
            this.Text = "KntScript AppHost demo";
            this.Load += new System.EventHandler(this.LabForm_Load);
            this.tabControlLab.ResumeLayout(false);
            this.tabAppLab.ResumeLayout(false);
            this.tabAppLab.PerformLayout();
            this.tabKntScriptLab.ResumeLayout(false);
            this.groupSamples.ResumeLayout(false);
            this.tabRichEditor.ResumeLayout(false);
            this.tabWebView2.ResumeLayout(false);
            this.tabWebView2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.webView2)).EndInit();
            this.tabRedMineLab.ResumeLayout(false);
            this.tabRedMineLab.PerformLayout();
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
        private System.Windows.Forms.Button buttonTest1;
        private System.Windows.Forms.ListBox listMessages;
        private System.Windows.Forms.Button buttonTest4;
        private System.Windows.Forms.TabPage tabRichEditor;
        private Pavonis.Html.Editor.HtmlEditorControl htmlDescription;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonSelectScriptDirectory;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private TabPage tabWebView2;
        private TabPage tabRedMineLab;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView2;
        private Button btnGoBack;
        private Button btnNavigate;
        private TextBox textStatusWebView2;
        private TextBox textUrlWebView2;
        private Button buttonTestKntRedmineApi;
        private TextBox textApiKey;
        private TextBox textHost;
        private Label label4;
        private Label label3;
        private TextBox textHuIdsRedmine;
        private Label label6;
        private ListBox listInfoRedmine;
        private Label label5;
    }
}