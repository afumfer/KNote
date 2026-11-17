namespace KNote.ClientWin.Views
{
    partial class RepositoryPeriodSummaryForm
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
            this.labelFrom = new System.Windows.Forms.Label();
            this.dateFrom = new System.Windows.Forms.DateTimePicker();
            this.checkFromBeginning = new System.Windows.Forms.CheckBox();
            this.labelTo = new System.Windows.Forms.Label();
            this.dateTo = new System.Windows.Forms.DateTimePicker();
            this.labelFolder = new System.Windows.Forms.Label();
            this.textFolder = new System.Windows.Forms.TextBox();
            this.buttonClearFolder = new System.Windows.Forms.Button();
            this.buttonFolder = new System.Windows.Forms.Button();
            this.labelKeywords = new System.Windows.Forms.Label();
            this.textKeywords = new System.Windows.Forms.TextBox();
            this.labelMeaning = new System.Windows.Forms.Label();
            this.textMeaning = new System.Windows.Forms.TextBox();
            this.labelCount = new System.Windows.Forms.Label();
            this.buttonReview = new System.Windows.Forms.Button();
            this.panelForm.SuspendLayout();
            this.SuspendLayout();
            //
            // panelForm
            //
            this.panelForm.Controls.Add(this.buttonReview);
            this.panelForm.Controls.Add(this.labelCount);
            this.panelForm.Controls.Add(this.textMeaning);
            this.panelForm.Controls.Add(this.labelMeaning);
            this.panelForm.Controls.Add(this.textKeywords);
            this.panelForm.Controls.Add(this.labelKeywords);
            this.panelForm.Controls.Add(this.buttonFolder);
            this.panelForm.Controls.Add(this.buttonClearFolder);
            this.panelForm.Controls.Add(this.textFolder);
            this.panelForm.Controls.Add(this.labelFolder);
            this.panelForm.Controls.Add(this.dateTo);
            this.panelForm.Controls.Add(this.labelTo);
            this.panelForm.Controls.Add(this.checkFromBeginning);
            this.panelForm.Controls.Add(this.dateFrom);
            this.panelForm.Controls.Add(this.labelFrom);
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
            this.labelDescription.Size = new System.Drawing.Size(608, 80);
            this.labelDescription.TabIndex = 20;
            //
            // labelFrom
            //
            this.labelFrom.AutoSize = true;
            this.labelFrom.Location = new System.Drawing.Point(9, 100);
            this.labelFrom.Name = "labelFrom";
            this.labelFrom.Size = new System.Drawing.Size(35, 15);
            this.labelFrom.TabIndex = 21;
            this.labelFrom.Text = "From";
            //
            // dateFrom
            //
            this.dateFrom.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateFrom.Location = new System.Drawing.Point(9, 118);
            this.dateFrom.Name = "dateFrom";
            this.dateFrom.Size = new System.Drawing.Size(130, 23);
            this.dateFrom.TabIndex = 0;
            this.dateFrom.ValueChanged += new System.EventHandler(this.period_Changed);
            //
            // checkFromBeginning
            //
            this.checkFromBeginning.AutoSize = true;
            this.checkFromBeginning.Location = new System.Drawing.Point(150, 120);
            this.checkFromBeginning.Name = "checkFromBeginning";
            this.checkFromBeginning.Size = new System.Drawing.Size(130, 19);
            this.checkFromBeginning.TabIndex = 1;
            this.checkFromBeginning.Text = "From the &beginning";
            this.checkFromBeginning.UseVisualStyleBackColor = true;
            this.checkFromBeginning.CheckedChanged += new System.EventHandler(this.period_Changed);
            //
            // labelTo
            //
            this.labelTo.AutoSize = true;
            this.labelTo.Location = new System.Drawing.Point(320, 100);
            this.labelTo.Name = "labelTo";
            this.labelTo.Size = new System.Drawing.Size(20, 15);
            this.labelTo.TabIndex = 22;
            this.labelTo.Text = "To";
            //
            // dateTo
            //
            this.dateTo.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTo.Location = new System.Drawing.Point(320, 118);
            this.dateTo.Name = "dateTo";
            this.dateTo.Size = new System.Drawing.Size(130, 23);
            this.dateTo.TabIndex = 2;
            this.dateTo.ValueChanged += new System.EventHandler(this.period_Changed);
            //
            // labelFolder
            //
            this.labelFolder.AutoSize = true;
            this.labelFolder.Location = new System.Drawing.Point(9, 154);
            this.labelFolder.Name = "labelFolder";
            this.labelFolder.Size = new System.Drawing.Size(150, 15);
            this.labelFolder.TabIndex = 23;
            this.labelFolder.Text = "Folder (and its subfolders)";
            //
            // textFolder
            //
            this.textFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textFolder.Location = new System.Drawing.Point(9, 172);
            this.textFolder.Name = "textFolder";
            this.textFolder.ReadOnly = true;
            this.textFolder.Size = new System.Drawing.Size(544, 23);
            this.textFolder.TabIndex = 3;
            this.textFolder.TabStop = false;
            //
            // buttonClearFolder
            //
            this.buttonClearFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClearFolder.Location = new System.Drawing.Point(558, 172);
            this.buttonClearFolder.Name = "buttonClearFolder";
            this.buttonClearFolder.Size = new System.Drawing.Size(27, 23);
            this.buttonClearFolder.TabIndex = 4;
            this.buttonClearFolder.Text = "X";
            this.buttonClearFolder.UseVisualStyleBackColor = true;
            this.buttonClearFolder.Click += new System.EventHandler(this.buttonClearFolder_Click);
            //
            // buttonFolder
            //
            this.buttonFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFolder.Location = new System.Drawing.Point(590, 172);
            this.buttonFolder.Name = "buttonFolder";
            this.buttonFolder.Size = new System.Drawing.Size(27, 23);
            this.buttonFolder.TabIndex = 5;
            this.buttonFolder.Text = "...";
            this.buttonFolder.UseVisualStyleBackColor = true;
            this.buttonFolder.Click += new System.EventHandler(this.buttonFolder_Click);
            //
            // labelKeywords
            //
            this.labelKeywords.AutoSize = true;
            this.labelKeywords.Location = new System.Drawing.Point(9, 208);
            this.labelKeywords.Name = "labelKeywords";
            this.labelKeywords.Size = new System.Drawing.Size(250, 15);
            this.labelKeywords.TabIndex = 24;
            this.labelKeywords.Text = "Keywords (! leaves a word out, \"phrases\")";
            //
            // textKeywords
            //
            this.textKeywords.Location = new System.Drawing.Point(9, 226);
            this.textKeywords.Name = "textKeywords";
            this.textKeywords.Size = new System.Drawing.Size(296, 23);
            this.textKeywords.TabIndex = 6;
            this.textKeywords.KeyDown += new System.Windows.Forms.KeyEventHandler(this.text_KeyDown);
            this.textKeywords.Leave += new System.EventHandler(this.textKeywords_Leave);
            //
            // labelMeaning
            //
            this.labelMeaning.AutoSize = true;
            this.labelMeaning.Location = new System.Drawing.Point(320, 208);
            this.labelMeaning.Name = "labelMeaning";
            this.labelMeaning.Size = new System.Drawing.Size(220, 15);
            this.labelMeaning.TabIndex = 25;
            this.labelMeaning.Text = "Talks about... (by meaning)";
            //
            // textMeaning
            //
            this.textMeaning.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textMeaning.Location = new System.Drawing.Point(320, 226);
            this.textMeaning.Name = "textMeaning";
            this.textMeaning.Size = new System.Drawing.Size(297, 23);
            this.textMeaning.TabIndex = 7;
            this.textMeaning.KeyDown += new System.Windows.Forms.KeyEventHandler(this.text_KeyDown);
            this.textMeaning.Leave += new System.EventHandler(this.textMeaning_Leave);
            //
            // labelCount
            //
            this.labelCount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCount.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.labelCount.Location = new System.Drawing.Point(9, 266);
            this.labelCount.Name = "labelCount";
            this.labelCount.Size = new System.Drawing.Size(608, 40);
            this.labelCount.TabIndex = 26;
            //
            // buttonReview
            //
            this.buttonReview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonReview.Location = new System.Drawing.Point(447, 312);
            this.buttonReview.Name = "buttonReview";
            this.buttonReview.Size = new System.Drawing.Size(170, 29);
            this.buttonReview.TabIndex = 8;
            this.buttonReview.Text = "&Review selection...";
            this.buttonReview.UseVisualStyleBackColor = true;
            this.buttonReview.Click += new System.EventHandler(this.buttonReview_Click);
            //
            // RepositoryPeriodSummaryForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(626, 570);
            this.Controls.Add(this.panelForm);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "RepositoryPeriodSummaryForm";
            this.Text = "Period summary";
            this.panelForm.ResumeLayout(false);
            this.panelForm.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.Label labelFrom;
        private System.Windows.Forms.DateTimePicker dateFrom;
        private System.Windows.Forms.CheckBox checkFromBeginning;
        private System.Windows.Forms.Label labelTo;
        private System.Windows.Forms.DateTimePicker dateTo;
        private System.Windows.Forms.Label labelFolder;
        private System.Windows.Forms.TextBox textFolder;
        private System.Windows.Forms.Button buttonClearFolder;
        private System.Windows.Forms.Button buttonFolder;
        private System.Windows.Forms.Label labelKeywords;
        private System.Windows.Forms.TextBox textKeywords;
        private System.Windows.Forms.Label labelMeaning;
        private System.Windows.Forms.TextBox textMeaning;
        private System.Windows.Forms.Label labelCount;
        private System.Windows.Forms.Button buttonReview;
    }
}
