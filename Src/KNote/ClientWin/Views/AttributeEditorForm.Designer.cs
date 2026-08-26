
namespace KNote.ClientWin.Views
{
    partial class AttributeEditorForm
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
            this.panelTop = new System.Windows.Forms.Panel();
            this.comboDataType = new System.Windows.Forms.ComboBox();
            this.labelDataType = new System.Windows.Forms.Label();
            this.comboNoteType = new System.Windows.Forms.ComboBox();
            this.labelNoteType = new System.Windows.Forms.Label();
            this.numericOrder = new System.Windows.Forms.NumericUpDown();
            this.labelOrder = new System.Windows.Forms.Label();
            this.checkRequiredValue = new System.Windows.Forms.CheckBox();
            this.textDescription = new System.Windows.Forms.TextBox();
            this.labelDescription = new System.Windows.Forms.Label();
            this.textName = new System.Windows.Forms.TextBox();
            this.labelName = new System.Windows.Forms.Label();
            this.panelTabulatedValues = new System.Windows.Forms.Panel();
            this.listViewTabulatedValues = new System.Windows.Forms.ListView();
            this.buttonEditTabValue = new System.Windows.Forms.Button();
            this.buttonDeleteTabValue = new System.Windows.Forms.Button();
            this.buttonAddTabValue = new System.Windows.Forms.Button();
            this.labelTabulatedValues = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericOrder)).BeginInit();
            this.panelTabulatedValues.SuspendLayout();
            this.SuspendLayout();
            //
            // panelTop
            //
            this.panelTop.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelTop.Controls.Add(this.comboDataType);
            this.panelTop.Controls.Add(this.labelDataType);
            this.panelTop.Controls.Add(this.comboNoteType);
            this.panelTop.Controls.Add(this.labelNoteType);
            this.panelTop.Controls.Add(this.numericOrder);
            this.panelTop.Controls.Add(this.labelOrder);
            this.panelTop.Controls.Add(this.checkRequiredValue);
            this.panelTop.Controls.Add(this.textDescription);
            this.panelTop.Controls.Add(this.labelDescription);
            this.panelTop.Controls.Add(this.textName);
            this.panelTop.Controls.Add(this.labelName);
            this.panelTop.Location = new System.Drawing.Point(5, 12);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(490, 335);
            this.panelTop.TabIndex = 0;
            //
            // comboDataType
            //
            this.comboDataType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboDataType.FormattingEnabled = true;
            this.comboDataType.Location = new System.Drawing.Point(9, 297);
            this.comboDataType.Name = "comboDataType";
            this.comboDataType.Size = new System.Drawing.Size(470, 23);
            this.comboDataType.TabIndex = 10;
            this.comboDataType.SelectedIndexChanged += new System.EventHandler(this.comboDataType_SelectedIndexChanged);
            //
            // labelDataType
            //
            this.labelDataType.AutoSize = true;
            this.labelDataType.Location = new System.Drawing.Point(9, 277);
            this.labelDataType.Name = "labelDataType";
            this.labelDataType.Size = new System.Drawing.Size(65, 15);
            this.labelDataType.TabIndex = 9;
            this.labelDataType.Text = "Data type:";
            //
            // comboNoteType
            //
            this.comboNoteType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboNoteType.FormattingEnabled = true;
            this.comboNoteType.Location = new System.Drawing.Point(9, 244);
            this.comboNoteType.Name = "comboNoteType";
            this.comboNoteType.Size = new System.Drawing.Size(470, 23);
            this.comboNoteType.TabIndex = 8;
            //
            // labelNoteType
            //
            this.labelNoteType.AutoSize = true;
            this.labelNoteType.Location = new System.Drawing.Point(9, 224);
            this.labelNoteType.Name = "labelNoteType";
            this.labelNoteType.Size = new System.Drawing.Size(68, 15);
            this.labelNoteType.TabIndex = 7;
            this.labelNoteType.Text = "Note type:";
            //
            // numericOrder
            //
            this.numericOrder.Location = new System.Drawing.Point(9, 191);
            this.numericOrder.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.numericOrder.Name = "numericOrder";
            this.numericOrder.Size = new System.Drawing.Size(80, 23);
            this.numericOrder.TabIndex = 6;
            //
            // labelOrder
            //
            this.labelOrder.AutoSize = true;
            this.labelOrder.Location = new System.Drawing.Point(9, 171);
            this.labelOrder.Name = "labelOrder";
            this.labelOrder.Size = new System.Drawing.Size(40, 15);
            this.labelOrder.TabIndex = 5;
            this.labelOrder.Text = "Order:";
            //
            // checkRequiredValue
            //
            this.checkRequiredValue.AutoSize = true;
            this.checkRequiredValue.Location = new System.Drawing.Point(9, 141);
            this.checkRequiredValue.Name = "checkRequiredValue";
            this.checkRequiredValue.Size = new System.Drawing.Size(107, 19);
            this.checkRequiredValue.TabIndex = 4;
            this.checkRequiredValue.Text = "Required value";
            this.checkRequiredValue.UseVisualStyleBackColor = true;
            //
            // textDescription
            //
            this.textDescription.Location = new System.Drawing.Point(9, 81);
            this.textDescription.Multiline = true;
            this.textDescription.Name = "textDescription";
            this.textDescription.Size = new System.Drawing.Size(470, 50);
            this.textDescription.TabIndex = 3;
            //
            // labelDescription
            //
            this.labelDescription.AutoSize = true;
            this.labelDescription.Location = new System.Drawing.Point(9, 61);
            this.labelDescription.Name = "labelDescription";
            this.labelDescription.Size = new System.Drawing.Size(72, 15);
            this.labelDescription.TabIndex = 2;
            this.labelDescription.Text = "Description:";
            //
            // textName
            //
            this.textName.Location = new System.Drawing.Point(9, 29);
            this.textName.Name = "textName";
            this.textName.Size = new System.Drawing.Size(470, 23);
            this.textName.TabIndex = 1;
            //
            // labelName
            //
            this.labelName.AutoSize = true;
            this.labelName.Location = new System.Drawing.Point(9, 9);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(42, 15);
            this.labelName.TabIndex = 0;
            this.labelName.Text = "Name:";
            //
            // panelTabulatedValues
            //
            this.panelTabulatedValues.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelTabulatedValues.Controls.Add(this.listViewTabulatedValues);
            this.panelTabulatedValues.Controls.Add(this.buttonEditTabValue);
            this.panelTabulatedValues.Controls.Add(this.buttonDeleteTabValue);
            this.panelTabulatedValues.Controls.Add(this.buttonAddTabValue);
            this.panelTabulatedValues.Controls.Add(this.labelTabulatedValues);
            this.panelTabulatedValues.Location = new System.Drawing.Point(5, 352);
            this.panelTabulatedValues.Name = "panelTabulatedValues";
            this.panelTabulatedValues.Size = new System.Drawing.Size(490, 130);
            this.panelTabulatedValues.TabIndex = 1;
            //
            // listViewTabulatedValues
            //
            this.listViewTabulatedValues.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewTabulatedValues.HideSelection = false;
            this.listViewTabulatedValues.Location = new System.Drawing.Point(0, 32);
            this.listViewTabulatedValues.MultiSelect = false;
            this.listViewTabulatedValues.Name = "listViewTabulatedValues";
            this.listViewTabulatedValues.Size = new System.Drawing.Size(481, 96);
            this.listViewTabulatedValues.TabIndex = 4;
            this.listViewTabulatedValues.UseCompatibleStateImageBehavior = false;
            this.listViewTabulatedValues.DoubleClick += new System.EventHandler(this.listViewTabulatedValues_DoubleClick);
            this.listViewTabulatedValues.Resize += new System.EventHandler(this.listViewTabulatedValues_Resize);
            //
            // buttonEditTabValue
            //
            this.buttonEditTabValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonEditTabValue.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonEditTabValue.Location = new System.Drawing.Point(454, 0);
            this.buttonEditTabValue.Name = "buttonEditTabValue";
            this.buttonEditTabValue.Size = new System.Drawing.Size(27, 26);
            this.buttonEditTabValue.TabIndex = 3;
            this.buttonEditTabValue.Text = "...";
            this.buttonEditTabValue.UseVisualStyleBackColor = true;
            this.buttonEditTabValue.Click += new System.EventHandler(this.buttonEditTabValue_Click);
            //
            // buttonDeleteTabValue
            //
            this.buttonDeleteTabValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDeleteTabValue.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonDeleteTabValue.Location = new System.Drawing.Point(424, 0);
            this.buttonDeleteTabValue.Name = "buttonDeleteTabValue";
            this.buttonDeleteTabValue.Size = new System.Drawing.Size(27, 26);
            this.buttonDeleteTabValue.TabIndex = 2;
            this.buttonDeleteTabValue.Text = "-";
            this.buttonDeleteTabValue.UseVisualStyleBackColor = true;
            this.buttonDeleteTabValue.Click += new System.EventHandler(this.buttonDeleteTabValue_Click);
            //
            // buttonAddTabValue
            //
            this.buttonAddTabValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAddTabValue.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.buttonAddTabValue.Location = new System.Drawing.Point(394, 0);
            this.buttonAddTabValue.Name = "buttonAddTabValue";
            this.buttonAddTabValue.Size = new System.Drawing.Size(27, 26);
            this.buttonAddTabValue.TabIndex = 1;
            this.buttonAddTabValue.Text = "+";
            this.buttonAddTabValue.UseVisualStyleBackColor = true;
            this.buttonAddTabValue.Click += new System.EventHandler(this.buttonAddTabValue_Click);
            //
            // labelTabulatedValues
            //
            this.labelTabulatedValues.AutoSize = true;
            this.labelTabulatedValues.Location = new System.Drawing.Point(0, 6);
            this.labelTabulatedValues.Name = "labelTabulatedValues";
            this.labelTabulatedValues.Size = new System.Drawing.Size(101, 15);
            this.labelTabulatedValues.TabIndex = 0;
            this.labelTabulatedValues.Text = "Tabulated values:";
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(417, 497);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(81, 29);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            //
            // buttonAccept
            //
            this.buttonAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAccept.Location = new System.Drawing.Point(330, 497);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(81, 29);
            this.buttonAccept.TabIndex = 2;
            this.buttonAccept.Text = "&Accept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.buttonAccept_Click);
            //
            // AttributeEditorForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(510, 538);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonAccept);
            this.Controls.Add(this.panelTabulatedValues);
            this.Controls.Add(this.panelTop);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AttributeEditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Attribute editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AttributeEditorForm_FormClosing);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.AttributeEditorForm_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.AttributeEditorForm_KeyUp);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericOrder)).EndInit();
            this.panelTabulatedValues.ResumeLayout(false);
            this.panelTabulatedValues.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.TextBox textName;
        private System.Windows.Forms.Label labelName;
        private System.Windows.Forms.TextBox textDescription;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.CheckBox checkRequiredValue;
        private System.Windows.Forms.Label labelOrder;
        private System.Windows.Forms.NumericUpDown numericOrder;
        private System.Windows.Forms.Label labelNoteType;
        private System.Windows.Forms.ComboBox comboNoteType;
        private System.Windows.Forms.Label labelDataType;
        private System.Windows.Forms.ComboBox comboDataType;
        private System.Windows.Forms.Panel panelTabulatedValues;
        private System.Windows.Forms.Label labelTabulatedValues;
        private System.Windows.Forms.Button buttonEditTabValue;
        private System.Windows.Forms.Button buttonDeleteTabValue;
        private System.Windows.Forms.Button buttonAddTabValue;
        private System.Windows.Forms.ListView listViewTabulatedValues;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;
    }
}
