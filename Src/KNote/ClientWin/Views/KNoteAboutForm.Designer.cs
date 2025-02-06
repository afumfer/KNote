
using KNote.Model;

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
            labelVersion = new Label();
            labelAppName = new Label();
            iconoANotas = new PictureBox();
            buttonAccept = new Button();
            labelInfo = new Label();
            labelRepository = new Label();
            ((System.ComponentModel.ISupportInitialize)iconoANotas).BeginInit();
            SuspendLayout();
            // 
            // labelVersion
            // 
            labelVersion.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelVersion.ForeColor = Color.Black;
            labelVersion.ImeMode = ImeMode.NoControl;
            labelVersion.Location = new Point(115, 45);
            labelVersion.Margin = new Padding(4, 0, 4, 0);
            labelVersion.Name = "labelVersion";
            labelVersion.Size = new Size(300, 17);
            labelVersion.TabIndex = 17;
            labelVersion.Text = "Versión: ";
            labelVersion.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // labelAppName
            // 
            labelAppName.Font = new Font("Courier New", 18F, FontStyle.Bold);
            labelAppName.ForeColor = Color.Black;
            labelAppName.ImeMode = ImeMode.NoControl;
            labelAppName.Location = new Point(112, 17);
            labelAppName.Margin = new Padding(4, 0, 4, 0);
            labelAppName.Name = "labelAppName";
            labelAppName.Size = new Size(190, 28);
            labelAppName.TabIndex = 16;
            labelAppName.Text = "App name ...";
            // 
            // iconoANotas
            // 
            iconoANotas.ErrorImage = (Image)resources.GetObject("iconoANotas.ErrorImage");
            iconoANotas.Image = (Image)resources.GetObject("iconoANotas.Image");
            iconoANotas.ImeMode = ImeMode.NoControl;
            iconoANotas.Location = new Point(30, 17);
            iconoANotas.Margin = new Padding(4, 3, 4, 3);
            iconoANotas.Name = "iconoANotas";
            iconoANotas.Size = new Size(74, 79);
            iconoANotas.TabIndex = 15;
            iconoANotas.TabStop = false;
            // 
            // buttonAccept
            // 
            buttonAccept.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonAccept.Location = new Point(438, 388);
            buttonAccept.Name = "buttonAccept";
            buttonAccept.Size = new Size(81, 29);
            buttonAccept.TabIndex = 18;
            buttonAccept.Text = "&Accept";
            buttonAccept.UseVisualStyleBackColor = true;
            buttonAccept.Click += buttonAccept_Click;
            // 
            // labelInfo
            // 
            labelInfo.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            labelInfo.ForeColor = Color.Black;
            labelInfo.ImeMode = ImeMode.NoControl;
            labelInfo.Location = new Point(116, 103);
            labelInfo.Margin = new Padding(4, 0, 4, 0);
            labelInfo.Name = "labelInfo";
            labelInfo.Size = new Size(389, 262);
            labelInfo.TabIndex = 19;
            labelInfo.Text = "Info ...";
            // 
            // labelRepository
            // 
            labelRepository.ForeColor = Color.Black;
            labelRepository.ImeMode = ImeMode.NoControl;
            labelRepository.Location = new Point(116, 62);
            labelRepository.Margin = new Padding(4, 0, 4, 0);
            labelRepository.Name = "labelRepository";
            labelRepository.Size = new Size(299, 24);
            labelRepository.TabIndex = 20;
            labelRepository.Text = "Repository: ";
            labelRepository.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // KNoteAboutForm
            // 
            AcceptButton = buttonAccept;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(531, 429);
            Controls.Add(labelRepository);
            Controls.Add(labelInfo);
            Controls.Add(buttonAccept);
            Controls.Add(labelVersion);
            Controls.Add(labelAppName);
            Controls.Add(iconoANotas);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "KNoteAboutForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "About KNote";
            Load += KNoteAboutForm_Load;
            ((System.ComponentModel.ISupportInitialize)iconoANotas).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label labelAppName;
        private System.Windows.Forms.PictureBox iconoANotas;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.Label labelInfo;
        private System.Windows.Forms.Label labelRepository;
    }
}