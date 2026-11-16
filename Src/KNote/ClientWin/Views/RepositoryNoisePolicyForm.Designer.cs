namespace KNote.ClientWin.Views
{
    partial class RepositoryNoisePolicyForm
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
            this.labelDescription = new System.Windows.Forms.Label();
            this.labelReadOnly = new System.Windows.Forms.Label();
            this.labelTypes = new System.Windows.Forms.Label();
            this.checkedListTypes = new System.Windows.Forms.CheckedListBox();
            this.labelNoTypes = new System.Windows.Forms.Label();
            this.labelFolders = new System.Windows.Forms.Label();
            this.listFolders = new System.Windows.Forms.ListBox();
            this.buttonAddFolder = new System.Windows.Forms.Button();
            this.buttonRemoveFolder = new System.Windows.Forms.Button();
            this.labelCount = new System.Windows.Forms.Label();
            this.buttonCountAgain = new System.Windows.Forms.Button();
            this.buttonDiscard = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.panelForm.SuspendLayout();
            this.SuspendLayout();
            //
            // panelForm
            //
            this.panelForm.Controls.Add(this.buttonSave);
            this.panelForm.Controls.Add(this.buttonDiscard);
            this.panelForm.Controls.Add(this.buttonCountAgain);
            this.panelForm.Controls.Add(this.labelCount);
            this.panelForm.Controls.Add(this.buttonRemoveFolder);
            this.panelForm.Controls.Add(this.buttonAddFolder);
            this.panelForm.Controls.Add(this.listFolders);
            this.panelForm.Controls.Add(this.labelFolders);
            this.panelForm.Controls.Add(this.labelNoTypes);
            this.panelForm.Controls.Add(this.checkedListTypes);
            this.panelForm.Controls.Add(this.labelTypes);
            this.panelForm.Controls.Add(this.labelReadOnly);
            this.panelForm.Controls.Add(this.labelDescription);
            this.panelForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelForm.Location = new System.Drawing.Point(0, 0);
            this.panelForm.Name = "panelForm";
            this.panelForm.Size = new System.Drawing.Size(626, 570);
            this.panelForm.TabIndex = 0;
            //
            // labelDescription
            //
            this.labelDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDescription.Location = new System.Drawing.Point(9, 12);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(608, 64);
            this.labelDescription.TabIndex = 10;
            //
            // labelReadOnly
            //
            this.labelReadOnly.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelReadOnly.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.labelReadOnly.Location = new System.Drawing.Point(9, 78);
            this.labelReadOnly.Name = "labelReadOnly";
            this.labelReadOnly.Size = new System.Drawing.Size(608, 20);
            this.labelReadOnly.TabIndex = 11;
            //
            // labelTypes
            //
            this.labelTypes.AutoSize = true;
            this.labelTypes.Location = new System.Drawing.Point(9, 104);
            this.labelTypes.Name = "labelTypes";
            this.labelTypes.Size = new System.Drawing.Size(118, 15);
            this.labelTypes.TabIndex = 12;
            this.labelTypes.Text = "Note types left out";
            //
            // checkedListTypes
            //
            this.checkedListTypes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.checkedListTypes.CheckOnClick = true;
            this.checkedListTypes.FormattingEnabled = true;
            this.checkedListTypes.IntegralHeight = false;
            this.checkedListTypes.Location = new System.Drawing.Point(9, 124);
            this.checkedListTypes.Name = "checkedListTypes";
            this.checkedListTypes.Size = new System.Drawing.Size(290, 306);
            this.checkedListTypes.TabIndex = 0;
            this.checkedListTypes.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.checkedListTypes_ItemCheck);
            //
            // labelNoTypes
            //
            this.labelNoTypes.Location = new System.Drawing.Point(15, 130);
            this.labelNoTypes.Name = "labelNoTypes";
            this.labelNoTypes.Size = new System.Drawing.Size(278, 40);
            this.labelNoTypes.TabIndex = 13;
            this.labelNoTypes.Text = "The repository has no note types of its own.";
            this.labelNoTypes.Visible = false;
            //
            // labelFolders
            //
            this.labelFolders.AutoSize = true;
            this.labelFolders.Location = new System.Drawing.Point(313, 104);
            this.labelFolders.Name = "labelFolders";
            this.labelFolders.Size = new System.Drawing.Size(107, 15);
            this.labelFolders.TabIndex = 14;
            this.labelFolders.Text = "Folders left out (0)";
            //
            // listFolders
            //
            this.listFolders.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listFolders.FormattingEnabled = true;
            this.listFolders.HorizontalScrollbar = true;
            this.listFolders.IntegralHeight = false;
            this.listFolders.Location = new System.Drawing.Point(313, 124);
            this.listFolders.Name = "listFolders";
            this.listFolders.Size = new System.Drawing.Size(304, 271);
            this.listFolders.TabIndex = 1;
            this.listFolders.SelectedIndexChanged += new System.EventHandler(this.listFolders_SelectedIndexChanged);
            this.listFolders.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listFolders_KeyDown);
            //
            // buttonAddFolder
            //
            this.buttonAddFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonAddFolder.Location = new System.Drawing.Point(313, 401);
            this.buttonAddFolder.Name = "buttonAddFolder";
            this.buttonAddFolder.Size = new System.Drawing.Size(100, 29);
            this.buttonAddFolder.TabIndex = 2;
            this.buttonAddFolder.Text = "&Add folder...";
            this.buttonAddFolder.UseVisualStyleBackColor = true;
            this.buttonAddFolder.Click += new System.EventHandler(this.buttonAddFolder_Click);
            //
            // buttonRemoveFolder
            //
            this.buttonRemoveFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonRemoveFolder.Location = new System.Drawing.Point(419, 401);
            this.buttonRemoveFolder.Name = "buttonRemoveFolder";
            this.buttonRemoveFolder.Size = new System.Drawing.Size(100, 29);
            this.buttonRemoveFolder.TabIndex = 3;
            this.buttonRemoveFolder.Text = "&Remove";
            this.buttonRemoveFolder.UseVisualStyleBackColor = true;
            this.buttonRemoveFolder.Click += new System.EventHandler(this.buttonRemoveFolder_Click);
            //
            // labelCount
            //
            this.labelCount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCount.Location = new System.Drawing.Point(9, 440);
            this.labelCount.Name = "labelCount";
            this.labelCount.Size = new System.Drawing.Size(482, 86);
            this.labelCount.TabIndex = 15;
            //
            // buttonCountAgain
            //
            this.buttonCountAgain.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCountAgain.Location = new System.Drawing.Point(497, 440);
            this.buttonCountAgain.Name = "buttonCountAgain";
            this.buttonCountAgain.Size = new System.Drawing.Size(120, 29);
            this.buttonCountAgain.TabIndex = 6;
            this.buttonCountAgain.Text = "&Count again";
            this.buttonCountAgain.UseVisualStyleBackColor = true;
            this.buttonCountAgain.Click += new System.EventHandler(this.buttonCountAgain_Click);
            //
            // buttonDiscard
            //
            this.buttonDiscard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonDiscard.Location = new System.Drawing.Point(9, 533);
            this.buttonDiscard.Name = "buttonDiscard";
            this.buttonDiscard.Size = new System.Drawing.Size(130, 29);
            this.buttonDiscard.TabIndex = 5;
            this.buttonDiscard.Text = "&Discard changes";
            this.buttonDiscard.UseVisualStyleBackColor = true;
            this.buttonDiscard.Click += new System.EventHandler(this.buttonDiscard_Click);
            //
            // buttonSave
            //
            this.buttonSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSave.Location = new System.Drawing.Point(477, 533);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(140, 29);
            this.buttonSave.TabIndex = 4;
            this.buttonSave.Text = "&Save policy";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            //
            // RepositoryNoisePolicyForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(626, 570);
            this.Controls.Add(this.panelForm);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "RepositoryNoisePolicyForm";
            this.Text = "Noise policy";
            this.panelForm.ResumeLayout(false);
            this.panelForm.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.Label labelReadOnly;
        private System.Windows.Forms.Label labelTypes;
        private System.Windows.Forms.CheckedListBox checkedListTypes;
        private System.Windows.Forms.Label labelNoTypes;
        private System.Windows.Forms.Label labelFolders;
        private System.Windows.Forms.ListBox listFolders;
        private System.Windows.Forms.Button buttonAddFolder;
        private System.Windows.Forms.Button buttonRemoveFolder;
        private System.Windows.Forms.Label labelCount;
        private System.Windows.Forms.Button buttonCountAgain;
        private System.Windows.Forms.Button buttonDiscard;
        private System.Windows.Forms.Button buttonSave;
    }
}
