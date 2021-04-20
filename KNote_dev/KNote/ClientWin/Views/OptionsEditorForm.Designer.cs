
namespace KNote.ClientWin.Views
{
    partial class OptionsEditorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsEditorForm));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.panelForm = new System.Windows.Forms.Panel();
            this.tabOptions = new System.Windows.Forms.TabControl();
            this.tabGlobalOptions = new System.Windows.Forms.TabPage();
            this.textAutosaveSeconds = new System.Windows.Forms.TextBox();
            this.textAlarmSeconds = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.labelAlarmSeconds = new System.Windows.Forms.Label();
            this.checkAutoSaveActivated = new System.Windows.Forms.CheckBox();
            this.checkAlarmActivated = new System.Windows.Forms.CheckBox();
            this.buttonSelectDirectoryResources = new System.Windows.Forms.Button();
            this.buttonFolderSearch = new System.Windows.Forms.Button();
            this.panelForm.SuspendLayout();
            this.tabOptions.SuspendLayout();
            this.tabGlobalOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.Location = new System.Drawing.Point(407, 294);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(78, 29);
            this.buttonCancel.TabIndex = 12;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonAccept
            // 
            this.buttonAccept.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAccept.Location = new System.Drawing.Point(325, 294);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(76, 29);
            this.buttonAccept.TabIndex = 11;
            this.buttonAccept.Text = "&Accept";
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.buttonAccept_Click);
            // 
            // panelForm
            // 
            this.panelForm.Controls.Add(this.tabOptions);
            this.panelForm.Controls.Add(this.buttonSelectDirectoryResources);
            this.panelForm.Controls.Add(this.buttonFolderSearch);
            this.panelForm.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelForm.Location = new System.Drawing.Point(0, 0);
            this.panelForm.Name = "panelForm";
            this.panelForm.Size = new System.Drawing.Size(497, 292);
            this.panelForm.TabIndex = 10;
            // 
            // tabOptions
            // 
            this.tabOptions.Controls.Add(this.tabGlobalOptions);
            this.tabOptions.Location = new System.Drawing.Point(6, 6);
            this.tabOptions.Name = "tabOptions";
            this.tabOptions.SelectedIndex = 0;
            this.tabOptions.Size = new System.Drawing.Size(485, 274);
            this.tabOptions.TabIndex = 18;
            // 
            // tabGlobalOptions
            // 
            this.tabGlobalOptions.Controls.Add(this.textAutosaveSeconds);
            this.tabGlobalOptions.Controls.Add(this.textAlarmSeconds);
            this.tabGlobalOptions.Controls.Add(this.label2);
            this.tabGlobalOptions.Controls.Add(this.labelAlarmSeconds);
            this.tabGlobalOptions.Controls.Add(this.checkAutoSaveActivated);
            this.tabGlobalOptions.Controls.Add(this.checkAlarmActivated);
            this.tabGlobalOptions.Location = new System.Drawing.Point(4, 24);
            this.tabGlobalOptions.Name = "tabGlobalOptions";
            this.tabGlobalOptions.Padding = new System.Windows.Forms.Padding(3);
            this.tabGlobalOptions.Size = new System.Drawing.Size(477, 246);
            this.tabGlobalOptions.TabIndex = 0;
            this.tabGlobalOptions.Text = "Global options";
            this.tabGlobalOptions.UseVisualStyleBackColor = true;
            // 
            // textAutosaveSeconds
            // 
            this.textAutosaveSeconds.Location = new System.Drawing.Point(169, 103);
            this.textAutosaveSeconds.Name = "textAutosaveSeconds";
            this.textAutosaveSeconds.Size = new System.Drawing.Size(101, 23);
            this.textAutosaveSeconds.TabIndex = 23;
            // 
            // textAlarmSeconds
            // 
            this.textAlarmSeconds.Location = new System.Drawing.Point(169, 35);
            this.textAlarmSeconds.Name = "textAlarmSeconds";
            this.textAlarmSeconds.Size = new System.Drawing.Size(101, 23);
            this.textAlarmSeconds.TabIndex = 22;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(55, 106);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(105, 15);
            this.label2.TabIndex = 21;
            this.label2.Text = "Autosave seconds:";
            // 
            // labelAlarmSeconds
            // 
            this.labelAlarmSeconds.AutoSize = true;
            this.labelAlarmSeconds.Location = new System.Drawing.Point(55, 38);
            this.labelAlarmSeconds.Name = "labelAlarmSeconds";
            this.labelAlarmSeconds.Size = new System.Drawing.Size(88, 15);
            this.labelAlarmSeconds.TabIndex = 20;
            this.labelAlarmSeconds.Text = "Alarm seconds:";
            // 
            // checkAutoSaveActivated
            // 
            this.checkAutoSaveActivated.AutoSize = true;
            this.checkAutoSaveActivated.Location = new System.Drawing.Point(13, 84);
            this.checkAutoSaveActivated.Name = "checkAutoSaveActivated";
            this.checkAutoSaveActivated.Size = new System.Drawing.Size(158, 19);
            this.checkAutoSaveActivated.TabIndex = 19;
            this.checkAutoSaveActivated.Text = "Autosave notes activated";
            this.checkAutoSaveActivated.UseVisualStyleBackColor = true;
            // 
            // checkAlarmActivated
            // 
            this.checkAlarmActivated.AutoSize = true;
            this.checkAlarmActivated.Location = new System.Drawing.Point(13, 16);
            this.checkAlarmActivated.Name = "checkAlarmActivated";
            this.checkAlarmActivated.Size = new System.Drawing.Size(109, 19);
            this.checkAlarmActivated.TabIndex = 18;
            this.checkAlarmActivated.Text = "Alarm activated";
            this.checkAlarmActivated.UseVisualStyleBackColor = true;
            // 
            // buttonSelectDirectoryResources
            // 
            this.buttonSelectDirectoryResources.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectDirectoryResources.Location = new System.Drawing.Point(775, 198);
            this.buttonSelectDirectoryResources.Name = "buttonSelectDirectoryResources";
            this.buttonSelectDirectoryResources.Size = new System.Drawing.Size(24, 23);
            this.buttonSelectDirectoryResources.TabIndex = 4;
            this.buttonSelectDirectoryResources.Text = "...";
            this.buttonSelectDirectoryResources.UseVisualStyleBackColor = true;
            // 
            // buttonFolderSearch
            // 
            this.buttonFolderSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonFolderSearch.BackColor = System.Drawing.SystemColors.Control;
            this.buttonFolderSearch.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonFolderSearch.Location = new System.Drawing.Point(1077, 199);
            this.buttonFolderSearch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.buttonFolderSearch.Name = "buttonFolderSearch";
            this.buttonFolderSearch.Size = new System.Drawing.Size(27, 25);
            this.buttonFolderSearch.TabIndex = 11;
            this.buttonFolderSearch.Text = "...";
            this.buttonFolderSearch.UseVisualStyleBackColor = false;
            // 
            // OptionsEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(497, 335);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonAccept);
            this.Controls.Add(this.panelForm);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsEditorForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "KaNote options";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OptionsEditorForm_FormClosing);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.OptionsEditorForm_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OptionsEditorForm_KeyUp);
            this.panelForm.ResumeLayout(false);
            this.tabOptions.ResumeLayout(false);
            this.tabGlobalOptions.ResumeLayout(false);
            this.tabGlobalOptions.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.Panel panelForm;
        private System.Windows.Forms.Button buttonSelectDirectoryResources;
        private System.Windows.Forms.Button buttonFolderSearch;
        private System.Windows.Forms.TabControl tabOptions;
        private System.Windows.Forms.TabPage tabGlobalOptions;
        private System.Windows.Forms.TextBox textAutosaveSeconds;
        private System.Windows.Forms.TextBox textAlarmSeconds;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label labelAlarmSeconds;
        private System.Windows.Forms.CheckBox checkAutoSaveActivated;
        private System.Windows.Forms.CheckBox checkAlarmActivated;
    }
}