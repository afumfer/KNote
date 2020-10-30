namespace KNote.ClientWin.Views
{
    partial class DemoForm
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
            this.button1 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.groupSamples = new System.Windows.Forms.GroupBox();
            this.buttonRunSample = new System.Windows.Forms.Button();
            this.buttonShowSample = new System.Windows.Forms.Button();
            this.listSamples = new System.Windows.Forms.ListBox();
            this.buttonInteract = new System.Windows.Forms.Button();
            this.buttonRunBackground = new System.Windows.Forms.Button();
            this.groupSamples.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 18);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(243, 28);
            this.button1.TabIndex = 0;
            this.button1.Text = "Run simple script (embedded code)";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.buttonRunScript_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(12, 123);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(243, 29);
            this.button3.TabIndex = 3;
            this.button3.Text = "Show KntConsole";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.buttonShowConsole_Click);
            // 
            // groupSamples
            // 
            this.groupSamples.Controls.Add(this.buttonRunSample);
            this.groupSamples.Controls.Add(this.buttonShowSample);
            this.groupSamples.Controls.Add(this.listSamples);
            this.groupSamples.Location = new System.Drawing.Point(12, 176);
            this.groupSamples.Name = "groupSamples";
            this.groupSamples.Size = new System.Drawing.Size(451, 307);
            this.groupSamples.TabIndex = 3;
            this.groupSamples.TabStop = false;
            this.groupSamples.Text = "Samples";
            // 
            // buttonRunSample
            // 
            this.buttonRunSample.Location = new System.Drawing.Point(262, 59);
            this.buttonRunSample.Name = "buttonRunSample";
            this.buttonRunSample.Size = new System.Drawing.Size(173, 30);
            this.buttonRunSample.TabIndex = 5;
            this.buttonRunSample.Text = "Run sample";
            this.buttonRunSample.UseVisualStyleBackColor = true;
            this.buttonRunSample.Click += new System.EventHandler(this.buttonRunSample_Click);
            // 
            // buttonShowSample
            // 
            this.buttonShowSample.Location = new System.Drawing.Point(262, 22);
            this.buttonShowSample.Name = "buttonShowSample";
            this.buttonShowSample.Size = new System.Drawing.Size(173, 31);
            this.buttonShowSample.TabIndex = 4;
            this.buttonShowSample.Text = "Show sample in KntConsole";
            this.buttonShowSample.UseVisualStyleBackColor = true;
            this.buttonShowSample.Click += new System.EventHandler(this.buttonShowSample_Click);
            // 
            // listSamples
            // 
            this.listSamples.FormattingEnabled = true;
            this.listSamples.ItemHeight = 15;
            this.listSamples.Location = new System.Drawing.Point(13, 19);
            this.listSamples.Name = "listSamples";
            this.listSamples.Size = new System.Drawing.Size(230, 274);
            this.listSamples.TabIndex = 2;
            this.listSamples.SelectedIndexChanged += new System.EventHandler(this.listSamples_SelectedIndexChanged);
            // 
            // buttonInteract
            // 
            this.buttonInteract.Location = new System.Drawing.Point(12, 52);
            this.buttonInteract.Name = "buttonInteract";
            this.buttonInteract.Size = new System.Drawing.Size(243, 28);
            this.buttonInteract.TabIndex = 1;
            this.buttonInteract.Text = "Interacting with kntscript (embedded code)";
            this.buttonInteract.UseVisualStyleBackColor = true;
            this.buttonInteract.Click += new System.EventHandler(this.buttonInteract_Click);
            // 
            // buttonRunBackground
            // 
            this.buttonRunBackground.Location = new System.Drawing.Point(12, 86);
            this.buttonRunBackground.Name = "buttonRunBackground";
            this.buttonRunBackground.Size = new System.Drawing.Size(243, 31);
            this.buttonRunBackground.TabIndex = 2;
            this.buttonRunBackground.Text = "Run simple script in bakground";
            this.buttonRunBackground.UseVisualStyleBackColor = true;
            this.buttonRunBackground.Click += new System.EventHandler(this.buttonRunBackground_Click);
            // 
            // DemoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(482, 495);
            this.Controls.Add(this.buttonRunBackground);
            this.Controls.Add(this.buttonInteract);
            this.Controls.Add(this.groupSamples);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "DemoForm";
            this.Text = "KntScript AppHost demo";
            this.Load += new System.EventHandler(this.DemoForm_Load);
            this.groupSamples.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.GroupBox groupSamples;
        private System.Windows.Forms.Button buttonRunSample;
        private System.Windows.Forms.Button buttonShowSample;
        private System.Windows.Forms.ListBox listSamples;
        private System.Windows.Forms.Button buttonInteract;
        private System.Windows.Forms.Button buttonRunBackground;
    }
}