namespace KNote.ClientWin.Views
{
    partial class NotifyForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NotifyForm));
            this.notifyKNote = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextKNoteMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuNewNote = new System.Windows.Forms.ToolStripMenuItem();
            this.menuShowKNoteManagment = new System.Windows.Forms.ToolStripMenuItem();
            this.menuPostItsVisibles = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuKNoteOptions = new System.Windows.Forms.ToolStripMenuItem();
            this.menuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.menuAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.timerKNote = new System.Windows.Forms.Timer(this.components);
            this.contextKNoteMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyKNote
            // 
            this.notifyKNote.BalloonTipText = "KeyNotex notify";
            this.notifyKNote.BalloonTipTitle = "KeyNotex";
            this.notifyKNote.ContextMenuStrip = this.contextKNoteMenu;
            this.notifyKNote.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyKNote.Icon")));
            this.notifyKNote.Text = "KNote";
            this.notifyKNote.Visible = true;
            // 
            // contextKNoteMenu
            // 
            this.contextKNoteMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuNewNote,
            this.menuShowKNoteManagment,
            this.menuPostItsVisibles,
            this.toolStripMenuItem1,
            this.menuKNoteOptions,
            this.menuHelp,
            this.menuAbout,
            this.toolStripMenuItem2,
            this.menuExit});
            this.contextKNoteMenu.Name = "contextMenuStrip1";
            this.contextKNoteMenu.Size = new System.Drawing.Size(220, 170);
            // 
            // menuNewNote
            // 
            this.menuNewNote.Name = "menuNewNote";
            this.menuNewNote.Size = new System.Drawing.Size(219, 22);
            this.menuNewNote.Text = "New note";
            this.menuNewNote.Click += new System.EventHandler(this.menuNewNote_Click);
            // 
            // menuShowKNoteManagment
            // 
            this.menuShowKNoteManagment.Name = "menuShowKNoteManagment";
            this.menuShowKNoteManagment.Size = new System.Drawing.Size(219, 22);
            this.menuShowKNoteManagment.Text = "Show KNote managment ...";
            this.menuShowKNoteManagment.Click += new System.EventHandler(this.menuShowKNoteManagment_Click);
            // 
            // menuPostItsVisibles
            // 
            this.menuPostItsVisibles.Checked = true;
            this.menuPostItsVisibles.CheckOnClick = true;
            this.menuPostItsVisibles.CheckState = System.Windows.Forms.CheckState.Checked;
            this.menuPostItsVisibles.Name = "menuPostItsVisibles";
            this.menuPostItsVisibles.Size = new System.Drawing.Size(219, 22);
            this.menuPostItsVisibles.Text = "Post-Its visibles";
            this.menuPostItsVisibles.Click += new System.EventHandler(this.menuPostItsVisibles_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(216, 6);
            // 
            // menuKNoteOptions
            // 
            this.menuKNoteOptions.Name = "menuKNoteOptions";
            this.menuKNoteOptions.Size = new System.Drawing.Size(219, 22);
            this.menuKNoteOptions.Text = "Options";
            this.menuKNoteOptions.Click += new System.EventHandler(this.menuKNoteOptions_Click);
            // 
            // menuHelp
            // 
            this.menuHelp.Name = "menuHelp";
            this.menuHelp.Size = new System.Drawing.Size(219, 22);
            this.menuHelp.Text = "Help";
            this.menuHelp.Click += new System.EventHandler(this.menuHelp_Click);
            // 
            // menuAbout
            // 
            this.menuAbout.Name = "menuAbout";
            this.menuAbout.Size = new System.Drawing.Size(219, 22);
            this.menuAbout.Text = "About KNote ...";
            this.menuAbout.Click += new System.EventHandler(this.menuAbout_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(216, 6);
            // 
            // menuExit
            // 
            this.menuExit.Name = "menuExit";
            this.menuExit.Size = new System.Drawing.Size(219, 22);
            this.menuExit.Text = "Exit";
            this.menuExit.Click += new System.EventHandler(this.menuExit_Click);
            // 
            // timerKNote
            // 
            this.timerKNote.Interval = 60000;
            this.timerKNote.Tick += new System.EventHandler(this.timerKNote_Tick);
            // 
            // NotifyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(33, 33);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "NotifyForm";
            this.ShowInTaskbar = false;
            this.Text = "KNote";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.contextKNoteMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.NotifyIcon notifyKNote;
        internal System.Windows.Forms.Timer timerKNote;
        private System.Windows.Forms.ContextMenuStrip contextKNoteMenu;
        private System.Windows.Forms.ToolStripMenuItem menuNewNote;
        private System.Windows.Forms.ToolStripMenuItem menuShowKNoteManagment;
        private System.Windows.Forms.ToolStripMenuItem menuPostItsVisibles;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem menuKNoteOptions;
        private System.Windows.Forms.ToolStripMenuItem menuHelp;
        private System.Windows.Forms.ToolStripMenuItem menuAbout;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem menuExit;
    }
}