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
            buttonCancel = new Button();
            progressProcess = new ProgressBar();
            labelProcess = new Label();
            labelInfo = new Label();
            SuspendLayout();
            // 
            // buttonCancel
            // 
            buttonCancel.Location = new Point(281, 112);
            buttonCancel.Name = "buttonCancel";
            buttonCancel.Size = new Size(84, 29);
            buttonCancel.TabIndex = 0;
            buttonCancel.Text = "&Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // progressProcess
            // 
            progressProcess.Location = new Point(15, 72);
            progressProcess.Name = "progressProcess";
            progressProcess.Size = new Size(353, 22);
            progressProcess.TabIndex = 1;
            // 
            // labelProcess
            // 
            labelProcess.AutoSize = true;
            labelProcess.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            labelProcess.Location = new Point(15, 18);
            labelProcess.Name = "labelProcess";
            labelProcess.Size = new Size(81, 15);
            labelProcess.TabIndex = 2;
            labelProcess.Text = "Label process";
            // 
            // labelInfo
            // 
            labelInfo.AutoSize = true;
            labelInfo.Location = new Point(15, 45);
            labelInfo.Name = "labelInfo";
            labelInfo.Size = new Size(16, 15);
            labelInfo.TabIndex = 3;
            labelInfo.Text = "...";
            // 
            // HeavyProcessForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(377, 153);
            Controls.Add(labelInfo);
            Controls.Add(labelProcess);
            Controls.Add(progressProcess);
            Controls.Add(buttonCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "HeavyProcessForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Processing task";
            FormClosing += HeavyProcessForm_FormClosing;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button buttonCancel;
        private ProgressBar progressProcess;
        private Label labelProcess;
        private Label labelInfo;
    }
}