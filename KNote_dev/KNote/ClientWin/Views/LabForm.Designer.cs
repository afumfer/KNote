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
            this.listMessages = new System.Windows.Forms.ListBox();
            this.buttonTest4 = new System.Windows.Forms.Button();
            this.buttonTest1 = new System.Windows.Forms.Button();
            this.tabKntScriptLab = new System.Windows.Forms.TabPage();
            this.groupSamples = new System.Windows.Forms.GroupBox();
            this.buttonRunSample = new System.Windows.Forms.Button();
            this.buttonShowSample = new System.Windows.Forms.Button();
            this.listSamples = new System.Windows.Forms.ListBox();
            this.buttonRunBackground = new System.Windows.Forms.Button();
            this.buttonInteract = new System.Windows.Forms.Button();
            this.buttonShowConsole = new System.Windows.Forms.Button();
            this.buttonRunScript = new System.Windows.Forms.Button();
            this.tabRichEditor = new System.Windows.Forms.TabPage();
            this.htmlDescription = new Pavonis.Html.Editor.HtmlEditorControl();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabAppLab.SuspendLayout();
            this.tabKntScriptLab.SuspendLayout();
            this.groupSamples.SuspendLayout();
            this.tabRichEditor.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabAppLab);
            this.tabControl1.Controls.Add(this.tabKntScriptLab);
            this.tabControl1.Controls.Add(this.tabRichEditor);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(670, 577);
            this.tabControl1.TabIndex = 4;
            // 
            // tabAppLab
            // 
            this.tabAppLab.Controls.Add(this.label2);
            this.tabAppLab.Controls.Add(this.label1);
            this.tabAppLab.Controls.Add(this.listMessages);
            this.tabAppLab.Controls.Add(this.buttonTest4);
            this.tabAppLab.Controls.Add(this.buttonTest1);
            this.tabAppLab.Location = new System.Drawing.Point(4, 24);
            this.tabAppLab.Name = "tabAppLab";
            this.tabAppLab.Padding = new System.Windows.Forms.Padding(3);
            this.tabAppLab.Size = new System.Drawing.Size(662, 549);
            this.tabAppLab.TabIndex = 0;
            this.tabAppLab.Text = "Lab app components";
            this.tabAppLab.UseVisualStyleBackColor = true;
            // 
            // listMessages
            // 
            this.listMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listMessages.FormattingEnabled = true;
            this.listMessages.ItemHeight = 15;
            this.listMessages.Location = new System.Drawing.Point(13, 255);
            this.listMessages.Name = "listMessages";
            this.listMessages.Size = new System.Drawing.Size(636, 274);
            this.listMessages.TabIndex = 6;
            // 
            // buttonTest4
            // 
            this.buttonTest4.Location = new System.Drawing.Point(12, 51);
            this.buttonTest4.Name = "buttonTest4";
            this.buttonTest4.Size = new System.Drawing.Size(300, 28);
            this.buttonTest4.TabIndex = 5;
            this.buttonTest4.Text = "Import anotas xml";
            this.buttonTest4.UseVisualStyleBackColor = true;
            this.buttonTest4.Click += new System.EventHandler(this.buttonTest4_Click);
            // 
            // buttonTest1
            // 
            this.buttonTest1.Location = new System.Drawing.Point(12, 16);
            this.buttonTest1.Name = "buttonTest1";
            this.buttonTest1.Size = new System.Drawing.Size(301, 29);
            this.buttonTest1.TabIndex = 2;
            this.buttonTest1.Text = "Run monitor";
            this.buttonTest1.UseVisualStyleBackColor = true;
            this.buttonTest1.Click += new System.EventHandler(this.buttonTest1_Click);
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
            this.tabKntScriptLab.Size = new System.Drawing.Size(662, 549);
            this.tabKntScriptLab.TabIndex = 1;
            this.tabKntScriptLab.Text = "KntScript lab";
            this.tabKntScriptLab.UseVisualStyleBackColor = true;
            // 
            // groupSamples
            // 
            this.groupSamples.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupSamples.Controls.Add(this.buttonRunSample);
            this.groupSamples.Controls.Add(this.buttonShowSample);
            this.groupSamples.Controls.Add(this.listSamples);
            this.groupSamples.Location = new System.Drawing.Point(16, 154);
            this.groupSamples.Name = "groupSamples";
            this.groupSamples.Size = new System.Drawing.Size(476, 379);
            this.groupSamples.TabIndex = 8;
            this.groupSamples.TabStop = false;
            this.groupSamples.Text = "Samples";
            // 
            // buttonRunSample
            // 
            this.buttonRunSample.Location = new System.Drawing.Point(297, 59);
            this.buttonRunSample.Name = "buttonRunSample";
            this.buttonRunSample.Size = new System.Drawing.Size(173, 30);
            this.buttonRunSample.TabIndex = 5;
            this.buttonRunSample.Text = "Run sample";
            this.buttonRunSample.UseVisualStyleBackColor = true;
            this.buttonRunSample.Click += new System.EventHandler(this.buttonRunSample_Click);
            // 
            // buttonShowSample
            // 
            this.buttonShowSample.Location = new System.Drawing.Point(297, 22);
            this.buttonShowSample.Name = "buttonShowSample";
            this.buttonShowSample.Size = new System.Drawing.Size(173, 31);
            this.buttonShowSample.TabIndex = 4;
            this.buttonShowSample.Text = "Show sample in KntConsole";
            this.buttonShowSample.UseVisualStyleBackColor = true;
            this.buttonShowSample.Click += new System.EventHandler(this.buttonShowSample_Click);
            // 
            // listSamples
            // 
            this.listSamples.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listSamples.FormattingEnabled = true;
            this.listSamples.ItemHeight = 15;
            this.listSamples.Location = new System.Drawing.Point(13, 19);
            this.listSamples.Name = "listSamples";
            this.listSamples.Size = new System.Drawing.Size(269, 349);
            this.listSamples.TabIndex = 2;
            this.listSamples.SelectedIndexChanged += new System.EventHandler(this.listSamples_SelectedIndexChanged);
            // 
            // buttonRunBackground
            // 
            this.buttonRunBackground.Location = new System.Drawing.Point(16, 82);
            this.buttonRunBackground.Name = "buttonRunBackground";
            this.buttonRunBackground.Size = new System.Drawing.Size(282, 31);
            this.buttonRunBackground.TabIndex = 6;
            this.buttonRunBackground.Text = "Run simple script in bakground";
            this.buttonRunBackground.UseVisualStyleBackColor = true;
            this.buttonRunBackground.Click += new System.EventHandler(this.buttonRunBackground_Click);
            // 
            // buttonInteract
            // 
            this.buttonInteract.Location = new System.Drawing.Point(16, 48);
            this.buttonInteract.Name = "buttonInteract";
            this.buttonInteract.Size = new System.Drawing.Size(282, 28);
            this.buttonInteract.TabIndex = 5;
            this.buttonInteract.Text = "Interacting with kntscript (embedded code)";
            this.buttonInteract.UseVisualStyleBackColor = true;
            this.buttonInteract.Click += new System.EventHandler(this.buttonInteract_Click);
            // 
            // buttonShowConsole
            // 
            this.buttonShowConsole.Location = new System.Drawing.Point(16, 119);
            this.buttonShowConsole.Name = "buttonShowConsole";
            this.buttonShowConsole.Size = new System.Drawing.Size(282, 29);
            this.buttonShowConsole.TabIndex = 7;
            this.buttonShowConsole.Text = "Show KntConsole";
            this.buttonShowConsole.UseVisualStyleBackColor = true;
            this.buttonShowConsole.Click += new System.EventHandler(this.buttonShowConsole_Click);
            // 
            // buttonRunScript
            // 
            this.buttonRunScript.Location = new System.Drawing.Point(16, 14);
            this.buttonRunScript.Name = "buttonRunScript";
            this.buttonRunScript.Size = new System.Drawing.Size(282, 28);
            this.buttonRunScript.TabIndex = 4;
            this.buttonRunScript.Text = "Run simple script (embedded code)";
            this.buttonRunScript.UseVisualStyleBackColor = true;
            this.buttonRunScript.Click += new System.EventHandler(this.buttonRunScript_Click);
            // 
            // tabRichEditor
            // 
            this.tabRichEditor.Controls.Add(this.htmlDescription);
            this.tabRichEditor.Location = new System.Drawing.Point(4, 24);
            this.tabRichEditor.Name = "tabRichEditor";
            this.tabRichEditor.Size = new System.Drawing.Size(662, 549);
            this.tabRichEditor.TabIndex = 2;
            this.tabRichEditor.Text = "Test rich editor";
            this.tabRichEditor.UseVisualStyleBackColor = true;
            // 
            // htmlDescription
            // 
            this.htmlDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.htmlDescription.InnerText = null;
            this.htmlDescription.Location = new System.Drawing.Point(14, 15);
            this.htmlDescription.Name = "htmlDescription";
            this.htmlDescription.Size = new System.Drawing.Size(634, 287);
            this.htmlDescription.TabIndex = 10;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 194);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 15);
            this.label1.TabIndex = 7;
            this.label1.Text = "label1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 209);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 15);
            this.label2.TabIndex = 8;
            this.label2.Text = "label2";
            // 
            // LabForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(689, 601);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "LabForm";
            this.Text = "KntScript AppHost demo";
            this.Load += new System.EventHandler(this.DemoForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabAppLab.ResumeLayout(false);
            this.tabAppLab.PerformLayout();
            this.tabKntScriptLab.ResumeLayout(false);
            this.groupSamples.ResumeLayout(false);
            this.tabRichEditor.ResumeLayout(false);
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
        private System.Windows.Forms.Button buttonTest1;
        private System.Windows.Forms.ListBox listMessages;
        private System.Windows.Forms.Button buttonTest4;
        private System.Windows.Forms.TabPage tabRichEditor;
        private Pavonis.Html.Editor.HtmlEditorControl htmlDescription;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}