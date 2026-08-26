
namespace KNote.ClientWin.Views
{
    partial class KAttributeTabulatedValueEditorForm
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
            this.numericOrder = new System.Windows.Forms.NumericUpDown();
            this.labelOrder = new System.Windows.Forms.Label();
            this.textDescription = new System.Windows.Forms.TextBox();
            this.labelDescription = new System.Windows.Forms.Label();
            this.textValue = new System.Windows.Forms.TextBox();
            this.labelValue = new System.Windows.Forms.Label();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.panelForm.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericOrder)).BeginInit();
            this.SuspendLayout();
            //
            // panelForm
            //
            this.panelForm.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelForm.Controls.Add(this.numericOrder);
            this.panelForm.Controls.Add(this.labelOrder);
            this.panelForm.Controls.Add(this.textDescription);
            this.panelForm.Controls.Add(this.labelDescription);
            this.panelForm.Controls.Add(this.textValue);
            this.panelForm.Controls.Add(this.labelValue);
            this.panelForm.Location = new System.Drawing.Point(5, 12);
            this.panelForm.Name = "panelForm";
            this.panelForm.Size = new System.Drawing.Size(340, 181);
            this.panelForm.TabIndex = 0;
            //
            // numericOrder
            //
            this.numericOrder.Location = new System.Drawing.Point(9, 141);
            this.numericOrder.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.numericOrder.Name = "numericOrder";
            this.numericOrder.Size = new System.Drawing.Size(80, 23);
            this.numericOrder.TabIndex = 5;
            //
            // labelOrder
            //
            this.labelOrder.AutoSize = true;
            this.labelOrder.Location = new System.Drawing.Point(9, 121);
            this.labelOrder.Name = "labelOrder";
            this.labelOrder.Size = new System.Drawing.Size(40, 15);
            this.labelOrder.TabIndex = 4;
            this.labelOrder.Text = "Order:";
            //
            // textDescription
            //
            this.textDescription.Location = new System.Drawing.Point(9, 81);
            this.textDescription.Name = "textDescription";
            this.textDescription.Size = new System.Drawing.Size(323, 23);
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
            // textValue
            //
            this.textValue.Location = new System.Drawing.Point(9, 29);
            this.textValue.Name = "textValue";
            this.textValue.Size = new System.Drawing.Size(323, 23);
            this.textValue.TabIndex = 1;
            //
            // labelValue
            //
            this.labelValue.AutoSize = true;
            this.labelValue.Location = new System.Drawing.Point(9, 9);
            this.labelValue.Name = "labelValue";
            this.labelValue.Size = new System.Drawing.Size(41, 15);
            this.labelValue.TabIndex = 0;
            this.labelValue.Text = "Value:";
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(257, 199);
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
            this.buttonAccept.Location = new System.Drawing.Point(170, 199);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(81, 29);
            this.buttonAccept.TabIndex = 1;
            this.buttonAccept.Text = "&Accept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.buttonAccept_Click);
            //
            // KAttributeTabulatedValueEditorForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 240);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonAccept);
            this.Controls.Add(this.panelForm);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "KAttributeTabulatedValueEditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Tabulated value editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.KAttributeTabulatedValueEditorForm_FormClosing);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.KAttributeTabulatedValueEditorForm_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.KAttributeTabulatedValueEditorForm_KeyUp);
            this.panelForm.ResumeLayout(false);
            this.panelForm.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericOrder)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.TextBox textValue;
        private System.Windows.Forms.Label labelValue;
        private System.Windows.Forms.TextBox textDescription;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.Label labelOrder;
        private System.Windows.Forms.NumericUpDown numericOrder;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;
    }
}
