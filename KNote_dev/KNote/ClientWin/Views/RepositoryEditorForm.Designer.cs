
namespace KNote.ClientWin.Views
{
    partial class RepositoryEditorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RepositoryEditorForm));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.panelForm = new System.Windows.Forms.Panel();
            this.buttonFolderSearch = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textNumber = new System.Windows.Forms.TextBox();
            this.textOrderNotes = new System.Windows.Forms.TextBox();
            this.textOrder = new System.Windows.Forms.TextBox();
            this.textTags = new System.Windows.Forms.TextBox();
            this.textName = new System.Windows.Forms.TextBox();
            this.panelForm.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(471, 266);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(64, 29);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonAccept
            // 
            this.buttonAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAccept.Location = new System.Drawing.Point(401, 266);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(64, 29);
            this.buttonAccept.TabIndex = 4;
            this.buttonAccept.Text = "&Accept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.buttonAccept_Click);
            // 
            // panelForm
            // 
            this.panelForm.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelForm.Controls.Add(this.buttonFolderSearch);
            this.panelForm.Controls.Add(this.label4);
            this.panelForm.Controls.Add(this.label3);
            this.panelForm.Controls.Add(this.label2);
            this.panelForm.Controls.Add(this.label1);
            this.panelForm.Controls.Add(this.textNumber);
            this.panelForm.Controls.Add(this.textOrderNotes);
            this.panelForm.Controls.Add(this.textOrder);
            this.panelForm.Controls.Add(this.textTags);
            this.panelForm.Controls.Add(this.textName);
            this.panelForm.Location = new System.Drawing.Point(4, 5);
            this.panelForm.Name = "panelForm";
            this.panelForm.Size = new System.Drawing.Size(541, 244);
            this.panelForm.TabIndex = 3;
            // 
            // buttonFolderSearch
            // 
            this.buttonFolderSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFolderSearch.BackColor = System.Drawing.SystemColors.Control;
            this.buttonFolderSearch.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonFolderSearch.Location = new System.Drawing.Point(801, 199);
            this.buttonFolderSearch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonFolderSearch.Name = "buttonFolderSearch";
            this.buttonFolderSearch.Size = new System.Drawing.Size(27, 25);
            this.buttonFolderSearch.TabIndex = 11;
            this.buttonFolderSearch.Text = "...";
            this.buttonFolderSearch.UseVisualStyleBackColor = false;
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
            this.textNumber.Location = new System.Drawing.Point(439, 29);
            this.textNumber.Name = "textNumber";
            this.textNumber.ReadOnly = true;
            this.textNumber.Size = new System.Drawing.Size(49, 23);
            this.textNumber.TabIndex = 5;
            this.textNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
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
            this.textName.Size = new System.Drawing.Size(434, 23);
            this.textName.TabIndex = 0;
            // 
            // RepositoryEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(547, 307);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonAccept);
            this.Controls.Add(this.panelForm);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RepositoryEditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Repository editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RepositoryEditorForm_FormClosing);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.RepositoryEditorForm_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.RepositoryEditorForm_KeyUp);
            this.panelForm.ResumeLayout(false);
            this.panelForm.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.Button buttonFolderSearch;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textNumber;
        private System.Windows.Forms.TextBox textOrderNotes;
        private System.Windows.Forms.TextBox textOrder;
        private System.Windows.Forms.TextBox textTags;
        private System.Windows.Forms.TextBox textName;
    }
}