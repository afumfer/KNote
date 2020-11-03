namespace KNote.ClientWin.Views
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
            this.labelVersion.Location = new System.Drawing.Point(112, 73);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(161, 15);
            this.labelVersion.TabIndex = 14;
            this.labelVersion.Text = "Versión: ";
            this.labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelMessage
            // 
            this.labelMessage.ForeColor = System.Drawing.Color.White;
            this.labelMessage.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelMessage.Location = new System.Drawing.Point(27, 152);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(161, 24);
            this.labelMessage.TabIndex = 13;
            this.labelMessage.Text = "Starting ...";
            this.labelMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelANotas
            // 
            this.labelANotas.Font = new System.Drawing.Font("Courier New", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelANotas.ForeColor = System.Drawing.Color.White;
            this.labelANotas.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelANotas.Location = new System.Drawing.Point(110, 40);
            this.labelANotas.Name = "labelANotas";
            this.labelANotas.Size = new System.Drawing.Size(163, 24);
            this.labelANotas.TabIndex = 12;
            this.labelANotas.Text = "KeyNote";
            // 
            // iconoANotas
            // 
            this.iconoANotas.ErrorImage = ((System.Drawing.Image)(resources.GetObject("iconoANotas.ErrorImage")));
            this.iconoANotas.Image = ((System.Drawing.Image)(resources.GetObject("iconoANotas.Image")));
            this.iconoANotas.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.iconoANotas.Location = new System.Drawing.Point(30, 40);
            this.iconoANotas.Name = "iconoANotas";
            this.iconoANotas.Size = new System.Drawing.Size(76, 76);
            this.iconoANotas.TabIndex = 11;
            this.iconoANotas.TabStop = false;
            // 
            // SplashForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.ClientSize = new System.Drawing.Size(306, 192);
            this.ControlBox = false;
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.labelMessage);
            this.Controls.Add(this.labelANotas);
            this.Controls.Add(this.iconoANotas);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
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