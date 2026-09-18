
namespace KNote.ClientWin.Views
{
    partial class UsersSelectorForm
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
            panelBottom = new Panel();
            buttonCancel = new Button();
            buttonAccept = new Button();
            panelForm = new Panel();
            listViewUsers = new ListView();
            panelBottom.SuspendLayout();
            panelForm.SuspendLayout();
            SuspendLayout();
            // 
            // panelBottom
            // 
            panelBottom.Controls.Add(buttonCancel);
            panelBottom.Controls.Add(buttonAccept);
            panelBottom.Dock = DockStyle.Bottom;
            panelBottom.Location = new Point(0, 387);
            panelBottom.Name = "panelBottom";
            panelBottom.Size = new Size(408, 44);
            panelBottom.TabIndex = 2;
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(317, 8);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(79, 24);
            buttonCancel.TabIndex = 5;
            buttonCancel.Text = "&Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // buttonAccept
            // 
            buttonAccept.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonAccept.DialogResult = DialogResult.OK;
            buttonAccept.Location = new Point(232, 8);
            buttonAccept.Name = "buttonAccept";
            buttonAccept.Size = new Size(79, 24);
            buttonAccept.TabIndex = 4;
            buttonAccept.Text = "&Accept";
            buttonAccept.UseVisualStyleBackColor = true;
            buttonAccept.Click += buttonAccept_Click;
            // 
            // panelForm
            // 
            panelForm.Controls.Add(listViewUsers);
            panelForm.Dock = DockStyle.Fill;
            panelForm.Location = new Point(0, 0);
            panelForm.Name = "panelForm";
            panelForm.Size = new Size(408, 387);
            panelForm.TabIndex = 3;
            // 
            // listViewUsers
            // 
            listViewUsers.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listViewUsers.Location = new Point(6, 6);
            listViewUsers.MultiSelect = false;
            listViewUsers.Name = "listViewUsers";
            listViewUsers.Size = new Size(396, 372);
            listViewUsers.TabIndex = 0;
            listViewUsers.UseCompatibleStateImageBehavior = false;
            listViewUsers.SelectedIndexChanged += listViewUsers_SelectedIndexChanged;
            listViewUsers.Resize += listViewUsers_Resize;
            // 
            // UsersSelectorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(408, 431);
            Controls.Add(panelForm);
            Controls.Add(panelBottom);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "UsersSelectorForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "User selector";
            Load += UsersSelectorForm_Load;
            panelBottom.ResumeLayout(false);
            panelForm.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.ListView listViewUsers;
    }
}
