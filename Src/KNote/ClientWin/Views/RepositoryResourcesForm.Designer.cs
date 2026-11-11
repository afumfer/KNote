namespace KNote.ClientWin.Views
{
    partial class RepositoryResourcesForm
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
            this.panelForm = new System.Windows.Forms.Panel();
            this.labelDescription = new System.Windows.Forms.Label();
            this.labelCounts = new System.Windows.Forms.Label();
            this.labelLastRun = new System.Windows.Forms.Label();
            this.checkPhotos = new System.Windows.Forms.CheckBox();
            this.checkDocuments = new System.Windows.Forms.CheckBox();
            this.checkPages = new System.Windows.Forms.CheckBox();
            this.checkOcrPages = new System.Windows.Forms.CheckBox();
            this.labelPersonPhotos = new System.Windows.Forms.Label();
            this.radioSkip = new System.Windows.Forms.RadioButton();
            this.radioLock = new System.Windows.Forms.RadioButton();
            this.radioReplace = new System.Windows.Forms.RadioButton();
            this.labelWarning = new System.Windows.Forms.Label();
            this.labelProgress = new System.Windows.Forms.Label();
            this.progressBarRun = new System.Windows.Forms.ProgressBar();
            this.listErrors = new System.Windows.Forms.ListBox();
            this.buttonCancelRun = new System.Windows.Forms.Button();
            this.buttonRun = new System.Windows.Forms.Button();
            this.panelForm.SuspendLayout();
            this.SuspendLayout();
            //
            // panelForm
            //
            this.panelForm.Controls.Add(this.buttonRun);
            this.panelForm.Controls.Add(this.buttonCancelRun);
            this.panelForm.Controls.Add(this.listErrors);
            this.panelForm.Controls.Add(this.progressBarRun);
            this.panelForm.Controls.Add(this.labelProgress);
            this.panelForm.Controls.Add(this.labelWarning);
            this.panelForm.Controls.Add(this.radioReplace);
            this.panelForm.Controls.Add(this.radioLock);
            this.panelForm.Controls.Add(this.radioSkip);
            this.panelForm.Controls.Add(this.labelPersonPhotos);
            this.panelForm.Controls.Add(this.checkOcrPages);
            this.panelForm.Controls.Add(this.checkPages);
            this.panelForm.Controls.Add(this.checkDocuments);
            this.panelForm.Controls.Add(this.checkPhotos);
            this.panelForm.Controls.Add(this.labelLastRun);
            this.panelForm.Controls.Add(this.labelCounts);
            this.panelForm.Controls.Add(this.labelDescription);
            this.panelForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelForm.Location = new System.Drawing.Point(0, 0);
            this.panelForm.Name = "panelForm";
            this.panelForm.Size = new System.Drawing.Size(626, 570);
            this.panelForm.TabIndex = 0;
            //
            // labelDescription
            //
            this.labelDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDescription.Location = new System.Drawing.Point(9, 12);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(608, 48);
            this.labelDescription.TabIndex = 10;
            //
            // labelCounts
            //
            this.labelCounts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCounts.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.labelCounts.Location = new System.Drawing.Point(9, 69);
            this.labelCounts.Name = "labelCounts";
            this.labelCounts.Size = new System.Drawing.Size(608, 66);
            this.labelCounts.TabIndex = 11;
            //
            // labelLastRun
            //
            this.labelLastRun.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLastRun.Location = new System.Drawing.Point(9, 137);
            this.labelLastRun.Name = "labelLastRun";
            this.labelLastRun.Size = new System.Drawing.Size(608, 20);
            this.labelLastRun.TabIndex = 12;
            //
            // labelPersonPhotos
            //
            this.labelPersonPhotos.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelPersonPhotos.Location = new System.Drawing.Point(9, 261);
            this.labelPersonPhotos.Name = "labelPersonPhotos";
            this.labelPersonPhotos.Size = new System.Drawing.Size(608, 34);
            this.labelPersonPhotos.TabIndex = 13;
            //
            // checkPhotos
            //
            this.checkPhotos.AutoSize = true;
            this.checkPhotos.Checked = true;
            this.checkPhotos.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkPhotos.Location = new System.Drawing.Point(12, 163);
            this.checkPhotos.Name = "checkPhotos";
            this.checkPhotos.Size = new System.Drawing.Size(140, 19);
            this.checkPhotos.TabIndex = 6;
            this.checkPhotos.Text = "Photos";
            this.checkPhotos.UseVisualStyleBackColor = true;
            this.checkPhotos.CheckedChanged += new System.EventHandler(this.checkProcess_CheckedChanged);
            //
            // checkDocuments
            //
            this.checkDocuments.AutoSize = true;
            this.checkDocuments.Checked = true;
            this.checkDocuments.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkDocuments.Location = new System.Drawing.Point(12, 186);
            this.checkDocuments.Name = "checkDocuments";
            this.checkDocuments.Size = new System.Drawing.Size(160, 19);
            this.checkDocuments.TabIndex = 7;
            this.checkDocuments.Text = "Documents";
            this.checkDocuments.UseVisualStyleBackColor = true;
            this.checkDocuments.CheckedChanged += new System.EventHandler(this.checkProcess_CheckedChanged);
            // 
            // checkPages
            // 
            this.checkPages.AutoSize = true;
            this.checkPages.Checked = true;
            this.checkPages.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkPages.Location = new System.Drawing.Point(12, 209);
            this.checkPages.Name = "checkPages";
            this.checkPages.Size = new System.Drawing.Size(160, 19);
            this.checkPages.TabIndex = 8;
            this.checkPages.Text = "Scanned pages";
            this.checkPages.UseVisualStyleBackColor = true;
            this.checkPages.CheckedChanged += new System.EventHandler(this.checkProcess_CheckedChanged);
            // 
            // checkOcrPages
            // 
            this.checkOcrPages.AutoSize = true;
            this.checkOcrPages.Checked = true;
            this.checkOcrPages.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkOcrPages.Location = new System.Drawing.Point(30, 232);
            this.checkOcrPages.Name = "checkOcrPages";
            this.checkOcrPages.Size = new System.Drawing.Size(160, 19);
            this.checkOcrPages.TabIndex = 9;
            this.checkOcrPages.Text = "Also the pages with an OCR layer";
            this.checkOcrPages.UseVisualStyleBackColor = true;
            this.checkOcrPages.CheckedChanged += new System.EventHandler(this.checkProcess_CheckedChanged);
            //
            // radioSkip
            //
            this.radioSkip.AutoSize = true;
            this.radioSkip.Checked = true;
            this.radioSkip.Location = new System.Drawing.Point(21, 297);
            this.radioSkip.Name = "radioSkip";
            this.radioSkip.Size = new System.Drawing.Size(220, 19);
            this.radioSkip.TabIndex = 3;
            this.radioSkip.TabStop = true;
            this.radioSkip.UseVisualStyleBackColor = true;
            this.radioSkip.CheckedChanged += new System.EventHandler(this.radioPersonDescriptions_CheckedChanged);
            //
            // radioLock
            //
            this.radioLock.AutoSize = true;
            this.radioLock.Location = new System.Drawing.Point(21, 320);
            this.radioLock.Name = "radioLock";
            this.radioLock.Size = new System.Drawing.Size(160, 19);
            this.radioLock.TabIndex = 4;
            this.radioLock.UseVisualStyleBackColor = true;
            this.radioLock.CheckedChanged += new System.EventHandler(this.radioPersonDescriptions_CheckedChanged);
            //
            // radioReplace
            //
            this.radioReplace.AutoSize = true;
            this.radioReplace.Location = new System.Drawing.Point(21, 343);
            this.radioReplace.Name = "radioReplace";
            this.radioReplace.Size = new System.Drawing.Size(380, 19);
            this.radioReplace.TabIndex = 5;
            this.radioReplace.UseVisualStyleBackColor = true;
            this.radioReplace.CheckedChanged += new System.EventHandler(this.radioPersonDescriptions_CheckedChanged);
            //
            // labelWarning
            //
            this.labelWarning.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelWarning.Location = new System.Drawing.Point(9, 371);
            this.labelWarning.Name = "labelWarning";
            this.labelWarning.Size = new System.Drawing.Size(608, 34);
            this.labelWarning.TabIndex = 14;
            //
            // labelProgress
            //
            this.labelProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelProgress.Location = new System.Drawing.Point(9, 409);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(608, 20);
            this.labelProgress.TabIndex = 15;
            //
            // progressBarRun
            //
            this.progressBarRun.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBarRun.Location = new System.Drawing.Point(9, 432);
            this.progressBarRun.Name = "progressBarRun";
            this.progressBarRun.Size = new System.Drawing.Size(608, 12);
            this.progressBarRun.TabIndex = 16;
            //
            // listErrors
            //
            this.listErrors.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listErrors.FormattingEnabled = true;
            this.listErrors.HorizontalScrollbar = true;
            this.listErrors.IntegralHeight = false;
            this.listErrors.Location = new System.Drawing.Point(9, 452);
            this.listErrors.Name = "listErrors";
            this.listErrors.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.listErrors.Size = new System.Drawing.Size(608, 72);
            this.listErrors.TabIndex = 17;
            //
            // buttonCancelRun
            //
            this.buttonCancelRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCancelRun.Location = new System.Drawing.Point(9, 533);
            this.buttonCancelRun.Name = "buttonCancelRun";
            this.buttonCancelRun.Size = new System.Drawing.Size(110, 29);
            this.buttonCancelRun.TabIndex = 2;
            this.buttonCancelRun.Text = "Cancel &run";
            this.buttonCancelRun.UseVisualStyleBackColor = true;
            this.buttonCancelRun.Click += new System.EventHandler(this.buttonCancelRun_Click);
            //
            // buttonRun
            //
            this.buttonRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRun.Location = new System.Drawing.Point(417, 533);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(200, 29);
            this.buttonRun.TabIndex = 0;
            this.buttonRun.Text = "&Process pending resources";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            //
            // RepositoryResourcesForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(626, 570);
            this.Controls.Add(this.panelForm);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "RepositoryResourcesForm";
            this.Text = "Repository resources";
            this.panelForm.ResumeLayout(false);
            this.panelForm.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.Label labelCounts;
        private System.Windows.Forms.Label labelLastRun;
        private System.Windows.Forms.CheckBox checkPhotos;
        private System.Windows.Forms.CheckBox checkDocuments;
        private System.Windows.Forms.CheckBox checkPages;
        private System.Windows.Forms.CheckBox checkOcrPages;
        private System.Windows.Forms.Label labelPersonPhotos;
        private System.Windows.Forms.RadioButton radioSkip;
        private System.Windows.Forms.RadioButton radioLock;
        private System.Windows.Forms.RadioButton radioReplace;
        private System.Windows.Forms.Label labelWarning;
        private System.Windows.Forms.Label labelProgress;
        private System.Windows.Forms.ProgressBar progressBarRun;
        private System.Windows.Forms.ListBox listErrors;
        private System.Windows.Forms.Button buttonCancelRun;
        private System.Windows.Forms.Button buttonRun;
    }
}
