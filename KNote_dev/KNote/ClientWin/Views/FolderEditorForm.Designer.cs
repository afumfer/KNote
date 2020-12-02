
namespace KNote.ClientWin.Views
{
    partial class FolderEditorForm
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
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textNumber = new System.Windows.Forms.TextBox();
            this.textParentFolder = new System.Windows.Forms.TextBox();
            this.textOrderNotes = new System.Windows.Forms.TextBox();
            this.textOrder = new System.Windows.Forms.TextBox();
            this.textTags = new System.Windows.Forms.TextBox();
            this.textName = new System.Windows.Forms.TextBox();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.panelForm.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelForm
            // 
            this.panelForm.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelForm.Controls.Add(this.label5);
            this.panelForm.Controls.Add(this.label4);
            this.panelForm.Controls.Add(this.label3);
            this.panelForm.Controls.Add(this.label2);
            this.panelForm.Controls.Add(this.label1);
            this.panelForm.Controls.Add(this.textNumber);
            this.panelForm.Controls.Add(this.textParentFolder);
            this.panelForm.Controls.Add(this.textOrderNotes);
            this.panelForm.Controls.Add(this.textOrder);
            this.panelForm.Controls.Add(this.textTags);
            this.panelForm.Controls.Add(this.textName);
            this.panelForm.Location = new System.Drawing.Point(4, 7);
            this.panelForm.Name = "panelForm";
            this.panelForm.Size = new System.Drawing.Size(496, 236);
            this.panelForm.TabIndex = 0;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 182);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(78, 15);
            this.label5.TabIndex = 10;
            this.label5.Text = "Parent folder:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(86, 124);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 15);
            this.label4.TabIndex = 9;
            this.label4.Text = "Order notes:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 124);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 15);
            this.label3.TabIndex = 8;
            this.label3.Text = "Order:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 15);
            this.label2.TabIndex = 7;
            this.label2.Text = "Tags:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 15);
            this.label1.TabIndex = 6;
            this.label1.Text = "Name:";
            // 
            // textNumber
            // 
            this.textNumber.BackColor = System.Drawing.SystemColors.Control;
            this.textNumber.Location = new System.Drawing.Point(448, 29);
            this.textNumber.Name = "textNumber";
            this.textNumber.ReadOnly = true;
            this.textNumber.Size = new System.Drawing.Size(40, 23);
            this.textNumber.TabIndex = 5;
            // 
            // textParentFolder
            // 
            this.textParentFolder.BackColor = System.Drawing.SystemColors.Control;
            this.textParentFolder.Location = new System.Drawing.Point(8, 200);
            this.textParentFolder.Name = "textParentFolder";
            this.textParentFolder.ReadOnly = true;
            this.textParentFolder.Size = new System.Drawing.Size(480, 23);
            this.textParentFolder.TabIndex = 4;
            // 
            // textOrderNotes
            // 
            this.textOrderNotes.Location = new System.Drawing.Point(86, 142);
            this.textOrderNotes.Name = "textOrderNotes";
            this.textOrderNotes.Size = new System.Drawing.Size(402, 23);
            this.textOrderNotes.TabIndex = 3;
            // 
            // textOrder
            // 
            this.textOrder.Location = new System.Drawing.Point(8, 142);
            this.textOrder.Name = "textOrder";
            this.textOrder.Size = new System.Drawing.Size(72, 23);
            this.textOrder.TabIndex = 2;
            // 
            // textTags
            // 
            this.textTags.Location = new System.Drawing.Point(8, 82);
            this.textTags.Name = "textTags";
            this.textTags.Size = new System.Drawing.Size(480, 23);
            this.textTags.TabIndex = 1;
            // 
            // textName
            // 
            this.textName.Location = new System.Drawing.Point(8, 29);
            this.textName.Name = "textName";
            this.textName.Size = new System.Drawing.Size(441, 23);
            this.textName.TabIndex = 0;
            // 
            // buttonAccept
            // 
            this.buttonAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAccept.Location = new System.Drawing.Point(324, 255);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(81, 29);
            this.buttonAccept.TabIndex = 1;
            this.buttonAccept.Text = "&Accept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.buttonAccept_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(411, 255);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(81, 29);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // FolderEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(504, 296);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonAccept);
            this.Controls.Add(this.panelForm);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FolderEditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Folder editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FolderEditorForm_FormClosing);
            this.Load += new System.EventHandler(this.FolderEditorForm_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FolderEditorForm_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.FolderEditorForm_KeyUp);
            this.panelForm.ResumeLayout(false);
            this.panelForm.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textNumber;
        private System.Windows.Forms.TextBox textParentFolder;
        private System.Windows.Forms.TextBox textOrderNotes;
        private System.Windows.Forms.TextBox textOrder;
        private System.Windows.Forms.TextBox textTags;
        private System.Windows.Forms.TextBox textName;
    }
}