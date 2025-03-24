namespace KNote.ClientWin.Views
{
    partial class KntChatGPTForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KntChatGPTForm));
            statusStripChat = new StatusStrip();
            toolStripStatusServiceRef = new ToolStripStatusLabel();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            toolStripStatusLabelTokens = new ToolStripStatusLabel();
            toolStripStatusLabelProcessingTime = new ToolStripStatusLabel();
            toolStripStatusLabel2 = new ToolStripStatusLabel();
            toolStripStatusLabelProcessing = new ToolStripStatusLabel();
            splitChat = new SplitContainer();
            radioGetStream = new RadioButton();
            radioGetCompletion = new RadioButton();
            textResult = new TextBox();
            labelResult = new Label();
            panelSeparator = new Panel();
            buttonViewSystem = new Button();
            buttonCatalogPrompts = new Button();
            buttonRestart = new Button();
            labelPrompt = new Label();
            textPrompt = new TextBox();
            buttonSend = new Button();
            statusStripChat.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitChat).BeginInit();
            splitChat.Panel1.SuspendLayout();
            splitChat.Panel2.SuspendLayout();
            splitChat.SuspendLayout();
            SuspendLayout();
            // 
            // statusStripChat
            // 
            statusStripChat.ImageScalingSize = new Size(20, 20);
            statusStripChat.Items.AddRange(new ToolStripItem[] { toolStripStatusServiceRef, toolStripStatusLabel1, toolStripStatusLabelTokens, toolStripStatusLabelProcessingTime, toolStripStatusLabel2, toolStripStatusLabelProcessing });
            statusStripChat.Location = new Point(0, 601);
            statusStripChat.Name = "statusStripChat";
            statusStripChat.Size = new Size(735, 22);
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
            splitChat.Panel1.Controls.Add(radioGetStream);
            splitChat.Panel1.Controls.Add(radioGetCompletion);
            splitChat.Panel1.Controls.Add(textResult);
            splitChat.Panel1.Controls.Add(labelResult);
            splitChat.Panel1MinSize = 200;
            // 
            // splitChat.Panel2
            // 
            splitChat.Panel2.Controls.Add(panelSeparator);
            splitChat.Panel2.Controls.Add(buttonViewSystem);
            splitChat.Panel2.Controls.Add(buttonCatalogPrompts);
            splitChat.Panel2.Controls.Add(buttonRestart);
            splitChat.Panel2.Controls.Add(labelPrompt);
            splitChat.Panel2.Controls.Add(textPrompt);
            splitChat.Panel2.Controls.Add(buttonSend);
            splitChat.Panel2MinSize = 50;
            splitChat.Size = new Size(735, 601);
            splitChat.SplitterDistance = 410;
            splitChat.SplitterWidth = 6;
            splitChat.TabIndex = 25;
            // 
            // radioGetStream
            // 
            radioGetStream.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            radioGetStream.AutoSize = true;
            radioGetStream.Checked = true;
            radioGetStream.Location = new Point(526, 7);
            radioGetStream.Name = "radioGetStream";
            radioGetStream.Size = new Size(83, 19);
            radioGetStream.TabIndex = 5;
            radioGetStream.TabStop = true;
            radioGetStream.Text = "Get Stream";
            radioGetStream.UseVisualStyleBackColor = true;
            // 
            // radioGetCompletion
            // 
            radioGetCompletion.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            radioGetCompletion.AutoSize = true;
            radioGetCompletion.Location = new Point(622, 7);
            radioGetCompletion.Name = "radioGetCompletion";
            radioGetCompletion.Size = new Size(109, 19);
            radioGetCompletion.TabIndex = 6;
            radioGetCompletion.Text = "Get Completion";
            radioGetCompletion.UseVisualStyleBackColor = true;
            // 
            // textResult
            // 
            textResult.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textResult.Font = new Font("Segoe UI", 9.75F);
            textResult.Location = new Point(6, 28);
            textResult.MaxLength = 0;
            textResult.Multiline = true;
            textResult.Name = "textResult";
            textResult.ScrollBars = ScrollBars.Vertical;
            textResult.Size = new Size(723, 377);
            textResult.TabIndex = 4;
            // 
            // labelResult
            // 
            labelResult.AutoSize = true;
            labelResult.Location = new Point(6, 9);
            labelResult.Name = "labelResult";
            labelResult.Size = new Size(42, 15);
            labelResult.TabIndex = 25;
            labelResult.Text = "Result:";
            // 
            // panelSeparator
            // 
            panelSeparator.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            panelSeparator.BackColor = SystemColors.ControlDarkDark;
            panelSeparator.Location = new Point(446, 5);
            panelSeparator.Name = "panelSeparator";
            panelSeparator.Size = new Size(3, 25);
            panelSeparator.TabIndex = 27;
            // 
            // buttonViewSystem
            // 
            buttonViewSystem.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonViewSystem.Location = new Point(615, 4);
            buttonViewSystem.Name = "buttonViewSystem";
            buttonViewSystem.Size = new Size(114, 26);
            buttonViewSystem.TabIndex = 26;
            buttonViewSystem.Text = "&View system root";
            buttonViewSystem.UseVisualStyleBackColor = true;
            buttonViewSystem.Click += buttonViewSystem_Click;
            // 
            // buttonCatalogPrompts
            // 
            buttonCatalogPrompts.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonCatalogPrompts.Location = new Point(455, 4);
            buttonCatalogPrompts.Name = "buttonCatalogPrompts";
            buttonCatalogPrompts.Size = new Size(154, 26);
            buttonCatalogPrompts.TabIndex = 25;
            buttonCatalogPrompts.Text = "Get prompt form &catalog";
            buttonCatalogPrompts.UseVisualStyleBackColor = true;
            buttonCatalogPrompts.Click += buttonCatalogPrompts_Click;
            // 
            // buttonRestart
            // 
            buttonRestart.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonRestart.Location = new Point(383, 4);
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
            labelPrompt.Location = new Point(8, 12);
            labelPrompt.Name = "labelPrompt";
            labelPrompt.Size = new Size(50, 15);
            labelPrompt.TabIndex = 24;
            labelPrompt.Text = "Prompt:";
            // 
            // textPrompt
            // 
            textPrompt.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textPrompt.Font = new Font("Segoe UI", 9.75F);
            textPrompt.Location = new Point(6, 34);
            textPrompt.MaxLength = 0;
            textPrompt.Multiline = true;
            textPrompt.Name = "textPrompt";
            textPrompt.ScrollBars = ScrollBars.Vertical;
            textPrompt.Size = new Size(723, 140);
            textPrompt.TabIndex = 0;
            // 
            // buttonSend
            // 
            buttonSend.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonSend.Location = new Point(321, 4);
            buttonSend.Name = "buttonSend";
            buttonSend.Size = new Size(56, 26);
            buttonSend.TabIndex = 2;
            buttonSend.Text = "&Send";
            buttonSend.UseVisualStyleBackColor = true;
            buttonSend.Click += buttonSend_Click;
            // 
            // KntChatGPTForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(735, 623);
            Controls.Add(splitChat);
            Controls.Add(statusStripChat);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "KntChatGPTForm";
            Text = "Simple ChatGPT";
            FormClosing += KntChatGPTForm_FormClosing;
            Load += ChatGPTForm_Load;
            statusStripChat.ResumeLayout(false);
            statusStripChat.PerformLayout();
            splitChat.Panel1.ResumeLayout(false);
            splitChat.Panel1.PerformLayout();
            splitChat.Panel2.ResumeLayout(false);
            splitChat.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitChat).EndInit();
            splitChat.ResumeLayout(false);
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
        private RadioButton radioGetStream;
        private RadioButton radioGetCompletion;
        private TextBox textResult;
        private Label labelResult;
        private Button buttonRestart;
        private Label labelPrompt;
        private TextBox textPrompt;
        private Button buttonSend;
        private Button buttonCatalogPrompts;
        private ToolStripStatusLabel toolStripStatusServiceRef;
        private ToolStripStatusLabel toolStripStatusLabel2;
        private Button buttonViewSystem;
        private Panel panelSeparator;
    }
}