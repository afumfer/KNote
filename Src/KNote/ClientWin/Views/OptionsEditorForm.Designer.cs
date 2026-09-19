
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
            this.tabEmailOptions = new System.Windows.Forms.TabPage();
            this.labelSmtpHost = new System.Windows.Forms.Label();
            this.textSmtpHost = new System.Windows.Forms.TextBox();
            this.labelSmtpPort = new System.Windows.Forms.Label();
            this.textSmtpPort = new System.Windows.Forms.TextBox();
            this.checkSmtpEnableSsl = new System.Windows.Forms.CheckBox();
            this.labelSmtpFromAddress = new System.Windows.Forms.Label();
            this.textSmtpFromAddress = new System.Windows.Forms.TextBox();
            this.labelSmtpFromDisplayName = new System.Windows.Forms.Label();
            this.textSmtpFromDisplayName = new System.Windows.Forms.TextBox();
            this.labelSmtpUsername = new System.Windows.Forms.Label();
            this.textSmtpUsername = new System.Windows.Forms.TextBox();
            this.labelSmtpPassword = new System.Windows.Forms.Label();
            this.textSmtpPassword = new System.Windows.Forms.TextBox();
            this.labelTestEmailTo = new System.Windows.Forms.Label();
            this.textTestEmailTo = new System.Windows.Forms.TextBox();
            this.buttonTestSmtp = new System.Windows.Forms.Button();
            this.labelInfo = new System.Windows.Forms.Label();
            this.checkCompactViewNotesList = new System.Windows.Forms.CheckBox();
            this.textAutosaveSeconds = new System.Windows.Forms.TextBox();
            this.textAlarmSeconds = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.labelAlarmSeconds = new System.Windows.Forms.Label();
            this.checkAutoSaveActivated = new System.Windows.Forms.CheckBox();
            this.checkAlarmActivated = new System.Windows.Forms.CheckBox();
            this.buttonSelectDirectoryResources = new System.Windows.Forms.Button();
            this.buttonFolderSearch = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.textChatHubUrl = new System.Windows.Forms.TextBox();
            this.buttonTestChatHubUrl = new System.Windows.Forms.Button();
            this.panelForm.SuspendLayout();
            this.tabOptions.SuspendLayout();
            this.tabGlobalOptions.SuspendLayout();
            this.tabEmailOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(407, 287);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(78, 29);
            this.buttonCancel.TabIndex = 12;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonAccept
            // 
            this.buttonAccept.Location = new System.Drawing.Point(325, 287);
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
            this.panelForm.Size = new System.Drawing.Size(497, 276);
            this.panelForm.TabIndex = 10;
            // 
            // tabOptions
            // 
            this.tabOptions.Controls.Add(this.tabGlobalOptions);
            this.tabOptions.Controls.Add(this.tabEmailOptions);
            this.tabOptions.Location = new System.Drawing.Point(6, 6);
            this.tabOptions.Name = "tabOptions";
            this.tabOptions.SelectedIndex = 0;
            this.tabOptions.Size = new System.Drawing.Size(485, 268);
            this.tabOptions.TabIndex = 18;
            // 
            // tabGlobalOptions
            // 
            this.tabGlobalOptions.Controls.Add(this.textChatHubUrl);
            this.tabGlobalOptions.Controls.Add(this.buttonTestChatHubUrl);
            this.tabGlobalOptions.Controls.Add(this.label1);
            this.tabGlobalOptions.Controls.Add(this.labelInfo);
            this.tabGlobalOptions.Controls.Add(this.checkCompactViewNotesList);
            this.tabGlobalOptions.Controls.Add(this.textAutosaveSeconds);
            this.tabGlobalOptions.Controls.Add(this.textAlarmSeconds);
            this.tabGlobalOptions.Controls.Add(this.label2);
            this.tabGlobalOptions.Controls.Add(this.labelAlarmSeconds);
            this.tabGlobalOptions.Controls.Add(this.checkAutoSaveActivated);
            this.tabGlobalOptions.Controls.Add(this.checkAlarmActivated);
            this.tabGlobalOptions.Location = new System.Drawing.Point(4, 24);
            this.tabGlobalOptions.Name = "tabGlobalOptions";
            this.tabGlobalOptions.Padding = new System.Windows.Forms.Padding(3);
            this.tabGlobalOptions.Size = new System.Drawing.Size(477, 240);
            this.tabGlobalOptions.TabIndex = 0;
            this.tabGlobalOptions.Text = "Global options";
            this.tabGlobalOptions.UseVisualStyleBackColor = true;
            //
            // tabEmailOptions
            //
            this.tabEmailOptions.Controls.Add(this.labelSmtpHost);
            this.tabEmailOptions.Controls.Add(this.textSmtpHost);
            this.tabEmailOptions.Controls.Add(this.labelSmtpPort);
            this.tabEmailOptions.Controls.Add(this.textSmtpPort);
            this.tabEmailOptions.Controls.Add(this.checkSmtpEnableSsl);
            this.tabEmailOptions.Controls.Add(this.labelSmtpFromAddress);
            this.tabEmailOptions.Controls.Add(this.textSmtpFromAddress);
            this.tabEmailOptions.Controls.Add(this.labelSmtpFromDisplayName);
            this.tabEmailOptions.Controls.Add(this.textSmtpFromDisplayName);
            this.tabEmailOptions.Controls.Add(this.labelSmtpUsername);
            this.tabEmailOptions.Controls.Add(this.textSmtpUsername);
            this.tabEmailOptions.Controls.Add(this.labelSmtpPassword);
            this.tabEmailOptions.Controls.Add(this.textSmtpPassword);
            this.tabEmailOptions.Controls.Add(this.labelTestEmailTo);
            this.tabEmailOptions.Controls.Add(this.textTestEmailTo);
            this.tabEmailOptions.Controls.Add(this.buttonTestSmtp);
            this.tabEmailOptions.Location = new System.Drawing.Point(4, 24);
            this.tabEmailOptions.Name = "tabEmailOptions";
            this.tabEmailOptions.Padding = new System.Windows.Forms.Padding(3);
            this.tabEmailOptions.Size = new System.Drawing.Size(477, 240);
            this.tabEmailOptions.TabIndex = 1;
            this.tabEmailOptions.Text = "Email (SMTP)";
            this.tabEmailOptions.UseVisualStyleBackColor = true;
            //
            // labelSmtpHost
            //
            this.labelSmtpHost.AutoSize = true;
            this.labelSmtpHost.Location = new System.Drawing.Point(9, 16);
            this.labelSmtpHost.Name = "labelSmtpHost";
            this.labelSmtpHost.Size = new System.Drawing.Size(68, 15);
            this.labelSmtpHost.TabIndex = 0;
            this.labelSmtpHost.Text = "SMTP host:";
            //
            // textSmtpHost
            //
            this.textSmtpHost.Location = new System.Drawing.Point(127, 12);
            this.textSmtpHost.Name = "textSmtpHost";
            this.textSmtpHost.Size = new System.Drawing.Size(220, 23);
            this.textSmtpHost.TabIndex = 1;
            //
            // labelSmtpPort
            //
            this.labelSmtpPort.AutoSize = true;
            this.labelSmtpPort.Location = new System.Drawing.Point(355, 16);
            this.labelSmtpPort.Name = "labelSmtpPort";
            this.labelSmtpPort.Size = new System.Drawing.Size(32, 15);
            this.labelSmtpPort.TabIndex = 2;
            this.labelSmtpPort.Text = "Port:";
            //
            // textSmtpPort
            //
            this.textSmtpPort.Location = new System.Drawing.Point(390, 12);
            this.textSmtpPort.Name = "textSmtpPort";
            this.textSmtpPort.Size = new System.Drawing.Size(50, 23);
            this.textSmtpPort.TabIndex = 3;
            //
            // checkSmtpEnableSsl
            //
            this.checkSmtpEnableSsl.AutoSize = true;
            this.checkSmtpEnableSsl.Location = new System.Drawing.Point(127, 47);
            this.checkSmtpEnableSsl.Name = "checkSmtpEnableSsl";
            this.checkSmtpEnableSsl.Size = new System.Drawing.Size(87, 19);
            this.checkSmtpEnableSsl.TabIndex = 4;
            this.checkSmtpEnableSsl.Text = "Enable SSL";
            this.checkSmtpEnableSsl.UseVisualStyleBackColor = true;
            //
            // labelSmtpFromAddress
            //
            this.labelSmtpFromAddress.AutoSize = true;
            this.labelSmtpFromAddress.Location = new System.Drawing.Point(9, 80);
            this.labelSmtpFromAddress.Name = "labelSmtpFromAddress";
            this.labelSmtpFromAddress.Size = new System.Drawing.Size(83, 15);
            this.labelSmtpFromAddress.TabIndex = 5;
            this.labelSmtpFromAddress.Text = "From address:";
            //
            // textSmtpFromAddress
            //
            this.textSmtpFromAddress.Location = new System.Drawing.Point(127, 76);
            this.textSmtpFromAddress.Name = "textSmtpFromAddress";
            this.textSmtpFromAddress.Size = new System.Drawing.Size(313, 23);
            this.textSmtpFromAddress.TabIndex = 6;
            //
            // labelSmtpFromDisplayName
            //
            this.labelSmtpFromDisplayName.AutoSize = true;
            this.labelSmtpFromDisplayName.Location = new System.Drawing.Point(9, 109);
            this.labelSmtpFromDisplayName.Name = "labelSmtpFromDisplayName";
            this.labelSmtpFromDisplayName.Size = new System.Drawing.Size(72, 15);
            this.labelSmtpFromDisplayName.TabIndex = 7;
            this.labelSmtpFromDisplayName.Text = "From name:";
            //
            // textSmtpFromDisplayName
            //
            this.textSmtpFromDisplayName.Location = new System.Drawing.Point(127, 105);
            this.textSmtpFromDisplayName.Name = "textSmtpFromDisplayName";
            this.textSmtpFromDisplayName.Size = new System.Drawing.Size(313, 23);
            this.textSmtpFromDisplayName.TabIndex = 8;
            //
            // labelSmtpUsername
            //
            this.labelSmtpUsername.AutoSize = true;
            this.labelSmtpUsername.Location = new System.Drawing.Point(9, 138);
            this.labelSmtpUsername.Name = "labelSmtpUsername";
            this.labelSmtpUsername.Size = new System.Drawing.Size(64, 15);
            this.labelSmtpUsername.TabIndex = 9;
            this.labelSmtpUsername.Text = "Username:";
            //
            // textSmtpUsername
            //
            this.textSmtpUsername.Location = new System.Drawing.Point(127, 134);
            this.textSmtpUsername.Name = "textSmtpUsername";
            this.textSmtpUsername.Size = new System.Drawing.Size(313, 23);
            this.textSmtpUsername.TabIndex = 10;
            //
            // labelSmtpPassword
            //
            this.labelSmtpPassword.AutoSize = true;
            this.labelSmtpPassword.Location = new System.Drawing.Point(9, 167);
            this.labelSmtpPassword.Name = "labelSmtpPassword";
            this.labelSmtpPassword.Size = new System.Drawing.Size(60, 15);
            this.labelSmtpPassword.TabIndex = 11;
            this.labelSmtpPassword.Text = "Password:";
            //
            // textSmtpPassword
            //
            this.textSmtpPassword.Location = new System.Drawing.Point(127, 163);
            this.textSmtpPassword.Name = "textSmtpPassword";
            this.textSmtpPassword.Size = new System.Drawing.Size(313, 23);
            this.textSmtpPassword.TabIndex = 12;
            this.textSmtpPassword.UseSystemPasswordChar = true;
            //
            // labelTestEmailTo
            //
            this.labelTestEmailTo.AutoSize = true;
            this.labelTestEmailTo.Location = new System.Drawing.Point(9, 201);
            this.labelTestEmailTo.Name = "labelTestEmailTo";
            this.labelTestEmailTo.Size = new System.Drawing.Size(107, 15);
            this.labelTestEmailTo.TabIndex = 13;
            this.labelTestEmailTo.Text = "Send test email to:";
            //
            // textTestEmailTo
            //
            this.textTestEmailTo.Location = new System.Drawing.Point(127, 197);
            this.textTestEmailTo.Name = "textTestEmailTo";
            this.textTestEmailTo.Size = new System.Drawing.Size(220, 23);
            this.textTestEmailTo.TabIndex = 14;
            //
            // buttonTestSmtp
            //
            this.buttonTestSmtp.Location = new System.Drawing.Point(355, 196);
            this.buttonTestSmtp.Name = "buttonTestSmtp";
            this.buttonTestSmtp.Size = new System.Drawing.Size(85, 25);
            this.buttonTestSmtp.TabIndex = 15;
            this.buttonTestSmtp.Text = "&Test";
            this.buttonTestSmtp.UseVisualStyleBackColor = true;
            this.buttonTestSmtp.Click += new System.EventHandler(this.buttonTestSmtp_Click);
            //
            // labelInfo
            // 
            this.labelInfo.AutoSize = true;
            this.labelInfo.Location = new System.Drawing.Point(6, 217);
            this.labelInfo.Name = "labelInfo";
            this.labelInfo.Size = new System.Drawing.Size(181, 15);
            this.labelInfo.TabIndex = 10;
            this.labelInfo.Text = "(*) Application restart is required ";
            // 
            // checkCompactViewNotesList
            // 
            this.checkCompactViewNotesList.AutoSize = true;
            this.checkCompactViewNotesList.Location = new System.Drawing.Point(13, 88);
            this.checkCompactViewNotesList.Name = "checkCompactViewNotesList";
            this.checkCompactViewNotesList.Size = new System.Drawing.Size(181, 19);
            this.checkCompactViewNotesList.TabIndex = 6;
            this.checkCompactViewNotesList.Text = "Compact view in notes list (*)";
            this.checkCompactViewNotesList.UseVisualStyleBackColor = true;
            // 
            // textAutosaveSeconds
            // 
            this.textAutosaveSeconds.Location = new System.Drawing.Point(309, 47);
            this.textAutosaveSeconds.Name = "textAutosaveSeconds";
            this.textAutosaveSeconds.Size = new System.Drawing.Size(58, 23);
            this.textAutosaveSeconds.TabIndex = 5;
            // 
            // textAlarmSeconds
            // 
            this.textAlarmSeconds.Location = new System.Drawing.Point(309, 12);
            this.textAlarmSeconds.Name = "textAlarmSeconds";
            this.textAlarmSeconds.Size = new System.Drawing.Size(58, 23);
            this.textAlarmSeconds.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(198, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(105, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "Autosave seconds:";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelAlarmSeconds
            // 
            this.labelAlarmSeconds.AutoSize = true;
            this.labelAlarmSeconds.Location = new System.Drawing.Point(198, 16);
            this.labelAlarmSeconds.Name = "labelAlarmSeconds";
            this.labelAlarmSeconds.Size = new System.Drawing.Size(88, 15);
            this.labelAlarmSeconds.TabIndex = 1;
            this.labelAlarmSeconds.Text = "Alarm seconds:";
            this.labelAlarmSeconds.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // checkAutoSaveActivated
            // 
            this.checkAutoSaveActivated.AutoSize = true;
            this.checkAutoSaveActivated.Location = new System.Drawing.Point(13, 51);
            this.checkAutoSaveActivated.Name = "checkAutoSaveActivated";
            this.checkAutoSaveActivated.Size = new System.Drawing.Size(158, 19);
            this.checkAutoSaveActivated.TabIndex = 3;
            this.checkAutoSaveActivated.Text = "Autosave notes activated";
            this.checkAutoSaveActivated.UseVisualStyleBackColor = true;
            // 
            // checkAlarmActivated
            // 
            this.checkAlarmActivated.AutoSize = true;
            this.checkAlarmActivated.Location = new System.Drawing.Point(13, 16);
            this.checkAlarmActivated.Name = "checkAlarmActivated";
            this.checkAlarmActivated.Size = new System.Drawing.Size(109, 19);
            this.checkAlarmActivated.TabIndex = 0;
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
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 128);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 15);
            this.label1.TabIndex = 7;
            this.label1.Text = "Chat hub url:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textChatHubUrl
            //
            this.textChatHubUrl.Location = new System.Drawing.Point(95, 125);
            this.textChatHubUrl.Name = "textChatHubUrl";
            this.textChatHubUrl.Size = new System.Drawing.Size(295, 23);
            this.textChatHubUrl.TabIndex = 8;
            //
            // buttonTestChatHubUrl
            //
            this.buttonTestChatHubUrl.Location = new System.Drawing.Point(396, 124);
            this.buttonTestChatHubUrl.Name = "buttonTestChatHubUrl";
            this.buttonTestChatHubUrl.Size = new System.Drawing.Size(66, 25);
            this.buttonTestChatHubUrl.TabIndex = 9;
            this.buttonTestChatHubUrl.Text = "&Test";
            this.buttonTestChatHubUrl.UseVisualStyleBackColor = true;
            this.buttonTestChatHubUrl.Click += new System.EventHandler(this.buttonTestChatHubUrl_Click);
            //
            // OptionsEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(497, 326);
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
            this.Text = "KNote options";
            this.panelForm.ResumeLayout(false);
            this.tabOptions.ResumeLayout(false);
            this.tabGlobalOptions.ResumeLayout(false);
            this.tabGlobalOptions.PerformLayout();
            this.tabEmailOptions.ResumeLayout(false);
            this.tabEmailOptions.PerformLayout();
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
        private CheckBox checkCompactViewNotesList;
        private Label labelInfo;
        private OpenFileDialog openFileDialog;
        private TextBox textChatHubUrl;
        private Label label1;
        private Button buttonTestChatHubUrl;
        private System.Windows.Forms.TabPage tabEmailOptions;
        private Label labelSmtpHost;
        private TextBox textSmtpHost;
        private Label labelSmtpPort;
        private TextBox textSmtpPort;
        private CheckBox checkSmtpEnableSsl;
        private Label labelSmtpFromAddress;
        private TextBox textSmtpFromAddress;
        private Label labelSmtpFromDisplayName;
        private TextBox textSmtpFromDisplayName;
        private Label labelSmtpUsername;
        private TextBox textSmtpUsername;
        private Label labelSmtpPassword;
        private TextBox textSmtpPassword;
        private Label labelTestEmailTo;
        private TextBox textTestEmailTo;
        private Button buttonTestSmtp;
    }
}