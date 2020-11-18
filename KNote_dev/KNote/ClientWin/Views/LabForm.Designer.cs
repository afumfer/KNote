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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabAppLab = new System.Windows.Forms.TabPage();
            this.tabKntScriptLab = new System.Windows.Forms.TabPage();
            this.buttonRunBackground = new System.Windows.Forms.Button();
            this.buttonInteract = new System.Windows.Forms.Button();
            this.buttonShowConsole = new System.Windows.Forms.Button();
            this.buttonRunScript = new System.Windows.Forms.Button();
            this.groupSamples = new System.Windows.Forms.GroupBox();
            this.buttonRunSample = new System.Windows.Forms.Button();
            this.buttonShowSample = new System.Windows.Forms.Button();
            this.listSamples = new System.Windows.Forms.ListBox();
            this.labelInfo1 = new System.Windows.Forms.Label();
            this.labelInfo2 = new System.Windows.Forms.Label();
            this.buttonTest1 = new System.Windows.Forms.Button();
            this.buttonTest2 = new System.Windows.Forms.Button();
            this.buttonTest3 = new System.Windows.Forms.Button();
            this.buttonTest4 = new System.Windows.Forms.Button();
            this.listMessages = new System.Windows.Forms.ListBox();
            this.panelTest1 = new System.Windows.Forms.Panel();
            this.tabControl1.SuspendLayout();
            this.tabAppLab.SuspendLayout();
            this.tabKntScriptLab.SuspendLayout();
            this.groupSamples.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabAppLab);
            this.tabControl1.Controls.Add(this.tabKntScriptLab);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(670, 577);
            this.tabControl1.TabIndex = 4;
            // 
            // tabAppLab
            // 
            this.tabAppLab.Controls.Add(this.panelTest1);
            this.tabAppLab.Controls.Add(this.listMessages);
            this.tabAppLab.Controls.Add(this.buttonTest4);
            this.tabAppLab.Controls.Add(this.buttonTest3);
            this.tabAppLab.Controls.Add(this.buttonTest2);
            this.tabAppLab.Controls.Add(this.buttonTest1);
            this.tabAppLab.Controls.Add(this.labelInfo2);
            this.tabAppLab.Controls.Add(this.labelInfo1);
            this.tabAppLab.Location = new System.Drawing.Point(4, 24);
            this.tabAppLab.Name = "tabAppLab";
            this.tabAppLab.Padding = new System.Windows.Forms.Padding(3);
            this.tabAppLab.Size = new System.Drawing.Size(662, 549);
            this.tabAppLab.TabIndex = 0;
            this.tabAppLab.Text = "Lab app components";
            this.tabAppLab.UseVisualStyleBackColor = true;
            // 
            // tabKntScriptLab
            // 
            this.tabKntScriptLab.Controls.Add(this.groupSamples);
            this.tabKntScriptLab.Controls.Add(this.buttonRunBackground);
            this.tabKntScriptLab.Controls.Add(this.buttonInteract);
            this.tabKntScriptLab.Controls.Add(this.buttonShowConsole);
            this.tabKntScriptLab.Controls.Add(this.buttonRunScript);
            this.tabKntScriptLab.Location = new System.Drawing.Point(4, 24);
            this.tabKntScriptLab.Name = "tabKntScriptLab";
            this.tabKntScriptLab.Padding = new System.Windows.Forms.Padding(3);
            this.tabKntScriptLab.Size = new System.Drawing.Size(789, 549);
            this.tabKntScriptLab.TabIndex = 1;
            this.tabKntScriptLab.Text = "KntScript lab";
            this.tabKntScriptLab.UseVisualStyleBackColor = true;
            // 
            // buttonRunBackground
            // 
            this.buttonRunBackground.Location = new System.Drawing.Point(16, 82);
            this.buttonRunBackground.Name = "buttonRunBackground";
            this.buttonRunBackground.Size = new System.Drawing.Size(243, 31);
            this.buttonRunBackground.TabIndex = 6;
            this.buttonRunBackground.Text = "Run simple script in bakground";
            this.buttonRunBackground.UseVisualStyleBackColor = true;
            this.buttonRunBackground.Click += new System.EventHandler(this.buttonRunBackground_Click);
            // 
            // buttonInteract
            // 
            this.buttonInteract.Location = new System.Drawing.Point(16, 48);
            this.buttonInteract.Name = "buttonInteract";
            this.buttonInteract.Size = new System.Drawing.Size(243, 28);
            this.buttonInteract.TabIndex = 5;
            this.buttonInteract.Text = "Interacting with kntscript (embedded code)";
            this.buttonInteract.UseVisualStyleBackColor = true;
            this.buttonInteract.Click += new System.EventHandler(this.buttonInteract_Click);
            // 
            // buttonShowConsole
            // 
            this.buttonShowConsole.Location = new System.Drawing.Point(16, 119);
            this.buttonShowConsole.Name = "buttonShowConsole";
            this.buttonShowConsole.Size = new System.Drawing.Size(243, 29);
            this.buttonShowConsole.TabIndex = 7;
            this.buttonShowConsole.Text = "Show KntConsole";
            this.buttonShowConsole.UseVisualStyleBackColor = true;
            this.buttonShowConsole.Click += new System.EventHandler(this.buttonShowConsole_Click);
            // 
            // buttonRunScript
            // 
            this.buttonRunScript.Location = new System.Drawing.Point(16, 14);
            this.buttonRunScript.Name = "buttonRunScript";
            this.buttonRunScript.Size = new System.Drawing.Size(243, 28);
            this.buttonRunScript.TabIndex = 4;
            this.buttonRunScript.Text = "Run simple script (embedded code)";
            this.buttonRunScript.UseVisualStyleBackColor = true;
            this.buttonRunScript.Click += new System.EventHandler(this.buttonRunScript_Click);
            // 
            // groupSamples
            // 
            this.groupSamples.Controls.Add(this.buttonRunSample);
            this.groupSamples.Controls.Add(this.buttonShowSample);
            this.groupSamples.Controls.Add(this.listSamples);
            this.groupSamples.Location = new System.Drawing.Point(16, 154);
            this.groupSamples.Name = "groupSamples";
            this.groupSamples.Size = new System.Drawing.Size(451, 307);
            this.groupSamples.TabIndex = 8;
            this.groupSamples.TabStop = false;
            this.groupSamples.Text = "Samples";
            // 
            // buttonRunSample
            // 
            this.buttonRunSample.Location = new System.Drawing.Point(262, 59);
            this.buttonRunSample.Name = "buttonRunSample";
            this.buttonRunSample.Size = new System.Drawing.Size(173, 30);
            this.buttonRunSample.TabIndex = 5;
            this.buttonRunSample.Text = "Run sample";
            this.buttonRunSample.UseVisualStyleBackColor = true;
            this.buttonRunSample.Click += new System.EventHandler(this.buttonRunSample_Click);
            // 
            // buttonShowSample
            // 
            this.buttonShowSample.Location = new System.Drawing.Point(262, 22);
            this.buttonShowSample.Name = "buttonShowSample";
            this.buttonShowSample.Size = new System.Drawing.Size(173, 31);
            this.buttonShowSample.TabIndex = 4;
            this.buttonShowSample.Text = "Show sample in KntConsole";
            this.buttonShowSample.UseVisualStyleBackColor = true;
            this.buttonShowSample.Click += new System.EventHandler(this.buttonShowSample_Click);
            // 
            // listSamples
            // 
            this.listSamples.FormattingEnabled = true;
            this.listSamples.ItemHeight = 15;
            this.listSamples.Location = new System.Drawing.Point(13, 19);
            this.listSamples.Name = "listSamples";
            this.listSamples.Size = new System.Drawing.Size(230, 274);
            this.listSamples.TabIndex = 2;
            this.listSamples.SelectedIndexChanged += new System.EventHandler(this.listSamples_SelectedIndexChanged);
            // 
            // labelInfo1
            // 
            this.labelInfo1.AutoSize = true;
            this.labelInfo1.Location = new System.Drawing.Point(290, 16);
            this.labelInfo1.Name = "labelInfo1";
            this.labelInfo1.Size = new System.Drawing.Size(38, 15);
            this.labelInfo1.TabIndex = 0;
            this.labelInfo1.Text = "label1";
            // 
            // labelInfo2
            // 
            this.labelInfo2.AutoSize = true;
            this.labelInfo2.Location = new System.Drawing.Point(290, 44);
            this.labelInfo2.Name = "labelInfo2";
            this.labelInfo2.Size = new System.Drawing.Size(38, 15);
            this.labelInfo2.TabIndex = 1;
            this.labelInfo2.Text = "label1";
            // 
            // buttonTest1
            // 
            this.buttonTest1.Location = new System.Drawing.Point(12, 16);
            this.buttonTest1.Name = "buttonTest1";
            this.buttonTest1.Size = new System.Drawing.Size(196, 29);
            this.buttonTest1.TabIndex = 2;
            this.buttonTest1.Text = "Run monitor";
            this.buttonTest1.UseVisualStyleBackColor = true;
            this.buttonTest1.Click += new System.EventHandler(this.buttonTest1_Click);
            // 
            // buttonTest2
            // 
            this.buttonTest2.Location = new System.Drawing.Point(12, 51);
            this.buttonTest2.Name = "buttonTest2";
            this.buttonTest2.Size = new System.Drawing.Size(196, 29);
            this.buttonTest2.TabIndex = 3;
            this.buttonTest2.Text = "Run components";
            this.buttonTest2.UseVisualStyleBackColor = true;
            this.buttonTest2.Click += new System.EventHandler(this.buttonTest2_Click);
            // 
            // buttonTest3
            // 
            this.buttonTest3.Location = new System.Drawing.Point(13, 86);
            this.buttonTest3.Name = "buttonTest3";
            this.buttonTest3.Size = new System.Drawing.Size(195, 28);
            this.buttonTest3.TabIndex = 4;
            this.buttonTest3.Text = "Run managment component";
            this.buttonTest3.UseVisualStyleBackColor = true;
            this.buttonTest3.Click += new System.EventHandler(this.buttonTest3_Click);
            // 
            // buttonTest4
            // 
            this.buttonTest4.Location = new System.Drawing.Point(13, 120);
            this.buttonTest4.Name = "buttonTest4";
            this.buttonTest4.Size = new System.Drawing.Size(195, 28);
            this.buttonTest4.TabIndex = 5;
            this.buttonTest4.Text = "Test modal component";
            this.buttonTest4.UseVisualStyleBackColor = true;
            this.buttonTest4.Click += new System.EventHandler(this.buttonTest4_Click);
            // 
            // listMessages
            // 
            this.listMessages.FormattingEnabled = true;
            this.listMessages.ItemHeight = 15;
            this.listMessages.Location = new System.Drawing.Point(13, 223);
            this.listMessages.Name = "listMessages";
            this.listMessages.Size = new System.Drawing.Size(300, 304);
            this.listMessages.TabIndex = 6;
            // 
            // panelTest1
            // 
            this.panelTest1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTest1.Location = new System.Drawing.Point(329, 222);
            this.panelTest1.Name = "panelTest1";
            this.panelTest1.Size = new System.Drawing.Size(311, 304);
            this.panelTest1.TabIndex = 7;
            // 
            // DemoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(689, 601);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "DemoForm";
            this.Text = "KntScript AppHost demo";
            this.Load += new System.EventHandler(this.DemoForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabAppLab.ResumeLayout(false);
            this.tabAppLab.PerformLayout();
            this.tabKntScriptLab.ResumeLayout(false);
            this.groupSamples.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabAppLab;
        private System.Windows.Forms.TabPage tabKntScriptLab;
        private System.Windows.Forms.Button buttonRunBackground;
        private System.Windows.Forms.Button buttonInteract;
        private System.Windows.Forms.Button buttonShowConsole;
        private System.Windows.Forms.Button buttonRunScript;
        private System.Windows.Forms.GroupBox groupSamples;
        private System.Windows.Forms.Button buttonRunSample;
        private System.Windows.Forms.Button buttonShowSample;
        private System.Windows.Forms.ListBox listSamples;
        private System.Windows.Forms.Label labelInfo2;
        private System.Windows.Forms.Label labelInfo1;
        private System.Windows.Forms.Button buttonTest1;
        private System.Windows.Forms.Panel panelTest1;
        private System.Windows.Forms.ListBox listMessages;
        private System.Windows.Forms.Button buttonTest4;
        private System.Windows.Forms.Button buttonTest3;
        private System.Windows.Forms.Button buttonTest2;
    }
}