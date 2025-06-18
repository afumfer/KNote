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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NotesSelectorForm));
            panelForm = new Panel();
            panelDataGridNotes = new Panel();
            dataGridNotes = new DataGridView();
            contextMenu = new ContextMenuStrip(components);
            panelBottom = new Panel();
            buttonCancel = new Button();
            buttonAccept = new Button();
            panelForm.SuspendLayout();
            panelDataGridNotes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridNotes).BeginInit();
            panelBottom.SuspendLayout();
            SuspendLayout();
            // 
            // panelForm
            // 
            panelForm.Controls.Add(panelDataGridNotes);
            panelForm.Controls.Add(panelBottom);
            panelForm.Dock = DockStyle.Fill;
            panelForm.Location = new Point(0, 0);
            panelForm.Margin = new Padding(3, 4, 3, 4);
            panelForm.Name = "panelForm";
            panelForm.Size = new Size(667, 615);
            panelForm.TabIndex = 0;
            // 
            // panelDataGridNotes
            // 
            panelDataGridNotes.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelDataGridNotes.Controls.Add(dataGridNotes);
            panelDataGridNotes.Location = new Point(1, 1);
            panelDataGridNotes.Margin = new Padding(3, 4, 3, 4);
            panelDataGridNotes.Name = "panelDataGridNotes";
            panelDataGridNotes.Size = new Size(666, 557);
            panelDataGridNotes.TabIndex = 2;
            // 
            // dataGridNotes
            // 
            dataGridNotes.AllowUserToAddRows = false;
            dataGridNotes.AllowUserToResizeRows = false;
            dataGridNotes.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridNotes.BackgroundColor = SystemColors.Window;
            dataGridNotes.BorderStyle = BorderStyle.None;
            dataGridNotes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridNotes.ContextMenuStrip = contextMenu;
            dataGridNotes.EditMode = DataGridViewEditMode.EditOnF2;
            dataGridNotes.GridColor = SystemColors.ControlDark;
            dataGridNotes.Location = new Point(0, 0);
            dataGridNotes.Margin = new Padding(3, 4, 3, 4);
            dataGridNotes.Name = "dataGridNotes";
            dataGridNotes.ReadOnly = true;
            dataGridNotes.RowHeadersVisible = false;
            dataGridNotes.RowHeadersWidth = 25;
            dataGridNotes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridNotes.Size = new Size(665, 555);
            dataGridNotes.TabIndex = 0;
            dataGridNotes.Text = "dataGridView1";
            dataGridNotes.ColumnHeaderMouseClick += dataGridNotes_ColumnHeaderMouseClick;
            dataGridNotes.SelectionChanged += dataGridNotes_SelectionChanged;
            dataGridNotes.DoubleClick += dataGridNotes_DoubleClick;
            dataGridNotes.KeyDown += dataGridNotes_KeyDown;
            dataGridNotes.KeyUp += dataGridNotes_KeyUp;
            // 
            // contextMenu
            // 
            contextMenu.ImageScalingSize = new Size(20, 20);
            contextMenu.Name = "contextMenu";
            contextMenu.Size = new Size(61, 4);
            // 
            // panelBottom
            // 
            panelBottom.Controls.Add(buttonCancel);
            panelBottom.Controls.Add(buttonAccept);
            panelBottom.Dock = DockStyle.Bottom;
            panelBottom.Location = new Point(0, 556);
            panelBottom.Margin = new Padding(3, 4, 3, 4);
            panelBottom.Name = "panelBottom";
            panelBottom.Size = new Size(667, 59);
            panelBottom.TabIndex = 1;
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.DialogResult = DialogResult.Cancel;
            buttonCancel.Location = new Point(567, 13);
            buttonCancel.Margin = new Padding(3, 4, 3, 4);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(90, 34);
            buttonCancel.TabIndex = 2;
            buttonCancel.Text = "&Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // buttonAccept
            // 
            buttonAccept.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonAccept.DialogResult = DialogResult.OK;
            buttonAccept.Location = new Point(471, 13);
            buttonAccept.Margin = new Padding(3, 4, 3, 4);
            buttonAccept.Name = "buttonAccept";
            buttonAccept.Size = new Size(90, 34);
            buttonAccept.TabIndex = 1;
            buttonAccept.Text = "&Accept";
            buttonAccept.UseVisualStyleBackColor = true;
            buttonAccept.Click += buttonAccept_Click;
            // 
            // NotesSelectorForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(667, 615);
            Controls.Add(panelForm);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 4, 3, 4);
            Name = "NotesSelectorForm";
            Text = "Notes selector";
            FormClosing += NotesSelectorForm_FormClosing;
            panelForm.ResumeLayout(false);
            panelDataGridNotes.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridNotes).EndInit();
            panelBottom.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.Panel panelDataGridNotes;
        private System.Windows.Forms.DataGridView dataGridNotes;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
    }
}