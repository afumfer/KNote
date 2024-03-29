﻿
namespace KNote.ClientWin.Views
{
    partial class ResourceEditorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResourceEditorForm));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.panelForm = new System.Windows.Forms.Panel();
            this.htmlPreview = new KntWebView.KWebView();
            this.textFileName = new System.Windows.Forms.TextBox();
            this.labelPreview = new System.Windows.Forms.Label();
            this.textDescription = new System.Windows.Forms.TextBox();
            this.textOrder = new System.Windows.Forms.TextBox();
            this.labelDescription = new System.Windows.Forms.Label();
            this.labelOrder = new System.Windows.Forms.Label();
            this.buttonSelectFile = new System.Windows.Forms.Button();
            this.labelFileName = new System.Windows.Forms.Label();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.panelForm.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(493, 452);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(81, 29);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonAccept
            // 
            this.buttonAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAccept.Location = new System.Drawing.Point(406, 452);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(81, 29);
            this.buttonAccept.TabIndex = 5;
            this.buttonAccept.Text = "&Accept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.buttonAccept_Click);
            // 
            // panelForm
            // 
            this.panelForm.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelForm.Controls.Add(this.htmlPreview);
            this.panelForm.Controls.Add(this.textFileName);
            this.panelForm.Controls.Add(this.labelPreview);
            this.panelForm.Controls.Add(this.textDescription);
            this.panelForm.Controls.Add(this.textOrder);
            this.panelForm.Controls.Add(this.labelDescription);
            this.panelForm.Controls.Add(this.labelOrder);
            this.panelForm.Controls.Add(this.buttonSelectFile);
            this.panelForm.Controls.Add(this.labelFileName);
            this.panelForm.Location = new System.Drawing.Point(1, 3);
            this.panelForm.Name = "panelForm";
            this.panelForm.Size = new System.Drawing.Size(583, 443);
            this.panelForm.TabIndex = 5;
            // 
            // htmlPreview
            // 
            this.htmlPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.htmlPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.htmlPreview.EnableUrlBox = true;
            this.htmlPreview.ForceHttps = false;
            this.htmlPreview.IsInitialized = false;
            this.htmlPreview.Location = new System.Drawing.Point(288, 73);
            this.htmlPreview.Name = "htmlPreview";            
            this.htmlPreview.ShowNavigationTools = false;
            this.htmlPreview.ShowStatusInfo = false;
            this.htmlPreview.Size = new System.Drawing.Size(285, 361);
            this.htmlPreview.TabIndex = 17;
            this.htmlPreview.TextUrl = "";
            // 
            // textFileName
            // 
            this.textFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textFileName.Enabled = false;
            this.textFileName.Location = new System.Drawing.Point(78, 14);
            this.textFileName.Name = "textFileName";
            this.textFileName.Size = new System.Drawing.Size(378, 23);
            this.textFileName.TabIndex = 0;
            // 
            // labelPreview
            // 
            this.labelPreview.AutoSize = true;
            this.labelPreview.Location = new System.Drawing.Point(288, 55);
            this.labelPreview.Name = "labelPreview";
            this.labelPreview.Size = new System.Drawing.Size(51, 15);
            this.labelPreview.TabIndex = 16;
            this.labelPreview.Text = "Preview:";
            // 
            // textDescription
            // 
            this.textDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.textDescription.Location = new System.Drawing.Point(11, 73);
            this.textDescription.MaxLength = 3332767;
            this.textDescription.Multiline = true;
            this.textDescription.Name = "textDescription";
            this.textDescription.Size = new System.Drawing.Size(271, 327);
            this.textDescription.TabIndex = 2;
            // 
            // textOrder
            // 
            this.textOrder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textOrder.Location = new System.Drawing.Point(57, 411);
            this.textOrder.Name = "textOrder";
            this.textOrder.Size = new System.Drawing.Size(62, 23);
            this.textOrder.TabIndex = 4;
            // 
            // labelDescription
            // 
            this.labelDescription.AutoSize = true;
            this.labelDescription.Location = new System.Drawing.Point(11, 55);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(70, 15);
            this.labelDescription.TabIndex = 13;
            this.labelDescription.Text = "Description:";
            // 
            // labelOrder
            // 
            this.labelOrder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelOrder.AutoSize = true;
            this.labelOrder.Location = new System.Drawing.Point(11, 414);
            this.labelOrder.Name = "labelOrder";
            this.labelOrder.Size = new System.Drawing.Size(40, 15);
            this.labelOrder.TabIndex = 12;
            this.labelOrder.Text = "Order:";
            // 
            // buttonSelectFile
            // 
            this.buttonSelectFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectFile.Location = new System.Drawing.Point(462, 14);
            this.buttonSelectFile.Name = "buttonSelectFile";
            this.buttonSelectFile.Size = new System.Drawing.Size(111, 25);
            this.buttonSelectFile.TabIndex = 1;
            this.buttonSelectFile.Text = "Select file";
            this.buttonSelectFile.UseVisualStyleBackColor = true;
            this.buttonSelectFile.Click += new System.EventHandler(this.buttonSelectFile_Click);
            // 
            // labelFileName
            // 
            this.labelFileName.AutoSize = true;
            this.labelFileName.Location = new System.Drawing.Point(11, 17);
            this.labelFileName.Name = "labelFileName";
            this.labelFileName.Size = new System.Drawing.Size(61, 15);
            this.labelFileName.TabIndex = 10;
            this.labelFileName.Text = "File name:";
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog1";
            // 
            // ResourceEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(586, 493);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonAccept);
            this.Controls.Add(this.panelForm);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ResourceEditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Resource editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ResourceEditorForm_FormClosing);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.ResourceEditorForm_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ResourceEditorForm_KeyUp);
            this.panelForm.ResumeLayout(false);
            this.panelForm.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.Label labelPreview;
        private System.Windows.Forms.TextBox textDescription;
        private System.Windows.Forms.TextBox textOrder;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.Label labelOrder;
        private System.Windows.Forms.Button buttonSelectFile;
        private System.Windows.Forms.Label labelFileName;
        private System.Windows.Forms.TextBox textFileName;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private KntWebView.KWebView htmlPreview;
    }
}