namespace KNote.ClientWin.Views
{
    partial class RepositorySemanticIndexForm
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
            this.labelStatus = new System.Windows.Forms.Label();
            this.labelCounts = new System.Windows.Forms.Label();
            this.labelLastCreation = new System.Windows.Forms.Label();
            this.labelWarning = new System.Windows.Forms.Label();
            this.labelProgress = new System.Windows.Forms.Label();
            this.progressBarIndex = new System.Windows.Forms.ProgressBar();
            this.listErrors = new System.Windows.Forms.ListBox();
            this.labelOtherIndexes = new System.Windows.Forms.Label();
            this.listOtherIndexes = new System.Windows.Forms.ListBox();
            this.buttonDeleteIndex = new System.Windows.Forms.Button();
            this.buttonCancelRun = new System.Windows.Forms.Button();
            this.buttonRebuild = new System.Windows.Forms.Button();
            this.buttonRun = new System.Windows.Forms.Button();
            this.panelForm.SuspendLayout();
            this.SuspendLayout();
            //
            // panelForm
            //
            this.panelForm.Controls.Add(this.buttonRun);
            this.panelForm.Controls.Add(this.buttonRebuild);
            this.panelForm.Controls.Add(this.buttonCancelRun);
            this.panelForm.Controls.Add(this.buttonDeleteIndex);
            this.panelForm.Controls.Add(this.listOtherIndexes);
            this.panelForm.Controls.Add(this.labelOtherIndexes);
            this.panelForm.Controls.Add(this.listErrors);
            this.panelForm.Controls.Add(this.progressBarIndex);
            this.panelForm.Controls.Add(this.labelProgress);
            this.panelForm.Controls.Add(this.labelWarning);
            this.panelForm.Controls.Add(this.labelLastCreation);
            this.panelForm.Controls.Add(this.labelCounts);
            this.panelForm.Controls.Add(this.labelStatus);
            this.panelForm.Controls.Add(this.labelDescription);
            this.panelForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelForm.Location = new System.Drawing.Point(0, 0);
            this.panelForm.Name = "panelForm";
            this.panelForm.Size = new System.Drawing.Size(626, 440);
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
            this.labelDescription.Text = "The meaning of every note (its data, text, tasks and described resources), calculated with the repository's embedding model, that the search by meaning uses. The notes that change are updated before each search by meaning or with Update now.";
            //
            // labelStatus
            //
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.labelStatus.Location = new System.Drawing.Point(9, 69);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(608, 34);
            this.labelStatus.TabIndex = 11;
            //
            // labelCounts
            //
            this.labelCounts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCounts.Location = new System.Drawing.Point(9, 105);
            this.labelCounts.Name = "labelCounts";
            this.labelCounts.Size = new System.Drawing.Size(608, 20);
            this.labelCounts.TabIndex = 12;
            //
            // labelLastCreation
            //
            this.labelLastCreation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelLastCreation.Location = new System.Drawing.Point(9, 127);
            this.labelLastCreation.Name = "labelLastCreation";
            this.labelLastCreation.Size = new System.Drawing.Size(608, 20);
            this.labelLastCreation.TabIndex = 13;
            //
            // labelWarning
            //
            this.labelWarning.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelWarning.Location = new System.Drawing.Point(9, 149);
            this.labelWarning.Name = "labelWarning";
            this.labelWarning.Size = new System.Drawing.Size(608, 34);
            this.labelWarning.TabIndex = 14;
            //
            // labelProgress
            //
            this.labelProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelProgress.Location = new System.Drawing.Point(9, 187);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(608, 20);
            this.labelProgress.TabIndex = 15;
            //
            // progressBarIndex
            //
            this.progressBarIndex.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBarIndex.Location = new System.Drawing.Point(9, 210);
            this.progressBarIndex.Name = "progressBarIndex";
            this.progressBarIndex.Size = new System.Drawing.Size(608, 12);
            this.progressBarIndex.TabIndex = 16;
            //
            // listErrors
            //
            this.listErrors.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listErrors.FormattingEnabled = true;
            this.listErrors.HorizontalScrollbar = true;
            this.listErrors.IntegralHeight = false;
            this.listErrors.Location = new System.Drawing.Point(9, 230);
            this.listErrors.Name = "listErrors";
            this.listErrors.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.listErrors.Size = new System.Drawing.Size(608, 66);
            this.listErrors.TabIndex = 17;
            //
            // labelOtherIndexes
            //
            this.labelOtherIndexes.AutoSize = true;
            this.labelOtherIndexes.Location = new System.Drawing.Point(9, 304);
            this.labelOtherIndexes.Name = "labelOtherIndexes";
            this.labelOtherIndexes.Size = new System.Drawing.Size(266, 15);
            this.labelOtherIndexes.TabIndex = 18;
            this.labelOtherIndexes.Text = "Indexes of other models (kept to compare them):";
            //
            // listOtherIndexes
            //
            this.listOtherIndexes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listOtherIndexes.FormattingEnabled = true;
            this.listOtherIndexes.IntegralHeight = false;
            this.listOtherIndexes.Location = new System.Drawing.Point(9, 324);
            this.listOtherIndexes.Name = "listOtherIndexes";
            this.listOtherIndexes.Size = new System.Drawing.Size(472, 70);
            this.listOtherIndexes.TabIndex = 3;
            this.listOtherIndexes.SelectedIndexChanged += new System.EventHandler(this.listOtherIndexes_SelectedIndexChanged);
            //
            // buttonDeleteIndex
            //
            this.buttonDeleteIndex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDeleteIndex.Location = new System.Drawing.Point(489, 324);
            this.buttonDeleteIndex.Name = "buttonDeleteIndex";
            this.buttonDeleteIndex.Size = new System.Drawing.Size(128, 29);
            this.buttonDeleteIndex.TabIndex = 4;
            this.buttonDeleteIndex.Text = "R&emove index";
            this.buttonDeleteIndex.UseVisualStyleBackColor = true;
            this.buttonDeleteIndex.Click += new System.EventHandler(this.buttonDeleteIndex_Click);
            //
            // buttonCancelRun
            //
            this.buttonCancelRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCancelRun.Location = new System.Drawing.Point(9, 403);
            this.buttonCancelRun.Name = "buttonCancelRun";
            this.buttonCancelRun.Size = new System.Drawing.Size(110, 29);
            this.buttonCancelRun.TabIndex = 2;
            this.buttonCancelRun.Text = "Cancel &run";
            this.buttonCancelRun.UseVisualStyleBackColor = true;
            this.buttonCancelRun.Click += new System.EventHandler(this.buttonCancelRun_Click);
            //
            // buttonRebuild
            //
            this.buttonRebuild.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRebuild.Location = new System.Drawing.Point(391, 403);
            this.buttonRebuild.Name = "buttonRebuild";
            this.buttonRebuild.Size = new System.Drawing.Size(110, 29);
            this.buttonRebuild.TabIndex = 1;
            this.buttonRebuild.Text = "Re&build";
            this.buttonRebuild.UseVisualStyleBackColor = true;
            this.buttonRebuild.Click += new System.EventHandler(this.buttonRebuild_Click);
            //
            // buttonRun
            //
            this.buttonRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRun.Location = new System.Drawing.Point(507, 403);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(110, 29);
            this.buttonRun.TabIndex = 0;
            this.buttonRun.Text = "&Create index";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            //
            // RepositorySemanticIndexForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(626, 440);
            this.Controls.Add(this.panelForm);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "RepositorySemanticIndexForm";
            this.Text = "Repository semantic index";
            this.panelForm.ResumeLayout(false);
            this.panelForm.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Label labelCounts;
        private System.Windows.Forms.Label labelLastCreation;
        private System.Windows.Forms.Label labelWarning;
        private System.Windows.Forms.Label labelProgress;
        private System.Windows.Forms.ProgressBar progressBarIndex;
        private System.Windows.Forms.ListBox listErrors;
        private System.Windows.Forms.Label labelOtherIndexes;
        private System.Windows.Forms.ListBox listOtherIndexes;
        private System.Windows.Forms.Button buttonDeleteIndex;
        private System.Windows.Forms.Button buttonCancelRun;
        private System.Windows.Forms.Button buttonRebuild;
        private System.Windows.Forms.Button buttonRun;
    }
}
