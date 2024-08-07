﻿namespace KNote.ClientWin.Views
{
    partial class SplashForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashForm));
            this.labelVersion = new System.Windows.Forms.Label();
            this.labelMessage = new System.Windows.Forms.Label();
            this.labelANotas = new System.Windows.Forms.Label();
            this.iconoANotas = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.iconoANotas)).BeginInit();
            this.SuspendLayout();
            // 
            // labelVersion
            // 
            this.labelVersion.ForeColor = System.Drawing.Color.White;
            this.labelVersion.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelVersion.Location = new System.Drawing.Point(121, 84);
            this.labelVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(188, 17);
            this.labelVersion.TabIndex = 14;
            this.labelVersion.Text = "Versión: ";
            this.labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelMessage
            // 
            this.labelMessage.ForeColor = System.Drawing.Color.White;
            this.labelMessage.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelMessage.Location = new System.Drawing.Point(13, 180);
            this.labelMessage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(326, 23);
            this.labelMessage.TabIndex = 13;
            this.labelMessage.Text = "Starting ...";
            this.labelMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelANotas
            // 
            this.labelANotas.Font = new System.Drawing.Font("Courier New", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.labelANotas.ForeColor = System.Drawing.Color.White;
            this.labelANotas.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelANotas.Location = new System.Drawing.Point(119, 46);
            this.labelANotas.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelANotas.Name = "labelANotas";
            this.labelANotas.Size = new System.Drawing.Size(190, 28);
            this.labelANotas.TabIndex = 12;
            this.labelANotas.Text = "KNote";
            // 
            // iconoANotas
            // 
            this.iconoANotas.ErrorImage = ((System.Drawing.Image)(resources.GetObject("iconoANotas.ErrorImage")));
            this.iconoANotas.Image = ((System.Drawing.Image)(resources.GetObject("iconoANotas.Image")));
            this.iconoANotas.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.iconoANotas.Location = new System.Drawing.Point(35, 46);
            this.iconoANotas.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.iconoANotas.Name = "iconoANotas";
            this.iconoANotas.Size = new System.Drawing.Size(76, 80);
            this.iconoANotas.TabIndex = 11;
            this.iconoANotas.TabStop = false;
            // 
            // SplashForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.ClientSize = new System.Drawing.Size(348, 213);
            this.ControlBox = false;
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.labelMessage);
            this.Controls.Add(this.labelANotas);
            this.Controls.Add(this.iconoANotas);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SplashForm";
            this.Opacity = 0.7D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.TopMost = true;
            this.Load += new System.EventHandler(this.SplashForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.iconoANotas)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label labelMessage;
        private System.Windows.Forms.Label labelANotas;
        private System.Windows.Forms.PictureBox iconoANotas;
    }
}