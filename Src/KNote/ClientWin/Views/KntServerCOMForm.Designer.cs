namespace KNote.ClientWin.Views
{
    partial class KntServerCOMForm
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
            textBoxSend = new TextBox();
            buttonStart = new Button();
            buttonSend = new Button();
            listBoxEcho = new ListBox();
            buttonStop = new Button();
            label1 = new Label();
            label2 = new Label();
            statusInfo = new StatusStrip();
            statusLabelInfo = new ToolStripStatusLabel();
            button1 = new Button();
            statusInfo.SuspendLayout();
            SuspendLayout();
            // 
            // textBoxSend
            // 
            textBoxSend.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textBoxSend.Location = new Point(14, 85);
            textBoxSend.Margin = new Padding(4, 3, 4, 3);
            textBoxSend.Multiline = true;
            textBoxSend.Name = "textBoxSend";
            textBoxSend.ScrollBars = ScrollBars.Vertical;
            textBoxSend.Size = new Size(760, 376);
            textBoxSend.TabIndex = 0;
            // 
            // buttonStart
            // 
            buttonStart.Location = new Point(14, 14);
            buttonStart.Margin = new Padding(4, 3, 4, 3);
            buttonStart.Name = "buttonStart";
            buttonStart.Size = new Size(86, 28);
            buttonStart.TabIndex = 1;
            buttonStart.Text = "&Start service";
            buttonStart.UseVisualStyleBackColor = true;
            buttonStart.Click += buttonStart_Click;
            // 
            // buttonSend
            // 
            buttonSend.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonSend.Location = new Point(586, 58);
            buttonSend.Margin = new Padding(4, 3, 4, 3);
            buttonSend.Name = "buttonSend";
            buttonSend.Size = new Size(189, 24);
            buttonSend.TabIndex = 2;
            buttonSend.Text = "Send text &to client";
            buttonSend.UseVisualStyleBackColor = true;
            buttonSend.Click += buttonSend_Click;
            // 
            // listBoxEcho
            // 
            listBoxEcho.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listBoxEcho.FormattingEnabled = true;
            listBoxEcho.ItemHeight = 15;
            listBoxEcho.Location = new Point(14, 504);
            listBoxEcho.Margin = new Padding(4, 3, 4, 3);
            listBoxEcho.Name = "listBoxEcho";
            listBoxEcho.Size = new Size(760, 139);
            listBoxEcho.TabIndex = 3;
            // 
            // buttonStop
            // 
            buttonStop.Location = new Point(107, 14);
            buttonStop.Margin = new Padding(4, 3, 4, 3);
            buttonStop.Name = "buttonStop";
            buttonStop.Size = new Size(86, 28);
            buttonStop.TabIndex = 4;
            buttonStop.Text = "Sto&p service";
            buttonStop.UseVisualStyleBackColor = true;
            buttonStop.Click += buttonStop_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(14, 486);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(97, 15);
            label1.TabIndex = 5;
            label1.Text = "Echo from client:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(14, 67);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(81, 15);
            label2.TabIndex = 6;
            label2.Text = "Text for client:";
            // 
            // statusInfo
            // 
            statusInfo.Items.AddRange(new ToolStripItem[] { statusLabelInfo });
            statusInfo.Location = new Point(0, 662);
            statusInfo.Name = "statusInfo";
            statusInfo.Padding = new Padding(1, 0, 16, 0);
            statusInfo.Size = new Size(789, 22);
            statusInfo.TabIndex = 8;
            statusInfo.Text = "statusStrip1";
            // 
            // statusLabelInfo
            // 
            statusLabelInfo.Name = "statusLabelInfo";
            statusLabelInfo.Size = new Size(16, 17);
            statusLabelInfo.Text = "...";
            // 
            // button1
            // 
            button1.Location = new Point(636, 14);
            button1.Margin = new Padding(4, 3, 4, 3);
            button1.Name = "button1";
            button1.Size = new Size(139, 24);
            button1.TabIndex = 9;
            button1.Text = "Test >>>>";
            button1.UseVisualStyleBackColor = true;
            button1.Visible = false;
            button1.Click += button1_Click;
            // 
            // KntServerCOMForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(789, 684);
            Controls.Add(button1);
            Controls.Add(statusInfo);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(buttonStop);
            Controls.Add(listBoxEcho);
            Controls.Add(buttonSend);
            Controls.Add(buttonStart);
            Controls.Add(textBoxSend);
            Margin = new Padding(4, 3, 4, 3);
            Name = "KntServerCOMForm";
            Text = "KNote ServerCOM";
            FormClosing += KntServerCOMForm_FormClosing;
            Load += KntServerCOMForm_Load;
            statusInfo.ResumeLayout(false);
            statusInfo.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.TextBox textBoxSend;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonSend;
        private System.Windows.Forms.ListBox listBoxEcho;
        private System.Windows.Forms.Button buttonStop;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.StatusStrip statusInfo;
        private System.Windows.Forms.ToolStripStatusLabel statusLabelInfo;
        private System.Windows.Forms.Button button1;
    }
}