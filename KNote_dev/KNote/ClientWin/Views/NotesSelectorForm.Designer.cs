namespace KNote.ClientWin.Views
{
    partial class NotesSelectorForm
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
            this.panelBottom = new System.Windows.Forms.Panel();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.panelDataGridNotes = new System.Windows.Forms.Panel();
            this.dataGridNotes = new System.Windows.Forms.DataGridView();
            this.panelForm.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.panelDataGridNotes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridNotes)).BeginInit();
            this.SuspendLayout();
            // 
            // panelForm
            // 
            this.panelForm.Controls.Add(this.panelBottom);
            this.panelForm.Controls.Add(this.panelDataGridNotes);
            this.panelForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelForm.Location = new System.Drawing.Point(0, 0);
            this.panelForm.Name = "panelForm";
            this.panelForm.Size = new System.Drawing.Size(552, 495);
            this.panelForm.TabIndex = 3;
            // 
            // panelBottom
            // 
            this.panelBottom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.panelBottom.Controls.Add(this.buttonCancel);
            this.panelBottom.Controls.Add(this.buttonAccept);
            this.panelBottom.Location = new System.Drawing.Point(0, 451);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(552, 44);
            this.panelBottom.TabIndex = 1;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(461, 8);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(79, 24);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonAccept
            // 
            this.buttonAccept.Location = new System.Drawing.Point(376, 8);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(79, 24);
            this.buttonAccept.TabIndex = 0;
            this.buttonAccept.Text = "&Accept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.buttonAccept_Click);
            // 
            // panelDataGridNotes
            // 
            this.panelDataGridNotes.Controls.Add(this.dataGridNotes);
            this.panelDataGridNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDataGridNotes.Location = new System.Drawing.Point(0, 0);
            this.panelDataGridNotes.Name = "panelDataGridNotes";
            this.panelDataGridNotes.Size = new System.Drawing.Size(552, 495);
            this.panelDataGridNotes.TabIndex = 2;
            // 
            // dataGridNotes
            // 
            this.dataGridNotes.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dataGridNotes.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridNotes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridNotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridNotes.Location = new System.Drawing.Point(0, 0);
            this.dataGridNotes.Name = "dataGridNotes";
            this.dataGridNotes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridNotes.Size = new System.Drawing.Size(552, 495);
            this.dataGridNotes.TabIndex = 0;
            this.dataGridNotes.Text = "dataGridView1";
            this.dataGridNotes.SelectionChanged += new System.EventHandler(this.dataGridNotes_SelectionChanged);
            this.dataGridNotes.DoubleClick += new System.EventHandler(this.dataGridNotes_DoubleClick);
            this.dataGridNotes.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGridNotes_KeyDown);
            this.dataGridNotes.KeyUp += new System.Windows.Forms.KeyEventHandler(this.dataGridNotes_KeyUp);
            // 
            // NotesSelectorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(552, 495);
            this.Controls.Add(this.panelForm);
            this.Name = "NotesSelectorForm";
            this.Text = "Notes selector";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.NotesSelectorForm_FormClosing);
            this.panelForm.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            this.panelDataGridNotes.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridNotes)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.Panel panelDataGridNotes;
        private System.Windows.Forms.DataGridView dataGridNotes;
    }
}