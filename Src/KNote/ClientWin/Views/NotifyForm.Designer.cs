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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NotifyForm));
            notifyKNote = new NotifyIcon(components);
            contextKNoteMenu = new ContextMenuStrip(components);
            menuNewNote = new ToolStripMenuItem();
            menuShowKNoteManagment = new ToolStripMenuItem();
            menuPostItsVisibles = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripSeparator();
            menuKNoteOptions = new ToolStripMenuItem();
            menuHelp = new ToolStripMenuItem();
            menuAbout = new ToolStripMenuItem();
            toolStripMenuItem2 = new ToolStripSeparator();
            menuExit = new ToolStripMenuItem();
            contextKNoteMenu.SuspendLayout();
            SuspendLayout();
            // 
            // notifyKNote
            // 
            notifyKNote.BalloonTipText = "KeyNotex notify";
            notifyKNote.BalloonTipTitle = "KeyNotex";
            notifyKNote.ContextMenuStrip = contextKNoteMenu;
            notifyKNote.Icon = (Icon)resources.GetObject("notifyKNote.Icon");
            notifyKNote.Text = "KaNote";
            notifyKNote.Visible = true;
            notifyKNote.DoubleClick += notifyKNote_DoubleClick;
            // 
            // contextKNoteMenu
            // 
            contextKNoteMenu.Items.AddRange(new ToolStripItem[] { menuNewNote, menuShowKNoteManagment, menuPostItsVisibles, toolStripMenuItem1, menuKNoteOptions, menuHelp, menuAbout, toolStripMenuItem2, menuExit });
            contextKNoteMenu.Name = "contextMenuStrip1";
            contextKNoteMenu.Size = new Size(226, 170);
            // 
            // menuNewNote
            // 
            menuNewNote.Name = "menuNewNote";
            menuNewNote.Size = new Size(225, 22);
            menuNewNote.Text = "New note";
            menuNewNote.Click += menuNewNote_Click;
            // 
            // menuShowKNoteManagment
            // 
            menuShowKNoteManagment.Name = "menuShowKNoteManagment";
            menuShowKNoteManagment.Size = new Size(225, 22);
            menuShowKNoteManagment.Text = "Show KaNote managment ...";
            menuShowKNoteManagment.Click += menuShowKNoteManagment_Click;
            // 
            // menuPostItsVisibles
            // 
            menuPostItsVisibles.Checked = true;
            menuPostItsVisibles.CheckOnClick = true;
            menuPostItsVisibles.CheckState = CheckState.Checked;
            menuPostItsVisibles.Name = "menuPostItsVisibles";
            menuPostItsVisibles.Size = new Size(225, 22);
            menuPostItsVisibles.Text = "Post-Its visibles";
            menuPostItsVisibles.Click += menuPostItsVisibles_Click;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(222, 6);
            // 
            // menuKNoteOptions
            // 
            menuKNoteOptions.Name = "menuKNoteOptions";
            menuKNoteOptions.Size = new Size(225, 22);
            menuKNoteOptions.Text = "Options";
            menuKNoteOptions.Click += menuKNoteOptions_Click;
            // 
            // menuHelp
            // 
            menuHelp.Name = "menuHelp";
            menuHelp.Size = new Size(225, 22);
            menuHelp.Text = "Help";
            menuHelp.Click += menuHelp_Click;
            // 
            // menuAbout
            // 
            menuAbout.Name = "menuAbout";
            menuAbout.Size = new Size(225, 22);
            menuAbout.Text = "About KNote ...";
            menuAbout.Click += menuAbout_Click;
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new Size(222, 6);
            // 
            // menuExit
            // 
            menuExit.Name = "menuExit";
            menuExit.Size = new Size(225, 22);
            menuExit.Text = "Exit";
            menuExit.Click += menuExit_Click;
            // 
            // NotifyForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(33, 33);
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 3, 4, 3);
            Name = "NotifyForm";
            ShowInTaskbar = false;
            Text = "KaNote";
            WindowState = FormWindowState.Minimized;            
            contextKNoteMenu.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        internal System.Windows.Forms.NotifyIcon notifyKNote;
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