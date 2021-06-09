
namespace KNote.ClientWin.Views
{
    partial class PostItEditorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PostItEditorForm));
            this.panelForm = new System.Windows.Forms.Panel();
            this.htmlDescription = new Pavonis.Html.Editor.HtmlEditorControl();
            this.labelStatus = new System.Windows.Forms.LabelNoCopy();
            this.picResize = new System.Windows.Forms.PictureBox();
            this.picMenu = new System.Windows.Forms.PictureBox();
            this.textDescription = new System.Windows.Forms.TextBox();
            this.labelCaption = new System.Windows.Forms.LabelNoCopy();
            this.menuPostIt = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuHide = new System.Windows.Forms.ToolStripMenuItem();
            this.menuAlwaysFront = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSaveNow = new System.Windows.Forms.ToolStripMenuItem();
            this.menuDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.menuS1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuExtendedEdition = new System.Windows.Forms.ToolStripMenuItem();
            this.menuPostItProperties = new System.Windows.Forms.ToolStripMenuItem();
            this.menuS2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuAlarmWithin = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFastAlarm10m = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFastAlarm30m = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFastAlarm1h = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFastAlarm2h = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFastAlarm4h = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFastAlarm8h = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFastAlarm10h = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFastAlarm12h = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFastAlarm24h = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFastAlarm1week = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFastAlarm1month = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFastAlarm1year = new System.Windows.Forms.ToolStripMenuItem();
            this.menuMoreActions = new System.Windows.Forms.ToolStripMenuItem();
            this.menuAddResolvedTask = new System.Windows.Forms.ToolStripMenuItem();
            this.panelForm.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picResize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMenu)).BeginInit();
            this.menuPostIt.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelForm
            // 
            this.panelForm.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelForm.Controls.Add(this.htmlDescription);
            this.panelForm.Controls.Add(this.labelStatus);
            this.panelForm.Controls.Add(this.picResize);
            this.panelForm.Controls.Add(this.picMenu);
            this.panelForm.Controls.Add(this.textDescription);
            this.panelForm.Controls.Add(this.labelCaption);
            this.panelForm.Location = new System.Drawing.Point(1, 1);
            this.panelForm.Name = "panelForm";
            this.panelForm.Size = new System.Drawing.Size(479, 340);
            this.panelForm.TabIndex = 3;
            // 
            // htmlDescription
            // 
            this.htmlDescription.InnerText = null;
            this.htmlDescription.Location = new System.Drawing.Point(25, 129);
            this.htmlDescription.Name = "htmlDescription";
            this.htmlDescription.Size = new System.Drawing.Size(277, 125);
            this.htmlDescription.TabIndex = 9;
            this.htmlDescription.ToolbarVisible = false;
            this.htmlDescription.Visible = false;
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelStatus.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
            this.labelStatus.Location = new System.Drawing.Point(3, 322);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(456, 14);
            this.labelStatus.TabIndex = 7;
            this.labelStatus.Text = "...";
            this.labelStatus.DoubleClick += new System.EventHandler(this.labelStatus_DoubleClick);
            // 
            // picResize
            // 
            this.picResize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.picResize.Cursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.picResize.Image = ((System.Drawing.Image)(resources.GetObject("picResize.Image")));
            this.picResize.Location = new System.Drawing.Point(463, 324);
            this.picResize.Name = "picResize";
            this.picResize.Size = new System.Drawing.Size(16, 16);
            this.picResize.TabIndex = 5;
            this.picResize.TabStop = false;
            this.picResize.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picResize_MouseDown);
            this.picResize.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picResize_MouseMove);
            // 
            // picMenu
            // 
            this.picMenu.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picMenu.Image = ((System.Drawing.Image)(resources.GetObject("picMenu.Image")));
            this.picMenu.Location = new System.Drawing.Point(5, 5);
            this.picMenu.Name = "picMenu";
            this.picMenu.Size = new System.Drawing.Size(16, 16);
            this.picMenu.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picMenu.TabIndex = 4;
            this.picMenu.TabStop = false;
            this.picMenu.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picMenu_MouseUp);
            // 
            // textDescription
            // 
            this.textDescription.BackColor = System.Drawing.Color.Beige;
            this.textDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textDescription.Location = new System.Drawing.Point(25, 50);
            this.textDescription.Margin = new System.Windows.Forms.Padding(6);
            this.textDescription.MaxLength = 0;
            this.textDescription.Multiline = true;
            this.textDescription.Name = "textDescription";
            this.textDescription.Size = new System.Drawing.Size(163, 70);
            this.textDescription.TabIndex = 3;
            // 
            // labelCaption
            // 
            this.labelCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCaption.BackColor = System.Drawing.Color.PaleGoldenrod;
            this.labelCaption.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.labelCaption.Location = new System.Drawing.Point(3, 3);
            this.labelCaption.Name = "labelCaption";
            this.labelCaption.Padding = new System.Windows.Forms.Padding(20, 2, 0, 0);
            this.labelCaption.Size = new System.Drawing.Size(472, 20);
            this.labelCaption.TabIndex = 6;
            this.labelCaption.Text = "...";
            this.labelCaption.DoubleClick += new System.EventHandler(this.labelCaption_DoubleClick);
            this.labelCaption.MouseDown += new System.Windows.Forms.MouseEventHandler(this.labelCaption_MouseDown);
            this.labelCaption.MouseMove += new System.Windows.Forms.MouseEventHandler(this.labelCaption_MouseMove);
            // 
            // menuPostIt
            // 
            this.menuPostIt.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuHide,
            this.menuAlwaysFront,
            this.menuSaveNow,
            this.menuDelete,
            this.menuS1,
            this.menuExtendedEdition,
            this.menuPostItProperties,
            this.menuS2,
            this.menuAlarmWithin,
            this.menuMoreActions});
            this.menuPostIt.Name = "menuPostIt";
            this.menuPostIt.Size = new System.Drawing.Size(216, 192);
            // 
            // menuHide
            // 
            this.menuHide.Name = "menuHide";
            this.menuHide.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.menuHide.Size = new System.Drawing.Size(215, 22);
            this.menuHide.Text = "Hide note";
            this.menuHide.Click += new System.EventHandler(this.postItMenu_Click);
            // 
            // menuAlwaysFront
            // 
            this.menuAlwaysFront.Name = "menuAlwaysFront";
            this.menuAlwaysFront.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.menuAlwaysFront.Size = new System.Drawing.Size(215, 22);
            this.menuAlwaysFront.Text = "Always front";
            this.menuAlwaysFront.Click += new System.EventHandler(this.postItMenu_Click);
            // 
            // menuSaveNow
            // 
            this.menuSaveNow.Name = "menuSaveNow";
            this.menuSaveNow.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.menuSaveNow.Size = new System.Drawing.Size(215, 22);
            this.menuSaveNow.Text = "Save now";
            this.menuSaveNow.Click += new System.EventHandler(this.postItMenu_Click);
            // 
            // menuDelete
            // 
            this.menuDelete.Name = "menuDelete";
            this.menuDelete.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D)));
            this.menuDelete.Size = new System.Drawing.Size(215, 22);
            this.menuDelete.Text = "Delete";
            this.menuDelete.Click += new System.EventHandler(this.postItMenu_Click);
            // 
            // menuS1
            // 
            this.menuS1.Name = "menuS1";
            this.menuS1.Size = new System.Drawing.Size(212, 6);
            // 
            // menuExtendedEdition
            // 
            this.menuExtendedEdition.Name = "menuExtendedEdition";
            this.menuExtendedEdition.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.menuExtendedEdition.Size = new System.Drawing.Size(215, 22);
            this.menuExtendedEdition.Text = "Extended edition ...";
            this.menuExtendedEdition.Click += new System.EventHandler(this.postItMenu_Click);
            // 
            // menuPostItProperties
            // 
            this.menuPostItProperties.Name = "menuPostItProperties";
            this.menuPostItProperties.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
            this.menuPostItProperties.Size = new System.Drawing.Size(215, 22);
            this.menuPostItProperties.Text = "PostIt properties ...";
            this.menuPostItProperties.Click += new System.EventHandler(this.postItMenu_Click);
            // 
            // menuS2
            // 
            this.menuS2.Name = "menuS2";
            this.menuS2.Size = new System.Drawing.Size(212, 6);
            // 
            // menuAlarmWithin
            // 
            this.menuAlarmWithin.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFastAlarm10m,
            this.menuFastAlarm30m,
            this.menuFastAlarm1h,
            this.menuFastAlarm2h,
            this.menuFastAlarm4h,
            this.menuFastAlarm8h,
            this.menuFastAlarm10h,
            this.menuFastAlarm12h,
            this.menuFastAlarm24h,
            this.menuFastAlarm1week,
            this.menuFastAlarm1month,
            this.menuFastAlarm1year});
            this.menuAlarmWithin.Name = "menuAlarmWithin";
            this.menuAlarmWithin.Size = new System.Drawing.Size(215, 22);
            this.menuAlarmWithin.Text = "Activate note &alarm within";
            // 
            // menuFastAlarm10m
            // 
            this.menuFastAlarm10m.Name = "menuFastAlarm10m";
            this.menuFastAlarm10m.Size = new System.Drawing.Size(132, 22);
            this.menuFastAlarm10m.Text = "10 minutes";
            this.menuFastAlarm10m.Click += new System.EventHandler(this.postItMenu_Click);
            // 
            // menuFastAlarm30m
            // 
            this.menuFastAlarm30m.Name = "menuFastAlarm30m";
            this.menuFastAlarm30m.Size = new System.Drawing.Size(132, 22);
            this.menuFastAlarm30m.Text = "30 minutes";
            this.menuFastAlarm30m.Click += new System.EventHandler(this.postItMenu_Click);
            // 
            // menuFastAlarm1h
            // 
            this.menuFastAlarm1h.Name = "menuFastAlarm1h";
            this.menuFastAlarm1h.Size = new System.Drawing.Size(132, 22);
            this.menuFastAlarm1h.Text = "1 hour";
            this.menuFastAlarm1h.Click += new System.EventHandler(this.postItMenu_Click);
            // 
            // menuFastAlarm2h
            // 
            this.menuFastAlarm2h.Name = "menuFastAlarm2h";
            this.menuFastAlarm2h.Size = new System.Drawing.Size(132, 22);
            this.menuFastAlarm2h.Text = "2 hours";
            this.menuFastAlarm2h.Click += new System.EventHandler(this.postItMenu_Click);
            // 
            // menuFastAlarm4h
            // 
            this.menuFastAlarm4h.Name = "menuFastAlarm4h";
            this.menuFastAlarm4h.Size = new System.Drawing.Size(132, 22);
            this.menuFastAlarm4h.Text = "4 hours";
            this.menuFastAlarm4h.Click += new System.EventHandler(this.postItMenu_Click);
            // 
            // menuFastAlarm8h
            // 
            this.menuFastAlarm8h.Name = "menuFastAlarm8h";
            this.menuFastAlarm8h.Size = new System.Drawing.Size(132, 22);
            this.menuFastAlarm8h.Text = "8 hours";
            this.menuFastAlarm8h.Click += new System.EventHandler(this.postItMenu_Click);
            // 
            // menuFastAlarm10h
            // 
            this.menuFastAlarm10h.Name = "menuFastAlarm10h";
            this.menuFastAlarm10h.Size = new System.Drawing.Size(132, 22);
            this.menuFastAlarm10h.Text = "10 hours";
            this.menuFastAlarm10h.Click += new System.EventHandler(this.postItMenu_Click);
            // 
            // menuFastAlarm12h
            // 
            this.menuFastAlarm12h.Name = "menuFastAlarm12h";
            this.menuFastAlarm12h.Size = new System.Drawing.Size(132, 22);
            this.menuFastAlarm12h.Text = "12 hours";
            this.menuFastAlarm12h.Click += new System.EventHandler(this.postItMenu_Click);
            // 
            // menuFastAlarm24h
            // 
            this.menuFastAlarm24h.Name = "menuFastAlarm24h";
            this.menuFastAlarm24h.Size = new System.Drawing.Size(132, 22);
            this.menuFastAlarm24h.Text = "24 hours";
            this.menuFastAlarm24h.Click += new System.EventHandler(this.postItMenu_Click);
            // 
            // menuFastAlarm1week
            // 
            this.menuFastAlarm1week.Name = "menuFastAlarm1week";
            this.menuFastAlarm1week.Size = new System.Drawing.Size(132, 22);
            this.menuFastAlarm1week.Text = "1 week";
            this.menuFastAlarm1week.Click += new System.EventHandler(this.postItMenu_Click);
            // 
            // menuFastAlarm1month
            // 
            this.menuFastAlarm1month.Name = "menuFastAlarm1month";
            this.menuFastAlarm1month.Size = new System.Drawing.Size(132, 22);
            this.menuFastAlarm1month.Text = "1 month";
            this.menuFastAlarm1month.Click += new System.EventHandler(this.postItMenu_Click);
            // 
            // menuFastAlarm1year
            // 
            this.menuFastAlarm1year.Name = "menuFastAlarm1year";
            this.menuFastAlarm1year.Size = new System.Drawing.Size(132, 22);
            this.menuFastAlarm1year.Text = "1 year";
            this.menuFastAlarm1year.Click += new System.EventHandler(this.postItMenu_Click);
            // 
            // menuMoreActions
            // 
            this.menuMoreActions.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuAddResolvedTask});
            this.menuMoreActions.Name = "menuMoreActions";
            this.menuMoreActions.Size = new System.Drawing.Size(215, 22);
            this.menuMoreActions.Text = "More actions";
            // 
            // menuAddResolvedTask
            // 
            this.menuAddResolvedTask.Name = "menuAddResolvedTask";
            this.menuAddResolvedTask.Size = new System.Drawing.Size(189, 22);
            this.menuAddResolvedTask.Text = "Add fast resolved task";
            this.menuAddResolvedTask.Click += new System.EventHandler(this.postItMenu_Click);
            // 
            // PostItEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Info;
            this.ClientSize = new System.Drawing.Size(481, 342);
            this.ControlBox = false;
            this.Controls.Add(this.panelForm);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PostItEditorForm";
            this.ShowInTaskbar = false;
            this.Text = "PostIt editor form";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PostItEditorForm_FormClosing);
            this.Load += new System.EventHandler(this.PostItEditorForm_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.PostItEditorForm_Paint);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.PostItEditorForm_KeyUp);
            this.panelForm.ResumeLayout(false);
            this.panelForm.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picResize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMenu)).EndInit();
            this.menuPostIt.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.TextBox textDescription;
        private System.Windows.Forms.PictureBox picMenu;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Label labelCaption;
        private System.Windows.Forms.PictureBox picResize;
        private System.Windows.Forms.ContextMenuStrip menuPostIt;
        private System.Windows.Forms.ToolStripMenuItem menuHide;
        private System.Windows.Forms.ToolStripMenuItem menuAlwaysFront;
        private System.Windows.Forms.ToolStripMenuItem menuSaveNow;
        private System.Windows.Forms.ToolStripMenuItem menuDelete;
        private System.Windows.Forms.ToolStripSeparator menuS1;
        private System.Windows.Forms.ToolStripMenuItem menuExtendedEdition;
        private System.Windows.Forms.ToolStripMenuItem menuPostItProperties;
        private Pavonis.Html.Editor.HtmlEditorControl htmlDescription;
        private System.Windows.Forms.ToolStripSeparator menuS2;
        private System.Windows.Forms.ToolStripMenuItem menuAlarmWithin;
        private System.Windows.Forms.ToolStripMenuItem menuFastAlarm10m;
        private System.Windows.Forms.ToolStripMenuItem menuFastAlarm30m;
        private System.Windows.Forms.ToolStripMenuItem menuFastAlarm1h;
        private System.Windows.Forms.ToolStripMenuItem menuFastAlarm2h;
        private System.Windows.Forms.ToolStripMenuItem menuFastAlarm4h;
        private System.Windows.Forms.ToolStripMenuItem menuFastAlarm8h;
        private System.Windows.Forms.ToolStripMenuItem menuFastAlarm10h;
        private System.Windows.Forms.ToolStripMenuItem menuFastAlarm12h;
        private System.Windows.Forms.ToolStripMenuItem menuFastAlarm24h;
        private System.Windows.Forms.ToolStripMenuItem menuFastAlarm1week;
        private System.Windows.Forms.ToolStripMenuItem menuFastAlarm1month;
        private System.Windows.Forms.ToolStripMenuItem menuFastAlarm1year;
        private System.Windows.Forms.ToolStripMenuItem menuMoreActions;
        private System.Windows.Forms.ToolStripMenuItem menuAddResolvedTask;
    }
}