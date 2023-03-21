namespace KNote.ClientWin.Views
{
    partial class ChatGPTForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChatGPTForm));
            labelResult = new Label();
            labelPrompt = new Label();
            textPrompt = new TextBox();
            buttonSend = new Button();
            textResult = new TextBox();
            buttonRestart = new Button();
            statusStripChat = new StatusStrip();
            toolStripStatusLabelTokens = new ToolStripStatusLabel();
            toolStripStatusLabelProcessingTime = new ToolStripStatusLabel();
            toolStripStatusLabelProcessing = new ToolStripStatusLabel();
            toolStripStatusLabel1 = new ToolStripStatusLabel();
            statusStripChat.SuspendLayout();
            SuspendLayout();
            // 
            // labelResult
            // 
            labelResult.AutoSize = true;
            labelResult.Location = new Point(10, 9);
            labelResult.Name = "labelResult";
            labelResult.Size = new Size(42, 15);
            labelResult.TabIndex = 19;
            labelResult.Text = "Result:";
            // 
            // labelPrompt
            // 
            labelPrompt.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            labelPrompt.AutoSize = true;
            labelPrompt.Location = new Point(6, 477);
            labelPrompt.Name = "labelPrompt";
            labelPrompt.Size = new Size(50, 15);
            labelPrompt.TabIndex = 18;
            labelPrompt.Text = "Prompt:";
            // 
            // textPrompt
            // 
            textPrompt.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textPrompt.Location = new Point(68, 473);
            textPrompt.Name = "textPrompt";
            textPrompt.Size = new Size(625, 23);
            textPrompt.TabIndex = 16;
            // 
            // buttonSend
            // 
            buttonSend.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonSend.Location = new Point(699, 469);
            buttonSend.Name = "buttonSend";
            buttonSend.Size = new Size(53, 27);
            buttonSend.TabIndex = 17;
            buttonSend.Text = "&Send";
            buttonSend.UseVisualStyleBackColor = true;
            buttonSend.Click += buttonSend_Click;
            // 
            // textResult
            // 
            textResult.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textResult.Location = new Point(10, 27);
            textResult.Multiline = true;
            textResult.Name = "textResult";
            textResult.ScrollBars = ScrollBars.Vertical;
            textResult.Size = new Size(803, 436);
            textResult.TabIndex = 20;
            // 
            // buttonRestart
            // 
            buttonRestart.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonRestart.Location = new Point(758, 469);
            buttonRestart.Name = "buttonRestart";
            buttonRestart.Size = new Size(55, 27);
            buttonRestart.TabIndex = 21;
            buttonRestart.Text = "&Restart";
            buttonRestart.UseVisualStyleBackColor = true;
            buttonRestart.Click += buttonRestart_Click;
            // 
            // statusStripChat
            // 
            statusStripChat.Items.AddRange(new ToolStripItem[] { toolStripStatusLabelTokens, toolStripStatusLabelProcessingTime, toolStripStatusLabel1, toolStripStatusLabelProcessing });
            statusStripChat.Location = new Point(0, 499);
            statusStripChat.Name = "statusStripChat";
            statusStripChat.Size = new Size(821, 22);
            statusStripChat.TabIndex = 22;
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
            // toolStripStatusLabelProcessing
            // 
            toolStripStatusLabelProcessing.BorderStyle = Border3DStyle.Raised;
            toolStripStatusLabelProcessing.Name = "toolStripStatusLabelProcessing";
            toolStripStatusLabelProcessing.Size = new Size(0, 17);
            // 
            // toolStripStatusLabel1
            // 
            toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            toolStripStatusLabel1.Size = new Size(10, 17);
            toolStripStatusLabel1.Text = "|";
            // 
            // ChatGPTForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(821, 521);
            Controls.Add(statusStripChat);
            Controls.Add(buttonRestart);
            Controls.Add(textResult);
            Controls.Add(labelResult);
            Controls.Add(labelPrompt);
            Controls.Add(textPrompt);
            Controls.Add(buttonSend);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "ChatGPTForm";
            Text = "Simple ChatGPT";
            Load += ChatGPTForm_Load;
            statusStripChat.ResumeLayout(false);
            statusStripChat.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelResult;
        private Label labelPrompt;
        private TextBox textPrompt;
        private Button buttonSend;
        private TextBox textResult;
        private Button buttonRestart;
        private StatusStrip statusStripChat;
        private ToolStripStatusLabel toolStripStatusLabelTokens;
        private ToolStripStatusLabel toolStripStatusLabelProcessing;
        private ToolStripStatusLabel toolStripStatusLabelProcessingTime;
        private ToolStripStatusLabel toolStripStatusLabel1;
    }
}