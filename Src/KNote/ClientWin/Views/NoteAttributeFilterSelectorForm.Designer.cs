
namespace KNote.ClientWin.Views
{
    partial class NoteAttributeFilterSelectorForm
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
            this.textValue = new System.Windows.Forms.TextBox();
            this.labelValue = new System.Windows.Forms.Label();
            this.comboAttribute = new System.Windows.Forms.ComboBox();
            this.labelAttribute = new System.Windows.Forms.Label();
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
            this.panelForm.Controls.Add(this.textValue);
            this.panelForm.Controls.Add(this.labelValue);
            this.panelForm.Controls.Add(this.comboAttribute);
            this.panelForm.Controls.Add(this.labelAttribute);
            this.panelForm.Location = new System.Drawing.Point(5, 12);
            this.panelForm.Name = "panelForm";
            this.panelForm.Size = new System.Drawing.Size(340, 121);
            this.panelForm.TabIndex = 0;
            //
            // textValue
            //
            this.textValue.Location = new System.Drawing.Point(9, 81);
            this.textValue.Name = "textValue";
            this.textValue.Size = new System.Drawing.Size(323, 23);
            this.textValue.TabIndex = 3;
            //
            // labelValue
            //
            this.labelValue.AutoSize = true;
            this.labelValue.Location = new System.Drawing.Point(9, 61);
            this.labelValue.Name = "labelValue";
            this.labelValue.Size = new System.Drawing.Size(41, 15);
            this.labelValue.TabIndex = 2;
            this.labelValue.Text = "Value:";
            //
            // comboAttribute
            //
            this.comboAttribute.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboAttribute.FormattingEnabled = true;
            this.comboAttribute.Location = new System.Drawing.Point(9, 29);
            this.comboAttribute.Name = "comboAttribute";
            this.comboAttribute.Size = new System.Drawing.Size(323, 23);
            this.comboAttribute.TabIndex = 1;
            //
            // labelAttribute
            //
            this.labelAttribute.AutoSize = true;
            this.labelAttribute.Location = new System.Drawing.Point(9, 9);
            this.labelAttribute.Name = "labelAttribute";
            this.labelAttribute.Size = new System.Drawing.Size(63, 15);
            this.labelAttribute.TabIndex = 0;
            this.labelAttribute.Text = "Attribute:";
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(257, 139);
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
            this.buttonAccept.Location = new System.Drawing.Point(170, 139);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(81, 29);
            this.buttonAccept.TabIndex = 1;
            this.buttonAccept.Text = "&Accept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.buttonAccept_Click);
            //
            // NoteAttributeFilterSelectorForm
            //
            this.AcceptButton = this.buttonAccept;
            this.CancelButton = this.buttonCancel;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 180);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonAccept);
            this.Controls.Add(this.panelForm);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NoteAttributeFilterSelectorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add attribute filter";
            this.panelForm.ResumeLayout(false);
            this.panelForm.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.TextBox textValue;
        private System.Windows.Forms.Label labelValue;
        private System.Windows.Forms.ComboBox comboAttribute;
        private System.Windows.Forms.Label labelAttribute;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;
    }
}
