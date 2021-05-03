
namespace KNote.ClientWin.Views
{
    partial class KNoteAboutForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KNoteAboutForm));
            this.labelVersion = new System.Windows.Forms.Label();
            this.labelANotas = new System.Windows.Forms.Label();
            this.iconoANotas = new System.Windows.Forms.PictureBox();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.labelInfo = new System.Windows.Forms.Label();
            this.labelRepository = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.iconoANotas)).BeginInit();
            this.SuspendLayout();
            // 
            // labelVersion
            // 
            this.labelVersion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.labelVersion.ForeColor = System.Drawing.Color.Black;
            this.labelVersion.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelVersion.Location = new System.Drawing.Point(115, 45);
            this.labelVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(300, 17);
            this.labelVersion.TabIndex = 17;
            this.labelVersion.Text = "Versión: ";
            this.labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelANotas
            // 
            this.labelANotas.Font = new System.Drawing.Font("Courier New", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.labelANotas.ForeColor = System.Drawing.Color.Black;
            this.labelANotas.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelANotas.Location = new System.Drawing.Point(112, 17);
            this.labelANotas.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelANotas.Name = "labelANotas";
            this.labelANotas.Size = new System.Drawing.Size(190, 28);
            this.labelANotas.TabIndex = 16;
            this.labelANotas.Text = "KaNote";
            // 
            // iconoANotas
            // 
            this.iconoANotas.ErrorImage = ((System.Drawing.Image)(resources.GetObject("iconoANotas.ErrorImage")));
            this.iconoANotas.Image = ((System.Drawing.Image)(resources.GetObject("iconoANotas.Image")));
            this.iconoANotas.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.iconoANotas.Location = new System.Drawing.Point(30, 17);
            this.iconoANotas.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.iconoANotas.Name = "iconoANotas";
            this.iconoANotas.Size = new System.Drawing.Size(74, 79);
            this.iconoANotas.TabIndex = 15;
            this.iconoANotas.TabStop = false;
            // 
            // buttonAccept
            // 
            this.buttonAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAccept.Location = new System.Drawing.Point(441, 355);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(81, 29);
            this.buttonAccept.TabIndex = 18;
            this.buttonAccept.Text = "&Accept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.buttonAccept_Click);
            // 
            // labelInfo
            // 
            this.labelInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelInfo.ForeColor = System.Drawing.Color.Black;
            this.labelInfo.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelInfo.Location = new System.Drawing.Point(116, 103);
            this.labelInfo.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(405, 238);
            this.labelInfo.TabIndex = 19;
            this.labelInfo.Text = "Info ...";
            // 
            // labelRepository
            // 
            this.labelRepository.ForeColor = System.Drawing.Color.Black;
            this.labelRepository.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.labelRepository.Location = new System.Drawing.Point(116, 62);
            this.labelRepository.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelRepository.Name = "labelRepository";
            this.labelRepository.Size = new System.Drawing.Size(299, 24);
            this.labelRepository.TabIndex = 20;
            this.labelRepository.Text = "Repository: ";
            this.labelRepository.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // KNoteAboutForm
            // 
            this.AcceptButton = this.buttonAccept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(534, 396);
            this.Controls.Add(this.labelRepository);
            this.Controls.Add(this.labelInfo);
            this.Controls.Add(this.buttonAccept);
            this.Controls.Add(this.labelVersion);
            this.Controls.Add(this.labelANotas);
            this.Controls.Add(this.iconoANotas);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "KNoteAboutForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About KaNote";
            this.Load += new System.EventHandler(this.KNoteAboutForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.iconoANotas)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label labelANotas;
        private System.Windows.Forms.PictureBox iconoANotas;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Label labelRepository;
    }
}