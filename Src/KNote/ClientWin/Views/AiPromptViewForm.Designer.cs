namespace KNote.ClientWin.Views
{
    partial class AiPromptViewForm
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
            labelSummary = new Label();
            textPrompt = new TextBox();
            buttonClose = new Button();
            SuspendLayout();
            //
            // labelSummary
            //
            labelSummary.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelSummary.ForeColor = SystemColors.GrayText;
            labelSummary.Location = new Point(12, 10);
            labelSummary.Name = "labelSummary";
            labelSummary.Size = new Size(676, 32);
            labelSummary.TabIndex = 0;
            labelSummary.Text = "Size and layers";
            //
            // textPrompt
            //
            textPrompt.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            textPrompt.BackColor = SystemColors.Window;
            textPrompt.Font = new Font("Consolas", 9.75F);
            textPrompt.Location = new Point(12, 46);
            textPrompt.Multiline = true;
            textPrompt.Name = "textPrompt";
            textPrompt.ReadOnly = true;
            textPrompt.ScrollBars = ScrollBars.Both;
            textPrompt.Size = new Size(676, 418);
            textPrompt.TabIndex = 1;
            textPrompt.WordWrap = true;
            //
            // buttonClose
            //
            buttonClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonClose.DialogResult = DialogResult.Cancel;
            buttonClose.Location = new Point(612, 474);
            buttonClose.Name = "buttonClose";
            buttonClose.Size = new Size(76, 29);
            buttonClose.TabIndex = 2;
            buttonClose.Text = "&Close";
            buttonClose.UseVisualStyleBackColor = true;
            //
            // AiPromptViewForm
            //
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonClose;
            ClientSize = new Size(700, 515);
            Controls.Add(buttonClose);
            Controls.Add(textPrompt);
            Controls.Add(labelSummary);
            MinimizeBox = false;
            MinimumSize = new Size(480, 320);
            Name = "AiPromptViewForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "What the AI receives";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label labelSummary;
        private TextBox textPrompt;
        private Button buttonClose;
    }
}
