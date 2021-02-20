
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
            this.labelStatus = new System.Windows.Forms.Label();
            this.picResize = new System.Windows.Forms.PictureBox();
            this.picMenu = new System.Windows.Forms.PictureBox();
            this.textDescription = new System.Windows.Forms.TextBox();
            this.labelCaption = new System.Windows.Forms.Label();
            this.menuPostIt = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuHide = new System.Windows.Forms.ToolStripMenuItem();
            this.menuAlwaysFront = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSaveNow = new System.Windows.Forms.ToolStripMenuItem();
            this.menuDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.menuS1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuExtendedEdition = new System.Windows.Forms.ToolStripMenuItem();
            this.menuPostItProperties = new System.Windows.Forms.ToolStripMenuItem();
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
            this.menuPostItProperties});
            this.menuPostIt.Name = "menuPostIt";
            this.menuPostIt.Size = new System.Drawing.Size(216, 142);
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
    }
}