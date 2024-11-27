
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
            webView2 = new KntWebView.KWebView();
            htmlDescription = new MSDN.Html.Editor.HtmlEditorControl();
            labelStatus = new LabelNoCopy();
            picResize = new PictureBox();
            picMenu = new PictureBox();
            textDescription = new TextBox();
            labelCaption = new LabelNoCopy();
            menuPostIt = new ContextMenuStrip(components);
            menuHide = new ToolStripMenuItem();
            menuAlwaysFront = new ToolStripMenuItem();
            menuSaveNow = new ToolStripMenuItem();
            menuDelete = new ToolStripMenuItem();
            menuS1 = new ToolStripSeparator();
            menuExtendedEdition = new ToolStripMenuItem();
            menuPostItProperties = new ToolStripMenuItem();
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
            ((System.ComponentModel.ISupportInitialize)picResize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picMenu).BeginInit();
            menuPostIt.SuspendLayout();
            SuspendLayout();
            // 
            // panelForm
            // 
            panelForm.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelForm.Controls.Add(webView2);
            panelForm.Controls.Add(htmlDescription);
            panelForm.Controls.Add(labelStatus);
            panelForm.Controls.Add(picResize);
            panelForm.Controls.Add(picMenu);
            panelForm.Controls.Add(textDescription);
            panelForm.Controls.Add(labelCaption);
            panelForm.Location = new Point(1, 1);
            panelForm.Name = "panelForm";
            panelForm.Size = new Size(479, 340);
            panelForm.TabIndex = 3;
            // 
            // webView2
            // 
            webView2.EnableUrlBox = true;
            webView2.ForceHttps = false;
            webView2.IsInitialized = false;
            webView2.Location = new Point(244, 91);
            webView2.Name = "webView2";
            webView2.ShowNavigationTools = true;
            webView2.ShowStatusInfo = true;
            webView2.Size = new Size(215, 110);
            webView2.TabIndex = 10;
            webView2.TextUrl = "";
            webView2.Visible = false;
            // 
            // htmlDescription
            // 
            htmlDescription.InnerText = null;
            htmlDescription.Location = new Point(238, 29);
            htmlDescription.Name = "htmlDescription";
            htmlDescription.Size = new Size(215, 56);
            htmlDescription.TabIndex = 9;
            htmlDescription.ToolbarVisible = false;
            htmlDescription.Visible = false;
            // 
            // labelStatus
            // 
            labelStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            labelStatus.ForeColor = SystemColors.ControlDarkDark;
            labelStatus.Location = new Point(3, 322);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(456, 14);
            labelStatus.TabIndex = 7;
            labelStatus.Text = "...";
            labelStatus.DoubleClick += labelStatus_DoubleClick;
            // 
            // picResize
            // 
            picResize.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            picResize.Cursor = Cursors.SizeNWSE;
            picResize.Image = (Image)resources.GetObject("picResize.Image");
            picResize.Location = new Point(463, 324);
            picResize.Name = "picResize";
            picResize.Size = new Size(16, 16);
            picResize.TabIndex = 5;
            picResize.TabStop = false;
            picResize.MouseDown += picResize_MouseDown;
            picResize.MouseMove += picResize_MouseMove;
            // 
            // picMenu
            // 
            picMenu.Cursor = Cursors.Hand;
            picMenu.Image = (Image)resources.GetObject("picMenu.Image");
            picMenu.Location = new Point(5, 5);
            picMenu.Name = "picMenu";
            picMenu.Size = new Size(16, 16);
            picMenu.SizeMode = PictureBoxSizeMode.StretchImage;
            picMenu.TabIndex = 4;
            picMenu.TabStop = false;
            picMenu.MouseUp += picMenu_MouseUp;
            // 
            // textDescription
            // 
            textDescription.BackColor = Color.Beige;
            textDescription.BorderStyle = BorderStyle.None;
            textDescription.Location = new Point(14, 29);
            textDescription.Margin = new Padding(6);
            textDescription.MaxLength = 0;
            textDescription.Multiline = true;
            textDescription.Name = "textDescription";
            textDescription.Size = new Size(215, 70);
            textDescription.TabIndex = 3;
            // 
            // labelCaption
            // 
            labelCaption.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelCaption.BackColor = Color.PaleGoldenrod;
            labelCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelCaption.Location = new Point(3, 3);
            labelCaption.Name = "labelCaption";
            labelCaption.Padding = new Padding(20, 2, 0, 0);
            labelCaption.Size = new Size(472, 20);
            labelCaption.TabIndex = 6;
            labelCaption.Text = "...";
            labelCaption.DoubleClick += labelCaption_DoubleClick;
            labelCaption.MouseDown += labelCaption_MouseDown;
            labelCaption.MouseMove += labelCaption_MouseMove;
            // 
            // menuPostIt
            // 
            menuPostIt.Items.AddRange(new ToolStripItem[] { menuHide, menuAlwaysFront, menuSaveNow, menuDelete, menuS1, menuExtendedEdition, menuPostItProperties, menuS2, menuAlarmWithin, menuMoreActions });
            menuPostIt.Name = "menuPostIt";
            menuPostIt.Size = new Size(216, 214);
            // 
            // menuHide
            // 
            menuHide.Name = "menuHide";
            menuHide.ShortcutKeys = Keys.Control | Keys.Q;
            menuHide.Size = new Size(215, 22);
            menuHide.Text = "Hide note";
            menuHide.Click += postItMenu_Click;
            // 
            // menuAlwaysFront
            // 
            menuAlwaysFront.Name = "menuAlwaysFront";
            menuAlwaysFront.ShortcutKeys = Keys.Control | Keys.F;
            menuAlwaysFront.Size = new Size(215, 22);
            menuAlwaysFront.Text = "Always front";
            menuAlwaysFront.Click += postItMenu_Click;
            // 
            // menuSaveNow
            // 
            menuSaveNow.Name = "menuSaveNow";
            menuSaveNow.ShortcutKeys = Keys.Control | Keys.S;
            menuSaveNow.Size = new Size(215, 22);
            menuSaveNow.Text = "Save now";
            menuSaveNow.Click += postItMenu_Click;
            // 
            // menuDelete
            // 
            menuDelete.Name = "menuDelete";
            menuDelete.ShortcutKeys = Keys.Control | Keys.D;
            menuDelete.Size = new Size(215, 22);
            menuDelete.Text = "Delete";
            menuDelete.Click += postItMenu_Click;
            // 
            // menuS1
            // 
            menuS1.Name = "menuS1";
            menuS1.Size = new Size(212, 6);
            // 
            // menuExtendedEdition
            // 
            menuExtendedEdition.Name = "menuExtendedEdition";
            menuExtendedEdition.ShortcutKeys = Keys.Control | Keys.E;
            menuExtendedEdition.Size = new Size(215, 22);
            menuExtendedEdition.Text = "Extended edition ...";
            menuExtendedEdition.Click += postItMenu_Click;
            // 
            // menuPostItProperties
            // 
            menuPostItProperties.Name = "menuPostItProperties";
            menuPostItProperties.ShortcutKeys = Keys.Control | Keys.P;
            menuPostItProperties.Size = new Size(215, 22);
            menuPostItProperties.Text = "PostIt properties ...";
            menuPostItProperties.Click += postItMenu_Click;
            // 
            // menuS2
            // 
            menuS2.Name = "menuS2";
            menuS2.Size = new Size(212, 6);
            // 
            // menuAlarmWithin
            // 
            menuAlarmWithin.DropDownItems.AddRange(new ToolStripItem[] { menuFastAlarm10m, menuFastAlarm30m, menuFastAlarm1h, menuFastAlarm2h, menuFastAlarm4h, menuFastAlarm8h, menuFastAlarm10h, menuFastAlarm12h, menuFastAlarm24h, menuFastAlarm1week, menuFastAlarm1month, menuFastAlarm1year });
            menuAlarmWithin.Name = "menuAlarmWithin";
            menuAlarmWithin.Size = new Size(215, 22);
            menuAlarmWithin.Text = "Activate note &alarm within";
            // 
            // menuFastAlarm10m
            // 
            menuFastAlarm10m.Name = "menuFastAlarm10m";
            menuFastAlarm10m.Size = new Size(132, 22);
            menuFastAlarm10m.Text = "10 minutes";
            menuFastAlarm10m.Click += postItMenu_Click;
            // 
            // menuFastAlarm30m
            // 
            menuFastAlarm30m.Name = "menuFastAlarm30m";
            menuFastAlarm30m.Size = new Size(132, 22);
            menuFastAlarm30m.Text = "30 minutes";
            menuFastAlarm30m.Click += postItMenu_Click;
            // 
            // menuFastAlarm1h
            // 
            menuFastAlarm1h.Name = "menuFastAlarm1h";
            menuFastAlarm1h.Size = new Size(132, 22);
            menuFastAlarm1h.Text = "1 hour";
            menuFastAlarm1h.Click += postItMenu_Click;
            // 
            // menuFastAlarm2h
            // 
            menuFastAlarm2h.Name = "menuFastAlarm2h";
            menuFastAlarm2h.Size = new Size(132, 22);
            menuFastAlarm2h.Text = "2 hours";
            menuFastAlarm2h.Click += postItMenu_Click;
            // 
            // menuFastAlarm4h
            // 
            menuFastAlarm4h.Name = "menuFastAlarm4h";
            menuFastAlarm4h.Size = new Size(132, 22);
            menuFastAlarm4h.Text = "4 hours";
            menuFastAlarm4h.Click += postItMenu_Click;
            // 
            // menuFastAlarm8h
            // 
            menuFastAlarm8h.Name = "menuFastAlarm8h";
            menuFastAlarm8h.Size = new Size(132, 22);
            menuFastAlarm8h.Text = "8 hours";
            menuFastAlarm8h.Click += postItMenu_Click;
            // 
            // menuFastAlarm10h
            // 
            menuFastAlarm10h.Name = "menuFastAlarm10h";
            menuFastAlarm10h.Size = new Size(132, 22);
            menuFastAlarm10h.Text = "10 hours";
            menuFastAlarm10h.Click += postItMenu_Click;
            // 
            // menuFastAlarm12h
            // 
            menuFastAlarm12h.Name = "menuFastAlarm12h";
            menuFastAlarm12h.Size = new Size(132, 22);
            menuFastAlarm12h.Text = "12 hours";
            menuFastAlarm12h.Click += postItMenu_Click;
            // 
            // menuFastAlarm24h
            // 
            menuFastAlarm24h.Name = "menuFastAlarm24h";
            menuFastAlarm24h.Size = new Size(132, 22);
            menuFastAlarm24h.Text = "24 hours";
            menuFastAlarm24h.Click += postItMenu_Click;
            // 
            // menuFastAlarm1week
            // 
            menuFastAlarm1week.Name = "menuFastAlarm1week";
            menuFastAlarm1week.Size = new Size(132, 22);
            menuFastAlarm1week.Text = "1 week";
            menuFastAlarm1week.Click += postItMenu_Click;
            // 
            // menuFastAlarm1month
            // 
            menuFastAlarm1month.Name = "menuFastAlarm1month";
            menuFastAlarm1month.Size = new Size(132, 22);
            menuFastAlarm1month.Text = "1 month";
            menuFastAlarm1month.Click += postItMenu_Click;
            // 
            // menuFastAlarm1year
            // 
            menuFastAlarm1year.Name = "menuFastAlarm1year";
            menuFastAlarm1year.Size = new Size(132, 22);
            menuFastAlarm1year.Text = "1 year";
            menuFastAlarm1year.Click += postItMenu_Click;
            // 
            // menuMoreActions
            // 
            menuMoreActions.DropDownItems.AddRange(new ToolStripItem[] { menuAddResolvedTask });
            menuMoreActions.Name = "menuMoreActions";
            menuMoreActions.Size = new Size(215, 22);
            menuMoreActions.Text = "More actions";
            // 
            // menuAddResolvedTask
            // 
            menuAddResolvedTask.Name = "menuAddResolvedTask";
            menuAddResolvedTask.Size = new Size(224, 22);
            menuAddResolvedTask.Text = "Add automatic resolved task";
            menuAddResolvedTask.Click += postItMenu_Click;
            // 
            // PostItEditorForm
            // 
            AutoScaleMode = AutoScaleMode.Inherit;
            BackColor = SystemColors.Info;
            ClientSize = new Size(481, 342);
            ControlBox = false;
            Controls.Add(panelForm);
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PostItEditorForm";
            ShowInTaskbar = false;
            Text = "PostIt editor form";
            FormClosing += PostItEditorForm_FormClosing;
            Load += PostItEditorForm_Load;
            Paint += PostItEditorForm_Paint;
            KeyUp += PostItEditorForm_KeyUp;
            panelForm.ResumeLayout(false);
            panelForm.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picResize).EndInit();
            ((System.ComponentModel.ISupportInitialize)picMenu).EndInit();
            menuPostIt.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.TextBox textDescription;
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
        private MSDN.Html.Editor.HtmlEditorControl htmlDescription;
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
        private KntWebView.KWebView webView2;
    }
}