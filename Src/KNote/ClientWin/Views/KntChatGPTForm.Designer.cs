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
            toolStripStatusLabelTokens = new ToolStripStatusLabel();
            toolStripStatusLabelProcessingTime = new ToolStripStatusLabel();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            toolStripStatusLabelProcessing = new ToolStripStatusLabel();
            splitChat = new SplitContainer();
            radioGetStream = new RadioButton();
            radioGetCompletion = new RadioButton();
            textResult = new TextBox();
            labelResult = new Label();
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
            statusStripChat.Items.AddRange(new ToolStripItem[] { toolStripStatusLabelTokens, toolStripStatusLabelProcessingTime, toolStripStatusLabel1, toolStripStatusLabelProcessing });
            statusStripChat.Location = new Point(0, 655);
            statusStripChat.Name = "statusStripChat";
            statusStripChat.Padding = new Padding(1, 0, 16, 0);
            statusStripChat.Size = new Size(782, 26);
            statusStripChat.TabIndex = 22;
            // 
            // toolStripStatusLabelTokens
            // 
            toolStripStatusLabelTokens.BorderStyle = Border3DStyle.Raised;
            toolStripStatusLabelTokens.Name = "toolStripStatusLabelTokens";
            toolStripStatusLabelTokens.Size = new Size(69, 20);
            toolStripStatusLabelTokens.Text = "Tokens: 0";
            // 
            // toolStripStatusLabelProcessingTime
            // 
            toolStripStatusLabelProcessingTime.Name = "toolStripStatusLabelProcessingTime";
            toolStripStatusLabelProcessingTime.Size = new Size(13, 20);
            toolStripStatusLabelProcessingTime.Text = " ";
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(13, 20);
            toolStripStatusLabel1.Text = "|";
            // 
            // toolStripStatusLabelProcessing
            // 
            toolStripStatusLabelProcessing.BorderStyle = Border3DStyle.Raised;
            toolStripStatusLabelProcessing.Name = "toolStripStatusLabelProcessing";
            toolStripStatusLabelProcessing.Size = new Size(0, 20);
            // 
            // splitChat
            // 
            splitChat.Dock = DockStyle.Fill;
            splitChat.Location = new Point(0, 0);
            splitChat.Margin = new Padding(3, 4, 3, 4);
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
            splitChat.Panel2.Controls.Add(buttonRestart);
            splitChat.Panel2.Controls.Add(labelPrompt);
            splitChat.Panel2.Controls.Add(textPrompt);
            splitChat.Panel2.Controls.Add(buttonSend);
            splitChat.Panel2MinSize = 50;
            splitChat.Size = new Size(782, 655);
            splitChat.SplitterDistance = 527;
            splitChat.SplitterWidth = 8;
            splitChat.TabIndex = 25;
            // 
            // radioGetStream
            // 
            radioGetStream.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            radioGetStream.AutoSize = true;
            radioGetStream.Checked = true;
            radioGetStream.Location = new Point(533, 9);
            radioGetStream.Margin = new Padding(3, 4, 3, 4);
            radioGetStream.Name = "radioGetStream";
            radioGetStream.Size = new Size(104, 24);
            radioGetStream.TabIndex = 5;
            radioGetStream.TabStop = true;
            radioGetStream.Text = "Get Stream";
            radioGetStream.UseVisualStyleBackColor = true;
            // 
            // radioGetCompletion
            // 
            radioGetCompletion.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            radioGetCompletion.AutoSize = true;
            radioGetCompletion.Location = new Point(643, 9);
            radioGetCompletion.Margin = new Padding(3, 4, 3, 4);
            radioGetCompletion.Name = "radioGetCompletion";
            radioGetCompletion.Size = new Size(135, 24);
            radioGetCompletion.TabIndex = 6;
            radioGetCompletion.Text = "Get Completion";
            radioGetCompletion.UseVisualStyleBackColor = true;
            // 
            // textResult
            // 
            textResult.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textResult.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            textResult.Location = new Point(7, 37);
            textResult.Margin = new Padding(3, 4, 3, 4);
            textResult.Multiline = true;
            textResult.Name = "textResult";
            textResult.ScrollBars = ScrollBars.Vertical;
            textResult.Size = new Size(767, 482);
            textResult.TabIndex = 4;
            // 
            // labelResult
            // 
            labelResult.AutoSize = true;
            labelResult.Location = new Point(3, 12);
            labelResult.Name = "labelResult";
            labelResult.Size = new Size(52, 20);
            labelResult.TabIndex = 25;
            labelResult.Text = "Result:";
            // 
            // buttonRestart
            // 
            buttonRestart.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonRestart.Location = new Point(712, 52);
            buttonRestart.Margin = new Padding(3, 4, 3, 4);
            buttonRestart.Name = "buttonRestart";
            buttonRestart.Size = new Size(63, 36);
            buttonRestart.TabIndex = 3;
            buttonRestart.Text = "&Restart";
            buttonRestart.UseVisualStyleBackColor = true;
            buttonRestart.Click += buttonRestart_Click;
            // 
            // labelPrompt
            // 
            labelPrompt.AutoSize = true;
            labelPrompt.Location = new Point(6, 6);
            labelPrompt.Name = "labelPrompt";
            labelPrompt.Size = new Size(61, 20);
            labelPrompt.TabIndex = 24;
            labelPrompt.Text = "Prompt:";
            // 
            // textPrompt
            // 
            textPrompt.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textPrompt.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            textPrompt.Location = new Point(74, 8);
            textPrompt.Margin = new Padding(3, 4, 3, 4);
            textPrompt.Multiline = true;
            textPrompt.Name = "textPrompt";
            textPrompt.ScrollBars = ScrollBars.Vertical;
            textPrompt.Size = new Size(631, 85);
            textPrompt.TabIndex = 0;
            // 
            // buttonSend
            // 
            buttonSend.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonSend.Location = new Point(712, 8);
            buttonSend.Margin = new Padding(3, 4, 3, 4);
            buttonSend.Name = "buttonSend";
            buttonSend.Size = new Size(63, 36);
            buttonSend.TabIndex = 2;
            buttonSend.Text = "&Send";
            buttonSend.UseVisualStyleBackColor = true;
            buttonSend.Click += buttonSend_Click;
            // 
            // KntChatGPTForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(782, 681);
            Controls.Add(splitChat);
            Controls.Add(statusStripChat);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 4, 3, 4);
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
    }
}