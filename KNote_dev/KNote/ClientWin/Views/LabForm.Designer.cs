namespace KNote.ClientWin.Views
{
    partial class LabForm
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
            this.buttonTest1 = new System.Windows.Forms.Button();
            this.buttonTest2 = new System.Windows.Forms.Button();
            this.listMessages = new System.Windows.Forms.ListView();
            this.panelTest1 = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // buttonTest1
            // 
            this.buttonTest1.Location = new System.Drawing.Point(12, 12);
            this.buttonTest1.Name = "buttonTest1";
            this.buttonTest1.Size = new System.Drawing.Size(161, 23);
            this.buttonTest1.TabIndex = 0;
            this.buttonTest1.Text = "Test 1";
            this.buttonTest1.UseVisualStyleBackColor = true;
            this.buttonTest1.Click += new System.EventHandler(this.buttonTest1_Click);
            // 
            // buttonTest2
            // 
            this.buttonTest2.Location = new System.Drawing.Point(12, 41);
            this.buttonTest2.Name = "buttonTest2";
            this.buttonTest2.Size = new System.Drawing.Size(159, 24);
            this.buttonTest2.TabIndex = 1;
            this.buttonTest2.Text = "Test 2";
            this.buttonTest2.UseVisualStyleBackColor = true;
            this.buttonTest2.Click += new System.EventHandler(this.buttonTest2_Click);
            // 
            // listMessages
            // 
            this.listMessages.HideSelection = false;
            this.listMessages.Location = new System.Drawing.Point(12, 174);
            this.listMessages.Name = "listMessages";
            this.listMessages.Size = new System.Drawing.Size(294, 241);
            this.listMessages.TabIndex = 2;
            this.listMessages.UseCompatibleStateImageBehavior = false;
            // 
            // panelTest1
            // 
            this.panelTest1.BackColor = System.Drawing.Color.White;
            this.panelTest1.Location = new System.Drawing.Point(361, 178);
            this.panelTest1.Name = "panelTest1";
            this.panelTest1.Size = new System.Drawing.Size(282, 236);
            this.panelTest1.TabIndex = 3;
            // 
            // LabForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 427);
            this.Controls.Add(this.panelTest1);
            this.Controls.Add(this.listMessages);
            this.Controls.Add(this.buttonTest2);
            this.Controls.Add(this.buttonTest1);
            this.Name = "LabForm";
            this.Text = "LabForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonTest1;
        private System.Windows.Forms.Button buttonTest2;
        private System.Windows.Forms.ListView listMessages;
        private System.Windows.Forms.Panel panelTest1;
    }
}