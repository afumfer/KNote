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
            label3 = new Label();
            label2 = new Label();
            textPrompt = new TextBox();
            buttonSend = new Button();
            textResult = new TextBox();
            buttonRestart = new Button();
            SuspendLayout();
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(6, 55);
            label3.Name = "label3";
            label3.Size = new Size(42, 15);
            label3.TabIndex = 19;
            label3.Text = "Result:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 15);
            label2.Name = "label2";
            label2.Size = new Size(50, 15);
            label2.TabIndex = 18;
            label2.Text = "Prompt:";
            // 
            // textPrompt
            // 
            textPrompt.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textPrompt.Location = new Point(68, 15);
            textPrompt.Name = "textPrompt";
            textPrompt.Size = new Size(607, 23);
            textPrompt.TabIndex = 16;
            // 
            // buttonSend
            // 
            buttonSend.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonSend.Location = new Point(681, 11);
            buttonSend.Name = "buttonSend";
            buttonSend.Size = new Size(61, 29);
            buttonSend.TabIndex = 17;
            buttonSend.Text = "Send";
            buttonSend.UseVisualStyleBackColor = true;
            buttonSend.Click += buttonSend_Click;
            // 
            // textResult
            // 
            textResult.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textResult.Location = new Point(12, 73);
            textResult.Multiline = true;
            textResult.Name = "textResult";
            textResult.Size = new Size(797, 436);
            textResult.TabIndex = 20;
            // 
            // buttonRestart
            // 
            buttonRestart.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonRestart.Location = new Point(748, 12);
            buttonRestart.Name = "buttonRestart";
            buttonRestart.Size = new Size(61, 29);
            buttonRestart.TabIndex = 21;
            buttonRestart.Text = "Restart";
            buttonRestart.UseVisualStyleBackColor = true;
            buttonRestart.Click += buttonRestart_Click;
            // 
            // ChatGPTForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(821, 521);
            Controls.Add(buttonRestart);
            Controls.Add(textResult);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(textPrompt);
            Controls.Add(buttonSend);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "ChatGPTForm";
            Text = "Simple ChatGPT";
            Load += ChatGPTForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label3;
        private Label label2;
        private TextBox textPrompt;
        private Button buttonSend;
        private TextBox textResult;
        private Button buttonRestart;
    }
}