
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
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.fontDialog = new System.Windows.Forms.FontDialog();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.panelForm = new System.Windows.Forms.Panel();
            this.buttonCaptionTextColor = new System.Windows.Forms.Button();
            this.buttonNoteFont = new System.Windows.Forms.Button();
            this.buttonNoteColor = new System.Windows.Forms.Button();
            this.buttonLightGray = new System.Windows.Forms.Button();
            this.buttonDark = new System.Windows.Forms.Button();
            this.labelText = new System.Windows.Forms.Label();
            this.labelNote = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonCaptionColor = new System.Windows.Forms.Button();
            this.labelCaption = new System.Windows.Forms.Label();
            this.buttonYellow = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.panelForm.SuspendLayout();
            this.SuspendLayout();
            // 
            // fontDialog
            // 
            this.fontDialog.ShowColor = true;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(501, 249);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(81, 29);
            this.buttonCancel.TabIndex = 8;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonAccept
            // 
            this.buttonAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAccept.Location = new System.Drawing.Point(418, 249);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(81, 29);
            this.buttonAccept.TabIndex = 7;
            this.buttonAccept.Text = "&Accept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.buttonAccept_Click);
            // 
            // panelForm
            // 
            this.panelForm.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelForm.BackColor = System.Drawing.SystemColors.Window;
            this.panelForm.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelForm.Controls.Add(this.buttonCaptionTextColor);
            this.panelForm.Controls.Add(this.buttonNoteFont);
            this.panelForm.Controls.Add(this.buttonNoteColor);
            this.panelForm.Controls.Add(this.buttonLightGray);
            this.panelForm.Controls.Add(this.buttonDark);
            this.panelForm.Controls.Add(this.labelText);
            this.panelForm.Controls.Add(this.labelNote);
            this.panelForm.Controls.Add(this.label2);
            this.panelForm.Controls.Add(this.buttonCaptionColor);
            this.panelForm.Controls.Add(this.labelCaption);
            this.panelForm.Controls.Add(this.buttonYellow);
            this.panelForm.Controls.Add(this.label1);
            this.panelForm.Location = new System.Drawing.Point(3, 6);
            this.panelForm.Name = "panelForm";
            this.panelForm.Size = new System.Drawing.Size(579, 227);
            this.panelForm.TabIndex = 8;
            // 
            // buttonCaptionTextColor
            // 
            this.buttonCaptionTextColor.Location = new System.Drawing.Point(399, 69);
            this.buttonCaptionTextColor.Name = "buttonCaptionTextColor";
            this.buttonCaptionTextColor.Size = new System.Drawing.Size(171, 26);
            this.buttonCaptionTextColor.TabIndex = 4;
            this.buttonCaptionTextColor.Text = "Change caption text color ...";
            this.buttonCaptionTextColor.UseVisualStyleBackColor = true;
            this.buttonCaptionTextColor.Click += new System.EventHandler(this.buttonStyle_Click);
            // 
            // buttonNoteFont
            // 
            this.buttonNoteFont.Location = new System.Drawing.Point(399, 180);
            this.buttonNoteFont.Name = "buttonNoteFont";
            this.buttonNoteFont.Size = new System.Drawing.Size(171, 26);
            this.buttonNoteFont.TabIndex = 6;
            this.buttonNoteFont.Text = "Change note font ...";
            this.buttonNoteFont.UseVisualStyleBackColor = true;
            this.buttonNoteFont.Click += new System.EventHandler(this.buttonStyle_Click);
            // 
            // buttonNoteColor
            // 
            this.buttonNoteColor.Location = new System.Drawing.Point(399, 148);
            this.buttonNoteColor.Name = "buttonNoteColor";
            this.buttonNoteColor.Size = new System.Drawing.Size(171, 26);
            this.buttonNoteColor.TabIndex = 5;
            this.buttonNoteColor.Text = "Change note color ...";
            this.buttonNoteColor.UseVisualStyleBackColor = true;
            this.buttonNoteColor.Click += new System.EventHandler(this.buttonStyle_Click);
            // 
            // buttonLightGray
            // 
            this.buttonLightGray.Location = new System.Drawing.Point(9, 100);
            this.buttonLightGray.Name = "buttonLightGray";
            this.buttonLightGray.Size = new System.Drawing.Size(139, 26);
            this.buttonLightGray.TabIndex = 2;
            this.buttonLightGray.Text = "Light gray style";
            this.buttonLightGray.UseVisualStyleBackColor = true;
            this.buttonLightGray.Click += new System.EventHandler(this.buttonStyle_Click);
            // 
            // buttonDark
            // 
            this.buttonDark.Location = new System.Drawing.Point(9, 68);
            this.buttonDark.Name = "buttonDark";
            this.buttonDark.Size = new System.Drawing.Size(139, 26);
            this.buttonDark.TabIndex = 1;
            this.buttonDark.Text = "Dark style ";
            this.buttonDark.UseVisualStyleBackColor = true;
            this.buttonDark.Click += new System.EventHandler(this.buttonStyle_Click);
            // 
            // labelText
            // 
            this.labelText.BackColor = System.Drawing.Color.LemonChiffon;
            this.labelText.Location = new System.Drawing.Point(188, 52);
            this.labelText.Name = "labelText";
            this.labelText.Size = new System.Drawing.Size(168, 134);
            this.labelText.TabIndex = 6;
            this.labelText.Text = "...";
            // 
            // labelNote
            // 
            this.labelNote.BackColor = System.Drawing.Color.LemonChiffon;
            this.labelNote.Location = new System.Drawing.Point(171, 37);
            this.labelNote.Name = "labelNote";
            this.labelNote.Size = new System.Drawing.Size(202, 169);
            this.labelNote.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(399, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(93, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "Customize style:";
            // 
            // buttonCaptionColor
            // 
            this.buttonCaptionColor.Location = new System.Drawing.Point(399, 37);
            this.buttonCaptionColor.Name = "buttonCaptionColor";
            this.buttonCaptionColor.Size = new System.Drawing.Size(171, 26);
            this.buttonCaptionColor.TabIndex = 3;
            this.buttonCaptionColor.Text = "Change caption color ...";
            this.buttonCaptionColor.UseVisualStyleBackColor = true;
            this.buttonCaptionColor.Click += new System.EventHandler(this.buttonStyle_Click);
            // 
            // labelCaption
            // 
            this.labelCaption.BackColor = System.Drawing.Color.Khaki;
            this.labelCaption.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.labelCaption.Location = new System.Drawing.Point(171, 17);
            this.labelCaption.Name = "labelCaption";
            this.labelCaption.Padding = new System.Windows.Forms.Padding(2, 2, 0, 0);
            this.labelCaption.Size = new System.Drawing.Size(202, 20);
            this.labelCaption.TabIndex = 2;
            this.labelCaption.Text = "Caption text ...";
            // 
            // buttonYellow
            // 
            this.buttonYellow.Location = new System.Drawing.Point(9, 36);
            this.buttonYellow.Name = "buttonYellow";
            this.buttonYellow.Size = new System.Drawing.Size(139, 26);
            this.buttonYellow.TabIndex = 0;
            this.buttonYellow.Text = "Yellow style ";
            this.buttonYellow.UseVisualStyleBackColor = true;
            this.buttonYellow.Click += new System.EventHandler(this.buttonStyle_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Predefined styles:";
            // 
            // PostItPropertiesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(586, 291);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonAccept);
            this.Controls.Add(this.panelForm);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PostItPropertiesForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "PostIt properties";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PostItPropertiesForm_FormClosing);
            this.Load += new System.EventHandler(this.PostItPropertiesForm_Load);
            this.panelForm.ResumeLayout(false);
            this.panelForm.PerformLayout();
            this.ResumeLayout(false);

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