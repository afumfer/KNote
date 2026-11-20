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
            this.labelPending = new System.Windows.Forms.Label();
            this.linkRelevance = new System.Windows.Forms.LinkLabel();
            this.labelSummary = new System.Windows.Forms.Label();
            this.labelSubject = new System.Windows.Forms.Label();
            this.textSubject = new System.Windows.Forms.TextBox();
            this.labelWords = new System.Windows.Forms.Label();
            this.numericWords = new System.Windows.Forms.NumericUpDown();
            this.labelLanguage = new System.Windows.Forms.Label();
            this.comboLanguage = new System.Windows.Forms.ComboBox();
            this.labelModel = new System.Windows.Forms.Label();
            this.comboModel = new System.Windows.Forms.ComboBox();
            this.labelConcepts = new System.Windows.Forms.Label();
            this.checkedConcepts = new System.Windows.Forms.CheckedListBox();
            this.labelEstimate = new System.Windows.Forms.Label();
            this.progressSummary = new System.Windows.Forms.ProgressBar();
            this.labelProgress = new System.Windows.Forms.Label();
            this.linkOpenNote = new System.Windows.Forms.LinkLabel();
            this.buttonCancelSummary = new System.Windows.Forms.Button();
            this.buttonEstimate = new System.Windows.Forms.Button();
            this.buttonGenerate = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericWords)).BeginInit();
            this.progressClassify = new System.Windows.Forms.ProgressBar();
            this.buttonClassify = new System.Windows.Forms.Button();
            this.buttonCancelClassify = new System.Windows.Forms.Button();
            this.panelForm.SuspendLayout();
            this.SuspendLayout();
            //
            // panelForm
            //
            this.panelForm.Controls.Add(this.buttonCancelClassify);
            this.panelForm.Controls.Add(this.buttonClassify);
            this.panelForm.Controls.Add(this.progressClassify);
            this.panelForm.Controls.Add(this.labelPending);
            this.panelForm.Controls.Add(this.linkRelevance);
            this.panelForm.Controls.Add(this.labelSummary);
            this.panelForm.Controls.Add(this.labelSubject);
            this.panelForm.Controls.Add(this.textSubject);
            this.panelForm.Controls.Add(this.labelWords);
            this.panelForm.Controls.Add(this.numericWords);
            this.panelForm.Controls.Add(this.labelLanguage);
            this.panelForm.Controls.Add(this.comboLanguage);
            this.panelForm.Controls.Add(this.labelModel);
            this.panelForm.Controls.Add(this.comboModel);
            this.panelForm.Controls.Add(this.labelConcepts);
            this.panelForm.Controls.Add(this.checkedConcepts);
            this.panelForm.Controls.Add(this.labelEstimate);
            this.panelForm.Controls.Add(this.progressSummary);
            this.panelForm.Controls.Add(this.labelProgress);
            this.panelForm.Controls.Add(this.linkOpenNote);
            this.panelForm.Controls.Add(this.buttonCancelSummary);
            this.panelForm.Controls.Add(this.buttonEstimate);
            this.panelForm.Controls.Add(this.buttonGenerate);
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
            this.panelForm.AutoScroll = true;
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
            // labelPending
            //
            this.labelPending.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelPending.Location = new System.Drawing.Point(9, 356);
            this.labelPending.Name = "labelPending";
            this.labelPending.Size = new System.Drawing.Size(398, 36);
            this.labelPending.TabIndex = 27;
            //
            // linkRelevance
            //
            this.linkRelevance.AutoSize = true;
            this.linkRelevance.Location = new System.Drawing.Point(9, 318);
            this.linkRelevance.Name = "linkRelevance";
            this.linkRelevance.Size = new System.Drawing.Size(120, 15);
            this.linkRelevance.TabIndex = 11;
            this.linkRelevance.TabStop = true;
            this.linkRelevance.Text = "Note relevance";
            this.linkRelevance.Visible = false;
            this.linkRelevance.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkRelevance_LinkClicked);
            //
            // labelSummary
            //
            this.labelSummary.AutoSize = true;
            this.labelSummary.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.labelSummary.Location = new System.Drawing.Point(9, 416);
            this.labelSummary.Name = "labelSummary";
            this.labelSummary.Size = new System.Drawing.Size(60, 15);
            this.labelSummary.TabIndex = 30;
            this.labelSummary.Text = "Summary";
            //
            // labelSubject
            //
            this.labelSubject.AutoSize = true;
            this.labelSubject.Location = new System.Drawing.Point(9, 440);
            this.labelSubject.Name = "labelSubject";
            this.labelSubject.Size = new System.Drawing.Size(250, 15);
            this.labelSubject.TabIndex = 31;
            this.labelSubject.Text = "Subject (empty: the period and the filters)";
            //
            // textSubject
            //
            this.textSubject.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textSubject.Location = new System.Drawing.Point(9, 458);
            this.textSubject.Name = "textSubject";
            this.textSubject.Size = new System.Drawing.Size(500, 23);
            this.textSubject.TabIndex = 12;
            //
            // labelWords
            //
            this.labelWords.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelWords.AutoSize = true;
            this.labelWords.Location = new System.Drawing.Point(517, 440);
            this.labelWords.Name = "labelWords";
            this.labelWords.Size = new System.Drawing.Size(40, 15);
            this.labelWords.TabIndex = 32;
            this.labelWords.Text = "Words";
            //
            // numericWords
            //
            this.numericWords.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numericWords.Increment = new decimal(new int[] { 100, 0, 0, 0 });
            this.numericWords.Location = new System.Drawing.Point(517, 458);
            this.numericWords.Name = "numericWords";
            this.numericWords.Size = new System.Drawing.Size(100, 23);
            this.numericWords.TabIndex = 13;
            //
            // labelLanguage
            //
            this.labelLanguage.AutoSize = true;
            this.labelLanguage.Location = new System.Drawing.Point(9, 490);
            this.labelLanguage.Name = "labelLanguage";
            this.labelLanguage.Size = new System.Drawing.Size(200, 15);
            this.labelLanguage.TabIndex = 33;
            this.labelLanguage.Text = "Language (empty: of the notes)";
            //
            // comboLanguage
            //
            this.comboLanguage.FormattingEnabled = true;
            this.comboLanguage.Location = new System.Drawing.Point(9, 508);
            this.comboLanguage.Name = "comboLanguage";
            this.comboLanguage.Size = new System.Drawing.Size(190, 23);
            this.comboLanguage.TabIndex = 14;
            //
            // labelModel
            //
            this.labelModel.AutoSize = true;
            this.labelModel.Location = new System.Drawing.Point(210, 490);
            this.labelModel.Name = "labelModel";
            this.labelModel.Size = new System.Drawing.Size(41, 15);
            this.labelModel.TabIndex = 34;
            this.labelModel.Text = "Model";
            //
            // comboModel
            //
            this.comboModel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboModel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboModel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboModel.FormattingEnabled = true;
            this.comboModel.Location = new System.Drawing.Point(210, 508);
            this.comboModel.Name = "comboModel";
            this.comboModel.Size = new System.Drawing.Size(407, 23);
            this.comboModel.TabIndex = 15;
            //
            // labelConcepts
            //
            this.labelConcepts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelConcepts.Location = new System.Drawing.Point(9, 542);
            this.labelConcepts.Name = "labelConcepts";
            this.labelConcepts.Size = new System.Drawing.Size(608, 15);
            this.labelConcepts.TabIndex = 38;
            this.labelConcepts.Text = "Concept attributes";
            //
            // checkedConcepts
            //
            this.checkedConcepts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkedConcepts.CheckOnClick = true;
            this.checkedConcepts.FormattingEnabled = true;
            this.checkedConcepts.IntegralHeight = false;
            this.checkedConcepts.Location = new System.Drawing.Point(9, 560);
            this.checkedConcepts.MultiColumn = true;
            this.checkedConcepts.ColumnWidth = 200;
            this.checkedConcepts.Name = "checkedConcepts";
            this.checkedConcepts.Size = new System.Drawing.Size(608, 94);
            this.checkedConcepts.TabIndex = 16;
            //
            // labelEstimate
            //
            this.labelEstimate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelEstimate.Location = new System.Drawing.Point(9, 662);
            this.labelEstimate.Name = "labelEstimate";
            this.labelEstimate.Size = new System.Drawing.Size(608, 64);
            this.labelEstimate.TabIndex = 35;
            //
            // progressSummary
            //
            this.progressSummary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressSummary.Location = new System.Drawing.Point(9, 730);
            this.progressSummary.Name = "progressSummary";
            this.progressSummary.Size = new System.Drawing.Size(608, 8);
            this.progressSummary.TabIndex = 36;
            this.progressSummary.Visible = false;
            //
            // labelProgress
            //
            this.labelProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelProgress.Location = new System.Drawing.Point(9, 742);
            this.labelProgress.Name = "labelProgress";
            this.labelProgress.Size = new System.Drawing.Size(608, 36);
            this.labelProgress.TabIndex = 37;
            //
            // linkOpenNote
            //
            this.linkOpenNote.AutoSize = true;
            this.linkOpenNote.Location = new System.Drawing.Point(9, 790);
            this.linkOpenNote.Name = "linkOpenNote";
            this.linkOpenNote.Size = new System.Drawing.Size(85, 15);
            this.linkOpenNote.TabIndex = 20;
            this.linkOpenNote.TabStop = true;
            this.linkOpenNote.Text = "Open the note";
            this.linkOpenNote.Visible = false;
            this.linkOpenNote.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkOpenNote_LinkClicked);
            //
            // buttonCancelSummary
            //
            this.buttonCancelSummary.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCancelSummary.Location = new System.Drawing.Point(197, 784);
            this.buttonCancelSummary.Name = "buttonCancelSummary";
            this.buttonCancelSummary.Size = new System.Drawing.Size(100, 29);
            this.buttonCancelSummary.TabIndex = 19;
            this.buttonCancelSummary.Text = "Cancel";
            this.buttonCancelSummary.UseVisualStyleBackColor = true;
            this.buttonCancelSummary.Visible = false;
            this.buttonCancelSummary.Click += new System.EventHandler(this.buttonCancelSummary_Click);
            //
            // buttonEstimate
            //
            this.buttonEstimate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonEstimate.Location = new System.Drawing.Point(303, 784);
            this.buttonEstimate.Name = "buttonEstimate";
            this.buttonEstimate.Size = new System.Drawing.Size(150, 29);
            this.buttonEstimate.TabIndex = 17;
            this.buttonEstimate.Text = "&Estimate";
            this.buttonEstimate.UseVisualStyleBackColor = true;
            this.buttonEstimate.Click += new System.EventHandler(this.buttonEstimate_Click);
            //
            // buttonGenerate
            //
            this.buttonGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonGenerate.Location = new System.Drawing.Point(467, 784);
            this.buttonGenerate.Name = "buttonGenerate";
            this.buttonGenerate.Size = new System.Drawing.Size(150, 29);
            this.buttonGenerate.TabIndex = 18;
            this.buttonGenerate.Text = "&Generate";
            this.buttonGenerate.UseVisualStyleBackColor = true;
            this.buttonGenerate.Click += new System.EventHandler(this.buttonGenerate_Click);
            //
            // progressClassify
            //
            this.progressClassify.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressClassify.Location = new System.Drawing.Point(9, 394);
            this.progressClassify.Name = "progressClassify";
            this.progressClassify.Size = new System.Drawing.Size(398, 8);
            this.progressClassify.TabIndex = 28;
            this.progressClassify.Visible = false;
            //
            // buttonClassify
            //
            this.buttonClassify.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClassify.Location = new System.Drawing.Point(417, 352);
            this.buttonClassify.Name = "buttonClassify";
            this.buttonClassify.Size = new System.Drawing.Size(200, 29);
            this.buttonClassify.TabIndex = 9;
            this.buttonClassify.Text = "&Classify doubtful notes with AI...";
            this.buttonClassify.UseVisualStyleBackColor = true;
            this.buttonClassify.Visible = false;
            this.buttonClassify.Click += new System.EventHandler(this.buttonClassify_Click);
            //
            // buttonCancelClassify
            //
            this.buttonCancelClassify.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancelClassify.Location = new System.Drawing.Point(517, 352);
            this.buttonCancelClassify.Name = "buttonCancelClassify";
            this.buttonCancelClassify.Size = new System.Drawing.Size(100, 29);
            this.buttonCancelClassify.TabIndex = 10;
            this.buttonCancelClassify.Text = "Ca&ncel";
            this.buttonCancelClassify.UseVisualStyleBackColor = true;
            this.buttonCancelClassify.Visible = false;
            this.buttonCancelClassify.Click += new System.EventHandler(this.buttonCancelClassify_Click);
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
            ((System.ComponentModel.ISupportInitialize)(this.numericWords)).EndInit();
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
        private System.Windows.Forms.Label labelPending;
        private System.Windows.Forms.LinkLabel linkRelevance;
        private System.Windows.Forms.Label labelSummary;
        private System.Windows.Forms.Label labelSubject;
        private System.Windows.Forms.TextBox textSubject;
        private System.Windows.Forms.Label labelWords;
        private System.Windows.Forms.NumericUpDown numericWords;
        private System.Windows.Forms.Label labelLanguage;
        private System.Windows.Forms.ComboBox comboLanguage;
        private System.Windows.Forms.Label labelModel;
        private System.Windows.Forms.ComboBox comboModel;
        private System.Windows.Forms.Label labelConcepts;
        private System.Windows.Forms.CheckedListBox checkedConcepts;
        private System.Windows.Forms.Label labelEstimate;
        private System.Windows.Forms.ProgressBar progressSummary;
        private System.Windows.Forms.Label labelProgress;
        private System.Windows.Forms.LinkLabel linkOpenNote;
        private System.Windows.Forms.Button buttonCancelSummary;
        private System.Windows.Forms.Button buttonEstimate;
        private System.Windows.Forms.Button buttonGenerate;
        private System.Windows.Forms.ProgressBar progressClassify;
        private System.Windows.Forms.Button buttonClassify;
        private System.Windows.Forms.Button buttonCancelClassify;
    }
}
