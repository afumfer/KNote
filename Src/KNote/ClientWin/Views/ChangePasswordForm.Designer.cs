namespace KNote.ClientWin.Views
{
    partial class ChangePasswordForm
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
            labelCurrentPassword = new Label();
            textCurrentPassword = new TextBox();
            labelNewPassword = new Label();
            textNewPassword = new TextBox();
            labelPolicy = new Label();
            labelConfirmPassword = new Label();
            textConfirmPassword = new TextBox();
            SuspendLayout();
            //
            // buttonCancel
            //
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Location = new Point(265, 214);
            buttonCancel.Margin = new Padding(3);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(64, 23);
            buttonCancel.TabIndex = 10;
            buttonCancel.Text = "&Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            //
            // buttonAccept
            //
            buttonAccept.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonAccept.Location = new Point(195, 214);
            buttonAccept.Margin = new Padding(3);
            buttonAccept.Name = "buttonAccept";
            buttonAccept.Size = new Size(64, 23);
            buttonAccept.TabIndex = 9;
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
            // labelCurrentPassword
            //
            labelCurrentPassword.AutoSize = true;
            labelCurrentPassword.Location = new Point(10, 60);
            labelCurrentPassword.Margin = new Padding(3, 0, 3, 0);
            labelCurrentPassword.Name = "labelCurrentPassword";
            labelCurrentPassword.Size = new Size(103, 15);
            labelCurrentPassword.TabIndex = 1;
            labelCurrentPassword.Text = "Current password:";
            //
            // textCurrentPassword
            //
            textCurrentPassword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textCurrentPassword.Location = new Point(10, 77);
            textCurrentPassword.Margin = new Padding(3);
            textCurrentPassword.Name = "textCurrentPassword";
            textCurrentPassword.Size = new Size(318, 19);
            textCurrentPassword.TabIndex = 2;
            textCurrentPassword.UseSystemPasswordChar = true;
            //
            // labelNewPassword
            //
            labelNewPassword.AutoSize = true;
            labelNewPassword.Location = new Point(10, 103);
            labelNewPassword.Margin = new Padding(3, 0, 3, 0);
            labelNewPassword.Name = "labelNewPassword";
            labelNewPassword.Size = new Size(86, 15);
            labelNewPassword.TabIndex = 3;
            labelNewPassword.Text = "New password:";
            //
            // textNewPassword
            //
            textNewPassword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textNewPassword.Location = new Point(10, 120);
            textNewPassword.Margin = new Padding(3);
            textNewPassword.Name = "textNewPassword";
            textNewPassword.Size = new Size(318, 19);
            textNewPassword.TabIndex = 4;
            textNewPassword.UseSystemPasswordChar = true;
            //
            // labelPolicy
            //
            labelPolicy.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelPolicy.ForeColor = SystemColors.GrayText;
            labelPolicy.Location = new Point(10, 142);
            labelPolicy.Margin = new Padding(3, 0, 3, 0);
            labelPolicy.Name = "labelPolicy";
            labelPolicy.Size = new Size(318, 15);
            labelPolicy.TabIndex = 5;
            //
            // labelConfirmPassword
            //
            labelConfirmPassword.AutoSize = true;
            labelConfirmPassword.Location = new Point(10, 163);
            labelConfirmPassword.Margin = new Padding(3, 0, 3, 0);
            labelConfirmPassword.Name = "labelConfirmPassword";
            labelConfirmPassword.Size = new Size(146, 15);
            labelConfirmPassword.TabIndex = 6;
            labelConfirmPassword.Text = "Confirm the new password:";
            //
            // textConfirmPassword
            //
            textConfirmPassword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textConfirmPassword.Location = new Point(10, 180);
            textConfirmPassword.Margin = new Padding(3);
            textConfirmPassword.Name = "textConfirmPassword";
            textConfirmPassword.Size = new Size(318, 19);
            textConfirmPassword.TabIndex = 7;
            textConfirmPassword.UseSystemPasswordChar = true;
            //
            // ChangePasswordForm
            //
            AcceptButton = buttonAccept;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(338, 245);
            Controls.Add(buttonCancel);
            Controls.Add(buttonAccept);
            Controls.Add(labelInfo);
            Controls.Add(labelCurrentPassword);
            Controls.Add(textCurrentPassword);
            Controls.Add(labelNewPassword);
            Controls.Add(textNewPassword);
            Controls.Add(labelPolicy);
            Controls.Add(labelConfirmPassword);
            Controls.Add(textConfirmPassword);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            KeyPreview = true;
            Margin = new Padding(3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ChangePasswordForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Change password";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Label labelCurrentPassword;
        private System.Windows.Forms.TextBox textCurrentPassword;
        private System.Windows.Forms.Label labelNewPassword;
        private System.Windows.Forms.TextBox textNewPassword;
        private System.Windows.Forms.Label labelPolicy;
        private System.Windows.Forms.Label labelConfirmPassword;
        private System.Windows.Forms.TextBox textConfirmPassword;
    }
}
