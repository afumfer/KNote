namespace KNote.ClientWin.Views
{
    partial class MonitorForm
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
            this.sqliteConnection1 = new Microsoft.Data.Sqlite.SqliteConnection();
            this.listBoxMessages = new System.Windows.Forms.ListBox();
            this.buttonClearMessages = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // sqliteConnection1
            // 
            this.sqliteConnection1.DefaultTimeout = 30;
            // 
            // listBoxMessages
            // 
            this.listBoxMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxMessages.FormattingEnabled = true;
            this.listBoxMessages.ItemHeight = 15;
            this.listBoxMessages.Location = new System.Drawing.Point(2, 42);
            this.listBoxMessages.Name = "listBoxMessages";
            this.listBoxMessages.Size = new System.Drawing.Size(942, 274);
            this.listBoxMessages.TabIndex = 0;
            // 
            // buttonClearMessages
            // 
            this.buttonClearMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClearMessages.Location = new System.Drawing.Point(817, 9);
            this.buttonClearMessages.Name = "buttonClearMessages";
            this.buttonClearMessages.Size = new System.Drawing.Size(119, 27);
            this.buttonClearMessages.TabIndex = 1;
            this.buttonClearMessages.Text = "Clear messages";
            this.buttonClearMessages.UseVisualStyleBackColor = true;
            this.buttonClearMessages.Click += new System.EventHandler(this.buttonClearMessages_Click);
            // 
            // MonitorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(948, 327);
            this.Controls.Add(this.buttonClearMessages);
            this.Controls.Add(this.listBoxMessages);
            this.Name = "MonitorForm";
            this.Text = "KNote monitor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MonitorForm_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Data.Sqlite.SqliteConnection sqliteConnection1;
        private System.Windows.Forms.ListBox listBoxMessages;
        private System.Windows.Forms.Button buttonClearMessages;
    }
}