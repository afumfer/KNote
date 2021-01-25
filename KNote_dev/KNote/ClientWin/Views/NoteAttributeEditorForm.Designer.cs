
namespace KNote.ClientWin.Views
{
    partial class NoteAttributeEditorForm
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
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.panelForm = new System.Windows.Forms.Panel();
            this.listViewValue = new System.Windows.Forms.ListView();
            this.comboValue = new System.Windows.Forms.ComboBox();
            this.checkValue = new System.Windows.Forms.CheckBox();
            this.buttonSelDate = new System.Windows.Forms.Button();
            this.labelAttribute = new System.Windows.Forms.Label();
            this.labelDescription = new System.Windows.Forms.Label();
            this.textValue = new System.Windows.Forms.TextBox();
            this.panelForm.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(413, 251);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(81, 29);
            this.buttonCancel.TabIndex = 10;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonAccept
            // 
            this.buttonAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAccept.Location = new System.Drawing.Point(326, 251);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(81, 29);
            this.buttonAccept.TabIndex = 9;
            this.buttonAccept.Text = "&Accept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.buttonAccept_Click);
            // 
            // panelForm
            // 
            this.panelForm.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelForm.Controls.Add(this.listViewValue);
            this.panelForm.Controls.Add(this.comboValue);
            this.panelForm.Controls.Add(this.checkValue);
            this.panelForm.Controls.Add(this.buttonSelDate);
            this.panelForm.Controls.Add(this.labelAttribute);
            this.panelForm.Controls.Add(this.labelDescription);
            this.panelForm.Controls.Add(this.textValue);
            this.panelForm.Location = new System.Drawing.Point(6, 6);
            this.panelForm.Name = "panelForm";
            this.panelForm.Size = new System.Drawing.Size(496, 233);
            this.panelForm.TabIndex = 8;
            // 
            // listViewValue
            // 
            this.listViewValue.HideSelection = false;
            this.listViewValue.Location = new System.Drawing.Point(301, 96);
            this.listViewValue.Name = "listViewValue";
            this.listViewValue.Size = new System.Drawing.Size(112, 76);
            this.listViewValue.TabIndex = 10;
            this.listViewValue.UseCompatibleStateImageBehavior = false;
            this.listViewValue.Visible = false;
            // 
            // comboValue
            // 
            this.comboValue.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboValue.FormattingEnabled = true;
            this.comboValue.Location = new System.Drawing.Point(168, 97);
            this.comboValue.Name = "comboValue";
            this.comboValue.Size = new System.Drawing.Size(105, 23);
            this.comboValue.TabIndex = 9;
            this.comboValue.Visible = false;
            // 
            // checkValue
            // 
            this.checkValue.AutoSize = true;
            this.checkValue.Location = new System.Drawing.Point(58, 101);
            this.checkValue.Name = "checkValue";
            this.checkValue.Size = new System.Drawing.Size(100, 19);
            this.checkValue.TabIndex = 8;
            this.checkValue.Text = "checkAtribute";
            this.checkValue.UseVisualStyleBackColor = true;
            this.checkValue.Visible = false;
            // 
            // buttonSelDate
            // 
            this.buttonSelDate.Location = new System.Drawing.Point(10, 97);
            this.buttonSelDate.Name = "buttonSelDate";
            this.buttonSelDate.Size = new System.Drawing.Size(24, 24);
            this.buttonSelDate.TabIndex = 7;
            this.buttonSelDate.Text = "...";
            this.buttonSelDate.UseVisualStyleBackColor = true;
            this.buttonSelDate.Visible = false;
            this.buttonSelDate.Click += new System.EventHandler(this.buttonSelDate_Click);
            // 
            // labelAttribute
            // 
            this.labelAttribute.AutoSize = true;
            this.labelAttribute.Location = new System.Drawing.Point(10, 14);
            this.labelAttribute.Name = "labelAttribute";
            this.labelAttribute.Size = new System.Drawing.Size(54, 15);
            this.labelAttribute.TabIndex = 2;
            this.labelAttribute.Text = "Attribute";
            // 
            // labelDescription
            // 
            this.labelDescription.AutoSize = true;
            this.labelDescription.Location = new System.Drawing.Point(10, 193);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(67, 15);
            this.labelDescription.TabIndex = 1;
            this.labelDescription.Text = "Description";
            // 
            // textValue
            // 
            this.textValue.Location = new System.Drawing.Point(10, 32);
            this.textValue.Multiline = true;
            this.textValue.Name = "textValue";
            this.textValue.Size = new System.Drawing.Size(478, 46);
            this.textValue.TabIndex = 0;
            this.textValue.Visible = false;
            // 
            // NoteAttributeEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(506, 292);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonAccept);
            this.Controls.Add(this.panelForm);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NoteAttributeEditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Note attribute editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.NoteAttributeEditorForm_FormClosing);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NoteAttributeEditorForm_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.NoteAttributeEditorForm_KeyUp);
            this.panelForm.ResumeLayout(false);
            this.panelForm.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.TextBox textValue;
        private System.Windows.Forms.Label labelAttribute;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.Button buttonSelDate;
        private System.Windows.Forms.ListView listViewValue;
        private System.Windows.Forms.ComboBox comboValue;
        private System.Windows.Forms.CheckBox checkValue;
    }
}