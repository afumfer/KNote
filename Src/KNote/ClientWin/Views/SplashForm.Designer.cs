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
            labelVersion = new Label();
            labelMessage = new Label();
            labelANotas = new Label();
            iconoANotas = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)iconoANotas).BeginInit();
            SuspendLayout();
            // 
            // labelVersion
            // 
            labelVersion.ForeColor = Color.White;
            labelVersion.ImeMode = ImeMode.NoControl;
            labelVersion.Location = new Point(123, 77);
            labelVersion.Margin = new Padding(4, 0, 4, 0);
            labelVersion.Name = "labelVersion";
            labelVersion.Size = new Size(188, 17);
            labelVersion.TabIndex = 14;
            labelVersion.Text = "Versión: ";
            labelVersion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelMessage
            // 
            labelMessage.ForeColor = Color.White;
            labelMessage.ImeMode = ImeMode.NoControl;
            labelMessage.Location = new Point(13, 180);
            labelMessage.Margin = new Padding(4, 0, 4, 0);
            labelMessage.Name = "labelMessage";
            labelMessage.Size = new Size(326, 23);
            labelMessage.TabIndex = 13;
            labelMessage.Text = "Starting ...";
            labelMessage.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelANotas
            // 
            labelANotas.Font = new Font("Courier New", 18F, FontStyle.Bold);
            labelANotas.ForeColor = Color.White;
            labelANotas.ImeMode = ImeMode.NoControl;
            labelANotas.Location = new Point(121, 35);
            labelANotas.Margin = new Padding(4, 0, 4, 0);
            labelANotas.Name = "labelANotas";
            labelANotas.Size = new Size(190, 28);
            labelANotas.TabIndex = 12;
            labelANotas.Text = "KNote";
            // 
            // iconoANotas
            // 
            iconoANotas.ImeMode = ImeMode.NoControl;
            iconoANotas.Location = new Point(26, 35);
            iconoANotas.Margin = new Padding(4, 3, 4, 3);
            iconoANotas.Name = "iconoANotas";
            iconoANotas.Size = new Size(76, 80);
            iconoANotas.TabIndex = 11;
            iconoANotas.TabStop = false;
            // 
            // SplashForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.DimGray;
            ClientSize = new Size(344, 209);
            ControlBox = false;
            Controls.Add(labelVersion);
            Controls.Add(labelMessage);
            Controls.Add(labelANotas);
            Controls.Add(iconoANotas);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SplashForm";
            Opacity = 0.7D;
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Load += SplashForm_Load;
            ((System.ComponentModel.ISupportInitialize)iconoANotas).EndInit();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label labelMessage;
        private System.Windows.Forms.Label labelANotas;
        private System.Windows.Forms.PictureBox iconoANotas;
    }
}