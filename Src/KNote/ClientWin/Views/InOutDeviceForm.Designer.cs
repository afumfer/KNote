﻿namespace KNote.ClientWin.Views
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InOutDeviceForm));
            textOut = new TextBox();
            SuspendLayout();
            // 
            // textOut
            // 
            textOut.BackColor = Color.Black;
            textOut.Dock = DockStyle.Fill;
            textOut.Font = new Font("Courier New", 11.25F, FontStyle.Regular, GraphicsUnit.Point);
            textOut.ForeColor = Color.White;
            textOut.Location = new Point(0, 0);
            textOut.Multiline = true;
            textOut.Name = "textOut";
            textOut.Size = new Size(737, 387);
            textOut.TabIndex = 0;
            textOut.TabStop = false;
            // 
            // InOutDeviceForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(737, 387);
            Controls.Add(textOut);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "InOutDeviceForm";
            ShowInTaskbar = false;
            Text = "KntScript out console";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox textOut;
    }
}