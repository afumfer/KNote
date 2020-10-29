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
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.treeViewFolders = new System.Windows.Forms.TreeView();
            this.imageListFolders = new System.Windows.Forms.ImageList(this.components);
            this.contextMenuFolders = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonCancel);
            this.panel1.Controls.Add(this.buttonAccept);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 418);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(394, 51);
            this.panel1.TabIndex = 0;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(296, 11);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(86, 29);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonAccept
            // 
            this.buttonAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAccept.Location = new System.Drawing.Point(207, 11);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(83, 29);
            this.buttonAccept.TabIndex = 0;
            this.buttonAccept.Text = "&Acept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            // 
            // treeViewFolders
            // 
            this.treeViewFolders.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewFolders.Location = new System.Drawing.Point(0, 0);
            this.treeViewFolders.Name = "treeViewFolders";
            this.treeViewFolders.Size = new System.Drawing.Size(394, 418);
            this.treeViewFolders.TabIndex = 1;
            // 
            // imageListFolders
            // 
            this.imageListFolders.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageListFolders.ImageSize = new System.Drawing.Size(16, 16);
            this.imageListFolders.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // contextMenuFolders
            // 
            this.contextMenuFolders.Name = "contextMenuFolders";
            this.contextMenuFolders.Size = new System.Drawing.Size(61, 4);
            // 
            // FoldersSelectorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(394, 469);
            this.Controls.Add(this.treeViewFolders);
            this.Controls.Add(this.panel1);
            this.Name = "FoldersSelectorForm";
            this.Text = "Folders selector";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FoldersSelectorForm_FormClosing);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.TreeView treeViewFolders;
        private System.Windows.Forms.ImageList imageListFolders;
        private System.Windows.Forms.ContextMenuStrip contextMenuFolders;
    }
}