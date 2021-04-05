namespace KNote.ClientWin.Views
{
    partial class FoldersSelectorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FoldersSelectorForm));
            this.imageListFolders = new System.Windows.Forms.ImageList(this.components);
            this.contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.panelForm = new System.Windows.Forms.Panel();
            this.treeViewFolders = new System.Windows.Forms.TreeView();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.panelForm.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageListFolders
            // 
            this.imageListFolders.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageListFolders.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListFolders.ImageStream")));
            this.imageListFolders.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListFolders.Images.SetKeyName(0, "folderOpene_16.png");
            this.imageListFolders.Images.SetKeyName(1, "folderLight_16.png");
            this.imageListFolders.Images.SetKeyName(2, "database_16.png");
            // 
            // contextMenu
            // 
            this.contextMenu.Name = "contextMenuFolders";
            this.contextMenu.Size = new System.Drawing.Size(61, 4);
            // 
            // panelForm
            // 
            this.panelForm.Controls.Add(this.treeViewFolders);
            this.panelForm.Controls.Add(this.panelBottom);
            this.panelForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelForm.Location = new System.Drawing.Point(0, 0);
            this.panelForm.Name = "panelForm";
            this.panelForm.Size = new System.Drawing.Size(460, 468);
            this.panelForm.TabIndex = 2;
            // 
            // treeViewFolders
            // 
            this.treeViewFolders.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeViewFolders.ContextMenuStrip = this.contextMenu;
            this.treeViewFolders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewFolders.ImageIndex = 0;
            this.treeViewFolders.ImageList = this.imageListFolders;
            this.treeViewFolders.Location = new System.Drawing.Point(0, 0);
            this.treeViewFolders.Name = "treeViewFolders";
            this.treeViewFolders.SelectedImageIndex = 0;
            this.treeViewFolders.Size = new System.Drawing.Size(460, 424);
            this.treeViewFolders.TabIndex = 1;
            this.treeViewFolders.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewFolders_AfterSelect);
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.buttonCancel);
            this.panelBottom.Controls.Add(this.buttonAccept);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 424);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(460, 44);
            this.panelBottom.TabIndex = 0;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(362, 8);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(86, 24);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonAccept
            // 
            this.buttonAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAccept.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonAccept.Location = new System.Drawing.Point(273, 8);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(83, 24);
            this.buttonAccept.TabIndex = 0;
            this.buttonAccept.Text = "&Accept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.buttonAccept_Click);
            // 
            // FoldersSelectorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(460, 468);
            this.Controls.Add(this.panelForm);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FoldersSelectorForm";
            this.Text = "Folders selector";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FoldersSelectorForm_FormClosing);
            this.panelForm.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ImageList imageListFolders;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.TreeView treeViewFolders;
    }
}