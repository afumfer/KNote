
namespace KNote.ClientWin.Views
{
    partial class TraceNoteEditorForm
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
            this.textWeight = new System.Windows.Forms.TextBox();
            this.labelWeight = new System.Windows.Forms.Label();
            this.textOrder = new System.Windows.Forms.TextBox();
            this.labelOrder = new System.Windows.Forms.Label();
            this.comboTraceNoteType = new System.Windows.Forms.ComboBox();
            this.labelTraceNoteType = new System.Windows.Forms.Label();
            this.buttonSelectRelatedNote = new System.Windows.Forms.Button();
            this.textRelatedNote = new System.Windows.Forms.TextBox();
            this.labelRelatedNote = new System.Windows.Forms.Label();
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
            this.panelForm.Controls.Add(this.textWeight);
            this.panelForm.Controls.Add(this.labelWeight);
            this.panelForm.Controls.Add(this.textOrder);
            this.panelForm.Controls.Add(this.labelOrder);
            this.panelForm.Controls.Add(this.comboTraceNoteType);
            this.panelForm.Controls.Add(this.labelTraceNoteType);
            this.panelForm.Controls.Add(this.buttonSelectRelatedNote);
            this.panelForm.Controls.Add(this.textRelatedNote);
            this.panelForm.Controls.Add(this.labelRelatedNote);
            this.panelForm.Location = new System.Drawing.Point(5, 12);
            this.panelForm.Name = "panelForm";
            this.panelForm.Size = new System.Drawing.Size(407, 181);
            this.panelForm.TabIndex = 0;
            //
            // textWeight
            //
            this.textWeight.Location = new System.Drawing.Point(150, 133);
            this.textWeight.Name = "textWeight";
            this.textWeight.Size = new System.Drawing.Size(90, 23);
            this.textWeight.TabIndex = 3;
            //
            // labelWeight
            //
            this.labelWeight.AutoSize = true;
            this.labelWeight.Location = new System.Drawing.Point(150, 113);
            this.labelWeight.Name = "labelWeight";
            this.labelWeight.Size = new System.Drawing.Size(48, 15);
            this.labelWeight.TabIndex = 8;
            this.labelWeight.Text = "Weight:";
            //
            // textOrder
            //
            this.textOrder.Location = new System.Drawing.Point(9, 133);
            this.textOrder.Name = "textOrder";
            this.textOrder.Size = new System.Drawing.Size(90, 23);
            this.textOrder.TabIndex = 2;
            //
            // labelOrder
            //
            this.labelOrder.AutoSize = true;
            this.labelOrder.Location = new System.Drawing.Point(9, 113);
            this.labelOrder.Name = "labelOrder";
            this.labelOrder.Size = new System.Drawing.Size(40, 15);
            this.labelOrder.TabIndex = 7;
            this.labelOrder.Text = "Order:";
            //
            // comboTraceNoteType
            //
            this.comboTraceNoteType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboTraceNoteType.FormattingEnabled = true;
            this.comboTraceNoteType.Location = new System.Drawing.Point(9, 81);
            this.comboTraceNoteType.Name = "comboTraceNoteType";
            this.comboTraceNoteType.Size = new System.Drawing.Size(390, 23);
            this.comboTraceNoteType.TabIndex = 1;
            //
            // labelTraceNoteType
            //
            this.labelTraceNoteType.AutoSize = true;
            this.labelTraceNoteType.Location = new System.Drawing.Point(9, 61);
            this.labelTraceNoteType.Name = "labelTraceNoteType";
            this.labelTraceNoteType.Size = new System.Drawing.Size(37, 15);
            this.labelTraceNoteType.TabIndex = 6;
            this.labelTraceNoteType.Text = "Type:";
            //
            // buttonSelectRelatedNote
            //
            this.buttonSelectRelatedNote.Location = new System.Drawing.Point(361, 28);
            this.buttonSelectRelatedNote.Name = "buttonSelectRelatedNote";
            this.buttonSelectRelatedNote.Size = new System.Drawing.Size(38, 24);
            this.buttonSelectRelatedNote.TabIndex = 0;
            this.buttonSelectRelatedNote.Text = "...";
            this.buttonSelectRelatedNote.UseVisualStyleBackColor = true;
            this.buttonSelectRelatedNote.Click += new System.EventHandler(this.buttonSelectRelatedNote_Click);
            //
            // textRelatedNote
            //
            this.textRelatedNote.Location = new System.Drawing.Point(9, 29);
            this.textRelatedNote.Name = "textRelatedNote";
            this.textRelatedNote.ReadOnly = true;
            this.textRelatedNote.Size = new System.Drawing.Size(346, 23);
            this.textRelatedNote.TabIndex = 9;
            this.textRelatedNote.TabStop = false;
            //
            // labelRelatedNote
            //
            this.labelRelatedNote.AutoSize = true;
            this.labelRelatedNote.Location = new System.Drawing.Point(9, 9);
            this.labelRelatedNote.Name = "labelRelatedNote";
            this.labelRelatedNote.Size = new System.Drawing.Size(78, 15);
            this.labelRelatedNote.TabIndex = 5;
            this.labelRelatedNote.Text = "Related note:";
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(324, 199);
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
            this.buttonAccept.Location = new System.Drawing.Point(237, 199);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(81, 29);
            this.buttonAccept.TabIndex = 1;
            this.buttonAccept.Text = "&Accept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.buttonAccept_Click);
            //
            // TraceNoteEditorForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(417, 240);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonAccept);
            this.Controls.Add(this.panelForm);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TraceNoteEditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Trace note editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TraceNoteEditorForm_FormClosing);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TraceNoteEditorForm_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TraceNoteEditorForm_KeyUp);
            this.panelForm.ResumeLayout(false);
            this.panelForm.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.TextBox textWeight;
        private System.Windows.Forms.Label labelWeight;
        private System.Windows.Forms.TextBox textOrder;
        private System.Windows.Forms.Label labelOrder;
        private System.Windows.Forms.ComboBox comboTraceNoteType;
        private System.Windows.Forms.Label labelTraceNoteType;
        private System.Windows.Forms.Button buttonSelectRelatedNote;
        private System.Windows.Forms.TextBox textRelatedNote;
        private System.Windows.Forms.Label labelRelatedNote;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;
    }
}
