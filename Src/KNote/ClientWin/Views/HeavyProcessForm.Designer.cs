namespace KNote.ClientWin.Views
{
    partial class HeavyProcessForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HeavyProcessForm));
            btnCancel = new Button();
            pbProcess = new ProgressBar();
            labelInfo = new Label();
            SuspendLayout();
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(281, 121);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(84, 29);
            btnCancel.TabIndex = 0;
            btnCancel.Text = "&Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // pbProcess
            // 
            pbProcess.Location = new Point(12, 80);
            pbProcess.Name = "pbProcess";
            pbProcess.Size = new Size(353, 22);
            pbProcess.TabIndex = 1;
            // 
            // labelInfo
            // 
            labelInfo.AutoSize = true;
            labelInfo.Location = new Point(15, 16);
            labelInfo.Name = "labelInfo";
            labelInfo.Size = new Size(83, 15);
            labelInfo.TabIndex = 2;
            labelInfo.Text = "Heavy process";
            // 
            // HeavyProcessForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(377, 167);
            Controls.Add(labelInfo);
            Controls.Add(pbProcess);
            Controls.Add(btnCancel);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "HeavyProcessForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Processing task";
            Load += HeavyProcessForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnCancel;
        private ProgressBar pbProcess;
        private Label labelInfo;
    }
}