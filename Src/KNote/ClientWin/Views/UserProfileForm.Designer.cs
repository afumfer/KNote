namespace KNote.ClientWin.Views
{
    partial class UserProfileForm
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
            buttonCancel = new Button();
            buttonAccept = new Button();
            labelInfo = new Label();
            labelUserName = new Label();
            textUserName = new TextBox();
            labelFullName = new Label();
            textFullName = new TextBox();
            labelEMail = new Label();
            textEMail = new TextBox();
            labelCurrentPassword = new Label();
            textCurrentPassword = new TextBox();
            checkApplyToAll = new CheckBox();
            SuspendLayout();
            //
            // buttonCancel
            //
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Location = new Point(265, 263);
            buttonCancel.Margin = new Padding(3);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(64, 23);
            buttonCancel.TabIndex = 12;
            buttonCancel.Text = "&Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            //
            // buttonAccept
            //
            buttonAccept.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonAccept.Location = new Point(195, 263);
            buttonAccept.Margin = new Padding(3);
            buttonAccept.Name = "buttonAccept";
            buttonAccept.Size = new Size(64, 23);
            buttonAccept.TabIndex = 11;
            buttonAccept.Text = "&Accept";
            buttonAccept.UseVisualStyleBackColor = true;
            buttonAccept.Click += buttonAccept_Click;
            //
            // labelInfo
            //
            labelInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelInfo.Location = new Point(10, 7);
            labelInfo.Margin = new Padding(3, 0, 3, 0);
            labelInfo.Name = "labelInfo";
            labelInfo.Size = new Size(318, 45);
            labelInfo.TabIndex = 0;
            //
            // labelUserName
            //
            labelUserName.AutoSize = true;
            labelUserName.Location = new Point(10, 60);
            labelUserName.Margin = new Padding(3, 0, 3, 0);
            labelUserName.Name = "labelUserName";
            labelUserName.Size = new Size(67, 15);
            labelUserName.TabIndex = 1;
            labelUserName.Text = "User name:";
            //
            // textUserName
            //
            textUserName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textUserName.Location = new Point(10, 77);
            textUserName.Margin = new Padding(3);
            textUserName.Name = "textUserName";
            textUserName.ReadOnly = true;
            textUserName.Size = new Size(318, 19);
            textUserName.TabIndex = 2;
            textUserName.TabStop = false;
            //
            // labelFullName
            //
            labelFullName.AutoSize = true;
            labelFullName.Location = new Point(10, 103);
            labelFullName.Margin = new Padding(3, 0, 3, 0);
            labelFullName.Name = "labelFullName";
            labelFullName.Size = new Size(62, 15);
            labelFullName.TabIndex = 3;
            labelFullName.Text = "Full name:";
            //
            // textFullName
            //
            textFullName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textFullName.Location = new Point(10, 120);
            textFullName.Margin = new Padding(3);
            textFullName.Name = "textFullName";
            textFullName.Size = new Size(318, 19);
            textFullName.TabIndex = 4;
            //
            // labelEMail
            //
            labelEMail.AutoSize = true;
            labelEMail.Location = new Point(10, 146);
            labelEMail.Margin = new Padding(3, 0, 3, 0);
            labelEMail.Name = "labelEMail";
            labelEMail.Size = new Size(41, 15);
            labelEMail.TabIndex = 5;
            labelEMail.Text = "Email:";
            //
            // textEMail
            //
            textEMail.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textEMail.Location = new Point(10, 163);
            textEMail.Margin = new Padding(3);
            textEMail.Name = "textEMail";
            textEMail.Size = new Size(318, 19);
            textEMail.TabIndex = 6;
            textEMail.TextChanged += textEMail_TextChanged;
            //
            // labelCurrentPassword
            //
            labelCurrentPassword.AutoSize = true;
            labelCurrentPassword.Location = new Point(10, 190);
            labelCurrentPassword.Margin = new Padding(3, 0, 3, 0);
            labelCurrentPassword.Name = "labelCurrentPassword";
            labelCurrentPassword.Size = new Size(236, 15);
            labelCurrentPassword.TabIndex = 7;
            labelCurrentPassword.Text = "Current password (to change the email):";
            //
            // textCurrentPassword
            //
            textCurrentPassword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textCurrentPassword.Enabled = false;
            textCurrentPassword.Location = new Point(10, 206);
            textCurrentPassword.Margin = new Padding(3);
            textCurrentPassword.Name = "textCurrentPassword";
            textCurrentPassword.Size = new Size(318, 19);
            textCurrentPassword.TabIndex = 8;
            textCurrentPassword.UseSystemPasswordChar = true;
            //
            // checkApplyToAll
            //
            checkApplyToAll.AutoSize = true;
            checkApplyToAll.Location = new Point(10, 234);
            checkApplyToAll.Margin = new Padding(3);
            checkApplyToAll.Name = "checkApplyToAll";
            checkApplyToAll.Size = new Size(230, 19);
            checkApplyToAll.TabIndex = 9;
            checkApplyToAll.Text = "Save it in every linked repository";
            checkApplyToAll.UseVisualStyleBackColor = true;
            //
            // UserProfileForm
            //
            AcceptButton = buttonAccept;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(338, 294);
            Controls.Add(buttonCancel);
            Controls.Add(buttonAccept);
            Controls.Add(labelInfo);
            Controls.Add(labelUserName);
            Controls.Add(textUserName);
            Controls.Add(labelFullName);
            Controls.Add(textFullName);
            Controls.Add(labelEMail);
            Controls.Add(textEMail);
            Controls.Add(labelCurrentPassword);
            Controls.Add(textCurrentPassword);
            Controls.Add(checkApplyToAll);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            KeyPreview = true;
            Margin = new Padding(3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "UserProfileForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "My profile";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Label labelUserName;
        private System.Windows.Forms.TextBox textUserName;
        private System.Windows.Forms.Label labelFullName;
        private System.Windows.Forms.TextBox textFullName;
        private System.Windows.Forms.Label labelEMail;
        private System.Windows.Forms.TextBox textEMail;
        private System.Windows.Forms.Label labelCurrentPassword;
        private System.Windows.Forms.TextBox textCurrentPassword;
        private System.Windows.Forms.CheckBox checkApplyToAll;
    }
}
