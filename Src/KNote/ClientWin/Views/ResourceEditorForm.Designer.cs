
namespace KNote.ClientWin.Views
{
    partial class ResourceEditorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResourceEditorForm));
            buttonCancel = new Button();
            buttonAccept = new Button();
            panelForm = new Panel();
            kntView = new KntWebView.KntEditView();
            textFileName = new TextBox();
            labelPreview = new Label();
            textDescription = new TextBox();
            textOrder = new TextBox();
            labelDescription = new Label();
            labelOrder = new Label();
            buttonSelectFile = new Button();
            labelFileName = new Label();
            checkLockDescription = new CheckBox();
            labelAuthor = new Label();
            openFileDialog = new OpenFileDialog();
            panelForm.SuspendLayout();
            SuspendLayout();
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Location = new Point(586, 588);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(81, 29);
            buttonCancel.TabIndex = 2;
            buttonCancel.Text = "&Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // buttonAccept
            // 
            buttonAccept.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonAccept.Location = new Point(499, 588);
            buttonAccept.Name = "buttonAccept";
            buttonAccept.Size = new Size(81, 29);
            buttonAccept.TabIndex = 1;
            buttonAccept.Text = "&Accept";
            buttonAccept.UseVisualStyleBackColor = true;
            buttonAccept.Click += buttonAccept_Click;
            // 
            // panelForm
            // 
            panelForm.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelForm.Controls.Add(kntView);
            panelForm.Controls.Add(labelAuthor);
            panelForm.Controls.Add(checkLockDescription);
            panelForm.Controls.Add(textFileName);
            panelForm.Controls.Add(labelPreview);
            panelForm.Controls.Add(textDescription);
            panelForm.Controls.Add(textOrder);
            panelForm.Controls.Add(labelDescription);
            panelForm.Controls.Add(labelOrder);
            panelForm.Controls.Add(buttonSelectFile);
            panelForm.Controls.Add(labelFileName);
            panelForm.Location = new Point(1, 3);
            panelForm.Name = "panelForm";
            panelForm.Size = new Size(676, 579);
            panelForm.TabIndex = 0;
            // 
            // kntView
            // 
            kntView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            kntView.BorderStyle = BorderStyle.FixedSingle;
            kntView.Location = new Point(11, 183);
            kntView.Name = "kntView";
            kntView.Size = new Size(655, 387);
            kntView.TabIndex = 17;
            // 
            // textFileName
            // 
            textFileName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textFileName.Enabled = false;
            textFileName.Location = new Point(89, 14);
            textFileName.Name = "textFileName";
            textFileName.Size = new Size(460, 23);
            textFileName.TabIndex = 0;
            // 
            // labelPreview
            // 
            labelPreview.AutoSize = true;
            labelPreview.Location = new Point(11, 165);
            labelPreview.Name = "labelPreview";
            labelPreview.Size = new Size(51, 15);
            labelPreview.TabIndex = 16;
            labelPreview.Text = "Preview:";
            // 
            // textDescription
            // 
            textDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textDescription.Location = new Point(89, 43);
            textDescription.MaxLength = 3332767;
            textDescription.Multiline = true;
            textDescription.Name = "textDescription";
            textDescription.ScrollBars = ScrollBars.Vertical;
            textDescription.Size = new Size(577, 64);
            textDescription.TabIndex = 2;
            textDescription.TextChanged += textDescription_TextChanged;
            //
            // checkLockDescription
            //
            checkLockDescription.AutoSize = true;
            checkLockDescription.Location = new Point(89, 115);
            checkLockDescription.Name = "checkLockDescription";
            checkLockDescription.Size = new Size(232, 19);
            checkLockDescription.TabIndex = 3;
            checkLockDescription.Text = "Lock description (AI won't change it)";
            checkLockDescription.UseVisualStyleBackColor = true;
            checkLockDescription.CheckedChanged += checkLockDescription_CheckedChanged;
            //
            // labelAuthor
            //
            labelAuthor.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            labelAuthor.AutoEllipsis = true;
            labelAuthor.ForeColor = SystemColors.GrayText;
            labelAuthor.Location = new Point(89, 140);
            labelAuthor.Name = "labelAuthor";
            labelAuthor.Size = new Size(577, 18);
            labelAuthor.TabIndex = 18;
            //
            // textOrder
            //
            textOrder.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            textOrder.Location = new Point(604, 113);
            textOrder.Name = "textOrder";
            textOrder.Size = new Size(62, 23);
            textOrder.TabIndex = 4;
            // 
            // labelDescription
            // 
            labelDescription.AutoSize = true;
            labelDescription.Location = new Point(11, 47);
            labelDescription.Name = "labelDescription";
            labelDescription.Size = new Size(70, 15);
            labelDescription.TabIndex = 13;
            labelDescription.Text = "Description:";
            // 
            // labelOrder
            // 
            labelOrder.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            labelOrder.AutoSize = true;
            labelOrder.Location = new Point(558, 117);
            labelOrder.Name = "labelOrder";
            labelOrder.Size = new Size(40, 15);
            labelOrder.TabIndex = 12;
            labelOrder.Text = "Order:";
            // 
            // buttonSelectFile
            // 
            buttonSelectFile.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonSelectFile.Location = new Point(555, 14);
            buttonSelectFile.Name = "buttonSelectFile";
            buttonSelectFile.Size = new Size(111, 25);
            buttonSelectFile.TabIndex = 1;
            buttonSelectFile.Text = "Select file";
            buttonSelectFile.UseVisualStyleBackColor = true;
            buttonSelectFile.Click += buttonSelectFile_Click;
            // 
            // labelFileName
            // 
            labelFileName.AutoSize = true;
            labelFileName.Location = new Point(11, 17);
            labelFileName.Name = "labelFileName";
            labelFileName.Size = new Size(61, 15);
            labelFileName.TabIndex = 10;
            labelFileName.Text = "File name:";
            // 
            // ResourceEditorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(679, 629);
            Controls.Add(buttonCancel);
            Controls.Add(buttonAccept);
            Controls.Add(panelForm);
            KeyPreview = true;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ResourceEditorForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Resource editor";
            Load += ResourceEditorForm_Load;
            panelForm.ResumeLayout(false);
            panelForm.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.Label labelPreview;
        private System.Windows.Forms.TextBox textDescription;
        private System.Windows.Forms.TextBox textOrder;
        private System.Windows.Forms.Label labelDescription;
        private System.Windows.Forms.Label labelOrder;
        private System.Windows.Forms.Button buttonSelectFile;
        private System.Windows.Forms.Label labelFileName;
        private System.Windows.Forms.TextBox textFileName;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private KntWebView.KntEditView kntView;
        private System.Windows.Forms.CheckBox checkLockDescription;
        private System.Windows.Forms.Label labelAuthor;
    }
}