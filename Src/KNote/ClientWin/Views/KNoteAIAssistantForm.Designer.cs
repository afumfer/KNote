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
            radioGetStream = new RadioButton();
            radioGetCompletion = new RadioButton();
            labelResult = new Label();
            panelSeparator = new Panel();
            buttonViewSystem = new Button();
            buttonCatalogPrompts = new Button();
            buttonRestart = new Button();
            labelPrompt = new Label();
            comboProviders = new ComboBox();
            buttonManageProviders = new Button();
            textPrompt = new TextBox();
            buttonSend = new Button();
            panelResultHeader = new Panel();
            panelPromptHeader = new Panel();
            statusStripChat.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitChat).BeginInit();
            splitChat.Panel1.SuspendLayout();
            splitChat.Panel2.SuspendLayout();
            splitChat.SuspendLayout();
            panelResultHeader.SuspendLayout();
            panelPromptHeader.SuspendLayout();
            SuspendLayout();
            //
            // statusStripChat
            //
            statusStripChat.ImageScalingSize = new Size(20, 20);
            statusStripChat.Items.AddRange(new ToolStripItem[] { toolStripStatusServiceRef, toolStripStatusLabel1, toolStripStatusLabelTokens, toolStripStatusLabelProcessingTime, toolStripStatusLabel2, toolStripStatusLabelProcessing });
            statusStripChat.Location = new Point(0, 601);
            statusStripChat.Name = "statusStripChat";
            statusStripChat.Size = new Size(858, 22);
            statusStripChat.TabIndex = 22;
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
            splitChat.Panel1MinSize = 200;
            //
            // splitChat.Panel2
            //
            splitChat.Panel2.Controls.Add(textPrompt);
            splitChat.Panel2.Controls.Add(panelPromptHeader);
            splitChat.Panel2MinSize = 50;
            splitChat.Size = new Size(858, 601);
            splitChat.SplitterDistance = 409;
            splitChat.SplitterWidth = 6;
            splitChat.TabIndex = 25;
            //
            // panelResultHeader
            //
            panelResultHeader.Controls.Add(labelResult);
            panelResultHeader.Controls.Add(radioGetStream);
            panelResultHeader.Controls.Add(radioGetCompletion);
            panelResultHeader.Controls.Add(buttonMarkDown);
            panelResultHeader.Controls.Add(buttonNavigate);
            panelResultHeader.Dock = DockStyle.Top;
            panelResultHeader.Location = new Point(0, 0);
            panelResultHeader.Name = "panelResultHeader";
            panelResultHeader.Size = new Size(858, 34);
            panelResultHeader.TabIndex = 29;
            //
            // buttonNavigate
            //
            buttonNavigate.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            buttonNavigate.Location = new Point(769, 4);
            buttonNavigate.Name = "buttonNavigate";
            buttonNavigate.Size = new Size(82, 26);
            buttonNavigate.TabIndex = 28;
            buttonNavigate.Text = "Navigate";
            buttonNavigate.UseVisualStyleBackColor = true;
            buttonNavigate.Click += buttonNavigate_Click;
            //
            // buttonMarkDown
            //
            buttonMarkDown.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            buttonMarkDown.Location = new Point(682, 4);
            buttonMarkDown.Name = "buttonMarkDown";
            buttonMarkDown.Size = new Size(82, 26);
            buttonMarkDown.TabIndex = 27;
            buttonMarkDown.Text = "Markdown";
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
            kntEditViewResult.TabIndex = 26;
            //
            // radioGetStream
            //
            radioGetStream.AutoSize = true;
            radioGetStream.Checked = true;
            radioGetStream.Font = new Font("Segoe UI", 8.25F);
            radioGetStream.Location = new Point(455, 9);
            radioGetStream.Name = "radioGetStream";
            radioGetStream.Size = new Size(81, 17);
            radioGetStream.TabIndex = 5;
            radioGetStream.TabStop = true;
            radioGetStream.Text = "Get Stream";
            radioGetStream.UseVisualStyleBackColor = true;
            //
            // radioGetCompletion
            //
            radioGetCompletion.AutoSize = true;
            radioGetCompletion.Font = new Font("Segoe UI", 8.25F);
            radioGetCompletion.Location = new Point(557, 9);
            radioGetCompletion.Name = "radioGetCompletion";
            radioGetCompletion.Size = new Size(106, 17);
            radioGetCompletion.TabIndex = 6;
            radioGetCompletion.Text = "Get Completion";
            radioGetCompletion.UseVisualStyleBackColor = true;
            //
            // labelResult
            //
            labelResult.AutoSize = true;
            labelResult.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelResult.Location = new Point(8, 12);
            labelResult.Name = "labelResult";
            labelResult.Size = new Size(46, 17);
            labelResult.TabIndex = 25;
            labelResult.Text = "Result:";
            //
            // panelPromptHeader
            //
            panelPromptHeader.Controls.Add(labelPrompt);
            panelPromptHeader.Controls.Add(comboProviders);
            panelPromptHeader.Controls.Add(buttonManageProviders);
            panelPromptHeader.Controls.Add(buttonSend);
            panelPromptHeader.Controls.Add(buttonRestart);
            panelPromptHeader.Controls.Add(panelSeparator);
            panelPromptHeader.Controls.Add(buttonCatalogPrompts);
            panelPromptHeader.Controls.Add(buttonViewSystem);
            panelPromptHeader.Dock = DockStyle.Top;
            panelPromptHeader.Location = new Point(0, 0);
            panelPromptHeader.Name = "panelPromptHeader";
            panelPromptHeader.Size = new Size(858, 34);
            panelPromptHeader.TabIndex = 30;
            //
            // panelSeparator
            //
            panelSeparator.BackColor = SystemColors.ControlDarkDark;
            panelSeparator.Location = new Point(569, 5);
            panelSeparator.Name = "panelSeparator";
            panelSeparator.Size = new Size(3, 25);
            panelSeparator.TabIndex = 27;
            //
            // buttonViewSystem
            //
            buttonViewSystem.Font = new Font("Segoe UI", 8.25F);
            buttonViewSystem.Location = new Point(738, 4);
            buttonViewSystem.Name = "buttonViewSystem";
            buttonViewSystem.Size = new Size(114, 26);
            buttonViewSystem.TabIndex = 26;
            buttonViewSystem.Text = "&View system root";
            buttonViewSystem.UseVisualStyleBackColor = true;
            buttonViewSystem.Click += buttonViewSystem_Click;
            //
            // buttonCatalogPrompts
            //
            buttonCatalogPrompts.Font = new Font("Segoe UI", 8.25F);
            buttonCatalogPrompts.Location = new Point(578, 4);
            buttonCatalogPrompts.Name = "buttonCatalogPrompts";
            buttonCatalogPrompts.Size = new Size(154, 26);
            buttonCatalogPrompts.TabIndex = 25;
            buttonCatalogPrompts.Text = "Get prompt from &catalog";
            buttonCatalogPrompts.UseVisualStyleBackColor = true;
            buttonCatalogPrompts.Click += buttonCatalogPrompts_Click;
            //
            // buttonRestart
            //
            buttonRestart.Font = new Font("Segoe UI", 8.25F);
            buttonRestart.Location = new Point(506, 4);
            buttonRestart.Name = "buttonRestart";
            buttonRestart.Size = new Size(56, 26);
            buttonRestart.TabIndex = 3;
            buttonRestart.Text = "&Restart";
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
            labelPrompt.TabIndex = 24;
            labelPrompt.Text = "Prompt:";
            //
            // comboProviders
            //
            comboProviders.DropDownStyle = ComboBoxStyle.DropDownList;
            comboProviders.Font = new Font("Segoe UI", 8.25F);
            comboProviders.FormattingEnabled = true;
            comboProviders.Location = new Point(184, 7);
            comboProviders.Name = "comboProviders";
            comboProviders.Size = new Size(180, 23);
            comboProviders.TabIndex = 1;
            comboProviders.SelectedIndexChanged += comboProviders_SelectedIndexChanged;
            //
            // buttonManageProviders
            //
            buttonManageProviders.Font = new Font("Segoe UI", 8.25F);
            buttonManageProviders.Location = new Point(370, 4);
            buttonManageProviders.Name = "buttonManageProviders";
            buttonManageProviders.Size = new Size(70, 26);
            buttonManageProviders.TabIndex = 4;
            buttonManageProviders.Text = "&Manage...";
            buttonManageProviders.UseVisualStyleBackColor = true;
            buttonManageProviders.Click += buttonManageProviders_Click;
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
            buttonSend.Location = new Point(444, 4);
            buttonSend.Name = "buttonSend";
            buttonSend.Size = new Size(56, 26);
            buttonSend.TabIndex = 2;
            buttonSend.Text = "&Send";
            buttonSend.UseVisualStyleBackColor = true;
            buttonSend.Click += buttonSend_Click;
            //
            // KNoteAIAssistantForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(858, 623);
            Controls.Add(splitChat);
            Controls.Add(statusStripChat);
            Icon = (Icon)resources.GetObject("$this.Icon");
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
        private Button buttonManageProviders;
        private TextBox textPrompt;
        private Button buttonSend;
        private Button buttonCatalogPrompts;
        private ToolStripStatusLabel toolStripStatusServiceRef;
        private ToolStripStatusLabel toolStripStatusLabel2;
        private Button buttonViewSystem;
        private Panel panelSeparator;
        private KntWebView.KntEditView kntEditViewResult;
        private RadioButton radioGetStream;
        private RadioButton radioGetCompletion;
        private Button buttonMarkDown;
        private Button buttonNavigate;
        private Panel panelResultHeader;
        private Panel panelPromptHeader;
    }
}
