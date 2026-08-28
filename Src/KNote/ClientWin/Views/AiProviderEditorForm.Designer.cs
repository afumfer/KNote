
namespace KNote.ClientWin.Views
{
    partial class AiProviderEditorForm
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
            this.textHost = new System.Windows.Forms.TextBox();
            this.labelHost = new System.Windows.Forms.Label();
            this.textApiKey = new System.Windows.Forms.TextBox();
            this.labelApiKey = new System.Windows.Forms.Label();
            this.textModelName = new System.Windows.Forms.TextBox();
            this.labelModelName = new System.Windows.Forms.Label();
            this.comboProvider = new System.Windows.Forms.ComboBox();
            this.labelProvider = new System.Windows.Forms.Label();
            this.textAlias = new System.Windows.Forms.TextBox();
            this.labelAlias = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.panelForm.SuspendLayout();
            this.SuspendLayout();
            //
            // panelForm
            //
            this.panelForm.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelForm.Controls.Add(this.textHost);
            this.panelForm.Controls.Add(this.labelHost);
            this.panelForm.Controls.Add(this.textApiKey);
            this.panelForm.Controls.Add(this.labelApiKey);
            this.panelForm.Controls.Add(this.textModelName);
            this.panelForm.Controls.Add(this.labelModelName);
            this.panelForm.Controls.Add(this.comboProvider);
            this.panelForm.Controls.Add(this.labelProvider);
            this.panelForm.Controls.Add(this.textAlias);
            this.panelForm.Controls.Add(this.labelAlias);
            this.panelForm.Location = new System.Drawing.Point(5, 12);
            this.panelForm.Name = "panelForm";
            this.panelForm.Size = new System.Drawing.Size(407, 260);
            this.panelForm.TabIndex = 0;
            //
            // textHost
            //
            this.textHost.Location = new System.Drawing.Point(9, 237);
            this.textHost.Name = "textHost";
            this.textHost.Size = new System.Drawing.Size(390, 23);
            this.textHost.TabIndex = 4;
            //
            // labelHost
            //
            this.labelHost.AutoSize = true;
            this.labelHost.Location = new System.Drawing.Point(9, 217);
            this.labelHost.Name = "labelHost";
            this.labelHost.Size = new System.Drawing.Size(94, 15);
            this.labelHost.TabIndex = 9;
            this.labelHost.Text = "Host (Ollama):";
            //
            // textApiKey
            //
            this.textApiKey.Location = new System.Drawing.Point(9, 185);
            this.textApiKey.Name = "textApiKey";
            this.textApiKey.PasswordChar = '*';
            this.textApiKey.Size = new System.Drawing.Size(390, 23);
            this.textApiKey.TabIndex = 3;
            //
            // labelApiKey
            //
            this.labelApiKey.AutoSize = true;
            this.labelApiKey.Location = new System.Drawing.Point(9, 165);
            this.labelApiKey.Name = "labelApiKey";
            this.labelApiKey.Size = new System.Drawing.Size(133, 15);
            this.labelApiKey.TabIndex = 8;
            this.labelApiKey.Text = "API key (optional):";
            //
            // textModelName
            //
            this.textModelName.Location = new System.Drawing.Point(9, 133);
            this.textModelName.Name = "textModelName";
            this.textModelName.Size = new System.Drawing.Size(390, 23);
            this.textModelName.TabIndex = 2;
            //
            // labelModelName
            //
            this.labelModelName.AutoSize = true;
            this.labelModelName.Location = new System.Drawing.Point(9, 113);
            this.labelModelName.Name = "labelModelName";
            this.labelModelName.Size = new System.Drawing.Size(44, 15);
            this.labelModelName.TabIndex = 7;
            this.labelModelName.Text = "Model:";
            //
            // comboProvider
            //
            this.comboProvider.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboProvider.FormattingEnabled = true;
            this.comboProvider.Location = new System.Drawing.Point(9, 81);
            this.comboProvider.Name = "comboProvider";
            this.comboProvider.Size = new System.Drawing.Size(390, 23);
            this.comboProvider.TabIndex = 1;
            this.comboProvider.SelectedIndexChanged += new System.EventHandler(this.comboProvider_SelectedIndexChanged);
            this.comboProvider.SelectionChangeCommitted += new System.EventHandler(this.comboProvider_SelectionChangeCommitted);
            //
            // labelProvider
            //
            this.labelProvider.AutoSize = true;
            this.labelProvider.Location = new System.Drawing.Point(9, 61);
            this.labelProvider.Name = "labelProvider";
            this.labelProvider.Size = new System.Drawing.Size(55, 15);
            this.labelProvider.TabIndex = 6;
            this.labelProvider.Text = "Provider:";
            //
            // textAlias
            //
            this.textAlias.Location = new System.Drawing.Point(9, 29);
            this.textAlias.Name = "textAlias";
            this.textAlias.Size = new System.Drawing.Size(390, 23);
            this.textAlias.TabIndex = 0;
            //
            // labelAlias
            //
            this.labelAlias.AutoSize = true;
            this.labelAlias.Location = new System.Drawing.Point(9, 9);
            this.labelAlias.Name = "labelAlias";
            this.labelAlias.Size = new System.Drawing.Size(36, 15);
            this.labelAlias.TabIndex = 5;
            this.labelAlias.Text = "Alias:";
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(324, 278);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(81, 29);
            this.buttonCancel.TabIndex = 6;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            //
            // buttonAccept
            //
            this.buttonAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAccept.Location = new System.Drawing.Point(237, 278);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(81, 29);
            this.buttonAccept.TabIndex = 5;
            this.buttonAccept.Text = "&Accept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.buttonAccept_Click);
            //
            // AiProviderEditorForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(417, 319);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonAccept);
            this.Controls.Add(this.panelForm);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AiProviderEditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "AI provider editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AiProviderEditorForm_FormClosing);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.AiProviderEditorForm_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.AiProviderEditorForm_KeyUp);
            this.panelForm.ResumeLayout(false);
            this.panelForm.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.TextBox textHost;
        private System.Windows.Forms.Label labelHost;
        private System.Windows.Forms.TextBox textApiKey;
        private System.Windows.Forms.Label labelApiKey;
        private System.Windows.Forms.TextBox textModelName;
        private System.Windows.Forms.Label labelModelName;
        private System.Windows.Forms.ComboBox comboProvider;
        private System.Windows.Forms.Label labelProvider;
        private System.Windows.Forms.TextBox textAlias;
        private System.Windows.Forms.Label labelAlias;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;
    }
}
