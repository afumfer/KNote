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
            this.toolStripConsole = new System.Windows.Forms.ToolStrip();
            this.buttonNew = new System.Windows.Forms.ToolStripButton();
            this.buttonOpen = new System.Windows.Forms.ToolStripButton();
            this.buttonSave = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.buttonRun = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statusFileName = new System.Windows.Forms.ToolStripStatusLabel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.textSourceCode = new System.Windows.Forms.TextBox();
            this.openFileDialogScript = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialogScript = new System.Windows.Forms.SaveFileDialog();
            this.toolStripConsole.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripConsole
            // 
            this.toolStripConsole.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buttonNew,
            this.buttonOpen,
            this.buttonSave,
            this.toolStripSeparator1,
            this.buttonRun});
            this.toolStripConsole.Location = new System.Drawing.Point(0, 0);
            this.toolStripConsole.Name = "toolStripConsole";
            this.toolStripConsole.Size = new System.Drawing.Size(1111, 25);
            this.toolStripConsole.TabIndex = 9;
            this.toolStripConsole.Text = "KntSctipt console";
            // 
            // buttonNew
            // 
            this.buttonNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonNew.Image = ((System.Drawing.Image)(resources.GetObject("buttonNew.Image")));
            this.buttonNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonNew.Name = "buttonNew";
            this.buttonNew.Size = new System.Drawing.Size(23, 22);
            this.buttonNew.Text = "New";
            this.buttonNew.Click += new System.EventHandler(this.buttonNew_Click);
            // 
            // buttonOpen
            // 
            this.buttonOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonOpen.Image = ((System.Drawing.Image)(resources.GetObject("buttonOpen.Image")));
            this.buttonOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonOpen.Name = "buttonOpen";
            this.buttonOpen.Size = new System.Drawing.Size(23, 22);
            this.buttonOpen.Text = "Open";
            this.buttonOpen.Click += new System.EventHandler(this.buttonOpen_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonSave.Image = ((System.Drawing.Image)(resources.GetObject("buttonSave.Image")));
            this.buttonSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(23, 22);
            this.buttonSave.Text = "Save";
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // buttonRun
            // 
            this.buttonRun.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buttonRun.Image = ((System.Drawing.Image)(resources.GetObject("buttonRun.Image")));
            this.buttonRun.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(23, 22);
            this.buttonRun.Text = "Run";
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusFileName});
            this.statusStrip1.Location = new System.Drawing.Point(0, 647);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1111, 22);
            this.statusStrip1.TabIndex = 10;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statusFileName
            // 
            this.statusFileName.Name = "statusFileName";
            this.statusFileName.Size = new System.Drawing.Size(1094, 17);
            this.statusFileName.Spring = true;
            this.statusFileName.Text = "KntScript";
            this.statusFileName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.splitContainer1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 25);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1111, 622);
            this.panel1.TabIndex = 11;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.textSourceCode);
            this.splitContainer1.Size = new System.Drawing.Size(1111, 622);
            this.splitContainer1.SplitterDistance = 569;
            this.splitContainer1.SplitterWidth = 5;
            this.splitContainer1.TabIndex = 1;
            // 
            // textSourceCode
            // 
            this.textSourceCode.AcceptsTab = true;
            this.textSourceCode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textSourceCode.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.textSourceCode.Location = new System.Drawing.Point(0, 0);
            this.textSourceCode.Margin = new System.Windows.Forms.Padding(6);
            this.textSourceCode.Multiline = true;
            this.textSourceCode.Name = "textSourceCode";
            this.textSourceCode.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textSourceCode.Size = new System.Drawing.Size(569, 622);
            this.textSourceCode.TabIndex = 3;
            this.textSourceCode.WordWrap = false;
            // 
            // openFileDialogScript
            // 
            this.openFileDialogScript.FileName = "KntScript";
            // 
            // KntScriptConsoleForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1111, 669);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStripConsole);
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "KntScriptConsoleForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "KntScript - Console";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.KntScriptConsoleForm_FormClosing);
            this.Load += new System.EventHandler(this.KntScriptForm_Load);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.KntScriptForm_KeyUp);
            this.toolStripConsole.ResumeLayout(false);
            this.toolStripConsole.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStripConsole;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox textSourceCode;
        private System.Windows.Forms.ToolStripButton buttonNew;
        private System.Windows.Forms.ToolStripButton buttonOpen;
        private System.Windows.Forms.ToolStripButton buttonSave;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton buttonRun;
        private System.Windows.Forms.ToolStripStatusLabel statusFileName;
        private System.Windows.Forms.OpenFileDialog openFileDialogScript;
        private System.Windows.Forms.SaveFileDialog saveFileDialogScript;
    }
}
