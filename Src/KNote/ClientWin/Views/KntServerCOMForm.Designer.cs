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
            this.textBoxSend = new System.Windows.Forms.TextBox();
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonSend = new System.Windows.Forms.Button();
            this.listBoxEcho = new System.Windows.Forms.ListBox();
            this.buttonStop = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.statusInfo = new System.Windows.Forms.StatusStrip();
            this.statusLabelInfo = new System.Windows.Forms.ToolStripStatusLabel();
            this.button1 = new System.Windows.Forms.Button();
            this.statusInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxSend
            // 
            this.textBoxSend.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSend.Location = new System.Drawing.Point(12, 74);
            this.textBoxSend.Multiline = true;
            this.textBoxSend.Name = "textBoxSend";
            this.textBoxSend.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxSend.Size = new System.Drawing.Size(652, 326);
            this.textBoxSend.TabIndex = 0;
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(12, 12);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(74, 24);
            this.buttonStart.TabIndex = 1;
            this.buttonStart.Text = "&Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonSend
            // 
            this.buttonSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSend.Location = new System.Drawing.Point(545, 53);
            this.buttonSend.Name = "buttonSend";
            this.buttonSend.Size = new System.Drawing.Size(119, 21);
            this.buttonSend.TabIndex = 2;
            this.buttonSend.Text = "Send &to client";
            this.buttonSend.UseVisualStyleBackColor = true;
            this.buttonSend.Click += new System.EventHandler(this.buttonSend_Click);
            // 
            // listBoxEcho
            // 
            this.listBoxEcho.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxEcho.FormattingEnabled = true;
            this.listBoxEcho.Location = new System.Drawing.Point(12, 437);
            this.listBoxEcho.Name = "listBoxEcho";
            this.listBoxEcho.Size = new System.Drawing.Size(656, 121);
            this.listBoxEcho.TabIndex = 3;
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(92, 12);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(74, 24);
            this.buttonStop.TabIndex = 4;
            this.buttonStop.Text = "Sto&p";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 421);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Echo from client:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Text to client:";
            // 
            // statusInfo
            // 
            this.statusInfo.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabelInfo});
            this.statusInfo.Location = new System.Drawing.Point(0, 571);
            this.statusInfo.Name = "statusInfo";
            this.statusInfo.Size = new System.Drawing.Size(676, 22);
            this.statusInfo.TabIndex = 8;
            this.statusInfo.Text = "statusStrip1";
            // 
            // statusLabelInfo
            // 
            this.statusLabelInfo.Name = "statusLabelInfo";
            this.statusLabelInfo.Size = new System.Drawing.Size(16, 17);
            this.statusLabelInfo.Text = "...";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(545, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(119, 21);
            this.button1.TabIndex = 9;
            this.button1.Text = "Test >>>>";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // ServerCOMForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(676, 593);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.statusInfo);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.listBoxEcho);
            this.Controls.Add(this.buttonSend);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.textBoxSend);
            this.Name = "ServerCOMForm";
            this.Text = "KNote ServerCOM";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.KntServerCOMForm_FormClosing);            
            this.statusInfo.ResumeLayout(false);
            this.statusInfo.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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