
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PostItEditorForm));
            panelForm = new Panel();
            panelContent = new Panel();
            kntEditView = new KntWebView.KntEditView();
            panelFooter = new Panel();
            labelStatus = new LabelNoCopy();
            progressStatus = new ProgressBar();
            picResize = new PictureBox();
            panelCaptionHeader = new Panel();
            labelCaption = new LabelNoCopy();
            picMenu = new PictureBox();
            menuPostIt = new ContextMenuStrip(components);
            menuHide = new ToolStripMenuItem();
            menuAlwaysFront = new ToolStripMenuItem();
            menuSaveNow = new ToolStripMenuItem();
            menuDelete = new ToolStripMenuItem();
            menuS1 = new ToolStripSeparator();
            menuExtendedEdition = new ToolStripMenuItem();
            menuPostItProperties = new ToolStripMenuItem();
            menuWindowsFormView = new ToolStripMenuItem();
            menuS2 = new ToolStripSeparator();
            menuAlarmWithin = new ToolStripMenuItem();
            menuFastAlarm10m = new ToolStripMenuItem();
            menuFastAlarm30m = new ToolStripMenuItem();
            menuFastAlarm1h = new ToolStripMenuItem();
            menuFastAlarm2h = new ToolStripMenuItem();
            menuFastAlarm4h = new ToolStripMenuItem();
            menuFastAlarm8h = new ToolStripMenuItem();
            menuFastAlarm10h = new ToolStripMenuItem();
            menuFastAlarm12h = new ToolStripMenuItem();
            menuFastAlarm24h = new ToolStripMenuItem();
            menuFastAlarm1week = new ToolStripMenuItem();
            menuFastAlarm1month = new ToolStripMenuItem();
            menuFastAlarm1year = new ToolStripMenuItem();
            menuMoreActions = new ToolStripMenuItem();
            menuAddResolvedTask = new ToolStripMenuItem();
            panelForm.SuspendLayout();
            panelContent.SuspendLayout();
            panelFooter.SuspendLayout();
            panelCaptionHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picResize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picMenu).BeginInit();
            menuPostIt.SuspendLayout();
            SuspendLayout();
            // 
            // panelForm
            // 
            panelForm.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelForm.Controls.Add(panelContent);
            panelForm.Controls.Add(panelFooter);
            panelForm.Controls.Add(panelCaptionHeader);
            panelForm.Controls.Add(picMenu);
            panelForm.Location = new Point(1, 2);
            panelForm.Margin = new Padding(4, 5, 4, 5);
            panelForm.Name = "panelForm";
            panelForm.Size = new Size(684, 567);
            panelForm.TabIndex = 3;
            //
            // panelContent
            //
            panelContent.Controls.Add(kntEditView);
            panelContent.Dock = DockStyle.Fill;
            panelContent.Location = new Point(0, 33);
            panelContent.Name = "panelContent";
            panelContent.Padding = new Padding(4);
            panelContent.Size = new Size(684, 501);
            panelContent.TabIndex = 8;
            //
            // kntEditView
            //
            kntEditView.BorderStyle = BorderStyle.FixedSingle;
            kntEditView.Dock = DockStyle.Fill;
            kntEditView.Location = new Point(4, 4);
            kntEditView.Name = "kntEditView";
            kntEditView.Size = new Size(676, 493);
            kntEditView.TabIndex = 0;
            //
            // panelFooter
            // 
            panelFooter.Controls.Add(labelStatus);
            panelFooter.Controls.Add(progressStatus);
            panelFooter.Controls.Add(picResize);
            panelFooter.Dock = DockStyle.Bottom;
            panelFooter.Location = new Point(0, 534);
            panelFooter.Margin = new Padding(4, 5, 4, 5);
            panelFooter.Name = "panelFooter";
            panelFooter.Size = new Size(684, 33);
            panelFooter.TabIndex = 6;
            // 
            // labelStatus
            // 
            labelStatus.Dock = DockStyle.Fill;
            labelStatus.ForeColor = SystemColors.ControlDarkDark;
            labelStatus.Location = new Point(0, 0);
            labelStatus.Margin = new Padding(4, 0, 4, 0);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(684, 33);
            labelStatus.TabIndex = 2;
            labelStatus.TextAlign = ContentAlignment.MiddleLeft;
            labelStatus.DoubleClick += labelStatus_DoubleClick;
            // 
            // progressStatus
            // 
            progressStatus.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            progressStatus.ForeColor = SystemColors.Window;
            progressStatus.Location = new Point(593, 10);
            progressStatus.Margin = new Padding(4, 5, 4, 5);
            progressStatus.Name = "progressStatus";
            progressStatus.Size = new Size(60, 17);
            progressStatus.Style = ProgressBarStyle.Marquee;
            progressStatus.TabIndex = 7;
            progressStatus.Visible = false;
            progressStatus.Click += progressStatus_Click;
            // 
            // picResize
            // 
            picResize.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            picResize.Cursor = Cursors.SizeNWSE;
            picResize.Image = (Image)resources.GetObject("picResize.Image");
            picResize.Location = new Point(665, 10);
            picResize.Margin = new Padding(4, 5, 4, 5);
            picResize.Name = "picResize";
            picResize.Size = new Size(17, 20);
            picResize.SizeMode = PictureBoxSizeMode.StretchImage;
            picResize.TabIndex = 5;
            picResize.TabStop = false;
            picResize.MouseDown += picResize_MouseDown;
            picResize.MouseMove += picResize_MouseMove;
            //
            // panelCaptionHeader
            //
            panelCaptionHeader.Controls.Add(labelCaption);
            panelCaptionHeader.Dock = DockStyle.Top;
            panelCaptionHeader.Location = new Point(0, 0);
            panelCaptionHeader.Name = "panelCaptionHeader";
            panelCaptionHeader.Padding = new Padding(2);
            panelCaptionHeader.Size = new Size(684, 37);
            panelCaptionHeader.TabIndex = 9;
            //
            // labelCaption
            //
            labelCaption.BackColor = Color.PaleGoldenrod;
            labelCaption.Dock = DockStyle.Fill;
            labelCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelCaption.Location = new Point(2, 2);
            labelCaption.Margin = new Padding(4, 0, 4, 0);
            labelCaption.Name = "labelCaption";
            labelCaption.Padding = new Padding(29, 3, 0, 0);
            labelCaption.Size = new Size(680, 33);
            labelCaption.TabIndex = 1;
            labelCaption.DoubleClick += labelCaption_DoubleClick;
            labelCaption.MouseDown += labelCaption_MouseDown;
            labelCaption.MouseMove += labelCaption_MouseMove;
            // 
            // picMenu
            // 
            picMenu.Cursor = Cursors.Hand;
            picMenu.Image = (Image)resources.GetObject("picMenu.Image");
            picMenu.Location = new Point(7, 8);
            picMenu.Margin = new Padding(4, 5, 4, 5);
            picMenu.Name = "picMenu";
            picMenu.Size = new Size(23, 27);
            picMenu.SizeMode = PictureBoxSizeMode.StretchImage;
            picMenu.TabIndex = 4;
            picMenu.TabStop = false;
            picMenu.MouseUp += picMenu_MouseUp;
            // 
            // menuPostIt
            // 
            menuPostIt.ImageScalingSize = new Size(24, 24);
            menuPostIt.Items.AddRange(new ToolStripItem[] { menuHide, menuAlwaysFront, menuSaveNow, menuDelete, menuS1, menuExtendedEdition, menuPostItProperties, menuWindowsFormView, menuS2, menuAlarmWithin, menuMoreActions });
            menuPostIt.Name = "menuPostIt";
            menuPostIt.Size = new Size(321, 304);
            // 
            // menuHide
            // 
            menuHide.Name = "menuHide";
            menuHide.ShortcutKeys = Keys.Control | Keys.Q;
            menuHide.Size = new Size(320, 32);
            menuHide.Text = "Hide note";
            menuHide.Click += postItMenu_Click;
            // 
            // menuAlwaysFront
            // 
            menuAlwaysFront.Name = "menuAlwaysFront";
            menuAlwaysFront.ShortcutKeys = Keys.Control | Keys.F;
            menuAlwaysFront.Size = new Size(320, 32);
            menuAlwaysFront.Text = "Always front";
            menuAlwaysFront.Click += postItMenu_Click;
            // 
            // menuSaveNow
            // 
            menuSaveNow.Name = "menuSaveNow";
            menuSaveNow.ShortcutKeys = Keys.Control | Keys.S;
            menuSaveNow.Size = new Size(320, 32);
            menuSaveNow.Text = "Save now";
            menuSaveNow.Click += postItMenu_Click;
            // 
            // menuDelete
            // 
            menuDelete.Name = "menuDelete";
            menuDelete.ShortcutKeys = Keys.Control | Keys.D;
            menuDelete.Size = new Size(320, 32);
            menuDelete.Text = "Delete";
            menuDelete.Click += postItMenu_Click;
            // 
            // menuS1
            // 
            menuS1.Name = "menuS1";
            menuS1.Size = new Size(317, 6);
            // 
            // menuExtendedEdition
            // 
            menuExtendedEdition.Name = "menuExtendedEdition";
            menuExtendedEdition.ShortcutKeys = Keys.Control | Keys.E;
            menuExtendedEdition.Size = new Size(320, 32);
            menuExtendedEdition.Text = "Extended edition ...";
            menuExtendedEdition.Click += postItMenu_Click;
            // 
            // menuPostItProperties
            // 
            menuPostItProperties.Name = "menuPostItProperties";
            menuPostItProperties.ShortcutKeys = Keys.Control | Keys.P;
            menuPostItProperties.Size = new Size(320, 32);
            menuPostItProperties.Text = "PostIt properties ...";
            menuPostItProperties.Click += postItMenu_Click;
            // 
            // menuWindowsFormView
            // 
            menuWindowsFormView.Name = "menuWindowsFormView";
            menuWindowsFormView.ShortcutKeys = Keys.Control | Keys.W;
            menuWindowsFormView.Size = new Size(320, 32);
            menuWindowsFormView.Text = "Windows Form View ";
            menuWindowsFormView.Click += postItMenu_Click;
            // 
            // menuS2
            // 
            menuS2.Name = "menuS2";
            menuS2.Size = new Size(317, 6);
            // 
            // menuAlarmWithin
            // 
            menuAlarmWithin.DropDownItems.AddRange(new ToolStripItem[] { menuFastAlarm10m, menuFastAlarm30m, menuFastAlarm1h, menuFastAlarm2h, menuFastAlarm4h, menuFastAlarm8h, menuFastAlarm10h, menuFastAlarm12h, menuFastAlarm24h, menuFastAlarm1week, menuFastAlarm1month, menuFastAlarm1year });
            menuAlarmWithin.Name = "menuAlarmWithin";
            menuAlarmWithin.Size = new Size(320, 32);
            menuAlarmWithin.Text = "Activate note &alarm within";
            // 
            // menuFastAlarm10m
            // 
            menuFastAlarm10m.Name = "menuFastAlarm10m";
            menuFastAlarm10m.Size = new Size(202, 34);
            menuFastAlarm10m.Text = "10 minutes";
            menuFastAlarm10m.Click += postItMenu_Click;
            // 
            // menuFastAlarm30m
            // 
            menuFastAlarm30m.Name = "menuFastAlarm30m";
            menuFastAlarm30m.Size = new Size(202, 34);
            menuFastAlarm30m.Text = "30 minutes";
            menuFastAlarm30m.Click += postItMenu_Click;
            // 
            // menuFastAlarm1h
            // 
            menuFastAlarm1h.Name = "menuFastAlarm1h";
            menuFastAlarm1h.Size = new Size(202, 34);
            menuFastAlarm1h.Text = "1 hour";
            menuFastAlarm1h.Click += postItMenu_Click;
            // 
            // menuFastAlarm2h
            // 
            menuFastAlarm2h.Name = "menuFastAlarm2h";
            menuFastAlarm2h.Size = new Size(202, 34);
            menuFastAlarm2h.Text = "2 hours";
            menuFastAlarm2h.Click += postItMenu_Click;
            // 
            // menuFastAlarm4h
            // 
            menuFastAlarm4h.Name = "menuFastAlarm4h";
            menuFastAlarm4h.Size = new Size(202, 34);
            menuFastAlarm4h.Text = "4 hours";
            menuFastAlarm4h.Click += postItMenu_Click;
            // 
            // menuFastAlarm8h
            // 
            menuFastAlarm8h.Name = "menuFastAlarm8h";
            menuFastAlarm8h.Size = new Size(202, 34);
            menuFastAlarm8h.Text = "8 hours";
            menuFastAlarm8h.Click += postItMenu_Click;
            // 
            // menuFastAlarm10h
            // 
            menuFastAlarm10h.Name = "menuFastAlarm10h";
            menuFastAlarm10h.Size = new Size(202, 34);
            menuFastAlarm10h.Text = "10 hours";
            menuFastAlarm10h.Click += postItMenu_Click;
            // 
            // menuFastAlarm12h
            // 
            menuFastAlarm12h.Name = "menuFastAlarm12h";
            menuFastAlarm12h.Size = new Size(202, 34);
            menuFastAlarm12h.Text = "12 hours";
            menuFastAlarm12h.Click += postItMenu_Click;
            // 
            // menuFastAlarm24h
            // 
            menuFastAlarm24h.Name = "menuFastAlarm24h";
            menuFastAlarm24h.Size = new Size(202, 34);
            menuFastAlarm24h.Text = "24 hours";
            menuFastAlarm24h.Click += postItMenu_Click;
            // 
            // menuFastAlarm1week
            // 
            menuFastAlarm1week.Name = "menuFastAlarm1week";
            menuFastAlarm1week.Size = new Size(202, 34);
            menuFastAlarm1week.Text = "1 week";
            menuFastAlarm1week.Click += postItMenu_Click;
            // 
            // menuFastAlarm1month
            // 
            menuFastAlarm1month.Name = "menuFastAlarm1month";
            menuFastAlarm1month.Size = new Size(202, 34);
            menuFastAlarm1month.Text = "1 month";
            menuFastAlarm1month.Click += postItMenu_Click;
            // 
            // menuFastAlarm1year
            // 
            menuFastAlarm1year.Name = "menuFastAlarm1year";
            menuFastAlarm1year.Size = new Size(202, 34);
            menuFastAlarm1year.Text = "1 year";
            menuFastAlarm1year.Click += postItMenu_Click;
            // 
            // menuMoreActions
            // 
            menuMoreActions.DropDownItems.AddRange(new ToolStripItem[] { menuAddResolvedTask });
            menuMoreActions.Name = "menuMoreActions";
            menuMoreActions.Size = new Size(320, 32);
            menuMoreActions.Text = "More actions";
            // 
            // menuAddResolvedTask
            // 
            menuAddResolvedTask.Name = "menuAddResolvedTask";
            menuAddResolvedTask.Size = new Size(304, 34);
            menuAddResolvedTask.Text = "Add quick resolved task";
            menuAddResolvedTask.Click += postItMenu_Click;
            // 
            // PostItEditorForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Info;
            ClientSize = new Size(687, 570);
            ControlBox = false;
            Controls.Add(panelForm);
            FormBorderStyle = FormBorderStyle.None;
            KeyPreview = true;
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PostItEditorForm";
            ShowInTaskbar = false;
            Text = "Post It View";
            FormClosing += PostItEditorForm_FormClosing;
            Load += PostItEditorForm_Load;
            Paint += PostItEditorForm_Paint;
            KeyUp += PostItEditorForm_KeyUp;
            panelForm.ResumeLayout(false);
            panelContent.ResumeLayout(false);
            panelFooter.ResumeLayout(false);
            panelCaptionHeader.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picResize).EndInit();
            ((System.ComponentModel.ISupportInitialize)picMenu).EndInit();
            menuPostIt.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.Panel panelContent;
        private System.Windows.Forms.Panel panelCaptionHeader;
        private System.Windows.Forms.Panel panelFooter;
        private System.Windows.Forms.PictureBox picMenu;
        private System.Windows.Forms.PictureBox picResize;
        private System.Windows.Forms.ContextMenuStrip menuPostIt;
        private System.Windows.Forms.ToolStripMenuItem menuHide;
        private System.Windows.Forms.ToolStripMenuItem menuAlwaysFront;
        private System.Windows.Forms.ToolStripMenuItem menuSaveNow;
        private System.Windows.Forms.ToolStripMenuItem menuDelete;
        private System.Windows.Forms.ToolStripSeparator menuS1;
        private System.Windows.Forms.ToolStripMenuItem menuExtendedEdition;
        private System.Windows.Forms.ToolStripMenuItem menuPostItProperties;
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
        private LabelNoCopy labelStatus;
        private LabelNoCopy labelCaption;
        private ToolStripMenuItem menuWindowsFormView;
        private KntWebView.KntEditView kntEditView;
        private ProgressBar progressStatus;
    }
}