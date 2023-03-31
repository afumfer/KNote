namespace KNote.ClientWin.Views
{
    partial class KntChatForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KntChatForm));
            labelServer = new Label();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            listMessages = new ListBox();
            textMessage = new TextBox();
            buttonSend = new Button();
            SuspendLayout();
            // 
            // labelServer
            // 
            labelServer.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            labelServer.AutoSize = true;
            labelServer.Location = new Point(89, 459);
            labelServer.Name = "labelServer";
            labelServer.Size = new Size(16, 15);
            labelServer.TabIndex = 17;
            labelServer.Text = "...";
            // 
            // label4
            // 
            label4.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label4.AutoSize = true;
            label4.Location = new Point(5, 459);
            label4.Name = "label4";
            label4.Size = new Size(85, 15);
            label4.TabIndex = 16;
            label4.Text = "Chat Hub URL:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(6, 56);
            label3.Name = "label3";
            label3.Size = new Size(79, 15);
            label3.TabIndex = 15;
            label3.Text = "Messages list:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 16);
            label2.Name = "label2";
            label2.Size = new Size(56, 15);
            label2.TabIndex = 14;
            label2.Text = "Message:";
            // 
            // listMessages
            // 
            listMessages.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listMessages.FormattingEnabled = true;
            listMessages.ItemHeight = 15;
            listMessages.Location = new Point(5, 74);
            listMessages.Name = "listMessages";
            listMessages.Size = new Size(789, 379);
            listMessages.TabIndex = 2;
            // 
            // textMessage
            // 
            textMessage.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textMessage.Location = new Point(68, 13);
            textMessage.Name = "textMessage";
            textMessage.Size = new Size(636, 23);
            textMessage.TabIndex = 0;
            // 
            // buttonSend
            // 
            buttonSend.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonSend.Location = new Point(710, 11);
            buttonSend.Name = "buttonSend";
            buttonSend.Size = new Size(84, 29);
            buttonSend.TabIndex = 1;
            buttonSend.Text = "Send";
            buttonSend.UseVisualStyleBackColor = true;
            buttonSend.Click += buttonSend_Click;
            // 
            // KntChatForm
            // 
            AcceptButton = buttonSend;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 483);
            Controls.Add(labelServer);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(listMessages);
            Controls.Add(textMessage);
            Controls.Add(buttonSend);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "KntChatForm";
            Text = "Simple chat";
            FormClosing += KntChatForm_FormClosing;
            Load += ChatForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelServer;
        private Label label4;
        private Label label3;
        private Label label2;
        private ListBox listMessages;
        private TextBox textMessage;
        private Button buttonSend;
    }
}