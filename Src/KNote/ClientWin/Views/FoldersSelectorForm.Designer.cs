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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FoldersSelectorForm));
            imageListFolders = new ImageList(components);
            contextMenu = new KNote.ClientWin.Utils.KntContextMenuStrip(components);
            panelForm = new Panel();
            treeViewFolders = new TreeView();
            panelBottom = new Panel();
            buttonCancel = new Button();
            buttonAccept = new Button();
            panelForm.SuspendLayout();
            panelBottom.SuspendLayout();
            SuspendLayout();
            // 
            // imageListFolders
            // 
            // 
            // contextMenu
            // 
            contextMenu.Name = "contextMenuFolders";
            contextMenu.Size = new Size(61, 4);
            // 
            // panelForm
            // 
            panelForm.Controls.Add(treeViewFolders);
            panelForm.Controls.Add(panelBottom);
            panelForm.Dock = DockStyle.Fill;
            panelForm.Location = new Point(0, 0);
            panelForm.Name = "panelForm";
            panelForm.Size = new Size(460, 468);
            panelForm.TabIndex = 2;
            // 
            // treeViewFolders
            // 
            treeViewFolders.BorderStyle = BorderStyle.None;
            treeViewFolders.ContextMenuStrip = contextMenu;
            treeViewFolders.Dock = DockStyle.Fill;
            treeViewFolders.ImageIndex = 0;
            treeViewFolders.ImageList = imageListFolders;
            treeViewFolders.Location = new Point(0, 0);
            treeViewFolders.Name = "treeViewFolders";
            treeViewFolders.SelectedImageIndex = 0;
            treeViewFolders.Size = new Size(460, 424);
            treeViewFolders.TabIndex = 0;
            treeViewFolders.AfterSelect += treeViewFolders_AfterSelect;
            // 
            // panelBottom
            // 
            panelBottom.Controls.Add(buttonCancel);
            panelBottom.Controls.Add(buttonAccept);
            panelBottom.Dock = DockStyle.Bottom;
            panelBottom.Location = new Point(0, 424);
            panelBottom.Name = "panelBottom";
            panelBottom.Size = new Size(460, 44);
            panelBottom.TabIndex = 1;
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(362, 8);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(86, 24);
            buttonCancel.TabIndex = 1;
            buttonCancel.Text = "&Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // buttonAccept
            // 
            buttonAccept.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonAccept.DialogResult = DialogResult.OK;
            buttonAccept.Enabled = false;
            buttonAccept.Location = new Point(273, 8);
            buttonAccept.Name = "buttonAccept";
            buttonAccept.Size = new Size(83, 24);
            buttonAccept.TabIndex = 0;
            buttonAccept.Text = "&Accept";
            buttonAccept.UseVisualStyleBackColor = true;
            buttonAccept.Click += buttonAccept_Click;
            // 
            // FoldersSelectorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(460, 468);
            Controls.Add(panelForm);
            Name = "FoldersSelectorForm";
            Text = "Folders selector";
            panelForm.ResumeLayout(false);
            panelBottom.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.ImageList imageListFolders;
        private KNote.ClientWin.Utils.KntContextMenuStrip contextMenu;
        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.TreeView treeViewFolders;
    }
}