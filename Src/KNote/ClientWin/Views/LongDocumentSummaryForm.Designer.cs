namespace KNote.ClientWin.Views
{
    partial class LongDocumentSummaryForm
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
            labelFile = new Label();
            labelIntro = new Label();
            labelLanguage = new Label();
            comboLanguage = new ComboBox();
            labelLanguageHelp = new Label();
            labelLength = new Label();
            comboLength = new ComboBox();
            labelModel = new Label();
            comboModel = new ComboBox();
            buttonEstimate = new Button();
            labelEstimate = new Label();
            buttonTranscribeFirst = new Button();
            progressRun = new ProgressBar();
            labelProgress = new Label();
            buttonOpenNote = new Button();
            buttonGenerate = new Button();
            buttonCancelRun = new Button();
            buttonClose = new Button();
            SuspendLayout();
            //
            // labelFile
            //
            labelFile.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelFile.AutoEllipsis = true;
            labelFile.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelFile.Location = new Point(12, 10);
            labelFile.Name = "labelFile";
            labelFile.Size = new Size(536, 20);
            labelFile.TabIndex = 0;
            labelFile.Text = "File";
            //
            // labelIntro
            //
            labelIntro.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelIntro.ForeColor = SystemColors.GrayText;
            labelIntro.Location = new Point(12, 32);
            labelIntro.Name = "labelIntro";
            labelIntro.Size = new Size(536, 36);
            labelIntro.TabIndex = 1;
            labelIntro.Text = "Intro";
            //
            // labelLanguage
            //
            labelLanguage.AutoSize = true;
            labelLanguage.Location = new Point(12, 76);
            labelLanguage.Name = "labelLanguage";
            labelLanguage.Size = new Size(59, 15);
            labelLanguage.TabIndex = 2;
            labelLanguage.Text = "Language";
            //
            // comboLanguage
            //
            comboLanguage.Location = new Point(12, 94);
            comboLanguage.Name = "comboLanguage";
            comboLanguage.Size = new Size(260, 23);
            comboLanguage.TabIndex = 3;
            //
            // labelLanguageHelp
            //
            labelLanguageHelp.AutoSize = true;
            labelLanguageHelp.ForeColor = SystemColors.GrayText;
            labelLanguageHelp.Location = new Point(280, 98);
            labelLanguageHelp.Name = "labelLanguageHelp";
            labelLanguageHelp.Size = new Size(40, 15);
            labelLanguageHelp.TabIndex = 4;
            labelLanguageHelp.Text = "Help";
            //
            // labelLength
            //
            labelLength.AutoSize = true;
            labelLength.Location = new Point(12, 126);
            labelLength.Name = "labelLength";
            labelLength.Size = new Size(44, 15);
            labelLength.TabIndex = 5;
            labelLength.Text = "Length";
            //
            // comboLength
            //
            comboLength.DropDownStyle = ComboBoxStyle.DropDownList;
            comboLength.Location = new Point(12, 144);
            comboLength.Name = "comboLength";
            comboLength.Size = new Size(260, 23);
            comboLength.TabIndex = 6;
            //
            // labelModel
            //
            labelModel.AutoSize = true;
            labelModel.Location = new Point(12, 176);
            labelModel.Name = "labelModel";
            labelModel.Size = new Size(41, 15);
            labelModel.TabIndex = 7;
            labelModel.Text = "Model";
            //
            // comboModel
            //
            comboModel.DropDownStyle = ComboBoxStyle.DropDownList;
            comboModel.Location = new Point(12, 194);
            comboModel.Name = "comboModel";
            comboModel.Size = new Size(260, 23);
            comboModel.TabIndex = 8;
            //
            // buttonEstimate
            //
            buttonEstimate.Location = new Point(12, 230);
            buttonEstimate.Name = "buttonEstimate";
            buttonEstimate.Size = new Size(100, 29);
            buttonEstimate.TabIndex = 9;
            buttonEstimate.Text = "Estimate";
            buttonEstimate.UseVisualStyleBackColor = true;
            buttonEstimate.Click += buttonEstimate_Click;
            //
            // labelEstimate
            //
            labelEstimate.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelEstimate.Location = new Point(12, 266);
            labelEstimate.Name = "labelEstimate";
            labelEstimate.Size = new Size(536, 72);
            labelEstimate.TabIndex = 10;
            //
            // buttonTranscribeFirst
            //
            buttonTranscribeFirst.Location = new Point(12, 340);
            buttonTranscribeFirst.Name = "buttonTranscribeFirst";
            buttonTranscribeFirst.Size = new Size(160, 29);
            buttonTranscribeFirst.TabIndex = 11;
            buttonTranscribeFirst.Text = "Transcribe them first";
            buttonTranscribeFirst.UseVisualStyleBackColor = true;
            buttonTranscribeFirst.Visible = false;
            buttonTranscribeFirst.Click += buttonTranscribeFirst_Click;
            //
            // progressRun
            //
            progressRun.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progressRun.Location = new Point(12, 380);
            progressRun.Name = "progressRun";
            progressRun.Size = new Size(536, 8);
            progressRun.TabIndex = 12;
            progressRun.Visible = false;
            //
            // labelProgress
            //
            labelProgress.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelProgress.Location = new Point(12, 394);
            labelProgress.Name = "labelProgress";
            labelProgress.Size = new Size(536, 36);
            labelProgress.TabIndex = 13;
            //
            // buttonOpenNote
            //
            buttonOpenNote.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            buttonOpenNote.Location = new Point(12, 439);
            buttonOpenNote.Name = "buttonOpenNote";
            buttonOpenNote.Size = new Size(120, 29);
            buttonOpenNote.TabIndex = 14;
            buttonOpenNote.Text = "Open the note";
            buttonOpenNote.UseVisualStyleBackColor = true;
            buttonOpenNote.Visible = false;
            buttonOpenNote.Click += buttonOpenNote_Click;
            //
            // buttonGenerate
            //
            buttonGenerate.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonGenerate.Location = new Point(306, 439);
            buttonGenerate.Name = "buttonGenerate";
            buttonGenerate.Size = new Size(90, 29);
            buttonGenerate.TabIndex = 15;
            buttonGenerate.Text = "Generate";
            buttonGenerate.UseVisualStyleBackColor = true;
            buttonGenerate.Click += buttonGenerate_Click;
            //
            // buttonCancelRun
            //
            buttonCancelRun.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancelRun.Location = new Point(402, 439);
            buttonCancelRun.Name = "buttonCancelRun";
            buttonCancelRun.Size = new Size(70, 29);
            buttonCancelRun.TabIndex = 16;
            buttonCancelRun.Text = "Cancel";
            buttonCancelRun.UseVisualStyleBackColor = true;
            buttonCancelRun.Click += buttonCancelRun_Click;
            //
            // buttonClose
            //
            buttonClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonClose.DialogResult = DialogResult.Cancel;
            buttonClose.Location = new Point(478, 439);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new Size(70, 29);
            buttonClose.TabIndex = 17;
            buttonClose.Text = "&Close";
            buttonClose.UseVisualStyleBackColor = true;
            buttonClose.Click += buttonClose_Click;
            //
            // LongDocumentSummaryForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonClose;
            ClientSize = new Size(560, 480);
            Controls.Add(labelFile);
            Controls.Add(labelIntro);
            Controls.Add(labelLanguage);
            Controls.Add(comboLanguage);
            Controls.Add(labelLanguageHelp);
            Controls.Add(labelLength);
            Controls.Add(comboLength);
            Controls.Add(labelModel);
            Controls.Add(comboModel);
            Controls.Add(buttonEstimate);
            Controls.Add(labelEstimate);
            Controls.Add(buttonTranscribeFirst);
            Controls.Add(progressRun);
            Controls.Add(labelProgress);
            Controls.Add(buttonOpenNote);
            Controls.Add(buttonGenerate);
            Controls.Add(buttonCancelRun);
            Controls.Add(buttonClose);
            MinimumSize = new Size(500, 500);
            Name = "LongDocumentSummaryForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Summarize document";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelFile;
        private Label labelIntro;
        private Label labelLanguage;
        private ComboBox comboLanguage;
        private Label labelLanguageHelp;
        private Label labelLength;
        private ComboBox comboLength;
        private Label labelModel;
        private ComboBox comboModel;
        private Button buttonEstimate;
        private Label labelEstimate;
        private Button buttonTranscribeFirst;
        private ProgressBar progressRun;
        private Label labelProgress;
        private Button buttonOpenNote;
        private Button buttonGenerate;
        private Button buttonCancelRun;
        private Button buttonClose;
    }
}
