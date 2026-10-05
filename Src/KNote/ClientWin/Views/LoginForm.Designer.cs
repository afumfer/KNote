namespace KNote.ClientWin.Views
{
    partial class LoginForm
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
            labelPassword = new Label();
            textPassword = new TextBox();
            linkWindowsAccount = new LinkLabel();
            panelSignIn = new Panel();
            pictureBoxSignIn = new PictureBox();
            panelSignIn.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxSignIn).BeginInit();
            SuspendLayout();
            //
            // buttonCancel
            //
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Location = new Point(456, 186);
            buttonCancel.Margin = new Padding(3);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(64, 23);
            buttonCancel.TabIndex = 7;
            buttonCancel.Text = "&Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            //
            // buttonAccept
            //
            buttonAccept.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonAccept.Location = new Point(386, 186);
            buttonAccept.Margin = new Padding(3);
            buttonAccept.Name = "buttonAccept";
            buttonAccept.Size = new Size(64, 23);
            buttonAccept.TabIndex = 6;
            buttonAccept.Text = "&Accept";
            buttonAccept.UseVisualStyleBackColor = true;
            buttonAccept.Click += buttonAccept_Click;
            //
            // labelInfo
            //
            labelInfo.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelInfo.Location = new Point(118, 14);
            labelInfo.Margin = new Padding(3, 0, 3, 0);
            labelInfo.Name = "labelInfo";
            labelInfo.Size = new Size(400, 32);
            labelInfo.TabIndex = 0;
            labelInfo.Text = "Enter your KNote user name and password. They will be checked in each of your repositories.";
            //
            // labelUserName
            //
            labelUserName.AutoSize = true;
            labelUserName.Location = new Point(118, 54);
            labelUserName.Margin = new Padding(3, 0, 3, 0);
            labelUserName.Name = "labelUserName";
            labelUserName.Size = new Size(67, 15);
            labelUserName.TabIndex = 1;
            labelUserName.Text = "User name:";
            //
            // textUserName
            //
            textUserName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textUserName.Location = new Point(118, 71);
            textUserName.Margin = new Padding(3);
            textUserName.MaxLength = 32;
            textUserName.Name = "textUserName";
            textUserName.Size = new Size(400, 23);
            textUserName.TabIndex = 2;
            //
            // labelPassword
            //
            labelPassword.AutoSize = true;
            labelPassword.Location = new Point(118, 101);
            labelPassword.Margin = new Padding(3, 0, 3, 0);
            labelPassword.Name = "labelPassword";
            labelPassword.Size = new Size(60, 15);
            labelPassword.TabIndex = 3;
            labelPassword.Text = "Password:";
            //
            // textPassword
            //
            textPassword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textPassword.Location = new Point(118, 118);
            textPassword.Margin = new Padding(3);
            textPassword.Name = "textPassword";
            textPassword.Size = new Size(400, 23);
            textPassword.TabIndex = 4;
            textPassword.UseSystemPasswordChar = true;
            //
            // linkWindowsAccount
            //
            linkWindowsAccount.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            linkWindowsAccount.Location = new Point(118, 150);
            linkWindowsAccount.Margin = new Padding(3, 0, 3, 0);
            linkWindowsAccount.Name = "linkWindowsAccount";
            linkWindowsAccount.Size = new Size(400, 18);
            linkWindowsAccount.TabIndex = 5;
            linkWindowsAccount.TabStop = true;
            linkWindowsAccount.Text = "Use my Windows account instead";
            linkWindowsAccount.LinkClicked += linkWindowsAccount_LinkClicked;
            //
            // panelSignIn
            //
            panelSignIn.BackColor = SystemColors.ControlDarkDark;
            panelSignIn.Controls.Add(pictureBoxSignIn);
            panelSignIn.Dock = DockStyle.Left;
            panelSignIn.Location = new Point(0, 0);
            panelSignIn.Margin = new Padding(0);
            panelSignIn.Name = "panelSignIn";
            panelSignIn.Size = new Size(104, 217);
            panelSignIn.TabIndex = 8;
            //
            // pictureBoxSignIn
            //
            pictureBoxSignIn.BackColor = SystemColors.ControlDarkDark;
            pictureBoxSignIn.Dock = DockStyle.Fill;
            pictureBoxSignIn.Location = new Point(0, 0);
            pictureBoxSignIn.Margin = new Padding(0);
            pictureBoxSignIn.Name = "pictureBoxSignIn";
            pictureBoxSignIn.Size = new Size(104, 217);
            pictureBoxSignIn.TabIndex = 0;
            pictureBoxSignIn.TabStop = false;
            //
            // LoginForm
            //
            AcceptButton = buttonAccept;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(530, 217);
            Controls.Add(buttonCancel);
            Controls.Add(buttonAccept);
            Controls.Add(labelInfo);
            Controls.Add(labelUserName);
            Controls.Add(textUserName);
            Controls.Add(labelPassword);
            Controls.Add(textPassword);
            Controls.Add(linkWindowsAccount);
            Controls.Add(panelSignIn);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            KeyPreview = true;
            Margin = new Padding(3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "LoginForm";
            ShowInTaskbar = true;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Sign in";
            panelSignIn.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxSignIn).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Label labelUserName;
        private System.Windows.Forms.TextBox textUserName;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.TextBox textPassword;
        private System.Windows.Forms.LinkLabel linkWindowsAccount;
        private System.Windows.Forms.Panel panelSignIn;
        private System.Windows.Forms.PictureBox pictureBoxSignIn;
    }
}
