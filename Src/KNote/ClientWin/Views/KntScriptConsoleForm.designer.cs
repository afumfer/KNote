namespace KNote.ClientWin.Views
{
    partial class KntScriptConsoleForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KntScriptConsoleForm));
            toolStripConsole = new ToolStrip();
            buttonNew = new ToolStripButton();
            buttonOpen = new ToolStripButton();
            buttonSave = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            buttonRunKntScript = new ToolStripButton();
            buttonRunCSCode = new ToolStripButton();
            buttonRunCSCodeStdOut = new ToolStripButton();
            statusStripKntConsole = new StatusStrip();
            statusAction = new ToolStripStatusLabel();
            statusFileName = new ToolStripStatusLabel();
            panel1 = new Panel();
            splitContainer1 = new SplitContainer();
            textSourceCode = new TextBox();
            openFileDialogScript = new OpenFileDialog();
            saveFileDialogScript = new SaveFileDialog();
            toolStripSeparator2 = new ToolStripSeparator();
            toolStripConsole.SuspendLayout();
            statusStripKntConsole.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // toolStripConsole
            // 
            toolStripConsole.Items.AddRange(new ToolStripItem[] { buttonNew, buttonOpen, buttonSave, toolStripSeparator1, buttonRunKntScript, toolStripSeparator2, buttonRunCSCode, buttonRunCSCodeStdOut });
            toolStripConsole.Location = new Point(0, 0);
            toolStripConsole.Name = "toolStripConsole";
            toolStripConsole.Size = new Size(1111, 25);
            toolStripConsole.TabIndex = 9;
            toolStripConsole.Text = "KntSctipt console";
            // 
            // buttonNew
            // 
            buttonNew.DisplayStyle = ToolStripItemDisplayStyle.Image;
            buttonNew.Image = (Image)resources.GetObject("buttonNew.Image");
            buttonNew.ImageTransparentColor = Color.Magenta;
            buttonNew.Name = "buttonNew";
            buttonNew.Size = new Size(23, 22);
            buttonNew.Text = "New";
            buttonNew.Click += buttonNew_Click;
            // 
            // buttonOpen
            // 
            buttonOpen.DisplayStyle = ToolStripItemDisplayStyle.Image;
            buttonOpen.Image = (Image)resources.GetObject("buttonOpen.Image");
            buttonOpen.ImageTransparentColor = Color.Magenta;
            buttonOpen.Name = "buttonOpen";
            buttonOpen.Size = new Size(23, 22);
            buttonOpen.Text = "Open";
            buttonOpen.Click += buttonOpen_Click;
            // 
            // buttonSave
            // 
            buttonSave.DisplayStyle = ToolStripItemDisplayStyle.Image;
            buttonSave.Image = (Image)resources.GetObject("buttonSave.Image");
            buttonSave.ImageTransparentColor = Color.Magenta;
            buttonSave.Name = "buttonSave";
            buttonSave.Size = new Size(23, 22);
            buttonSave.Text = "Save";
            buttonSave.Click += buttonSave_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 25);
            // 
            // buttonRunKntScript
            // 
            buttonRunKntScript.Image = (Image)resources.GetObject("buttonRunKntScript.Image");
            buttonRunKntScript.ImageTransparentColor = Color.Magenta;
            buttonRunKntScript.Name = "buttonRunKntScript";
            buttonRunKntScript.Size = new Size(99, 22);
            buttonRunKntScript.Text = "Run KntScript";
            buttonRunKntScript.TextAlign = ContentAlignment.MiddleRight;
            buttonRunKntScript.Click += buttonRun_Click;
            // 
            // buttonRunCSCode
            // 
            buttonRunCSCode.Image = (Image)resources.GetObject("buttonRunCSCode.Image");
            buttonRunCSCode.ImageTransparentColor = Color.Magenta;
            buttonRunCSCode.Name = "buttonRunCSCode";
            buttonRunCSCode.Size = new Size(94, 22);
            buttonRunCSCode.Text = "Run CS code";
            buttonRunCSCode.Click += buttonRunCSCode_Click;
            // 
            // buttonRunCSCodeStdOut
            // 
            buttonRunCSCodeStdOut.Image = (Image)resources.GetObject("buttonRunCSCodeStdOut.Image");
            buttonRunCSCodeStdOut.ImageTransparentColor = Color.Magenta;
            buttonRunCSCodeStdOut.Name = "buttonRunCSCodeStdOut";
            buttonRunCSCodeStdOut.Size = new Size(188, 22);
            buttonRunCSCodeStdOut.Text = "Run CS code in stdout console";
            buttonRunCSCodeStdOut.Click += buttonRunCSCodeStdOut_Click;
            // 
            // statusStripKntConsole
            // 
            statusStripKntConsole.Items.AddRange(new ToolStripItem[] { statusAction, statusFileName });
            statusStripKntConsole.Location = new Point(0, 647);
            statusStripKntConsole.Name = "statusStripKntConsole";
            statusStripKntConsole.Padding = new Padding(1, 0, 16, 0);
            statusStripKntConsole.Size = new Size(1111, 22);
            statusStripKntConsole.TabIndex = 10;
            statusStripKntConsole.Text = "statusStrip1";
            // 
            // statusAction
            // 
            statusAction.Name = "statusAction";
            statusAction.Size = new Size(0, 17);
            // 
            // statusFileName
            // 
            statusFileName.AutoSize = false;
            statusFileName.Name = "statusFileName";
            statusFileName.Size = new Size(1094, 17);
            statusFileName.Spring = true;
            statusFileName.Text = "KntScript";
            statusFileName.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // panel1
            // 
            panel1.Controls.Add(splitContainer1);
            panel1.Dock = DockStyle.Fill;
            panel1.Location = new Point(0, 25);
            panel1.Margin = new Padding(4, 3, 4, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(1111, 622);
            panel1.TabIndex = 11;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Margin = new Padding(4, 3, 4, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(textSourceCode);
            splitContainer1.Size = new Size(1111, 622);
            splitContainer1.SplitterDistance = 569;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 1;
            // 
            // textSourceCode
            // 
            textSourceCode.AcceptsTab = true;
            textSourceCode.Dock = DockStyle.Fill;
            textSourceCode.Font = new Font("Courier New", 11.25F);
            textSourceCode.Location = new Point(0, 0);
            textSourceCode.Margin = new Padding(6);
            textSourceCode.Multiline = true;
            textSourceCode.Name = "textSourceCode";
            textSourceCode.ScrollBars = ScrollBars.Both;
            textSourceCode.Size = new Size(569, 622);
            textSourceCode.TabIndex = 3;
            textSourceCode.WordWrap = false;
            // 
            // openFileDialogScript
            // 
            openFileDialogScript.FileName = "KntScript";
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 25);
            // 
            // KntScriptConsoleForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1111, 669);
            Controls.Add(panel1);
            Controls.Add(statusStripKntConsole);
            Controls.Add(toolStripConsole);
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Margin = new Padding(4, 3, 4, 3);
            Name = "KntScriptConsoleForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "KntScript - Console";
            FormClosing += KntScriptConsoleForm_FormClosing;
            Load += KntScriptForm_Load;
            KeyUp += KntScriptForm_KeyUp;
            toolStripConsole.ResumeLayout(false);
            toolStripConsole.PerformLayout();
            statusStripKntConsole.ResumeLayout(false);
            statusStripKntConsole.PerformLayout();
            panel1.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStripConsole;
        private System.Windows.Forms.StatusStrip statusStripKntConsole;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox textSourceCode;
        private System.Windows.Forms.ToolStripButton buttonNew;
        private System.Windows.Forms.ToolStripButton buttonOpen;
        private System.Windows.Forms.ToolStripButton buttonSave;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton buttonRunKntScript;
        private System.Windows.Forms.ToolStripStatusLabel statusFileName;
        private System.Windows.Forms.OpenFileDialog openFileDialogScript;
        private System.Windows.Forms.SaveFileDialog saveFileDialogScript;
        private ToolStripButton buttonRunCSCode;
        private ToolStripButton buttonRunCSCodeStdOut;
        private ToolStripStatusLabel statusAction;
        private ToolStripSeparator toolStripSeparator2;
    }
}
