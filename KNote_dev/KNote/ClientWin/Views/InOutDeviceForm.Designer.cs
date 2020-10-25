namespace KNote.ClientWin.Views
{
    partial class InOutDeviceForm
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
            this.textOut = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textOut
            // 
            this.textOut.BackColor = System.Drawing.Color.Black;
            this.textOut.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textOut.Font = new System.Drawing.Font("Courier New", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.textOut.ForeColor = System.Drawing.Color.White;
            this.textOut.Location = new System.Drawing.Point(0, 0);
            this.textOut.Multiline = true;
            this.textOut.Name = "textOut";
            this.textOut.Size = new System.Drawing.Size(429, 399);
            this.textOut.TabIndex = 0;
            // 
            // InOutDeviceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(429, 399);
            this.Controls.Add(this.textOut);
            this.Name = "InOutDeviceForm";
            this.Text = "InOutDeviceForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textOut;
    }
}