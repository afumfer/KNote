
namespace KNote.ClientWin.Views
{
    partial class UserRegisterForm
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
            labelPassword = new Label();
            textPassword = new TextBox();
            SuspendLayout();
            //
            // buttonCancel
            //
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Location = new Point(378, 391);
            buttonCancel.Margin = new Padding(4, 5, 4, 5);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(91, 38);
            buttonCancel.TabIndex = 10;
            buttonCancel.Text = "&Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            //
            // buttonAccept
            //
            buttonAccept.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonAccept.Location = new Point(279, 391);
            buttonAccept.Margin = new Padding(4, 5, 4, 5);
            buttonAccept.Name = "buttonAccept";
            buttonAccept.Size = new Size(91, 38);
            buttonAccept.TabIndex = 9;
            buttonAccept.Text = "&Accept";
            buttonAccept.UseVisualStyleBackColor = true;
            buttonAccept.Click += buttonAccept_Click;
            //
            // labelInfo
            //
            labelInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelInfo.Location = new Point(14, 12);
            labelInfo.Margin = new Padding(4, 0, 4, 0);
            labelInfo.Name = "labelInfo";
            labelInfo.Size = new Size(455, 75);
            labelInfo.TabIndex = 0;
            //
            // labelUserName
            //
            labelUserName.AutoSize = true;
            labelUserName.Location = new Point(14, 100);
            labelUserName.Margin = new Padding(4, 0, 4, 0);
            labelUserName.Name = "labelUserName";
            labelUserName.Size = new Size(96, 25);
            labelUserName.TabIndex = 1;
            labelUserName.Text = "User name:";
            //
            // textUserName
            //
            textUserName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textUserName.Location = new Point(14, 128);
            textUserName.Margin = new Padding(4, 5, 4, 5);
            textUserName.Name = "textUserName";
            textUserName.ReadOnly = true;
            textUserName.Size = new Size(455, 31);
            textUserName.TabIndex = 2;
            //
            // labelFullName
            //
            labelFullName.AutoSize = true;
            labelFullName.Location = new Point(14, 172);
            labelFullName.Margin = new Padding(4, 0, 4, 0);
            labelFullName.Name = "labelFullName";
            labelFullName.Size = new Size(89, 25);
            labelFullName.TabIndex = 3;
            labelFullName.Text = "Full name:";
            //
            // textFullName
            //
            textFullName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textFullName.Location = new Point(14, 200);
            textFullName.Margin = new Padding(4, 5, 4, 5);
            textFullName.Name = "textFullName";
            textFullName.Size = new Size(455, 31);
            textFullName.TabIndex = 4;
            //
            // labelEMail
            //
            labelEMail.AutoSize = true;
            labelEMail.Location = new Point(14, 244);
            labelEMail.Margin = new Padding(4, 0, 4, 0);
            labelEMail.Name = "labelEMail";
            labelEMail.Size = new Size(58, 25);
            labelEMail.TabIndex = 5;
            labelEMail.Text = "Email:";
            //
            // textEMail
            //
            textEMail.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textEMail.Location = new Point(14, 272);
            textEMail.Margin = new Padding(4, 5, 4, 5);
            textEMail.Name = "textEMail";
            textEMail.Size = new Size(455, 31);
            textEMail.TabIndex = 6;
            //
            // labelPassword
            //
            labelPassword.AutoSize = true;
            labelPassword.Location = new Point(14, 316);
            labelPassword.Margin = new Padding(4, 0, 4, 0);
            labelPassword.Name = "labelPassword";
            labelPassword.Size = new Size(91, 25);
            labelPassword.TabIndex = 7;
            labelPassword.Text = "Password:";
            //
            // textPassword
            //
            textPassword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textPassword.Location = new Point(14, 344);
            textPassword.Margin = new Padding(4, 5, 4, 5);
            textPassword.Name = "textPassword";
            textPassword.Size = new Size(455, 31);
            textPassword.TabIndex = 8;
            textPassword.UseSystemPasswordChar = true;
            //
            // UserRegisterForm
            //
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(483, 443);
            Controls.Add(buttonCancel);
            Controls.Add(buttonAccept);
            Controls.Add(labelInfo);
            Controls.Add(labelUserName);
            Controls.Add(textUserName);
            Controls.Add(labelFullName);
            Controls.Add(textFullName);
            Controls.Add(labelEMail);
            Controls.Add(textEMail);
            Controls.Add(labelPassword);
            Controls.Add(textPassword);
            KeyPreview = true;
            Margin = new Padding(4, 5, 4, 5);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "UserRegisterForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Register user";
            FormClosing += UserRegisterForm_FormClosing;
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
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.TextBox textPassword;
    }
}
