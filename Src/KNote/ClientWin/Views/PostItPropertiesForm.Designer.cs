
namespace KNote.ClientWin.Views
{
    partial class PostItPropertiesForm
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
            colorDialog = new ColorDialog();
            fontDialog = new FontDialog();
            buttonCancel = new Button();
            buttonAccept = new Button();
            panelForm = new Panel();
            buttonCaptionTextColor = new Button();
            buttonNoteFont = new Button();
            buttonNoteColor = new Button();
            buttonLightGray = new Button();
            buttonDark = new Button();
            labelText = new Label();
            labelNote = new Label();
            label2 = new Label();
            buttonCaptionColor = new Button();
            labelCaption = new Label();
            buttonYellow = new Button();
            label1 = new Label();
            panelForm.SuspendLayout();
            SuspendLayout();
            // 
            // fontDialog
            // 
            fontDialog.ShowColor = true;
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Location = new Point(501, 249);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(81, 29);
            buttonCancel.TabIndex = 8;
            buttonCancel.Text = "&Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // buttonAccept
            // 
            buttonAccept.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonAccept.Location = new Point(418, 249);
            buttonAccept.Name = "buttonAccept";
            buttonAccept.Size = new Size(81, 29);
            buttonAccept.TabIndex = 7;
            buttonAccept.Text = "&Accept";
            buttonAccept.UseVisualStyleBackColor = true;
            buttonAccept.Click += buttonAccept_Click;
            // 
            // panelForm
            // 
            panelForm.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelForm.BackColor = SystemColors.Window;
            panelForm.BorderStyle = BorderStyle.FixedSingle;
            panelForm.Controls.Add(buttonCaptionTextColor);
            panelForm.Controls.Add(buttonNoteFont);
            panelForm.Controls.Add(buttonNoteColor);
            panelForm.Controls.Add(buttonLightGray);
            panelForm.Controls.Add(buttonDark);
            panelForm.Controls.Add(labelText);
            panelForm.Controls.Add(labelNote);
            panelForm.Controls.Add(label2);
            panelForm.Controls.Add(buttonCaptionColor);
            panelForm.Controls.Add(labelCaption);
            panelForm.Controls.Add(buttonYellow);
            panelForm.Controls.Add(label1);
            panelForm.Location = new Point(3, 6);
            panelForm.Name = "panelForm";
            panelForm.Size = new Size(579, 227);
            panelForm.TabIndex = 8;
            // 
            // buttonCaptionTextColor
            // 
            buttonCaptionTextColor.Location = new Point(399, 69);
            buttonCaptionTextColor.Name = "buttonCaptionTextColor";
            buttonCaptionTextColor.Size = new Size(171, 26);
            buttonCaptionTextColor.TabIndex = 4;
            buttonCaptionTextColor.Text = "Change caption text color ...";
            buttonCaptionTextColor.UseVisualStyleBackColor = true;
            buttonCaptionTextColor.Click += buttonStyle_Click;
            // 
            // buttonNoteFont
            // 
            buttonNoteFont.Location = new Point(399, 180);
            buttonNoteFont.Name = "buttonNoteFont";
            buttonNoteFont.Size = new Size(171, 26);
            buttonNoteFont.TabIndex = 6;
            buttonNoteFont.Text = "Change note font ...";
            buttonNoteFont.UseVisualStyleBackColor = true;
            buttonNoteFont.Click += buttonStyle_Click;
            // 
            // buttonNoteColor
            // 
            buttonNoteColor.Location = new Point(399, 148);
            buttonNoteColor.Name = "buttonNoteColor";
            buttonNoteColor.Size = new Size(171, 26);
            buttonNoteColor.TabIndex = 5;
            buttonNoteColor.Text = "Change note color ...";
            buttonNoteColor.UseVisualStyleBackColor = true;
            buttonNoteColor.Click += buttonStyle_Click;
            // 
            // buttonLightGray
            // 
            buttonLightGray.Location = new Point(9, 100);
            buttonLightGray.Name = "buttonLightGray";
            buttonLightGray.Size = new Size(139, 26);
            buttonLightGray.TabIndex = 2;
            buttonLightGray.Text = "Light gray style";
            buttonLightGray.UseVisualStyleBackColor = true;
            buttonLightGray.Click += buttonStyle_Click;
            // 
            // buttonDark
            // 
            buttonDark.Location = new Point(9, 68);
            buttonDark.Name = "buttonDark";
            buttonDark.Size = new Size(139, 26);
            buttonDark.TabIndex = 1;
            buttonDark.Text = "Dark style ";
            buttonDark.UseVisualStyleBackColor = true;
            buttonDark.Click += buttonStyle_Click;
            // 
            // labelText
            // 
            labelText.BackColor = Color.LemonChiffon;
            labelText.Location = new Point(188, 52);
            labelText.Name = "labelText";
            labelText.Size = new Size(168, 134);
            labelText.TabIndex = 6;
            labelText.Text = "...";
            // 
            // labelNote
            // 
            labelNote.BackColor = Color.LemonChiffon;
            labelNote.Location = new Point(171, 37);
            labelNote.Name = "labelNote";
            labelNote.Size = new Size(202, 169);
            labelNote.TabIndex = 5;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(399, 17);
            label2.Name = "label2";
            label2.Size = new Size(93, 15);
            label2.TabIndex = 4;
            label2.Text = "Customize style:";
            // 
            // buttonCaptionColor
            // 
            buttonCaptionColor.Location = new Point(399, 37);
            buttonCaptionColor.Name = "buttonCaptionColor";
            buttonCaptionColor.Size = new Size(171, 26);
            buttonCaptionColor.TabIndex = 3;
            buttonCaptionColor.Text = "Change caption color ...";
            buttonCaptionColor.UseVisualStyleBackColor = true;
            buttonCaptionColor.Click += buttonStyle_Click;
            // 
            // labelCaption
            // 
            labelCaption.BackColor = Color.Khaki;
            labelCaption.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelCaption.Location = new Point(171, 17);
            labelCaption.Name = "labelCaption";
            labelCaption.Padding = new Padding(2, 2, 0, 0);
            labelCaption.Size = new Size(202, 20);
            labelCaption.TabIndex = 2;
            labelCaption.Text = "Caption text ...";
            // 
            // buttonYellow
            // 
            buttonYellow.Location = new Point(9, 36);
            buttonYellow.Name = "buttonYellow";
            buttonYellow.Size = new Size(139, 26);
            buttonYellow.TabIndex = 0;
            buttonYellow.Text = "Yellow style ";
            buttonYellow.UseVisualStyleBackColor = true;
            buttonYellow.Click += buttonStyle_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(9, 17);
            label1.Name = "label1";
            label1.Size = new Size(99, 15);
            label1.TabIndex = 0;
            label1.Text = "Predefined styles:";
            // 
            // PostItPropertiesForm
            // 
            AcceptButton = buttonAccept;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(586, 291);
            Controls.Add(buttonCancel);
            Controls.Add(buttonAccept);
            Controls.Add(panelForm);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "PostItPropertiesForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "PostIt properties";
            FormClosing += PostItPropertiesForm_FormClosing;
            Load += PostItPropertiesForm_Load;
            panelForm.ResumeLayout(false);
            panelForm.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ColorDialog colorDialog;
        private System.Windows.Forms.FontDialog fontDialog;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.Label labelCaption;
        private System.Windows.Forms.Button buttonYellow;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonNoteFont;
        private System.Windows.Forms.Button buttonNoteColor;
        private System.Windows.Forms.Button buttonLightGray;
        private System.Windows.Forms.Button buttonDark;
        private System.Windows.Forms.Label labelText;
        private System.Windows.Forms.Label labelNote;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonCaptionColor;
        private System.Windows.Forms.Button buttonCaptionTextColor;
    }
}