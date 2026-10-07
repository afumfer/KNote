namespace KNote.ClientWin.Views
{
    partial class KNoteAIAssistantForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KNoteAIAssistantForm));
            statusStripChat = new StatusStrip();
            toolStripStatusServiceRef = new ToolStripStatusLabel();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            toolStripStatusLabelTokens = new ToolStripStatusLabel();
            toolStripStatusLabelProcessingTime = new ToolStripStatusLabel();
            toolStripStatusLabel2 = new ToolStripStatusLabel();
            toolStripStatusLabelProcessing = new ToolStripStatusLabel();
            splitChat = new SplitContainer();
            buttonNavigate = new Button();
            buttonMarkDown = new Button();
            kntEditViewResult = new KntWebView.KntEditView();
            labelResult = new Label();
            panelSeparator = new Panel();
            buttonRestart = new Button();
            labelPrompt = new Label();
            comboProviders = new ComboBox();
            textPrompt = new TextBox();
            buttonSend = new Button();
            panelResultHeader = new Panel();
            panelPromptHeader = new Panel();
            menuAssistant = new MenuStrip();
            menuActions = new ToolStripMenuItem();
            menuSend = new ToolStripMenuItem();
            menuRestart = new ToolStripMenuItem();
            menuActionsSeparator1 = new ToolStripSeparator();
            menuModel = new ToolStripMenuItem();
            menuActionsSeparator2 = new ToolStripSeparator();
            menuNavigateView = new ToolStripMenuItem();
            menuMarkdownView = new ToolStripMenuItem();
            menuOptions = new ToolStripMenuItem();
            menuGetStream = new ToolStripMenuItem();
            menuGetCompletion = new ToolStripMenuItem();
            menuOptionsSeparator1 = new ToolStripSeparator();
            menuCatalogPrompts = new ToolStripMenuItem();
            menuViewSystem = new ToolStripMenuItem();
            menuOptionsSeparator2 = new ToolStripSeparator();
            menuShowModelInfo = new ToolStripMenuItem();
            menuOptionsSeparator3 = new ToolStripSeparator();
            menuManageModels = new ToolStripMenuItem();
            statusStripChat.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitChat).BeginInit();
            splitChat.Panel1.SuspendLayout();
            splitChat.Panel2.SuspendLayout();
            splitChat.SuspendLayout();
            panelResultHeader.SuspendLayout();
            panelPromptHeader.SuspendLayout();
            menuAssistant.SuspendLayout();
            SuspendLayout();
            //
            // statusStripChat
            //
            statusStripChat.ImageScalingSize = new Size(20, 20);
            statusStripChat.Items.AddRange(new ToolStripItem[] { toolStripStatusServiceRef, toolStripStatusLabel1, toolStripStatusLabelTokens, toolStripStatusLabelProcessingTime, toolStripStatusLabel2, toolStripStatusLabelProcessing });
            statusStripChat.Location = new Point(0, 601);
            statusStripChat.Name = "statusStripChat";
            statusStripChat.Size = new Size(858, 22);
            statusStripChat.TabIndex = 1;
            //
            // toolStripStatusServiceRef
            //
            toolStripStatusServiceRef.Name = "toolStripStatusServiceRef";
            toolStripStatusServiceRef.Size = new Size(44, 17);
            toolStripStatusServiceRef.Text = "Service";
            //
            // toolStripStatusLabel1
            //
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(10, 17);
            toolStripStatusLabel1.Text = "|";
            //
            // toolStripStatusLabelTokens
            //
            toolStripStatusLabelTokens.BorderStyle = Border3DStyle.Raised;
            toolStripStatusLabelTokens.Name = "toolStripStatusLabelTokens";
            toolStripStatusLabelTokens.Size = new Size(55, 17);
            toolStripStatusLabelTokens.Text = "Tokens: 0";
            //
            // toolStripStatusLabelProcessingTime
            //
            toolStripStatusLabelProcessingTime.Name = "toolStripStatusLabelProcessingTime";
            toolStripStatusLabelProcessingTime.Size = new Size(10, 17);
            toolStripStatusLabelProcessingTime.Text = " ";
            //
            // toolStripStatusLabel2
            //
            toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            toolStripStatusLabel2.Size = new Size(10, 17);
            toolStripStatusLabel2.Text = "|";
            //
            // toolStripStatusLabelProcessing
            //
            toolStripStatusLabelProcessing.BorderStyle = Border3DStyle.Raised;
            toolStripStatusLabelProcessing.Name = "toolStripStatusLabelProcessing";
            toolStripStatusLabelProcessing.Size = new Size(0, 17);
            //
            // splitChat
            //
            splitChat.Dock = DockStyle.Fill;
            splitChat.Location = new Point(0, 0);
            splitChat.Name = "splitChat";
            splitChat.Orientation = Orientation.Horizontal;
            //
            // splitChat.Panel1
            //
            splitChat.Panel1.Controls.Add(kntEditViewResult);
            splitChat.Panel1.Controls.Add(panelResultHeader);
            splitChat.Panel1.Padding = new Padding(4);
            splitChat.Panel1.TabIndex = 1;
            splitChat.Panel1MinSize = 200;
            //
            // splitChat.Panel2
            //
            splitChat.Panel2.Controls.Add(textPrompt);
            splitChat.Panel2.Controls.Add(panelPromptHeader);
            splitChat.Panel2.Padding = new Padding(4);
            splitChat.Panel2.TabIndex = 0;
            splitChat.Panel2MinSize = 50;
            splitChat.Size = new Size(858, 601);
            splitChat.SplitterDistance = 409;
            splitChat.SplitterWidth = 6;
            splitChat.TabIndex = 0;
            //
            // panelResultHeader
            //
            panelResultHeader.Controls.Add(labelResult);
            panelResultHeader.Controls.Add(buttonMarkDown);
            panelResultHeader.Controls.Add(buttonNavigate);
            panelResultHeader.Dock = DockStyle.Top;
            panelResultHeader.Location = new Point(0, 0);
            panelResultHeader.Name = "panelResultHeader";
            panelResultHeader.Size = new Size(858, 34);
            panelResultHeader.TabIndex = 0;
            //
            // buttonNavigate
            //
            buttonNavigate.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            buttonNavigate.Location = new Point(753, 4);
            buttonNavigate.Name = "buttonNavigate";
            buttonNavigate.Size = new Size(98, 26);
            buttonNavigate.TabIndex = 4;
            buttonNavigate.Text = "Navigate";
            buttonNavigate.TextImageRelation = TextImageRelation.ImageBeforeText;
            buttonNavigate.UseVisualStyleBackColor = true;
            buttonNavigate.Click += buttonNavigate_Click;
            //
            // buttonMarkDown
            //
            buttonMarkDown.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            buttonMarkDown.Location = new Point(650, 4);
            buttonMarkDown.Name = "buttonMarkDown";
            buttonMarkDown.Size = new Size(98, 26);
            buttonMarkDown.TabIndex = 3;
            buttonMarkDown.Text = "Markdown";
            buttonMarkDown.TextImageRelation = TextImageRelation.ImageBeforeText;
            buttonMarkDown.UseVisualStyleBackColor = true;
            buttonMarkDown.Click += buttonMarkDown_Click;
            //
            // kntEditViewResult
            //
            kntEditViewResult.Dock = DockStyle.Fill;
            kntEditViewResult.Location = new Point(0, 34);
            kntEditViewResult.Margin = new Padding(3, 4, 3, 4);
            kntEditViewResult.Name = "kntEditViewResult";
            kntEditViewResult.Size = new Size(858, 567);
            kntEditViewResult.TabIndex = 1;
            //
            // labelResult
            //
            labelResult.AutoSize = true;
            labelResult.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelResult.Location = new Point(8, 12);
            labelResult.Name = "labelResult";
            labelResult.Size = new Size(46, 17);
            labelResult.TabIndex = 0;
            labelResult.Text = "Result:";
            //
            // panelPromptHeader
            //
            panelPromptHeader.Controls.Add(labelPrompt);
            panelPromptHeader.Controls.Add(comboProviders);
            panelPromptHeader.Controls.Add(buttonSend);
            panelPromptHeader.Controls.Add(buttonRestart);
            panelPromptHeader.Controls.Add(panelSeparator);
            panelPromptHeader.Dock = DockStyle.Top;
            panelPromptHeader.Location = new Point(0, 0);
            panelPromptHeader.Name = "panelPromptHeader";
            panelPromptHeader.Size = new Size(858, 34);
            panelPromptHeader.TabIndex = 1;
            //
            // panelSeparator
            //
            panelSeparator.BackColor = SystemColors.ControlDarkDark;
            panelSeparator.Location = new Point(309, 5);
            panelSeparator.Name = "panelSeparator";
            panelSeparator.Size = new Size(3, 25);
            panelSeparator.TabIndex = 3;
            //
            // buttonRestart
            //
            buttonRestart.Font = new Font("Segoe UI", 8.25F);
            buttonRestart.Location = new Point(224, 4);
            buttonRestart.Name = "buttonRestart";
            buttonRestart.Size = new Size(78, 26);
            buttonRestart.TabIndex = 2;
            buttonRestart.Text = "&Restart";
            buttonRestart.TextImageRelation = TextImageRelation.ImageBeforeText;
            buttonRestart.UseVisualStyleBackColor = true;
            buttonRestart.Click += buttonRestart_Click;
            //
            // labelPrompt
            //
            labelPrompt.AutoSize = true;
            labelPrompt.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelPrompt.Location = new Point(8, 12);
            labelPrompt.Name = "labelPrompt";
            labelPrompt.Size = new Size(54, 17);
            labelPrompt.TabIndex = 0;
            labelPrompt.Text = "Prompt:";
            //
            // comboProviders
            //
            comboProviders.DropDownStyle = ComboBoxStyle.DropDownList;
            comboProviders.Font = new Font("Segoe UI", 8.25F);
            comboProviders.FormattingEnabled = true;
            comboProviders.Location = new Point(318, 7);
            comboProviders.Name = "comboProviders";
            comboProviders.Size = new Size(180, 23);
            comboProviders.TabIndex = 4;
            comboProviders.SelectedIndexChanged += comboProviders_SelectedIndexChanged;
            //
            // textPrompt
            //
            textPrompt.Dock = DockStyle.Fill;
            textPrompt.Font = new Font("Segoe UI", 9.75F);
            textPrompt.Location = new Point(0, 34);
            textPrompt.MaxLength = 0;
            textPrompt.Multiline = true;
            textPrompt.Name = "textPrompt";
            textPrompt.ScrollBars = ScrollBars.Vertical;
            textPrompt.Size = new Size(858, 158);
            textPrompt.TabIndex = 0;
            //
            // buttonSend
            //
            buttonSend.Font = new Font("Segoe UI", 8.25F);
            buttonSend.Location = new Point(140, 4);
            buttonSend.Name = "buttonSend";
            buttonSend.Size = new Size(78, 26);
            buttonSend.TabIndex = 1;
            buttonSend.Text = "&Send";
            buttonSend.TextImageRelation = TextImageRelation.ImageBeforeText;
            buttonSend.UseVisualStyleBackColor = true;
            buttonSend.Click += buttonSend_Click;
            //
            // menuAssistant
            //
            menuAssistant.Items.AddRange(new ToolStripItem[] { menuActions, menuOptions });
            menuAssistant.Location = new Point(0, 0);
            menuAssistant.Name = "menuAssistant";
            menuAssistant.Padding = new Padding(7, 2, 0, 2);
            menuAssistant.Size = new Size(858, 24);
            menuAssistant.TabIndex = 2;
            menuAssistant.Text = "KNote AI Assistant menu";
            //
            // menuActions
            //
            menuActions.DropDownItems.AddRange(new ToolStripItem[] { menuSend, menuRestart, menuActionsSeparator1, menuModel, menuActionsSeparator2, menuNavigateView, menuMarkdownView });
            menuActions.Name = "menuActions";
            menuActions.Size = new Size(59, 20);
            menuActions.Text = "&Actions";
            //
            // menuSend
            //
            menuSend.Name = "menuSend";
            menuSend.ShortcutKeys = Keys.Control | Keys.Return;
            menuSend.Size = new Size(200, 22);
            menuSend.Text = "&Send";
            menuSend.Click += buttonSend_Click;
            //
            // menuRestart
            //
            menuRestart.Name = "menuRestart";
            menuRestart.Size = new Size(200, 22);
            menuRestart.Text = "&Restart";
            menuRestart.Click += buttonRestart_Click;
            //
            // menuActionsSeparator1
            //
            menuActionsSeparator1.Name = "menuActionsSeparator1";
            menuActionsSeparator1.Size = new Size(197, 6);
            //
            // menuModel
            //
            menuModel.Name = "menuModel";
            menuModel.Size = new Size(200, 22);
            menuModel.Text = "&Model";
            menuModel.DropDownOpening += menuModel_DropDownOpening;
            //
            // menuActionsSeparator2
            //
            menuActionsSeparator2.Name = "menuActionsSeparator2";
            menuActionsSeparator2.Size = new Size(197, 6);
            //
            // menuNavigateView
            //
            menuNavigateView.Name = "menuNavigateView";
            menuNavigateView.Size = new Size(200, 22);
            menuNavigateView.Text = "&Navigate view";
            menuNavigateView.Click += buttonNavigate_Click;
            //
            // menuMarkdownView
            //
            menuMarkdownView.Name = "menuMarkdownView";
            menuMarkdownView.Size = new Size(200, 22);
            menuMarkdownView.Text = "Mar&kdown view";
            menuMarkdownView.Click += buttonMarkDown_Click;
            //
            // menuOptions
            //
            menuOptions.DropDownItems.AddRange(new ToolStripItem[] { menuGetStream, menuGetCompletion, menuOptionsSeparator1, menuCatalogPrompts, menuViewSystem, menuOptionsSeparator2, menuShowModelInfo, menuOptionsSeparator3, menuManageModels });
            menuOptions.Name = "menuOptions";
            menuOptions.Size = new Size(61, 20);
            menuOptions.Text = "&Options";
            //
            // menuGetStream
            //
            menuGetStream.Name = "menuGetStream";
            menuGetStream.Size = new Size(260, 22);
            menuGetStream.Text = "Get &stream";
            menuGetStream.Click += menuGetStream_Click;
            //
            // menuGetCompletion
            //
            menuGetCompletion.Name = "menuGetCompletion";
            menuGetCompletion.Size = new Size(260, 22);
            menuGetCompletion.Text = "Get c&ompletion";
            menuGetCompletion.Click += menuGetCompletion_Click;
            //
            // menuOptionsSeparator1
            //
            menuOptionsSeparator1.Name = "menuOptionsSeparator1";
            menuOptionsSeparator1.Size = new Size(257, 6);
            //
            // menuCatalogPrompts
            //
            menuCatalogPrompts.Name = "menuCatalogPrompts";
            menuCatalogPrompts.ShortcutKeys = Keys.Control | Keys.K;
            menuCatalogPrompts.Size = new Size(260, 22);
            menuCatalogPrompts.Text = "Get prompt from &catalog ...";
            menuCatalogPrompts.Click += menuCatalogPrompts_Click;
            //
            // menuViewSystem
            //
            menuViewSystem.Name = "menuViewSystem";
            menuViewSystem.Size = new Size(260, 22);
            menuViewSystem.Text = "&View system root ...";
            menuViewSystem.Click += menuViewSystem_Click;
            //
            // menuOptionsSeparator2
            //
            menuOptionsSeparator2.Name = "menuOptionsSeparator2";
            menuOptionsSeparator2.Size = new Size(257, 6);
            //
            // menuShowModelInfo
            //
            menuShowModelInfo.Name = "menuShowModelInfo";
            menuShowModelInfo.Size = new Size(260, 22);
            menuShowModelInfo.Text = "Show model &info (tokens, time)";
            menuShowModelInfo.Click += menuShowModelInfo_Click;
            //
            // menuOptionsSeparator3
            //
            menuOptionsSeparator3.Name = "menuOptionsSeparator3";
            menuOptionsSeparator3.Size = new Size(257, 6);
            //
            // menuManageModels
            //
            menuManageModels.Name = "menuManageModels";
            menuManageModels.Size = new Size(260, 22);
            menuManageModels.Text = "&Manage models ...";
            menuManageModels.Click += menuManageModels_Click;
            //
            // KNoteAIAssistantForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(858, 623);
            MinimumSize = new Size(820, 480);
            Controls.Add(splitChat);
            Controls.Add(statusStripChat);
            Controls.Add(menuAssistant);
            MainMenuStrip = menuAssistant;
            Name = "KNoteAIAssistantForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "KNote AI Assistant";
            FormClosing += KNoteAIAssistantForm_FormClosing;
            Load += KNoteAIAssistantForm_Load;
            statusStripChat.ResumeLayout(false);
            statusStripChat.PerformLayout();
            splitChat.Panel1.ResumeLayout(false);
            splitChat.Panel1.PerformLayout();
            splitChat.Panel2.ResumeLayout(false);
            splitChat.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitChat).EndInit();
            splitChat.ResumeLayout(false);
            panelResultHeader.ResumeLayout(false);
            panelResultHeader.PerformLayout();
            panelPromptHeader.ResumeLayout(false);
            panelPromptHeader.PerformLayout();
            menuAssistant.ResumeLayout(false);
            menuAssistant.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private StatusStrip statusStripChat;
        private ToolStripStatusLabel toolStripStatusLabelTokens;
        private ToolStripStatusLabel toolStripStatusLabelProcessing;
        private ToolStripStatusLabel toolStripStatusLabelProcessingTime;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private SplitContainer splitChat;
        private Label labelResult;
        private Button buttonRestart;
        private Label labelPrompt;
        private ComboBox comboProviders;
        private TextBox textPrompt;
        private Button buttonSend;
        private ToolStripStatusLabel toolStripStatusServiceRef;
        private ToolStripStatusLabel toolStripStatusLabel2;
        private Panel panelSeparator;
        private KntWebView.KntEditView kntEditViewResult;
        private Button buttonMarkDown;
        private Button buttonNavigate;
        private Panel panelResultHeader;
        private Panel panelPromptHeader;
        private MenuStrip menuAssistant;
        private ToolStripMenuItem menuActions;
        private ToolStripMenuItem menuSend;
        private ToolStripMenuItem menuRestart;
        private ToolStripSeparator menuActionsSeparator1;
        private ToolStripMenuItem menuModel;
        private ToolStripSeparator menuActionsSeparator2;
        private ToolStripMenuItem menuNavigateView;
        private ToolStripMenuItem menuMarkdownView;
        private ToolStripMenuItem menuOptions;
        private ToolStripMenuItem menuGetStream;
        private ToolStripMenuItem menuGetCompletion;
        private ToolStripSeparator menuOptionsSeparator1;
        private ToolStripMenuItem menuCatalogPrompts;
        private ToolStripMenuItem menuViewSystem;
        private ToolStripSeparator menuOptionsSeparator2;
        private ToolStripMenuItem menuShowModelInfo;
        private ToolStripSeparator menuOptionsSeparator3;
        private ToolStripMenuItem menuManageModels;
    }
}
