namespace KNote.ClientWin.Views
{
    partial class DocumentTextForm
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
            labelState = new Label();
            buttonRead = new Button();
            progressLoad = new ProgressBar();
            textDocument = new TextBox();
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
            labelFile.Size = new Size(676, 20);
            labelFile.TabIndex = 0;
            labelFile.Text = "File";
            //
            // labelState
            //
            labelState.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelState.ForeColor = SystemColors.GrayText;
            labelState.Location = new Point(12, 32);
            labelState.Name = "labelState";
            labelState.Size = new Size(560, 34);
            labelState.TabIndex = 1;
            labelState.Text = "State";
            //
            // buttonRead
            //
            buttonRead.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonRead.Location = new Point(578, 34);
            buttonRead.Name = "buttonRead";
            buttonRead.Size = new Size(110, 29);
            buttonRead.TabIndex = 2;
            buttonRead.Text = "Read document";
            buttonRead.UseVisualStyleBackColor = true;
            buttonRead.Click += buttonRead_Click;
            //
            // progressLoad
            //
            progressLoad.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progressLoad.Location = new Point(12, 70);
            progressLoad.Name = "progressLoad";
            progressLoad.Size = new Size(676, 6);
            progressLoad.TabIndex = 3;
            //
            // textDocument
            //
            textDocument.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textDocument.BackColor = SystemColors.Window;
            textDocument.Font = new Font("Segoe UI", 9.75F);
            textDocument.Location = new Point(12, 82);
            textDocument.MaxLength = 0;
            textDocument.Multiline = true;
            textDocument.Name = "textDocument";
            textDocument.ReadOnly = true;
            textDocument.ScrollBars = ScrollBars.Vertical;
            textDocument.Size = new Size(676, 382);
            textDocument.TabIndex = 4;
            textDocument.WordWrap = true;
            //
            // buttonClose
            //
            buttonClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonClose.DialogResult = DialogResult.Cancel;
            buttonClose.Location = new Point(612, 474);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new Size(76, 29);
            buttonClose.TabIndex = 5;
            buttonClose.Text = "&Close";
            buttonClose.UseVisualStyleBackColor = true;
            //
            // DocumentTextForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonClose;
            ClientSize = new Size(700, 515);
            Controls.Add(buttonClose);
            Controls.Add(textDocument);
            Controls.Add(progressLoad);
            Controls.Add(buttonRead);
            Controls.Add(labelState);
            Controls.Add(labelFile);
            MinimizeBox = false;
            MinimumSize = new Size(480, 320);
            Name = "DocumentTextForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Text of the document";
            ResumeLayout(false);
        }

        #endregion

        private Label labelFile;
        private Label labelState;
        private Button buttonRead;
        private ProgressBar progressLoad;
        private TextBox textDocument;
        private Button buttonClose;
    }
}
