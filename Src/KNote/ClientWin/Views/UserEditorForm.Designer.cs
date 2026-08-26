
namespace KNote.ClientWin.Views
{
    partial class UserEditorForm
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
            this.buttonResetPassword = new System.Windows.Forms.Button();
            this.textPassword = new System.Windows.Forms.TextBox();
            this.labelPassword = new System.Windows.Forms.Label();
            this.checkAdmin = new System.Windows.Forms.CheckBox();
            this.checkProjectManager = new System.Windows.Forms.CheckBox();
            this.checkStaff = new System.Windows.Forms.CheckBox();
            this.checkPublic = new System.Windows.Forms.CheckBox();
            this.labelRoles = new System.Windows.Forms.Label();
            this.textFullName = new System.Windows.Forms.TextBox();
            this.labelFullName = new System.Windows.Forms.Label();
            this.textEMail = new System.Windows.Forms.TextBox();
            this.labelEMail = new System.Windows.Forms.Label();
            this.textUserName = new System.Windows.Forms.TextBox();
            this.labelUserName = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.panelForm.SuspendLayout();
            this.SuspendLayout();
            //
            // panelForm
            //
            this.panelForm.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelForm.Controls.Add(this.buttonResetPassword);
            this.panelForm.Controls.Add(this.textPassword);
            this.panelForm.Controls.Add(this.labelPassword);
            this.panelForm.Controls.Add(this.checkAdmin);
            this.panelForm.Controls.Add(this.checkProjectManager);
            this.panelForm.Controls.Add(this.checkStaff);
            this.panelForm.Controls.Add(this.checkPublic);
            this.panelForm.Controls.Add(this.labelRoles);
            this.panelForm.Controls.Add(this.textFullName);
            this.panelForm.Controls.Add(this.labelFullName);
            this.panelForm.Controls.Add(this.textEMail);
            this.panelForm.Controls.Add(this.labelEMail);
            this.panelForm.Controls.Add(this.textUserName);
            this.panelForm.Controls.Add(this.labelUserName);
            this.panelForm.Location = new System.Drawing.Point(5, 12);
            this.panelForm.Name = "panelForm";
            this.panelForm.Size = new System.Drawing.Size(470, 345);
            this.panelForm.TabIndex = 0;
            //
            // buttonResetPassword
            //
            this.buttonResetPassword.Location = new System.Drawing.Point(320, 303);
            this.buttonResetPassword.Name = "buttonResetPassword";
            this.buttonResetPassword.Size = new System.Drawing.Size(139, 26);
            this.buttonResetPassword.TabIndex = 13;
            this.buttonResetPassword.Text = "Reset password";
            this.buttonResetPassword.UseVisualStyleBackColor = true;
            this.buttonResetPassword.Click += new System.EventHandler(this.buttonResetPassword_Click);
            //
            // textPassword
            //
            this.textPassword.Location = new System.Drawing.Point(9, 305);
            this.textPassword.Name = "textPassword";
            this.textPassword.PasswordChar = '*';
            this.textPassword.Size = new System.Drawing.Size(300, 23);
            this.textPassword.TabIndex = 12;
            //
            // labelPassword
            //
            this.labelPassword.AutoSize = true;
            this.labelPassword.Location = new System.Drawing.Point(9, 285);
            this.labelPassword.Name = "labelPassword";
            this.labelPassword.Size = new System.Drawing.Size(66, 15);
            this.labelPassword.TabIndex = 11;
            this.labelPassword.Text = "Password:";
            //
            // checkAdmin
            //
            this.checkAdmin.AutoSize = true;
            this.checkAdmin.Location = new System.Drawing.Point(20, 254);
            this.checkAdmin.Name = "checkAdmin";
            this.checkAdmin.Size = new System.Drawing.Size(60, 19);
            this.checkAdmin.TabIndex = 10;
            this.checkAdmin.Text = "Admin";
            this.checkAdmin.UseVisualStyleBackColor = true;
            //
            // checkProjectManager
            //
            this.checkProjectManager.AutoSize = true;
            this.checkProjectManager.Location = new System.Drawing.Point(20, 231);
            this.checkProjectManager.Name = "checkProjectManager";
            this.checkProjectManager.Size = new System.Drawing.Size(122, 19);
            this.checkProjectManager.TabIndex = 9;
            this.checkProjectManager.Text = "Project manager";
            this.checkProjectManager.UseVisualStyleBackColor = true;
            //
            // checkStaff
            //
            this.checkStaff.AutoSize = true;
            this.checkStaff.Location = new System.Drawing.Point(20, 208);
            this.checkStaff.Name = "checkStaff";
            this.checkStaff.Size = new System.Drawing.Size(51, 19);
            this.checkStaff.TabIndex = 8;
            this.checkStaff.Text = "Staff";
            this.checkStaff.UseVisualStyleBackColor = true;
            //
            // checkPublic
            //
            this.checkPublic.AutoSize = true;
            this.checkPublic.Location = new System.Drawing.Point(20, 185);
            this.checkPublic.Name = "checkPublic";
            this.checkPublic.Size = new System.Drawing.Size(93, 19);
            this.checkPublic.TabIndex = 7;
            this.checkPublic.Text = "Public user";
            this.checkPublic.UseVisualStyleBackColor = true;
            //
            // labelRoles
            //
            this.labelRoles.AutoSize = true;
            this.labelRoles.Location = new System.Drawing.Point(9, 165);
            this.labelRoles.Name = "labelRoles";
            this.labelRoles.Size = new System.Drawing.Size(42, 15);
            this.labelRoles.TabIndex = 6;
            this.labelRoles.Text = "Roles:";
            //
            // textFullName
            //
            this.textFullName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right));
            this.textFullName.Location = new System.Drawing.Point(9, 133);
            this.textFullName.Name = "textFullName";
            this.textFullName.Size = new System.Drawing.Size(450, 23);
            this.textFullName.TabIndex = 5;
            //
            // labelFullName
            //
            this.labelFullName.AutoSize = true;
            this.labelFullName.Location = new System.Drawing.Point(9, 113);
            this.labelFullName.Name = "labelFullName";
            this.labelFullName.Size = new System.Drawing.Size(69, 15);
            this.labelFullName.TabIndex = 4;
            this.labelFullName.Text = "Full name:";
            //
            // textEMail
            //
            this.textEMail.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right));
            this.textEMail.Location = new System.Drawing.Point(9, 81);
            this.textEMail.Name = "textEMail";
            this.textEMail.Size = new System.Drawing.Size(450, 23);
            this.textEMail.TabIndex = 3;
            //
            // labelEMail
            //
            this.labelEMail.AutoSize = true;
            this.labelEMail.Location = new System.Drawing.Point(9, 61);
            this.labelEMail.Name = "labelEMail";
            this.labelEMail.Size = new System.Drawing.Size(40, 15);
            this.labelEMail.TabIndex = 2;
            this.labelEMail.Text = "Email:";
            //
            // textUserName
            //
            this.textUserName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right));
            this.textUserName.Location = new System.Drawing.Point(9, 29);
            this.textUserName.Name = "textUserName";
            this.textUserName.Size = new System.Drawing.Size(450, 23);
            this.textUserName.TabIndex = 1;
            //
            // labelUserName
            //
            this.labelUserName.AutoSize = true;
            this.labelUserName.Location = new System.Drawing.Point(9, 9);
            this.labelUserName.Name = "labelUserName";
            this.labelUserName.Size = new System.Drawing.Size(76, 15);
            this.labelUserName.TabIndex = 0;
            this.labelUserName.Text = "User name:";
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(400, 372);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(81, 29);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            //
            // buttonAccept
            //
            this.buttonAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAccept.Location = new System.Drawing.Point(313, 372);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(81, 29);
            this.buttonAccept.TabIndex = 1;
            this.buttonAccept.Text = "&Accept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.buttonAccept_Click);
            //
            // UserEditorForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(490, 413);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonAccept);
            this.Controls.Add(this.panelForm);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UserEditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "User editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UserEditorForm_FormClosing);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.UserEditorForm_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.UserEditorForm_KeyUp);
            this.panelForm.ResumeLayout(false);
            this.panelForm.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.Label labelUserName;
        private System.Windows.Forms.TextBox textUserName;
        private System.Windows.Forms.Label labelEMail;
        private System.Windows.Forms.TextBox textEMail;
        private System.Windows.Forms.Label labelFullName;
        private System.Windows.Forms.TextBox textFullName;
        private System.Windows.Forms.Label labelRoles;
        private System.Windows.Forms.CheckBox checkPublic;
        private System.Windows.Forms.CheckBox checkStaff;
        private System.Windows.Forms.CheckBox checkProjectManager;
        private System.Windows.Forms.CheckBox checkAdmin;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.TextBox textPassword;
        private System.Windows.Forms.Button buttonResetPassword;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;
    }
}
