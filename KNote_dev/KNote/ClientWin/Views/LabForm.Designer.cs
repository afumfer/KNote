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
            this.buttonTest3 = new System.Windows.Forms.Button();
            this.labelInfo1 = new System.Windows.Forms.Label();
            this.labelInfo2 = new System.Windows.Forms.Label();
            this.labelInfo3 = new System.Windows.Forms.Label();
            this.buttonTest4 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonTest1
            // 
            this.buttonTest1.Location = new System.Drawing.Point(12, 12);
            this.buttonTest1.Name = "buttonTest1";
            this.buttonTest1.Size = new System.Drawing.Size(294, 23);
            this.buttonTest1.TabIndex = 0;
            this.buttonTest1.Text = "Test 1 - Monitor Component";
            this.buttonTest1.UseVisualStyleBackColor = true;
            this.buttonTest1.Click += new System.EventHandler(this.buttonTest1_Click);
            // 
            // buttonTest2
            // 
            this.buttonTest2.Location = new System.Drawing.Point(14, 41);
            this.buttonTest2.Name = "buttonTest2";
            this.buttonTest2.Size = new System.Drawing.Size(292, 24);
            this.buttonTest2.TabIndex = 1;
            this.buttonTest2.Text = "Test 2 - Run Folders /Notes selector";
            this.buttonTest2.UseVisualStyleBackColor = true;
            this.buttonTest2.Click += new System.EventHandler(this.buttonTest2_Click);
            // 
            // listMessages
            // 
            this.listMessages.HideSelection = false;
            this.listMessages.Location = new System.Drawing.Point(12, 290);
            this.listMessages.Name = "listMessages";
            this.listMessages.Size = new System.Drawing.Size(294, 241);
            this.listMessages.TabIndex = 2;
            this.listMessages.UseCompatibleStateImageBehavior = false;
            // 
            // panelTest1
            // 
            this.panelTest1.BackColor = System.Drawing.Color.White;
            this.panelTest1.Location = new System.Drawing.Point(327, 295);
            this.panelTest1.Name = "panelTest1";
            this.panelTest1.Size = new System.Drawing.Size(282, 236);
            this.panelTest1.TabIndex = 3;
            // 
            // buttonTest3
            // 
            this.buttonTest3.Location = new System.Drawing.Point(12, 71);
            this.buttonTest3.Name = "buttonTest3";
            this.buttonTest3.Size = new System.Drawing.Size(294, 24);
            this.buttonTest3.TabIndex = 4;
            this.buttonTest3.Text = "Test 3- Run KNoteManagment";
            this.buttonTest3.UseVisualStyleBackColor = true;
            this.buttonTest3.Click += new System.EventHandler(this.buttonTest3_Click);
            // 
            // labelInfo1
            // 
            this.labelInfo1.AutoSize = true;
            this.labelInfo1.Location = new System.Drawing.Point(12, 180);
            this.labelInfo1.Name = "labelInfo1";
            this.labelInfo1.Size = new System.Drawing.Size(37, 15);
            this.labelInfo1.TabIndex = 5;
            this.labelInfo1.Text = "Info 1";
            // 
            // labelInfo2
            // 
            this.labelInfo2.AutoSize = true;
            this.labelInfo2.Location = new System.Drawing.Point(12, 205);
            this.labelInfo2.Name = "labelInfo2";
            this.labelInfo2.Size = new System.Drawing.Size(37, 15);
            this.labelInfo2.TabIndex = 6;
            this.labelInfo2.Text = "Info 2";
            // 
            // labelInfo3
            // 
            this.labelInfo3.AutoSize = true;
            this.labelInfo3.Location = new System.Drawing.Point(11, 229);
            this.labelInfo3.Name = "labelInfo3";
            this.labelInfo3.Size = new System.Drawing.Size(37, 15);
            this.labelInfo3.TabIndex = 7;
            this.labelInfo3.Text = "Info 3";
            // 
            // buttonTest4
            // 
            this.buttonTest4.Location = new System.Drawing.Point(506, 12);
            this.buttonTest4.Name = "buttonTest4";
            this.buttonTest4.Size = new System.Drawing.Size(135, 24);
            this.buttonTest4.TabIndex = 8;
            this.buttonTest4.Text = "Test 4";
            this.buttonTest4.UseVisualStyleBackColor = true;
            this.buttonTest4.Click += new System.EventHandler(this.buttonTest4_Click);
            // 
            // LabForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(750, 548);
            this.Controls.Add(this.buttonTest4);
            this.Controls.Add(this.labelInfo3);
            this.Controls.Add(this.labelInfo2);
            this.Controls.Add(this.labelInfo1);
            this.Controls.Add(this.buttonTest3);
            this.Controls.Add(this.panelTest1);
            this.Controls.Add(this.listMessages);
            this.Controls.Add(this.buttonTest2);
            this.Controls.Add(this.buttonTest1);
            this.Name = "LabForm";
            this.Text = "LabForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonTest1;
        private System.Windows.Forms.Button buttonTest2;
        private System.Windows.Forms.ListView listMessages;
        private System.Windows.Forms.Panel panelTest1;
        private System.Windows.Forms.Button buttonTest3;
        private System.Windows.Forms.Label labelInfo1;
        private System.Windows.Forms.Label labelInfo2;
        private System.Windows.Forms.Label labelInfo3;
        private System.Windows.Forms.Button buttonTest4;
    }
}