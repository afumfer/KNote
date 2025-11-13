namespace KNote.ClientWin.Views
{
    partial class ReadVarForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReadVarForm));
            panelButtons = new Panel();
            buttonCancel = new Button();
            buttonAcept = new Button();
            panelControls = new Panel();
            panelButtons.SuspendLayout();
            SuspendLayout();
            // 
            // panelButtons
            // 
            panelButtons.Controls.Add(buttonCancel);
            panelButtons.Controls.Add(buttonAcept);
            panelButtons.Dock = DockStyle.Bottom;
            panelButtons.Location = new Point(0, 325);
            panelButtons.Name = "panelButtons";
            panelButtons.Size = new Size(586, 50);
            panelButtons.TabIndex = 2;
            // 
            // buttonCancel
            // 
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Location = new Point(496, 11);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(81, 29);
            buttonCancel.TabIndex = 1;
            buttonCancel.Text = "&Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // buttonAcept
            // 
            buttonAcept.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonAcept.Location = new Point(409, 11);
            buttonAcept.Name = "buttonAcept";
            buttonAcept.Size = new Size(81, 29);
            buttonAcept.TabIndex = 1;
            buttonAcept.Text = "&Acept";
            buttonAcept.UseVisualStyleBackColor = true;
            buttonAcept.Click += buttonAccept_Click;
            // 
            // panelControls
            // 
            panelControls.BackColor = Color.White;
            panelControls.Dock = DockStyle.Fill;
            panelControls.Location = new Point(0, 0);
            panelControls.Name = "panelControls";
            panelControls.Size = new Size(586, 325);
            panelControls.TabIndex = 3;
            // 
            // ReadVarForm
            // 
            AcceptButton = buttonAcept;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = buttonCancel;
            ClientSize = new Size(586, 375);
            Controls.Add(panelControls);
            Controls.Add(panelButtons);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ReadVarForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "KntScript read vars";            
            Shown += ReadVarForm_Shown;
            panelButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Panel panelControls;
        private Button buttonCancel;
        private Button buttonAcept;
    }
}