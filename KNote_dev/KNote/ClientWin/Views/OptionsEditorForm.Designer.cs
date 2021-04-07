
namespace KNote.ClientWin.Views
{
    partial class OptionsEditorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsEditorForm));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.panelForm = new System.Windows.Forms.Panel();
            this.buttonSelectDirectoryResources = new System.Windows.Forms.Button();
            this.buttonFolderSearch = new System.Windows.Forms.Button();
            this.panelForm.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(427, 377);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(64, 29);
            this.buttonCancel.TabIndex = 12;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonAccept
            // 
            this.buttonAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAccept.Location = new System.Drawing.Point(357, 377);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(64, 29);
            this.buttonAccept.TabIndex = 11;
            this.buttonAccept.Text = "&Accept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            // 
            // panelForm
            // 
            this.panelForm.Controls.Add(this.buttonSelectDirectoryResources);
            this.panelForm.Controls.Add(this.buttonFolderSearch);
            this.panelForm.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelForm.Location = new System.Drawing.Point(0, 0);
            this.panelForm.Name = "panelForm";
            this.panelForm.Size = new System.Drawing.Size(503, 363);
            this.panelForm.TabIndex = 10;
            // 
            // buttonSelectDirectoryResources
            // 
            this.buttonSelectDirectoryResources.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectDirectoryResources.Location = new System.Drawing.Point(781, 198);
            this.buttonSelectDirectoryResources.Name = "buttonSelectDirectoryResources";
            this.buttonSelectDirectoryResources.Size = new System.Drawing.Size(24, 23);
            this.buttonSelectDirectoryResources.TabIndex = 4;
            this.buttonSelectDirectoryResources.Text = "...";
            this.buttonSelectDirectoryResources.UseVisualStyleBackColor = true;
            // 
            // buttonFolderSearch
            // 
            this.buttonFolderSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFolderSearch.BackColor = System.Drawing.SystemColors.Control;
            this.buttonFolderSearch.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonFolderSearch.Location = new System.Drawing.Point(1083, 199);
            this.buttonFolderSearch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonFolderSearch.Name = "buttonFolderSearch";
            this.buttonFolderSearch.Size = new System.Drawing.Size(27, 25);
            this.buttonFolderSearch.TabIndex = 11;
            this.buttonFolderSearch.Text = "...";
            this.buttonFolderSearch.UseVisualStyleBackColor = false;
            // 
            // OptionsEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(503, 418);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonAccept);
            this.Controls.Add(this.panelForm);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsEditorForm";
            this.Text = "KaNote options";
            this.panelForm.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.Button buttonSelectDirectoryResources;
        private System.Windows.Forms.Button buttonFolderSearch;
    }
}