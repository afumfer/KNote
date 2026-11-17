namespace KNote.ClientWin.Views
{
    partial class ReviewSelectionForm
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
            this.tabLists = new System.Windows.Forms.TabControl();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.labelChanges = new System.Windows.Forms.Label();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            //
            // tabLists
            //
            this.tabLists.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabLists.Location = new System.Drawing.Point(0, 0);
            this.tabLists.Name = "tabLists";
            this.tabLists.SelectedIndex = 0;
            this.tabLists.Size = new System.Drawing.Size(944, 513);
            this.tabLists.TabIndex = 0;
            //
            // panelBottom
            //
            this.panelBottom.Controls.Add(this.labelChanges);
            this.panelBottom.Controls.Add(this.buttonAccept);
            this.panelBottom.Controls.Add(this.buttonCancel);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 513);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(944, 48);
            this.panelBottom.TabIndex = 1;
            //
            // labelChanges
            //
            this.labelChanges.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelChanges.Location = new System.Drawing.Point(9, 16);
            this.labelChanges.Name = "labelChanges";
            this.labelChanges.Size = new System.Drawing.Size(700, 20);
            this.labelChanges.TabIndex = 2;
            //
            // buttonAccept
            //
            this.buttonAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAccept.Location = new System.Drawing.Point(728, 10);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(100, 29);
            this.buttonAccept.TabIndex = 0;
            this.buttonAccept.Text = "&Accept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.buttonAccept_Click);
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(834, 10);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(100, 29);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            //
            // ReviewSelectionForm
            //
            this.AcceptButton = this.buttonAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(944, 561);
            this.Controls.Add(this.tabLists);
            this.Controls.Add(this.panelBottom);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Name = "ReviewSelectionForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Review selection";
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabLists;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Label labelChanges;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.Button buttonCancel;
    }
}
